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

        private Queue<string> cvQueues;

        private Queue<string> hpQueues;

        private int cinReqCount;

        private Boolean cinGate;

        private Thread cinListenThread;

        private int shuzi = 0;

        private bool record;

        internal Log Log { set; get; }

        public Cyvi(AoDisplay ad, bool record)
        {
            //this.count = count;
            this.record = record;
            if (record)
            {
                Log = new Log(); Log.Start(312, true);
            }
            this.ad = ad;
            this.cvQueues = new Queue<string>();
            this.cinReqCount = 0;
            this.cinGate = false;
            this.hpQueues = new Queue<string>();
            cinListenThread = new Thread(() => Util.SafeExecute(() => CinListenStarts(),
                delegate(Exception e) { if (Log != null) { Log.Logger(e.ToString()); } }));
        }

        public void Init() { cinListenThread.Start(); }

        private void CinListenStarts()
        {
            string line;
            do
            {
                line = Console.ReadLine().Trim().ToUpper();
                if (line.StartsWith("/"))
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
            } while (line != null);
        }

        private void FCout(ushort me, string msg)
        {
            Console.WriteLine(msg);
            if (record)
                Log.Logger("%%" + msg);
        }
        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            FCout(me, string.Format(msgFormat, args));
        }
        private string FCin(ushort me, string hint)
        {
            Console.WriteLine("===> " + hint);
            int count = cinReqCount;
            for (int i = 0; i < count; ++i)
                cvQueues.Enqueue(CinSentinel);
            while (cinReqCount > 0)
                Thread.Sleep(50);

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

        // Open Cin Tunnel
        public void OpenCinTunnel(ushort me)
        {
            //cinGate = true;
            //lock (cinGate)
            //{
                //cinGate[me] = true;
            //}
        }
        // Close Cin Tunnel
        public void CloseCinTunnel(ushort me)
        {
            //cinGate = false;
            //while (cinReqCount > 0)
            //{
            //    cvQueues.Enqueue(CinSentinel);
            //    --cinReqCount;
            //}
            //lock (cinGate)
            //{
            //    cinGate[me] = false;
            //    while (cinReqCount[me] > 0)
            //    {
            //        cvQueues[me].Enqueue(CinSentinel);
            //        --cinReqCount[me];
            //    }
            //}
        }
        // Reset Cin Tunnel, clean all pending input request
        public void ResetCinTunnel(ushort me)
        {
            cinReqCount = 0;
        }
        public string CinSentinel { get { return "\\"; } }

        // TODO:DEBUG: only display hidden message
        public void Cout0(ushort me, string msg)
        {
            //if (me == 0 || me == 1 || me == 2)
            //    Console.WriteLine(me + "~" + msg);
        }
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
    }
}
