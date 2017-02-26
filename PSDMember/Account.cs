using ProtoBuf;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSDMember
{

    [Serializable]
    [ProtoContract]
    public class Account
    {
        [ProtoMember(1)]
        [Key] // TODO: set key as an integer (UID?)
        public ushort UserID { set; get; }
        [NonSerialized]
        public string mPassword;
        public string Password { set { mPassword = value; } get { return mPassword; } }

        [ProtoMember(2)]
        public int Credits { set; get; }
        [ProtoMember(3)]
        public int Wins { set; get; }
        [ProtoMember(4)]
        public int Losses { set; get; }
        [ProtoMember(5)]
        public int Finishes { set; get; }
        [ProtoMember(6)]
        public int TotalGames { set; get; }
        [ProtoMember(7)]
        public string Nick { set; get; }
        // LOVE, EXP, G...

        #region Game status
        [NonSerialized] // TODO: change into a status enum (Idle, Disconnected...)
        private bool mIsDead;
        [NotMapped]
        public bool IsDead { set { mIsDead = value; } get { return mIsDead; } }
        #endregion Game Status

        [NonSerialized]
        private LoginToken mLoginToken;
        [NotMapped]
        public LoginToken LoginToken
        {
            get { return mLoginToken; }
            set { mLoginToken = value; }
        }
    }
}
