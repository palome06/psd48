using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PSD.Base.Rules;
using System.Net.Sockets;

namespace PSD.ClientZero
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

        #region Hall
        public ZI(string name, int avatar, string server, int port,
            int teamCode, int selCode, int levelCode, string[] trainer, bool record, bool msglog)
        {
            this.name = name; this.avatar = avatar;
            this.server = server; this.port = port;
            this.teamCode = teamCode; this.selCode = selCode;
            this.levelCode = levelCode; this.trainer = trainer;
            this.record = record; this.msglog = msglog;
            roomMates = new List<IchiPlayer>();
        }

        public void StartHall()
        {
            VW.Ayvi ayvi = new VW.Ayvi(playerCapactity, record, msglog);
            VI = ayvi;
            VI.Init(); VI.SetInGame(false);

            TcpClient client = new TcpClient(server, port);
            NetworkStream tcpStream = client.GetStream();
            int version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision;
            string trainerjoin = (this.trainer != null && trainer.Length > 0) ? ("," + string.Join(",", trainer)) : "";
            SentByteLine(tcpStream, "C0CO," + name + "," + avatar + ","
                + teamCode + "," + selCode + "," + levelCode + "," + version + trainerjoin);

            Thread msgThread = new Thread(delegate()
            {
                try
                {
                    //string readLine = Console.ReadLine();
                    string readLine = VI.RequestTalk(uid);
                    while (readLine != null)
                    {
                        lock (tcpStream)
                            SentByteLine(tcpStream, "C0TK," + uid + "," + readLine);
                        readLine = VI.RequestTalk(uid);
                        //readLine = Console.ReadLine();
                    }
                }
                catch (System.IO.IOException) { }
            });
            bool done = false;
            while (!done)
            {
                string line = ReadByteLine(tcpStream);
                if (line.StartsWith("G0XV,"))
                {
                    string expectVersion = line.Substring("C0XV,".Length);
                    VI.Cout(uid, "Version Missmatch. Expect " + expectVersion + ", please get updated.", uid);

                }
                else if (line.StartsWith("C0CN,"))
                {
                    uid = ushort.Parse(line.Substring("C1CO,".Length));
                    VI.Cout(uid, "Allocated with uid {0}", uid);
                }
                else if (line.StartsWith("C1RM,"))
                {
                    string[] splits = line.Split(',');
                    room = int.Parse(splits[1]);
                    for (int i = 2; i < splits.Length; i += 3)
                        roomMates.Add(new IchiPlayer()
                        {
                            Uid = ushort.Parse(splits[i]),
                            Name = splits[i + 1],
                            Avatar = int.Parse(splits[i + 2])
                        });
                    if (roomMates.Count > 0)
                        VI.Cout(uid, "您进入{0}#房间，其它成员为：{1}", room,
                            string.Join(",", roomMates.Select(p => "[" + p.Uid + "]" + p.Name)));
                    else
                        VI.Cout(uid, "您进入{0}#房间并成为房主。", room);
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
                    VI.Cout(uid, "新成员{0}加入房间{1}#。", "[" + ip.Uid + "]" + ip.Name, room);
                }
                else if (line.StartsWith("C1LV,"))
                {
                    ushort ut = ushort.Parse(line.Substring("C1LV,".Length));
                    foreach (IchiPlayer ip in roomMates)
                    {
                        if (ip.Uid == ut)
                        {
                            roomMates.Remove(ip);
                            VI.Cout(uid, "{0}离开房间。", "[" + ip.Uid + "]" + ip.Name);
                            break;
                        }
                    }
                }
                else if (line.StartsWith("C1SA,"))
                {
                    SentByteLine(tcpStream, "C1ST," + uid);
                    Console.WriteLine("Start XIClient");
                    //VI.Init();
                    VI.SetInGame(true);
                    XIClient xic = new XIClient(uid, name, teamCode, VI, server, room, record, msglog, false);
                    xic.RunAsync();
                    //client.Close();
                    done = true;
                }
                else if (line.StartsWith("C1TK,")) // Talk case
                {
                    int idx = line.IndexOf(',', "C1TK,".Length);
                    string nick = Util.Substring(line, "C1TK,".Length, idx);
                    string content = Util.Substring(line, idx + 1, -1);
                    VI.Chat(content, nick);
                }
            }
            if (msgThread != null && msgThread.IsAlive)
                msgThread.Abort();
        }
        #endregion Hall
        #region HallWatcher
        public ZI(string name, string server, int port, bool record)
        {
            this.name = name;
            this.server = server; this.port = port;
            this.record = record;
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
                        do
                        {
                            Console.WriteLine("=>请选择您将旁观的房间(" + string.Join(",", crooms) + ")");
                            string inputHouse = Console.ReadLine().Trim();
                            int house = int.Parse(inputHouse);
                            if (crooms.Contains(house))
                            {
                                SentByteLine(tcpStream, "C0QS," + house);
                                break;
                            }
                        } while (true);
                    }
                    else
                    {
                        Console.WriteLine("<=== 当前没有正在游戏的房间。");
                        client.Close();
                        return false;
                    }
                }
                else if (line.StartsWith("C1SQ,"))
                {
                    room = ushort.Parse(line.Substring("C1SQ,".Length));
                    if (room != 0)
                    {
                        Console.WriteLine("Start XIClient For Watcher");
                        VW.Ayvi ayvi = new VW.Ayvi(playerCapactity, record, msglog);
                        VI = ayvi; VI.Init();
                        XIClient xic = new XIClient(uid, name, teamCode,
                            VI, server, room, record, msglog, true);
                        //client.Close();
                        xic.RunAsync();
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("<=== 申请旁观失败。");
                        client.Close();
                        return false;
                    }
                }
            }
        }
        #endregion HallWatcher
        #region ResumeHall
        private ZI(string name, string server, int port, bool record, bool msglog, int room)
        {
            this.name = name; this.server = server; this.port = port;
            // selCode and pkgCode pending
            this.record = record; this.msglog = msglog;
            roomMates = new List<IchiPlayer>();
            this.room = room;
        }
		public static ZI CreateResumeHall(string name, string server, int port, bool record, bool msglog, int room) {
			return new ZI(name, server, port, record, msglog, room);
		}
        public void ResumeHall()
        {
            VW.Ayvi ayvi = new VW.Ayvi(playerCapactity, record, msglog);
            VI = ayvi;
            VI.Init(); VI.SetInGame(false);

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
                    Console.WriteLine("Re-connection Rejected.");
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
                    Console.WriteLine("Restart XIClient");
                    VI.SetInGame(true);
                    XIClient.CreateInResumeHall(centerUid, subUid, name, VI,
                        server, roomNumber, passcode, record, msglog).RunAsync();
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

        #endregion Utils Functions

        private static string GetValue(string[] args, int index, string hint)
        {
            if (args != null && args.Length > index)
                return args[index];
            else
            {
                Console.WriteLine("=>" + hint);
                return Console.ReadLine().Trim();
            }
        }

        public static void Main(string[] args)
        {
            string login = GetValue(args, 0, "请选择登录方式(0:直接登录，1:大厅，2:大厅重连)");
            if (login == "AKB48Show!")
            {
                EncryptFile();
                return;
            }
            bool hall = (login == "1");
            bool reset = (login == "2");
            string server = GetValue(args, 1, "请输入服务器地址");
            string name;
            do
            {
                name = GetValue(args, 2, "请输入您的用户名");
                if (name.Length > 0 && !name.Contains(","))
                    break;
                else
                    Console.WriteLine("<===抱歉，请采用文雅用户名。");
            } while (true);
            bool record = (GetValue(args, 3, "请选择是否保存战果(0:是，1:否)") == "0");
            bool msglog = (GetValue(args, 4, "是否保存日志记录(0:是，1:否)") == "0");
            if (reset)
            {
                string grmStr = GetValue(args, 5, "请输入您要重连的房间号(0则最近匹配)");
                int grm;
                if (!int.TryParse(grmStr, out grm))
                    grm = 0;
                int port = Base.NetworkCode.HALL_PORT;
                ZI.CreateResumeHall(name, server, port, record, msglog, grm).ResumeHall();
            }
            else if (hall)
            {
                bool watch = (GetValue(args, 5, "选择是否旁观(0:是，1:否)") == "0");
                if (!watch)
                {
                    string team = GetValue(args, 6, "选队倾向(0:随意，1:随机选队，2:不选队，" +
                        "3:选红队，4:选蓝队，5:IP优先匹配)");
                    int teamCode;
                    switch (team)
                    {
                        case "1": teamCode = RuleCode.HOPE_NOTCARE; break;
                        case "2": teamCode = RuleCode.HOPE_NO; break;
                        case "3": teamCode = RuleCode.HOPE_AKA; break;
                        case "4": teamCode = RuleCode.HOPE_AO; break;
                        case "5": teamCode = RuleCode.HOPE_IP; break;
                        default: teamCode = RuleCode.DEF_CODE; break;
                    }
                    string sel = GetValue(args, 7,
                        "选将模式(31:三选一/RM:随机/BP:禁选/RD:轮选/ZY:昭鹰/CP:协同/IN:客栈/SS:北软/CJ:召唤/TC:六明六暗)");
                    int selCode = RuleCode.CastMode(sel.Trim().ToUpper());
                    int levelCode;
                    string level = GetValue(args, 8, "房间等级，(1:新手场/2:标准场/3:高手场/4:至尊场/5:界限突破场，后附+为特训");
                    bool isTrain = level.Contains("+");
                    string[] trainer = isTrain ? Util.Substring(level, level.IndexOf('+') + 1, -1).Split(',') : null;
                    if (isTrain)
                        level = level.Substring(0, level.IndexOf('+'));
                    if (!int.TryParse(level, out levelCode) || levelCode < 0 || levelCode > RuleCode.LEVEL_IPV)
                        levelCode = RuleCode.LEVEL_RCM;
                    else if (levelCode == 0)
                        levelCode = RuleCode.DEF_CODE;
                    else
                        levelCode = (levelCode << 1) | (isTrain ? RuleCode.LEVEL_TRAIN_MASK : 0);
                    //string server = "127.0.0.1";
                    //string name = "Yuan";
                    int port = Base.NetworkCode.HALL_PORT;
                    int avatar = new Random().Next(120);
                    ZI xin = new ZI(name, avatar, server, port, teamCode, selCode, levelCode, trainer, record, msglog);
                    xin.StartHall();
                }
                else
                {
                    int port = Base.NetworkCode.HALL_PORT;
                    ZI xin = new ZI(name, server, port, record);
                    xin.StartWatchHall();
                }
            }
            else
            {
                bool watch = (GetValue(args, 5, "选择是否旁观(0:是，1:否)") == "0");
                string room = GetValue(args, 6, "请输入您的房间号(0为默认)");
                int roomCode = int.Parse(room) + Base.NetworkCode.DIR_PORT;
                if (roomCode >= 65535 || roomCode < 1024)
                    roomCode = Base.NetworkCode.DIR_PORT;
                string team = GetValue(args, 7, "请输入您希望加入的队伍 (1/2，0为随意)");
                int teamCode = (team == "0" || team == "1" || team == "2") ? team[0] - '0' : 0;

                //string server = "127.0.0.1";
                //int port = 40201;
                //string name = "Yuan";
                int avatar = new Random().Next(120);

                XIClient.CreateInDirectConnect(server, roomCode, name, avatar,
                    teamCode, record, msglog, watch).RunAsync();
            }
            while (true)
                Thread.Sleep(10000);
        }

        private static void EncryptFile()
        {
            Console.WriteLine("=>今天天气还好么？吃了没？吃了。吃了再吃点~");
            string ans = Console.ReadLine().Trim();
            if (ans == "AKB48Show!")
            {
                int version = 0; ushort uid = 0;
                bool issv = false;
                Console.Write("  ");
                string filepath = Console.ReadLine().Trim();
				string otpath = filepath + ".txt";
                //Console.Write(" ");
                //string otpath = Console.ReadLine().Trim();
                var iter = System.IO.File.ReadLines(filepath).GetEnumerator();
                if (iter.MoveNext())
                {
                    string firstLine = iter.Current;
                    string[] firsts = firstLine.Split(' ');
                    if (firsts[0].StartsWith("VERSION="))
                        version = int.Parse(firsts[0].Substring("VERSION=".Length));
                    if (firsts[1].StartsWith("UID="))
                        uid = ushort.Parse(firsts[1].Substring("UID=".Length));
                    else if (firsts[1].StartsWith("ISSV="))
                        issv = true;
                }
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(otpath, true))
                {
                    if (issv)
                        sw.WriteLine("VERSION={0} ISSV=1", version);
                    else
                        sw.WriteLine("VERSION={0} UID={1}", version, uid);
                    sw.Flush();
                    while (iter.MoveNext())
                    {
                        string line = iter.Current;
                        if (version >= 99)
                        {
                            line = Base.LogES.DESDecrypt(line, "AKB48Show!",
                                (version * version).ToString());
                        }
                        sw.WriteLine(line);
                    }
                };
            }
        }
    }
}
