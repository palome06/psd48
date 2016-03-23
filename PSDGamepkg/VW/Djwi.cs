using System;
using System.Linq;
using PSD.Base.Utils;
using BC = System.Collections.Concurrent.BlockingCollection<string>;
using CQ = System.Collections.Concurrent.ConcurrentQueue<string>;

namespace PSD.PSDGamepkg.VW
{
    // Direct Local WI implementation for both SV and CL
    public class Djwi : Base.VW.IWISV, Base.VW.IWICL
    {
        private int Count { set; get; }
        private Log Log { set; get; }
        /// <summary>
        /// msg0Queues: message from $i to 0
        /// </summary>
        private BC[] msg0Pools;
        /// <summary>
        /// msgNQueues: message from 0 to $i
        /// </summary>
        private BC[] msgNPools;

        public Djwi(int count, Log log)
        {
            Count = count + 1;
            //msg0Queues = new Queue<string>[this.count];
            msg0Pools = new BC[Count];
            msgNPools = new BC[Count];
            for (int i = 0; i < Count; ++i)
            {
                msg0Pools[i] = new BC(new CQ());
                msgNPools[i] = new BC(new CQ());
            }
            Log = log;
        }

        #region Implemetation

        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (me == 0)
            {
                string rvDeq = msg0Pools[from].Take();
                if (!string.IsNullOrEmpty(rvDeq))
                    Log.Logger(me + "<" + from + ":" + rvDeq);
                return rvDeq;
            }
            else if (from == 0)
                return msgNPools[me].Take();
            else
                return null;
        }
        // infinite process starts
        public void RecvInfStart() { }
        // receive each message during the process
        public Base.VW.Msgs RecvInfRecv()
        {
            string msg;
            int index = BC.TakeFromAny(msg0Pools, out msg);
            if (index < Count && index > 0)
            {
                Log.Logger("0<" + index + ":" + msg);
                return new Base.VW.Msgs(msg, (ushort)index, 0, false);
            }
            else return null;
        }
        public Base.VW.Msgs RecvInfRecvPending()
        {
            return RecvInfRecv();
        }
        // infinite process ends
        public void RecvInfEnd() { }
        // reset the terminal flag to 0, start new stage
        public void RecvInfTermin() { }
        // Send raw message from $me to $to
        public void Send(string msg, ushort me, ushort to)
        {
            if (me == 0)
            {
                msgNPools[to].Add(msg);
                Log.Logger(me + ">" + to + ":" + msg);
            }
            else if (to == 0)
                msg0Pools[me].Add(msg);
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
            Send(msg, System.Linq.Enumerable.Range(1, Count - 1).Select(p => (ushort)p).ToArray());
        }
        // Send direct message that won't be caught by RecvInfRecv from $me to 0
        public void SendDirect(string msg, ushort me)
        {
            msg0Pools[me].Add(msg);
        }
        // Do not support Talk and Hear in Djwi mode
        public string Hear() { return ""; }

        public void Close() { }

        public void Dispose()
        {
            foreach (BC bc in msg0Pools)
                bc.Dispose();
            foreach (BC bc in msgNPools)
                bc.Dispose();
        }
        #endregion Implementation
    }
}
