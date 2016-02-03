using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.PSDGamepkg.JNS
{
    public abstract class JNSBase
    {
        protected Base.VW.IVI VI { private set; get; }
        //private VW.IWI WI { private set; get; }
        protected XI XI { private set; get; }

        public JNSBase(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }


        #region Skill Util

        protected static bool Equal(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;
            else if (obj1 == null && obj2 != null)
                return false;
            else if (obj1 != null && obj2 == null)
                return false;
            else
                return obj1.Equals(obj2);
        }

        protected bool IsMathISOS(string skillName, Player player, string fuse)
        {
            string[] parts = fuse.Split(',');
            if (parts[1] == player.Uid.ToString())
            {
                for (int i = 3; i < parts.Length; ++i)
                    if (parts[i] == skillName)
                        return true;
            }
            return false;
        }
        protected void Harm(Player src, Player py, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            if (src != null)
                TargetPlayer(src.Uid, py.Uid);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                new Artiad.Harm(py.Uid, src == null ? 0 : src.Uid, five, n, mask)));
        }

        protected void Harm(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            if (invs.Any())
            {
                if (src != null)
                    TargetPlayer(src.Uid, invs.Select(p => p.Uid));
                XI.RaiseGMessage(Artiad.Harm.ToMessage(invs.Select(p =>
                    new Artiad.Harm(p.Uid, src == null ? 0 : src.Uid, five, n, mask))));
            }
        }

        protected void Harm(Player src, IEnumerable<Player> invs,
            IEnumerable<int> ns, FiveElement five = FiveElement.A, long mask = 0)
        {
            if (invs.Any())
            {
                if (src != null)
                    TargetPlayer(src.Uid, invs.Select(p => p.Uid));
                List<Player> linvs = invs.ToList();
                List<int> lns = ns.ToList();
                int sz = linvs.Count;
                XI.RaiseGMessage(Artiad.Harm.ToMessage(Enumerable.Range(0, sz).Select(p =>
                    new Artiad.Harm(linvs[p].Uid, src == null ? 0 : src.Uid, five, lns[p], mask))));
            }
        }

        protected void Cure(Player src, Player py, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            if (src != null)
                TargetPlayer(src.Uid, py.Uid);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                new Artiad.Cure(py.Uid, src == null ? 0 : src.Uid, five, n, mask)));
        }

        protected void Cure(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A, long mask = 0)
        {
            if (invs.Any())
            {
                if (src != null)
                    TargetPlayer(src.Uid, invs.Select(p => p.Uid));
                XI.RaiseGMessage(Artiad.Cure.ToMessage(invs.Select(p => new Artiad.Cure(
                    p.Uid, src == null ? 0 : src.Uid, five, n, mask))));
            }
        }

        protected void Cure(Player src, IEnumerable<Player> invs,
            IEnumerable<int> ns, FiveElement five = FiveElement.A, long mask = 0)
        {
            if (invs.Any())
            {
                if (src != null)
                    TargetPlayer(src.Uid, invs.Select(p => p.Uid));
                List<Player> linvs = invs.ToList();
                List<int> lns = ns.ToList();
                int sz = linvs.Count;
                XI.RaiseGMessage(Artiad.Cure.ToMessage(Enumerable.Range(0, sz).Select(p =>
                    new Artiad.Cure(linvs[p].Uid, src == null ? 0 : src.Uid, five, lns[p], mask))));
            }
        }
        protected void TargetPlayer(ushort from, ushort to)
        {
            if (to != 0)
                XI.RaiseGMessage("G2YS,T," + from + ",T," + to);
        }
        protected void TargetPlayer(ushort from, IEnumerable<ushort> tos)
        {
            List<ushort> to = tos.Where(p => p != 0 && p < 1000).ToList();
            if (to.Count > 0)
                XI.RaiseGMessage("G2YS,T," + from + "," + string.Join(",", to.Select(p => "T," + p)));
        }

        protected string AffichePlayers(Func<Player, bool> condition, Func<Player, string> output)
        {
            return string.Join(",", XI.Board.Garden.Values.Where(p => condition(p)).Select(p => output(p)));
        }
        protected string FormatPlayers(Func<Player, bool> condition)
        {
            string mid = string.Join("p", XI.Board.Garden.Values.Where(
                p => condition(p)).Select(p => p.Uid));
            return string.IsNullOrEmpty(mid) ? "" : ("(p" + mid + ")");
        }

        protected string AOthers(Player py) { return FormatPlayers(p => p.IsAlive && p.Uid != py.Uid); }
        protected string AOthersTared(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Uid != py.Uid).Select(p => p.Uid)) + ")";
        }
        protected string AAlls(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive).Select(p => p.Uid)) + ")";
        }
        protected string AAllTareds(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared).Select(p => p.Uid)) + ")";
        }
        protected string ATeammates(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == py.Team).Select(p => p.Uid)) + ")";
        }
        protected string ATeammatesTared(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Team == py.Team).Select(p => p.Uid)) + ")";
        }
        protected string AEnemy(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Team == py.OppTeam).Select(p => p.Uid)) + ")";
        }
        protected string AEnemyTared(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsTared && p.Team == py.OppTeam).Select(p => p.Uid)) + ")";
        }
        protected string AnyoneAliveString()
        {
            return "T1" + AAlls(null);
        }
        protected string StdRunes()
        {
            return "(p" + string.Join("p", XI.LibTuple.RL.GetFullAppendableList()) + ")";
        }
        #endregion Skill Util
    }
}
