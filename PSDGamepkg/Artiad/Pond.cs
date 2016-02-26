using PSD.Base;
using System.Linq;
using System.Collections.Generic;

namespace PSD.PSDGamepkg.Artiad
{
    public class PondRefresh
    {
        // Whether need re-check whether hit or not
        public bool CheckHit { set; get; }
        public string ToMessage() { return "G09P," + (CheckHit ? 0 : 1); }
        public static PondRefresh Parse(string line)
        {
            int idx = line.IndexOf(',');
            return new PondRefresh() { CheckHit = line.Substring(idx + 1) == "0" };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            if (CheckHit)
            {
                System.Func<Player, bool> hit = (p) => p.DEXi > 0 || (p.DEXi == 0 && p.DEX >= XI.Board.Battler.AGL);
                XI.Board.HinderSucc = hit(XI.Board.Hinder);
                XI.Board.SupportSucc = hit(XI.Board.Supporter);
                XI.Board.RDrums.Keys.ToList().ForEach(p => XI.Board.RDrums[p] = hit(p));
                XI.Board.ODrums.Keys.ToList().ForEach(p => XI.Board.ODrums[p] = hit(p));
                IDictionary<ushort, bool> drums = new Dictionary<ushort, bool>();
                XI.Board.RDrums.Keys.ToList().ForEach(p => drums[p.Uid] = XI.Board.RDrums[p]);                
                XI.Board.ODrums.Keys.ToList().ForEach(p => drums[p.Uid] = XI.Board.ODrums[p]);
                new HitRefreshSemaphore()
                {
                    Supporter = XI.Board.Supporter.Uid,
                    SHit = XI.Board.SupportSucc,
                    Hinder = XI.Board.Hinder.Uid,
                    HHit = XI.Board.HinderSucc,
                    Drums = drums
                }.Telegraph(WI.BCast);
            }
            new PondRefreshSemaphore()
            {
                RTeam = XI.Board.Rounder.Team,
                RPool = XI.Board.CalculateRPool(),
                OTeam = XI.Board.Rounder.OppTeam,
                OPool = XI.Board.CalculateOPool()
            }.Telegraph(WI.BCast);
        }
    }

    public class PondRefreshSemaphore
    {
        public int RTeam { set; get; }
        public int RPool { set; get; }
        public int OTeam { set; get; }
        public int OPool { set; get; }
        public void Telegraph(System.Action<string> send)
        {
            send("E09P,1," + RTeam + "," + RPool + "," + OTeam + "," + OPool);
        }
    }
    public class HitRefreshSemaphore
    {
        public ushort Supporter { set; get; }
        public bool SHit { set; get; }
        public ushort Hinder { set; get; }
        public bool HHit { set; get; }
        public IDictionary<ushort, bool> Drums { set; get; }     
        public void Telegraph(System.Action<string> send)
        {
            System.Func<bool, int> hitInt = (p) => p ? 1 : 0;
            List<string> msgs = new List<string>();
            msgs.Add(Supporter + "," + hitInt(SHit));
            msgs.Add(Hinder + "," + hitInt(HHit));
            msgs.AddRange(Drums.Select(p => p.Key + "," + hitInt(p.Value)));

            send("E09P,0," + string.Join(",", msgs));
        }
    }
}
