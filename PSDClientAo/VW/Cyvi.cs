using System;
using System.Collections.Generic;
using System.Linq;
using PSD.Base.VW;
using System.Threading;
using Algo = PSD.Base.Utils.Algo;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PSD.ClientAo.VW
{
    public class Cyvi : IVI
    {
        // help message (e.g. /h)
        private BlockingCollection<string> hpQueue;
        // chat and setting message
        private BlockingCollection<string> tkQueue;
        // main queue
        private BlockingCollection<string> cvQueue;

        public string CinSentinel { get { return "\\"; } }
        // general ctoken for moniter upstream
        private CancellationTokenSource ctoken;
        // token for Cin, would be cancelled and refreshed when notified
        private CancellationTokenSource curToken;

        internal Base.ClLog Log { set; get; }
        // Set whether the game is started or still in preparation
        // thus whether operations can be accepted or not
        public void SetInGame(bool value) { mInGame = value; }
        private bool mInGame;

        public Cyvi(AoDisplay ad)
        {
            hpQueue = new BlockingCollection<string>();
            tkQueue = new BlockingCollection<string>();
            ctoken = new CancellationTokenSource();
            curToken = null;
            cvQueue = new BlockingCollection<string>();
            AD = ad;
        }

        private AoDisplay AD { set; get; }

        #region Implements
        public void Init()
        {
            AD.yfMinami.input += Offer;
            AD.yfDeal.input += Offer;
            AD.yfJoy.input += Offer;
            AD.yfArena.input += Offer;
            AD.yfMigi.input += Offer;
        }
        // accept the line from console or other places
        public void Offer(string line)
        {
            Task.Factory.StartNew(() =>
            {
                if (!string.IsNullOrEmpty(line) && line != CinSentinel)
                {
                    if (line.StartsWith("@@")) // Chat
                        tkQueue.Add("Y1," + line.Substring("@@".Length));
                    else if (line.StartsWith("@#")) // Setting
                    {
                        if (mInGame)
                            tkQueue.Add("Y3," + line.Substring("@#".Length));
                    }
                    else
                    {
                        line = line.Trim().ToUpper();
                        if (line.StartsWith("/"))
                            hpQueue.Add(line.Substring("/".Length));
                        else if (mInGame)
                            cvQueue.Add(line);
                    }
                }
            });
        }

        public void Chat(string msg, string nick)
        {
            AD?.DisplayChat(nick, msg);
        }

        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                AD.yfMigi.IncrText("===> " + string.Format(hintFormat, args));
                AD.yfMigi.svText.ScrollToEnd();
            }));
            return Cin(me);
        }
        private string Cin(ushort me)
        {
            try { return cvQueue.Take(curToken.Token); }
            catch (OperationCanceledException) { return CinSentinel; }
        }
        private void PreCinClearup()
        {
            curToken?.Cancel();
            curToken?.Dispose();
            curToken = new CancellationTokenSource();
            while (cvQueue.Count > 0)
                cvQueue.Take();
            ResetAllInputs();
        }

        public void CloseCinTunnel(ushort me)
        {
            CancellationTokenSource token = curToken;
            curToken = null;
            token?.Cancel();
            token?.Dispose();
            ResetAllInputs();
        }

        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            string msg = string.Format(msgFormat, args);
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                AD.yfMigi.IncrText(msg);
                AD.yfMigi.svText.ScrollToEnd();
            }));
            Log?.Record(msg);
        }

        public string RequestHelp(ushort me) { return hpQueue.Take(ctoken.Token); }

        public string RequestTalk(ushort me) { return tkQueue.Take(ctoken.Token); }

        public void Close()
        {
            ctoken.Cancel(); ctoken.Dispose();
            hpQueue.Dispose();
            tkQueue.Dispose();
            cvQueue.Dispose();
        }
        /// <summary>
        /// start an async Listening task
        /// </summary>
        /// <param name="action">the acutal listen action</param>
        private void StartListenTask(Action action)
        {
            Action<Exception> ae = (e) => { Log?.Logg(e.ToString()); };
            Task.Factory.StartNew(() => ZI.SafeExecute(action, ae), ctoken.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        #endregion Implements

        #region Message Flow Section
        public void SetRoom(int room)
        {
            AD.SetRoom(room);
        }
        public void SetNick(int pos, string name, int avatar)
        {
            AoPlayer[] aps = new AoPlayer[] { null, AD.yfPlayerR2.AoPlayer,
                AD.yfPlayerR3.AoPlayer, AD.yfPlayerO3.AoPlayer, AD.yfPlayerO2.AoPlayer,
                AD.yfPlayerO1.AoPlayer, AD.yfPlayerR1.AoPlayer };
            aps[pos].Nick = name;
            aps[pos].IsAlive = true;
            //aps[pos].SelectHero = avatar;
        }
        public void ReportNoServer(string ipAddress)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                Auxs.MessageHouse.Show("找不到远端服务器", ipAddress + "对您一开始是拒绝的。");
            }));
        }
        public void ReportWrongVersion(string version)
        {
            AD.Dispatcher.BeginInvoke((Action)(() =>
            {
                Auxs.MessageHouse.Show("PSDClientAo 版本不符", "远端服务器版本为" + version + "，请进行调整。");
            }));
        }
        // Reset All Input UI, not handled with kernel I/O work flow
        private void ResetAllInputs()
        {
            HideTip();
            AD.yfBag.Me.ResumeTux();
            AD.yfPlayerR2.AoPlayer.ResumeExCards();
            AD.yfPlayerR2.AoPlayer.ResumePets();
            AD.yfPlayerR2.AoPlayer.DisableWeapon();
            AD.yfPlayerR2.AoPlayer.DisableArmor();
            AD.yfPlayerR2.AoPlayer.DisableTrove();
            AD.yfPlayerR2.AoPlayer.DisableExEquip();
            AD.yfPlayerR2.AoPlayer.ResumeRunes();
            // Disable pets
            AD.yfJoy.CEE.ResetHightlight();
            AD.Mix.FinishSelectTarget();
            AD.ResetAllSelectedList();
        }
        #endregion Message Flow Section
        #region Detail Cin Events
        // Cin of case Y.
        internal void ShowTip(string tip) { AD.yfMinami.Minami.ShowTip(tip); }
        internal void ShowTip(string tipBase, params string[] pars)
        {
            ShowTip(string.Format(tipBase, pars));
        }
        internal void HideTip() { AD.yfMinami.Minami.HideTip(); }
        internal string CinY(ushort uid, int count, bool cancellable, params string[] names)
        {
            PreCinClearup();
            AD.yfMinami.Minami.Show(count, names);
            if (cancellable)
                AD.yfJoy.CEE.CancelValid = true;
            else
                AD.yfJoy.CEE.CancelValid = false;
            return Cin(uid);
        }
        // Invalid, only cancel
        internal string Cin00(ushort uid)
        {
            PreCinClearup();
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
            PreCinClearup();
            ShowTip("请确定。");
            AD.yfJoy.DecideMessage = "0";
            AD.yfJoy.CEE.DecideValid = true;
            string any = Cin(uid);
            if (any != CinSentinel)
                HideTip();
            return any;
        }
        // specific input
        internal string Cin48(ushort uid, string hint, params string[] pars)
        {
            PreCinClearup();
            ShowTip(hint, pars);
            string any = Cin(uid);
            if (any != CinSentinel)
                HideTip();
            return any;
        }
        internal string CinI(ushort uid, string prevComment,
            int r1, int r2, IEnumerable<string> uss, bool cancellable, bool keep)
        {
            PreCinClearup();
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
            int r1, int r2, IEnumerable<ushort> heros, bool cancellable, bool keep)
        {
            PreCinClearup();
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(heros.Select(p => "H" + p), null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }

        internal string CinC(ushort uid, string prevComment, int r1, int r2,
            List<ushort> uss, int zero, bool cancellable, bool keep)
        {
            PreCinClearup();
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
            PreCinClearup();
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
            PreCinClearup();
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
            PreCinClearup();
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
            PreCinClearup();
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
            PreCinClearup();
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
            PreCinClearup();
            ShowTip(prevComment);
            List<int> uss;
            if (r2 <= 6)
                uss = Enumerable.Range(r1, r2 - r1 + 1).ToList();
            else if (r1 > 6)
                uss = new List<int> { 7 };
            else
                uss = Enumerable.Range(r1, 8 - r1).ToList();
            AD.yfDeal.Deal.Show(uss.Select(p => "D" + p), null, 1, 1, cancellable, false);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            if (result == "7")
                result = "6+";
            return result;
        }
        internal string CinG(ushort uid, string prevComment, int r1, int r2,
            IEnumerable<ushort> dbSerials, bool cancellable, bool keep)
        {
            PreCinClearup();
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(dbSerials.Select(p => "G" + p),
                null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinG()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }
        internal string CinF(ushort uid, string prevComment, int r1, int r2,
            IEnumerable<ushort> runes, bool cancellable, bool keep)
        {
            PreCinClearup();
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(runes.Select(p => "R" + p),
                null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinF()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }
        internal string CinE(ushort uid, string prevComment, int r1, int r2,
            List<ushort> uss, bool cancellable, bool keep)
        {
            PreCinClearup();
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(uss.Select(p => "E" + p), null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinE()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
        }
        internal string CinV(ushort uid, string prevComment, int r1, int r2,
          IEnumerable<ushort> uss, bool cancellable, bool keep)
        {
            PreCinClearup();
            ShowTip(prevComment);
            AD.yfDeal.Deal.Show(uss.Select(p => "V" + p), null, r1, r2, cancellable, keep);
            string result = Cin(uid);
            if (result != CinSentinel)
                HideTip();
            return result;
        }
        internal void OCinV()
        {
            AD.yfDeal.Deal.FinishTable();
            HideTip();
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
            PreCinClearup();
            ShowTip(comment);
            List<ushort> pets = uss.Where(p => p.StartsWith("PT")).Select(
                p => ushort.Parse(p.Substring("PT".Length))).ToList();
            List<ushort> tars = uss.Where(p => p.StartsWith("T")).Select(
                p => ushort.Parse(p.Substring("T".Length))).ToList();
            List<ushort> exsps = uss.Where(p => p.StartsWith("I")).Select(
                p => ushort.Parse(p.Substring("I".Length))).ToList();
            if (tars.Count > 0)
                AD.Mix.StartSelectTarget(tars, 1, 1);
            if (pets.Count > 0)
                AD.Mix.StartSelectPT(pets, false);
            if (exsps.Count > 0)
                AD.Mix.StartSelectExsp(exsps);
            if (cancellable)
                AD.yfJoy.CEE.CancelValid = true;
            string result = Cin(uid);
            //if (result != CinSentinel)
            //{
            if (!keep)
            {
                AD.Mix.FinishSelectTarget();
                AD.Mix.FinishSelectPT();
                AD.Mix.FinishSelectExsp();
            }
            else
                AD.Mix.LockSelectTarget();
            HideTip();
            //}
            return result;
        }

        internal string CinCMD0(ushort uid)
        {
            PreCinClearup();
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
            PreCinClearup();
            ShowTip("请响应.");
            List<ushort> txs = new List<ushort>();
            List<string> njs = new List<string>();
            List<ushort> pts = new List<ushort>();
            List<ushort> sfs = new List<ushort>();
            List<ushort> yjs = new List<ushort>();
            IDictionary<string, string> prevPara = new Dictionary<string, string>();

            foreach (string ops in optLst)
            {
                int idx = ops.IndexOf(',');
                string title = Algo.Substring(ops, 0, idx);
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
                else if (title.StartsWith("FW"))
                    sfs.Add(ushort.Parse(title.Substring("FW".Length)));
                else if (title.StartsWith("YJ"))
                    yjs.Add(ushort.Parse(title.Substring("YJ".Length)));
                if (idx >= 0)
                    prevPara.Add(title, ops.Substring(idx + 1));
            }
            if (txs.Count > 0)
                AD.Mix.StartSelectTX(txs);
            if (njs.Count > 0)
            {
                IDictionary<string, string> encoding = new Dictionary<string, string>();
                foreach (string nj in njs)
                    encoding.Add(AD.Tuple.NJL.EncodeNCAction(nj).Name, nj);
                AD.yfMinami.Minami.ShowWithEncoding(njs.Count, "请选择执行NPC效果或取消.", encoding);
            }
            if (pts.Count > 0)
                AD.Mix.StartSelectPT(pts, true);
            if (sfs.Count > 0)
                AD.Mix.StartSelectSF(sfs);
            if (yjs.Count > 0)
                AD.Mix.StartSelectYJ(yjs.Select(p => Base.Card.NMBLib.CodeOfNPC(p)).ToList());
            AD.yfJoy.CEE.CancelValid = cancellable;
            string any = Cin(uid);
            if (any != CinSentinel)
            {
                AD.Mix.FinishSelectQard();
                AD.Mix.FinishSelectPT();
                AD.Mix.FinishSelectSF();
                AD.Mix.FinishSelectYJ();
                HideTip();
            }
            if (any.StartsWith("YJ"))
            {
                any = any.Substring("YJ".Length);
                int idx = any.IndexOf(",");
                if (idx < 0)
                    any = "YJ" + Base.Card.NMBLib.OriginalNPC(ushort.Parse(any));
                else
                {
                    any = "YJ" + Base.Card.NMBLib.OriginalNPC(
                        ushort.Parse(any.Substring(0, idx))) + any.Substring(idx);
                }
            }
            return any;
        }

        #endregion Detail Cin Events

        //private void SetSentialToPendingTunnel()
        //{
        //    int count = cinReqCount;
        //    for (int i = 0; i < count; ++i)
        //        cvQueues.Enqueue(CinSentinel);
        //    while (cinReqCount > 0)
        //        Thread.Sleep(50);
        //    // it should never happends, possibly add a signal here.
        //    //while (cvQueues.Count > 0 && cvQueues.Peek() == CinSentinel)
        //    //    cvQueues.Dequeue();
        //}
    }
}
