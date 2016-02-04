using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using PSD.Base.Utils;

namespace PSD.PSDGamepkg.VW
{
    public class Djwi : Base.VW.IWISV, Base.VW.IWICL
    {
        private int count;
        /// <summary>
        /// msg0Queues: message from $i to 0
        /// </summary>
        private Rueue<Base.VW.Msgs>[] msg0Queues;
        /// <summary>
        /// msgNQueues: message from 0 to $i
        /// </summary>
        private Queue<string>[] msgNQueues;

        //private Thread cinListenThread;
        // message queue of handling inf
        //private Queue<Base.VW.Msgs> infMsgs;
        // whether listen on or not
        private int infOn;

        private Log log;

        public Djwi(int count, Log log)
        {
            this.count = count + 1;
            //msg0Queues = new Queue<string>[this.count];
            msg0Queues = new Rueue<Base.VW.Msgs>[this.count];
            msgNQueues = new Queue<string>[this.count];
            for (int i = 0; i < this.count; ++i)
            {
                //msg0Queues[i] = new Queue<string>();
                msg0Queues[i] = new Rueue<Base.VW.Msgs>();
                msgNQueues[i] = new Queue<string>();
            }
            //cinListenThread = new Thread(CinListenStarts);
            //infMsgs = new Queue<Base.VW.Msgs>();
            infOn = 0;
            this.log = log;
        }

        private static string Substring(string @string, int start, int end) {
            return @string.Substring(start, end - start);
        }

        #region Implemetation

        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (me == 0)
            {
                string rvDeq = null;
                do 
                {
                    lock (msg0Queues[from])
                    {
                        if (msg0Queues[from].Count != 0)
                            rvDeq = msg0Queues[from].Dequeue().Msg;
                    }
                    if (rvDeq == null)
                        Thread.Sleep(100);
                } while (rvDeq == null);
                //Console.WriteLine("☆◇○" + me + "<" + from + ":" + ret + "○◇☆");
                if (rvDeq != null)
                    log.Logger(me + "<" + from + ":" + rvDeq);
                return rvDeq;
            }
            else if (from == 0)
            {
                string rvDeq = null;
                do
                {
                    lock (msgNQueues[me])
                    {
                        if (msgNQueues[me].Count != 0)
                            rvDeq = msgNQueues[me].Dequeue();
                    }
                    if (rvDeq == null)
                        Thread.Sleep(100);
                } while (rvDeq == null);
                return rvDeq;
            }
            else
                return null;
        }
        // infinite process starts
        public void RecvInfStart()
        {
            ++infOn;
            //infMsgs.Clear();
            //infOn = true;
            //while (infOn)
            //{
            //    foreach (ushort u in from)
            //    {
            //        if (msg0Queues[u].Count != 0)
            //            infMsgs.Enqueue(new Msgs(msg0Queues[u].Dequeue(), u, 0));
            //    }
            //    Thread.Sleep(200);
            //}
        }
        // receive each message during the process
        public Base.VW.Msgs RecvInfRecv()
        {
            foreach (Rueue<Base.VW.Msgs> queue in msg0Queues)
            {
                lock (queue)
                {
                    if (queue.Count > 0)
                    {
                        foreach (Base.VW.Msgs msg in queue)
                        {
                            if (!msg.Direct)
                            {
                                queue.Remove(msg);
                                log.Logger(msg.To + "<" + msg.From + ":" + msg.Msg);
                                return msg;
                            }
                        }
                    }
                }
            }
            return null;
            //if (infMsgs.Count != 0)
            //{
            //    lock (infMsgs)
            //    {
            //        Base.VW.Msgs ret = infMsgs.Dequeue();
            //        //Console.WriteLine("☆◇○" + ret.To + "<" + ret.From + ":" + ret.Msg + "○◇☆");
            //        log.Logger(ret.To + "<" + ret.From + ":" + ret.Msg);
            //        return ret;
            //    }
            //}
            //else
            //    return null;
        }
        public Base.VW.Msgs RecvInfRecvPending()
        {
            do
            {
                Base.VW.Msgs msgs = RecvInfRecv();
                if (msgs != null)
                    return msgs;
                Thread.Sleep(200);
            } while (true);
        }
        // infinite process ends
        public void RecvInfEnd()
        {
            --infOn;
            //if (infOn <= 0) { infOn = 0; infMsgs.Clear(); }
            if (infOn <= 0) { infOn = 0; }
        }
        // reset the terminal flag to 0, start new stage
        public void RecvInfTermin()
        {
            //infOn = 0; infMsgs.Clear();
            infOn = 0;
        }
        // Send raw message from $me to $to
        public void Send(string msg, ushort me, ushort to)
        {
            if (me == 0)
            {
                lock (msgNQueues[to])
                {
                    msgNQueues[to].Enqueue(msg);
                }
                //Console.WriteLine("☆◇○" + me + ">" + to + ":" + msg + "○◇☆");
                log.Logger(me + ">" + to + ":" + msg);
            }
            else if (to == 0)
            {
                //if (infOn <= 0)
                //{
                //    lock (msg0Queues)
                //    {
                //        msg0Queues[me].Enqueue(msg);
                //    }
                //}
                //else
                //{
                //    lock (infMsgs)
                //    {
                //        infMsgs.Enqueue(new Base.VW.Msgs(msg, me, to, false));
                //    }
                //}
                lock (msg0Queues[me])
                    msg0Queues[me].Enqueue(new Base.VW.Msgs(msg, me, 0, false));
            }
        }
        // Send raw message to multiple $to
        public void Send(string msg, ushort[] tos)
        {
            foreach (ushort to in tos)
                Send(msg, 0, to);
        }

        public void Live(string msg) { }
        // Send raw message to the whole
        public void BCast(string msg)
        {
            var squares = System.Linq.Enumerable.Range(1, count - 1);
            foreach (int to in squares)
                Send(msg, 0, (ushort)to);
        }
        // Send direct message that won't be caught by RecvInfRecv from $me to 0
        public void SendDirect(string msg, ushort me)
        {
            lock (msg0Queues[me])
                msg0Queues[me].Enqueue(new Base.VW.Msgs(msg, me, 0, true));
        }
        // Do not support Talk and Hear in Djwi mode
        public string Hear() { return ""; }

        public void Close() { }
        #endregion Implementation

        private static string Pkize(string msg, ushort from, ushort to)
        {
            return "<" + from + "-" + to + ">" + msg;
        }
    }
}
