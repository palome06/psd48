using System.Runtime.Serialization;

namespace PSDMember
{
    [DataContract(Name = "SeatStatus")]
    public enum SeatStatus
    {
        [EnumMember]
        Closed,
        [EnumMember]
        Free,
        [EnumMember]
        Leader,
        [EnumMember]
        Prepared,
        [EnumMember]
        Gaming,
    }

    public class Seat
    {
        public SeatStatus Status { set; get; }
        public Account Account { set; get; }
    }
}