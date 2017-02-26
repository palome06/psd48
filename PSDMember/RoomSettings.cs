using ProtoBuf;
using System;
using System.Collections.Generic;

namespace PSDMember
{
    [Serializable]
    [ProtoContract]
    public class RoomSettings
    {
        [ProtoMember(1)]
        public List<Account> Accounts { set; get; }
        [ProtoMember(2)]
        public int TotalPlayers { set; get; }
        [ProtoMember(3)]
        public int TimeOutLimits { set; get; }

        public RoomSettings()
        {
            Accounts = new List<Account>();
        }
    }
}