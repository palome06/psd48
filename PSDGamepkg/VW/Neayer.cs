using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PSD.PSDGamepkg.VW
{
    public class Neayer
    {
        public string Name { private set; get; }
        public ushort Avatar { private set; get; }
        // Uid used in the room (1~6)
        public ushort Uid { set; get; }
        // Uid used in Center registration
        public ushort AUid { set; get; }

        public int HopeTeam { set; get; }

        public Socket Tunnel { set; get; }

        public bool Alive { set; get; }

        public Neayer(string name, ushort avatar)
        {
            Name = name; Avatar = avatar;
            Alive = true;
        }
    }

    public class Netcher
    {
        public string Name { private set; get; }
         // Netcher doesn't need auid field in centre
        public ushort Uid { private set; get; }

        public Socket Tunnel { set; get; }
        public Netcher(string name, ushort uid)
        {
            Name = name; Uid = uid;
        }
    }
}
