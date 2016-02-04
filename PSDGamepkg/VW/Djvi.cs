using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg.VW
{
    public class Djvi : Base.VW.IVI
    {
        private class LockableInteger
        {
            public int value;
            public LockableInteger(int value) { this.value = value; }
        }

        private int count;

        private Queue<string>[] cvQueues;
        // request count
        private LockableInteger[] cinReqCount;
        // whether enabled or not
        private bool[] cinGate;

        private Thread cinListenThread;

        private Queue<string>[] rqQueues;

        private Log Log { set; get; }

        public Djvi(int count, Log log)
        {
            this.count = count;
            this.cvQueues = new Queue<string>[this.count + 1];
            this.cinReqCount = new LockableInteger[this.count + 1];
            this.cinGate = new bool[this.count + 1];
            rqQueues = new Queue<string>[this.count + 1];
            this.Log = log;
            for (int i = 0; i < this.count + 1; ++i)
            {
                cvQueues[i] = new Queue<string>();
                cinReqCount[i] = new LockableInteger(0);
                cinGate[i] = false;
                rqQueues[i] = new Queue<string>();
            }
            //cinListenThread = new Thread(CinListenStarts);
            cinListenThread = new Thread(() => XI.SafeExecute(() => CinListenStarts(),
                delegate(Exception e) { Log.Logger(e.ToString()); }));
        }

        public void Init() { cinListenThread.Start(); }
        public void SetInGame(bool value) { }

        private void CinListenStarts()
        {
            string line;
            do
            {
                line = Console.ReadLine().Trim().ToUpper();
                Match match = Regex.Match(line, @"<\d*>.*", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int idx = line.IndexOf('>', match.Index);
                    ushort usr = ushort.Parse(Base.Utils.Algo.Substring(line, match.Index + 1, idx));
                    string content = Base.Utils.Algo.Substring(line, idx + 1, -1);
                    if (content.StartsWith("/"))
                    {
                        lock (rqQueues[usr])
                            rqQueues[usr].Enqueue(content.Substring(1));
                    }
                    else
                    {
                        //lock (cinGate)
                        //{
                        if (cinGate[usr])
                        {
                            lock (cvQueues)
                            {
                                cvQueues[usr].Enqueue(content);
                            }
                        }
                        //}
                    }
                    ++shuzi;
                }
            } while (line != null);
        }

        private int shuzi = 0;

        private void FCout(ushort me, string msg)
        {
            //if (me == 0 || me == 1 || me == 2)
            //if (me != 5 && me != 6)
            Console.WriteLine("{" + me + "}" + msg);
        }
        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            FCout(me, string.Format(msgFormat, args));
        }
        private string FCin(ushort me, string hint)
        {
            Console.WriteLine("===> {" + me + "}" + hint);
            int count = cinReqCount[me].value;
            for (int i = 0; i < count; ++i)
                cvQueues[me].Enqueue(CinSentinel);
            while (cinReqCount[me].value > 0)
                Thread.Sleep(100);

            ++cinReqCount[me].value;
            cinGate[me] = true;
            string msg = null;
            do
            {
                lock (cvQueues[me])
                {
                    if (cvQueues[me].Count > 0)
                        msg = cvQueues[me].Dequeue();
                }
                if (msg == null)
                    Thread.Sleep(100);
            } while (msg == null);
            //if (msg != CinSentinel)
            if (cinReqCount[me].value - 1 == 0)
                cinGate[me] = false;
            --cinReqCount[me].value;
            return msg;
        }
        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            return FCin(me, string.Format(hintFormat, args));
        }

        // Open Cin Tunnel
        public void OpenCinTunnel(ushort me)
        {
            //lock (cinGate)
            //{
            //    int count = cinReqCount[me];
            //    for (int i = 0; i < count; ++i)
            //        cvQueues[me].Enqueue(CinSentinel);
            //    //while (cinReqCount[me] > 0)
            //    //{
            //    //    cvQueues[me].Enqueue(CinSentinel);
            //    //    --cinReqCount[me];
            //    //}
            //    cinGate[me] = true;
            //}
        }
        // Close Cin Tunnel
        public void CloseCinTunnel(ushort me)
        {
            //lock (cinGate)
            //{
            //    cinGate[me] = false;
            //    //while (cinReqCount[me] > 0)
            //    //{
            //    //    cvQueues[me].Enqueue(CinSentinel);
            //    //    --cinReqCount[me];
            //    //}
            //    //int count = cinReqCount[me];
            //    //for (int i = 0; i < count; ++i)
            //    //    cvQueues[me].Enqueue(CinSentinel);
            //}
        }
        // Terminate Cin Tunnel, give pending Cin CinSentinel as result
        public void TerminCinTunnel(ushort me)
        {
            //int count = cinReqCount[me].value;
            //for (int i = 0; i < count; ++i)
            //    cvQueues[me].Enqueue(CinSentinel);
            //while (true)
            //{
            //    lock (cinReqCount[me])
            //    {
            //        if (cinReqCount[me].value <= 0)
            //            break;
            //    }
            //    Thread.Sleep(100);
            //}
            // TODO: not sure why it won't work, some CinCential is stucked in FCin loop.
            ResetCinTunnel(me);
        }
        // Reset Cin Tunnel, clean all pending input request
        public void ResetCinTunnel(ushort me)
        {
            lock (cinReqCount[me])
                cinReqCount[me].value = 0;
        }

        public string CinSentinel { get { return "\\"; } }

        public string Request(ushort me)
        {
            string msg = null;
            do
            {
                lock (rqQueues[me])
                {
                    if (rqQueues[me].Count > 0)
                        msg = rqQueues[me].Dequeue().ToString();
                }
                Thread.Sleep(100);
            } while (msg == null);
            return msg;
        }
        // Do not support Talk in Djvi mode
        public string RequestTalk(ushort me) { return ""; }
        public void Chat(Base.VW.Msgs msg) { }
        public void Chat(string msg, string nick) { }
    }
}
