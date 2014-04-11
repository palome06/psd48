using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PSD.PSDCenter
{
    public class Neayer
    {
        public string Name { private set; get; }
        public ushort Avatar { private set; get; }
        public ushort Uid { private set; get; }
        public string Ip { set; get; }

        public int HopeTeam { set; get; }

        public Socket Tunnel { set; get; }

        public bool Alive { set; get; }

        public Neayer(string name, ushort avatar, ushort uid)
        {
            Name = name; Avatar = avatar; Uid = uid;
            Alive = true;
        }
    }
    // Neayer for watcher
    public class Netcher
    {
        public string Name { private set; get; }
        public ushort Uid { private set; get; }
        public string Ip { set; get; }

        public Socket Tunnel { set; get; }
        public Netcher(string name, ushort uid)
        {
            Name = name; Uid = uid;
        }
    }
}
