using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.VW;
using System.Text.RegularExpressions;
using System.Threading;

namespace PSD.ClientAo.VW
{
    public class Cyvi : IVI
    {
        private AoDisplay ad;
        // queue for command code, help and talk respectively
        private Queue<string> cvQueues, hpQueues, tkQueues;

        private int cinReqCount;

        private Boolean cinGate;

        private Thread cinListenThread, talkListenThread;

        private int shuzi = 0;
        private int activeCin = 0;

        internal Base.Log Log { set; get; }

        public Cyvi(AoDisplay ad, bool record, bool msglog)
        {
            //this.count = count;
            //Log = new Base.Log(); Log.Start(312, record, msglog, 0);
            this.ad = ad;
            this.cvQueues = new Queue<string>();
            this.cinReqCount = 0;
            this.cinGate = false;
            this.hpQueues = new Queue<string>();
            tkQueues = new Queue<string>();
            InputCommand = new Queue<string>();
            InputTalk = new Queue<string>();
            cinListenThread = new Thread(() => Util.SafeExecute(() => CinListenStarts(),
                delegate(Exception e) { if (Log != null) { Log.Logg(e.ToString()); } }));
            talkListenThread = new Thread(() => Util.SafeExecute(() => TalkListenStarts(),
                delegate(Exception e) { if (Log != null) { Log.Logg(e.ToString()); } }));
        }

        public void Init()
        {
            cinListenThread.Start();
            talkListenThread.Start();
            AD.yfMinami.input += InsertMessage;
            AD.yfDeal.input += InsertMessage;
            AD.yfJoy.input += InsertMessage;
            AD.yfArena.input += InsertMessage;
            AD.yfMigi.VI = this;
        }
        private bool InGame { set; get; }
        // Set whether in game mode, whether operations can be accepted
        public void SetInGame(bool value) { InGame = value; }

        public void AbortCinThread()
        {
            if (cinListenThread != null && cinListenThread.IsAlive)
                cinListenThread.Abort();
        }

        #region Special Zone

        public AoDisplay AD { get { return ad; } }
        public Queue<string> InputCommand { private set; get; }
        public Queue<string> InputTalk { private set; get; }

