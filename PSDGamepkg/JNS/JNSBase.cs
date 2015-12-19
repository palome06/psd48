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
        protected void Harm(Player src, Player py, int n, FiveElement five = FiveElement.A, int mask = 0)
        {
            TargetPlayer(src.Uid, py.Uid);
            XI.RaiseGMessage(Artiad.Harm.ToMessage(
                new Artiad.Harm(py.Uid, src == null ? 0 : src.Uid, five, n, mask)));
        }

        protected void Harm(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A, int mask = 0)
        {
            TargetPlayer(src.Uid, invs.Select(p => p.Uid));
            XI.RaiseGMessage(Artiad.Harm.ToMessage(invs.Select(p =>
                new Artiad.Harm(p.Uid, src.Uid, five, n, mask))));
        }

        protected void Harm(Player src, List<Player> invs,
            List<int> ns, List<int> mask = null, FiveElement five = FiveElement.A)
        {
            TargetPlayer(src.Uid, invs.Select(p => p.Uid));
            int sz = invs.Count;
            XI.RaiseGMessage(Artiad.Harm.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Artiad.Harm(invs[p].Uid, src.Uid, five, ns[p], mask == null ? 0 : mask[p]))));
        }

        protected void Cure(Player src, Player py, int n, FiveElement five = FiveElement.A)
        {
            TargetPlayer(src.Uid, py.Uid);
            XI.RaiseGMessage(Artiad.Cure.ToMessage(
                new Artiad.Cure(py.Uid, src.Uid, five, n)));
        }

        protected void Cure(Player src, IEnumerable<Player> invs, int n, FiveElement five = FiveElement.A)
        {
            TargetPlayer(src.Uid, invs.Select(p => p.Uid));
            XI.RaiseGMessage(Artiad.Cure.ToMessage(invs.Select(p => new Artiad.Cure(
                p.Uid, src.Uid, five, n))));
        }

        protected void Cure(Player src, List<Player> invs,
            List<int> ns, FiveElement five = FiveElement.A)
        {
            TargetPlayer(src.Uid, invs.Select(p => p.Uid));
            int sz = invs.Count;
            XI.RaiseGMessage(Artiad.Cure.ToMessage(Enumerable.Range(0, sz).Select
                (p => new Artiad.Cure(invs[p].Uid, src.Uid, five, ns[p]))));
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

        protected string AOthers(Player py)
        {
            return "(p" + string.Join("p", XI.Board.Garden.Values.Where(
                p => p.IsAlive && p.Uid != py.Uid).Select(p => p.Uid)) + ")";
        }
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
        protected string AnyoneAliveString()
        {
            return "T1" + AAlls(null);
        }
        #endregion Skill Util
    }
}
