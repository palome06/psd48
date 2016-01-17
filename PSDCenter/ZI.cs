using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using PSD.Base.Rules;
using System.IO;
using System.Diagnostics;
using System.IO.Pipes;

namespace PSD.PSDCenter
{
    public class ZI
    {
        private IDictionary<ushort, Neayer> neayers;
        private IDictionary<ushort, Netcher> netchers;
        private IDictionary<int, Room> rooms;
        // losers in the set
        private ISet<Neayer> losers;
        // the substitution map from newcomer to connection loser
        private IDictionary<ushort, ushort> substitudes;
        // show which room players are allocated at
        private IDictionary<ushort, int> location;

        private TcpListener listener;
        private int port;

        private const int playerCapacity = 6;

        public void Run()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            while (true)
            {
                Socket socket = listener.AcceptSocket();
                new Thread(delegate() { RegisterSocket(socket); }).Start();
            }
        }

        public void RegisterSocket(Socket socket)
        {
            NetworkStream ns = new NetworkStream(socket);
            string data = ReadByteLine(ns);
            if (data.StartsWith("C0CO,"))
            {
                string[] blocks = data.Split(',');
                string user = blocks[1];
                ushort avatar = ushort.Parse(blocks[2]);
                int teamCode = int.Parse(blocks[3]);
                int selCode = int.Parse(blocks[4]);
                int levelCode = int.Parse(blocks[5]);
                string[] trainers = null;
                if (blocks.Length > 6)
                {
                    trainers = new string[blocks.Length - 6];
                    for (int i = 6; i < blocks.Length; ++i)
                        trainers[i - 6] = blocks[i];
                }

                ushort uid = RequestUid();
                Neayer ny = new Neayer(user, avatar, uid)
                {
                    HopeTeam = teamCode - RuleCode.HOPE_NOTCARE,
                    Tunnel = socket,
                    Ip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString() + "|"
                        + (socket.LocalEndPoint as IPEndPoint).Address.ToString()
                };
                Console.WriteLine(user + " has entered the hall.");
                SentByteLine(ns, "C0CN," + uid);
                neayers.Add(uid, ny);
                //Thread lThread = new Thread(delegate() { ListenToTalkSocket(socket); });
                //lThread.Start();

                Room reqRoom = RequestRoom(teamCode, selCode, levelCode, trainers);
                try
                {
                    lock (reqRoom.players)
                    {
                        string members = string.Join(",", reqRoom.players.Select(p => (
                            neayers[p].Uid + "," + neayers[p].Name + "," + neayers[p].Avatar)));
                        if (members.Length > 0)
                            members = "," + members;
                        SentByteLine(ns, "C1RM," + reqRoom.Number + members);
                        Console.WriteLine(user + " is allocated with room " + reqRoom.Number + "#.");
                        location[uid] = reqRoom.Number;
                        lock (reqRoom.players)
                        {
                            reqRoom.players.Add(uid);
                            foreach (ushort nyru in reqRoom.players)
                            {
                                Neayer nyr = neayers[nyru];
                                SentByteLine(new NetworkStream(nyr.Tunnel),
                                    "C1NW," + uid + "," + ny.Name + "," + avatar);
                            }
                            if (reqRoom.players.Count >= playerCapacity)
                                reqRoom.Ready = true;
                        }
                    }
                    if (reqRoom.Ready)
                    {
                        //if (lThread != null && lThread.IsAlive)
                        //    lThread.Abort();
                        string ag = "1 " + reqRoom.ConvToString() + " " +
                            string.Join(",", reqRoom.players);
                        string pg = "psd48pipe" + reqRoom.Number;
                        //string pgr = "psd48piper" + reqRoom.Number;
                        new Thread(delegate()
                        {
                            Thread.Sleep(1000);
                            Process.Start("PSDGamepkg.exe", ag);
                        }).Start();
                        reqRoom.Ps = new NamedPipeServerStream(pg, PipeDirection.InOut);
                        var ps = reqRoom.Ps;
                        ps.WaitForConnection();
                        string line;
                        while ((line = ReadBytes(ps)) != "")
                        {
                            if (line.StartsWith("C3RD"))
                            {
                                lock (reqRoom.players)
                                {
                                    foreach (ushort nyru in reqRoom.players)
                                    {
                                        Neayer nyr = neayers[nyru];
                                        SentByteLine(new NetworkStream(nyr.Tunnel), "C1SA,0");
                                    }
                                }
                            }
                            else if (line.StartsWith("C3LV")) // Terminate unexpected
                            {
                                Console.WriteLine("Room " + reqRoom.Number + "# is forced closed.");
                                rooms.Remove(reqRoom.Number);
                                //reqRoom.Ps.Close(); // TODO: LV don't termintate directly
                                break;
                            }
                            else if (line.StartsWith("C3TM")) // Terminate gracefully
                            {
                                Console.WriteLine("Room " + reqRoom.Number + "# terminates gracefully.");
                                rooms.Remove(reqRoom.Number);
                                // reqRoom.Ps.Close(); // TODO: LV don't termintate directly
                                break;
                            }
                            else if (line.StartsWith("C3LS"))
                            {
                                ushort ut = ushort.Parse(line.Substring("C3LS,".Length));
                                Console.WriteLine("Player " + ut + "#[" +
                                    neayers[ut].Name + "] loses connection with Room " + reqRoom.Number + "#.");
                                // $ut is the old uid
                                if (rooms.ContainsKey(reqRoom.Number) && neayers.ContainsKey(ut))
                                    losers.Add(neayers[ut]);
                            }
                            else if (line.StartsWith("C3RA"))
                            {
                                ushort ut = ushort.Parse(line.Substring("C3RA,".Length));
                                if (substitudes.ContainsKey(ut))
                                {
                                    ushort subsut = substitudes[ut];
                                    location[ut] = reqRoom.Number;
                                    Neayer rny = neayers[subsut];
                                    losers.Remove(rny);
                                    if (neayers.ContainsKey(subsut))
                                        neayers.Remove(subsut);
                                    substitudes.Remove(ut);
                                }
                                Console.WriteLine("Player " + ut + "#[" +
                                    neayers[ut].Name + "] has been back to Room " + reqRoom.Number + "#.");
                            }
                            else if (line.StartsWith("C3RV"))
                            {
                                Console.WriteLine("Room " + reqRoom.Number + "# is recovered.");
                            }
                        }
                    }
                    else
                    {
                        // Wait for C1ST message to terminate the socket
                        string reply = ReadByteLine(ns);
                        while (true)
                        {
                            if (reply.StartsWith("C0TK,"))
                                HandleTalkInConnection(socket, reply.Substring("C0TK,".Length));
                            else if (reply.StartsWith("C1ST,"))
                                break;
                            reply = ReadByteLine(ns);
                        }
                    }
                    socket.Close();
                }
                catch (IOException)
                {
                    neayers[uid].Alive = false;
                    lock (reqRoom.players)
                    {
                        bool any = false;
                        foreach (ushort ut in reqRoom.players)
                        {
                            Neayer nyr = neayers[ut];
                            if (nyr.Alive)
                            {
                                any = true;
                                if (IsTunnelAlive(nyr.Tunnel))
                                    SentByteLine(new NetworkStream(nyr.Tunnel), "C1LV," + uid);
                            }
                        }
                        location.Remove(uid);
                        if (!any)
                        {
                            int num = reqRoom.Number;
                            rooms.Remove(reqRoom.Number);
                            Console.WriteLine("Room {0}# is closed.", num);
                        }
                        reqRoom.players.Remove(uid);
                        neayers.Remove(uid);
                    }
                    //neayers.Remove(uid);
                    //reqRoom.players.Remove(uid);
                    //lock (reqRoom.players)
                    //{
                    //    if (reqRoom.players.Count > 0)
                    //    {
                    //        foreach (ushort ut in reqRoom.players)
                    //        {
                    //            Neayer nyr = neayers[ut];
                    //            try
                    //            {
                    //                bool part1 = nyr.Tunnel.Poll(1000, SelectMode.SelectRead);
                    //                bool part2 = (nyr.Tunnel.Available == 0);
                    //                if (!part1 || !part2)
                    //                    SentByteLine(new NetworkStream(nyr.Tunnel), "C1LV," + uid);
                    //            }
                    //            catch (IOException) { }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        int num = reqRoom.Number;
                    //        rooms.Remove(reqRoom.Number);
                    //        Console.WriteLine("Room {0}# is closed.", num);
                    //    }
                    //}
                }
            }
            else if (data.StartsWith("C0QI,"))
            {
                string[] blocks = data.Split(',');
                string user = blocks[1];

                ushort uid = RequestQSUid();
                try
                {
                    Netcher ny = new Netcher(user, uid) { Tunnel = socket };
                    string rms = string.Join(",", rooms.Where(
                        p => p.Value.Ready).Select(p => p.Key));
                    if (rms.Length > 0)
                        rms = "," + rms;
                    SentByteLine(ns, "C0QJ," + uid + rms);
                    string reply = ReadByteLine(ns);
                    while (!reply.StartsWith("C0QS,"))
                        reply = ReadByteLine(ns);

                    int room = int.Parse(reply.Substring(reply.IndexOf(',') + 1));
                    if (rooms.ContainsKey(room) && rooms[room].Ready)
                    {
                        SentByteLine(ns, "C1SQ," + room);
                        netchers.Add(uid, ny);
                        rooms[room].watchers.Add(uid);
                    }
                    else
                        SentByteLine(ns, "C1SQ,0");
                }
                catch (IOException)
                {
                    netchers.Remove(uid);
                }
            }
            else if (data.StartsWith("C4CO,")) // Reconnection Request
            {
				bool foundOrg = false;
                string[] blocks = data.Split(',');
                string user = blocks[1];
                int wantedRoom = int.Parse(blocks[2]);
                string cip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString() + "|"
                        + (socket.LocalEndPoint as IPEndPoint).Address.ToString();
                lock (losers)
                {
                    foreach (Neayer loser in losers)
                    {
                        if (loser.Ip == cip && loser.Name == user && location.ContainsKey(loser.Uid))
                        {
                            int curRoomNo = location[loser.Uid];
                            if (rooms.ContainsKey(curRoomNo) && (curRoomNo == wantedRoom || wantedRoom == 0))
                            {
                                ushort uid = RequestUid();
                                Neayer ny = new Neayer(user, 0, uid)
                                {
                                    HopeTeam = 0,
                                    Tunnel = socket,
                                    Ip = cip
                                };
                                neayers[uid] = ny;
                                substitudes[uid] = loser.Uid;
                                Console.WriteLine(user + " has required for re-connection.");
                                SentByteLine(ns, "C4RM," + uid + "," + loser.Uid + "," + curRoomNo + "," + user + ",#CD0");
								foundOrg = true; break;
								// TODO: set the room code as "#CD0" for testing.
								
                                // Accept it as substitute
                                //location[uid] = curRoomNo;
                                //Room curRoom = rooms[curRoomNo];
                                //if (curRoom.Ps != null)
                                //    WriteBytes(curRoom.Ps, "C3RA," + uid);
                            }
                        }
                    }
                }
				if (!foundOrg)
					SentByteLine(ns, "C4RM,0");
            }
        }

