using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PSDMember
{
    [DataContract(Name = "RoomGenre")]
    public enum RoomGenre
    {
        [EnumMember]
        VS,
    }
    
    [DataContract(Name = "RoomStatus")]
    public enum RoomStatus
    {
        [EnumMember]
        Waiting,
        [EnumMember]
        Gaming,
        [EnumMember]
        Pending,
        [EnumMember]
        Dead
    }

    public class Room
    {
        public object _lock;

        public ushort ID { set; get; }
        public string Title { set; get; }

        public RoomGenre Genre { set; get; }
        public RoomStatus Status { set; get; }

        public bool AllowSpectator { set; get; }
        public bool AllowChat { set; get; }

        public ushort Leader { set; get; }
        public List<Seat> Seats { private set; get; }

        public string IP { set; get; }
        public int Port { set; get; }

        public bool IsEmpty
        {
            get { return !Seats.Any(); }
        }

        public RoomSettings Settings { set; get; }

        public Room() { Seats = new List<Seat>(); }
    }

    [DataContract(Name = "RoomOperationResult")]
    public enum RoomOperationResult
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        InvalidToken = -1,
        [EnumMember]
        Full = -2,
        [EnumMember]
        Password = -3,
        [EnumMember]
        Locked = -4,
        [EnumMember]
        Invalid = -5,
        [EnumMember]
        NotAutheticated = -6,
    }
}