        public void SetRoom(int room)
        {
            AD.SetRoom(room);
        }
        public void SetNick(int pos, string name, int avatar)
        {
            AoPlayer ap = null;
            switch (pos)
            {
                case 1: ap = AD.yfPlayerR2.AoPlayer; break;
                case 2: ap = AD.yfPlayerR3.AoPlayer; break;
                case 3: ap = AD.yfPlayerO3.AoPlayer; break;
                case 4: ap = AD.yfPlayerO2.AoPlayer; break;
                case 5: ap = AD.yfPlayerO1.AoPlayer; break;
                case 6: ap = AD.yfPlayerR1.AoPlayer; break;
            }
            if (ap != null)
            {
                ap.Nick = name;
                ap.IsAlive = true;
                //ap.SelectHero = avatar;
            }
        }
        private bool InsertMessage(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("@@"))
                {
                    lock (tkQueues)
                        tkQueues.Enqueue("Y1," + line.Substring("@@".Length));
                }
                else
                {
                    line = line.ToUpper().Trim();
                    if (line.StartsWith("@#"))
                    {
                        lock (tkQueues)
                            tkQueues.Enqueue("Y3," + line.Substring("@#".Length));
                    }
                    else if (line.StartsWith("/"))
                    {
                        lock (hpQueues)
                        {
                            hpQueues.Enqueue(line.Substring(1));
                        }
                    }
                    else
                    {
                        if (cinGate)
                            lock (cvQueues)
                            {
                                cvQueues.Enqueue(line);
                            }
                        ++shuzi;
                    }
                }
                return true;
            }
            else return false;
        }
        public void PreCin(ushort me)
        {
            int count = cinReqCount;
            for (int i = 0; i < count; ++i)
                cvQueues.Enqueue(CinSentinel);
            while (cinReqCount > 0)
                Thread.Sleep(50);
        }
        private string Cin(ushort me)
        {
            ++cinReqCount;
            cinGate = true;
            string msg = null;
            do
            {
                lock (cvQueues)
                {
                    if (cvQueues.Count > 0)
                        msg = cvQueues.Dequeue();
                }
                Thread.Sleep(100);
            } while (msg == null);
            //if (msg != CinSentinel)
            --cinReqCount;

            if (cinReqCount == 0)
                cinGate = false;
            return msg;
        }
        // Cin of case Y.
        internal string CinY(ushort uid, int count, bool cancellable, params string[] names)
        {
            PreCin(uid);
            AD.yfMinami.Minami.Show(count, names);
            if (cancellable)
                AD.yfJoy.CEE.CancelValid = true;
            else
                AD.yfJoy.CEE.CancelValid = false;
            return Cin(uid);
        }
        internal void ShowTip(string tip) { AD.yfMinami.Minami.ShowTip(tip); }
        internal void ShowTip(string tipBase, params string[] pars)
        {
            ShowTip(string.Format(tipBase, pars));
        }
        internal void HideTip() { AD.yfMinami.Minami.HideTip(); }
        // Invalid, only cancel
        internal string Cin00(ushort uid)
        {
            PreCin(uid);
            ShowTip("不能指定合法目标.");
            AD.yfJoy.CEE.CancelValid = true;
            string any = Cin(uid);
            if (any != CinSentinel)
                HideTip();
            return any;
        }
        // any input
        internal string Cin01(ushort uid)
        {
            PreCin(uid);
            ShowTip("请确定。");
            AD.yfJoy.DecideMessage = "0";
            AD.yfJoy.CEE.DecideValid = true;
            string any = Cin(uid);
            if (any != CinSentinel)
                HideTip();
            return any;
        }
        // specific input
        internal string Cin48(ushort uid, string hint, params string [] pars)
        {
            PreCin(uid);
            ShowTip(hint, pars);
            string any = Cin(uid);
            if (any != CinSentinel)
                HideTip();
            return any;
        }
        internal string CinI(ushort uid, string prevComment,
            int r1, int r2, IEnumerable<string> uss, bool cancellable, bool keep)
        {
            PreCin(uid);
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(uss, null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinI()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }
        internal string CinH(ushort uid, string prevComment,
            int r1, int r2, IEnumerable<ushort> heros)
        {
            PreCin(uid);
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(heros.Select(p => "H" + p), null, r1, r2, false, false);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }

        internal string CinC(ushort uid, string prevComment, int r1, int r2,
            List<ushort> uss, int zero, bool cancellable, bool keep)
        {
            PreCin(uid);
            ShowTip(prevComment);
            if (zero > 0)
            {
                string[] c0 = new string[zero];
                for (int i = 0; i < zero; ++i)
                    c0[i] = "C0";
                AD.yfDeal.Deal.Show(c0, uss.Select(p => "C" + p), r1, r2, cancellable, keep);
            }
            else
                AD.yfDeal.Deal.Show(uss.Select(p => "C" + p), null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinC()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }

        internal string CinM(ushort uid, string prevComment, int r1, int r2,
            List<ushort> uss, bool cancellable, bool keep)
        {
            PreCin(uid);
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(uss.Select(p => "M" + p), null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinM()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }

        internal string CinQ(ushort uid, string comment, int r1, int r2,
            List<ushort> uss, int zero, bool cancellable, bool keep)
        {
            PreCin(uid);
            ShowTip(comment);
            AD.Mix.StartSelectQard(uss.ToList(), r1, r2);
            if (cancellable)
                AD.yfJoy.CEE.CancelValid = true;
            string result = Cin(uid);
            if (result != CinSentinel)
            {
                if (!keep)
                    AD.Mix.FinishSelectQard();
                else
                    AD.Mix.LockSelectQard();
                HideTip();
            }
            return result;
        }
        internal void OCinQ()
        {
            AD.Mix.FinishSelectQard();
            HideTip();
        }

        internal string CinT(ushort uid, IEnumerable<ushort> targets, int r1,
            int r2, string comment, bool cancellable, bool keep)
        {
            PreCin(uid);
            ShowTip(comment);
            AD.Mix.StartSelectTarget(targets.ToList(), r1, r2);
            if (cancellable)
                AD.yfJoy.CEE.CancelValid = true;
            string result = Cin(uid);
            if (result != CinSentinel)
            {
                if (!keep)
                    AD.Mix.FinishSelectTarget();
                else
                    AD.Mix.LockSelectTarget();
                HideTip();
            }
            return result;
        }

        internal void OCinT()
        {
            AD.Mix.FinishSelectTarget();
            HideTip();
        }
        
        internal string CinZ(ushort uid, string prevComment, int r1, int r2, 
            IEnumerable<ushort> uss, bool cancellable, bool keep)
        {
            PreCin(uid);
            string comment = (r1 != r2) ?
                (string.Format("请选择{0}-{1}张卡牌为{2}目标。", r1, r2, prevComment)) :
                (string.Format("请选择{0}张卡牌为{1}目标。", r1, prevComment));

            ShowTip(comment);
            AD.yfDeal.Deal.Show(uss.Select(p => "C" + p), null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinZ()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }
        internal string CinX(ushort uid, int r1, int r2, List<string> ussnm,
            bool cancellable, bool keep)
        {
            PreCin(uid);
            bool single = (r1 == r2 && r1 == ussnm.Count);
            string comment;
            if (ussnm.Count == 1)
                comment = "";
            else if (single)
                comment = "请重排以下卡牌。";
            else
                comment = "请重排以下卡牌，弃置第二行的卡牌，保留" + r1 + "张卡牌。";
            ShowTip(comment);
            AD.yfDeal.Deal.ShowXArrage(ussnm, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }

        internal string CinD(ushort uid, int r1, int r2, string prevComment, bool cancellable)
        {
            PreCin(uid);
            ShowTip(prevComment);
            List<int> uss;
            if (r2 <= 6)
                uss = Enumerable.Range(r1, r2 - r1 + 1).ToList();
            else
                uss = Enumerable.Range(r1, 7).ToList();
            AD.yfDeal.Deal.Show(uss.Select(p => "D" + p), null, 1, 1, cancellable, false);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            if (result == "7")
                result = "6+";
            return result;
        }

        internal void Watch(ushort uid, IEnumerable<string> enumerable, string tvTag)
        {
            AD.yhTV.Show(enumerable, tvTag);
            //OI.AoTV tv = new OI.AoTV(AD);
            //tv.Show(enumerable);
        }
        internal void OWatch(ushort uid, string title)
        {
            AD.yhTV.Recycle(title);
        }

        internal string CinTP(ushort uid, IEnumerable<string> uss, string comment,
            bool cancellable, bool keep)
        {
            PreCin(uid);
            ShowTip(comment);
            List<ushort> pets = uss.Where(p => p.StartsWith("PT")).Select(
                p => ushort.Parse(p.Substring("PT".Length))).ToList();
            List<ushort> tars = uss.Where(p => p.StartsWith("T")).Select(
                p => ushort.Parse(p.Substring("T".Length))).ToList();
            if (tars.Count > 0)
                AD.Mix.StartSelectTarget(tars, 1, 1);
            if (pets.Count > 0)
                AD.Mix.StartSelectPT(pets, false);
            if (cancellable)
                AD.yfJoy.CEE.CancelValid = true;
            string result = Cin(uid);
            if (result != CinSentinel)
            {
                if (!keep)
                {
                    AD.Mix.FinishSelectTarget();
                    AD.Mix.FinishSelectPT();
                }
                else
                    AD.Mix.LockSelectTarget();
                HideTip();
            }
            return result;
        }

        internal string CinCMD0(ushort uid)
        {
            PreCin(uid);
            ShowTip("您无法行动，请放弃行动.");
            //AD.yfJoy.CEE.ResetHightlight();
            AD.yfJoy.CEE.CancelValid = true;
            string any = Cin(uid);
            if (any != CinSentinel)
                HideTip();
            return any;
        }
        internal string CinCMD(ushort uid, List<string> optLst, bool cancellable)
        {
            PreCin(uid);
            ShowTip("请响应.");
            List<ushort> txs = new List<ushort>();
            List<string> njs = new List<string>();
            List<ushort> pts = new List<ushort>();
            IDictionary<string, string> prevPara = new Dictionary<string, string>();

            foreach (string ops in optLst)
            {
                int idx = ops.IndexOf(',');
                string title = Util.Substring(ops, 0, idx);
                if (title.StartsWith("JN")) // Might contains target, JN60102(4)
                    AD.yfJoy.CEE.SetSkillHighlight(title, true);
                else if (title.StartsWith("CZ"))
                    AD.yfJoy.CEE.SetCZHighlight(title, true);
                else if (title.StartsWith("TX"))
                    txs.Add(ushort.Parse(title.Substring("TX".Length)));
                else if (title.StartsWith("NJ"))
                    njs.Add(title);
                else if (title.StartsWith("PT"))
                    pts.Add(ushort.Parse(title.Substring("PT".Length)));
                if (idx >= 0)
                    prevPara.Add(title, ops.Substring(idx + 1));
            }
            if (txs.Count > 0)
                AD.Mix.StartSelectTX(txs);
            if (njs.Count > 0)
            {
                IDictionary<string, string> encoding = new Dictionary<string, string>();
                foreach (string nj in njs)
                    encoding.Add(ad.Tuple.NJL.EncodeNCAction(nj).Name, nj);
                AD.yfMinami.Minami.ShowWithEncoding(njs.Count, "请选择执行NPC效果或取消.", encoding);
            }
            if (pts.Count > 0)
                AD.Mix.StartSelectPT(pts, true);
            AD.yfJoy.CEE.CancelValid = cancellable;
            string any = Cin(uid);
            if (any != CinSentinel)
            {
                AD.Mix.FinishSelectQard();
                AD.Mix.FinishSelectPT();
                HideTip();
            }
            return any;
        }

        #endregion Special Zone

        #region Implementation

        private void CinListenStarts()
        {
            string line = "";
            do
            {
                lock (InputCommand)
                {
                    if (InputCommand.Count > 0)
                        line = InputCommand.Dequeue();
                    else
                        line = "";
                }
                if (line == "" || !InsertMessage(line))
                    Thread.Sleep(500);
            } while (line != null);
        }

        private void TalkListenStarts()
        {
            string line;
            do 
            {
                lock (InputTalk)
                {
                    if (InputTalk.Count > 0)
                        line = InputTalk.Dequeue();
                    else
                        line = "";
                }
                if (line == "" || !InsertMessage(line))
                    Thread.Sleep(500);
            } while (line != null);
        }

        private void FCout(ushort me, string msg)
        {
            ad.Dispatcher.BeginInvoke((Action)(() =>
            {
                ad.yfMigi.IncrText(msg);
                ad.yfMigi.svText.ScrollToEnd();
            }));
            if (Log != null)
                Log.Record(msg);
        }

        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            FCout(me, string.Format(msgFormat, args));
        }

        private string FCin(ushort me, string hint)
        {
            // TODO: specified FCin??
            ad.Dispatcher.BeginInvoke((Action)(() =>
            {
                ad.yfMigi.IncrText("===> " + hint);
                ad.yfMigi.svText.ScrollToEnd();
            }));
            int count = cinReqCount;
            for (int i = 0; i < count; ++i)
                cvQueues.Enqueue(CinSentinel);
            while (cinReqCount > 0)
                Thread.Sleep(50);
            while (cvQueues.Count > 0 && cvQueues.Peek() == CinSentinel)
                cvQueues.Dequeue();

            ++cinReqCount;
            cinGate = true;
            string msg = null;
            do
            {
                lock (cvQueues)
                {
                    if (cvQueues.Count > 0)
                        msg = cvQueues.Dequeue();
                }
                Thread.Sleep(100);
            } while (msg == null);
            //if (msg != CinSentinel)
            --cinReqCount;

            if (cinReqCount == 0)
                cinGate = false;
            return msg;
        }
        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            return FCin(me, string.Format(hintFormat, args));
        }

        private void ResetAllInputs()
        {
            HideTip();
            ad.yfBag.Me.ResumeTux();
            ad.yfPlayerR2.AoPlayer.ResumeExCards();
            ad.yfPlayerR2.AoPlayer.ResumePets();
            ad.yfPlayerR2.AoPlayer.DisableWeapon();
            ad.yfPlayerR2.AoPlayer.DisableArmor();
            ad.yfPlayerR2.AoPlayer.DisableExEquip();
            // Disable pets
            ad.yfJoy.CEE.ResetHightlight();
            ad.Mix.FinishSelectTarget();
            ad.ResetAllSelectedList();
        }
        // Open Cin Tunnel
        public void OpenCinTunnel(ushort me) { ResetAllInputs(); ++activeCin; }
        // Close Cin Tunnel
        public void CloseCinTunnel(ushort me)
        {
            //if (!cinGate)
            if (activeCin > 0)
                --activeCin;
            if (activeCin == 0)
            {
                ResetAllInputs();
                ad.HideProgressBar(me);
            }
        }
        // Close Obsoleted Tunnel
        public void CloseObsoletedTunnel(ushort me)
        {
            ResetAllInputs();
            ad.HideProgressBar(me);
        }
        // Terminate Cin Tunnel, give pending Cin CinSentinel as result
        public void TerminCinTunnel(ushort me)
        {
            int count = cinReqCount;
            for (int i = 0; i < count; ++i)
                cvQueues.Enqueue(CinSentinel);
            while (cinReqCount > 0)
                Thread.Sleep(50);
            if (!cinGate)
                ResetAllInputs();
        }
        // Reset Cin Tunnel, clean all pending input request
        public void ResetCinTunnel(ushort me) { cinReqCount = 0; }
        public string CinSentinel { get { return "\\"; } }

        // TODO:DEBUG: only display hidden message
        public void Cout0(ushort me, string msg) { }
        // Request in Client
        public string Request(ushort me)
        {
            string msg = null;
            do
            {
                lock (hpQueues)
                {
                    if (hpQueues.Count > 0)
                        msg = hpQueues.Dequeue().ToString();
                }
                Thread.Sleep(100);
            } while (msg == null);
            return msg;
        }
        public string RequestTalk(ushort me)
        {
            string msg = null;
            do
            {
                lock (tkQueues)
                {
                    if (tkQueues.Count > 0)
                        msg = tkQueues.Dequeue().ToString();
                }
                Thread.Sleep(100);
            } while (msg == null);
            return msg;
        }
        //public void Chat(Msgs msg)
        //{
        //    if (AD != null)
        //        AD.DisplayChat(msg.From, msg.Msg);
        //}
        public void Chat(string msg, string nick)
        {
            if (AD != null)
                AD.DisplayChat(nick, msg);
        }
        #endregion Implementation
    }
}
