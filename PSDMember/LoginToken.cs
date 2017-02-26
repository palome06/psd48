using ProtoBuf;
using System;

namespace PSDMember
{
    [Serializable]
    [ProtoContract]
    public struct LoginToken
    {
        [ProtoMember(1)]
        public Guid TokenString { set; get; }
    }

    public enum LoginStatus
    {
        SUCCESS = 0,
        VERSION_MISMATCH = 1,
        INCORRECT_PASSWORD = 2,
        OTHER_FAILURE = 3
    }
}
