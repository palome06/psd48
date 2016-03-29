using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PSD.Base;
using System.IO;
using PSD.Base.Utils;
using System.Runtime.CompilerServices;

namespace PSD.PSDGamepkg.VW
{
    // Server In it
    public class Aywi : Base.VW.IWISV
    {
        public const int MSG_SIZE = 4096;
        // token to terminate all running thread when Aywi is closed
        private CancellationTokenSource ctoken;

        private TcpListener listener;
        private int port; // actual port number taken room int consideration

        // network stream to center, to replace Pipe
        private NetworkStream cns;
        // Action to block all U/V inputs
        public Action OnBlockUVInputs { private set; get; }

        private IDictionary<ushort, Neayer> neayers;
        private IDictionary<ushort, Netcher> netchers;
        private Base.VW.IVI vi;
        // queue to store update news in waiting stage, "AUid,Uid" pair
        private ConcurrentQueue<string> updateNeayersInWaiting;
        private bool isRecvBlocked = false;
        // indicate whether the room is hanged up, thus blocking all recv action
        public bool IsHangedUp
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set { isRecvBlocked = value; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return isRecvBlocked; }
        }

        // only used in direct mode
        private ushort curCount = 1;
        private ushort watchCount = 1001;

        private Log Log { set; get; }

        #region Network of Player
        // n1 is a temporary version of neayers (uid not rearranged), netchers is usable
        private void ConnectDo(Socket socket, List<ushort> valids, IDictionary<ushort, Neayer> n1)
        {
            NetworkStream ns = new NetworkStream(socket);
            string data = ReadByteLine(ns);
            //string addr = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            if (data == null) { return; }
            else if (data.StartsWith("C2CO,"))
            {
                string[] blocks = data.Split(',');
                ushort ut = ushort.Parse(blocks[1]);
                string uname = blocks[2];
                ushort uava = ushort.Parse(blocks[3]);
                int uHope = int.Parse(blocks[4]);
                //if (uHope == Base.Rules.RuleCode.HOPE_AKA)
                //    uHope = 1;
                //else if (uHope == Base.Rules.RuleCode.HOPE_AO)
                //    uHope = 2;
                //else if (uHope == Base.Rules.RuleCode.HOPE_IP)
                //{
                //    if (addrDict.ContainsKey(addr))
                //        uHope = addrDict[addr];
                //    else
                //        addrDict.Add(addr, uHope = addrRank++);
                //}
                //else
                //    uHope = 0;

                if (n1 == null)
                    SentByteLine(ns, "C2CN,0");

                if (valids != null)
                {
                    if (valids.Contains(ut) && GetAliveNeayersCount() < playerCapacity)
                    {
                        Neayer ny = new Neayer(uname, uava)
                        {
                            Uid = ut, // actual Uid isn't set yet
                            AUid = ut,
                            HopeTeam = uHope,
                            Tunnel = socket
                        };
                        n1.Add(ut, ny);
                        vi.Cout(0, "[{0}]{1} joined.", ny.AUid, uname);
                        SentByteLine(ns, "C2CN," + ny.AUid);
                    }
                    else
                        SentByteLine(ns, "C2CN,0");
                }
                else // In Direct mode, exit isn't allowed, AUid isn't useful.
                {
                    ushort allocUid = (ushort)(curCount++);
                    Neayer ny = new Neayer(uname, uava)
                    {
                        Uid = allocUid,
                        AUid = allocUid,
                        HopeTeam = uHope,
                        Tunnel = socket
                    };
                    string c2rm = "";
                    string c2nw = "C2NW," + ny.Uid + "," + ny.Name + "," + ny.Avatar;
                    foreach (Neayer nyr in n1.Values)
                    {
                        c2rm += "," + nyr.Uid + "," + nyr.Name + "," + nyr.Avatar;
                        SentByteLine(new NetworkStream(nyr.Tunnel), c2nw);
                    }
                    n1.Add(ny.Uid, ny);
                    vi.Cout(0, "[{0}]{1} joined.", ny.Uid, uname);
                    SentByteLine(ns, "C2CN," + ny.Uid);
                    if (c2rm.Length > 0)
                        SentByteLine(ns, "C2RM" + c2rm);
                }
            }
            else if (data.StartsWith("C2QI,"))
            {
                string[] blocks = data.Split(',');
                ushort ut = (valids != null) ? ushort.Parse(blocks[1]) : watchCount++;
                string uname = blocks[2];
                while (netchers.ContainsKey(ut))
                    ++ut;
                Netcher nc = new Netcher(uname, ut) { Tunnel = socket }; 
                netchers.Add(ut, nc);
                SentByteLine(ns, "C2QJ," + ut);
            }
        }
        public void TcpListenerStart()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }
        // Hall path of construct Aywi, successors is the list player to join, null when indirect
        public IDictionary<ushort, Player> Connect(Base.VW.IVI vi, bool selTeam, List<ushort> valids)
        {
            IDictionary<ushort, Neayer> n1 = new Dictionary<ushort, Neayer>();
            neayers = new Dictionary<ushort, Neayer>();
            netchers = new Dictionary<ushort, Netcher>();
            this.vi = vi;

            while (n1.Count < playerCapacity)
            {
                Socket socket = listener.AcceptSocket();
                try { ConnectDo(socket, valids, n1); } // no leave allowed.
                catch (SocketException) { }
                //Thread.Sleep(50);
            }
            IDictionary<ushort, ushort> cgmap = Rearrange(selTeam, n1);
            IDictionary<ushort, Player> newGarden = new Dictionary<ushort, Player>();
            neayers = new Dictionary<ushort, Neayer>();
            foreach (var pair in n1)
            {
                Neayer ny = pair.Value;
                ushort ut = cgmap[pair.Key];
                Player player = new Player(ny.Name, ny.Avatar, ut)
                {
                    Team = (ut % 2 == 0) ? 2 : 1,
                    IsAlive = true,
                    AUid = pair.Key
                };
                ny.Uid = ut;
                newGarden.Add(ut, player);
                neayers.Add(ut, ny);
            }
            foreach (var pair in neayers)
            {
                Task.Factory.StartNew(() => XI.SafeExecute(() => KeepOnListenRecv(pair.Value),
                    delegate(Exception e) { if (Log != null) { Log.Logger(e.ToString()); } }), ctoken.Token);
                SentByteLine(new NetworkStream(pair.Value.Tunnel), "C2SA,0");
            }
            foreach (var pair in netchers)
            {
                Task.Factory.StartNew(() => XI.SafeExecute(() => KeepOnListenRecv(pair.Value),
                    delegate(Exception e) { if (Log != null) { Log.Logger(e.ToString()); } }), ctoken.Token);
                SentByteLine(new NetworkStream(pair.Value.Tunnel), "C2SA,0");
            }
            Task.Factory.StartNew(() => XI.SafeExecute(() => KeepOnListenSend(),
                delegate(Exception e) { if (Log != null) { Log.Logger(e.ToString()); } }), ctoken.Token);

            //foreach (var pair in neayers)
            //    SentByteLine(new NetworkStream(pair.Value.Tunnel), "C2AS,0");
            //foreach (var pair in netchers)
            //    SentByteLine(new NetworkStream(pair.Value.Tunnel), "C2AS,0");

            //watchReceptionThread = new Thread(() => Algo.SafeExecute(() =>
            //{
            //    Socket socket = listener.AcceptSocket();
            //    try { ConnectDo(socket, valids, n1); }
            //    catch (SocketException) { }
            //    Thread.Sleep(50);
            //},
            //  delegate(Exception e) { if (Log != null) { Log.Logger(e.ToString()); } }));
            //watchReceptionThread.Start();
            return newGarden;
        }
        private IDictionary<ushort, ushort> Rearrange(bool selTeam, IDictionary<ushort, Neayer> n1)
        {
            IDictionary<ushort, ushort> uidMap = new Dictionary<ushort, ushort>();
            if (selTeam)
            {
                List<int> range = new List<int>(Enumerable.Range(1, playerCapacity));
                List<int> team1 = range.Where(p => p % 2 == 1).ToList();
                team1.Shuffle();
                List<int> team2 = range.Where(p => p % 2 == 0).ToList();
                team2.Shuffle();
                Random random = new Random();
                if (random.Next() % 2 == 0)
                {
                    List<int> tmp = team1; team1 = team2; team2 = tmp;
                }

                List<ushort> hope1 = new List<ushort>();
                List<ushort> hope2 = new List<ushort>();
                List<ushort> hope0 = new List<ushort>();

                IDictionary<string, List<ushort>> dictCount = new Dictionary<string, List<ushort>>();
                foreach (var pair in n1)
                {
                    Neayer ny = pair.Value;
                    string name = "";
                    if (ny.HopeTeam == Base.Rules.RuleCode.HOPE_AKA)
                        name = "AKA";
                    else if (ny.HopeTeam == Base.Rules.RuleCode.HOPE_AO)
                        name = "AO";
                    else if (ny.HopeTeam == Base.Rules.RuleCode.HOPE_IP)
                        name = (ny.Tunnel.RemoteEndPoint as IPEndPoint).Address.ToString();
                    else
                        hope0.Add(ny.Uid);
                    if (name != "")
                    {
                        if (!dictCount.ContainsKey(name))
                            dictCount.Add(name, new List<ushort>());
                        dictCount[name].Add(ny.Uid);
                    }
                }
                //List<List<ushort>> rests = dictCount.Values.Where(p => p.Count > 1).ToList();
                var pq = new DS.PriorityQueue<List<ushort>>(new DS.ListSizeComparer<ushort>());
                foreach (var list in dictCount.Values)
                {
                    if (list.Count > 1)
                        pq.Push(list);
                    else if (list.Count == 1)
                        hope0.Add(list[0]);
                }

                while (pq.Count > 0)
                {
                    List<ushort> list = pq.Pop();
                    int a = hope1.Count, b = hope2.Count;
                    if (a + list.Count <= b)
                        hope1.AddRange(list);
                    else if (b + list.Count <= a)
                        hope2.AddRange(list);
                    else if (a <= b)
                        hope1.AddRange(list);
                    else
                        hope2.AddRange(list);
                }
                //foreach (var pair in n1)
                //{
                //    Neayer ny = pair.Value;
                //    if (ny.HopeTeam == 1)
                //        hope1.Add(ny.Uid);
                //    else if (ny.HopeTeam == 2)
                //        hope2.Add(ny.Uid);
                //    else
                //        hope0.Add(ny.Uid);
                //}
                hope1.Shuffle();
                hope2.Shuffle();
                hope0.Shuffle();

                int dif1 = hope1.Count - team1.Count;
                if (dif1 > 0)
                {
                    var removes = hope1.Take(dif1).ToList();
                    hope1.RemoveAll(p => removes.Contains(p));
                    hope0.AddRange(removes);
                }
                else if (dif1 < 0)
                {
                    var removes = hope0.Take(-dif1).ToList();
                    hope0.RemoveAll(p => removes.Contains(p));
                    hope1.AddRange(removes);
                }
                for (int i = 0; i < hope1.Count; ++i)
                    uidMap.Add(hope1[i], (ushort)team1[i]);
                int dif2 = hope2.Count - team2.Count;
                if (dif2 > 0)
                {
                    var removes = hope2.Take(dif2).ToList();
                    hope2.RemoveAll(p => removes.Contains(p));
                    hope0.AddRange(removes);
                }
                else if (dif2 < 0)
                {
                    var removes = hope0.Take(-dif2).ToList();
                    hope0.RemoveAll(p => removes.Contains(p));
                    hope2.AddRange(removes);
                }
                for (int i = 0; i < hope2.Count; ++i)
                    uidMap.Add(hope2[i], (ushort)team2[i]);
            }
            else
            {
                List<int> uidList = new List<int>(Enumerable.Range(1, playerCapacity));
                uidList.Shuffle();
                int i = 0;
                foreach (var pair in n1)
                    uidMap.Add(pair.Key, (ushort)uidList[i++]);
            }
            return uidMap;
        }
		// Catch new room comer, containing watcher and reconnector
		public ushort CatchNewRoomComer()
        {
            if (listener != null)
            {
                Socket socket = listener.AcceptSocket();
                NetworkStream ns = new NetworkStream(socket);
                string data = ReadByteLine(ns);
                if (data == null) { return 0; }
                else if (data.StartsWith("C2QI,")) // Watcher case
                {
                    string[] blocks = data.Split(',');
                    ushort ut = ushort.Parse(blocks[1]);
                    if (ut == 0)
                        ut = watchCount++;
                    string uname = blocks[2];
                    while (netchers.ContainsKey(ut))
                        ++ut;
                    Netcher nc = new Netcher(uname, ut) { Tunnel = socket };
                    netchers.Add(ut, nc);
                    SentByteLine(ns, "C2QJ," + ut);
                    SentByteLine(ns, "C2SA,0");
                    return ut;
                }
                else if (data.StartsWith("C4CR,")) // Reconnect case
                {
					// C4CR,u,i,A,cd
					string[] blocks = data.Split(',');
					ushort newUt = ushort.Parse(blocks[1]);
					ushort oldUt = ushort.Parse(blocks[2]);
					string uname = blocks[3];
					string roomPwd = blocks[4];
                    ushort gameUt = neayers.Values.Where(p => p.AUid == oldUt).First().Uid;

                    Neayer ny = new Neayer(uname, 0) // set avatar = 0, not care
                    {
                        AUid = newUt,
                        Uid = gameUt,
                        HopeTeam = 0,
                        Tunnel = socket,
                        Alive = false
                    };
                    neayers[ny.Uid] = ny;
                    Task.Factory.StartNew(() => XI.SafeExecute(() => KeepOnListenRecv(ny),
                        delegate(Exception e) { Log.Logger(e.ToString()); }), ctoken.Token);
                    SentByteLine(ns, "C4CS," + ny.Uid);
                    ny.Alive = true;
                    WakeTunnelInWaiting(ny.AUid, ny.Uid);
                    return ny.Uid;
                }
                else
                    SentByteLine(ns, "C2CN,0");
            }
            return 0;
        }
        #endregion Network of Player
        #region Communication and Tunnel

        public bool IsTalkSilence { set; get; }
        // $paruru is of type Neayer, register to the socket
        // only support neayer. Watcher -> Indirect Live Message only
        private void KeepOnListenRecv(object paruru)
        {
            Neayer ny = (Neayer)paruru;
            while (true)
            {
                string line = "";
                try
                {
                    line = ReadByteLine(new NetworkStream(ny.Tunnel));
                    if (line == null) { ny.Tunnel.Close(); OnLoseConnection(ny.Uid); break; }
                }
                catch (IOException) { OnLoseConnection(ny.Uid); break; }
                // Always return, Keep on find survivors if game not end, otherwise...
                // and stop searching for survivors after a time limit (300s)
                if (line.StartsWith("Y")) // word
                {
                    if (yMsgHandler != null)
                        yMsgHandler(line, ny.Uid);
                }
                else if (!string.IsNullOrEmpty(line) && !IsHangedUp)
                {
                    msg0Pools[ny.Uid].Add(line);
                    //Log.Logger(0 + "<" + ny.Uid + ":" + line);
                }
                else
                    Thread.Sleep(80);
            }
        }
        private void KeepOnListenSend()
        {
            while (true)
            {
                Base.VW.Msgs msg = infNMsgs.Take();
                if (msg.From == 0) // send won't be blocked
                {
                    if (neayers.ContainsKey(msg.To) && neayers[msg.To].Alive)
                    {
                        Log.Logger(0 + ">" + msg.To + ";" + msg.Msg);
                        try { SentByteLine(new NetworkStream(neayers[msg.To].Tunnel), msg.Msg); }
                        catch (IOException) { OnLoseConnection(msg.To); break; }
                    }
                    else
                        KeepOnListenSendWatcher(msg);
                }
                else
                    Thread.Sleep(80);
            }
        }
        // Watcher case, won't cause exception to notify leaves
        private bool KeepOnListenSendWatcher(Base.VW.Msgs msg)
        {
            try {
                if (netchers.ContainsKey(msg.To))
                    SentByteLine(new NetworkStream(netchers[msg.To].Tunnel), msg.Msg);
                return true;
            }
            catch (IOException)
            {
                // On Leave of watcher, don't care.
                Log.Logger("%%Watcher(" + msg.To + ") Leaves.");
                netchers.Remove(msg.To);
                if (GetAliveNeayersCount() == 0 && netchers.Count == 0)
                    Environment.Exit(0);
                return false;
            }
        }
        // Start a new thread to listen to waiting stage
        private bool StartWaitingStage()
        {
            int timeout = 0;
            do {
                string news;
                while (updateNeayersInWaiting.TryDequeue(out news))
                {
                    // Totally give up, then report and back
                    if (string.IsNullOrEmpty(news) || news == "0") { break; }
                    ushort[] uts = news.Split(',').Select(p => ushort.Parse(p)).ToArray();
                    Send("H0BK," + uts[1], neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
                    Live("H0BK," + uts[1]);
                    // Awake the neayer
                    if (neayers.ContainsKey(uts[1]))
                        neayers[uts[1]].Alive = true;
                    SentByteLine(cns, "C3RA," + uts[0]);
                    // Check whether all members has gathered.
                    if (GetAliveNeayersCount() == playerCapacity)
                    {
                        // OK, all gathered.
                        BCast("H0RK,0");
                        SentByteLine(cns, "C3RV,0");
                        IsHangedUp = false;
                        return true;
                    }
                }
                Thread.Sleep(100);
                ++timeout;
                // Warning happens at 5 minuts and 7 minuts
                // Time out time set as 8 minutes
                if (timeout == 3000 || timeout == 4200)
                {
                    int left = (4800 - timeout) / 10;
                    Send("H0WD," + left, neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
                    Live("H0WD," + left);
                }
                else if (timeout == 4800) { break; }
            } while (true);
            OnBrokenConnection();
            return false;
        }
        private void WakeTunnelInWaiting(ushort auid, ushort suid)
        {
            updateNeayersInWaiting.Enqueue(auid + "," + suid);
        }
        // lose the connection with $who, hoping to get echo and resume game
        private void OnLoseConnection(ushort who)
        {
            if (neayers.ContainsKey(who) && neayers[who].Alive)
            {
                neayers[who].Alive = false;
                if (vi != null) vi.Cout(0, "玩家{0}掉线，房间等待重连中.", who);
                Send("H0WT," + who, neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
                Live("H0WT," + who);

                // Report to Centre
                SentByteLine(cns, "C3LS," + neayers[who].AUid);
                if (GetAliveNeayersCount() == 0 && netchers.Count == 0)
                    Bye();

                if (!IsHangedUp)
                {
                    // Start Waiting thread and init news signal queue
                    IsHangedUp = true;
                    updateNeayersInWaiting = new ConcurrentQueue<string>();
                    Task.Factory.StartNew(() => XI.SafeExecute(() => StartWaitingStage(),
                        delegate(Exception e) { if (Log != null) { Log.Logger(e.ToString()); } }), ctoken.Token);
                }
            }
        }
        // completetly lose connection, then terminate the room
        private void OnBrokenConnection()
        {
            if (vi != null) vi.Cout(0, "房间严重损坏，本场游戏终结.");
            Send("H0LT,0", neayers.Where(p => p.Value.Alive).Select(p => p.Key).ToArray());
            Live("H0LT,0");
            if (GetAliveNeayersCount() == 0 && netchers.Count == 0)
                Bye();
        }
        // report to fake pipe
        public void Report(string message)
        {
            if (cns != null)
                SentByteLine(cns, message);
        }
        #endregion Communication and Tunnel

        #region Fake Pipe
        public void StartFakePipe(int roomNum)
        {
            TcpClient client = new TcpClient("127.0.0.1", Base.NetworkCode.HALL_PORT);
            NetworkStream tcpStream = client.GetStream();
            SentByteLine(tcpStream, "C3HI," + roomNum);
            cns = tcpStream;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private int GetAliveNeayersCount()
        {
            lock (neayers)
                return neayers.Values.Count(p => p.Alive);
        }
        #endregion Fake Pipe

        private const int playerCapacity = 6;
        /// <summary>
        /// msg0Pools: message from $i to 0
        /// msgNPools: message from 0 to $i
        /// </summary>
        private BlockingCollection<string>[] msg0Pools;
        // message queue of handling inf
        //private Queue<Base.VW.Msgs> inf0Msgs;
        // only for send
        private BlockingCollection<Base.VW.Msgs> infNMsgs;
        // queue for talk message, format as Y2,4,Hello
        //private Queue<Base.VW.Msgs> talkMsgs;
        // whether listen on or not
        // handler of Y message, inits from XI Instance.
        private Action<string, ushort> yMsgHandler;

        public Aywi(int port, Log log, Action<string, ushort> yHandler)
        {
            this.port = port;
            msg0Pools = new BlockingCollection<string>[playerCapacity + 1];
            for (int i = 0; i<= playerCapacity; ++i)
                msg0Pools[i] = new BlockingCollection<string>(new ConcurrentQueue<string>());
            infNMsgs = new BlockingCollection<Base.VW.Msgs>(new ConcurrentQueue<Base.VW.Msgs>());

            IsTalkSilence = false;
            this.yMsgHandler = yHandler;
            this.Log = log;
            ctoken = new CancellationTokenSource();
        }
        #region Implemetation
        // Get input result from $from to $me (require reply from $side to $me)
        public string Recv(ushort me, ushort from)
        {
            if (me == 0)
            {
                string rvDeq = msg0Pools[from].Take();
                if (rvDeq != null)
                {
                    Log.Logger("=" + from + ":" + rvDeq);
                    return rvDeq;
                }
            }
            return null;
        }
        // infinite process starts
        public void RecvInfStart() { }
        // receive each message during the process
        public Base.VW.Msgs RecvInfRecv()
        {
            string msg;
            int index = BlockingCollection<string>.TakeFromAny(msg0Pools, out msg);
            if (index <= playerCapacity && index > 0)
            {
                Log.Logger("==" + index + ":" + msg);
                return new Base.VW.Msgs(msg, (ushort)index, 0);
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
                infNMsgs.Add(new Base.VW.Msgs(msg, me, to));
        }
        // Send raw message to multiple $to
        public void Send(string msg, ushort[] tos)
        {
            foreach (ushort to in tos)
            {
                if (neayers.ContainsKey(to))
                    Send(msg, 0, to);
            }
        }
        public void Live(string msg)
        {
            foreach (ushort to in netchers.Keys)
                Send(msg, 0, to);
        }
        // Send raw message to the whole
        public void BCast(string msg)
        {
            foreach (ushort to in neayers.Keys)
                Send(msg, 0, to);
            foreach (ushort to in netchers.Keys)
                Send(msg, 0, to);
        }
        // Send direct message that won't be caught by RecvInfRecv from $me to 0
        public void SendDirect(string msg, ushort me) { }
        public void Dispose() { }
        public void Bye()
        {
            SentByteLine(cns, "C3LV,0");
            Environment.Exit(0);
        }
        #endregion Implementation

        #region Stream Utils
        // Read from Socket Tunnel
        private static string ReadByteLine(NetworkStream ns)
        {
            if (ns != null)
            {
                byte[] byte2 = new byte[2];
                try
                {
                    ns.Read(byte2, 0, 2);
                    ushort value = (ushort)((byte2[0] << 8) + byte2[1]);
                    byte[] actual = new byte[MSG_SIZE];
                    if (value > MSG_SIZE)
                        value = MSG_SIZE;
                    ns.Read(actual, 0, value);
                    return value > 0 ? Encoding.Unicode.GetString(actual, 0, value) : null;
                }
                catch (IOException) { return ""; }
            }
            else return "";
        }
        // Write into Socket Tunnel
        private static void SentByteLine(NetworkStream ns, string value)
        {
            if (ns != null)
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
        }
        #endregion Stream Utils
    }
}
