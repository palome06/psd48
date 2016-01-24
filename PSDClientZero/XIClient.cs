using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using PSD.Base;
using PSD.Base.Card;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.ClientZero
{
    public class XIClient
    {
        #region Basic Members
        public Base.VW.IVI VI { private set; get; }
        public Base.VW.IWICL WI { private set; get; }

        public Base.LibGroup Tuple { private set; get; }

        private bool joined; // whether joined (SF or not)
        private ushort m_uid;
        // uid in room, referenced in game procedure
        // 2 ways to get updated. H0SD events or H09G
        private ushort Uid
        {
            set
            {
                m_uid = value;
                if (!joined)
                {
                    var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                    Console.Title = ass.Name + " v" + ass.Version + " - " + Uid;
                }
            }
            get { return m_uid; }
        }
        // uid in center, referenced in tracking player's status
        private readonly ushort auid;
        private readonly string name;
        private readonly int avatar;

        private readonly string server;
        private readonly int port;

        private const int totalPlayer = 6;       
        public int Room { private set; get; }

        private int hopeTeam;
        public int SelMode { private set; get; }
        public int LevelCode { private set; get; }
        private Base.Rules.Casting casting;
        //private List<int> selCandidates;
        // Displays Members
        internal ZeroDisplay zd;

        public ZeroField Z0F { private set; get; }
        public ZeroMe Z0M { private set; get; }
        public ZeroPiles Z0P { private set; get; }
        public IDictionary<ushort, ZeroPlayer> Z0D { private set; get; }

        private bool GameGraceEnd { set; get; }

        //public IDictionary<ushort, int> Heros { private set; get; }
        // list of running threads handling events
        private List<Thread> listOfThreads;

        private Queue<string> unhandledMsg;

        public Log Log { private set; get; }

        private void CommonConstruct()
        {
            Tuple = new LibGroup();
            listOfThreads = new List<Thread>();
            zd = new ZeroDisplay(this);
            Z0D = new Dictionary<ushort, ZeroPlayer>();
            unhandledMsg = new Queue<string>();
            GameGraceEnd = false;
        }
        // Constructor 1#: Used for Hall setting
        public XIClient(ushort uid, string name, int teamCode, Base.VW.IVI vi,
            string server, int room, bool record, bool msglog, bool watcher)
        {
            joined = false;
            this.auid = uid; this.name = name; this.VI = vi;
            hopeTeam = teamCode;
            this.server = server;
            this.Room = room;
            this.port = Base.NetworkCode.HALL_PORT + room;

            VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, hopeTeam, uid, this);
            Log = new Log(); Log.Start(auid, record, msglog, 0);
            bywi.Log = Log;
            if (VI is VW.Ayvi)
                (VI as VW.Ayvi).Log = Log;
            WI = bywi; bywi.StartConnect(watcher);
            VI.Cout(uid, "游戏开始咯~");
            
            CommonConstruct();
        }
        // Constructor 2#: Used for SF setting, use decided WI and VI
        public XIClient(ushort uid, Base.VW.IWICL wi, Base.VW.IVI vi)
        {
            joined = true;
            this.auid = this.Uid = uid;
            WI = wi; VI = vi;
            this.hopeTeam = 0;
            Log = new Log(); Log.Start(auid, false, false, 0);
            CommonConstruct();
        }
        // Constructor 3#: Used for Direct Connection
        private XIClient(string server, int port, string name,
            int avatar, int hopeTeam, bool record, bool msglog, bool watcher)
        {
            joined = false;
            this.server = server; this.port = port;
            this.name = name; this.avatar = avatar;
            this.hopeTeam = hopeTeam;

            VW.Ayvi ayvi = new VW.Ayvi(totalPlayer, record, msglog);
            VI = ayvi;
            VI.Init(); ayvi.SetInGame(true);
            VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, hopeTeam, 0, this);
            WI = bywi;
            
            Log = new Log(); Log.Start(Uid, record, msglog, 0);
            if (!bywi.StartConnectDirect(watcher, VI))
            {
                VI.Cout(Uid, "咦，您是不是掉线或者连错人了:-(");
                auid = 0; return;
            }
            VI.Cout(Uid, watcher ? "您开始旁观~" : "游戏开始咯~");
            this.auid = bywi.Uid;
            bywi.Log = Log; ayvi.Log = Log;
            WI.Send("C2ST," + Uid, Uid, 0);
            CommonConstruct();
        }
        public static XIClient CreateInDirectConnect(string server, int port, string name,
            int avatar, int hopeTeam, bool record, bool msglog, bool watcher)
        {
            return new XIClient(server, port, name, avatar, hopeTeam, record, msglog, watcher);
        }
        // Constructor 4#: Used for ResumeHall
		// passCode is the password for a settled room
        private XIClient(ushort newUid, ushort oldUid, string name, Base.VW.IVI vi,
            string server, int room, string passCode, bool record, bool msglog)
        {
            joined = false;
            this.auid = newUid;
            this.name = name; this.VI = vi;
            this.server = server;
            this.Room = room;
            this.port = Base.NetworkCode.HALL_PORT + room;
            
            VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, hopeTeam = 0, newUid, this);
            Log = new Log(); Log.Start(auid, record, msglog, 0);
            bywi.Log = Log;
            if (VI is VW.Ayvi)
                (VI as VW.Ayvi).Log = Log;
            WI = bywi; bywi.StartConnectResume(oldUid, passCode);
            // After that, Uid get updated.
            this.Uid = bywi.Uid;
            VI.Cout(Uid, "游戏继续啦~");

            CommonConstruct();
        }
        public static XIClient CreateInResumeHall(ushort newUid, ushort oldUid, string name,
            Base.VW.IVI vi, string server, int room, string passCode, bool record, bool msglog)
        {
            return new XIClient(newUid, oldUid, name, vi, server, room, passCode, record, msglog);
        }

        // Hero selection proceeding
        public void RunAsync()
        {
            if (auid == 0) // Connection Fail, do nothing
                return;
            new Thread(() => ZI.SafeExecute(() =>
            {
                while (true)
                {
                    string acmd = VI.Request(Uid);
                    if (acmd != null)
                        OnRequestLocalCmd(acmd);
                }
            }, delegate(Exception e) { Log.Logg(e.ToString()); })).Start();
            if (!joined)
                new Thread(() => ZI.SafeExecute(() =>
                {
                    while (true)
                    {
                        string say = VI.RequestTalk(Uid);
                        if (say != null && WI != null)
                            WI.SendDirect(say, Uid);
                    }
                }, delegate(Exception e) { Log.Logg(e.ToString()); })).Start();
            SingleThreadMessageStart();
            if (!joined)
                new Thread(() => ZI.SafeExecute(() =>
                {
                    while (true)
                    {
                        string hear = WI.Hear();
                        if (hear != null)
                            HandleYMessage(hear);
                    }
                }, delegate(Exception e) { Log.Logg(e.ToString()); })).Start();
            new Thread(() => ZI.SafeExecute(() => 
            {
                while (true)
                {
                    string readLine = WI.Recv(Uid, 0);
                    //if (uid == 1)
                    //VI.Cout(uid, "★●▲■" + readLine + "★●▲■");
                    if (!string.IsNullOrEmpty(readLine))
                    {
                        lock (unhandledMsg)
                        {
                            unhandledMsg.Enqueue(readLine);
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
            }, delegate(Exception e) { Log.Logg(e.ToString()); })).Start();
        }
        // called at the beginning or Cin-interrupted
        //public void SingleThreadMessageStart(List<string> list)
        public void SingleThreadMessageStart()
        {
            //ParameterizedThreadStart ParStart = new ParameterizedThreadStart(SingleThreadMessage);
            //Thread myThread = new Thread(ParStart);
            Thread myThread = new Thread(() => ZI.SafeExecute(() => SingleThreadMessage(),
                        delegate(Exception e) { Log.Logg(e.ToString()); }));
            lock (listOfThreads)
            {
                if (listOfThreads.Count > 100)
                {
                    List<Thread> nt = listOfThreads.Where(p => p.IsAlive).ToList();
                    listOfThreads.Clear();
                    listOfThreads.AddRange(nt);
                }
                listOfThreads.Add(myThread);
            }
            //myThread.Start(list);
            myThread.Start();
        }

        //private void SingleThreadMessage(object olist) {
        private void SingleThreadMessage()
        {
            //List<string> list = olist as List<string>;
            //unhandledMsg = queue;
            //unhandledMsg.Clear();
            //foreach (string str in list)
            //    unhandledMsg.Enqueue(str);
            //if (unhandledMsg.Count > 0)
            //    unhandledMsg.Dequeue();
            while (true)
            {
                string peek = "";
                lock (unhandledMsg)
                {
                    if (unhandledMsg.Count > 0)
                    {
                        //string peek = unhandledMsg.Peek();
                        //bool interrupted = HMMain(peek);
                        //if (interrupted)
                        //    break;
                        //else if (unhandledMsg.Count > 0)
                        //    unhandledMsg.Dequeue();
                        peek = unhandledMsg.Dequeue();
                    }
                    else
                        peek = "";
                }
                if (peek != "")
                {
                    bool interrupted = HMMain(peek);
                    if (interrupted)
                        break;
                } else
                    Thread.Sleep(100);
            }
        }

        private bool StartCinEtc()
        {
            //List<string> newList = new List<string>();
            //if (unhandledMsg.Count > 0)
            //    unhandledMsg.Dequeue();
            //newList.AddRange(unhandledMsg);
            //if (unhandledMsg.Count > 0)
            //    unhandledMsg.Dequeue();
            SingleThreadMessageStart();
            VI.OpenCinTunnel(Uid);
            return true;
        }
        #endregion Basic Members

        #region Board & Requests

        private void OnRequestLocalCmd(string cmd)
        {
            if (cmd.Equals("A") && Uid < 1000) // list out my card, not valid for watcher
            {
                VI.Cout(Uid, "{0}", Z0M.ToString());
            }
            else if (cmd.Equals("F")) // list out current monster
            {
                VI.Cout(Uid, "{0}", Z0F.ToString());
            }
            else if (cmd.StartsWith("G")) // list out Player
            {
                if (cmd.Length > 1)
                {
                    ushort me = (ushort)(cmd[1] - '0');
                    if (Z0D.ContainsKey(me))
                    {
                        ZeroPlayer zp = Z0D[me];
                        VI.Cout(Uid, "{0}", zp.ToString());
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("***************");
                    List<ushort> lt = Z0D.Keys.ToList(); lt.Sort();
                    foreach (ushort ut in lt)
                        sb.Append(Z0D[ut].ToStringSingleMask());
                    VI.Cout(Uid, "{0}", sb);
                }
            }
            else if (cmd.Equals("P")) // list out Piles
            {
                VI.Cout(Uid, "{0}", Z0P.ToString());
            }
            else if (cmd.StartsWith("I") && cmd.Length >= 3)
            {
                char tps = cmd[1];
                ushort code;
                string cmdrst = cmd.Substring(2);
                if (tps == 'M')
                {
                    if (ushort.TryParse(cmdrst, out code))
                    {
                        string line = zd.MonsterInfo(code);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                    else
                    {
                        string line = zd.MonsterInfo(cmdrst);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                }
                else if (tps == 'T')
                {
                    if (ushort.TryParse(cmdrst, out code))
                    {
                        string line = zd.TuxInfo(code);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                    else
                    {
                        string line = zd.TuxInfo(cmdrst);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                }
                else if (tps == 'E')
                {
                    if (ushort.TryParse(cmdrst, out code))
                    {
                        string line = zd.EveInfo(code);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                    else
                    {
                        string line = zd.EveInfo(cmdrst);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                }
                else if (tps == 'H')
                {
                    if (ushort.TryParse(cmdrst, out code))
                    {
                        string line = zd.HeroInfo(code);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                    else
                    {
                        string line = zd.HeroInfo(cmdrst);
                        if (!string.IsNullOrEmpty(line))
                            VI.Cout(Uid, "{0}", line);
                    }
                }
                else if (tps == 'S')
                {
                    string line = zd.SkillInfo(cmdrst);
                    if (!string.IsNullOrEmpty(line))
                        VI.Cout(Uid, "{0}", line);
                }
            }
            else if (cmd.StartsWith("H"))
                VI.Cout(Uid, "{0}", zd.GetHelp());
        }

        #endregion Board & Requests

        #region Message Main
        // return whether can be interrupted
        private bool HMMain(object pararu)
        {
            string readLine = (string)pararu;
            //Log.Logger(readLine);
            // start a new thread to handle with the message
            int cdx = readLine.IndexOf(',');
            string cop = Algo.Substring(readLine, 0, cdx);
            if (cop.StartsWith("E0"))
            {
                HandleE0Message(readLine);
                return false;
            }
            if (cop.StartsWith("F0"))
            {
                HandleF0Message(readLine);
                SingleThreadMessageStart();
                return true;
            }
            else if (cop.StartsWith("U"))
            {
                char rank = cop[1];
                string[] blocks = Algo.Splits(readLine.Substring("U1,".Length), ";;");
                switch (rank)
                {
                    case '1':
                        return HandleU1Message(blocks[0], blocks[1]);
                    case '3':
                        return HandleU3Message(blocks[0], blocks[1], blocks[2]);
                    case '5':
                        return HandleU5Message(blocks[0], blocks[1], blocks[2]);
                    case '7':
                        return HandleU7Message(blocks[0], blocks[1], blocks[2], blocks[3]);
                    case '9':
                        return HandleU9Message(blocks[0], blocks[1], blocks[2]);
                    case 'A':
                        return HandleUAMessage(blocks[0], blocks[1], blocks[2]);
                    case 'B':
                        VI.Cout(Uid, "您不可取消行动."); return false;
                    case 'C':
                        VI.Cout(Uid, "{0}放弃行动.", blocks[0]); return false;
                }
                return true;
            }
            else if (cop.StartsWith("V"))
            {
                char rank = cop[1];
                switch (rank)
                {
                    case '0': return HandleV0Message(readLine.Substring("V0,".Length));
                    case '2': return HandleV2Message(readLine.Substring("V2,".Length));
                    case '3': return HandleV3Message(readLine.Substring("V3,".Length));
                    case '5': return HandleV5Message(readLine.Substring("V5,".Length));
                    default: return true;
                }
            }
            else if (cop.StartsWith("R"))
                return HandleRMessage(readLine);
            else if (cop.StartsWith("H"))
                return HandleHMessage(cop, readLine.Substring(cdx + 1));
            return false;
        }

        #endregion Message Main

        #region Format Input

        private string FormattedInputWithCancelFlag(string line)
        {
            if (string.IsNullOrEmpty(line))
                return "";
            string output = "";
            string prevComment = "";
            foreach (string block in Algo.Splits(line, ","))
            {
            repaint:
                bool inputValid = true;
                string arg = block;
                string cancel = "";
                string roundInput = "";
                if (block.StartsWith("/"))
                {
                    if (block.Equals("//"))
                    {
                        VI.Cin(Uid, "请按任意键继续.");
                        roundInput = "0";
                    }
                    else if (block.Length > 1)
                    {
                        arg = block.Substring(1);
                        cancel = "(0为取消发动)";
                    }
                    else
                    {
                        VI.Cin(Uid, "不能指定合法目标.");
                        roundInput = "0";
                    }
                }
                if (arg.StartsWith("+")) // Keep, ignore in Console case
                    arg = arg.Substring(1);
                if (arg.StartsWith("#"))
                {
                    prevComment = arg.Substring(1); continue;
                }
                if (arg[0] == 'T')
                {
                    // format T1~2(p1p3p5),T1(p1p3),#Text
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    // TODO: handle with AND of multiple condition
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            // TODO: consider of empty bracket
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}名角色为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Player(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}名角色为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Player(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}名角色为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}名角色为{1}目标，可选{2}{3}.", r, prevComment, zd.Player(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}名角色为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'C')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Tux(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}张卡牌为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Tux(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}张卡牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标，可选{2}{3}.", r, prevComment, zd.Tux(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'Q')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Tux(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}张卡牌为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Tux(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}张卡牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标，可选{2}{3}.", r, prevComment, zd.Tux(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'Z')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}张公共卡牌为{1}目标，可选{2}{3}.",
                                    argv.Length, prevComment, zd.Tux(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}张公共卡牌为{2}目标，可选{3}{4}.",
                                    r1, r2, prevComment, zd.Tux(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}张卡牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}张公共卡牌为{1}目标，可选{2}{3}.",
                                r, prevComment, zd.Tux(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}张公共卡牌为{1}目标{2}.",
                                r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'M')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}张怪物牌为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Tux(uss), cancel);
                            } else
                            input = VI.Cin(Uid, "请选择{0}至{1}张怪物牌为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Monster(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}张怪物牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}张怪物牌为{1}目标，可选{2}{3}.", r, prevComment, zd.Monster(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}张怪物牌为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= (CountItemFromComma(input) == r);
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'I')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    int r1, r2;
                    if (idx >= 1)
                    {
                        r1 = int.Parse(Substring(arg, 1, idx));
                        r2 = int.Parse(Substring(arg, idx + 1, jdx));
                    }
                    else
                        r1 = r2 = int.Parse(Substring(arg, 1, jdx));

                    string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                    List<string> uss = argv.Select(p => p.Substring("I".Length)).ToList();
                    if (argv.Length < r1)
                        r1 = r2 = argv.Length;
                    if (r1 != r2)
                        input = VI.Cin(Uid, "请选择{0}至{1}张专属牌为{2}目标，可选{3}{4}.",
                            r1, r2, prevComment, zd.MixedCards(argv), cancel);
                    else
                        input = VI.Cin(Uid, "请选择{0}张专属牌为{1}目标，可选{2}{3}.",
                            r1, prevComment, zd.MixedCards(argv), cancel);
                    inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    inputValid &= input.Split(',').Intersect(uss).Any();
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'Y') // Yes or not selection
                {
                    int posCan = (int)(arg[1] - '0');
                    string[] coms;
                    if (prevComment == "")
                        coms = Enumerable.Repeat("", posCan).ToArray();
                    else
                    {
                        string[] prevs = Algo.Splits(prevComment, "##");
                        if (posCan + 1 > prevs.Length)
                        {
                            IEnumerable<string> v1 = prevs.Select(p => ":" + p);
                            IEnumerable<string> v2 = Enumerable.Repeat("", posCan + 1 - prevs.Length);
                            coms = v1.Concat(v2).ToArray();
                        }
                        else
                            coms = prevs.Take(posCan + 1).Select(p => ":" + p).ToArray();
                    }
                    string input = "{0}。请执行选项{1}—" + string.Join(",",
                        Enumerable.Range(1, posCan).Select(p => p + coms[p]));
                    roundInput = VI.Cin(Uid, input, coms[0].Substring(1), cancel); // Erase the colon.
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'X') // Arrangement
                {
                    // format X(p1p3p5)
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    int rest = int.Parse(Algo.Substring(arg, 1, jdx));
                    string[] argv = Algo.Substring(arg, jdx + "(p".Length, kdx).Split('p');
                    char cardType = argv[0][0];
                    List<ushort> uss = argv.Select(p => ushort.Parse(p.Substring(1))).ToList();
                    if (cardType == 'M')
                        roundInput = VI.Cin(Uid, "请重排以下{0}怪物{1}{2}.", prevComment, zd.Monster(uss), cancel);
                    else if (cardType == 'E')
                        roundInput = VI.Cin(Uid, "请重排以下{0}事件{1}{2}.", prevComment, zd.Eve(uss), cancel);
                    else if (cardType == 'C')
                        roundInput = VI.Cin(Uid, "请重排以下{0}手牌{1}{2}.", prevComment, zd.Tux(uss), cancel);
                    inputValid &= roundInput.Split(',').Intersect(uss.Select(p => p.ToString())).Any();
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'W') // Arrangement
                {
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string[] argv = Algo.Substring(arg, jdx + "(p".Length, kdx).Split('p');
                    var uss = argv.Select(p => ushort.Parse(p));
                    roundInput = VI.Cin(Uid, "请重排以下{0}卡牌{1}{2}.", prevComment, zd.Tux(uss), cancel);
                    inputValid &= roundInput.Split(',').Intersect(argv).Any();
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'S')
                {
                    roundInput = VI.Cin(Uid, "请选择{0}一方{1}.", prevComment, cancel);
                    inputValid &= (roundInput == "1" || roundInput == "2");
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'D')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        roundInput = VI.Cin(Uid, "请输入{0}数值({1}~{2}){3}.", prevComment, r1, r2, cancel);
                        int ipValue = int.Parse(roundInput);
                        inputValid &= (ipValue >= r1 && ipValue <= r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        roundInput = VI.Cin(Uid, "请输入{0}数值({1}){2}.", prevComment, r, cancel);
                        int ipValue = int.Parse(roundInput);
                        inputValid &= (ipValue == r);
                    }
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'G')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}种卡牌为{1}目标，可选{2}{3}.",
                                    argv.Length, prevComment, zd.TuxDbSerial(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}种卡牌为{2}目标，可选{3}{4}."
                                    , r1, r2, prevComment, zd.TuxDbSerial(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}种卡牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}种卡牌为{1}目标，可选{2}{3}.",
                                r, prevComment, zd.TuxDbSerial(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}种卡牌为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'J')
                {
                    // format T1~2(p1p3p5),T1(p1p3),#Text
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<string> judgeArgv = argv.Select(p => p.StartsWith("T") ? p.Substring("T".Length) : p).ToList();
                            List<string> uss = argv.Select(p => p.StartsWith("T") ? p.Substring("T".Length) : ("!" + p)).ToList();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请以{0}人{1}，可选{2}{3}.", argv.Length, prevComment,
                                    zd.PlayerWithMonster(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请以{0}至{1}人{2}，可选{3}{4}.", r1, r2,
                                    prevComment, zd.PlayerWithMonster(uss), cancel);
                            inputValid &= input.Split(',').Intersect(judgeArgv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请以{0}至{1}人{2}{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<string> judgeArgv = argv.Select(p => p.StartsWith("T") ? p.Substring("T".Length) : p).ToList();
                            List<string> uss = argv.Select(p => p.StartsWith("T") ? p.Substring("T".Length) : ("!" + p)).ToList();
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请以{0}人{1}，可选{2}{3}.", r, prevComment, zd.PlayerWithMonster(uss), cancel);
                            inputValid &= input.Split(',').Intersect(judgeArgv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请以{0}人{1}{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    // T-ed non 0 target
                    if (input.Length > 0 && input != "0" && Char.IsDigit(input[0]))
                        input = "T" + input;
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'F')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}枚标记为{1}目标，可选{2}{3}.",
                                    argv.Length, prevComment, zd.RuneWithCode(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}枚标记为{2}目标，可选{3}{4}."
                                    , r1, r2, prevComment, zd.RuneWithCode(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}枚标记为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}枚标记为{1}目标，可选{2}{3}.",
                                r, prevComment, zd.RuneWithCode(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}枚标记为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'E')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}张事件牌为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Eve(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}张事件牌为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Eve(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}张事件牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}张事件牌为{1}目标，可选{2}{3}.", r, prevComment, zd.Eve(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}张事件牌为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'V')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            int[] uss = argv.Select(p => int.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}种属性为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Prop(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}种属性为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Prop(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}种属性为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            int[] uss = argv.Select(p => int.Parse(p)).ToArray();
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}种属性为{1}目标，可选{2}{3}.", r, prevComment, zd.Prop(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}种属性为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'H')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            int[] uss = argv.Select(p => int.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                input = VI.Cin(Uid, "请选择{0}名角色为{1}目标，可选{2}{3}.",
                                    argv.Length, prevComment, zd.HeroWithCode(uss), cancel);
                            }
                            else
                                input = VI.Cin(Uid, "请选择{0}至{1}名角色为{2}目标，可选{3}{4}."
                                    , r1, r2, prevComment, zd.HeroWithCode(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}至{1}名角色为{2}目标{3}.", r1, r2, prevComment, cancel);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            int[] uss = argv.Select(p => int.Parse(p)).ToArray();
                            if (argv.Length < r)
                                r = argv.Length;
                            input = VI.Cin(Uid, "请选择{0}名角色为{1}目标，可选{2}{3}.",
                                r, prevComment, zd.HeroWithCode(uss), cancel);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}名角色为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == '!')
                {
                    roundInput = arg.Substring(1);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == '^')
                {

                }

                if (roundInput == "0" && block.StartsWith("/"))
                {
                    output += ",0"; return "/" + output.Substring(1);
                }
                else if (inputValid || roundInput == VI.CinSentinel || roundInput == "0")
                    output += "," + roundInput;
                else {
                    VI.Cout(Uid, "抱歉，您的输入有误。");
                    goto repaint;
                }
            }
            return output == "" ? "" : output.Substring(1);
        }
        private string FormattedInput(string line)
        {
            string result = FormattedInputWithCancelFlag(line);
            if (result.StartsWith("/"))
                result = result.Substring(1);
            return result;
        }
        #endregion Format Input

        #region E
        private void HandleE0Message(string readLine)
        {
            string[] args = readLine.Split(',');
            switch (args[0])
            {
                case "E0IT":
                    for (int idx = 1; idx < args.Length; )
                    {
                        ushort who = ushort.Parse(args[idx]);
                        int type = int.Parse(args[idx + 1]);
                        if (type == 0)
                        {
                            int n = int.Parse(args[idx + 2]);
                            List<ushort> cards = Algo.TakeRange(args, idx + 3, idx + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            Z0D[who].TuxCount += n;
                            if (who == Uid)
                                Z0M.Tux.AddRange(cards);
                            idx += (n + 3);
                        }
                        else if (type == 1)
                        {
                            int n = int.Parse(args[idx + 2]);
                            Z0D[who].TuxCount += n;
                            idx += 3;
                        }
                    }
                    break;
                case "E0OT":
                    for (int idx = 1; idx < args.Length; )
                    {
                        ushort who = ushort.Parse(args[idx]);
                        int type = int.Parse(args[idx + 1]);
                        if (type == 0)
                        {
                            int n = int.Parse(args[idx + 2]);
                            List<ushort> cards = Algo.TakeRange(args, idx + 3, idx + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            int tuxCount = 0;
                            foreach (ushort ut in cards)
                            {
                                Tux tux = Tuple.TL.DecodeTux(ut);
                                if (Z0D[who].Weapon == ut)
                                    Z0D[who].Weapon = 0;
                                else if (Z0D[who].Armor == ut)
                                    Z0D[who].Armor = 0;
                                else if (Z0D[who].Trove == ut)
                                    Z0D[who].Trove = 0;
                                else if (Z0D[who].ExCards.Contains(ut))
                                    Z0D[who].ExCards.Remove(ut);
                                else if (Z0D[who].Fakeq.ContainsKey(ut))
                                    Z0D[who].Fakeq.Remove(ut);
                                else
                                    ++tuxCount;
                            }
                            if (who == Uid)
                            {
                                Z0D[who].TuxCount -= tuxCount;
                                Z0M.Tux.RemoveAll(p => cards.Contains(p));
                            }
                            idx += (n + 3);
                        }
                        else if (type == 1)
                        {
                            int n = int.Parse(args[idx + 2]);
                            Z0D[who].TuxCount -= n;
                            idx += 3;
                        }
                    }
                    break;
                case "E0IN":
                    {
                        //string[] names = new string[] { "手牌", "怪物牌", "事件牌" };
                        //VI.Cout(uid, "{0}剩余牌数-{1}.", names[int.Parse(args[1])], args[2]);
                        ushort utype = ushort.Parse(args[1]);
                        int n = int.Parse(args[2]);
                        if (utype == 0)
                            Z0P.TuxCount -= n;
                        else if (utype == 1)
                            Z0P.MonCount -= n;
                        else if (utype == 2)
                            Z0P.EveCount -= n;
                    }
                    break;
                case "E0ON":
                    for (int idx = 1; idx < args.Length; )
                    {
                        ushort fromZone = ushort.Parse(args[idx]);
                        string cardType = args[idx + 1];
                        int n = int.Parse(args[idx + 2]);
                        if (n > 0)
                        {
                            List<ushort> cds = Algo.TakeRange(args, idx + 3, idx + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            string cdInfos = null;
                            if (cardType == "C")
                            {
                                Z0P.TuxDises += n;
                                if (fromZone == 0)
                                    cdInfos = zd.Tux(cds);
                            }
                            else if (cardType == "M")
                            {
                                Z0P.MonDises += n;
                                if (fromZone == 0)
                                    cdInfos = zd.Monster(cds);
                            }
                            else if (cardType == "E")
                            {
                                Z0P.EveDises += n;
                                if (fromZone == 0)
                                    cdInfos = zd.Eve(cds);
                            }
                            if (cdInfos != null)
                                VI.Cout(Uid, "{0}被弃置进入弃牌堆.", cdInfos);
                        }
                        idx += (3 + n);
                    }
                    break;
                case "E0CN":
                    {
                        ushort utype = ushort.Parse(args[1]);
                        int n = args.Length - 2;
                        if (utype == 0)
                            Z0P.TuxDises -= n;
                        else if (utype == 1)
                            Z0P.MonDises -= n;
                        else if (utype == 2)
                            Z0P.EveDises -= n;
                    }
                    break;
                case "E0RN":
                    for (int i = 1; i < args.Length; )
                    {
                        ushort from = ushort.Parse(args[i]);
                        ushort to = ushort.Parse(args[i + 1]);
                        int n = int.Parse(args[i + 2]);
                        List<ushort> tuxes = Algo.TakeRange(args, i + 3, i + 3 + n)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (to == 0)
                            VI.Cout(Uid, "{0}展示了卡牌{1}.", zd.Player(from), zd.Tux(tuxes));
                        else if (from == 0)
                            VI.Cout(Uid, "{0}收回了卡牌{1}.", zd.Player(to), zd.Tux(tuxes));
                        i += (n + 3);
                    }
                    break;
                case "E0HQ":
                    {
                        ushort type = ushort.Parse(args[1]);
                        ushort to = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            ushort from = ushort.Parse(args[3]);
                            int utype = int.Parse(args[4]);
                            if (utype == 0)
                            {
                                int n = int.Parse(args[5]);
                                List<ushort> cards = Algo.TakeRange(args, 6, args.Length)
                                    .Select(p => ushort.Parse(p)).ToList();
                                VI.Cout(Uid, "{0}从{1}获得了{2}.", zd.Player(to), zd.Player(from), zd.Tux(cards));
                                //foreach (ushort card in cards)
                                //{
                                //    if (Z0D[from].Weapon == card)
                                //        Z0D[from].Weapon = 0;
                                //    else if (Z0D[from].Armor == card)
                                //        Z0D[from].Armor = 0;
                                //    else if (Z0D[from].ExCards.Contains(card))
                                //        Z0D[from].ExCards.Remove(card);
                                //    else
                                //    {
                                //        --Z0D[from].TuxCount;
                                //        if (uid == from)
                                //            Z0M.Tux.Remove(card);
                                //    }
                                //}
                                //Z0D[to].TuxCount += (args.Length - 6);
                                //if (uid == to)
                                //    Z0M.Tux.AddRange(cards);
                            }
                            else if (utype == 1)
                            {
                                int n = int.Parse(args[5]);
                                VI.Cout(Uid, "{0}从{1}获得了{2}张牌.", zd.Player(to), zd.Player(from), n);
                                //Z0D[from].TuxCount -= n;
                                //Z0D[to].TuxCount += n;
                            }
                        }
                        else if (type == 2)
                        {
                            List<ushort> cards = Algo.TakeRange(args, 3, args.Length)
                                .Select(p => ushort.Parse(p)).ToList();
                            VI.Cout(Uid, "{0}摸取了{1}.", zd.Player(to), zd.Tux(cards));
                            //Z0P.TuxCount -= args.Length - 3;
                            //Z0D[to].TuxCount += cards.Count;
                            //if (uid == to)
                            //    Z0M.Tux.AddRange(cards);
                        }
                        else if (type == 3)
                        {
                            int n = int.Parse(args[3]);
                            VI.Cout(Uid, "{0}摸取了{1}张牌.", zd.Player(to), n);
                            //Z0D[to].TuxCount += n;
                        }
                        else if (type == 4)
                        {
                            for (int idx = 3; idx < args.Length; )
                            {
                                ushort fromZone = ushort.Parse(args[idx]);
                                int n = int.Parse(args[idx + 1]);
                                ushort[] tuxes = Algo.TakeRange(args, idx + 2, idx + 2 + n)
                                    .Select(p => ushort.Parse(p)).ToArray();
                                if (fromZone != 0)
                                    VI.Cout(Uid, "{0}从{1}的区域内获得了牌{2}.", zd.Player(to),
                                        zd.Player(fromZone), zd.Tux(tuxes));
                                else
                                    VI.Cout(Uid, "{0}取得了牌{1}.", zd.Player(to), zd.Tux(tuxes));
                                idx += (n + 2);
                            }
                        }
                        break;
                    }
                case "E0QZ":
                    VI.Cout(Uid, "{0}弃置卡牌{1}.", zd.Player(ushort.Parse(args[1])),
                        zd.Tux(Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p))));
                    break;
                case "E0IH":
                case "E0OH":
                    {
                        List<string> hpIssues = new List<string>();
                        List<ushort> revive = new List<ushort>();
                        for (int i = 1; i < args.Length; i += 5)
                        {
                            ushort from = ushort.Parse(args[i]);
                            ushort isLove = ushort.Parse(args[i + 1]);
                            int prop = ushort.Parse(args[i + 2]);
                            ushort n = ushort.Parse(args[i + 3]);
                            ushort now = ushort.Parse(args[i + 4]);
                            string msgBase = "{0}HP" + ("E0IH" == args[0] ? "+" : "-") + "{1}({2}),当前HP={3}.";
                            if (isLove == 1)
                                hpIssues.Add(string.Format(msgBase, zd.Player(from), n, "倾慕", now));
                            else
                                hpIssues.Add(string.Format(msgBase, zd.Player(from), n, zd.PropName(prop), now));
                            Z0D[from].HP = now;
                            if (now == n && "E0IH" == args[0])
                                revive.Add(from);
                        }
                        if (hpIssues.Count > 0)
                            VI.Cout(Uid, string.Join("\n", hpIssues));
                        if (revive.Count > 0)
                            VI.Cout(Uid, "{0}脱离濒死状态.", zd.Player(revive));
                    }
                    break;
                case "E0ZH":
                    VI.Cout(Uid, "{0}处于濒死状态.", zd.Player(
                        Algo.TakeRange(args, 1, args.Length).Select(p => ushort.Parse(p))));
                    break;
                case "E0LV":
                    for (int idx = 1; idx < args.Length;)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        int count = int.Parse(args[idx + 1]);
                        VI.Cout(Uid, "{0}对{1}发动了倾慕.", zd.PlayerWithMonster(
                            Algo.TakeRange(args, idx + 2, idx + 2 + count)), zd.Player(who));
                        idx += (2 + count);
                        Z0D[who].IsLoved = true;
                    }
                    break;
                case "E0ZW":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; ++i)
                        {
                            ushort py = ushort.Parse(args[i]);
                            result += "," + zd.Player(py);
                            Z0D[py].IsAlive = false;
                            Z0D[py].HP = 0;
                        }
                        if (result != "")
                            VI.Cout(Uid, "{0}因阵亡退场.", result.Substring(1));
                        break;
                    }
                case "E0IY":
                    {
                        bool changed = (args[1] == "0");
                        ushort who = ushort.Parse(args[2]);
                        int hero = int.Parse(args[3]);
                        if (Z0D.ContainsKey(who))
                        {
                            Z0D[who].SelectHero = hero;
                            Z0D[who].IsAlive = true;
                            if (args[1] == "0")
                            {
                                Z0D[who].ParseFromHeroLib();
                                VI.Cout(Uid, "{0}#玩家转化为{1}角色.", who, zd.Hero(hero));
                            }
                            else if (args[1] == "1")
                                VI.Cout(Uid, "{0}#玩家变身为{1}角色.", who, zd.Hero(hero));
                            else if (args[1] == "2")
                            {
                                Z0D[who].ClearStatus();
                                VI.Cout(Uid, "{1}加入到{0}#位置.", who, zd.Hero(hero));
                            }
                        }
                    }
                    break;
                case "E0OY":
                    if (args[1] == "0" || args[1] == "2")
                    {
                        ushort who = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}退场.", zd.Player(who));
                    }
                    break;
                case "E0WN":
                    VI.Cout(Uid, "{0}方获胜.", args[1]); break;
                case "E0DS":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            int n = int.Parse(args[3]);
                            VI.Cout(Uid, "{0}被横置.", zd.Player(who), n);
                            Z0D[who].Immobilized = true;
                        }
                        else if (type == 1)
                        {
                            VI.Cout(Uid, "{0}解除横置.", zd.Player(who));
                            Z0D[who].Immobilized = false;
                        }
                        break;
                    }
                case "E0FU":
                    if (args[1].Equals("0"))
                    {
                        string cardType = args[2];
                        ushort[] ravs = Algo.TakeRange(args, 3, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        if (cardType == "C")
                            VI.Cout(Uid, "你观看了{0}.", zd.Tux(ravs));
                        else if (cardType == "M")
                            VI.Cout(Uid, "你观看了{0}.", zd.Monster(ravs));
                        else if (cardType == "E")
                            VI.Cout(Uid, "你观看了{0}.", zd.Eve(ravs));
                        else if (cardType == "G")
                            VI.Cout(Uid, "你观看了{0}.", zd.TuxDbSerial(ravs));
                    }
                    else if (args[1].Equals("1"))
                    {
                        ushort n = ushort.Parse(args[3]);
                        VI.Cout(Uid, "{0}张卡牌正被观看.", n);
                    }
                    else if (args[1].Equals("2"))
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort[] invs = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        string cardType = args[3];
                        if (cardType == "C")
                            VI.Cout(Uid, "{0}展示了卡牌{1}.", who, zd.Tux(invs));
                        if (cardType == "M")
                            VI.Cout(Uid, "{0}展示了卡牌{1}.", who, zd.Monster(invs));
                        if (cardType == "E")
                            VI.Cout(Uid, "{0}展示了卡牌{1}.", who, zd.Eve(invs));
                        if (cardType == "G")
                            VI.Cout(Uid, "{0}展示了卡牌{1}.", who, zd.TuxDbSerial(invs));
                        if (cardType == "F")
                            VI.Cout(Uid, "{0}声明了标记{1}.", who, zd.Rune(invs));
                        if (cardType == "V")
                            VI.Cout(Uid, "{0}声明了属性{1}.", who, zd.Prop(invs.Select(p => (int)p)));
                    }
                    else if (args[1].Equals("3"))
                        VI.Cout(Uid, "观看完毕.");
                    else if (args[1].Equals("5"))
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort[] invs = Algo.TakeRange(args, 3, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        VI.Cout(Uid, "{0}声明了{1}.", who, zd.TuxDbSerial(invs));
                    }
                    break;
                case "E0QU":
                    if (args[1].Equals("0"))
                    {
                        var ravs = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p));
                        VI.Cout(Uid, "{0}被移离观看区.", zd.Tux(ravs));
                    }
                    else if (args[1].Equals("1"))
                        VI.Cout(Uid, "{0}张牌被移离观看区.", args[2]);
                    else if (args[1].Equals("2"))
                        VI.Cout(Uid, "观看区被清空.");
                    break;
                case "E0CC": // prepare to use card
                    {
                        // E0CC,A,0,TP02,17,36
                        ushort ust = ushort.Parse(args[1]);
                        ushort adapter = ushort.Parse(args[2]);
                        ushort pst = ushort.Parse(args[3]);
                        string txkn = args[4];
                        List<ushort> ravs = Algo.TakeRange(args, 5, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (pst == ust)
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(txkn));
                        else
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}，为{3}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(txkn), zd.Player(pst));
                        //Z0D[ust].TuxCount -= ravs.Count;
                        //if (ust == uid)
                        //    Z0M.Tux.RemoveAll(p => ravs.Contains(p));
                        break;
                    }
                case "E0CD": // use card and want a target
                    {
                        // E0CD,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        List<ushort> argst = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        VI.Cout(Uid, "{0}{1}预定作用于{2}.", zd.Tux(args[3]),
                            (argst.Count > 0 ? ("(" + string.Join(",", argst) + ")") : ""), zd.Player(ust));
                        break;
                    }
                case "E0CE": // use card and take action
                    {
                        // E0CE,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        List<ushort> argst = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        VI.Cout(Uid, "{0}{1}对{2}生效.", zd.Tux(args[3]),
                            (argst.Count > 0 ? ("(" + string.Join(",", argst) + ")") : ""), zd.Player(ust));
                        break;
                    }
                 case "E0CL": // cancel card
                    {
                        // E0CL,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        string cardName = args[2];
                        if (args.Length > 3)
                        {
                            ushort[] ravs = Algo.TakeRange(args, 3, args.Length).Select(p => ushort.Parse(p)).ToArray();
                            VI.Cout(Uid, "{0}的{1}({2})被抵消.", zd.Player(ust), zd.Tux(cardName), string.Join("p", ravs));
                        }
                        else
                            VI.Cout(Uid, "{0}的{1}被抵消.", zd.Player(ust), zd.Tux(cardName));
                        break;
                    }
                case "E0XZ":
                    {
                        ushort py = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        IDictionary<string, string> dd = new Dictionary<string, string>();
                        IDictionary<string, Func<IEnumerable<ushort>, string>> df =
                            new Dictionary<string, Func<IEnumerable<ushort>, string>>();
                        dd.Add("1", "手牌堆"); dd.Add("2", "怪物牌堆"); dd.Add("3", "事件牌堆");
                        df.Add("1", zd.Tux); df.Add("2", zd.Monster); df.Add("3", zd.Eve);
                        if (type == 0)
                        {
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(Uid, "您观看{0}结果为{1}.", dd[args[3]], df[args[3]](ravs));
                        }
                        else if (type == 1)
                            VI.Cout(Uid, "{0}观看{1}上方{2}张牌.", zd.Player(py), dd[args[3]], args[4]);
                        else if (type == 2)
                        {
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(Uid, "您调整{0}结果为{1}.", dd[args[3]], df[args[3]](ravs));
                        }
                        else if (type == 3)
                        {
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(Uid, "{0}调整{1}的新顺序为{2}.", zd.Player(py), dd[args[3]], string.Join(",", ravs));
                        }
                        else if (type == 4)
                            VI.Cout(Uid, "{0}不调整牌堆顺序.", zd.Player(py));
                        else if (type == 5)
                        {
                            ushort who = ushort.Parse(args[3]);
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(Uid, "您观看{0}手牌结果为{1}.", zd.Player(who), zd.Tux(ravs));
                        }
                        else if (type == 6)
                            VI.Cout(Uid, "{0}观看了{1}的手牌.", zd.Player(py), zd.Player(ushort.Parse(args[3])));
                        break;
                    }
                case "E0ZB":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        string[] words = { "武器区", "防具区", "特殊区", "佩戴区", "额外装备区", "秘宝区" };
                        if (type == 0)
                        {
                            ushort where = ushort.Parse(args[3]);
                            ushort card = ushort.Parse(args[4]);
                            if (where == 1)
                                Z0D[me].Weapon = card;
                            else if (where == 2)
                                Z0D[me].Armor = card;
                            else if (where == 3)
                                Z0D[me].ExCards.Add(card);
                            else if (where == 4)
                            {
                                string asCode = args[5] == "0" ? Tuple.TL.DecodeTux(card).Code : args[5];
                                Z0D[me].Fakeq[card] = asCode;
                            }
                            else if (where == 5)
                                Z0D[me].ExEquip = card;
                            else if (where == 6)
                                Z0D[me].Trove = card;
                            VI.Cout(Uid, "{0}装备了{1}到{2}.", zd.Player(me), zd.Tux(card), words[where - 1]);
                            //--Z0D[me].TuxCount;
                            //if (me == uid)
                            //    Z0M.Tux.Remove(card);
                        }
                        else if (type == 1)
                        {
                            ushort where = ushort.Parse(args[4]);
                            ushort from = ushort.Parse(args[3]);
                            ushort card = ushort.Parse(args[5]);
                            if (where == 1)
                                Z0D[me].Weapon = card;
                            else if (where == 2)
                                Z0D[me].Armor = card;
                            else if (where == 3)
                                Z0D[me].ExCards.Add(card);
                            else if (where == 4)
                            {
                                string asCode = args[6] == "0" ? Tuple.TL.DecodeTux(card).Code : args[6];
                                Z0D[me].Fakeq[card] = asCode;
                            }
                            else if (where == 5)
                                Z0D[me].ExEquip = card;
                            else if (where == 6)
                                Z0D[me].Trove = card;
                            if (from != 0)
                                VI.Cout(Uid, "{0}的装备{2}进入{1}的{3}.",
                                    zd.Player(from), zd.Player(me), zd.Tux(card), words[where - 1]);
                            else
                                VI.Cout(Uid, "{0}装备了卡牌{1}到{2}.",
                                    zd.Player(me), zd.Tux(card), words[where - 1]);
                        }
                        break;
                    }
                case "E0ZC":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort consumeType = ushort.Parse(args[2]);
                        ushort where = ushort.Parse(args[3]);
                        ushort card = ushort.Parse(args[4]);
                        ushort target; int type; int argvIdx;
                        if (consumeType == 0 || consumeType == 1)
                        {
                            target = 0;
                            type = int.Parse(args[5]);
                            argvIdx = 6;
                        }
                        else
                        {
                            target = ushort.Parse(args[5]);
                            type = int.Parse(args[6]);
                            argvIdx = 7;
                        }
                        string argvs = "";
                        for (int i = argvIdx; i < args.Length; ++i)
                            argvs += "," + args[i];
                        if (argvs != "")
                            argvs = "(" + argvs.Substring(1) + ")";

                        bool hind = false; int acType = consumeType % 2;
                        TuxEqiup tuxeq = Tuple.TL.DecodeTux(card) as TuxEqiup;
                        if (tuxeq != null && tuxeq.CsHind[acType][type])
                            hind = true;
                        if (!hind)
                        {
                            string[] wherestr = new string[] { "", "武器区", "防具区",
                                "特殊区", "佩戴区", "额外装备区", "秘宝区" };
                            string[] patstr = new string[] {
                                "{0}发动了{1}卡牌{2}[{3}]特效{4}.",
                                "{0}爆发了{1}卡牌{2}[{3}]{4}.", "",
                                "{0}发动了{1}卡牌{2}[{3}]特效{4},作用于{5}.",
                                "{0}爆发了{1}卡牌{2}[{3}]{4},作用于{5}."
                            };
                            VI.Cout(Uid, patstr[consumeType], zd.Player(me), wherestr[where],
                                zd.Tux(card), type, argvs, zd.Player(target));
                        }
                        break;
                    }
                //case "E0ZU":
                //    {
                //        ushort type = ushort.Parse(args[1]);
                //        ushort who = ushort.Parse(args[2]);
                //        ushort card = ushort.Parse(args[3]);
                //    }
                //    break;
                case "E0ZL":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort card = ushort.Parse(args[i + 1]);
                            result += string.Format(",{0}的{1}", zd.Player(who), zd.Tux(card));
                        }
                        if (result != "")
                            VI.Cout(Uid, "{0}装备特效无效化.", result.Substring(1));
                    }
                    break;
                case "E0ZS":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort card = ushort.Parse(args[i + 1]);
                            result += string.Format(",{0}的{1}", zd.Player(who), zd.Tux(card));
                        }
                        if (result != "")
                            VI.Cout(Uid, "{0}装备特效开始生效.", result.Substring(1));
                    }
                    break;
                case "E0IA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        if (type == 0 || type == 1)
                        {
                            int n = int.Parse(args[3]);
                            int bs = int.Parse(args[4]);
                            int tp = args.Length > 5 ? int.Parse(args[5]) : bs;
                            if (who < 1000)
                            {
                                VI.Cout(Uid, "{0}战力+{1},当前为{2}/{3}.", zd.Player(who), n, tp, bs);
                                Z0D[who].STR = tp;
                                Z0D[who].STRa = bs;
                            }
                            else
                                VI.Cout(Uid, "{0}战力+{1},当前战力为{2}.", zd.Monster((ushort)(who - 1000)), n, tp);
                        }
                        else if (type == 2)
                            VI.Cout(Uid, "{0}强制战斗胜利.", who == 1 ? "红方" : "蓝方");
                        break;
                    }
                case "E0OA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        if (type == 0 || type == 1)
                        {
                            int n = int.Parse(args[3]);
                            int bs = int.Parse(args[4]);
                            int tp = args.Length > 5 ? int.Parse(args[5]) : bs;
                            if (who < 1000)
                            {
                                VI.Cout(Uid, "{0}战力-{1},当前为{2}/{3}.", zd.Player(who), n, tp, bs);
                                Z0D[who].STR = tp;
                                Z0D[who].STRa = bs;
                            }
                            else
                                VI.Cout(Uid, "{0}战力-{1},当前战力为{2}.", zd.Monster((ushort)(who - 1000)), n, tp);
                        }
                        else if (type == 2)
                            VI.Cout(Uid, "{0}强制战斗失败.", who == 1 ? "红方" : "蓝方");
                        break;
                    }
                case "E0IX":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        if (type == 0 || type == 1)
                        {
                            int n = int.Parse(args[3]);
                            int bs = int.Parse(args[4]);
                            int tp = args.Length > 5 ? int.Parse(args[5]) : bs;
                            if (who < 1000)
                            {
                                VI.Cout(Uid, "{0}命中+{1},当前命中为{2}/{3}.", zd.Player(who), n, tp, bs);
                                Z0D[who].DEX = tp;
                                Z0D[who].DEXa = bs;
                            }
                            else
                                VI.Cout(Uid, "{0}命中+{1},当前命中为{2}.", zd.Monster((ushort)(who - 1000)), n, tp);
                        }
                        else if (type == 2)
                            VI.Cout(Uid, "{0}强制命中.", zd.Player(who));
                        break;
                    }
                case "E0OX":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        if (type == 0 || type == 1)
                        {
                            int n = int.Parse(args[3]);
                            int bs = int.Parse(args[4]);
                            int tp = args.Length > 5 ? int.Parse(args[5]) : bs;
                            if (who < 1000)
                            {
                                VI.Cout(Uid, "{0}命中-{1},当前命中为{2}/{3}.", zd.Player(who), n, tp, bs);
                                Z0D[who].DEX = tp;
                                Z0D[who].DEXa = bs;
                            }
                            else
                                VI.Cout(Uid, "{0}命中-{1},当前命中为{2}.", zd.Monster((ushort)(who - 1000)), n, tp);
                        }
                        else if (type == 2)
                            VI.Cout(Uid, "{0}强制被闪避.", zd.Player(who));
                        break;
                    }
                case "E0AX":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int str = int.Parse(args[2]);
                        int dex = int.Parse(args[3]);
                        VI.Cout(Uid, "{0}战力恢复为{1},命中恢复为{2}.", zd.Player(who), str, dex);
                        Z0D[who].STR = Z0D[who].STRa = str;
                        Z0D[who].DEX = Z0D[who].DEXa = dex;
                        break;
                    }
                case "E0IB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int delta = ushort.Parse(args[2]);
                        int cur = int.Parse(args[3]);
                        VI.Cout(Uid, "{0}战力+{1},现在为{2}.", zd.Monster(x), delta, cur);
                        break;
                    }
                case "E0OB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int delta = int.Parse(args[2]);
                        int cur = int.Parse(args[3]);
                        VI.Cout(Uid, "{0}战力-{1},现在为{2}.", zd.Monster(x), delta, cur);
                        break;
                    }
                case "E0IW":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int delta = ushort.Parse(args[2]);
                        int cur = int.Parse(args[3]);
                        VI.Cout(Uid, "{0}闪避+{1},现在为{2}.", zd.Monster(x), delta, cur);
                        break;
                    }
                case "E0OW":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int delta = int.Parse(args[2]);
                        int cur = int.Parse(args[3]);
                        VI.Cout(Uid, "{0}闪避-{1},现在为{2}.", zd.Monster(x), delta, cur);
                        break;
                    }
                case "E0WB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int cur = int.Parse(args[2]);
                        if (args.Length >= 4)
                        {
                            int agl = int.Parse(args[3]);
                            VI.Cout(Uid, "{0}战力恢复为{1}，闪避恢复为{2}.", zd.Monster(x), cur, agl);
                        }
                        else
                            VI.Cout(Uid, "{0}战力恢复为{1}.", zd.Monster(x), cur);
                    }
                    break;
                case "E09P":
                    if (args[1] == "0")
                    {
                        ushort s = ushort.Parse(args[2]);
                        bool sy = args[3].Equals("1");
                        ushort h = ushort.Parse(args[4]);
                        bool hy = args[5].Equals("1");
                        string comp1 = (s != 0) ? "{0}支援" + (sy ? "成功" : "失败") : "无支援";
                        string comp2 = (h != 0) ? "{1}妨碍" + (hy ? "成功" : "失败") : "无妨碍";
                        VI.Cout(Uid, comp1 + "，" + comp2 + "。", zd.Player(s), zd.Player(h));
                    }
                    else if (args[1] == "1")
                    {
                        ushort rside = ushort.Parse(args[2]);
                        int rpool = ushort.Parse(args[3]);
                        ushort oside = ushort.Parse(args[4]);
                        int opool = ushort.Parse(args[5]);
                        VI.Cout(Uid, "{0}方战力={1}，{2}方战力={3}.", rside, rpool, oside, opool);
                        Z0F.RPool = rpool; Z0F.OPool = opool;
                    }
                    break;
                case "E0IP":
                    {
                        ushort side = ushort.Parse(args[1]);
                        ushort delta = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}方战力+{1}.", side, delta);
                        break;
                    }
                case "E0OP":
                    {
                        ushort side = ushort.Parse(args[1]);
                        ushort delta = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}方战力-{1}.", side, delta);
                        break;
                    }
                case "E0CZ":
                    if (args[1] == "0")
                        VI.Cout(Uid, "{0}禁止使用战牌.", zd.Player(ushort.Parse(args[2])));
                    else if (args[1] == "1")
                        VI.Cout(Uid, "{0}恢复使用战牌权.", zd.Player(ushort.Parse(args[2])));
                    else if (args[1] == "2")
                        VI.Cout(Uid, "全体恢复使用战牌权.");
                    break;
                case "E0HC":
                    {
                        int type = int.Parse(args[1]);
                        ushort who = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            ushort from = ushort.Parse(args[3]);
                            var cards = Algo.TakeRange(args, 4, args.Length).Select(p => ushort.Parse(p));
                            VI.Cout(Uid, "{0}可以获得宠物{1}.", zd.Player(who), zd.Monster(cards));
                        }
                        else if (type == 1)
                        {
                            ushort from = ushort.Parse(args[3]);
                            ushort kokan = ushort.Parse(args[4]);
                            var cards = Algo.TakeRange(args, 5, args.Length).Select(p => ushort.Parse(p));
                            VI.Cout(Uid, "{0}可获得宠物{1}.", zd.Player(who), zd.Monster(cards));
                        }
                    }
                    break;
                case "E0HH":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort consumeType = ushort.Parse(args[2]);
                        ushort mons = ushort.Parse(args[3]);
                        int innerType = int.Parse(args[4]);
                        Monster monster = Tuple.ML.Decode(mons);
                        if (monster != null && Algo.Equals(monster.EAHinds, consumeType, innerType, false))
                        {
                            string argvs = "";
                            for (int i = 5; i < args.Length; ++i)
                                argvs += "," + args[i];
                            if (argvs != "")
                                argvs = "(" + argvs.Substring(1) + ")";

                            if (consumeType == 0)
                            {
                                VI.Cout(Uid, "{0}发动了宠物{1}[{2}]特效{3}.",
                                    zd.Player(me), zd.Monster(mons), innerType, argvs);
                            }
                            else if (consumeType == 1)
                            {
                                VI.Cout(Uid, "{0}爆发了宠物{1}[{2}]{3}.", zd.Player(me),
                                    zd.Monster(mons), innerType, argvs);
                            }
                            else if (consumeType == 2)
                                VI.Cout(Uid, "宠物{0}效果{2}被触发.", zd.Monster(mons), innerType, argvs);
                        }
                        break;
                    }
                case "E0HI":
                    {
                        IDictionary<ushort, List<ushort>> imc = new Dictionary<ushort, List<ushort>>();
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort pet = ushort.Parse(args[i + 1]);
                            if (!imc.ContainsKey(who))
                                imc[who] = new List<ushort>();
                            imc[who].Add(pet);
                        }
                        foreach (var pair in imc)
                            VI.Cout(Uid, "{0}的宠物{1}被爆发.", zd.Player(pair.Key), zd.Monster(pair.Value));
                    }
                    break;
                case "E0HD":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort from = ushort.Parse(args[2]);
                        ushort pet = ushort.Parse(args[3]);
                        if (from == 0)
                            VI.Cout(Uid, "{0}获得了宠物{1}.", zd.Player(who), zd.Monster(pet));
                        else
                            VI.Cout(Uid, "{0}从{1}获得了宠物{2}.", zd.Player(who),
                                zd.Player(from), zd.Monster(pet));
                        if (!Z0D[who].Pets.Contains(pet))
                            Z0D[who].Pets.Add(pet);
                    }
                    break;
                case "E0HL":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort pet = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}失去了宠物{1}.", zd.Player(who), zd.Monster(pet));
                        Z0D[who].Pets.Remove(pet);
                    }
                    break;
                case "E0HU":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> cedcards = new List<ushort>();
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort mon = ushort.Parse(args[i]);
                            cedcards.Add(mon);
                        }
                    }
                    break;
                case "E0HZ":
                    if (args[1] == "0")
                    {
                        ushort who = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}放弃触发混战.", zd.Player(who));
                    }
                    else if (args[1] == "1" || args[1] == "2")
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        Z0F.Monster2 = mon;
                        VI.Cout(Uid, "{0}触发混战的结果为【{1}】.", zd.Player(who), zd.Monster(mon));
                    }
                    else if (args[1] == "3")
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        Z0F.Monster2 = mon;
                        VI.Cout(Uid, "{0}触发混战的结果为【{1}】，钦慕效果生效.", zd.Player(who), zd.Monster(mon));
                    }
                    break;
                case "E0TT":
                    VI.Cout(Uid, "{0}掷骰的结果为{1}.", zd.Player(ushort.Parse(args[1])), args[2]);
                    break;
                case "E0T7":
                    VI.Cout(Uid, "{0}更改掷骰的结果为{1}.", zd.Player(ushort.Parse(args[1])), args[3]);
                    break;
                case "E0IJ":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            ushort delta = ushort.Parse(args[3]);
                            ushort cur = ushort.Parse(args[4]);
                            VI.Cout(Uid, "{0}的{1}+{2}，现在为{3}.", zd.Player(who),
                                zd.HeroTokenAlias(Z0D[who].SelectHero), delta, cur);
                            Z0D[who].Token = cur;
                        }
                        else if (type == 1)
                        {
                            int count1 = int.Parse(args[3]);
                            List<string> heros1 = Algo.TakeRange(args, 4, 4 + count1).ToList();
                            int count2 = int.Parse(args[4 + count1]);
                            List<string> heros2 = Algo.TakeRange(args, 5 + count1,
                                5 + count1 + count2).ToList();
                            VI.Cout(Uid, "{0}的{1}增加{2}，现在为{3}.", zd.Player(who),
                                zd.HeroPeopleAlias(Z0D[who].SelectHero), zd.MixedCards(heros1), zd.MixedCards(heros2));
                            Z0D[who].SpecialCards.AddRange(heros1);
                        }
                        else if (type == 2)
                        {
                            int count1 = int.Parse(args[3]);
                            List<ushort> tars1 = Algo.TakeRange(args, 4, 4 + count1)
                                .Select(p => ushort.Parse(p)).ToList();
                            int count2 = int.Parse(args[4 + count1]);
                            List<ushort> tars2 = Algo.TakeRange(args, 5 + count1,
                                5 + count1 + count2).Select(p => ushort.Parse(p)).ToList();
                            if (count1 == count2)
                                VI.Cout(Uid, "{0}的{1}目标指定为{2}.", zd.Player(who),
                                    zd.HeroPlayerTarAlias(Z0D[who].SelectHero), zd.Player(tars1));
                            else
                                VI.Cout(Uid, "{0}的{1}目标增加{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroPlayerTarAlias(Z0D[who].SelectHero), zd.Player(tars1), zd.Player(tars2));
                            Z0D[who].PlayerTars.AddRange(tars1);
                        }
                        else if (type == 3)
                        {
                            VI.Cout(Uid, "{0}已发动{1}.", zd.Player(who), zd.HeroAwakeAlias(Z0D[who].SelectHero));
                            Z0D[who].AwakeSignal = true;
                        }
                        else if (type == 4)
                        {
                            bool hind = args[3] != "0";
                            if (!hind)
                            {
                                int count1 = int.Parse(args[4]);
                                List<ushort> folder1 = Algo.TakeRange(args, 5, 5 + count1)
                                    .Select(p => ushort.Parse(p)).ToList();
                                int count2 = int.Parse(args[5 + count1]);
                                List<ushort> folder2 = Algo.TakeRange(args, 6 + count1,
                                    6 + count1 + count2).Select(p => ushort.Parse(p)).ToList();
                                VI.Cout(Uid, "{0}的{1}增加{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroFolderAlias(Z0D[who].SelectHero), zd.Tux(folder1), zd.Tux(folder2));
                                Z0D[who].FolderCount += count1;
                                Z0M.Folder.AddRange(folder1);
                            }
                            else
                            {
                                int count1 = int.Parse(args[4]);
                                int count2 = int.Parse(args[5]);
                                VI.Cout(Uid, "{0}的{1}数增加{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroFolderAlias(Z0D[who].SelectHero), count1, count2);
                                Z0D[who].FolderCount += count1;
                            }
                        }
                    }
                    break;
                case "E0OJ":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            ushort delta = ushort.Parse(args[3]);
                            ushort cur = ushort.Parse(args[4]);
                            VI.Cout(Uid, "{0}的{1}-{2}，现在为{3}.", zd.Player(who),
                                zd.HeroTokenAlias(Z0D[who].SelectHero), delta, cur);
                            Z0D[who].Token = cur;
                        }
                        else if (type == 1)
                        {
                            int count1 = int.Parse(args[3]);
                            List<string> heros1 = Algo.TakeRange(args, 4, 4 + count1).ToList();
                            int count2 = int.Parse(args[4 + count1]);
                            List<string> heros2 = Algo.TakeRange(args, 5 + count1,
                                5 + count1 + count2).ToList();
                            VI.Cout(Uid, "{0}的{1}减少{2}，现在为{3}.", zd.Player(who),
                                zd.HeroPeopleAlias(Z0D[who].SelectHero), zd.MixedCards(heros1), zd.MixedCards(heros2));
                            Z0D[who].SpecialCards.RemoveAll(p => heros1.Contains(p));
                        }
                        else if (type == 2)
                        {
                            int count1 = int.Parse(args[3]);
                            List<ushort> tars1 = Algo.TakeRange(args, 4, 4 + count1)
                                .Select(p => ushort.Parse(p)).ToList();
                            int count2 = int.Parse(args[4 + count1]);
                            List<ushort> tars2 = Algo.TakeRange(args, 5 + count1,
                                5 + count1 + count2).Select(p => ushort.Parse(p)).ToList();
                            if (count2 == 0)
                                VI.Cout(Uid, "{0}失去{1}目标.", zd.Player(who),
                                    zd.HeroPlayerTarAlias(Z0D[who].SelectHero));
                            else
                                VI.Cout(Uid, "{0}的{1}目标减少{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroPlayerTarAlias(Z0D[who].SelectHero), zd.Player(tars1), zd.Player(tars2));
                            Z0D[who].PlayerTars.RemoveAll(p => tars1.Contains(p));
                        }
                        else if (type == 3)
                        {
                            VI.Cout(Uid, "{0}已取消{1}.", zd.Player(who), zd.HeroAwakeAlias(Z0D[who].SelectHero));
                            Z0D[who].AwakeSignal = false;
                        }
                        else if (type == 4)
                        {
                            bool hind = args[3] != "0";
                            if (!hind)
                            {
                                int count1 = int.Parse(args[4]);
                                List<ushort> folder1 = Algo.TakeRange(args, 5, 5 + count1)
                                    .Select(p => ushort.Parse(p)).ToList();
                                int count2 = int.Parse(args[5 + count1]);
                                List<ushort> folder2 = Algo.TakeRange(args, 6 + count1,
                                    6 + count1 + count2).Select(p => ushort.Parse(p)).ToList();
                                if (count2 == 0)
                                    VI.Cout(Uid, "{0}的{1}减少{2}.", zd.Player(who),
                                        zd.HeroFolderAlias(Z0D[who].SelectHero), zd.Tux(folder1));
                                else
                                    VI.Cout(Uid, "{0}的{1}减少{2}，现在为{3}.", zd.Player(who),
                                        zd.HeroFolderAlias(Z0D[who].SelectHero), zd.Tux(folder1), zd.Tux(folder2));
                                Z0D[who].FolderCount -= count1;
                                Z0M.Folder.RemoveAll(p => folder1.Contains(p));
                            }
                            else
                            {
                                int count1 = int.Parse(args[4]);
                                int count2 = int.Parse(args[5]);
                                VI.Cout(Uid, "{0}的{1}数减少{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroFolderAlias(Z0D[who].SelectHero), count1, count2);
                                Z0D[who].FolderCount -= count1;
                            }
                        }
                    }
                    break;
                case "E0WK":
                    for (int idx = 1; idx < args.Length; idx += 2)
                    {
                        int team = int.Parse(args[idx]);
                        int value = int.Parse(args[idx + 1]);
                        Z0P.Score[team] = value;
                    }
                    break;
                case "E0AK":
                    for (int idx = 1; idx < args.Length; idx += 5)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        int hp = int.Parse(args[idx + 1]);
                        int hpb = int.Parse(args[idx + 2]);
                        int str = int.Parse(args[idx + 3]);
                        int dex = int.Parse(args[idx + 4]);
                        Z0D[who].HP = hp;
                        Z0D[who].HPa = hpb;
                        Z0D[who].STR = Z0D[who].STRa = str;
                        Z0D[who].DEX = Z0D[who].DEXa = dex;
                    }
                    break;
                case "E0IL":
                    for (int idx = 1; idx < args.Length; idx += 2)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        ushort npc = ushort.Parse(args[idx + 1]);
                        Z0D[who].Escue.Add(npc);
                        VI.Cout(Uid, "{0}获得助战NPC{1}.", zd.Player(who), zd.Monster(npc));
                    }
                    break;
                case "E0OL":
                    for (int idx = 1; idx < args.Length; idx += 2)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        ushort npc = ushort.Parse(args[idx + 1]);
                        Z0D[who].Escue.Remove(npc);
                        VI.Cout(Uid, "{0}失去助战NPC{1}.", zd.Player(who), zd.Monster(npc));
                    }
                    break;
                case "E0SW":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort piles = ushort.Parse(args[2]);
                        List<ushort> cards = Algo.TakeRange(args, 3,
                            args.Length).Select(p => ushort.Parse(p)).ToList();
                        if (cards.Count > 0)
                        {
                            if (piles == 0)
                                VI.Cout(Uid, "{0}展示了{1}.", zd.Player(who), zd.Tux(cards));
                            else if (piles == 1)
                                VI.Cout(Uid, "{0}展示了{1}.", zd.Player(who), zd.Monster(cards));
                            else if (piles == 2)
                                VI.Cout(Uid, "{0}展示了{1}.", zd.Player(who), zd.Eve(cards));
                        }
                    }
                    break;
                case "E0AS":
                    break;
                case "E0IE":
                case "E0OE":
                    {
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort who = ushort.Parse(args[i]);
                            if (args[0] == "E0IE")
                                Z0D[who].PetDisabled = false;
                            else
                                Z0D[who].PetDisabled = true;
                        }
                    }
                    break;
                case "E0YM":
                    if (args[1] == "0")
                    {
                        ushort mons = ushort.Parse(args[2]);
                        if (mons != 0)
                            VI.Cout(Uid, "本场战斗怪物为【{0}】.", zd.Monster(mons));
                        Z0F.Monster1 = mons;
                    }
                    else if (args[1] == "1")
                    {
                        ushort mons = ushort.Parse(args[2]);
                        if (mons != 0)
                            VI.Cout(Uid, "混战结果为【{0}】.", zd.Monster(mons));
                        Z0F.Monster2 = mons;
                    }
                    else if (args[1] == "2")
                    {
                        ushort eve = ushort.Parse(args[2]);
                        if (eve != 0)
                            VI.Cout(Uid, "执行事件牌为【{0}】.", zd.Eve(eve));
                        Z0F.Eve1 = eve;
                    }
                    else if (args[1] == "3")
                    {
                        ushort npc = ushort.Parse(args[2]);
                        if (npc != 0)
                            VI.Cout(Uid, "翻出的NPC牌为【{0}】.", zd.Monster(npc));
                        // Z0F.Wang = npc;
                    }
                    else if (args[1] == "4")
                    {
                        int hro = int.Parse(args[2]);
                        if (hro != 0)
                            VI.Cout(Uid, "翻出的角色牌为【{0}】.", zd.Hero(hro));
                    }
                    else if (args[1] == "5")
                    {
                        ushort[] mons = Algo.TakeRange(args, 2, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        VI.Cout(Uid, "翻出怪物牌为【{0}】.", zd.Monster(mons));
                    }
                    else if (args[1] == "6")
                    {
                        int position = int.Parse(args[2]);
                        if (args[3] == "0")
                        {
                            VI.Cout(Uid, "一张NPC牌被插入放置于牌堆顶第{0}张.", (position + 1));
                            ++Z0P.MonCount;
                        }
                        else {
                            List<ushort> mons = Algo.TakeRange(args, 3, args.Length)
                                .Select(p => ushort.Parse(p)).ToList();
                            VI.Cout(Uid, "NPC牌【{0}】被插入放置于牌堆顶第{1}张.", mons, (position + 1));
                            Z0P.MonCount += mons.Count;
                        }
                    }
                    else if (args[1] == "7")
                    {
                        int count = int.Parse(args[2]);
                        VI.Cout(Uid, "{0}张怪物牌/NPC牌被置入怪物牌堆.", count);
                        Z0P.MonCount += count;
                    }
                    else if (args[1] == "8")
                    {
                        ushort[] tuxes = Algo.TakeRange(args, 2, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        VI.Cout(Uid, "翻出手牌为【{0}】.", zd.Tux(tuxes));
                    }
                    break;
                case "E0IS":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        for (int i = 2; i < args.Length; ++i)
                            VI.Cout(Uid, "{0}获得了技能『{1}』.", zd.Player(ut), zd.SkillName(args[i]));
                    }
                    break;
                case "E0OS":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        for (int i = 2; i < args.Length; ++i)
                            VI.Cout(Uid, "{0}失去了技能『{1}』.", zd.Player(ut), zd.SkillName(args[i]));
                    }
                    break;
                case "E0LH":
                    for (int i = 1; i < args.Length; i += 3)
                    {
                        ushort incr = ushort.Parse(args[i]);
                        ushort ut = ushort.Parse(args[i + 1]);
                        ushort to = ushort.Parse(args[i + 2]);
                        VI.Cout(Uid, "{0}HP上限{1}为{2}点.", zd.Player(ut), (incr == 0 ? "减少" : "增加"), to);
                        Z0D[ut].HPa = to;
                        if (Z0D[ut].HP > Z0D[ut].HPa)
                            Z0D[ut].HP = Z0D[ut].HPa;
                    }
                    break;
                case "E0IV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        int hro = int.Parse(args[2]);
                        string guestName = "副角色牌";
                        Hero hero = Tuple.HL.InstanceHero(Z0D[ut].SelectHero);
                        if (hero != null && !string.IsNullOrEmpty(hero.GuestAlias))
                            guestName = hero.GuestAlias;
                        VI.Cout(Uid, "{0}迎来了{1}「{2}」.", zd.Player(ut), guestName, zd.Hero(hro));
                        Z0D[ut].Coss = hro;
                    }
                    break;
                case "E0OV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        int hro = int.Parse(args[2]);
                        int next = int.Parse(args[3]);
                        string guestName = "副角色牌";
                        Hero hero = Tuple.HL.InstanceHero(Z0D[ut].SelectHero);
                        if (hero != null && !string.IsNullOrEmpty(hero.GuestAlias))
                            guestName = hero.GuestAlias;
                        VI.Cout(Uid, "{0}送走了{1}「{2}」.", zd.Player(ut), guestName, zd.Hero(hro));
                        Z0D[ut].Coss = next;
                    }
                    break;
                case "E0PB":
                    for (int i = 2; i < args.Length; )
                    {
                        ushort who = ushort.Parse(args[i]);
                        ushort hind = ushort.Parse(args[i + 1]);
                        int n = ushort.Parse(args[i + 2]);
                        if (hind == 0)
                        {
                            List<ushort> cards = Algo.TakeRange(args, i + 3, i + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            if (args[1] == "0")
                            {
                                VI.Cout(Uid, "{0}将{1}放回手牌堆顶.", zd.Player(who), zd.Tux(cards));
                                Z0P.TuxCount += n;
                            }
                            else if (args[1] == "1")
                            {
                                VI.Cout(Uid, "{0}将{1}放回怪牌堆顶.", zd.Player(who), zd.Monster(cards));
                                Z0P.MonCount += n;
                            }
                            else if (args[1] == "2")
                            {
                                VI.Cout(Uid, "{0}将{1}放回事件牌堆顶.", zd.Player(who), zd.Eve(cards));
                                Z0P.EveCount += n;
                            }
                            i += (3 + n);
                        }
                        else if (hind == 1)
                        {
                            if (args[1] == "0")
                            {
                                VI.Cout(Uid, "{0}将{1}张牌放回手牌堆顶.", zd.Player(who), n);
                                Z0P.TuxCount += n;
                            }
                            else if (args[1] == "1")
                            {
                                VI.Cout(Uid, "{0}将{1}张牌放回怪牌堆顶.", zd.Player(who), n);
                                Z0P.MonCount += n;
                            }
                            else if (args[1] == "2")
                            {
                                VI.Cout(Uid, "{0}将{1}张牌放回事件牌堆顶.", zd.Player(who), n);
                                Z0P.EveCount += n;
                            }
                            i += 3;
                        }
                    }
                    break;
                case "E0HR":
                    if (args[1] == "0")
                        VI.Cout(Uid, "当前行动顺序变为正方向。");
                    else if (args[1] == "1")
                        VI.Cout(Uid, "当前行动顺序变成逆方向。");
                    break;
                case "E0FI":
                    if (args[1] == "O")
                        VI.Cout(Uid, "不触发战斗.");
                    else if (args[1] == "U")
                    {
                        Z0F.Hinder = 0; Z0F.Supporter = 0;
                        // A0F.Horn = 0;
                    }
                    else
                    {
                        List<ushort> leavers = new List<ushort>();
                        List<ushort> joiners = new List<ushort>();
                        List<string> msgs = new List<string>();
                        for (int i = 1; i < args.Length; i += 3)
                        {
                            char position = args[i][0];
                            ushort old = ushort.Parse(args[i + 1]);
                            ushort s = ushort.Parse(args[i + 2]);
                            string name = (s == 0 ? "无人" :
                                (s < 1000 ? zd.Player(s) : zd.Monster((ushort)(s - 1000))));
                            if (old != 0)
                                leavers.Add(old);
                            if (s != 0) {
                                joiners.Add(s);
                                if (position == 'T') {
                                    msgs.Add(string.Format("{0}触发战斗", name));
                                    //A0F.Trigger = s;
                                } else if (position == 'S') {
                                    msgs.Add(string.Format("{0}支援", name));
                                    Z0F.Supporter = s;
                                } else if (position == 'H') {
                                    msgs.Add(string.Format("{0}妨碍", name));
                                    Z0F.Hinder = s;
                                } else if (position == 'W') {
                                    msgs.Add(string.Format("{0}代为触发战斗", name));
                                    //A0F.Horn = s;
                                }
                            }
                        }
                        leavers.RemoveAll(p => joiners.Contains(p));
                        if (leavers.Count > 0)
                        {
                            msgs.AddRange(leavers.Select(p => string.Format("{0}退出战斗", 
                                (p == 0 ? "无人" : (p < 1000 ? zd.Player(p) :
                                zd.Monster((ushort)(p - 1000)))))));
                        }
                        if (msgs.Count > 0)
                            VI.Cout(Uid, string.Join(",", msgs) + ".");
                    }
                    break;
                case "E0SN":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort lugUt = ushort.Parse(args[2]);
                        bool dirIn = args[3] == "0";
                        string[] cards = Algo.TakeRange(args, 4, args.Length);
                        if (!Z0D[who].Treasures.ContainsKey(lugUt))
                            Z0D[who].Treasures[lugUt] = new List<string>();
                        if (dirIn)
                        {
                            Z0D[who].Treasures[lugUt].AddRange(cards);
                            VI.Cout(Uid, "{0}被收入{1}的{2}.",
                                zd.MixedCards(cards), zd.Player(who), zd.Tux(lugUt));
                        }
                        else
                        {
                            Z0D[who].Treasures[lugUt].RemoveAll(p => cards.Contains(p));
                            VI.Cout(Uid, "{0}被从{1}的{2}中取出.",
                                zd.MixedCards(cards), zd.Player(who), zd.Tux(lugUt));
                        }
                    }
                    break;
                case "E0MA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort guad = ushort.Parse(args[2]);
                        Z0D[who].Guardian = guad;
                        if (guad != 0)
                            VI.Cout(Uid, "{0}选择{1}.", zd.Player(who), zd.Guard(guad));
                        else
                            VI.Cout(Uid, "{0}撤销{1}.", zd.Player(who),
                                zd.GuardAlias(Z0D[who].SelectHero, Z0D[who].Coss));
                    }
                    break;
                case "E0ZJ":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        ushort slot = ushort.Parse(args[2]);
                        ushort eq = ushort.Parse(args[3]);
                        if (slot == 1) { Z0D[ut].Weapon = eq; Z0D[ut].ExEquip = 0; }
                        else if (slot == 2) { Z0D[ut].Armor = eq; Z0D[ut].ExEquip = 0; }
                        else if (slot == 3) { Z0D[ut].Trove = eq; Z0D[ut].ExEquip = 0; }
                    }
                    break;
                case "E0IF":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort[] sfs = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToArray();
                        Z0D[who].Runes.AddRange(sfs);
                        VI.Cout(Uid, "{0}获得身法{1}.", zd.Player(who), zd.Rune(sfs));
                    }
                    break;
                case "E0OF":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort[] sfs = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToArray();
                        Z0D[who].Runes.RemoveAll(p => sfs.Contains(p));
                        VI.Cout(Uid, "{0}失去身法{1}.", zd.Player(who), zd.Rune(sfs));
                    }
                    break;
            }
        }
        #endregion E
        #region F
        private void HandleF0Message(string readLine)
        {
            lock (listOfThreads)
            {
                foreach (Thread td in listOfThreads)
                {
                    if (td != Thread.CurrentThread && td.IsAlive)
                        td.Abort();
                }
                listOfThreads.Clear();
                listOfThreads.Add(Thread.CurrentThread);
            }
            // Reset Cin Count
            VI.TerminCinTunnel(Uid);
            WI.Send(readLine, Uid, 0);
            string[] args = readLine.Split(',');
            switch (args[0])
            {
                case "F0JM":
                    VI.Cout(Uid, "强制跳转至阶段{0}.", args[1]);
                    break;
                case "F0WN":
                    if (args[1] == "0")
                        VI.Cout(Uid, "游戏结束，平局.");
                    else
                        VI.Cout(Uid, "游戏结束，{0}方获胜.", args[1]);
                    GameGraceEnd = true;
                    break;
            }

        }
        #endregion F
        #region U
        private bool HandleU1Message(string inv, string mai)
        {
            bool cinCalled = false;
            ushort[] invs = inv.Split(',').Select(p => ushort.Parse(p)).ToArray();
            if (string.IsNullOrEmpty(mai) || mai.StartsWith("0,"))
            {
                VI.Cout(Uid, "等待下列玩家行动:{0}...", zd.Player(invs));
                int sina = int.Parse(mai.Substring("0,".Length));
                if ((sina & 1) != 0 && invs.Contains(Uid))
                {
                    cinCalled = StartCinEtc();
                    string input = VI.Cin(Uid, "您无法行动，输入任意键声明行动结束.");
                    if (input != VI.CinSentinel)
                        WI.Send("U2,0," + sina, Uid, 0);
                    VI.CloseCinTunnel(Uid);
                    return cinCalled;
                }
                else
                {
                    WI.Send("U2,0," + sina, Uid, 0);
                    VI.CloseCinTunnel(Uid);
                    return false;
                }
            }
            VI.Cout(Uid, "下列玩家与你均可行动:{0}.", zd.Player(invs));
            bool decided = false;
            while (!decided)
            {
                IDictionary<string, string> skTable = new Dictionary<string, string>();
                cinCalled = StartCinEtc();
                string[] blocks = mai.Split(';');
                string opt = "您可以发动";
                foreach (string block in blocks)
                {
                    //int jdx = -1;
                    //if (block.StartsWith("JN") || block.StartsWith("CZ") || block.StartsWith("NJ"))
                    //    jdx = block.IndexOf(',');
                    //else
                    //{
                    //    int kdx = block.IndexOf(',');
                    //    if (kdx >= 0)
                    //        jdx = block.IndexOf(',', kdx + 1);
                    //}
                    int jdx = block.IndexOf(',');
                    if (jdx < 0)
                    {
                        opt += zd.SKTXCZ(block) + ";";
                        skTable.Add(block, "^");
                    }
                    else
                    {
                        string name = block.Substring(0, jdx);
                        string rest = block.Substring(jdx + 1);
                        opt += zd.SKTXCZ(name) + ";";
                        skTable.Add(name, rest);
                    }
                }
                VI.Cout(Uid, opt.Substring(0, opt.Length - 1));
                string inputBase = VI.Cin(Uid, "请做出您的选择，0为放弃行动:");
                if (inputBase == VI.CinSentinel)
                    decided = true;
                else if (inputBase == "0")
                {
                    decided = true;
                    VI.Cout(Uid, "您决定放弃行动.");
                    WI.Send("U2,0", Uid, 0);
                }
                else if (skTable.ContainsKey(inputBase))
                {
                    if (skTable[inputBase] == "^")
                    {
                        decided = true;
                        WI.Send("U2," + inputBase, Uid, 0);
                    }
                    else
                    {
                        string input = FormattedInputWithCancelFlag(skTable[inputBase]);
                        if (!input.StartsWith("/"))
                        {
                            decided = true;
                            if (input != "")
                                input = "," + input;
                            WI.Send("U2," + inputBase + input, Uid, 0);
                        }
                    }
                    // otherwise, cancel and not action immediately, still wait
                }
            }
            return cinCalled;
        }
        private bool HandleU3Message(string mai, string prev, string inType)
        {
            bool cinCalled = false;
            cinCalled = StartCinEtc();
            string action = zd.AnalysisAction(mai, inType);
            VI.Cout(Uid, "已尝试{0}{1}，请继续：", action, zd.SKTXCZ(prev));
            string input = FormattedInputWithCancelFlag(mai);
            VI.CloseCinTunnel(Uid);
            if (!input.StartsWith("/") && input != "")
                WI.Send("U4," + prev + "," + input, Uid, 0);
            else
                WI.Send("U4,0", Uid, 0);
            return cinCalled;
        }
        private bool HandleU5Message(string involved, string mai, string inType)
        {
            VI.CloseCinTunnel(Uid);
            ushort owner = ushort.Parse(involved);
            string action = zd.AnalysisAction(mai, inType);
            string sktxcz = zd.SKTXCZ(mai, true, inType);
            if (sktxcz != "" && sktxcz.Contains(":"))
            {
                if (owner != Uid)
                    VI.Cout(Uid, "{0}{1}了{2}.", zd.Player(owner), action, sktxcz);
                else
                    VI.Cout(Uid, "您{0}了{1}.", action, sktxcz);
            }
            return false;
        }
        private bool HandleU7Message(string inv, string mai, string prev, string inType)
        {
            bool cinCalled = StartCinEtc();
            ushort owner = ushort.Parse(inv);
            string action = zd.AnalysisAction(mai, inType);
            VI.Cout(Uid, "{0}{1}{2}过程中，请继续：", zd.Player(owner), action, zd.SKTXCZ(prev));
            string input = FormattedInputWithCancelFlag(mai);
            VI.CloseCinTunnel(Uid);
            WI.SendDirect("U8," + prev + "," + input, Uid);
            return cinCalled;
        }
        private bool HandleU9Message(string inv, string prev, string inType)
        {
            ushort owner = ushort.Parse(inv);
            VI.Cout(Uid, "等待{0}响应中:{1}...", zd.Player(owner), zd.SKTXCZ(prev));
            VI.CloseCinTunnel(Uid);
            return false;
        }
        private bool HandleUAMessage(string inv, string mai, string inType)
        {
            ushort owner = ushort.Parse(inv);
            string action = inType.Contains('!') ? "爆发" : "执行";
            VI.Cout(Uid, "{0}{1}{2}完毕.", zd.Player(owner), action, zd.SKTXCZ(mai));
            return false;
        }
        #endregion U
        #region V
        public bool HandleV0Message(string cmdrst)
        {
            StartCinEtc();
            string[] blocks = cmdrst.Split(',');
            int invCount = int.Parse(blocks[0]);
            string input = FormattedInputWithCancelFlag(string.Join(
                    ",", Algo.TakeRange(blocks, 1 + invCount, blocks.Length)));
            VI.CloseCinTunnel(Uid);
            WI.Send("V1," + input, Uid, 0);
            return true;
        }
        public bool HandleV2Message(string cmdrst)
        {
            StartCinEtc();
            int idx = cmdrst.IndexOf(',');
            ushort major = ushort.Parse(cmdrst.Substring(0, idx));
            bool decided = false;
            while (!decided)
            {
                string input = FormattedInputWithCancelFlag(cmdrst.Substring(idx + 1));
                if (input == VI.CinSentinel)
                    decided = true;
                else
                {
                    WI.Send("V4," + input, Uid, 0);
                    decided = true;
                    return true;
                }
            }
            return false;
        }
        public bool HandleV3Message(string cmdrst)
        {
            string[] splits = cmdrst.Split(',');
            List<ushort> invs = Algo.TakeRange(splits, 0, splits.Length)
                .Select(p => ushort.Parse(p)).ToList();
            VI.Cout(Uid, "等待{0}响应.", zd.Player(invs));
            return false;
        }
        public bool HandleV5Message(string cmdrst)
        {
            VI.CloseCinTunnel(Uid);
            return false;
        }
        #endregion V
        #region R

        private bool HandleRMessage(string readLine)
        {
            int idx = readLine.IndexOf(',');
            ushort rounder = (ushort)(readLine[1] - '0');
            string cop = Substring(readLine, "R0".Length, idx);
            string para = idx >= 0 ? readLine.Substring(idx + 1) : "";

            bool cinCalled = false;

            switch (cop)
            {
                case "001":
                    {
                        ushort type = ushort.Parse(para);
                        if (type == 0)
                            VI.Cout(Uid, "{0}回合开始.", zd.Player(rounder));
                        else if (type == 1)
                            VI.Cout(Uid, "{0}回合跳过.", zd.Player(rounder));
                        else if (type == 2)
                        {
                            VI.Cout(Uid, "{0}回合跳过，恢复正常.", zd.Player(rounder));
                            Z0D[rounder].Immobilized = false;
                        }
                        break;
                    }
                case "EV1":
                    {
                        //if (rounder == Uid)
                        //{
                        //    cinCalled = StartCinEtc();
                        //    string reply = VI.Cin(Uid, "是否翻看事件牌？0/1：");
                        //    if (!reply.Equals("1"))
                        //        reply = "0";
                        //    WI.Send("R" + rounder + "EV2," + reply, Uid, 0);
                        //    VI.CloseCinTunnel(Uid);
                        //}
                        //else
                        if (rounder != Uid)
                        {
                            VI.Cout(Uid, "等待{0}是否翻看事件牌...", zd.Player(rounder));
                            VI.CloseCinTunnel(Uid);
                        }
                    }
                    break;
                case "EV2":
                    if (para == "0")
                    {
                        Z0F.Eve1 = 0;
                        VI.Cout(Uid, "决定不翻看事件牌.");
                    }
                    //else
                    //{
                    //    ushort no = ushort.Parse(para);
                    //    Z0F.Eve1 = no;
                    //    VI.Cout(Uid, "翻看事件牌【{0}】.", zd.Eve(no));
                    //}
                    break;
                case "GR":
                    if (para == "0")
                        VI.Cout(Uid, "{0}技牌阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}技牌阶段结束.", zd.Player(rounder));
                    break;
                case "GE":
                    if (para == "0")
                        VI.Cout(Uid, "{0}技牌结束阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}技牌结束阶段结束.", zd.Player(rounder));
                    break;
                case "Z0":
                    if (para == "0")
                        VI.Cout(Uid, "{0}战牌开始阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}战牌开始阶段结束.", zd.Player(rounder));
                    break;
                //case "ZW1":
                //    {
                //        string[] args = para.Split(',');
                //        ushort isSupport = ushort.Parse(args[0]);
                //        ushort isDecider = ushort.Parse(args[1]);
                //        ushort centre = ushort.Parse(args[2]);
                //        List<string> candidates = new List<string>();
                //        //string[] candidates = new string[args.Length - 2];
                //        bool cancellable = false;
                //        if (isDecider == 0 || isDecider == 1)
                //        {
                //            for (int i = 3; i < args.Length; ++i)
                //            {
                //                ushort ut;
                //                if (args[i] == "0")
                //                    cancellable = true;
                //                else if (ushort.TryParse(args[i], out ut))
                //                    candidates.Add(args[i]);
                //                else
                //                    candidates.Add("!" + args[i]);
                //            }
                //            cinCalled = StartCinEtc();
                //        }
                //        string identity = isSupport != 0 ? "妨碍者{1}—0则不妨碍." :
                //            "支援者{1}—{0}则不支援" + (cancellable ? "，0则不打怪." : ".");
                //        if (isDecider == 0) // decider
                //        {
                //            while (true)
                //            {
                //                string select = VI.Cin(Uid, "{0}战斗阶段开始，请决定" + identity,
                //                    zd.Player(rounder), zd.PlayerWithMonster(candidates));
                //                if (candidates.Contains(select) || candidates.Contains("!" + select))
                //                {
                //                    WI.Send("R" + rounder + "ZW4," + select, Uid, 0);
                //                    VI.CloseCinTunnel(Uid);
                //                    break;
                //                }
                //                else if (select == "0")
                //                {
                //                    if (cancellable && isSupport == 0) // Supporter = 0, not fight
                //                    {
                //                        WI.Send("R" + rounder + "ZW4,0", Uid, 0);
                //                        VI.CloseCinTunnel(Uid);
                //                        break;
                //                    }
                //                    else if (isSupport != 0) // Hinder = 0, not hinder
                //                    {
                //                        WI.Send("R" + rounder + "ZW4," + rounder, Uid, 0);
                //                        VI.CloseCinTunnel(Uid);
                //                        break;
                //                    }
                //                }
                //                else if (select == VI.CinSentinel)
                //                    break;
                //            }
                //            Thread.Sleep(100);
                //        }
                //        else if (isDecider == 1)
                //        {
                //            while (true)
                //            {
                //                string select = VI.Cin(Uid, "{0}战斗阶段开始，请推选" + identity,
                //                    zd.Player(rounder), zd.PlayerWithMonster(candidates));
                //                if (candidates.Contains(select) || candidates.Contains("!" + select))
                //                {
                //                    WI.Send("R" + rounder + "ZW2," + select, Uid, 0);
                //                    //VI.CloseCinTunnel(uid);
                //                    break;
                //                }
                //                else if (select == "0")
                //                {
                //                    if (cancellable && isSupport == 0) // Supporter = 0, not fight
                //                    {
                //                        WI.Send("R" + rounder + "ZW2,0", Uid, 0);
                //                        VI.CloseCinTunnel(Uid);
                //                        break;
                //                    }
                //                    else if (isSupport != 0) // Hinder = 0, not hinder
                //                    {
                //                        WI.Send("R" + rounder + "ZW2," + rounder, Uid, 0);
                //                        VI.CloseCinTunnel(Uid);
                //                        break;
                //                    }
                //                }
                //                else if (select == VI.CinSentinel)
                //                    break;
                //            }
                //            Thread.Sleep(100);
                //        }
                //        else if (isDecider == 2) // Watcher
                //            VI.Cout(Uid, "等待{0}决定{1}.", zd.Player(centre), isSupport != 0 ? "妨碍者" : "支援者");
                //        break;
                //    }
                case "ZW3":
                case "ZW5":
                    {
                        int jdx = para.IndexOf(',');
                        string suggest = cop[2] == '3' ? "建议" : "决定";
                        if (jdx >= 0)
                        {
                            ushort from = ushort.Parse(para.Substring(0, jdx));
                            string advice = para.Substring(jdx + 1);
                            if (advice.StartsWith("T"))
                            {
                                ushort who = ushort.Parse(advice.Substring("T".Length));
                                if (who == rounder)
                                    VI.Cout(Uid, "{0}{1}不让其它人参与战斗.", zd.Player(from), suggest);
                                else
                                    VI.Cout(Uid, "{0}{1}{2}参与战斗.", zd.Player(from), suggest, zd.Player(who)); 
                            }
                            else if (advice.StartsWith("G"))
                            {
                                ushort monCode = Tuple.ML.Encode(advice);
                                if (monCode > 0)
                                    VI.Cout(Uid, "{0}{1}{2}参与战斗.", zd.Player(from), suggest, zd.Monster(monCode));
                            }
                            else if (advice.StartsWith("/"))
                                VI.Cout(Uid, "{0}{1}不打怪.", zd.Player(from), suggest);
                        }
                        else
                        {
                            ushort from = ushort.Parse(para);
                            VI.Cout(Uid, "{0}已经做出了{1}.", zd.Player(from), suggest);
                        }
                    }
                    break;
                case "ZW7":
                    {
                        string[] args = para.Split(',');
                        if (args[0] == "0")
                        {
                            ushort mons = ushort.Parse(args[1]);
                            VI.Cout(Uid, "{0}决定不打怪，放弃怪物{1}.", zd.Player(rounder), zd.Monster(mons));
                        }
                        //else if (args[0] == "1")
                        //{
                        //    ushort s = ushort.Parse(args[1]);
                        //    string ss = (s == 0) ? "不支援" : "{1}进行支援";
                        //    VI.Cout(Uid, "{0}决定打怪，" + ss + ".", zd.Player(rounder),
                        //        s < 1000 ? zd.Player(s) : zd.Monster((ushort)(s - 1000)));
                        //    Z0F.Supporter = s;
                        //}
                        //else if (args[0] == "2")
                        //{
                        //    ushort h = ushort.Parse(args[1]);
                        //    string sh = (h == 0) ? "不妨碍" : "{0}进行妨碍";
                        //    VI.Cout(Uid, sh + ".", zd.Player(rounder),
                        //        h < 1000 ? zd.Player(h) : zd.Monster((ushort)(h - 1000)));
                        //    Z0F.Hinder = h;
                        //}
                    }
                    VI.TerminCinTunnel(Uid);
                    break;
                case "ZM1":
                    {
                        Z0F.Monster1 = 0; Z0F.Monster2 = 0;
                        //ushort mons = ushort.Parse(para);
                        //VI.Cout(Uid, "本场战斗怪物为【{0}】.", zd.Monster(mons));
                        //Z0F.Monster1 = mons;
                        break;
                    }
                case "NP1":
                    if (para == "0")
                        VI.Cout(Uid, "{0}NPC响应开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}NPC响应结束.", zd.Player(rounder));
                    break;
                case "NP2":
                    {
                        ushort mons = ushort.Parse(para);
                        //VI.Cout(Uid, "{0}跳过NPC，继续翻看怪物牌，结果为【{1}】.", zd.Player(rounder),
                        //    zd.Monster(mons));
                        VI.Cout(Uid, "{0}跳过NPC，继续翻看怪物牌.", zd.Player(rounder));
                        //Z0F.Monster1 = mons;
                        break;
                    }
                case "Z1":
                    if (para == "0")
                        VI.Cout(Uid, "{0}战斗开始阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}战斗开始阶段结束.", zd.Player(rounder));
                    break;
                //case "ZC1":
                //    {
                //        string[] args = para.Split(',');
                //        ushort s = ushort.Parse(args[0]);
                //        bool sy = args[1].Equals("1");
                //        ushort h = ushort.Parse(args[2]);
                //        bool hy = args[3].Equals("1");
                //        string comp1 = (s != 0) ? "{0}支援" + (sy ? "成功" : "失败") : "无支援";
                //        string comp2 = (h != 0) ? "{1}妨碍" + (hy ? "成功" : "失败") : "无妨碍";
                //        VI.Cout(uid, comp1 + "，" + comp2 + "。", zd.Player(s), zd.Player(h));
                //        break;
                //    }
                case "ZD":
                    if (para == "0")
                        VI.Cout(Uid, "{0}战牌阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}战牌阶段结束.", zd.Player(rounder));
                    break;
                case "VN":
                    VI.Cout(Uid, "判断本场战斗胜负结束,触发方{0}.", para == "0" ? "胜利" : "失败");
                    break;
                //case "VT":
                //case "VS":
                //    VI.Cout(Uid, "判断战斗胜负阶段中..."); break;                
                case "Z2":
                    Z0F.OPool = 0; Z0F.RPool = 0; break;
                case "Z3":
                    Z0F.Supporter = 0; Z0F.Hinder = 0;
                    break;
                case "ZF":
                    if (para == "0")
                        VI.Cout(Uid, "{0}战牌结束阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}战牌结束阶段结束.", zd.Player(rounder));
                    break;
                case "BC":
                    if (para == "0")
                        VI.Cout(Uid, "{0}补牌阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}补牌阶段结束.", zd.Player(rounder));
                    break;
                case "TM":
                    if (para == "0")
                        VI.Cout(Uid, "{0}回合结束阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}回合结束阶段结束.", zd.Player(rounder));
                    break;
                case "ED":
                    Z0F.Supporter = 0; Z0F.Hinder = 0;
                    Z0F.Monster1 = 0; Z0F.Monster2 = 0;
                    Z0F.Eve1 = 0;
                    Z0F.RPool = 0; Z0F.OPool = 0;
                    break;
            }
            return cinCalled;
        }
        #endregion R
        #region H
        private bool HandleHMessage(string cop, string cmdrst)
        {
            bool cinCalled = false;
            switch (cop)
            {
                //case "H1NW":
                //    {
                //        int idx = cmdrst.IndexOf(',');
                //        ushort who = ushort.Parse(cmdrst.Substring(0, idx));
                //        string name = cmdrst.Substring(idx + 1);
                //        if (Z0D.ContainsKey(who))
                //            Z0D[who] = new ZeroPlayer(name, this) { Uid = who };
                //        else
                //            Z0D.Add(who, new ZeroPlayer(name, this) { Uid = who });
                //    }
                //    break;
                case "H0SD":
                    {
                        string[] blocks = Algo.Splits(cmdrst, ",");
                        for (int i = 0; i < blocks.Length; i += 3)
                        {
                            ushort ut = ushort.Parse(blocks[i]);
                            ushort aut = ushort.Parse(blocks[i + 1]);
                            string name = blocks[i + 2];
                            if (Z0D.ContainsKey(ut))
                            {
                                if (Z0D[ut].Name != name)
                                    Z0D[ut] = new ZeroPlayer(name, this) { Uid = ut };
                            }
                            else
                                Z0D.Add(ut, new ZeroPlayer(name, this) { Uid = ut });
                            if (aut == auid)
                                Uid = ut;
                        }
                        string members = "";
                        for (ushort i = 1; i <= totalPlayer; ++i)
                        {
                            if (i == Uid)
                                members += (",[" + i + "#:" + Z0D[i].Name + "]");
                            else
                                members += ("," + i + "#:" + Z0D[i].Name);
                        }
                        if (members != "")
                            VI.Cout(Uid, "座位排序如下：" + members.Substring(1));
                    }
                    break;
                case "H0SM":
                    {
                        string[] blocks = cmdrst.Split(',');
                        SelMode = int.Parse(blocks[0]);
                        LevelCode = int.Parse(blocks[1]);
                    }
                    break;
                case "H0SW":
                    {
                        string[] splits = cmdrst.Split(',');
                        ushort type = ushort.Parse(splits[0]);
                        List<ushort> pys = Algo.TakeRange(splits, 1, splits.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        pys.Remove(Uid);
                        if (pys.Count > 0)
                        {
                            string[] words = new string[] { "选择", "禁选" };
                            VI.Cout(Uid, "等待玩家{0}{1}人物...", "{" + string.Join(",", pys) + "}", words[type]);
                        }
                    }
                    break;
                //case "H1LV":
                //    Z0D.Remove(ushort.Parse(cmdrst));
                //    break;
                //case "H1CN":
                //    VI.Cout(uid, "游戏开始啦！");
                //    break;
                case "H0RT":
                    casting = new Base.Rules.CastingPick(); break;
                case "H0RM":
                    {
                        List<int> cands = cmdrst.Split(',').Select(p => int.Parse(p)).ToList();
                        bool randomed = false;
                        var cp = casting as Base.Rules.CastingPick;
                        if (cands.Count > 0 && cands.Last() == 0)
                        {
                            cands.RemoveAt(cands.Count - 1);
                            randomed = true;
                        }
                        cp.Init(Uid, cands, randomed ? new int[] { 0 }.ToList() : null);
                        cinCalled = StartCinEtc();
                        while (true)
                        {
                            string input = VI.Cin(Uid, "您可选角色-{0}{1}", zd.HeroWithCode(cp.Xuan[Uid]),
                                (cp.Huan.ContainsKey(Uid) && cp.Huan[Uid].Count > 0 ? "(按0随机替换1人)" : ""));
                            int hero;
                            if (int.TryParse(input, out hero))
                            {
                                if ((hero != 0 && cp.Xuan[Uid].Contains(hero)) || cp.Huan[Uid].Count > 0)
                                {
                                    WI.Send("H0RN," + hero, Uid, 0);
                                    break;
                                }
                            }
                        }
                        VI.CloseCinTunnel(Uid);
                    }
                    break;
                case "H0RO":
                    {
                        string[] args = cmdrst.Split(',');
                        int code = int.Parse(args[0]);
                        if (code == 0)
                        {
                            ushort puid = ushort.Parse(args[1]);
                            VI.Cout(Uid, "玩家{0}#已经选定角色.", puid);
                        }
                        else if (code == 1)
                        {
                            ushort puid = ushort.Parse(args[1]);
                            int heroCode = int.Parse(args[2]);
                            if ((casting as Base.Rules.CastingPick).Pick(puid, heroCode))
                            {
                                if (puid == Uid)
                                    VI.Cout(Uid, "确认您的选择为{0}.", zd.Hero(heroCode));
                                else
                                    VI.Cout(Uid, "玩家{0}#已选择了角色{1}.", puid, zd.Hero(heroCode));
                                Z0D[puid].SelectHero = heroCode;
                                Z0D[puid].IsAlive = true;
                            }
                        }
                    }
                    break;
                case "H0RS":
                    {
                        int jdx = cmdrst.IndexOf(',');
                        int from = int.Parse(cmdrst.Substring(0, jdx));
                        int to = int.Parse(cmdrst.Substring(jdx + 1));
                        Base.Rules.CastingPick cp = casting as Base.Rules.CastingPick;
                        if (cp.SwitchTo(Uid, from, to) != 0)
                        {
                            cinCalled = StartCinEtc();
                            while (true)
                            {
                                string input = VI.Cin(Uid, "您可选角色-{0}.", zd.HeroWithCode(cp.Xuan[Uid]));
                                int hero;
                                if (int.TryParse(input, out hero))
                                {
                                    if ((hero != 0 && cp.Xuan[Uid].Contains(hero)))
                                    {
                                        WI.Send("H0RN," + hero, Uid, 0);
                                        break;
                                    }
                                }
                            }
                            VI.CloseCinTunnel(Uid);
                        }
                    }
                    break;
                case "H0TT":
                    {
                        string[] args = cmdrst.Split(',');
                        int xsz = int.Parse(args[0]);
                        List<int> x = Algo.TakeRange(args, 1, 1 + xsz)
                            .Select(p => int.Parse(p)).ToList();
                        int bsz1 = int.Parse(args[xsz + 1]);
                        List<int> b1 = Algo.TakeRange(args, 2 + xsz, 2 + xsz + bsz1)
                            .Select(p => int.Parse(p)).ToList();
                        int bsz2 = int.Parse(args[xsz + bsz1 + 2]);
                        List<int> b2 = Algo.TakeRange(args, 3 + xsz + bsz1, 3 + xsz + bsz1 + bsz2)
                            .Select(p => int.Parse(p)).ToList();

                        Base.Rules.CastingTable ct = new Base.Rules.CastingTable(x, b1, b2);
                        casting = ct;
                        VI.Cout(Uid, "当前可选角色-{0}.", zd.HeroWithCode(ct.Xuan));
                    }
                    break;
                case "H0TX":
                    {
                        // Verify whether these lists are the same.
                        //string[] args = cmdrst.Split(',');
                        //List<int> cands = Algo.TakeRange(args, 0, args.Length)
                        //    .Select(p => int.Parse(p)).ToList();
                        //selCandidates.Clear(); selCandidates.AddRange(cands);
                        cinCalled = StartCinEtc();
                        Base.Rules.CastingTable ct = casting as Base.Rules.CastingTable;
                        while (true)
                        {
                            string input = VI.Cin(Uid, "请选择一名角色，当前可选角色：\n{0}",
                                zd.HeroWithCode(ct.Xuan));
                            int hero;
                            if (int.TryParse(input, out hero) && ct.Xuan.Contains(hero))
                            {
                                WI.Send("H0TN," + hero, Uid, 0);
                                break;
                            }
                        }
                        VI.CloseCinTunnel(Uid);
                    }
                    break;
                case "H0TO":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort puid = ushort.Parse(args[0]);
                        int heroCode = int.Parse(args[1]);
                        if (puid == Uid)
                            VI.Cout(Uid, "确认您的选择为{0}.", zd.Hero(heroCode));
                        else
                            VI.Cout(Uid, "玩家{0}#已选择了角色{1}.", puid, zd.Hero(heroCode));
                        Z0D[puid].SelectHero = heroCode;
                        Z0D[puid].IsAlive = true;
                        (casting as Base.Rules.CastingTable).Pick(puid, heroCode);
                    }
                    break;
                case "H0TA":
                    {
                        Base.Rules.CastingTable ct = casting as Base.Rules.CastingTable;
                        string[] args = cmdrst.Split(',');
                        cinCalled = StartCinEtc();
                        while (true)
                        {
                            bool canGiveup = args.Contains("0");
                            string input = VI.Cin(Uid, "请禁选一名角色" + (canGiveup ? "(0为不禁选)" : "") +
                                 "，当前可选角色：\n{0}", zd.HeroWithCode(ct.Xuan));
                            int hero;
                            if (int.TryParse(input, out hero) && (ct.Xuan.Contains(hero) || canGiveup && hero == 0))
                            {
                                WI.Send("H0TB," + hero, Uid, 0);
                                break;
                            }
                        }
                        VI.CloseCinTunnel(Uid);
                    }
                    break;
                case "H0TC":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort puid = ushort.Parse(args[0]);
                        List<int> hrs = Algo.TakeRange(args, 1, args.Length)
                            .Select(p => int.Parse(p)).ToList();
                        Base.Rules.CastingTable ct = casting as Base.Rules.CastingTable;
                        if (hrs.Count == 0 && hrs[0] == 0)
                            VI.Cout(Uid, "玩家{0}#未禁选.", puid, zd.Hero(hrs));
                        else
                        {
                            VI.Cout(Uid, "玩家{0}#禁选了角色{1}.", puid, zd.Hero(hrs));
                            foreach (int heroCode in hrs)
                                ct.Ban(puid, heroCode);
                        }
                        if (ct.Xuan.Count > 0)
                            VI.Cout(Uid, "当前剩余角色:\n{0}.", zd.HeroWithCode(ct.Xuan));
                    }
                    break;
                case "H0TJ":
                    {
                        string[] args = cmdrst.Split(',');
                        List<int> hrs = Algo.TakeRange(args, 0, args.Length)
                            .Select(p => int.Parse(p)).ToList();
                        VI.Cout(Uid, "新增了角色{0}.", zd.Hero(hrs));
                        Base.Rules.CastingTable ct = casting as Base.Rules.CastingTable;
                        foreach (int heroCode in hrs)
                            ct.PutBack(heroCode);
                        if (ct.Xuan.Count > 0)
                            VI.Cout(Uid, "当前剩余角色:\n{0}.", zd.HeroWithCode(ct.Xuan));
                    }
                    break;
                case "H0PT":
                    {
                        string[] args = cmdrst.Split(',');
                        int idx = 0;
                        int xsz = int.Parse(args[idx]); ++idx;
                        List<int> x = Algo.TakeRange(args, idx, idx + xsz)
                            .Select(p => int.Parse(p)).ToList();
                        idx += xsz;
                        int drsz = int.Parse(args[idx]); ++idx;
                        List<int> dr = Algo.TakeRange(args, idx, idx + drsz)
                            .Select(p => int.Parse(p)).ToList();
                        idx += drsz;
                        int dbsz = int.Parse(args[idx]); ++idx;
                        List<int> db = Algo.TakeRange(args, idx, idx + dbsz)
                            .Select(p => int.Parse(p)).ToList();
                        idx += dbsz;
                        int brsz = int.Parse(args[idx]); ++idx;
                        List<int> br = Algo.TakeRange(args, idx, idx + brsz)
                            .Select(p => int.Parse(p)).ToList();
                        idx += brsz;
                        int bbsz = int.Parse(args[idx]); ++idx;
                        List<int> bb = Algo.TakeRange(args, idx, idx + bbsz)
                            .Select(p => int.Parse(p)).ToList();
                        idx += bbsz;

                        Base.Rules.CastingPublic cp = new Base.Rules.CastingPublic(x, dr, db, br, bb);
                        casting = cp;
                        cp.ToHint(Uid, VI, zd.HeroWithCode, zd.Hero);
                    }
                    break;
                case "H0PA":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort who = ushort.Parse(args[0]);
                        if (who == Uid)
                        {
                            Base.Rules.CastingPublic cp = casting as Base.Rules.CastingPublic;
                            cinCalled = StartCinEtc();
                            while (true)
                            {
                                string input = VI.Cin(Uid, "请禁选一名角色，当前可选角色：\n{0}",
                                    zd.HeroWithCode(cp.Xuan));
                                int hero;
                                if (int.TryParse(input, out hero) && cp.Xuan.Contains(hero))
                                {
                                    WI.Send("H0PB," + hero, Uid, 0);
                                    break;
                                }
                            }
                            VI.CloseCinTunnel(Uid);
                        }
                        else
                            VI.Cout(Uid, "请等待{0}#禁将.", who);
                    }
                    break;
                case "H0PC":
                    {
                        string[] args = cmdrst.Split(',');
                        int team = int.Parse(args[0]);
                        int selAva = int.Parse(args[1]);                        
                        Base.Rules.CastingPublic cp = casting as Base.Rules.CastingPublic;
                        cp.Ban(team == 1, selAva);
                        VI.Cout(Uid, "{0}方禁选{1}.", team == 1 ? "红" : "蓝", zd.Hero(selAva));
                    }
                    break;
                case "H0PM":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort who = ushort.Parse(args[0]);
                        if (who == Uid)
                        {
                            Base.Rules.CastingPublic cp = casting as Base.Rules.CastingPublic;
                            cinCalled = StartCinEtc();
                            while (true)
                            {
                                string input = VI.Cin(Uid, "请选择一名角色，当前可选角色：\n{0}",
                                    zd.HeroWithCode(cp.Xuan));
                                int hero;
                                if (int.TryParse(input, out hero) && cp.Xuan.Contains(hero))
                                {
                                    WI.Send("H0PN," + hero, Uid, 0);
                                    break;
                                }
                            }
                            VI.CloseCinTunnel(Uid);
                        }
                        else
                            VI.Cout(Uid, "请等待{0}#选将.", who);
                    }
                    break;
                case "H0PO":
                    {
                        string[] args = cmdrst.Split(',');
                        int team = int.Parse(args[0]);
                        int selAva = int.Parse(args[1]);
                        Base.Rules.CastingPublic cp = casting as Base.Rules.CastingPublic;
                        cp.Pick(team == 1, selAva);
                        VI.Cout(Uid, "{0}方选择了{1}.", team == 1 ? "红" : "蓝", zd.Hero(selAva));
                    }
                    break;
                case "H0CT":
                    {
                        string[] args = cmdrst.Split(',');
                        int xsz1 = int.Parse(args[0]);
                        List<int> x1 = Algo.TakeRange(args, 1, 1 + xsz1)
                            .Select(p => int.Parse(p)).ToList();
                        int xsz2 = int.Parse(args[xsz1 + 1]);
                        List<int> x2 = Algo.TakeRange(args, 2 + xsz1, 2 + xsz1 + xsz2)
                            .Select(p => int.Parse(p)).ToList();
                        Base.Rules.CastingCongress cc = new Base.Rules.CastingCongress(x1, x2, new List<int>());
                        casting = cc;
                        cc.CaptainMode = false;
                        for (int i = xsz1 + xsz2 + 2; i < args.Length; i += 2)
                            cc.Init(ushort.Parse(args[i]), int.Parse(args[i + 1]));

                        cc.ToHint(Uid, VI, zd.HeroWithCode, zd.Hero);
                        cinCalled = StartCinEtc();
                        while ((Uid % 2 == 0 && !cc.DecidedAo) || (Uid % 2 == 1 && !cc.DecidedAka))
                        {

                            List<int> xuanR = (Uid % 2 == 0) ? cc.XuanAo : cc.XuanAka;
                            if (VI is ClientZero.VW.Ayvi)
                            {
                                string op = (VI as ClientZero.VW.Ayvi).Cin48(Uid);
                                op = op.Trim().ToUpper();
                                int selAva;
                                bool has = cc.Ding[Uid] != 0;
                                if (op == "X" && cc.IsCaptain(Uid))
                                    WI.Send("H0CD,0", Uid, 0);
                                else if (op == "0" && has)
                                    WI.Send("H0CB," + cc.Ding[Uid], Uid, 0);
                                else if (int.TryParse(op, out selAva) && xuanR.Contains(selAva))
                                    WI.Send("H0CN," + Uid + "," + selAva, Uid, 0);
                            }
                        }
                        VI.CloseCinTunnel(Uid);
                    }
                    break;
                case "H0CI":
                    {
                        string[] args = cmdrst.Split(',');
                        int xsz1 = int.Parse(args[0]);
                        List<int> x1 = Algo.TakeRange(args, 1, 1 + xsz1)
                            .Select(p => int.Parse(p)).ToList();
                        int xsz2 = int.Parse(args[xsz1 + 1]);
                        List<int> x2 = Algo.TakeRange(args, 2 + xsz1, 2 + xsz1 + xsz2)
                            .Select(p => int.Parse(p)).ToList();
                        Base.Rules.CastingCongress cc = new Base.Rules.CastingCongress(x1, x2, new List<int>());
                        casting = cc;
                        cc.CaptainMode = true;
                        for (int i = xsz1 + xsz2 + 2; i < args.Length; i += 2)
                            cc.Init(ushort.Parse(args[i]), int.Parse(args[i + 1]));

                        cc.ToHint(Uid, VI, zd.HeroWithCode, zd.Hero);
                        if (cc.IsCaptain(Uid)) // Captain Only
                        {
                            VI.Cout(Uid, "===> 选择目标玩家与角色，以逗号分隔；0为退回，X为选将确定.");
                            cinCalled = StartCinEtc();
                            bool isAka = (Uid % 2 == 1);
                            while ((isAka && !cc.DecidedAka) || (!isAka && !cc.DecidedAo))
                            {
                                List<int> xuanR = (Uid % 2 == 0) ? cc.XuanAo : cc.XuanAka;
                                if (VI is ClientZero.VW.Ayvi)
                                {
                                    string op = (VI as ClientZero.VW.Ayvi).Cin48(Uid);
                                    op = op.Trim().ToUpper();
                                    bool has = cc.Ding[Uid] != 0;
                                    if (op == "X")
                                        WI.Send("H0CD,0", Uid, 0);
                                    else
                                    {
                                        int idx = op.IndexOf(',');
                                        if (idx >= 0)
                                        {
                                            ushort who; int selAva;
                                            if (ushort.TryParse(op.Substring(0, idx), out who) &&
                                                int.TryParse(op.Substring(idx + 1), out selAva))
                                            {
                                                if (selAva == 0 && cc.Ding[who] != 0)
                                                    WI.Send("H0CB," + cc.Ding[who], Uid, 0);
                                                else
                                                    WI.Send("H0CN," + who + "," + selAva, Uid, 0);
                                            }
                                        }
                                    }
                                }
                            }
                            VI.CloseCinTunnel(Uid);
                        }
                    }
                    break;
                case "H0CO":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort puid = ushort.Parse(args[0]);
                        int heroCode = int.Parse(args[1]);
                        int backCode = int.Parse(args[2]);
                        ushort backTo = ushort.Parse(args[3]);
                        if (puid == Uid)
                            VI.Cout(Uid, "您预选了{0}.", zd.Hero(heroCode));
                        else
                            VI.Cout(Uid, "玩家{0}#已预选了{1}.", puid, zd.Hero(heroCode));
                        if (backTo != 0)
                            VI.Cout(Uid, "玩家{0}#已预选了{1}.", backTo, zd.Hero(backCode));
                        Base.Rules.CastingCongress cc = casting as Base.Rules.CastingCongress;
                        cc.Set(puid, heroCode);
                        cc.ToHint(Uid, VI, zd.HeroWithCode, zd.Hero);
                        if (!cc.CaptainMode || cc.IsCaptain(Uid))
                            cc.ToInputRequire(Uid, VI);
                    }
                    break;
                case "H0CC":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort puid = ushort.Parse(args[0]);
                        int heroCode = int.Parse(args[1]);
                        VI.Cout(Uid, "{0}被放回选将池中.", zd.Hero(heroCode));
                        Base.Rules.CastingCongress cc = casting as Base.Rules.CastingCongress;
                        cc.Set(0, heroCode);
                        cc.ToHint(Uid, VI, zd.HeroWithCode, zd.Hero);
                        if (!cc.CaptainMode || cc.IsCaptain(Uid))
                            cc.ToInputRequire(Uid, VI);
                    }
                    break;
                case "H0CE":
                    {
                        string[] args = cmdrst.Split(',');
                        int code = int.Parse(args[0]);
                        if (code == 0)
                            VI.Cout(Uid, "抱歉，您队伍角色尚未全部选定。");
                        else if (code == 1)
                        { // The Opponent
                            int team = int.Parse(args[1]);
                            if (IsUtTeam(team))
                                VI.Cout(Uid, "我方已经选定角色.");
                            else if (!IsUtOpp(team))
                                VI.Cout(Uid, "对方已经选定角色.");
                            else
                                VI.Cout(Uid, "{0}方已经选定角色.", team == 2 ? "蓝" : "红");
                        }
                        else if (code == 2)
                        {
                            Base.Rules.CastingCongress cc = casting as Base.Rules.CastingCongress;
                            if (IsUtAo())
                                cc.DecidedAo = true;
                            else
                                cc.DecidedAka = true;
                            VI.TerminCinTunnel(Uid);
                            string msg = "我方选择结果为：";
                            for (int i = 1; i < args.Length; i += 2)
                            {
                                ushort ut = ushort.Parse(args[i]);
                                int ava = int.Parse(args[i + 1]);
                                Z0D[ut].SelectHero = ava;
                                Z0D[ut].IsAlive = true;
                                msg += (ut + ":" + zd.Hero(ava) + ",");
                            }
                            VI.Cout(Uid, "{0}.", msg.Substring(0, msg.Length - 1));
                        }
                    }
                    break;
                case "H0SL":
                    {
                        string[] args = cmdrst.Split(',');
                        for (int i = 0; i < args.Length; i += 2)
                        {
                            ushort puid = ushort.Parse(args[i]);
                            int heroCode = ushort.Parse(args[i + 1]);
                            //msg += (puid + ":" + zd.Hero(heroCode) + ",");
                            Z0D[puid].SelectHero = heroCode;
                            Z0D[puid].ParseFromHeroLib();
                        }
                        VI.Cout(Uid, "选择结果为-" + string.Join(",", Z0D.Values
                            .Where(p => p.SelectHero != 0)
                            .Select(p => p.Uid + ":" + zd.Hero(p.SelectHero))));
                        Z0F = new ZeroField(this);
                        Z0M = new ZeroMe(this);
                    }
                    break;
                case "H0DP":
                    {
                        string[] args = cmdrst.Split(',');
                        Z0P = new ZeroPiles(this)
                        {
                            TuxCount = int.Parse(args[0]),
                            MonCount = int.Parse(args[1]),
                            EveCount = int.Parse(args[2]),
                            TuxDises = 0,
                            MonDises = 0,
                            EveDises = 0
                        };
                    }
                    break;
                case "H0SN":
                    break;
                case "H0ST":
                    if (cmdrst.StartsWith("0"))
                    {
                        VI.Cout(Uid, "游戏开始阶段开始...");
                        //VI.ReleaseCin(uid);
                    }
                    else if (cmdrst.StartsWith("1"))
                    {
                        VI.Cout(Uid, "游戏开始阶段结束...");
                        //VI.ReleaseCin(uid);
                    }
                    break;
                case "H0TM":
                    VI.Cout(Uid, "游戏结束啦。");
                    break;
                case "H09N":
                    {
                        string[] blocks = cmdrst.Split(',');
                        for (int idx = 0; idx < blocks.Length; idx += 2)
                        {
                            ushort who = ushort.Parse(blocks[idx]);
                            string name = blocks[idx + 1];
                            Z0D[who] = new ZeroPlayer(name, this);
                        }
                        break;
                    }
                case "H09G":
                    Algo.LongMessageParse(cmdrst.Split(','), (who) => { Z0D[who].Uid = who; },
                        (who, key, value) =>
                        {
                            ZeroPlayer zp = Z0D[who];
                            switch (key)
                            {
                                case "hero": zp.SelectHero = (int)value; break;
                                case "state":
                                    zp.IsAlive = (((int)value & 1) != 0);
                                    zp.IsLoved = (((int)value & 2) != 0);
                                    zp.Immobilized = (((int)value & 4) != 0);
                                    zp.PetDisabled = (((int)value & 8) != 0); break;
                                case "hp": zp.HP = (ushort)value; break;
                                case "hpa": zp.HPa = (ushort)value; break;
                                case "str": zp.STR = (ushort)value; break;
                                case "stra": zp.STRa = (ushort)value; break;
                                case "dex": zp.DEX = (ushort)value; break;
                                case "dexa": zp.DEXa = (ushort)value; break;
                                case "tuxCount": zp.TuxCount = (int)value; break;
                                case "wp": zp.Weapon = (ushort)value; break;
                                case "am": zp.Armor = (ushort)value; break;
                                case "tr": zp.Trove = (ushort)value; break;
                                case "exq": zp.ExEquip = (ushort)value; break;
                                case "lug":
                                    zp.Treasures[zp.Trove].AddRange((string[])value); break;
                                case "guard": zp.Guardian = (ushort)value; break;
                                case "coss": zp.Coss = (ushort)value; break;
                                case "pet":
                                    zp.Pets.Clear(); zp.Pets.AddRange((ushort[])value); break;
                                case "excard":
                                    zp.ExCards.Clear(); zp.ExCards.AddRange((ushort[])value); break;
                                case "token": zp.Token = (int)value; break;
                                case "fakeq":
                                    for (int i = 0; i < ((string[])value).Length; i += 2)
                                        zp.Fakeq[ushort.Parse(((string[])value)[i])] = ((string[])value)[i + 1];
                                    break;
                                case "rune":
                                    zp.Runes.Clear(); zp.Runes.AddRange((ushort[])value); break;
                                case "excl":
                                    zp.SpecialCards.Clear(); zp.SpecialCards.AddRange((string[])value); break;
                                case "tar":
                                    zp.PlayerTars.Clear(); zp.PlayerTars.AddRange((ushort[])value); break;
                                case "awake":
                                    zp.AwakeSignal = (ushort)value == 1; break;
                                case "foldsz": zp.FolderCount = (int)value; break;
                                case "escue":
                                    zp.Escue.Clear(); zp.Escue.AddRange((ushort[])value); break;
                            }
                        }, Board.StatusKey);
                    break;
                case "H09P":
                    {
                        Z0M = new ZeroMe(this);
                        Z0F = new ZeroField(this);
                        Z0P = new ZeroPiles(this);

                        string[] blocks = cmdrst.Split(',');
                        Z0F.Eve1 = ushort.Parse(blocks[0]);
                        Z0P.TuxCount = int.Parse(blocks[1]);
                        Z0P.MonCount = int.Parse(blocks[2]);
                        Z0P.EveCount = int.Parse(blocks[3]);

                        Z0P.TuxDises = int.Parse(blocks[4]);
                        Z0P.MonDises = int.Parse(blocks[5]);
                        Z0P.EveDises = int.Parse(blocks[6]);

                        ushort rounder = ushort.Parse(blocks[7]);
                        ushort supporter = ushort.Parse(blocks[8]);
                        ushort hinder = ushort.Parse(blocks[9]);

                        Z0F.Hinder = hinder;
                        Z0F.Supporter = supporter;

                        ushort mon1 = ushort.Parse(blocks[10]);
                        ushort mon2 = ushort.Parse(blocks[11]);
                        ushort eve1 = ushort.Parse(blocks[12]);
                        Z0F.Monster1 = mon1; Z0F.Monster2 = mon2; Z0F.Eve1 = eve1;

                        for (int i = 13; i < Math.Min(blocks.Length, 17); i += 2)
                        {
                            if (blocks[i] == "1")
                            {
                                if (rounder % 2 == 0) Z0F.RPool = int.Parse(blocks[i + 1]);
                                else Z0F.OPool = int.Parse(blocks[i + 1]);
                            }
                            else if (blocks[i] == "2")
                            {
                                if (rounder % 2 == 0) Z0F.OPool = int.Parse(blocks[i + 1]);
                                else Z0F.RPool = int.Parse(blocks[i + 1]);
                            }
                        }
                        for (int i = 17; i < blocks.Length; i += 2)
                            Z0P.Score[int.Parse(blocks[i])] = int.Parse(blocks[i + 1]);
                    }
                    break;
                case "H0LT":
                    if (!GameGraceEnd) {
                        ushort who = ushort.Parse(cmdrst);
                        if (who != 0)
                            VI.Cout(Uid, "玩家{0}#逃跑，游戏终结。", who);
                        else
                            VI.Cout(Uid, "服务器被延帝抓走啦，游戏结束。", who);
                    }
                    break;
                case "H0WT":
                    if (!GameGraceEnd)
                    {
                        ushort who = ushort.Parse(cmdrst);
                        if (who != 0)
                            VI.Cout(Uid, "玩家{0}#断线，请耐心等待其重连～", who);
                    }
                    break;
                case "H0WD":
                    if (!GameGraceEnd) {
                        int secLeft = int.Parse(cmdrst);
                        VI.Cout(Uid, "房间将在{0}秒后彻底关闭。");
                    }
                    break;
                case "H0BK":
                    {
                        ushort who = ushort.Parse(cmdrst);
                        if (who != 0)
                            VI.Cout(Uid, "玩家{0}#恢复连接。", who);
                    }
                    break;
                case "H0RK":
                    VI.Cout(Uid, "房间已恢复正常。");
                    break;
                case "H09F":
                    {
                        string[] blocks = cmdrst.Split(',');
                        int idx = 0;
                        int tuxCount = int.Parse(blocks[idx]);
                        ++idx;
                        List<ushort> tuxes = Algo.TakeRange(blocks, idx, idx + tuxCount)
                            .Select(p => ushort.Parse(p)).ToList();
                        Z0M.Tux = tuxes;
                        idx += tuxCount;
                        int folderCount = int.Parse(blocks[idx]);
                        ++idx;
                        List<ushort> folders = Algo.TakeRange(blocks, idx, idx + folderCount)
                            .Select(p => ushort.Parse(p)).ToList();
                        Z0M.Folder = folders;
                        idx += folderCount;
                        int skillCount = int.Parse(blocks[idx]);
                        ++idx;
                        //List<string> skills = Algo.TakeRange(blocks, idx, idx + skillCount).ToList();
                        //Z0M.Skills = skills;
                        idx += skillCount;
                    }
                    break;
            }
            return cinCalled;
        }
        #endregion H
        #region Y
        private void HandleYMessage(string cop)
        {
            if (cop.StartsWith("Y2"))
            {
                string grp = cop.Substring("Y2,".Length);
                int idx = grp.IndexOf(',');
                ushort uit = ushort.Parse(grp.Substring(0, idx));
                string msgtext = grp.Substring(idx + 1);
                if (Z0D.ContainsKey(uit))
                {
                    if (Z0D[uit].SelectHero != 0)
                        VI.Chat(msgtext, zd.Hero(Z0D[uit].SelectHero) + "(" + Z0D[uit].Name + ")");
                    else
                        VI.Chat(msgtext, Z0D[uit].Name);
                }
                else
                    VI.Chat(msgtext, uit + "#");
            }
            else if (cop.StartsWith("Y4"))
            {
                string grp = cop.Substring("Y4,".Length);
                ushort opt = ushort.Parse(grp);
                if (opt == 1)
                    VI.Cout(Uid, "切换为启用技能优化模式。");
                else if (opt == 2)
                    VI.Cout(Uid, "切换为禁用技能优化模式。");
                else if (opt == 3)
                    VI.Cout(Uid, "切换为启用特殊牌优化模式。");
                else if (opt == 4)
                    VI.Cout(Uid, "切换为禁用特殊牌优化模式。");
            }
        }
        #endregion Y

        private static string Substring(string @string, int start, int end)
        {
            if (end >= 0)
                return @string.Substring(start, end - start);
            else
                return @string.Substring(start);
        }

        private static int CountItemFromComma(string line) {
            if (string.IsNullOrEmpty(line))
                return 0;
            int count = 1;
            int idx = line.IndexOf(',');
            while (idx < line.Length && idx >= 0) {
                ++count;
                idx = line.IndexOf(',', idx + 1);
            }
            return count;
        }

        private bool IsUtAka() { return Uid % 2 == 1 && Uid > 0 && Uid < 1000; }
        private bool IsUtAo() { return Uid % 2 == 0 && Uid > 0 && Uid < 1000; }
        private bool IsUtTeam(int team) { return Uid % 2 + 1 == team && Uid > 0 && Uid < 1000; }
        private bool IsUtOpp(int team) { return Uid % 2 + team == 3 && Uid > 0 && Uid < 1000; }
        private static bool IsUtTeammate(ushort ut, ushort uu)
        {
            return ut % 2 == uu % 2 && ut > 0 && ut < 1000 && uu > 0 && uu < 1000;
        }

        #region Network Report
        internal void ReportConnectionLost()
        {
            VI.Cout(Uid, "服务器被罗刹鬼婆抓走了，游戏结束。");
        }
        #endregion Network Report
    }
}
