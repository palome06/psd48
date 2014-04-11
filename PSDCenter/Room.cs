using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace PSD.PSDCenter
{
    public class Room
    {
        public int Number { private set; get; }

        public List<ushort> players;
        public List<ushort> watchers;

        public NamedPipeServerStream Ps { set; get; }

        public int OptTeam { set; get; }
        public int OptSel { set; get; }
        public int OptPkg { set; get; }

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

        public Room(int number, int optTeam, int optSel, int optPkg)
        {
            Number = number;
            players = new List<ushort>();
            watchers = new List<ushort>();
            OptTeam = optTeam; OptSel = optSel; OptPkg = optPkg;
            Ready = false; Ps = null;
        }

        public string ConvToString()
        {
            return Number + " " + OptTeam + "," + OptSel + "," + OptPkg;
        }

        //public static Room CreateRoom(int number, int optTeam, int optSel, int optPkg)
        //{
        //    Room room = new Room(number, optTeam, optSel, optPkg);
        //    // Start a new processes
        //}
    }
}
