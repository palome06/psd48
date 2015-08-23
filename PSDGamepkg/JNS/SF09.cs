using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base;
using PSD.Base.Card;

namespace PSD.PSDGamepkg.JNS
{
    public class RuneCottage
    {
        private Base.VW.IVI VI { set; get; }
        //private VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public RuneCottage(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }
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
        public bool SF03Valid(Player player, string fuse)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (player.Uid == harm.Who && harm.N > 0 && Artiad.Harm.GetPropedElement().Contains(harm.Element))
                    return true;
            }
            return false;
        }
        public void SF03Action(Player player, string fuse, string args)
        {
            List<Artiad.Harm> harms = Artiad.Harm.Parse(fuse);
            List<Artiad.Harm> rvs = new List<Artiad.Harm>();
            IDictionary<ushort, List<Artiad.Harm>> dict = new Dictionary<ushort, List<Artiad.Harm>>();
            foreach (Artiad.Harm harm in harms)
            {
                Player py = XI.Board.Garden[harm.Who];
                if (player.Uid == harm.Who && Artiad.Harm.GetPropedElement().Contains(harm.Element))
                    rvs.Add(harm);
            }
            harms.RemoveAll(p => rvs.Contains(p));
            if (harms.Count > 0)
                XI.InnerGMessage(Artiad.Harm.ToMessage(harms), -17);
        }
        public void SF06Action(Player player, string fuse, string args)
        {
            int dv = XI.Board.DiceValue;
            int[] vals = new int[] { -2, -1, 1, 2 }.Where(p => dv + p >= 1 && dv + p <= 6).ToArray();
            int idx = int.Parse(args) - 1;
            XI.RaiseGMessage("G0T7," + player.Uid + "," + dv + "," + (dv + vals[idx]));
        }
        public string SF06Input(Player player, string fuse, string prev)
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
