using PSD.ClientZero.VW;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg.VW
{
    public class Djvi : Base.VW.IVI
    {
        private Ayvi[] ayvis;

        private Log Log { set; get; }

        public string CinSentinel { get { return "\\"; } }

        private CancellationTokenSource ctoken;

        public Djvi(int playerCount, Log log)
        {
            ayvis = new Ayvi[playerCount];
            for (int i = 0; i < ayvis.Length; ++i)
                ayvis[i] = new Ayvi();
            Log = log;
            ctoken = new CancellationTokenSource();
        }

        public void Init() { StartListenTask(ListenToUpstream); }

        private void ListenToUpstream()
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                line = line.Trim().ToUpper();
                Match match = Regex.Match(line, @"<\d*>.*", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int idx = line.IndexOf('>', match.Index);
                    ushort usr = ushort.Parse(Base.Utils.Algo.Substring(line, match.Index + 1, idx));
                    string content = Base.Utils.Algo.Substring(line, idx + 1, -1);
                    ayvis[usr - 1].Offer(content);
                }
            }
        }

        public void SetInGame(bool value)
        {
            foreach (Ayvi ayvi in ayvis)
                ayvi.SetInGame(value);
        }

        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            Console.WriteLine("{" + me + "}" + string.Format(msgFormat, args));
        }

        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            if (!string.IsNullOrEmpty(hintFormat))
                hintFormat = "{{" + me + "}}" + hintFormat;
            return ayvis[me - 1].Cin(me, hintFormat, args);
        }

        public void CloseCinTunnel(ushort me)
        {
            ayvis[me - 1].CloseCinTunnel(me);
        }

        public string RequestHelp(ushort me)
        {
            return ayvis[me - 1].RequestHelp(me);
        }

        // do not support pure chat here
        public string RequestTalk(ushort me) { return ""; }
        public void Chat(string msg, string nick) { }

        public void OpenCinTunnel(ushort me) { }
        public void TerminCinTunnel(ushort me) { }

        public void Close()
        {
            ctoken.Cancel(); ctoken.Dispose();
            foreach (Ayvi ayvi in ayvis)
                ayvi.Close();
        }
        /// <summary>
        /// start an async Listening task
        /// </summary>
        /// <param name="action">the acutal listen action</param>
        private void StartListenTask(Action action)
        {
            Action<Exception> ae = (e) => { if (Log != null) Log.Logger(e.ToString()); };
            Task.Factory.StartNew(() => XI.SafeExecute(action, ae), ctoken.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