        private void HandleTalkInConnection(Socket socket, string msg)
        {
            int idx = msg.IndexOf(',');
            ushort ut = ushort.Parse(msg.Substring(0, idx));
            string nick = neayers[ut].Name;
            string content = msg.Substring(idx + 1);
            foreach (Room rm in rooms.Values)
            {
                if (rm.players.Contains(ut))
                {
                    lock (rm.players)
                    {
                        foreach (ushort py in rm.players)
                            SentByteLine(new NetworkStream(neayers[py].Tunnel),
                                "C1TK," + nick + "," + content);
                    }
                }
            }
        }

        private static bool IsTunnelAlive(Socket socket)
        {
            try
            {
                bool part1 = socket.Poll(1000, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                return !part1 && part2;
            }
            catch (IOException) { return false; }
        }

        //public void ListenToTalkSocket(Socket socket)
        //{
        //    try
        //    {
        //        NetworkStream ns = new NetworkStream(socket);
        //        string data = ReadByteLine(ns);
        //        while (data != null && data.StartsWith("C0TK,"))
        //        {
        //            int i1 = "C0TK".Length;
        //            int i2 = data.IndexOf(',', i1 + 1);
        //            int i3 = data.IndexOf(',', i2 + 1);
        //            ushort ut = ushort.Parse(data.Substring(i1 + 1, i2 - i1 - 1));
        //            string nick = data.Substring(i2 + 1, i3 - i2 - 1);
        //            string content = data.Substring(i3 + 1);
        //            foreach (Room rm in rooms.Values)
        //            {
        //                if (rm.players.Contains(ut))
        //                {
        //                    lock (rm.players)
        //                    {
        //                        foreach (ushort py in rm.players)
        //                            SentByteLine(new NetworkStream(neayers[py].Tunnel),
        //                                "C1TK," + nick + "," + content);
        //                    }
        //                }
        //            }
        //            data = ReadByteLine(ns);
        //        }
        //    }
        //    catch (IOException) { }
        //}

        #region Constructor and Utils

        private ushort curCount = 1, rmCount = 1;
        private ushort qsCount = 1001;

        private ushort RequestUid() { return curCount++; }
        private ushort RequestRoom() { return rmCount++; }
        private ushort RequestQSUid() { return qsCount++; }

        private Room RequestRoom(int teamCode, int selCode, int levelCode, string[] trainers)
        {
            lock (rooms)
            {
                foreach (Room rm in rooms.Values)
                {
                    if (!rm.Ready)
                    {
                        bool b1 = (teamCode == RuleCode.DEF_CODE) ||
                            ((rm.OptTeam == RuleCode.HOPE_NO) && (teamCode == RuleCode.HOPE_NO)) ||
                            ((rm.OptTeam == RuleCode.HOPE_YES) &&
                            ((teamCode & RuleCode.HOPE_YES) == RuleCode.HOPE_YES));
                        bool b2 = (selCode == RuleCode.DEF_CODE) || (rm.OptSel == selCode);
                        bool b3 = (levelCode == RuleCode.DEF_CODE) || (rm.OptLevel == levelCode);
                        if (b1 && b2 && b3)
                            return rm;
                    }
                }

                if (teamCode == RuleCode.DEF_CODE)
                    teamCode = RuleCode.HOPE_IP;
                else if ((teamCode & RuleCode.HOPE_YES) == RuleCode.HOPE_YES)
                    teamCode = RuleCode.HOPE_YES;
                if (selCode == RuleCode.DEF_CODE)
                    selCode = RuleCode.MODE_31;
                if (levelCode == RuleCode.DEF_CODE)
                    levelCode = RuleCode.LEVEL_RCM;
                Room room = new Room(RequestRoom(), teamCode, selCode, levelCode, trainers);
                Console.WriteLine("Room {0}# is created. [{1}][{2}][{3}]",
                    room.Number, room.OptTeam, room.OptSel, room.OptLevel);
                rooms[room.Number] = room; return room;
            }
        }

        public ZI(int port)
        {
            this.port = port;
            neayers = new Dictionary<ushort, Neayer>();
            rooms = new Dictionary<int, Room>();
            netchers = new Dictionary<ushort, Netcher>();
            losers = new HashSet<Neayer>();
            substitudes = new Dictionary<ushort, ushort>();
            location = new Dictionary<ushort, int>();
        }

        private static string ReadByteLine(NetworkStream ns)
        {
            byte[] byte2 = new byte[2];
            ns.Read(byte2, 0, 2);
            ushort value = (ushort)((byte2[0] << 8) + byte2[1]);
            byte[] actual = new byte[2048];
            if (value > 2048)
                value = 2048;
            ns.Read(actual, 0, value);
            return Encoding.Unicode.GetString(actual, 0, value);
        }

        private static void SentByteLine(NetworkStream ns, string value)
        {
            byte[] actual = Encoding.Unicode.GetBytes(value);
            int al = actual.Length;
            byte[] buf = new byte[al + 2];
            buf[0] = (byte)(al >> 8);
            buf[1] = (byte)(al & 0xFF);
            actual.CopyTo(buf, 2);
            ns.Write(buf, 0, al + 2);
            ns.Flush();
        }

        private static string ReadBytes(NamedPipeServerStream ps)
        {
            byte[] byte2 = new byte[4096];
            int readCount = ps.Read(byte2, 0, 4096);
            if (readCount > 0)
                return Encoding.Unicode.GetString(byte2, 0, readCount);
            else
                return "";
        }
        private static void WriteBytes(NamedPipeServerStream ps, string value)
        {
            byte[] byte2 = Encoding.Unicode.GetBytes(value);
            if (byte2.Length > 0)
                ps.Write(byte2, 0, byte2.Length);
            ps.Flush();
        }
        #endregion Constructor and Utils

        public static void Main(string[] args)
        {
            Console.WriteLine("PSDCenter スタート.");
            IPAddress[] ipHost = Dns.GetHostAddresses(Dns.GetHostName());
            if (ipHost.Length > 0)
            {
                Console.WriteLine("本机IP合法地址为：");
                foreach (IPAddress ip in ipHost)
                {
                    string ipt = ip.ToString();
                    if (ipt.StartsWith("10.") || ipt.StartsWith("127.0.") || ipt.StartsWith("192.168."))
                        Console.WriteLine("    [内网地址]" + ipt);
                    else if (ipt.Contains(":"))
                        Console.WriteLine("    [IPv6地址]" + ipt);
                    else
                        Console.WriteLine("   " + ipt);
                }
            }
            else
                Console.WriteLine("本机目前网卡异常，仅支持本地模式。");
            new ZI(Base.NetworkCode.HALL_PORT).Run();
        }
    }
}
