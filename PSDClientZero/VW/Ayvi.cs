using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.VW;
using System.Text.RegularExpressions;
using System.Threading;

namespace PSD.ClientZero.VW
{
    public class Ayvi : IVI
    {
        // queue for command code, help and talk respectively
        private Queue<string> cvQueues, hpQueues, tkQueues;

        private int cinReqCount;

        private Boolean cinGate;

        private Thread cinListenThread;

        private int shuzi = 0;

        internal Base.Log Log { set; get; }

        public Ayvi(int count, bool record, bool msgLog)
        {
            //this.count = count;
            //Log = new Base.Log(); Log.Start(312, record, msgLog, 0);
            this.cvQueues = new Queue<string>();
            this.cinReqCount = 0;
            this.cinGate = false;
            this.hpQueues = new Queue<string>();
            this.tkQueues = new Queue<string>();
            cinListenThread = new Thread(() => Util.SafeExecute(() => CinListenStarts(),
                delegate(Exception e) { if (Log != null) Log.Logg(e.ToString()); }));
        }

        public void Init() { cinListenThread.Start(); }
        private bool InGame { set; get; }
        // Set whether in game mode, whether operations can be accepted
        public void SetInGame(bool value) { InGame = value; }

        private void CinListenStarts()
        {
            string line;
            do
            {
                line = Console.ReadLine();
                if (line.StartsWith("@@"))
                {
                    lock (tkQueues)
                        tkQueues.Enqueue("Y1," + line.Substring("@@".Length));
                }
                else
                {
                    if (InGame)
                    {
                        line = line.Trim().ToUpper();
                        if (line.StartsWith("@#"))
                        {
                            lock (tkQueues)
                                tkQueues.Enqueue("Y3," + line.Substring("@#".Length));
                        }
                        else if (line.StartsWith("/"))
                        {
                            lock (hpQueues)
                                hpQueues.Enqueue(line.Substring("/".Length));
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
                }
            } while (line != null);
        }

        private void FCout(ushort me, string msg)
        {
            Console.WriteLine(msg);
            if (Log != null) 
                Log.Record(msg);
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
        // Terminate Cin Tunnel, give pending Cin CinSentinel as result
        public void TerminCinTunnel(ushort me)
        {
            int count = cinReqCount;
            for (int i = 0; i < count; ++i)
                cvQueues.Enqueue(CinSentinel);
            while (cinReqCount > 0)
                Thread.Sleep(50);
        }
        // Reset Cin Tunnel, clean all pending input request
        public void ResetCinTunnel(ushort me) { cinReqCount = 0; }
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
        // Talk in Client
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
        //    Console.WriteLine("**" + msg.From + "#:" + msg.Msg + "**");
        //}
        public void Chat(string msg, string nick)
        {
            Console.WriteLine("**[" + nick + "]:" + msg + "**");
        }

        #region Sepcial Zone
        // Cin without prefix "===>" signal
        public string Cin48(ushort ut)
        {
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
        #endregion Special Zone
    }
}
