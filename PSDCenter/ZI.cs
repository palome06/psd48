using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PSD.Base.Rules;
using System.IO;
using System.Diagnostics;

namespace PSD.PSDCenter
{
    public class ZI
    {
        public const int MSG_SIZE = 4096;

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
        // token to terminate all running thread when closed
        // private CancellationTokenSource ctoken;

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
            string data = Base.VW.WHelper.ReadByteLine(ns);
            if (data == null) { return; }
            else if (data.StartsWith("C0CO,"))
            {
                string[] blocks = data.Split(',');
                string user = blocks[1];
                ushort avatar = ushort.Parse(blocks[2]);
                int teamCode = int.Parse(blocks[3]);
                int selCode = int.Parse(blocks[4]);
                int levelCode = int.Parse(blocks[5]);
                
                var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                if (blocks.Length <= 6 || int.Parse(blocks[6]) != ass.Version.Revision)
                { // version error, report exit
                    Console.WriteLine("{0} has tried connecting with a wrong version.", user);
                    Base.VW.WHelper.SentByteLine(ns, "C0XV," + ass.Version.ToString());
                    socket.Close();
                    return;
                }

                string[] trainers = null;
                int trainerOffset = 7;
                if (blocks.Length > trainerOffset)
                {
                    trainers = new string[blocks.Length - trainerOffset];
                    for (int i = trainerOffset; i < blocks.Length; ++i)
                        trainers[i - trainerOffset] = blocks[i];
                }

                ushort uid = RequestUid();
                Neayer ny = new Neayer(user, avatar, uid)
                {
                    HopeTeam = teamCode - RuleCode.HOPE_NOTCARE,
                    Tunnel = socket,
                    Ip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString() + "|"
                        + (socket.LocalEndPoint as IPEndPoint).Address.ToString()
                };
                Console.WriteLine("{0} has entered the hall.", user);
                Base.VW.WHelper.SentByteLine(ns, "C0CN," + uid);
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
                        Base.VW.WHelper.SentByteLine(ns, "C1RM," + reqRoom.Number + members);
                        Console.WriteLine("{0} is allocated with room {1}#.", user, reqRoom.Number);
                        location[uid] = reqRoom.Number;
                        foreach (ushort nyru in reqRoom.players)
                        {
                            Neayer nyr = neayers[nyru];
                            Base.VW.WHelper.SentByteLine(new NetworkStream(nyr.Tunnel),
                                "C1NW," + uid + "," + ny.Name + "," + avatar);
                        }
                        reqRoom.players.Add(uid);
                        if (reqRoom.players.Count >= playerCapacity)
                            reqRoom.Ready = true;
                    }
                    if (reqRoom.Ready)
                        reqRoom.CreateRoomPkg();
                    // Wait for C1ST message to terminate the socket
                    do
                    {
                        string reply = Base.VW.WHelper.ReadByteLine(ns);
                        if (string.IsNullOrEmpty(reply))
                        {
                            socket.Close(); // close twice would lead to IOException
                            SomeoneHasLeave(uid, reqRoom); break;
                        }
                        else if (reply.StartsWith("C0TK,"))
                            HandleTalkInConnection(socket, reply.Substring("C0TK,".Length));
                        else if (reply.StartsWith("C1ST,"))
                            break;
                    } while (true);
                    socket.Close();
                }
                catch (IOException) { SomeoneHasLeave(uid, reqRoom); }
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
                    Base.VW.WHelper.SentByteLine(ns, "C0QJ," + uid + rms);
                    string reply = Base.VW.WHelper.ReadByteLine(ns);
                    while (!reply.StartsWith("C0QS,"))
                        reply = Base.VW.WHelper.ReadByteLine(ns);

