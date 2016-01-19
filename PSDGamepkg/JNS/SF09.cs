using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base;
using PSD.Base.Card;

namespace PSD.PSDGamepkg.JNS
{
    public class RuneCottage : JNSBase
    {
        public RuneCottage(XI xi, Base.VW.IVI vi) : base(xi, vi) { }
        // Action class, 0 thread receives and maintain data base.
        // Input class, player.Uid thread receives and maintain data base.
        public IDictionary<string, Rune> RegisterDelegates(RuneLib lib)
        {
            RuneCottage rc = this;
            IDictionary<string, Rune> sf01 = new Dictionary<string, Rune>();
            foreach (Rune sf in lib.Firsts)
            {
                sf01.Add(sf.Code, sf);
                string sfCode = sf.Code;
                var methodAction = rc.GetType().GetMethod(sfCode + "Action");
                if (methodAction != null)
                    sf.Action += new Rune.ActionDelegate(delegate(Player player, string fuse, string argst)
                    {
                        methodAction.Invoke(rc, new object[] { player, fuse, argst });
                    });
                var methodValid = rc.GetType().GetMethod(sfCode + "Valid");
                if (methodValid != null)
                    sf.Valid += new Rune.ValidDelegate(delegate(Player player, string fuse)
                    {
                        return (bool)methodValid.Invoke(rc, new object[] { player, fuse });
                    });
                var methodInput = rc.GetType().GetMethod(sfCode + "Input");
                if (methodInput != null)
                    sf.Input += new Rune.InputDelegate(delegate(Player player, string fuse, string prev)
                    {
                        return (string)methodInput.Invoke(rc, new object[] { player, fuse, prev });
                    });
            }
            return sf01;
        }

        public void SF01Action(Player player, string fuse, string args)
        {
            ushort side = ushort.Parse(args);
            XI.RaiseGMessage("G0IP," + side + ",2");
        }
        public string SF01Input(Player player, string fuse, string prev)
        {
            if (prev == "")
                return "S";
            else
                return "";
        }
        public bool SF02Valid(Player player, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void SF02Action(Player player, string fuse, string args)
        {
            XI.RaiseGMessage("G0IX," + player.Uid + ",1,2");
        }
        public bool SF03Valid(Player player, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => player.Uid == p.Who && p.N > 0 && p.Element.IsPropedElement() &&
                !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
        }
        public void SF03Action(Player player, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            harms.RemoveAll(p => player.Uid == p.Who && p.N > 0 && p.Element.IsPropedElement() &&
                !HPEvoMask.IMMUNE_INVAO.IsSet(p.Mask) && !HPEvoMask.DECR_INVAO.IsSet(p.Mask));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -18);
        }
        public bool SF04Valid(Player player, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return harms.Any(p => player.Uid == p.Who && p.N > 0 &&
                 p.Source != player.Uid && !HPEvoMask.TUX_INAVO.IsSet(p.Mask));
        }
        public void SF04Action(Player player, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            bool isAvoid = false;
            if (harms.Any(p => player.Uid == p.Who && p.N > 0 && p.Source != player.Uid && p.Element == FiveElement.A))
            {
                string select = XI.AsyncInput(player.Uid, "#是否抵御此伤害？##是##否,Y2", "SF04", "0");
                if (select == "1")
                    isAvoid = true;
            }
            if (isAvoid)
                harms.RemoveAll(p => player.Uid == p.Who && p.N > 0 && p.Source != player.Uid && p.Element == FiveElement.A);
            else
                XI.RaiseGMessage("G0DH," + player.Uid + ",0,1");
            Cure(player, player, 1);
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -149);
        }
        public bool SF05Valid(Player player, string fuse)
        {
            return XI.Board.IsAttendWar(player);
        }
        public void SF05Action(Player player, string fuse, string args)
        {
            XI.RaiseGMessage("G0OA," + player.Uid + ",1,2");
        }
        public bool SF06Valid(Player player, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            return player.IsAlive && harms.Any(p => p.Who == player.Uid && p.N > 0);
        }
        public void SF06Action(Player player, string fuse, string args)
        {
            Harm(null, player, 1);
            XI.RaiseGMessage("G1CK," + player.Uid + ",SF06,0");
        }
        public void SF07Action(Player player, string fuse, string args)
        {
            int dv = XI.Board.DiceValue;
            int[] vals = new int[] { -2, -1, 1, 2 }.Where(p => dv + p >= 1 && dv + p <= 6).ToArray();
            int idx = int.Parse(args) - 1;
            XI.RaiseGMessage("G0T7," + player.Uid + "," + dv + "," + (dv + vals[idx]));
        }
        public string SF07Input(Player player, string fuse, string prev)
        {
            if (prev == "")
            {
                int dv = XI.Board.DiceValue;
                int[] vals = new int[] { -2, -1, 1, 2 }.Where(p => dv + p >= 1 && dv + p <= 6).ToArray();
                return "#请选择调整的数值##" + string.Join("##", vals.Select(p =>
                    p > 0 ? ("+" + p) : p.ToString())) + ",/Y" + vals.Length;
            }
            else
                return "";
        }
    }
}
