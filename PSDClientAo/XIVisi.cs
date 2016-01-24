using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.ClientAo
{
    public partial class XIVisi
    {
        #region Basic Members
        public VW.Cyvi VI { private set; get; }
        public Base.VW.IWICL WI { private set; get; }

        public Base.LibGroup Tuple { private set; get; }

        public const ushort WATCHER_1ST_PERSPECT = 1;

        private ushort m_uid;
        // uid in room, referenced in game procedure
        // 2 ways to get updated. H0SD events or H09G
        private ushort Uid
        {
            set
            {
                m_uid = value;
                var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                ad.ResetGameTitle(ass.Name + " v" + ass.Version + " - " + Uid);
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
        private int mLevelCode;
        public int LevelCode
        {
            private set
            {
                //int mPkgCode = mLevelCode >> 1;
                int mValue = value >> 1;
                //bool old2Equip = mPkgCode > 0 && mPkgCode < 3;
                //bool new2Equip = mValue > 0 && mValue < 3;
                //if (old2Equip != new2Equip)
                //    ad.SetPlayerXBSlot(!new2Equip);
                ad.SetPlayerXBSlot(mValue == 0 || mValue >= 3);
                mLevelCode = value;
            }
            get { return mLevelCode; }
        }
        private readonly bool isReplay;
        //private Base.Rules.Casting casting; // exists in yfArena.AoArena
        // Displays Members
        internal ZeroDisplay zd;
        internal AoDisplay ad;

        public AoField A0F { private set; get; }
        public AoMe A0M { private set; get; }
        public IDictionary<ushort, AoPlayer> A0P { private set; get; }
        public AoCEE A0C { private set; get; }
        public OI.AoDeal A0D { private set; get; }
        public AoOrchis A0O { private set; get; }
        private bool GameGraceEnd { set; get; }

        private Auxs.FlashWindowHelper flashHelper;
        //var helper = new Auxs.FlashWindowHelper(System.Windows.Application.Current);
        //// Flashes the window and taskbar 5 times and stays solid 
        //// colored until user focuses the main window
        //ad.Dispatcher.BeginInvoke((Action)(() =>
        //{
        //    helper.FlashApplicationWindow(ad);
        //}));

        // list of running threads handling events
        private List<Thread> listOfThreads;

        private Queue<string> unhandledMsg;

        public Log Log { private set; get; }
        // Constructor 1#: Used for Hall setting
        public XIVisi(ushort uid, string name, int teamCode, Base.VW.IVI vi,
            string server, int room, bool record, bool logmsg, bool watcher, AoDisplay ad)
        {
            this.auid = uid; this.name = name;
            this.VI = vi as VW.Cyvi;
            hopeTeam = teamCode;
            this.server = server;
            this.Room = room;
            this.port = Base.NetworkCode.HALL_PORT + room;

            this.ad = ad;
            VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, hopeTeam, uid, this);
            Log = new Log(); Log.Start(auid, record, logmsg, 0);
            bywi.Log = Log;
            CommonConstruct();
            //if (VI is VW.Ayvi)
            //    (VI as VW.Ayvi).Log = Log;
            WI = bywi; bywi.StartConnect(watcher);
            if (watcher)
                Uid = bywi.Uid;
            VI.Cout(uid, "游戏开始咯~");
            isReplay = false;
        }
        // Constructor 2#: Used for Direct Connection
        private XIVisi(string server, int port, string name, int avatar,
            int hopeTeam, bool record, bool watcher, bool msglog, AoDisplay ad)
        {
            this.server = server; this.port = port;
            this.name = name; this.avatar = avatar;
            this.hopeTeam = hopeTeam;

            VI = new VW.Cyvi(ad, record, msglog);
            VI.Init(); VI.SetInGame(true);
            VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, hopeTeam, 0, this);
            WI = bywi;

            this.ad = ad;
            Log = new Log(); Log.Start(Uid, record, msglog, 0);
            CommonConstruct();
            if (!bywi.StartConnectDirect(watcher, VI))
            {
                VI.Cout(Uid, "咦，您是不是掉线或者连错人了:-(");
                auid = 0; return;
            }
            VI.Cout(Uid, watcher ? "您开始旁观~" : "游戏开始咯~");
            this.auid = bywi.Uid;
            if (watcher)
                Uid = bywi.Uid;
            bywi.Log = Log; VI.Log = Log;
            WI.Send("C2ST," + Uid, Uid, 0);
            isReplay = false;
        }
        public static XIVisi CreateInDirectConnect(string server, int port, string name,
            int avatar, int hopeTeam, bool record, bool watcher, bool msglog, AoDisplay ad)
        {
            return new XIVisi(server, port, name, avatar,
                hopeTeam, record, watcher, msglog, ad);
        }
        // Constructor 3#: Used for Replay mode
        public XIVisi(string fileName, AoDisplay ad)
        {
            VI = new VW.Cyvi(ad, false, false);
            VI.Init(); VI.SetInGame(true);
            VW.Eywi eywi = new VW.Eywi(fileName);
            WI = eywi;

            this.ad = ad;
            Log = new Log(); Log.Start(Uid, false, false, 0);
            CommonConstruct();
            this.auid = eywi.Uid;
            isReplay = true;
        }
        // Constructor 4#: Used for ResumeHall
        // passCode is the password for a settled room
        private XIVisi(ushort newUid, ushort oldUid, string name, Base.VW.IVI vi,
            string server, int room, string passCode, bool record, bool msglog, AoDisplay ad)
        {
            this.auid = newUid;
            this.name = name; this.VI = vi as VW.Cyvi;
            this.server = server;
            this.Room = room;
            this.port = Base.NetworkCode.HALL_PORT + room;
            this.ad = ad;

            VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, hopeTeam = 0, newUid, this);
            Log = new Log(); Log.Start(auid, record, msglog, 0);
            bywi.Log = Log; VI.Log = Log;
            WI = bywi;
            CommonConstruct();
            bywi.StartConnectResume(oldUid, passCode);
            // After that, Uid get updated.
            this.Uid = bywi.Uid;
            VI.Cout(Uid, "游戏继续啦~");
            isReplay = false;
        }
        public static XIVisi CreateInResumeHall(ushort newUid, ushort oldUid, string name,
            Base.VW.IVI vi, string server, int room, string passCode, bool record,
            bool msglog, AoDisplay ad)
        {
            return new XIVisi(newUid, oldUid, name, vi, server, room, passCode, record, msglog, ad);
        }

        public void CommonConstruct()
        {
            flashHelper = new Auxs.FlashWindowHelper(System.Windows.Application.Current);
            SelMode = Base.Rules.RuleCode.DEF_CODE;
            //bool wiInitDone = false;
            //VW.Bywi bywi = new VW.Bywi(server, port, name, avatar, totalPlayer, hopeTeam);
            //WI = bywi;
            //bywi.Init(VI, ref wiInitDone);

            //while (!wiInitDone)
            //    Thread.Sleep(100);

            //this.uid = bywi.Uid;
            //Log = new Log(); Log.Start((uid < 1000 && !record) ? uid : 0, record);
            //bywi.Log = Log;
            //ayvi.Log = Log;
            //string msg = WI.Recv(uid, 0);
            //while (msg != "C1ST,0")
            //{
            //    msg = WI.Recv(uid, 0);
            //    Thread.Sleep(100);
            //}
            //WI.Send("C1ST," + uid, uid, 0);
            //if (uid == 0)
            //    VI.Cout(uid, "咦，您是不是掉线或者连错人了:-(");
            //else if (uid < 1000)
            //    VI.Cout(uid, "游戏开始咯~");
            //else
            //    VI.Cout(uid, "您开始旁观~");
            //var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            //Console.Title = ass.Name + " v" + ass.Version + " - " + uid;

            Tuple = ad.Tuple;
            if (WI is VW.Eywi)
            {
                VW.Eywi eywi = WI as VW.Eywi;
                new TupleAdjuster().ConvertTuple(Tuple, eywi.Version);
            }
            listOfThreads = new List<Thread>();
            zd = new ZeroDisplay(this);
            A0P = new Dictionary<ushort, AoPlayer>();
            unhandledMsg = new Queue<string>();

            A0F = ad.yfPilesBar.Field;
            A0M = ad.yfBag.Me;
            A0C = ad.yfJoy.CEE;
            A0D = ad.yfDeal.Deal;
            A0O = ad.yfOrchis40.Orch;

            GameGraceEnd = false;
        }

        private static ushort RoundUid(ushort uid, int delta)
        {
            int value = uid + delta;
            while (value > 6)
                value -= 6;
            while (value <= 0)
                value += 6;
            return (ushort)value;
        }

        // Hero selection proceeding
        public void RunAsync()
        {
            //new Thread(() => Algo.SafeExecute(() =>
            //{
            //    while (true)
            //    {
            //        string acmd = VI.Request(uid);
            //        if (acmd != null)
            //            OnRequestLocalCmd(acmd);
            //        Thread.Sleep(700);
            //    }
            //}, delegate(Exception e) { Log.Logger(e.ToString()); })).Start();
            //SingleThreadMessageStart(new List<string>());
            if (auid == 0)
                return;
            new Thread(() => ZI.SafeExecute(() =>
            {
                while (true)
                {
                    string say = VI.RequestTalk(Uid);
                    if (say != null && WI != null)
                        WI.SendDirect(say, Uid);
                }
            }, delegate (Exception e) { Log.Logg(e.ToString()); })).Start();
            SingleThreadMessageStart();
            new Thread(() => ZI.SafeExecute(() =>
            {
                while (true)
                {
                    string hear = WI.Hear();
                    if (hear != null)
                        HandleYMessage(hear);
                }
            }, delegate (Exception e) { Log.Logg(e.ToString()); })).Start();
            new Thread(() => ZI.SafeExecute(() =>
            {
                while (true)
                {
                    string readLine = WI.Recv(Uid, 0);
                    //if (uid == 1) VI.Cout(uid, "★●▲■" + readLine + "★●▲■");
                    if (!string.IsNullOrEmpty(readLine))
                    {
                        bool clogfree = readLine.StartsWith("<|>");
                        if (clogfree)
                            readLine = readLine.Substring("<|>".Length);
                        lock (unhandledMsg)
                        {
                            unhandledMsg.Enqueue(readLine);
                        }
                        if (isReplay && WI is VW.Eywi)
                        {
                            VW.Eywi eywi = WI as VW.Eywi;
                            if (!clogfree)
                                Thread.Sleep(eywi.Duration);
                            while (!eywi.InProcess)
                                Thread.Sleep(80);
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
            }, delegate (Exception e) { Log.Logg(e.ToString()); })).Start();
        }
        // called at the beginning or Cin-interrupted
        //public void SingleThreadMessageStart(List<string> list)
        public void SingleThreadMessageStart()
        {
            //ParameterizedThreadStart ParStart = new ParameterizedThreadStart(SingleThreadMessage);
            //Thread myThread = new Thread(ParStart);
            Thread myThread = new Thread(() => ZI.SafeExecute(() => SingleThreadMessage(),
                        delegate (Exception e) { Log.Logg(e.ToString()); }));
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
        // List<string> list = olist as List<string>;
        // unhandledMsg = queue;
        // unhandledMsg.Clear();
        // foreach (string str in list)
        //    unhandledMsg.Enqueue(str);
        // if (unhandledMsg.Count > 0)
        //    unhandledMsg.Dequeue();
        private void SingleThreadMessage()
        {
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
                }
                else
                    Thread.Sleep(100);
            }
        }

        private bool StartCinEtc()
        {
            SingleThreadMessageStart();
            VI.OpenCinTunnel(Uid);
            flashHelper.AFlashApplicationWindow(ad);
            return true;
        }

        public void CancelThread()
        {
            if (VI != null)
                VI.AbortCinThread();
            if (WI != null)
                WI.Close();
            foreach (var td in listOfThreads)
            {
                if (td != null && td.IsAlive)
                    td.Abort();
            }
        }
        #endregion Basic Members

        #region Message Main

        private bool HMMain(object pararu)
        {
            string readLine = (string)pararu;
            //Log.Logger(readLine);
            // start a new thread to handle with the message
            int cdx = readLine.IndexOf(',');
            string cop = Algo.Substring(readLine, 0, cdx);
            if (cop == "") { } // Reserved for strange string in replay
            else if (WI is VW.Eywi && DealWithOldMessage(ref readLine))
                return false;
            else if (cop.StartsWith("E0"))
            {
                HandleE0Message(readLine);
                return false;
            }
            else if (cop.StartsWith("F0"))
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
                        {
                            ushort ut = ushort.Parse(blocks[0]);
                            VI.Cout(Uid, "{0}放弃行动.", ut);
                            ad.HideProgressBar(ut);
                            return false;
                        }
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

        private string DirectRInput(string line) { return FormattedInput(line); }
        private string FormattedInputWithCancelFlag(string line)
        {
            if (string.IsNullOrEmpty(line))
                return "";
            string output = "";
            string prevComment = "";
            List<char> keepList = new List<char>();
            flashHelper.AFlashApplicationWindow(ad);
            ad.ShowProgressBar(Uid);
            foreach (string block in Algo.Splits(line, ","))
            {
            repaint:
                bool inputValid = true;
                string arg = block;
                string cancel = "";
                bool keep = false;
                bool cancellable = false;
                string roundInput = "";
                if (block.StartsWith("/"))
                {
                    if (block.Equals("//"))
                    {
                        //var helper = new Auxs.FlashWindowHelper(System.Windows.Application.Current);
                        //// Flashes the window and taskbar 5 times and stays solid 
                        //// colored until user focuses the main window
                        //ad.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        //    helper.FlashApplicationWindow(ad);
                        //}));
                        //VI.Cin(uid, "请按任意键继续.");
                        VI.Cin01(Uid);
                        roundInput = "0";
                    }
                    else if (block.Length > 1)
                    {
                        arg = block.Substring(1);
                        cancel = "(0为取消发动)";
                        cancellable = true;
                    }
                    else
                    {
                        VI.Cin00(Uid);
                        //VI.Cin(uid, "不能指定合法目标.");
                        roundInput = "0";
                    }
                }
                if (arg.StartsWith("+"))
                {
                    keep = true;
                    arg = arg.Substring(1);
                    keepList.Add(arg[0]);
                }
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
                            var uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                //input = VI.Cin(uid, "请选择{0}名角色为{1}目标，可选{2}{3}.", argv.Length, prevComment, zd.Tux(uss), cancel);
                                input = VI.CinT(Uid, uss, r1, r2, string.Format("请选择{0}名角色为{1}目标.", r1, prevComment), cancellable, keep);
                            }
                            else
                                //input = VI.Cin(uid, "请选择{0}至{1}名角色为{2}目标，可选{3}{4}.", r1, r2, prevComment, zd.Player(uss), cancel);
                                input = VI.CinT(Uid, uss, r1, r2, string.Format("请选择{0}至{1}名角色为{2}目标.", r1, r2, prevComment), cancellable, keep);
                            //input = VI.CinT(uid, uss, r1, r2, prevComment, cancellable);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.CinT(Uid, null, r1, r2, string.Format("请选择{0}至{1}名角色为{2}目标.", r1, r2, prevComment), cancellable, keep);
                        //input = VI.Cin(uid, "请选择{0}至{1}名角色为{2}目标{3}.", r1, r2, prevComment, cancel);
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
                            //input = VI.CinT(uid, uss, r, r, prevComment, cancellable);
                            //input = VI.Cin(uid, "请选择{0}名角色为{1}目标，可选{2}{3}.", r, prevComment, zd.Player(uss), cancel);
                            input = VI.CinT(Uid, uss, r, r, string.Format("请选择{0}名角色为{1}目标.", r, prevComment), cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            //input = VI.Cin(uid, "请选择{0}名角色为{1}目标{2}.", r, prevComment, cancel);
                            //input = VI.CinT(uid, null, r, r, prevComment, cancellable);
                            input = VI.CinT(Uid, null, r, r, string.Format("请选择{0}名角色为{1}目标.", r, prevComment), cancellable, keep);
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
                        string inst;
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}张卡牌为{1}目标.", argv.Length, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}张卡牌为{2}目标.", r1, r2, prevComment);
                            int zero = uss.Count(p => p == 0);
                            uss.RemoveAll(p => p == 0);
                            input = VI.CinC(Uid, inst, r1, r2, uss, zero, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            inst = string.Format("请选择{0}至{1}张卡牌为{2}目标.", r1, r2, prevComment);
                            input = VI.CinC(Uid, inst, r1, r2, null, 0, cancellable, keep);
                        }
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r)
                                r = argv.Length;
                            string inst = string.Format("请选择{0}张卡牌为{1}目标.", r, prevComment);
                            int zero = uss.Count(p => p == 0);
                            uss.RemoveAll(p => p == 0);
                            input = VI.CinC(Uid, inst, r, r, uss, zero, cancellable, keep);
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
                        string inst;
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}张卡牌为{1}目标.", argv.Length, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}张卡牌为{2}目标.", r1, r2, prevComment);
                            int zero = uss.Count(p => p == 0);
                            uss.RemoveAll(p => p == 0);
                            input = VI.CinQ(Uid, inst, r1, r2, uss, zero, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            inst = string.Format("请选择{0}至{1}张卡牌为{2}目标.", r1, r2, prevComment);
                            input = VI.CinQ(Uid, inst, r1, r2, null, 0, cancellable, keep);
                        }
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r)
                                r = argv.Length;
                            string inst = string.Format("请选择{0}张卡牌为{1}目标.", r, prevComment);
                            int zero = uss.Count(p => p == 0);
                            uss.RemoveAll(p => p == 0);
                            input = VI.CinQ(Uid, inst, r, r, uss, zero, cancellable, keep);
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
                                input = VI.CinZ(Uid, prevComment, r1, r2, uss, cancellable, keep);
                            }
                            else
                                input = VI.CinZ(Uid, prevComment, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.CinZ(Uid, prevComment, r1, r2, null, cancellable, keep);
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.CinZ(Uid, prevComment, r, r, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.CinZ(Uid, prevComment, r, r, null, cancellable, keep);
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
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            string inst;
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}张怪物牌为{1}目标。", r1, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}张怪物牌为{2}目标。", r1, r2, prevComment);
                            input = VI.CinM(Uid, inst, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            string inst = string.Format("请选择{0}至{1}张怪物牌为{2}目标。", r1, r2, prevComment);
                            input = VI.CinM(Uid, inst, r1, r2, null, cancellable, keep);
                        }
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r)
                                r = argv.Length;
                            string inst = string.Format("请选择{0}张怪物牌为{1}目标。", r, prevComment);
                            input = VI.CinM(Uid, inst, r, r, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            string inst = string.Format("请选择{0}张怪物牌为{1}目标。", r, prevComment);
                            input = VI.CinM(Uid, inst, r, r, null, cancellable, keep);
                        }
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
                    VI.ShowTip(prevComment);
                    int r1, r2;
                    if (idx >= 1)
                    {
                        r1 = int.Parse(Substring(arg, 1, idx));
                        r2 = int.Parse(Substring(arg, idx + 1, jdx));
                    }
                    else
                        r1 = r2 = int.Parse(Substring(arg, 1, jdx));

                    if (jdx >= 0)
                    {
                        string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                        List<string> uss = argv.Select(p => p.Substring("I".Length)).ToList();
                        input = VI.CinI(Uid, prevComment, r1, r2, argv, cancellable, keep);
                        inputValid &= input.Split(',').Intersect(uss).Any();
                    }
                    else { input = ""; inputValid = false; }
                    inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'Y') // Yes or not selection
                {
                    int posCan = (int)(arg[1] - '0');
                    string[] coms;
                    if (prevComment == "")
                        coms = Enumerable.Repeat("", posCan + 1).ToArray();
                    else
                    {
                        string[] prevs = Algo.Splits(prevComment, "##");
                        if (posCan + 1 > prevs.Length)
                        {
                            IEnumerable<string> v1 = prevs.ToList();
                            IEnumerable<string> v2 = Enumerable.Repeat("", posCan + 1 - prevs.Length);
                            coms = v1.Concat(v2).ToArray();
                        }
                        else
                            coms = prevs.Take(posCan + 1).ToArray();
                    }
                    roundInput = VI.CinY(Uid, posCan, cancellable, coms);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'X') // Arrangement
                {
                    // format X(p1p3p5)
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    int rest = int.Parse(Algo.Substring(arg, 1, jdx));
                    string[] argv = Algo.Substring(arg, jdx + "(p".Length, kdx).Split('p');
                    //List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                    //roundInput = VI.Cin(uid, "请重排以下{0}怪物{1}{2}.", prevComment, zd.Monster(uss), cancel);
                    //List<string> ussnm = argv.Select(p => "M" + p).ToList();
                    List<string> ussnm = argv.Select(p => p.Substring(1)).ToList();
                    roundInput = VI.CinX(Uid, rest, rest, argv.ToList(), cancellable, keep);
                    inputValid &= roundInput.Split(',').Intersect(ussnm).Any();
                    prevComment = ""; cancel = "";
                }
                //else if (arg[0] == 'W') // Arrangement
                //{
                //    int jdx = arg.IndexOf('(');
                //    int kdx = arg.IndexOf(')');
                //    string[] argv = Algo.Substring(arg, jdx + "(p".Length, kdx).Split('p');
                //    var uss = argv.Select(p => ushort.Parse(p));
                //    roundInput = VI.Cin(uid, "请重排以下{0}卡牌{1}{2}.", prevComment, zd.Tux(uss), cancel);
                //    inputValid &= roundInput.Split(',').Intersect(argv).Any();
                //    prevComment = ""; cancel = "";
                //}
                else if (arg[0] == 'S')
                {
                    roundInput = VI.CinY(Uid, 2, cancellable,
                        new string[] { "请选择将此效果作用于哪方？", "我方", "对方" });
                    if (roundInput == "1")
                        roundInput = A0P[Uid].Team.ToString();
                    else if (roundInput == "2")
                        roundInput = A0P[Uid].OppTeam.ToString();
                    //roundInput = VI.Cin(uid, "请选择{0}一方{1}.", prevComment, cancel);
                    inputValid &= (roundInput == "1" || roundInput == "2");
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'D')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string coma = string.Format("请输入{0}数值.", prevComment);
                    int ipValue = 0;
                    int r1, r2;
                    if (idx >= 1)
                    {
                        r1 = int.Parse(Substring(arg, 1, idx));
                        r2 = int.Parse(Substring(arg, idx + 1, jdx));
                    }
                    else
                        r1 = r2 = int.Parse(Substring(arg, 1, jdx));
                    string eachInput = VI.CinD(Uid, r1, r2, coma, cancellable);
                    while (eachInput == "6+")
                    {
                        inputValid &= (r2 > 6);
                        if (!inputValid)
                            break;
                        ipValue += 6;
                        coma = string.Format("请继续输入{0}数值，已累加{1}.", prevComment, ipValue);
                        eachInput = VI.CinD(Uid, (r1 - ipValue < 1 ? 1 : r1 - ipValue), r2 - ipValue, coma, cancellable);
                    }
                    int ipValueThis = int.Parse(eachInput);
                    if (ipValueThis == 0) ipValue = 0;
                    else ipValue += ipValueThis;
                    roundInput = ipValue.ToString();
                    inputValid &= (ipValue >= r1 && ipValue <= r2);
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
                        string inst;
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}种卡牌为{1}目标.", argv.Length, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}种卡牌为{2}目标.", r1, r2, prevComment);
                            input = VI.CinG(Uid, inst, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            inst = string.Format("请选择{0}至{1}种卡牌为{2}目标.", r1, r2, prevComment);
                            input = VI.CinG(Uid, inst, r1, r2, null, cancellable, keep);
                        }
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
                            string inst = string.Format("请选择{0}张卡牌为{1}目标.", r, prevComment);
                            input = VI.CinG(Uid, inst, r, r, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}张卡牌为{1}目标{2}.", r, prevComment, cancel);
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
                    // TODO: handle with AND of multiple condition
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');

                        List<string> judgeArgv = argv.Select(p => p.StartsWith("T") ? p.Substring("T".Length) : p).ToList();
                        if (argv.Length < r1)
                        {
                            r1 = r2 = argv.Length;
                            string hint = string.Format("请以{0}人{1}.", argv.Length, prevComment);
                            input = VI.CinTP(Uid, argv, hint, cancellable, false);
                        }
                        else
                        {
                            string hint = string.Format("请以{0}至{1}人{2}.", r1, r2, prevComment);
                            input = VI.CinTP(Uid, argv, hint, cancellable, false);
                        }
                        inputValid &= input.Split(',').Intersect(judgeArgv).Any();
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                        List<string> judgeArgv = argv.Select(p => p.StartsWith("T") ? p.Substring("T".Length) : p).ToList();
                        if (argv.Length < r)
                            r = argv.Length;
                        string hint = string.Format("请以{0}人{1}.", r, prevComment);
                        input = VI.CinTP(Uid, argv, hint, cancellable, false);
                        inputValid &= input.Split(',').Intersect(judgeArgv).Any();
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
                        string inst;
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}枚标记为{1}目标.", argv.Length, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}枚标记为{2}目标.", r1, r2, prevComment);
                            input = VI.CinF(Uid, inst, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            inst = string.Format("请选择{0}至{1}枚标记为{2}目标.", r1, r2, prevComment);
                            input = VI.CinF(Uid, inst, r1, r2, null, cancellable, keep);
                        }
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
                            string inst = string.Format("请选择{0}枚标记为{1}目标.", r, prevComment);
                            input = VI.CinF(Uid, inst, r, r, uss, cancellable, keep);
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
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            string inst;
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}张事件牌为{1}目标。", r1, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}张事件牌为{2}目标。", r1, r2, prevComment);
                            input = VI.CinE(Uid, inst, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            string inst = string.Format("请选择{0}至{1}张事件牌为{2}目标。", r1, r2, prevComment);
                            input = VI.CinE(Uid, inst, r1, r2, null, cancellable, keep);
                        }
                        inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                            if (argv.Length < r)
                                r = argv.Length;
                            string inst = string.Format("请选择{0}张事件牌为{1}目标。", r, prevComment);
                            input = VI.CinE(Uid, inst, r, r, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            string inst = string.Format("请选择{0}张事件牌为{1}目标。", r, prevComment);
                            input = VI.CinE(Uid, inst, r, r, null, cancellable, keep);
                        }
                        inputValid &= (CountItemFromComma(input) == r);
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
                        string inst;
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}名角色为{1}目标.", argv.Length, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}名角色为{2}目标.", r1, r2, prevComment);
                            input = VI.CinH(Uid, inst, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            inst = string.Format("请选择{0}至{1}名角色为{2}目标.", r1, r2, prevComment);
                            input = VI.CinH(Uid, inst, r1, r2, null, cancellable, keep);
                        }
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
                            string inst = string.Format("请选择{0}名角色为{1}目标.", r, prevComment);
                            input = VI.CinH(Uid, inst, r, r, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}名角色为{1}目标{2}.", r, prevComment, cancel);
                        inputValid &= CountItemFromComma(input) == r;
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                // Pending: cannot cancel case, not sure where it would be called or not
                //else if (arg[0] == 'H')
                //{
                //    int idx = arg.IndexOf('~');
                //    int jdx = arg.IndexOf('(');
                //    int kdx = arg.IndexOf(')');
                //    string input;
                //    VI.ShowTip(prevComment);
                //    int r1, r2;
                //    if (idx >= 1)
                //    {
                //        r1 = int.Parse(Substring(arg, 1, idx));
                //        r2 = int.Parse(Substring(arg, idx + 1, jdx));
                //    }
                //    else
                //        r1 = r2 = int.Parse(Substring(arg, 1, jdx));

                //    if (jdx >= 0)
                //    {
                //        string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                //        List<ushort> uss = argv.Select(p => ushort.Parse(p)).ToList();
                //        input = VI.CinH(Uid, prevComment, r1, r2, uss, false, false);
                //        inputValid &= input.Split(',').Intersect(argv).Any();
                //    }
                //    else { input = ""; inputValid = false; }
                //    //else
                //    //    input = VI.Cin(uid, "请选择{0}至{1}张角色牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                //    inputValid &= !(CountItemFromComma(input) < r1 || CountItemFromComma(input) > r2);
                //    prevComment = ""; cancel = "";
                //    roundInput = input;
                //}
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
                        string inst;
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            ushort[] uss = argv.Select(p => ushort.Parse(p)).ToArray();
                            if (argv.Length < r1)
                            {
                                r1 = r2 = argv.Length;
                                inst = string.Format("请选择{0}种属性为{1}目标.", argv.Length, prevComment);
                            }
                            else
                                inst = string.Format("请选择{0}至{1}种属性为{2}目标.", r1, r2, prevComment);
                            input = VI.CinV(Uid, inst, r1, r2, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                        {
                            inst = string.Format("请选择{0}至{1}种属性为{2}目标.", r1, r2, prevComment);
                            input = VI.CinV(Uid, inst, r1, r2, null, cancellable, keep);
                        }
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
                            string inst = string.Format("请选择{0}种属性为{1}目标.", r, prevComment);
                            input = VI.CinV(Uid, inst, r, r, uss, cancellable, keep);
                            inputValid &= input.Split(',').Intersect(argv).Any();
                        }
                        else
                            input = VI.Cin(Uid, "请选择{0}种属性为{1}目标{2}.", r, prevComment, cancel);
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

                if (!keep)
                {
                    foreach (char ch in keepList)
                    {
                        if (ch == 'T')
                            VI.OCinT();
                        else if (ch == 'Z')
                            VI.OCinZ();
                        else if (ch == 'C')
                            VI.OCinC();
                        else if (ch == 'M')
                            VI.OCinM();
                        else if (ch == 'I')
                            VI.OCinI();
                        else if (ch == 'Q')
                            VI.OCinQ();
                        else if (ch == 'E')
                            VI.OCinE();
                        else if (ch == 'V')
                            VI.OCinV();
                    }
                    keepList.Clear();
                }
                if (roundInput == "0" && block.StartsWith("/"))
                {
                    output += ",0"; return "/" + output.Substring(1);
                }
                else if (inputValid || roundInput == VI.CinSentinel || roundInput == "0")
                    output += "," + roundInput;
                else
                {
                    //System.Windows.MessageBox.Show("input=" + roundInput);
                    VI.Cout(Uid, "抱歉，您的输入有误。");
                    goto repaint;
                }
            }
            if (keepList.Count > 0)
            {
                foreach (char ch in keepList)
                {
                    if (ch == 'T')
                        VI.OCinT();
                    else if (ch == 'Z')
                        VI.OCinZ();
                    else if (ch == 'C')
                        VI.OCinC();
                    else if (ch == 'M')
                        VI.OCinM();
                    else if (ch == 'I')
                        VI.OCinI();
                    else if (ch == 'Q')
                        VI.OCinQ();
                }
            }
            ad.HideProgressBar(Uid);
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
                    for (int idx = 1; idx < args.Length;)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        int type = int.Parse(args[idx + 1]);
                        if (type == 0)
                        {
                            int n = int.Parse(args[idx + 2]);
                            List<ushort> cards = Algo.TakeRange(args, idx + 3, idx + 3 + n)
                                .Select(p => ushort.Parse(p)).ToList();
                            A0P[who].TuxCount += n;
                            if (who == Uid)
                                A0M.insTux(cards);
                            else if (Uid >= 1000 && who == WATCHER_1ST_PERSPECT)
                                A0M.insTux(Enumerable.Repeat((ushort)0, n).ToList());
                            idx += (n + 3);
                        }
                        else if (type == 1)
                        {
                            int n = int.Parse(args[idx + 2]);
                            A0P[who].TuxCount += n;
                            if (Uid >= 1000 && who == WATCHER_1ST_PERSPECT)
                                A0M.insTux(Enumerable.Repeat((ushort)0, n).ToList());
                            idx += 3;
                        }
                    }
                    break;
                case "E0OT":
                    for (int idx = 1; idx < args.Length;)
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
                                if (A0P[who].Weapon == ut)
                                    A0P[who].Weapon = 0;
                                else if (A0P[who].Armor == ut)
                                    A0P[who].Armor = 0;
                                else if (A0P[who].IsExCardsContain(ut))
                                    A0P[who].DelExCards(ut);
                                else if (A0P[who].Fakeq.ContainsKey(ut))
                                    A0P[who].DelFakeq(ut);
                                else if (A0P[who].ExEquip == ut)
                                    A0P[who].ExEquip = 0;
                                else if (A0P[who].Trove == ut)
                                    A0P[who].Trove = 0;
                                else
                                    ++tuxCount;
                            }
                            if (who == Uid)
                            {
                                A0P[who].TuxCount -= tuxCount;
                                //A0M.Tux.RemoveAll(p => cards.Contains(p));
                                A0M.delTux(cards);
                            }
                            else if (Uid >= 1000 && who == WATCHER_1ST_PERSPECT)
                                for (int i = 0; i < tuxCount; ++i)
                                    A0M.delTux(0);
                            idx += (n + 3);
                        }
                        else if (type == 1)
                        {
                            int n = int.Parse(args[idx + 2]);
                            A0P[who].TuxCount -= n;
                            if (Uid >= 1000 && who == WATCHER_1ST_PERSPECT)
                                for (int i = 0; i < n; ++i)
                                    A0M.delTux(0);
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
                            A0F.TuxCount -= n;
                        else if (utype == 1)
                            A0F.MonCount -= n;
                        else if (utype == 2)
                            A0F.EveCount -= n;
                    }
                    break;
                case "E0ON":
                    for (int idx = 1; idx < args.Length;)
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
                                A0F.TuxDises += n;
                                if (fromZone == 0)
                                    cdInfos = zd.Tux(cds);
                            }
                            else if (cardType == "M")
                            {
                                A0F.MonDises += n;
                                if (fromZone == 0)
                                    cdInfos = zd.Monster(cds);
                            }
                            else if (cardType == "E")
                            {
                                A0F.EveDises += n;
                                if (fromZone == 0)
                                    cdInfos = zd.Eve(cds);
                            }
                            if (cdInfos != null)
                            {
                                VI.Cout(Uid, "{0}被弃置进入弃牌堆.", cdInfos);
                                List<string> cedcards = cds.Select(
                                    p => cardType + p).ToList();
                                A0O.FlyingGet(cedcards, 0, 0);
                            }
                        }
                        idx += (3 + n);
                    }
                    break;
                case "E0CN":
                    {
                        ushort utype = ushort.Parse(args[1]);
                        int n = args.Length - 2;
                        ushort[] cards = Algo.TakeRange(args, 2, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        if (utype == 0)
                            A0F.TuxDises -= n;
                        else if (utype == 1)
                            A0F.MonDises -= n;
                        else if (utype == 2)
                            A0F.EveDises -= n;
                    }
                    break;
                case "E0RN":
                    for (int i = 1; i < args.Length;)
                    {
                        ushort from = ushort.Parse(args[i]);
                        ushort to = ushort.Parse(args[i + 1]);
                        int n = int.Parse(args[i + 2]);
                        List<ushort> tuxes = Algo.TakeRange(args, i + 3, i + 3 + n)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (to == 0)
                        {
                            List<string> cedcards = tuxes.Select(p => "C" + p).ToList();
                            A0O.FlyingGet(cedcards, from, to);
                            VI.Cout(Uid, "{0}展示了卡牌{1}.", zd.Player(from), zd.Tux(tuxes));
                        }
                        else if (from == 0)
                        {
                            List<string> cedcards = tuxes.Select(p => "C" + p).ToList();
                            A0O.FlyingGet(cedcards, from, to);
                            VI.Cout(Uid, "{0}收回了卡牌{1}.", zd.Player(to), zd.Tux(tuxes));
                        }
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
                                List<string> cedcards = cards.Select(p => "C" + p).ToList();
                                A0O.FlyingGet(cedcards, from, to);
                            }
                            else if (utype == 1)
                            {
                                int n = int.Parse(args[5]);
                                VI.Cout(Uid, "{0}从{1}获得了{2}张牌.", zd.Player(to), zd.Player(from), n);
                                //Z0D[from].TuxCount -= n;
                                //Z0D[to].TuxCount += n;
                                List<string> cedcards = Enumerable.Repeat("C0", n).ToList();
                                A0O.FlyingGet(cedcards, from, to);
                            }
                        }
                        else if (type == 2)
                        {
                            List<ushort> cards = Algo.TakeRange(args, 3, args.Length)
                                .Select(p => ushort.Parse(p)).ToList();
                            VI.Cout(Uid, "{0}摸取了{1}.", zd.Player(to), zd.Tux(cards));
                            List<string> cedcards = cards.Select(p => "C" + p).ToList();
                            A0O.FlyingGet(cedcards, 0, to);
                            //Z0P.TuxCount -= args.Length - 3;
                            //Z0D[to].TuxCount += cards.Count;
                            //if (uid == to)
                            //    Z0M.Tux.AddRange(cards);
                        }
                        else if (type == 3)
                        {
                            int n = int.Parse(args[3]);
                            VI.Cout(Uid, "{0}摸取了{1}张牌.", zd.Player(to), n);
                            List<string> cedcards = Enumerable.Repeat("C0", n).ToList();
                            A0O.FlyingGet(cedcards, 0, to);
                            //Z0D[to].TuxCount += n;
                        }
                        else if (type == 4)
                        {
                            for (int idx = 3; idx < args.Length;)
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
                                A0O.FlyingGet(tuxes.Select(p => "C" + p).ToList(), fromZone, to);
                                idx += (n + 2);
                            }
                        }
                        break;
                    }
                case "E0QZ":
                    {
                        ushort from = ushort.Parse(args[1]);
                        var cards = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToList();
                        VI.Cout(Uid, "{0}弃置卡牌{1}.", zd.Player(from), zd.Tux(cards));
                        List<string> cedcards = cards.Select(p => "C" + p).ToList();
                        A0O.FlyingGet(cedcards, from, 0);
                    }
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
                            ushort prop = ushort.Parse(args[i + 2]);
                            ushort n = ushort.Parse(args[i + 3]);
                            ushort now = ushort.Parse(args[i + 4]);
                            string msgBase = "{0}HP" + ("E0IH" == args[0] ? "+" : "-") + "{1}({2}),当前HP={3}.";
                            if (isLove == 1)
                                hpIssues.Add(string.Format(msgBase, zd.Player(from), n, "倾慕", now));
                            else
                                hpIssues.Add(string.Format(msgBase, zd.Player(from), n, zd.PropName(prop), now));
                            A0P[from].HP = now;
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
                        A0P[who].IsLoved = true;
                    }
                    break;
                case "E0ZW":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; ++i)
                        {
                            ushort py = ushort.Parse(args[i]);
                            result += "," + zd.Player(py);
                            A0P[py].IsAlive = false;
                            A0P[py].HP = 0;
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
                        if (A0P.ContainsKey(who))
                        {
                            A0P[who].SelectHero = hero;
                            A0P[who].IsAlive = true;
                            if (args[1] == "0")
                            {
                                A0P[who].ParseFromHeroLib();
                                VI.Cout(Uid, "{0}#玩家转化为{1}角色.", who, zd.Hero(hero));
                            }
                            else if (args[1] == "1")
                                VI.Cout(Uid, "{0}#玩家变身为{1}角色.", who, zd.Hero(hero));
                            else if (args[1] == "2")
                            {
                                A0P[who].ClearStatus();
                                VI.Cout(Uid, "{1}加入到{0}#位置.", who, zd.Hero(hero));
                            }
                            A0P[who].UpdateExCardSpTitle();
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
                            A0P[who].Immobilized = true;
                        }
                        else if (type == 1)
                        {
                            VI.Cout(Uid, "{0}解除横置.", zd.Player(who));
                            A0P[who].Immobilized = false;
                        }
                        break;
                    }
                case "E0FU":
                    if (args[1].Equals("0"))
                    {
                        string cardType = args[2];
                        var ravs = Algo.TakeRange(args, 3, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        VI.Watch(Uid, ravs.Select(p => cardType + p), "E0FU");
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
                        string cardType = args[2];
                        ushort n = ushort.Parse(args[3]);
                        VI.Watch(Uid, Enumerable.Repeat(cardType + "0", n), "E0FU");
                    }
                    else if (args[1].Equals("2"))
                    {
                        ushort who = ushort.Parse(args[2]);
                        string cardType = args[3];
                        List<ushort> invs = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        A0O.FlyingGet(invs.Select(p => cardType + p).ToList(), who, who, true);
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
                        VI.OWatch(Uid, "E0FU");
                    else if (args[1].Equals("4"))
                    {
                        if (WI is VW.Eywi)
                        {
                            string cardType = args[2];
                            var ravs = Algo.TakeRange(args, 3, args.Length)
                                .Select(p => ushort.Parse(p)).ToList();
                            VI.Watch(Uid, ravs.Select(p => cardType + p), "E0FU");
                        }
                    }
                    else if (args[1].Equals("5"))
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort[] invs = Algo.TakeRange(args, 3, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        A0O.FlyingGet(invs.Select(p => "G" + p).ToList(), who, who, true);
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
                        ushort pst = ushort.Parse(args[3]);
                        List<ushort> ravs = Algo.TakeRange(args, 5, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (pst == 0)
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(args[4]));
                        else
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}，为{3}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(args[4]), zd.Player(pst));
                        //Z0D[ust].TuxCount -= ravs.Count;
                        //if (ust == uid)
                        //    Z0M.Tux.RemoveAll(p => ravs.Contains(p));
                        //if (!ravs.Contains(0))
                        //{
                        //    List<string> cedcards = ravs.Select(p => "C" + p).ToList();
                        //    A0O.FlyingGet(cedcards, ust, 0);
                        //}
                        break;
                    }
                case "E0CD": // use card and want a target
                    {
                        // E0CD,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        List<ushort> argst = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        string sarg = argst.Count > 0 ? ("(" + string.Join(",", argst) + ")") : "";
                        VI.Cout(Uid, "{0}{1}预定作用于{2}.", zd.Tux(args[3]), sarg, zd.Player(ust));
                        break;
                    }
                case "E0CE": // use card and take action
                    {
                        // E0CE,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        List<ushort> argst = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        string sarg = argst.Count > 0 ? ("(" + string.Join(",", argst) + ")") : "";
                        VI.Cout(Uid, "{0}{1}对{2}生效.", zd.Tux(args[3]), sarg, zd.Player(ust));
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
                            VI.Cout(Uid, "{0}的{1}({2})被抵消.", zd.Player(ust), zd.Tux(cardName), string.Join(",", ravs));
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
                            //VI.Cout(uid, "您观看{0}结果为{1}.", dd[args[3]], df[args[3]](ravs));
                            VI.Watch(Uid, ravs.Select(p => "M" + p), "E0XZ");
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
                                A0P[me].Weapon = card;
                            else if (where == 2)
                                A0P[me].Armor = card;
                            else if (where == 3)
                                A0P[me].InsExCards(card);
                            else if (where == 4)
                            {
                                string asCode = args[5] == "0" ? Tuple.TL.DecodeTux(card).Code : args[5];
                                A0P[me].InsFakeq(card, asCode);
                            }
                            else if (where == 5)
                                A0P[me].ExEquip = card;
                            else if (where == 6)
                                A0P[me].Trove = card;
                            VI.Cout(Uid, "{0}装备了{1}到{2}.", zd.Player(me), zd.Tux(card), words[where - 1]);
                            A0O.FlyingGet("C" + card, me, me);
                        }
                        else if (type == 1)
                        {
                            ushort where = ushort.Parse(args[4]);
                            ushort from = ushort.Parse(args[3]);
                            ushort card = ushort.Parse(args[5]);
                            if (where == 1)
                                A0P[me].Weapon = card;
                            else if (where == 2)
                                A0P[me].Armor = card;
                            else if (where == 3)
                                A0P[me].InsExCards(card);
                            else if (where == 4)
                            {
                                string asCode = args[6] == "0" ? Tuple.TL.DecodeTux(card).Code : args[6];
                                A0P[me].InsFakeq(card, asCode);
                            }
                            else if (where == 5)
                                A0P[me].ExEquip = card;
                            else if (where == 6)
                                A0P[me].Trove = card;
                            if (from != 0)
                            {
                                VI.Cout(Uid, "{0}的装备{2}进入{1}的{3}.",
                                    zd.Player(from), zd.Player(me), zd.Tux(card), words[where - 1]);
                                A0O.FlyingGet("C" + card, from, me);
                            }
                            else
                            {
                                VI.Cout(Uid, "{0}装备了{1}到{2}.",
                                    zd.Player(me), zd.Tux(card), words[where - 1]);
                                A0O.FlyingGet("C" + card, me, me);
                            }
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
                            string[] wherestr = new string[] { "", "武器区", "防具区", "特殊区",
                                "佩戴区", "额外装备区", "秘宝区" };
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
                //        List<string> cedcards = new List<string>();
                //        cedcards.Add("C" + card);
                //        if (type == 0)
                //            A0O.FlyingGet(cedcards, who, 0);
                //        else if (type == 1)
                //            A0O.FlyingGet(cedcards, who, who);
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
                                A0P[who].STR = tp;
                                A0P[who].STRa = bs;
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
                                A0P[who].STR = tp;
                                A0P[who].STRa = bs;
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
                                A0P[who].DEX = tp;
                                A0P[who].DEXa = bs;
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
                                A0P[who].DEX = tp;
                                A0P[who].DEXa = bs;
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
                        A0P[who].STR = A0P[who].STRa = str;
                        A0P[who].DEX = A0P[who].DEXa = dex;
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

                        if (s != 0 && s < 1000)
                        {
                            if (sy)
                                A0P[s].SetAsSpSucc();
                            else
                                A0P[s].SetAsSpFail();
                        }
                        if (h != 0 && h < 1000)
                        {
                            if (hy)
                                A0P[h].SetAsSpSucc();
                            else
                                A0P[h].SetAsSpFail();
                        }
                    }
                    else if (args[1] == "1")
                    {
                        ushort rside = ushort.Parse(args[2]);
                        int rpool = ushort.Parse(args[3]);
                        ushort oside = ushort.Parse(args[4]);
                        int opool = ushort.Parse(args[5]);
                        VI.Cout(Uid, "{0}方战力={1}，{2}方战力={3}.", rside, rpool, oside, opool);
                        if (rside == 1) { A0F.PoolAka = rpool; A0F.PoolAo = opool; }
                        else { A0F.PoolAka = opool; A0F.PoolAo = rpool; }
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
                            VI.Cout(Uid, "{0}从{1}处获得了宠物{2}.", zd.Player(who), zd.Player(from), zd.Monster(pet));
                        if (!A0P[who].Pets.Contains(pet))
                            A0P[who].InsPet(pet);
                        List<string> cedcards = new List<string>();
                        cedcards.Add("M" + pet);
                        A0O.FlyingGet(cedcards, from, who);
                    }
                    break;
                case "E0HL":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort pet = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}失去了宠物{1}.", zd.Player(who), zd.Monster(pet));
                        Monster monster = Tuple.ML.Decode(pet);
                        int five = monster.Element.Elem2Index();
                        A0P[who].DelPet(pet);
                        List<string> cedcards = new List<string>();
                        cedcards.Add("M" + pet);
                        A0O.FlyingGet(cedcards, who, 0);
                    }
                    break;
                case "E0HU":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<string> cedcards = new List<string>();
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort mon = ushort.Parse(args[i]);
                            cedcards.Add("M" + mon);
                        }
                        A0O.FlyingGet(cedcards, who, who);
                    }
                    break;
                case "E0HZ":
                    if (args[1] == "0")
                    {
                        ushort who = ushort.Parse(args[2]);
                        A0F.Monster2 = 0;
                        VI.Cout(Uid, "{0}放弃触发混战.", zd.Player(who));
                    }
                    else if (args[1] == "1" || args[1] == "2")
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        A0F.Monster2 = mon;
                        VI.Cout(Uid, "{0}触发混战的结果为【{1}】.", zd.Player(who), zd.Monster(mon));
                    }
                    else if (args[1] == "3")
                    {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        A0F.Monster2 = mon;
                        VI.Cout(Uid, "触发混战【{0}】的钦慕效果生效.", zd.Monster(mon));
                    }
                    break;
                case "E0TT":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort point = ushort.Parse(args[2]);
                        VI.Cout(Uid, "{0}掷骰的结果为{1}.", zd.Player(who), point);
                        A0O.FlyingGet("D" + point, who, 0);
                    }
                    break;
                case "E0T7":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort orgPt = ushort.Parse(args[2]);
                        ushort newPt = ushort.Parse(args[3]);
                        VI.Cout(Uid, "{0}更改掷骰的结果为{1}.", zd.Player(who), newPt);
                        A0O.FlyingGet("D" + newPt, who, 0);
                    }
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
                                zd.HeroTokenAlias(A0P[who].SelectHero), delta, cur);
                            A0P[who].Token = cur;
                        }
                        else if (type == 1)
                        {
                            int count1 = int.Parse(args[3]);
                            List<string> heros1 = Algo.TakeRange(args, 4, 4 + count1).ToList();
                            int count2 = int.Parse(args[4 + count1]);
                            List<string> heros2 = Algo.TakeRange(args, 5 + count1,
                                5 + count1 + count2).ToList();
                            VI.Cout(Uid, "{0}的{1}增加{2}，现在为{3}.", zd.Player(who),
                                zd.HeroPeopleAlias(A0P[who].SelectHero), zd.MixedCards(heros1), zd.MixedCards(heros2));
                            A0P[who].InsExSpCard(heros1);
                            //A0O.FlyingGet(heros1, 0, who);
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
                                    zd.HeroPlayerTarAlias(A0P[who].SelectHero), zd.Player(tars1));
                            else
                                VI.Cout(Uid, "{0}的{1}目标增加{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroPlayerTarAlias(A0P[who].SelectHero), zd.Player(tars1), zd.Player(tars2));
                            A0P[who].InsPlayerTar(tars1);
                        }
                        else if (type == 3)
                        {
                            VI.Cout(Uid, "{0}已发动{1}.", zd.Player(who),
                                zd.HeroAwakeAlias(A0P[who].SelectHero));
                            A0P[who].Awake = true;
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
                                    zd.HeroFolderAlias(A0P[who].SelectHero), zd.Tux(folder1), zd.Tux(folder2));
                                A0P[who].InsMyFolder(folder1);
                            }
                            else
                            {
                                int count1 = int.Parse(args[4]);
                                int count2 = int.Parse(args[5]);
                                VI.Cout(Uid, "{0}的{1}数增加{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroFolderAlias(A0P[who].SelectHero), count1, count2);
                                A0P[who].FolderCount += count1;
                            }
                        }
                        else
                            break;
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
                                zd.HeroTokenAlias(A0P[who].SelectHero), delta, cur);
                            A0P[who].Token = cur;
                        }
                        else if (type == 1)
                        {
                            int count1 = int.Parse(args[3]);
                            List<string> heros1 = Algo.TakeRange(args, 4, 4 + count1).ToList();
                            int count2 = int.Parse(args[4 + count1]);
                            List<string> heros2 = Algo.TakeRange(args, 5 + count1,
                                5 + count1 + count2).ToList();
                            VI.Cout(Uid, "{0}的{1}减少{2}，现在为{3}.", zd.Player(who),
                                zd.HeroPeopleAlias(A0P[who].SelectHero), zd.MixedCards(heros1), zd.MixedCards(heros2));
                            A0P[who].DelExSpCard(heros1);
                            //A0O.FlyingGet(heros1, who, 0);
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
                                    zd.HeroPlayerTarAlias(A0P[who].SelectHero));
                            else
                                VI.Cout(Uid, "{0}的{1}目标减少{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroPlayerTarAlias(A0P[who].SelectHero), zd.Player(tars1), zd.Player(tars2));
                            A0P[who].DelPlayerTar(tars1);
                        }
                        else if (type == 3)
                        {
                            VI.Cout(Uid, "{0}已取消{1}.", zd.Player(who),
                                zd.HeroAwakeAlias(A0P[who].SelectHero));
                            A0P[who].Awake = false;
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
                                        zd.HeroFolderAlias(A0P[who].SelectHero), zd.Tux(folder1));
                                else
                                    VI.Cout(Uid, "{0}的{1}减少{2}，现在为{3}.", zd.Player(who),
                                        zd.HeroFolderAlias(A0P[who].SelectHero), zd.Tux(folder1), zd.Tux(folder2));
                                A0P[who].DelMyFolder(folder1);
                            }
                            else
                            {
                                int count1 = int.Parse(args[4]);
                                int count2 = int.Parse(args[5]);
                                VI.Cout(Uid, "{0}的{1}数减少{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroFolderAlias(A0P[who].SelectHero), count1, count2);
                                A0P[who].FolderCount -= count1;
                            }
                        }
                        else
                            break;
                    }
                    break;
                case "E0WK":
                    for (int idx = 1; idx < args.Length; idx += 2)
                    {
                        int team = int.Parse(args[idx]);
                        int value = int.Parse(args[idx + 1]);
                        if (team == 1)
                            A0F.ScoreAka = value;
                        else if (team == 2)
                            A0F.ScoreAo = value;
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
                        A0P[who].HP = hp;
                        A0P[who].HPa = hpb;
                        A0P[who].STR = A0P[who].STRa = str;
                        A0P[who].DEX = A0P[who].DEXa = dex;
                    }
                    break;
                case "E0IL":
                    for (int idx = 1; idx < args.Length; idx += 2)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        ushort npc = ushort.Parse(args[idx + 1]);
                        A0P[who].InsEscue(npc);
                        VI.Cout(Uid, "{0}获得助战NPC{1}.", zd.Player(who), zd.Monster(npc));
                        A0O.FlyingGet("M" + npc, 0, who);
                    }
                    break;
                case "E0OL":
                    for (int idx = 1; idx < args.Length; idx += 2)
                    {
                        ushort who = ushort.Parse(args[idx]);
                        ushort npc = ushort.Parse(args[idx + 1]);
                        A0P[who].DelEscue(npc);
                        VI.Cout(Uid, "{0}失去助战NPC{1}.", zd.Player(who), zd.Monster(npc));
                        A0O.FlyingGet("M" + npc, who, 0);
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
                    if (args[1] == "0")
                        foreach (ushort ut in A0P.Keys)
                        {
                            if (ut < 1000)
                                ad.HideProgressBar(ut);
                        }
                    else
                    {
                        List<ushort> uts = Algo.TakeRange(args, 1, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        foreach (ushort ut in uts)
                            ad.HideProgressBar(ut);
                    }
                    break;
                case "E0IE":
                case "E0OE":
                    if (args[1] == "0")
                    {
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort who = ushort.Parse(args[i]);
                            if (args[0] == "E0IE")
                                A0P[who].PetDisabled = false;
                            else
                                A0P[who].PetDisabled = true;
                        }
                    }
                    break;
                case "E0YM":
                    if (args[1] == "0")
                    {
                        ushort mons = ushort.Parse(args[2]);
                        if (mons != 0)
                            VI.Cout(Uid, "本场战斗怪物为【{0}】.", zd.Monster(mons));
                        A0F.Mon1From = ushort.Parse(args[3]);
                        A0F.Monster1 = mons;
                    }
                    else if (args[1] == "1")
                    {
                        ushort mons = ushort.Parse(args[2]);
                        if (mons != 0)
                            VI.Cout(Uid, "混战结果为【{0}】.", zd.Monster(mons));
                        A0F.Mon2From = ushort.Parse(args[3]);
                        A0F.Monster2 = mons;
                    }
                    else if (args[1] == "2")
                    {
                        ushort eve = ushort.Parse(args[2]);
                        if (eve != 0)
                            VI.Cout(Uid, "执行事件牌为【{0}】.", zd.Eve(eve));
                        A0F.Eve1From = ushort.Parse(args[3]);
                        A0F.Eve1 = eve;
                    }
                    else if (args[1] == "3")
                    {
                        ushort npc = ushort.Parse(args[2]);
                        if (npc != 0)
                        {
                            VI.Cout(Uid, "翻出NPC为【{0}】.", zd.Monster(npc));
                            A0O.FlyingGet("M" + npc, 0, 0, true);
                        }
                        A0F.Mon1From = ushort.Parse(args[3]);
                        A0F.Monster1 = npc;
                    }
                    else if (args[1] == "4")
                    {
                        int hro = ushort.Parse(args[2]);
                        if (hro != 0)
                        {
                            VI.Cout(Uid, "翻出角色牌为【{0}】.", zd.Hero(hro));
                            A0O.FlyingGet("H" + hro, 0, 0, true);
                        }
                    }
                    else if (args[1] == "5")
                    {
                        ushort[] mons = Algo.TakeRange(args, 2, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        VI.Cout(Uid, "翻出怪物牌为【{0}】.", zd.Monster(mons));
                        A0O.FlyingGet(mons.Select(p => "M" + p).ToList(), 0, 0, true);
                    }
                    else if (args[1] == "6")
                    {
                        int position = int.Parse(args[2]);
                        if (args[3] == "0")
                        {
                            VI.Cout(Uid, "一张NPC牌被插入放置于牌堆顶第{0}张.", (position + 1));
                            ++A0F.MonCount;
                            A0O.FlyingGet("M0", 0, 0, true);
                        }
                        else
                        {
                            List<ushort> mons = Algo.TakeRange(args, 3, args.Length)
                                .Select(p => ushort.Parse(p)).ToList();
                            VI.Cout(Uid, "NPC牌【{0}】被插入放置于牌堆顶第{1}张.", mons, (position + 1));
                            A0O.FlyingGet(mons.Select(p => "M" + p).ToList(), 0, 0, true);
                            A0F.MonCount += mons.Count;
                        }
                    }
                    else if (args[1] == "7")
                    {
                        int count = int.Parse(args[2]);
                        VI.Cout(Uid, "{0}张怪物牌/NPC牌被置入怪物牌堆.", count);
                        A0O.FlyingGet(Enumerable.Repeat("M0", count).ToList(), 0, 0, true);
                        A0F.MonCount += count;
                    }
                    else if (args[1] == "8")
                    {
                        ushort[] tuxes = Algo.TakeRange(args, 2, args.Length)
                            .Select(p => ushort.Parse(p)).ToArray();
                        VI.Cout(Uid, "翻出手牌为【{0}】.", zd.Tux(tuxes));
                        A0O.FlyingGet(tuxes.Select(p => "C" + p).ToList(), 0, 0, true);
                    }
                    break;
                case "E0IS":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        for (int i = 2; i < args.Length; ++i)
                        {
                            string skillStr = args[i];
                            A0P[ut].GainSkill(skillStr);
                            VI.Cout(Uid, "{0}获得了技能『{1}』.", zd.Player(ut), zd.SkillName(args[i]));
                        }
                    }
                    break;
                case "E0OS":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        for (int i = 2; i < args.Length; ++i)
                        {
                            string skillStr = args[i];
                            Base.Skill sk = Tuple.SL.EncodeSkill(skillStr);
                            A0P[ut].LoseSkill(skillStr);
                            VI.Cout(Uid, "{0}失去了技能『{1}』.", zd.Player(ut), zd.SkillName(args[i]));
                        }
                    }
                    break;
                case "E0LH":
                    for (int i = 1; i < args.Length; i += 3)
                    {
                        ushort incr = ushort.Parse(args[i]);
                        ushort ut = ushort.Parse(args[i + 1]);
                        ushort to = ushort.Parse(args[i + 2]);
                        VI.Cout(Uid, "{0}HP上限{1}为{2}点.",
                            zd.Player(ut), (incr == 0 ? "减少" : "增加"), to);
                        A0P[ut].HPa = to;
                        if (A0P[ut].HP > A0P[ut].HPa)
                            A0P[ut].HP = A0P[ut].HPa;
                    }
                    break;
                case "E0IV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        int hro = int.Parse(args[2]);
                        string guestName = "副角色牌";
                        Hero hero = Tuple.HL.InstanceHero(A0P[ut].SelectHero);
                        if (hero != null && !string.IsNullOrEmpty(hero.GuestAlias))
                            guestName = hero.GuestAlias;
                        VI.Cout(Uid, "{0}迎来了{1}「{2}」.", zd.Player(ut), guestName, zd.Hero(hro));
                        A0O.FlyingGet("H" + hro, 0, ut);
                        A0P[ut].Coss = hro;
                    }
                    break;
                case "E0OV":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        int hro = int.Parse(args[2]);
                        int next = int.Parse(args[3]);
                        string guestName = "副角色牌";
                        Hero hero = Tuple.HL.InstanceHero(A0P[ut].SelectHero);
                        if (hero != null && !string.IsNullOrEmpty(hero.GuestAlias))
                            guestName = hero.GuestAlias;
                        VI.Cout(Uid, "{0}送走了{1}「{2}」.", zd.Player(ut), guestName, zd.Hero(hro));
                        A0O.FlyingGet("H" + hro, ut, 0);
                        A0P[ut].Coss = next;
                    }
                    break;
                case "E0PB":
                    for (int i = 2; i < args.Length;)
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
                                A0O.FlyingGet(cards.Select(p => "C" + p).ToList(), who, 0);
                                A0F.TuxCount += n;
                            }
                            else if (args[1] == "1")
                            {
                                VI.Cout(Uid, "{0}将{1}放回怪牌堆顶.", zd.Player(who), zd.Monster(cards));
                                A0O.FlyingGet(cards.Select(p => "M" + p).ToList(), who, 0);
                                A0F.MonCount += n;
                            }
                            else if (args[1] == "2")
                            {
                                VI.Cout(Uid, "{0}将{1}放回事件牌堆顶.", zd.Player(who), zd.Eve(cards));
                                A0O.FlyingGet(cards.Select(p => "E" + p).ToList(), who, 0);
                                A0F.EveCount += n;
                            }
                            i += (3 + n);
                        }
                        else if (hind == 1)
                        {
                            if (args[1] == "0")
                            {
                                VI.Cout(Uid, "{0}将{1}张牌放回手牌堆顶.", zd.Player(who), n);
                                A0O.FlyingGet(Algo.RepeatString("C0", n), who, 0);
                                A0F.TuxCount += n;
                            }
                            else if (args[1] == "1")
                            {
                                VI.Cout(Uid, "{0}将{1}张牌放回怪牌堆顶.", zd.Player(who), n);
                                A0O.FlyingGet(Algo.RepeatString("M0", n), who, 0);
                                A0F.MonCount += n;
                            }
                            else if (args[1] == "2")
                            {
                                VI.Cout(Uid, "{0}将{1}张牌放回事件牌堆顶.", zd.Player(who), n);
                                A0O.FlyingGet(Algo.RepeatString("E0", n), who, 0);
                                A0F.EveCount += n;
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
                        ushort rounder = ushort.Parse(args[2]);
                        A0P.Where(p => p.Key != rounder).ToList().ForEach((p) => p.Value.SetAsClear());
                        A0F.Hinder = 0; A0F.Supporter = 0;
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
                            if (s != 0)
                            {
                                joiners.Add(s);
                                if (position == 'T')
                                {
                                    msgs.Add(string.Format("{0}触发战斗", name));
                                    //A0F.Trigger = s;
                                    if (s != 0 && s < 1000)
                                        A0P[s].SetAsRounder();
                                }
                                else if (position == 'S')
                                {
                                    msgs.Add(string.Format("{0}支援", name));
                                    A0F.Supporter = s;
                                    if (s != 0 && s < 1000)
                                        A0P[s].SetAsSpSucc();
                                }
                                else if (position == 'H')
                                {
                                    msgs.Add(string.Format("{0}妨碍", name));
                                    A0F.Hinder = s;
                                    if (s != 0 && s < 1000)
                                        A0P[s].SetAsSpSucc();
                                }
                                else if (position == 'W')
                                {
                                    msgs.Add(string.Format("{0}代为触发战斗", name));
                                    //A0F.Horn = s;
                                    if (s != 0 && s < 1000)
                                    {
                                        A0P.Values.ToList().ForEach((p) => p.SetAsNotTrigger());
                                        A0P[s].SetAsDelegate();
                                    }
                                }
                            }
                        }
                        leavers.RemoveAll(p => joiners.Contains(p));
                        if (leavers.Count > 0)
                        {
                            msgs.AddRange(leavers.Select(p => string.Format("{0}退出战斗",
                                (p == 0 ? "无人" : (p < 1000 ? zd.Player(p) :
                                zd.Monster((ushort)(p - 1000)))))));
                            leavers.ForEach(p => { if (p < 1000) A0P[p].SetAsClear(); });
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
                        List<string> cards = Algo.TakeRange(args, 4, args.Length).ToList();
                        if (lugUt == A0P[who].Trove)
                        {
                            if (dirIn)
                            {
                                A0P[who].InsIntoLuggage(lugUt, cards);
                                VI.Cout(Uid, "{0}被收入{1}的{2}.",
                                    zd.MixedCards(cards), zd.Player(who), zd.Tux(lugUt));
                                //A0O.FlyingGet(cards, who, who);
                            }
                            else
                            {
                                A0P[who].DelIntoLuggage(lugUt, cards);
                                VI.Cout(Uid, "{0}被从{1}的{2}中取出.",
                                    zd.MixedCards(cards), zd.Player(who), zd.Tux(lugUt));
                                //A0O.FlyingGet(cards, who, 0);
                            }
                        }
                    }
                    break;
                case "E0TZ":
                    {
                        ushort to = ushort.Parse(args[1]);
                        ushort from = ushort.Parse(args[2]);
                        string[] cards = Algo.TakeRange(args, 3, args.Length);
                        A0O.FlyingGet(cards.ToList(), from, to);
                    }
                    break;
                case "E0MA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        ushort guad = ushort.Parse(args[2]);
                        A0P[who].Guardian = guad;
                        if (guad != 0)
                            VI.Cout(Uid, "{0}选择{1}.", zd.Player(who), zd.Guard(guad));
                        else
                            VI.Cout(Uid, "{0}撤销{1}.", zd.Player(who),
                                zd.GuardAlias(A0P[who].SelectHero, 0)); // TODO: set as A0P[who].Coss
                    }
                    break;
                case "E0ZJ":
                    {
                        ushort ut = ushort.Parse(args[1]);
                        ushort slot = ushort.Parse(args[2]);
                        ushort eq = ushort.Parse(args[3]);
                        if (slot == 1) { A0P[ut].Weapon = eq; A0P[ut].ExEquip = 0; }
                        else if (slot == 2) { A0P[ut].Armor = eq; A0P[ut].ExEquip = 0; }
                        else if (slot == 3) { A0P[ut].Trove = eq; A0P[ut].ExEquip = 0; }
                    }
                    break;
                case "E0YS":
                    {
                        char fromType = args[1][0];
                        ushort fromUt = ushort.Parse(args[2]);
                        for (int i = 3; i < args.Length; i += 2)
                        {
                            char toType = args[i][0];
                            ushort toUt = ushort.Parse(args[i + 1]);
                            A0O.NextTrail(fromType, fromUt, toType, toUt);
                        }
                    }
                    break;
                case "E0IF":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> sfs = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToList();
                        sfs.ForEach(p => A0P[who].InsRune(p));
                        VI.Cout(Uid, "{0}获得身法{1}.", zd.Player(who), zd.Rune(sfs));
                        A0O.FlyingGet(sfs.Select(p => "R" + p).ToList(), who, who);
                    }
                    break;
                case "E0OF":
                    {
                        ushort who = ushort.Parse(args[1]);
                        List<ushort> sfs = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToList();
                        sfs.ForEach(p => A0P[who].DelRune(p));
                        VI.Cout(Uid, "{0}失去身法{1}.", zd.Player(who), zd.Rune(sfs));
                        A0O.FlyingGet(sfs.Select(p => "R" + p).ToList(), who, 0);
                    }
                    break;
            }
        }
        #endregion E
        #region F
        private void HandleF0Message(string readLine)
        {
            VI.TerminCinTunnel(Uid);
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
                    {
                        if (A0P.ContainsKey(Uid))
                        {
                            if (args[1] == A0P[Uid].Team.ToString())
                            {
                                VI.Cout(Uid, "游戏结束，我方获胜.");
                                ad.SetCanan(CananPaint.CananSignal.ISWIN);
                            }
                            else
                            {
                                VI.Cout(Uid, "游戏结束，我方告负.");
                                ad.SetCanan(CananPaint.CananSignal.ISLOSE);
                            }
                        }
                        else if (A0P.ContainsKey(WATCHER_1ST_PERSPECT))
                        {
                            if (args[1] == A0P[WATCHER_1ST_PERSPECT].Team.ToString())
                            {
                                VI.Cout(Uid, "游戏结束，我方获胜.");
                                ad.SetCanan(CananPaint.CananSignal.ISWIN);
                            }
                            else
                            {
                                VI.Cout(Uid, "游戏结束，我方告负.");
                                ad.SetCanan(CananPaint.CananSignal.ISLOSE);
                            }
                        }
                        else
                            VI.Cout(Uid, "游戏结束，{0}方获胜.", args[1]);
                    }
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
                //VI.Cout(Uid, "等待下列玩家行动:{0}...", zd.Player(invs));
                int sina = string.IsNullOrEmpty(mai) ? 0 : int.Parse(mai.Substring("0,".Length));
                if ((sina & 1) != 0 && invs.Contains(Uid))
                {
                    if (!isReplay)
                    {
                        // manually cancel the call
                        cinCalled = StartCinEtc();
                        ShowProgresses(invs);
                        string input = VI.CinCMD0(Uid);
                        if (input == VI.CinSentinel)
                            return false;
                        VI.CloseCinTunnel(Uid);
                        WI.Send("U2,0," + sina, Uid, 0);
                    }
                }
                else
                {
                    // automatically not call at all
                    ShowProgresses(invs);
                    VI.CloseCinTunnel(Uid);
                    WI.Send("U2,0," + sina, Uid, 0);
                }
                return cinCalled;
            }
            //VI.Cout(Uid, "下列玩家与你均可行动:{0}.", zd.Player(invs));
            if (!isReplay)
            {
                flashHelper.AFlashApplicationWindow(ad);
                bool decided = false;
                while (!decided)
                {
                    IDictionary<string, string> skTable = new Dictionary<string, string>();
                    cinCalled = StartCinEtc();
                    string[] blocks = mai.Split(';');
                    //string opt = "您可以发动";
                    List<string> optlst = new List<string>();
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
                            //opt += zd.SKTXCZ(block) + ";";
                            optlst.Add(block);
                            skTable.Add(block, "^");
                        }
                        else
                        {
                            string name = block.Substring(0, jdx);
                            string rest = block.Substring(jdx + 1);
                            //opt += zd.SKTXCZ(name) + ";";
                            optlst.Add(name);
                            skTable.Add(name, rest);
                        }
                    }
                    //VI.Cout(uid, string.Join("&", optlst));
                    ShowProgresses(invs);
                    string inputBase = VI.CinCMD(Uid, optlst, true);
                    if (inputBase == VI.CinSentinel)
                        return false;
                    VI.CloseCinTunnel(Uid);
                    if (inputBase == "0")
                    {
                        decided = true;
                        VI.Cout(Uid, "您决定放弃行动.");
                        WI.Send("U2,0", Uid, 0);
                    }
                    else if (skTable.ContainsKey(inputBase))
                    {
                        if (skTable[inputBase] == "^") // Lock case
                        {
                            decided = true;
                            WI.Send("U2," + inputBase, Uid, 0);
                        }
                        else
                        {
                            cinCalled |= StartCinEtc();
                            string input = FormattedInputWithCancelFlag(skTable[inputBase]);
                            VI.CloseCinTunnel(Uid);
                            if (input == VI.CinSentinel)
                                return false;
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
            }
            return cinCalled;
        }
        private bool HandleU3Message(string mai, string prev, string inType)
        {
            bool cinCalled = false;
            string action = zd.AnalysisAction(mai, inType);
            VI.Cout(Uid, "已尝试{0}{1}，请继续：", action, zd.SKTXCZ(prev));
            if (!isReplay)
            {
                cinCalled = StartCinEtc();
                ad.ShowProgressBar(Uid);
                string input = FormattedInputWithCancelFlag(mai);
                if (input == VI.CinSentinel)
                    return false;
                VI.CloseCinTunnel(Uid);
                if (!input.StartsWith("/") && input != "")
                    WI.Send("U4," + prev + "," + input, Uid, 0);
                else
                    WI.Send("U4,0", Uid, 0);
            }
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
                ad.HideProgressBar(owner);
            }
            return false;
        }
        private bool HandleU7Message(string inv, string mai, string prev, string inType)
        {
            bool cinCalled = false;
            ushort owner = ushort.Parse(inv);
            string action = zd.AnalysisAction(mai, inType);
            VI.Cout(Uid, "{0}{1}{2}过程中，请继续：", zd.Player(owner), action, zd.SKTXCZ(prev));
            if (!isReplay)
            {
                flashHelper.AFlashApplicationWindow(ad);
                cinCalled = StartCinEtc();
                ad.ShowProgressBar(Uid);
                string input = FormattedInputWithCancelFlag(mai);
                if (input == VI.CinSentinel)
                    return false;
                VI.CloseCinTunnel(Uid);
                WI.SendDirect("U8," + prev + "," + input, Uid);
            }
            return cinCalled;
        }
        private bool HandleU9Message(string inv, string prev, string inType)
        {
            ushort owner = ushort.Parse(inv);
            VI.Cout(Uid, "等待{0}响应中:{1}...", zd.Player(owner), zd.SKTXCZ(prev));
            VI.CloseCinTunnel(Uid);
            ad.ShowProgressBar(owner);
            return false;
        }
        private bool HandleUAMessage(string inv, string mai, string inType)
        {
            ushort owner = ushort.Parse(inv);
            string action = inType.Contains('!') ? "爆发" : "执行";
            VI.Cout(Uid, "{0}{1}{2}完毕.", zd.Player(owner), action, zd.SKTXCZ(mai));
            ad.HideProgressBar(owner);
            return false;
        }
        #endregion U
        #region V
        public bool HandleV0Message(string cmdrst)
        {
            if (!isReplay)
            {
                StartCinEtc();
                string[] blocks = cmdrst.Split(',');
                int invCount = int.Parse(blocks[0]);
                for (int i = 0; i < invCount; ++i)
                    ad.ShowProgressBar(ushort.Parse(blocks[i + 1]));
                string input = FormattedInputWithCancelFlag(string.Join(
                    ",", Algo.TakeRange(blocks, 1 + invCount, blocks.Length)));
                if (input == VI.CinSentinel)
                    return false;
                VI.CloseCinTunnel(Uid);
                WI.Send("V1," + input, Uid, 0);
                return true;
            }
            else
                return false;
        }
        public bool HandleV2Message(string cmdrst)
        {
            if (!isReplay)
            {
                StartCinEtc();
                int idx = cmdrst.IndexOf(',');
                ushort major = ushort.Parse(cmdrst.Substring(0, idx));
                string input = FormattedInputWithCancelFlag(cmdrst.Substring(idx + 1));
                if (input == VI.CinSentinel)
                    return false;
                VI.CloseCinTunnel(Uid);
                WI.Send("V4," + input, Uid, 0);
                return true;
            }
            else return false;
        }
        public bool HandleV3Message(string cmdrst)
        {
            string[] splits = cmdrst.Split(',');
            List<ushort> invs = Algo.TakeRange(splits, 0, splits.Length)
                .Select(p => ushort.Parse(p)).ToList();
            VI.Cout(Uid, "等待{0}响应.", zd.Player(invs));
            VI.CloseCinTunnel(Uid);
            ShowProgresses(invs);
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
                        A0P[rounder].SetAsRounder();
                        if (type == 0)
                            VI.Cout(Uid, "{0}回合开始.", zd.Player(rounder));
                        else if (type == 1)
                            VI.Cout(Uid, "{0}回合跳过.", zd.Player(rounder));
                        else if (type == 2)
                        {
                            VI.Cout(Uid, "{0}回合跳过，恢复正常.", zd.Player(rounder));
                            A0P[rounder].Immobilized = false;
                        }
                        break;
                    }
                case "EV1":
                    {
                        ad.ShowProgressBar(rounder);
                        //if (rounder == Uid)
                        //{
                        //    //cinCalled = StartCinEtc();
                        //    string reply = DirectRInput("#您是否翻看事件牌？##不翻看##翻看,Y2");
                        //    if (reply.Equals("2"))
                        //        WI.Send("R" + rounder + "EV2,1", Uid, 0);
                        //    else
                        //        WI.Send("R" + rounder + "EV2,0", Uid, 0);                         
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
                        A0F.Eve1 = 0;
                        VI.Cout(Uid, "决定不翻看事件牌.");
                    }
                    //else
                    //{
                    //    ushort no = ushort.Parse(para);
                    //    A0F.Eve1 = no;
                    //    VI.Cout(Uid, "翻看事件牌【{0}】.", zd.Eve(no));
                    //}
                    ad.HideProgressBar(rounder);
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
                //            //string identity = isSupport != 0 ? "妨碍者{1}—0则不妨碍." :
                //            //    "支援者{1}—{0}则不支援" + (cancellable ? "，0则不打怪." : ".");
                //            cinCalled = StartCinEtc();
                //            ad.ShowProgressBar(centre);
                //        }
                //        string identity = isSupport != 0 ?
                //            "妨碍者{1}—0则不妨碍." : "支援者{1}—{0}则不支援";
                //        if (isDecider == 0) // decider
                //        {
                //            while (true)
                //            {
                //                string hint = "请决定" + (isSupport == 0 ? ("支援者，选择" + zd.PurePlayer(rounder)
                //                    + "则不支援.") : "妨碍者.");
                //                string select = VI.CinTP(Uid, candidates, hint, cancellable, false);
                //                //string select = VI.Cin(uid, "{0}战斗阶段开始，请决定" + identity,
                //                //    zd.Player(rounder), zd.PlayerWithMonster(candidates));
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
                //                string hint = "请推选" + (isSupport == 0 ? ("支援者，选择" + zd.PurePlayer(rounder)
                //                    + "则不支援.") : "妨碍者.");
                //                string select = VI.CinTP(Uid, candidates, hint, cancellable, false);
                //                //string select = VI.Cin(uid, "{0}战斗阶段开始，请推选" + identity,
                //                //    zd.Player(rounder), zd.PlayerWithMonster(candidates));
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
                //        else if (isDecider == 2)
                //        {
                //            VI.Cout(Uid, "等待{0}决定{1}.", centre, isSupport != 0 ? "妨碍者" : "支援者");
                //            if (centre != Uid)
                //                ad.yfJoy.CEE.DecideValid = false;
                //            ad.ShowProgressBar(centre);
                //        }
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
                            if (cop[2] != '3')
                                ad.HideProgressBar(from);
                        }
                        else
                        {
                            ushort from = ushort.Parse(para);
                            VI.Cout(Uid, "{0}已经做出了{1}.", zd.Player(from), suggest);
                            if (cop[2] != '3')
                                ad.HideProgressBar(from);
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
                            //List<string> cedcards = new List<string>();
                            //cedcards.Add("M" + mons);
                            //A0O.FlyingGet(cedcards, 0, 0);
                        }
                        //else if (args[0] == "1")
                        //{
                        //    ushort s = ushort.Parse(args[1]);
                        //    string ss = (s == 0) ? "不支援" : "{1}进行支援";
                        //    VI.Cout(Uid, "{0}决定打怪，" + ss + ".", zd.Player(rounder),
                        //        s < 1000 ? zd.Player(s) : zd.Monster((ushort)(s - 1000)));
                        //    A0F.Supporter = s;
                        //    if (s != 0 && s < 1000)
                        //        A0P[s].SetAsSpSucc();
                        //}
                        //else if (args[0] == "2")
                        //{
                        //    ushort h = ushort.Parse(args[1]);
                        //    string sh = (h == 0) ? "不妨碍" : "{0}进行妨碍";
                        //    VI.Cout(Uid, sh + ".", h < 1000 ? zd.Player(h) : zd.Monster((ushort)(h - 1000)));
                        //    A0F.Hinder = h;
                        //    if (h != 0 && h < 1000)
                        //        A0P[h].SetAsSpSucc();
                        //}
                    }
                    //VI.TerminCinTunnel(Uid);
                    break;
                case "ZM1":
                    {
                        A0F.Monster1 = 0; A0F.Monster2 = 0;
                        //ushort mons = ushort.Parse(para);
                        //VI.Cout(Uid, "本场战斗怪物为【{0}】.", zd.Monster(mons));
                        //A0F.Monster1 = mons;
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
                        //A0F.Monster1 = mons;
                        VI.Cout(Uid, "{0}跳过NPC，继续翻看怪物牌.", zd.Player(rounder));
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
                //    break;
                case "Z2":
                    A0F.PoolAka = 0; A0F.PoolAo = 0; break;
                case "ZF":
                    if (para == "0")
                        VI.Cout(Uid, "{0}战牌结束阶段开始.", zd.Player(rounder));
                    else if (para == "1")
                        VI.Cout(Uid, "{0}战牌结束阶段结束.", zd.Player(rounder));
                    break;
                case "BC":
                    A0P.Where(p => p.Key != rounder).ToList().ForEach((p) => p.Value.SetAsClear());
                    A0P[rounder].SetAsRounder();
                    A0F.Supporter = 0; A0F.Hinder = 0;
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
                    A0P.Values.ToList().ForEach((p) => p.SetAsClear());
                    A0F.Supporter = 0; A0F.Hinder = 0;
                    A0F.Monster1 = 0; A0F.Monster2 = 0;
                    A0F.Eve1 = 0;
                    A0F.PoolAka = 0; A0F.PoolAo = 0;
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
                            ushort aut = ushort.Parse(blocks[i + 1]);
                            if (aut == auid)
                            {
                                Uid = ushort.Parse(blocks[i]);
                                break;
                            }
                        }
                        int[] delta; ushort besu;
                        AoPlayer[] players = new AoPlayer[] {ad.yfPlayerR2.AoPlayer,
                            ad.yfPlayerO2.AoPlayer, ad.yfPlayerR3.AoPlayer, ad.yfPlayerO3.AoPlayer,
                            ad.yfPlayerR1.AoPlayer, ad.yfPlayerO1.AoPlayer};
                        if (IsUtAo())
                        {
                            besu = Uid;
                            delta = new int[] { 0, -1, 2, 1, 4, 3 };
                        }
                        else if (IsUtAka())
                        {
                            besu = Uid;
                            delta = new int[] { 0, 1, 2, 3, 4, 5 };
                        }
                        else
                        {
                            besu = 1;
                            delta = new int[] { 0, 1, 2, 3, 4, 5 };
                        }

                        for (int i = 0; i < totalPlayer; ++i)
                            A0P.Add(RoundUid(besu, delta[i]), players[i]);

                        for (int i = 0; i < blocks.Length; i += 3)
                        {
                            ushort ut = ushort.Parse(blocks[i]);
                            string name = blocks[i + 2];
                            A0P[ut].Nick = name; A0P[ut].Rank = ut;
                        }
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
                            VI.ShowTip("等待其它玩家{0}人物...", words[type]);
                            foreach (ushort ut in pys)
                                ad.ShowProgressBar(ut);
                        }
                    }
                    break;
                //        case "H1LV":
                //            //Z0D.Remove(ushort.Parse(cmdrst));
                //            break;
                //        case "H1CN":
                //            VI.Cout(uid, "游戏开始啦！");
                //            break;
                //        case "H0ST":
                //            if (cmdrst.StartsWith("0"))
                //            {
                //                VI.Cout(uid, "游戏开始阶段开始...");
                //                //VI.ReleaseCin(uid);
                //            }
                //            else if (cmdrst.StartsWith("1"))
                //            {
                //                VI.Cout(uid, "游戏开始阶段结束...");
                //                //VI.ReleaseCin(uid);
                //            }
                //            break;
                case "H0RT":
                    ad.yfArena.AoArena.Casting = new Base.Rules.CastingPick(); break;
                case "H0RM":
                    {
                        List<int> cands = cmdrst.Split(',').Select(p => int.Parse(p)).ToList();
                        bool randomed = false;

                        var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPick;
                        if (cands.Count > 0 && cands.Last() == 0)
                        {
                            cands.RemoveAt(cands.Count - 1);
                            randomed = true;
                        }
                        cp.Init(Uid, cands, randomed ? new int[] { 0 }.ToList() : null);
                        string hint = randomed ? "请选择您的角色，空白牌为随机替换。"
                            : "请选择您的角色。";
                        ad.yfArena.AoArena.Show();
                        cinCalled = StartCinEtc();
                        ad.ShowProgressBar(Uid);
                        while (true)
                        {
                            string input = VI.Cin48(Uid, hint);
                            int hero;
                            if (input == VI.CinSentinel)
                                break;
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
                            ad.HideProgressBar(puid);
                        }
                        else if (code == 1)
                        {
                            ushort puid = ushort.Parse(args[1]);
                            int heroCode = int.Parse(args[2]);
                            var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPick;
                            if (cp.Pick(puid, heroCode))
                            {
                                if (puid == Uid)
                                {
                                    VI.Cout(Uid, "确认您的选择为{0}.", zd.Hero(heroCode));
                                    ad.yfArena.AoArena.Shutdown();
                                }
                                else
                                    VI.Cout(Uid, "玩家{0}#已选择了角色{1}.", puid, zd.Hero(heroCode));
                                ad.HideProgressBar(puid);
                                A0P[puid].SelectHero = heroCode;
                                A0P[puid].IsAlive = true;
                            }
                        }
                    }
                    break;
                case "H0RS":
                    {
                        int jdx = cmdrst.IndexOf(',');
                        int from = int.Parse(cmdrst.Substring(0, jdx));
                        int to = int.Parse(cmdrst.Substring(jdx + 1));
                        var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPick;
                        if (cp.SwitchTo(Uid, from, to) != 0)
                        {
                            ad.yfArena.AoArena.Switch(from, to);
                            cinCalled = StartCinEtc();
                            while (true)
                            {
                                string input = VI.Cin48(Uid, "请选择您的角色。");
                                int hero;
                                if (input == VI.CinSentinel)
                                    break;
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
                        ad.yfArena.AoArena.Casting = ct;
                        ad.yfArena.AoArena.Show();
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
                        var ct = ad.yfArena.AoArena.Casting as Base.Rules.CastingTable;
                        ad.yfArena.AoArena.Active(null, false);
                        while (true)
                        {
                            string input = VI.Cin48(Uid, "请选择一名角色.");
                            int hero;
                            if (input == VI.CinSentinel)
                                break;
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
                        var ct = ad.yfArena.AoArena.Casting as Base.Rules.CastingTable;
                        if (ct.Pick(puid, heroCode))
                        {
                            if (puid == Uid)
                            {
                                VI.Cout(Uid, "确认您的选择为{0}.", zd.Hero(heroCode));
                                ad.yfArena.AoArena.Disactive(null);
                            }
                            else
                                VI.Cout(Uid, "玩家{0}#已选择了角色{1}.", puid, zd.Hero(heroCode));
                            ad.HideProgressBar(puid);
                            A0P[puid].SelectHero = heroCode;
                            A0P[puid].IsAlive = true;
                            ad.yfArena.AoArena.Remove(heroCode);
                        }
                    }
                    break;
                case "H0TA":
                    {
                        cinCalled = StartCinEtc();
                        string[] args = cmdrst.Split(',');
                        var ct = ad.yfArena.AoArena.Casting as Base.Rules.CastingTable;
                        while (true)
                        {
                            bool canGiveup = args.Contains("0");
                            ad.yfArena.AoArena.Active(null, canGiveup);
                            string input = VI.Cin48(Uid, "请禁选一名角色.");
                            int hero;
                            if (input == VI.CinSentinel)
                                break;
                            if (input == "0" && canGiveup)
                            {
                                WI.Send("H0TB,0", Uid, 0); break;
                            }
                            else if (int.TryParse(input, out hero) && ct.Xuan.Contains(hero))
                            {
                                WI.Send("H0TB," + hero, Uid, 0); break;
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
                        if (hrs.Count == 0 && hrs[0] == 0)
                            VI.Cout(Uid, "玩家{0}#未禁选.", puid, zd.Hero(hrs));
                        else
                        {
                            var ct = ad.yfArena.AoArena.Casting as Base.Rules.CastingTable;
                            foreach (int heroCode in hrs)
                            {
                                if (ct.Ban(puid, heroCode))
                                    ad.yfArena.AoArena.BanBy(puid, heroCode);
                            }
                            VI.Cout(Uid, "玩家{0}#禁选了角色{1}.", puid, zd.Hero(hrs));
                        }
                        ad.HideProgressBar(puid);
                        if (Uid == puid)
                            ad.yfArena.AoArena.Disactive(null);
                        //if (ct.Xuan.Count > 0)
                        //    VI.Cout(Uid, "当前剩余角色:\n{0}.", zd.HeroWithCode(ct.Xuan));
                    }
                    break;
                case "H0TJ":
                    {
                        string[] args = cmdrst.Split(',');
                        List<int> hrs = Algo.TakeRange(args, 0, args.Length)
                            .Select(p => int.Parse(p)).ToList();
                        VI.Cout(Uid, "新增了角色{0}.", zd.Hero(hrs));
                        var ct = ad.yfArena.AoArena.Casting as Base.Rules.CastingTable;
                        foreach (int heroCode in hrs)
                        {
                            if (ct.PutBack(heroCode))
                                ad.yfArena.AoArena.PuckBack(heroCode);
                        }
                        ad.yfArena.AoArena.Disactive(null);
                        //if (ct.Xuan.Count > 0)
                        //    VI.Cout(Uid, "当前剩余角色:\n{0}.", zd.HeroWithCode(ct.Xuan));
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

                        if (ad.yfArena.AoArena.Casting as Base.Rules.CastingPublic == null)
                        {
                            Base.Rules.CastingPublic cp = new Base.Rules.CastingPublic(x, dr, db, br, bb);
                            ad.yfArena.AoArena.Casting = cp;
                            ad.yfArena.AoArena.Show();
                        }
                        //else
                        //    ad.yfArena.AoArena.Check();
                    }
                    break;
                case "H0PA":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort who = ushort.Parse(args[0]);
                        if (who == Uid)
                        {
                            var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPublic;
                            cinCalled = StartCinEtc();
                            while (true)
                            {
                                ad.yfArena.AoArena.Active(new int[] { 1, 2 }, false);
                                string input = VI.Cin(Uid, "请禁选一名角色.");
                                int hero;
                                if (input == VI.CinSentinel)
                                    break;
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
                        var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPublic;
                        if (cp.Ban(team == 1, selAva))
                        {
                            VI.Cout(Uid, "{0}方禁选{1}.", team == 1 ? "红" : "蓝", zd.Hero(selAva));
                            ad.yfArena.AoArena.BanBy((ushort)team, selAva);
                            ad.yfArena.AoArena.Disactive(new int[] { 1, 2 });
                        }
                    }
                    break;
                case "H0PM":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort who = ushort.Parse(args[0]);
                        if (who == Uid)
                        {
                            var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPublic;
                            cinCalled = StartCinEtc();
                            while (true)
                            {
                                ad.yfArena.AoArena.Active(new int[] { 1, 2 }, false);
                                string input = VI.Cin(Uid, "请选择一名角色.",
                                    zd.HeroWithCode(cp.Xuan));
                                if (input == VI.CinSentinel)
                                    break;
                                int hero;
                                if (int.TryParse(input, out hero))
                                    if (cp.Xuan.Contains(hero) || (hero == 0 && cp.SilencedIdx < cp.Secrets.Count))
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
                        var cp = ad.yfArena.AoArena.Casting as Base.Rules.CastingPublic;

                        bool real = cp.PickReport(team == 1, selAva);
                        if (selAva == 0)
                        {
                            VI.Cout(Uid, "{0}方选择了未知人物.", team == 1 ? "红" : "蓝");
                            ad.yfArena.AoArena.PickBy((ushort)team, 0);
                            ad.yfArena.AoArena.Disactive(new int[] { 1, 2 });
                        }
                        else
                        {
                            VI.Cout(Uid, "{0}方选择了{1}.", team == 1 ? "红" : "蓝", zd.Hero(selAva));
                            ad.yfArena.AoArena.PickBy((ushort)team, selAva);
                            ad.yfArena.AoArena.Disactive(new int[] { 1, 2 });
                        }
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
                        ad.yfArena.AoArena.Casting = cc;
                        cc.CaptainMode = false;
                        for (int i = xsz1 + xsz2 + 2; i < args.Length; i += 2)
                            cc.Init(ushort.Parse(args[i]), int.Parse(args[i + 1]));
                        ad.yfArena.AoArena.Show();
                        //cc.ToHint(Uid, VI, zd.HeroWithCode, zd.Hero);
                        cinCalled = StartCinEtc();
                        bool isAka = (Uid % 2 == 1);
                        while ((!isAka && !cc.DecidedAo) || (isAka && !cc.DecidedAka))
                        {
                            List<int> xuanR = isAka ? cc.XuanAka : cc.XuanAo;
                            if (VI is VW.Cyvi)
                            {
                                string op = (VI as VW.Cyvi).Cin48(Uid, "请选择您的角色。");
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
                        ad.yfArena.AoArena.Casting = cc;
                        cc.CaptainMode = true;
                        for (int i = xsz1 + xsz2 + 2; i < args.Length; i += 2)
                            cc.Init(ushort.Parse(args[i]), int.Parse(args[i + 1]));
                        ad.yfArena.AoArena.Show();
                        if (cc.IsCaptain(Uid)) // Captain Only
                        {
                            cinCalled = StartCinEtc();
                            bool isAka = (Uid % 2 == 1);
                            while ((isAka && !cc.DecidedAka) || (!isAka && !cc.DecidedAo))
                            {
                                List<int> xuanR = isAka ? cc.XuanAka : cc.XuanAo;
                                if (VI is VW.Cyvi)
                                {
                                    string op = (VI as VW.Cyvi).Cin48(Uid, "请为我方玩家分配角色.");
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
                        var cc = ad.yfArena.AoArena.Casting as Base.Rules.CastingCongress;
                        cc.Set(puid, heroCode);
                        if (puid == Uid)
                            VI.Cout(Uid, "您预选了{0}.", zd.Hero(heroCode));
                        else
                            VI.Cout(Uid, "玩家{0}#已预选了{1}.", puid, zd.Hero(heroCode));
                        ad.yfArena.AoArena.CongressDing(puid, heroCode, cc.CaptainMode);
                        if (backCode != 0)
                        {
                            if (backTo != 0)
                            {
                                VI.Cout(Uid, "玩家{0}#已预选了{1}.", backTo, zd.Hero(backCode));
                                ad.yfArena.AoArena.CongressDing(backTo, backCode, cc.CaptainMode);
                            }
                            else
                            {
                                VI.Cout(Uid, "{0}被放回选将池中.", zd.Hero(backCode));
                                ad.yfArena.AoArena.CongressBack(backCode);
                            }
                        }
                        //if (!cc.CaptainMode || (Uid == 1 || Uid == 2))
                        //    cc.ToInputRequire(Uid, VI);
                    }
                    break;
                case "H0CC":
                    {
                        string[] args = cmdrst.Split(',');
                        ushort puid = ushort.Parse(args[0]);
                        int heroCode = int.Parse(args[1]);
                        VI.Cout(Uid, "{0}被放回选将池中.", zd.Hero(heroCode));
                        var cc = ad.yfArena.AoArena.Casting as Base.Rules.CastingCongress;
                        cc.Set(0, heroCode);
                        ad.yfArena.AoArena.CongressBack(heroCode);
                        //if (!cc.CaptainMode || (Uid == 1 || Uid == 2))
                        //    cc.ToInputRequire(Uid, VI);
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
                            var cc = ad.yfArena.AoArena.Casting as Base.Rules.CastingCongress;
                            if (IsUtAo())
                                cc.DecidedAo = true;
                            else
                                cc.DecidedAka = true;
                            VI.TerminCinTunnel(Uid);
                            ad.yfArena.AoArena.Shutdown();
                            string msg = "我方选择结果为：";
                            for (int i = 1; i < args.Length; i += 2)
                            {
                                ushort ut = ushort.Parse(args[i]);
                                int ava = int.Parse(args[i + 1]);
                                A0P[ut].SelectHero = ava;
                                A0P[ut].IsAlive = true;
                                msg += (ut + ":" + zd.Hero(ava) + ",");
                            }
                            VI.Cout(Uid, "{0}.", msg.Substring(0, msg.Length - 1));
                        }
                    }
                    break;

                case "H0SN":
                    ad.yfArena.AoArena.Shutdown(); break;

                case "H0SL":
                    {
                        string[] args = cmdrst.Split(',');
                        for (int i = 0; i < args.Length; i += 2)
                        {
                            ushort puid = ushort.Parse(args[i]);
                            int heroCode = ushort.Parse(args[i + 1]);
                            //msg += (puid + ":" + zd.Hero(heroCode) + ",");
                            A0P[puid].SelectHero = heroCode;
                            A0P[puid].ParseFromHeroLib();
                        }
                        VI.Cout(Uid, "选择结果为-" + string.Join(",", A0P.Values
                            .Where(p => p.SelectHero != 0)
                            .Select(p => p.Rank + ":" + zd.Hero(p.SelectHero))));
                    }
                    break;
                case "H0DP":
                    {
                        string[] args = cmdrst.Split(',');
                        A0F.TuxCount = int.Parse(args[0]); A0F.TuxDises = 0;
                        A0F.MonCount = int.Parse(args[1]); A0F.MonDises = 0;
                        A0F.EveCount = int.Parse(args[2]); A0F.TuxDises = 0;
                    }
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
                        AoPlayer[] players = new AoPlayer[] {ad.yfPlayerR2.AoPlayer,
                            ad.yfPlayerO2.AoPlayer, ad.yfPlayerR3.AoPlayer, ad.yfPlayerO3.AoPlayer,
                            ad.yfPlayerR1.AoPlayer, ad.yfPlayerO1.AoPlayer};
                        ushort besu; int[] delta;
                        if (IsUtAo())
                        {
                            besu = Uid;
                            delta = new int[] { 0, -1, 2, 1, 4, 3 };
                        }
                        else if (IsUtAka())
                        {
                            besu = Uid;
                            delta = new int[] { 0, 1, 2, 3, 4, 5 };
                        }
                        else
                        {
                            besu = WATCHER_1ST_PERSPECT;
                            delta = new int[] { 0, 1, 2, 3, 4, 5 };
                        }

                        for (int i = 0; i < totalPlayer; ++i)
                            A0P.Add(RoundUid(besu, delta[i]), players[i]);

                        string[] blocks = cmdrst.Split(',');
                        for (int idx = 0; idx < blocks.Length; idx += 2)
                        {
                            ushort who = ushort.Parse(blocks[idx]);
                            string name = blocks[idx + 1];
                            A0P[who].Nick = name; A0P[who].Rank = who;
                        }
                        break;
                    }
                case "H09R":
                    if (WI is VW.Eywi)
                    {
                        Uid = ushort.Parse(cmdrst);
                    }
                    break;
                case "H09G":
                    Algo.LongMessageParse(cmdrst.Split(','), InitPlayerPositionFromLongMessage,
                        InitPlayerFullFromLongMessage, Board.StatusKey);
                    break;
                case "H09P":
                    {
                        //Z0M = new ZeroMe(this);
                        //Z0F = new ZeroField(this);
                        //Z0P = new ZeroPiles(this);
                        string[] blocks = cmdrst.Split(',');
                        A0F.Eve1 = ushort.Parse(blocks[0]);
                        A0F.TuxCount = int.Parse(blocks[1]);
                        A0F.MonCount = int.Parse(blocks[2]);
                        A0F.EveCount = int.Parse(blocks[3]);

                        A0F.TuxDises = int.Parse(blocks[4]);
                        A0F.MonDises = int.Parse(blocks[5]);
                        A0F.EveDises = int.Parse(blocks[6]);

                        //A0F.Rounder
                        ushort rounder = ushort.Parse(blocks[7]);
                        ushort supporter = ushort.Parse(blocks[8]);
                        ushort hinder = ushort.Parse(blocks[9]);
                        foreach (AoPlayer ap in A0P.Values)
                            ap.SetAsClear();
                        if (rounder != 0)
                            A0P[rounder].SetAsRounder();
                        if (supporter != 0)
                        {
                            A0F.Supporter = supporter;
                            if (supporter < 1000)
                                A0P[supporter].SetAsSpSucc();
                        }
                        if (hinder != 0)
                        {
                            A0F.Hinder = hinder;
                            if (hinder < 1000)
                                A0P[hinder].SetAsSpSucc();
                        }
                        ushort mon1 = ushort.Parse(blocks[10]);
                        ushort mon2 = ushort.Parse(blocks[11]);
                        ushort eve1 = ushort.Parse(blocks[12]);
                        A0F.Monster1 = mon1; A0F.Monster2 = mon2; A0F.Eve1 = eve1;

                        for (int i = 13; i < Math.Min(blocks.Length, 17); i += 2)
                        {
                            if (blocks[i] == "1")
                                A0F.PoolAka = int.Parse(blocks[i + 1]);
                            else if (blocks[i] == "2")
                                A0F.PoolAo = int.Parse(blocks[i + 1]);
                        }
                        for (int i = 17; i < blocks.Length; i += 2)
                        {
                            if (blocks[i] == "1")
                                A0F.ScoreAka = int.Parse(blocks[i + 1]);
                            else if (blocks[i] == "2")
                                A0F.ScoreAo = int.Parse(blocks[i + 1]);
                        }
                    }
                    break;
                case "H0LT":
                    if (!GameGraceEnd)
                    {
                        ushort who = ushort.Parse(cmdrst);
                        if (who != 0)
                            VI.Cout(Uid, "玩家{0}#断线，游戏结束。", who);
                        else
                            VI.Cout(Uid, "服务器被延帝抓走啦，游戏结束。");
                        ad.SetCanan(CananPaint.CananSignal.FAIL_CONNECTION);
                    }
                    break;
                case "H0WT":
                    if (!GameGraceEnd)
                    {
                        ushort who = ushort.Parse(cmdrst);
                        if (who != 0)
                        {
                            VI.Cout(Uid, "玩家{0}#断线，请耐心等待其重连～", who);
                            A0P[who].SetAsLoser();
                        }
                        ReportConnectionLost();
                    }
                    break;
                case "H0WD":
                    if (!GameGraceEnd)
                    {
                        int secLeft = int.Parse(cmdrst);
                        VI.Cout(Uid, "房间将在{0}秒后彻底关闭。", secLeft);
                        if (secLeft == 180)
                            ad.SetCanan(CananPaint.CananSignal.LOSE_COUNTDOWN_48);
                        else if (secLeft == 60)
                            ad.SetCanan(CananPaint.CananSignal.LOSE_COUNTDOWN_12);
                    }
                    break;
                case "H0BK":
                    {
                        ushort who = ushort.Parse(cmdrst);
                        if (who != 0 && who != Uid)
                        {
                            VI.Cout(Uid, "玩家{0}#恢复连接。", who);
                            if (A0P.ContainsKey(who))
                                A0P[who].SetAsBacker();
                        }
                    }
                    break;
                case "H0RK":
                    VI.Cout(Uid, "房间已恢复正常。");
                    ad.SetCanan(CananPaint.CananSignal.NORMAL);
                    break;
                case "H09F":
                    {
                        string[] blocks = cmdrst.Split(',');
                        int idx = 0;
                        int tuxCount = int.Parse(blocks[idx]);
                        ++idx;
                        List<ushort> tuxes = Algo.TakeRange(blocks, idx, idx + tuxCount)
                            .Select(p => ushort.Parse(p)).ToList();
                        A0M.insTux(tuxes);
                        idx += tuxCount;
                        int folderCount = int.Parse(blocks[idx]);
                        ++idx;
                        List<ushort> folders = Algo.TakeRange(blocks, idx, idx + folderCount)
                            .Select(p => ushort.Parse(p)).ToList();
                        A0P[Uid].InsMyFolder(folders);
                        //A0M.InsMyFolder(folders);
                        idx += folderCount;
                        int skillCount = int.Parse(blocks[idx]);
                        ++idx;
                        if (skillCount > 0)
                        {
                            List<string> skills = Algo.TakeRange(blocks, idx, idx + skillCount).ToList();
                            A0P[Uid].ClearSkill();
                            A0P[Uid].GainSkill(skills);
                        }
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
                if (A0P.ContainsKey(uit))
                {
                    if (A0P[uit].SelectHero != 0)
                        VI.Chat(msgtext, zd.Hero(A0P[uit].SelectHero) + "(" + A0P[uit].Nick + ")");
                    else
                        VI.Chat(msgtext, A0P[uit].Nick);
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

        #region Utils

        private static string Substring(string @string, int start, int end)
        {
            if (end >= 0)
                return @string.Substring(start, end - start);
            else
                return @string.Substring(start);
        }

        private static int CountItemFromComma(string line)
        {
            if (string.IsNullOrEmpty(line))
                return 0;
            int count = 1;
            int idx = line.IndexOf(',');
            while (idx < line.Length && idx >= 0)
            {
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

        private void ShowProgresses(IEnumerable<ushort> invs)
        {
            foreach (ushort inv in invs)
                ad.ShowProgressBar(inv);
        }
        // from H09G now
        private void InitPlayerPositionFromLongMessage(ushort who)
        {
            if (!A0P.ContainsKey(who)) { return; }
            AoPlayer ap = A0P[who];
            ap.Rank = who; ap.Team = (ap.Rank == 0 ? 0 : (ap.Rank % 2 == 1 ? 1 : 2));
        }
        // from H09G now
        private void InitPlayerFullFromLongMessage(ushort who, string key, object value)
        {
            if (!A0P.ContainsKey(who)) { return; }
            AoPlayer ap = A0P[who];
            switch (key)
            {
                case "hero": ap.SelectHero = (int)value; break;
                case "state":
                    ap.IsAlive = (((int)value & 1) != 0);
                    ap.IsLoved = (((int)value & 2) != 0);
                    ap.Immobilized = (((int)value & 4) != 0);
                    ap.PetDisabled = (((int)value & 8) != 0); break;
                case "hp": ap.HP = (ushort)value; break;
                case "hpa": ap.HPa = (ushort)value; break;
                case "str": ap.STR = (ushort)value; break;
                case "stra": ap.STRa = (ushort)value; break;
                case "dex": ap.DEX = (ushort)value; break;
                case "dexa": ap.DEXa = (ushort)value; break;
                case "tuxCount":
                    ap.TuxCount = (int)value;
                    if (Uid >= 1000 && who == WATCHER_1ST_PERSPECT)
                        A0M.insTux(Enumerable.Repeat((ushort)0, ap.TuxCount).ToList());
                    break;
                case "wp": ap.Weapon = (ushort)value; break;
                case "am": ap.Armor = (ushort)value; break;
                case "tr": ap.Trove = (ushort)value; break;
                case "exq": ap.ExEquip = (ushort)value; break;
                case "lug": ap.InitToLuggage((string[])value); break;
                case "guard": ap.Guardian = (ushort)value; break;
                case "coss": ap.Coss = (ushort)value; break;
                case "pet": ap.InsPet((ushort[])value); break;
                case "excard": ap.InsExCards((ushort[])value); break;
                case "token": ap.Token = (int)value; break;
                case "fakeq":
                    if (value is string[])
                    {
                        for (int i = 0; i < ((string[])value).Length; i += 2)
                            ap.InsFakeq(ushort.Parse(((string[])value)[i]), ((string[])value)[i + 1]);
                    }
                    else if (value is ushort[])
                    {
                        for (int i = 0; i < ((ushort[])value).Length; ++i)
                            ap.InsFakeq(((ushort[])value)[i], "0");
                    }
                    break;
                case "rune":
                    foreach (ushort ut in ((ushort[])value))
                        ap.InsRune(ut);
                    break;
                case "excl": ap.InsExSpCard((string[])value); break;
                case "tar":
                    if (value is ushort[])
                        ap.InsPlayerTar((ushort[])value);
                    else if (value is ushort)
                        ap.InsPlayerTar((ushort)value);
                    break;
                case "awake": ap.Awake = (ushort)value == 1; break;
                case "foldsz": ap.FolderCount = (int)value; break;
                case "escue":
                    foreach (ushort ut in ((ushort[])value))
                        ap.InsEscue(ut);
                    break;
            }
        }
        #endregion Utils
        #region Network Report
        internal void ReportConnectionLost()
        {
            if (!GameGraceEnd)
            {
                VI.Cout(Uid, "网络连接故障，等待重连中...");
                ad.SetCanan(CananPaint.CananSignal.LOSE_CONNECTION);
            }
        }
        #endregion Network Report
        #region Replay
        internal void ReplayPrev()
        {
            VW.Eywi eywi = WI as VW.Eywi;
            if (isReplay && eywi != null)
                --eywi.MagIndex;
        }
        internal void ReplayPlay()
        {
            VW.Eywi eywi = WI as VW.Eywi;
            if (isReplay && eywi != null)
                eywi.InProcess = true;
        }
        internal void ReplayPause()
        {
            VW.Eywi eywi = WI as VW.Eywi;
            if (isReplay && eywi != null)
                eywi.InProcess = false;
        }
        internal void ReplayNext()
        {
            VW.Eywi eywi = WI as VW.Eywi;
            if (isReplay && eywi != null)
                ++eywi.MagIndex;
        }
        internal string GetMagi()
        {
            VW.Eywi eywi = WI as VW.Eywi;
            return eywi.CurrentMagi;
        }
        #endregion Replay
    }
}