                    int room = int.Parse(reply.Substring(reply.IndexOf(',') + 1));
                    if (rooms.ContainsKey(room) && rooms[room].Ready)
                    {
                        Base.VW.WHelper.SentByteLine(ns, "C1SQ," + room);
                        netchers.Add(uid, ny);
                        rooms[room].watchers.Add(uid);
                    }
                    else
                        Base.VW.WHelper.SentByteLine(ns, "C1SQ,0");
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
                                Console.WriteLine("{0} has required for re-connection.", user);
                                Base.VW.WHelper.SentByteLine(ns, "C4RM," + uid + "," +
                                    loser.Uid + "," + curRoomNo + "," + user + ",#CD0");
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
                {
                    Console.WriteLine("{0} is rejected when re-connecting.", user);
                    Base.VW.WHelper.SentByteLine(ns, "C4RM,0");
                }
            }
            else if (data.StartsWith("C3HI,")) // hello from pkg to replace pipestream
            {
                ushort roomNum = ushort.Parse(data.Substring("C3HI,".Length));
                Room reqRoom = rooms[roomNum];
                if (reqRoom != null)
                {
                    lock (reqRoom.players)
                    {
                        foreach (ushort nyru in reqRoom.players)
                        {
                            Neayer nyr = neayers[nyru];
                            Base.VW.WHelper.SentByteLine(new NetworkStream(nyr.Tunnel), "C1SA,0");
                        }
                    }
                    string line;
                    while (!string.IsNullOrEmpty(line = Base.VW.WHelper.ReadByteLine(ns)))
                    {
                        if (line.StartsWith("C3LV")) // Terminate unexpected
                        {
                            Console.WriteLine("Room {0}# is forced closed.", reqRoom.Number);
                            rooms.Remove(reqRoom.Number);
                            break;
                        }
                        else if (line.StartsWith("C3TM")) // Terminate gracefully
                        {
                            Console.WriteLine("Room {0}# terminates gracefully.", reqRoom.Number);
                            rooms.Remove(reqRoom.Number);
                            break;
                        }
                        else if (line.StartsWith("C3LS"))
                        {
                            ushort ut = ushort.Parse(line.Substring("C3LS,".Length));
                            Console.WriteLine("Player {0}#[{1}] loses connection with Room {2}#.",
                                ut, neayers[ut].Name, reqRoom.Number);
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
                            Console.WriteLine("Player {0}#[{1}] has been back to Room {2}#.",
                                ut, neayers[ut].Name, reqRoom.Number);
                        }
                        else if (line.StartsWith("C3RV"))
                        {
                            Console.WriteLine("Room {0}# is recovered.", reqRoom.Number);
                        }
                        else if (line.StartsWith("C3HX"))
                        {
                            Console.WriteLine("Room {0}# has shutdown before getting ready.", reqRoom.Number);
                            rooms.Remove(reqRoom.Number);
                            break;
                        }
                    }
                }
            }
        }

        private void SomeoneHasLeave(ushort uid, Room reqRoom)
        {
            neayers[uid].Alive = false;
            Console.WriteLine("Player {0}#[{1}] left the hall.", uid, neayers[uid].Name);
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
                            Base.VW.WHelper.SentByteLine(new NetworkStream(nyr.Tunnel), "C1LV," + uid);
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
                            Base.VW.WHelper.SentByteLine(new NetworkStream(neayers[py].Tunnel),
                                "C1TK," + nick + "," + content);
                    }
                }
            }
        }

        private static bool IsTunnelAlive(Socket socket)
        {
            try
            {
                return socket.Available == 0 && !socket.Poll(1000, SelectMode.SelectRead);
            }
            catch (SocketException) { return false; }
            catch (ObjectDisposedException) { return false; }
            catch (IOException) { return false; }
        }

        //public void ListenToTalkSocket(Socket socket)
        //{
        //    try
        //    {
        //        NetworkStream ns = new NetworkStream(socket);
        //        string data = Base.VW.WHelper.ReadByteLine(ns);
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
        //                            Base.VW.WHelper.SentByteLine(new NetworkStream(neayers[py].Tunnel),
        //                                "C1TK," + nick + "," + content);
        //                    }
        //                }
        //            }
        //            data = Base.VW.WHelper.ReadByteLine(ns);
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
        #endregion Constructor and Utils

        public static void Main(string[] args)
        {
            Console.WriteLine("PSDCenter Launched.");
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
            // Wangpengfei.Meme();
        }
    }

    class Wangpengfei
    {
        public static void Meme()
        {
            CancellationTokenSource cto = new CancellationTokenSource();
            BlockingCollection<int> bc = new BlockingCollection<int>();
            Task t1 = Task.Factory.StartNew(() => 
            {
                int count = 0;
                while (true) {
                    // try {
                        Thread.Sleep(1000);
                        bc.Add(count++, cto.Token);
                    // } catch (Exception e) { Console.WriteLine(e.Message); break; }
                }
            }, cto.Token);
            Task t2 = Task.Factory.StartNew(() => 
            {
                while (true) {
                    // try {
                        int ct = bc.Take(cto.Token);
                        Console.WriteLine("Running at {0}#.", ct);
                    // } catch (Exception e) { Console.WriteLine(e.Message); break; }
                    //if (cto.Token.IsCancellationRequested) { break; }
                }
            }, cto.Token);
            Task t3 = Task.Factory.StartNew(() =>
            {
                int i = 0, j = 4;
                if (j / i == 1)
                    ++j;
            }, cto.Token);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3500);
                Console.WriteLine("3500!");
                cto.Cancel();

                Thread.Sleep(1500);
                Console.WriteLine("t1={0},t2={1}", t1.IsCanceled, t2.IsCanceled);
                Console.WriteLine("t1={0},t3={1}", t1.Exception == null ? "null" : t1.Exception.ToString(),
                    t3.Exception == null ? "null" : string.Join("\n", t3.Exception.InnerExceptions));
            });
            Console.ReadLine();
        }
        // WI.BCast -> {}
        // WI.Deliver(params[] content) -> {}
    }
}
