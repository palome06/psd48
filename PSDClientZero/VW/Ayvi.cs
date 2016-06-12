using System;
using PSD.Base.VW;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PSD.ClientZero.VW
{
    public class Ayvi : IVI
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

        public Ayvi()
        {
            hpQueue = new BlockingCollection<string>();
            tkQueue = new BlockingCollection<string>();
            ctoken = new CancellationTokenSource();
            curToken = null;
            cvQueue = new BlockingCollection<string>();
        }

        public void Init()
        {
            StartListenTask(() =>
            {
                string line;
                while ((line = Console.ReadLine()) != null)
                    Offer(line);
            });
        }
        // accept the line from console or other places
        public void Offer(string line)
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

        public void Chat(string msg, string nick) { Console.WriteLine("**[{0}]:{1}**", msg, nick); }

        public string Cin(ushort me, string hintFormat, params object[] args)
        {
            if (curToken != null)
            {
                curToken.Cancel();
                curToken.Dispose();
            }
            curToken = new CancellationTokenSource();
            while (cvQueue.Count > 0)
                cvQueue.Take();
            if (!string.IsNullOrEmpty(hintFormat))
                Console.WriteLine("===> " + string.Format(hintFormat, args));
            try { return cvQueue.Take(curToken.Token); }
            catch (OperationCanceledException) { return CinSentinel; }
        }

        public void CloseCinTunnel(ushort me)
        {
            CancellationTokenSource token = curToken;
            curToken = null;
            if (token != null)
            {
                token.Cancel();
                token.Dispose();
            }
        }

        public void Cout(ushort me, string msgFormat, params object[] args)
        {
            string msg = string.Format(msgFormat, args);
            Console.WriteLine(msg);
            if (Log != null)
                Log.Record(msg);
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
            Action<Exception> ae = (e) => { if (Log != null) Log.Logg(e.ToString()); };
            Task.Factory.StartNew(() => ZI.SafeExecute(action, ae), ctoken.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
