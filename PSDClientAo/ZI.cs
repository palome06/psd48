using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.ClientAo
{
    public class ZI
    {
        internal class IchiPlayer
        {
            internal string Name { set; get; }
            internal int Avatar { set; get; }
            internal ushort Uid { set; get; }
        }

        private string name;
        private int avatar;
        private int teamCode, selCode, levelCode;
        private string[] trainer;

        private ushort uid;
        private int room;
        private List<IchiPlayer> roomMates;

        private string server;
        private int port;

        private const int playerCapactity = 6;
        private readonly bool record;
        private readonly bool msglog;

        public Base.VW.IVI VI { private set; get; }
        public AoDisplay AD { set; get; }
        public XIVisi XV { private set; get; }

        public Login.LoginDoor LoginDoor { private set; get; }

        #region Hall
        public ZI(string name, int avatar, string server, int port, int teamCode, int selCode,
            int levelCode, string[] trainer, bool record, bool msglog, AoDisplay ad)
        {
            this.name = name; this.avatar = avatar;
            this.server = server; this.port = port;
            this.teamCode = teamCode; this.selCode = selCode;
            this.levelCode = levelCode; this.trainer = trainer;
            this.record = record; this.msglog = msglog;
            roomMates = new List<IchiPlayer>();
            AD = ad; XV = null;
            LoginDoor = null;
        }

        public void StartHall()
        {
            VW.Cyvi cyvi = new VW.Cyvi(AD, record, msglog);
            VI = cyvi; VI.Init(); VI.SetInGame(false);

            TcpClient client = null;
            try
            {
                client = new TcpClient(server, port);
            }
            catch (SocketException) { cyvi.ReportNoServer(server); return; }
            NetworkStream tcpStream = client.GetStream();
            string trainerjoin = (this.trainer != null && trainer.Length > 0) ? ("," + string.Join(",", trainer)) : "";
            int version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision;
            SentByteLine(tcpStream, "C0CO," + name + "," + avatar + ","
                + teamCode + "," + selCode + "," + levelCode + "," + version + trainerjoin);
            Thread msgThread = new Thread(delegate()
            {
                try
                {
                    string readLine = VI.RequestTalk(uid);
                    while (readLine != null)
                    {
                        lock (tcpStream)
                            SentByteLine(tcpStream, "C0TK," + uid + "," + readLine);
                        readLine = VI.RequestTalk(uid);
                    }
                }
                catch (System.IO.IOException) { }
            });
            bool done = false;
            while (!done)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("C0XV,"))
                {
                    cyvi.ReportWrongVersion(line.Substring("C0XV,".Length));
                    return;
                }
                if (line.StartsWith("C0CN,"))
                {
                    uid = ushort.Parse(line.Substring("C1CO,".Length));
                    cyvi.SetNick(1, name, avatar);
                }
                else if (line.StartsWith("C1RM,"))
                {
                    string[] splits = line.Split(',');
                    room = int.Parse(splits[1]);
                    int pos = 2;
                    for (int i = 2; i < splits.Length; i += 3)
                    {
                        IchiPlayer ip = new IchiPlayer()
                        {
                            Uid = ushort.Parse(splits[i]),
                            Name = splits[i + 1],
                            Avatar = int.Parse(splits[i + 2])
                        };
                        roomMates.Add(ip);
                        if (ip.Uid != uid)
                            cyvi.SetNick(pos++, ip.Name, ip.Avatar);
                    }
                    cyvi.SetRoom(room);
                    msgThread.Start();
                }
                else if (line.StartsWith("C1NW,"))
                {
                    string[] splits = line.Split(',');
                    IchiPlayer ip = new IchiPlayer()
                    {
                        Uid = ushort.Parse(splits[1]),
                        Name = splits[2],
                        Avatar = int.Parse(splits[3])
                    };
                    roomMates.Add(ip);
                    if (ip.Uid != uid)
                        cyvi.SetNick(roomMates.Count, ip.Name, ip.Avatar);
                }
                else if (line.StartsWith("C1LV,"))
                {
                    ushort ut = ushort.Parse(line.Substring("C1LV,".Length));
                    for (int idx = 0; idx < roomMates.Count; ++idx)
                    {
                        IchiPlayer ip = roomMates[idx];
                        if (ip.Uid == ut)
                        {
                            roomMates.RemoveAt(idx);
                            break;
                        }
                    }
                    int pos = 2;
                    foreach (IchiPlayer ip in roomMates)
                    {
                        if (ip.Uid != uid)
                            cyvi.SetNick(pos++, ip.Name, ip.Avatar);
                    }
                    while (pos <= 6)
                        cyvi.SetNick(pos++, "", 0);
                }
                else if (line.StartsWith("C1SA,"))
                {
                    SentByteLine(tcpStream, "C1ST," + uid);
                    VI.SetInGame(true);
                    XV = new XIVisi(uid, name, teamCode, VI, server,
                        room, record, msglog, false, AD);
                    XV.RunAsync();
                    //client.Close();
                    done = true;
                }
                else if (line.StartsWith("C1TK,"))
                {
                    int idx = line.IndexOf(',', "C1TK,".Length);
                    string nick = Algo.Substring(line, "C1TK,".Length, idx);
                    string content = Algo.Substring(line, idx + 1, -1);
                    VI.Chat(content, nick);
                }
            }
            if (msgThread != null && msgThread.IsAlive)
                msgThread.Abort();
        }
        #endregion Hall
        #region HallWatcher
        public ZI(string name, string server, int port, bool record, Login.LoginDoor login)
        {
            this.name = name;
            this.server = server; this.port = port;
            this.record = record;
            //this.AD = ad;
            this.LoginDoor = login;
            this.AD = null;
        }

        public bool StartWatchHall()
        {
            TcpClient client = new TcpClient(server, port);
            NetworkStream tcpStream = client.GetStream();
            SentByteLine(tcpStream, "C0QI," + name);
            while (true)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("C0QJ,"))
                {
                    string[] splits = line.Split(',');
                    uid = ushort.Parse(splits[1]);
                    List<int> crooms = new List<int>();
                    for (int i = 2; i < splits.Length; ++i)
                        crooms.Add(int.Parse(splits[i]));
                    if (crooms.Count > 0)
                    {
                        if (LoginDoor != null)
                            LoginDoor.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                LoginDoor.ReportRoom(crooms);
                            }));
                        do
                        {
                            int gr = LoginDoor.DecidedRoom;
                            if (gr != 0 && crooms.Contains(gr))
                            {
                                SentByteLine(tcpStream, "C0QS," + gr);
                                break;
                            }
                            Thread.Sleep(500);
                        } while (true);
                    }
                    else
                    {
                        LoginDoor.ReportWatchFail(0);
                        return false;
                    }
                }
                else if (line.StartsWith("C1SQ,"))
                {
                    room = ushort.Parse(line.Substring("C1SQ,".Length));
                    if (room != 0)
                    {
                        LoginDoor.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            AoDisplay a0d = new AoDisplay(server, room, uid, name, record, msglog);
                            LoginDoor.Close();
                            a0d.Show();
                        }));
                        return true;
                    }
                    else
                    {
                        LoginDoor.ReportWatchFail(1);
                        client.Close();
                        return false;
                    }
                }
            }
        }
        #endregion HallWatcher
        #region ResumeHall
        private ZI(string name, string server, int port, bool record,
            bool msglog, int room, AoDisplay ad)
        {
            this.name = name; this.server = server; this.port = port;
            // selCode and pkgCode pending
            this.record = record; this.msglog = msglog;
            roomMates = new List<IchiPlayer>();
            this.room = room; AD = ad;
        }
        public static ZI CreateResumeHall(string name, string server,
            int port, bool record, bool msglog, int room, AoDisplay ad)
        {
            return new ZI(name, server, port, record, msglog, room, ad);
        }
        public void ResumeHall()
        {
            VW.Cyvi cyvi = new VW.Cyvi(AD, record, msglog);
            VI = cyvi; VI.Init(); VI.SetInGame(true);

            TcpClient client = new TcpClient(server, port);
            NetworkStream tcpStream = client.GetStream();
            SentByteLine(tcpStream, "C4CO," + name + "," + room);

            bool done = false;
            while (!done)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("C4RM,0"))
                {
                    done = true;
                    System.Windows.MessageBox.Show("重连被拒绝.");
                }
                else if (line.StartsWith("C4RM,")) // Reconnection case
                {
                    string[] parts = line.Split(',');
                    ushort centerUid = ushort.Parse(parts[1]); // AUid
                    ushort subUid = ushort.Parse(parts[2]); // Uid
                    int roomNumber = int.Parse(parts[3]);
                    string nick = parts[4];
                    string passcode = parts[5];
                    // start new connection...
                    VI.SetInGame(true);
                    cyvi.SetRoom(roomNumber);
                    XV = XIVisi.CreateInResumeHall(centerUid, subUid, name, VI,
                        server, roomNumber, passcode, record, msglog, AD);
                    XV.RunAsync();
                    done = true;
                }
            }
        }
        #endregion ResumeHall

        #region Utils Functions

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

        public static void SafeExecute(Action action, Action<Exception> handler)
        {
            //try { action(); }
            //catch (Exception ex) { handler(ex); throw ex; }
            action();
        }

        #endregion Utils Functions
    }
}
