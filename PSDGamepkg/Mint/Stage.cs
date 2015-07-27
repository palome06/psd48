using PSD.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.PSDGamepkg.Mint
{
    public enum StageType { INTELLIGENT, CONSERVATIVE, WEAKER };

    public abstract class Stage : Mint
    {
        public ushort Rd { private set; get; }
        public Stage NextStage { set; get; }

        public override MintType MintType { get { return MintType.ROUND; } }
        public virtual Stage StageType { get { return StageType.INTELLIGENT; } }
        public RoundStart(ushort rd) : base() { Rd = rd; }
        public static new RoundBase Parse(string message)
        {
            return new RoundStart((ushort)(message[1] - '0'));
        }
        public override void Handle(XI xi, int priority) { if (priority == 0) Handle(xi); }
    }
    public class RoundStart : Stage
    {
        public override string Head { get { return "R" + Rd + "00"; } }
        public RoundStart(ushort ut) : base(ut) { }
        public override void Handle(XI xi)
        {
            xi.Board.Garden[Rd].ResetRAM();
            if (!xi.Board.Garden[Rd].Immobilized)
                NextStage = new RoundShift(Rd);
            else
            {
                xi.RaiseGMessage("G0QR," + rounder);
                xi.RaiseGMessage("G0DS," + rounder + ",1");
                NextStage = new RoundEnd(Rd);
            }
        }
    }
    public class RoundShift
    {
        public override string Head { get { return "R" + Rd + "OC"; } }
        public RoundStart(ushort ut) : base(ut) { NextStage = new RoundReady(Rd); }
    }
    public class RoundReady
    {
        public override string Head { get { return "R" + Rd + "ST"; } }
        public RoundReady(ushort ut) : base(ut) { NextStage = new RoundEnv(Rd); }
    }
    public class RoundEnv
    {
        public override string Head { get { return "R" + Rd + "EV"; } }
        public RoundEnv(ushort ut) : base(ut) { NextStage = new RoundSK(Rd); }
    }
    public class RoundSK
    {
        public override string Head { get { return "R" + Rd + "GR"; } }
        public override RMintType RMintType { get { return RMintType.CONSERVATIVE; } }
        public RoundSK(ushort ut) : base(ut) { }
    }
    public class RoundEnd
    {
        public override string Head { get { return "R" + Rd + "ED"; } }
        public RoundEnd(ushort ut) : base(ut) { }
    }
}
