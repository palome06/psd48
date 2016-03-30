using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PSD.PSDCenter
{
    public class Room
    {
        public int Number { private set; get; }

        public List<ushort> players;
        public List<ushort> watchers;

        public int OptTeam { set; get; }
        public int OptSel { set; get; }
        public int OptLevel { set; get; }

        public string[] Trainers { set; get; }

        private Process proc;

        private bool mReady;
        public bool Ready {
            set
            {
                mReady = value;
                if (mReady)
                    Console.WriteLine("Game starts at room {0}#.", Number);
            }
            get { return mReady; }
        }

        public Room(int number, int optTeam, int optSel, int optLevel, string[] trainers)
        {
            Number = number;
            players = new List<ushort>();
            watchers = new List<ushort>();
            OptTeam = optTeam; OptSel = optSel; OptLevel = optLevel;
            Ready = false;
            // Ps = null;
            if (trainers == null)
                Trainers = new string[0];
            else if (trainers != null && trainers.Length > 6)
                Trainers = trainers.Take(6).ToArray();
            else
                Trainers = trainers;
            Trainers = Trainers.Select(p => p.Replace(" ", "").ToUpper()).Where(p => p != "").ToArray();
        }

        public string ConvToString()
        {
            return Number + " " + OptTeam + "," + OptSel + "," + OptLevel + " " +
                (Trainers == null || Trainers.Length == 0 ? "^" : (string.Join(",", Trainers)));
        }

        public void CreateRoomPkg()
        {
            string ag = "1 " + ConvToString() + " " + string.Join(",", players);
            //string pg = "psd48pipe" + Number;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                proc = Process.Start(new ProcessStartInfo("PSDGamepkg.exe", ag) { UseShellExecute = true });
            });
        }
        public void CloseRoomPkg()
        {
            if (proc != null)
                proc.Kill();
        }
    }
}
