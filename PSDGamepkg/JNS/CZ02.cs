using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base;
using PSD.Base.Card;

namespace PSD.PSDGamepkg.JNS
{
    public class OperationCottage
    {
        private Base.VW.IVI VI { set; get; }
        //private VW.IWI WI { private set; get; }
        private XI XI { set; get; }

        public OperationCottage(XI xi, Base.VW.IVI vi)
        {
            this.XI = xi; this.VI = vi;
        }
        // Action class, 0 thread receives and maintain data base.
        // Input class, player.Uid thread receives and maintain data base.
        public IDictionary<string, Operation> RegisterDelegates(OperationLib lib)
        {
            OperationCottage oc = this;
            IDictionary<string, Operation> cz01 = new Dictionary<string, Operation>();
            foreach (Operation cz in lib.Firsts)
            {
                cz01.Add(cz.Code, cz);
                string czCode = cz.Code;
                var methodAction = oc.GetType().GetMethod(czCode + "Action");
                if (methodAction != null)
                    cz.Action += new Operation.ActionDelegate(delegate(Player player, string fuse, string argst)
                    {
                        methodAction.Invoke(oc, new object[] { player, fuse, argst });
                    });
                var methodValid = oc.GetType().GetMethod(czCode + "Valid");
                if (methodValid != null)
                    cz.Valid += new Operation.ValidDelegate(delegate(Player player, string fuse)
                    {
                        return (bool)methodValid.Invoke(oc, new object[] { player, fuse });
                    });
                var methodInput = oc.GetType().GetMethod(czCode + "Input");
                if (methodInput != null)
                    cz.Input += new Operation.InputDelegate(delegate(Player player, string fuse, string prev)
                    {
                        return (string)methodInput.Invoke(oc, new object[] { player, fuse, prev });
                    });
            }
            return cz01;
        }

        public void CZ01Action(Player player, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            Tux tux = XI.LibTuple.TL.DecodeTux(card);
            if (player.Tux.Contains(card))
            {
                int price = player.GetPrice(tux.Code, false);
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                if (price > 0)
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0," + price);
            }
            else if (player.Weapon == card || player.ExEquip == card)
            {
                int price = player.GetPrice(tux.Code, true);
                XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                if (price > 0)
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0," + price);
            }
        }
        public string CZ01Input(Player player, string fuse, string prev)
        {
            if (prev != "")
                return "";
            else
            {
                var tl = XI.LibTuple.TL;
                List<ushort> goods = new List<ushort>();
                goods.AddRange(player.Tux.Where(p => player.GetPrice(tl.DecodeTux(p).Code, false) > 0));
                if (!player.WeaponDisabled)
                    goods.AddRange(player.ListOutAllEquips().Where(p => player.GetPrice(tl.DecodeTux(p).Code, true) > 0));
                return "/Q1(p" + string.Join("p", goods) + ")";
            }
        }
        public bool CZ01Valid(Player player, string fuse)
        {
            ushort who = (ushort)(fuse[fuse.IndexOf('R') + 1] - '0');
            var tl = XI.LibTuple.TL;
            return player.Uid == who && (player.Tux.Any(p => player.GetPrice(tl.DecodeTux(p).Code, false) > 0) ||
                    player.ListOutAllEquips().Any(p => player.GetPrice(tl.DecodeTux(p).Code, true) > 0));
        }

        public void CZ02Action(Player player, string fuse, string args)
        {
            XI.RaiseGMessage("G1SG,0");
            string yes = XI.AsyncInput(player.Uid, "#是否发动混战？##不发动##发动,Y2", "CZ02", "0");
            if (yes.Equals("2"))
            {
                ushort mons = XI.Board.MonPiles.Dequeue();
                XI.RaiseGMessage("G2IN,1,1");
                XI.RaiseGMessage("G0HZ," + player.Uid + "," + mons);
            }
            else
                XI.RaiseGMessage("G0HZ," + player.Uid + ",0");
        }
        public bool CZ02Valid(Player player, string fuse)
        {
            ushort who = (ushort)(fuse[fuse.IndexOf('R') + 1] - '0');
            if (player.Uid == who)
                return !XI.Board.FightTangled && XI.Board.MonPiles.Count > 0;
            else
                return false;
        }
        // Legecy code
        public void CZ03Action(Player player, string fuse, string args)
        {
            // string[] argblock = args.Split(',');
            // ushort which = ushort.Parse(argblock[0]);
            // XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(p)).Escue.Action(player, type, fuse, args);
            ushort which = ushort.Parse(args);
            if (player.Escue.Contains(which))
            {
                player.Escue.Remove(which);
                XI.RaiseGMessage("G2OL," + player.Uid + "," + which);
                XI.RaiseGMessage("G0ON," + player.Uid + ",M,1," + which);
                ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "S", "CZ03", "0"));
                XI.RaiseGMessage("G0IP," + side + ",1");
            }
        }
        public string CZ03Input(Player player, string fuse, string prev)
        {
            if (prev == "")
            {
                // List<ushort> ess = player.Escue.Where(p => XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(p))
                //     .EscueValid(player, type, fuse)).ToList();
                // return "/M1(p" + string.Join("p", ess) + ")";
                return "/M1(p" + string.Join("p", player.Escue) + ")";
            }
            else
            {
                // ushort ut = ushort.Prase(Algo.Substring(prev, 0, idx));
                // return XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(ut)).EscueInput(player, type, prev);
                return "";
            }
        }
        public bool CZ03Valid(Player player, string fuse)
        {
            return player.Escue.Count > 0;
            //return player.Escue.Any(p => XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(p)).EscueValid(player, type, fuse));
        }
        public bool CZ04Valid(Player player, string fuse)
        {
            if (player.TroveDisabled)
                return false;
            if (player.Tux.Count <= 0)
                return false;
            List<ushort> troves = new List<ushort>();
            if (player.Trove != 0) { troves.Add(player.Trove); }
            if (player.ExEquip != 0 && ((player.ExMask & 0x4) != 0)) { troves.Add(player.ExEquip); }
            troves.RemoveAll(p => !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup() ||
                !(XI.LibTuple.TL.DecodeTux(p) as TuxEqiup).IsIllusion());
            foreach (ushort trove in troves)
            {
                Illusion ill = XI.LibTuple.TL.DecodeTux(trove) as Illusion;
                string[] candidates = Artiad.ContentRule.GetIllusionResult(ill.Code);
                foreach (string tuxCode in candidates)
                {
                    TuxEqiup tue = XI.LibTuple.TL.EncodeTuxCode(tuxCode) as TuxEqiup;
                    if (tue == null)
                        continue;
                    if (XI.Board.Garden.Values.Any(p => p.ListOutAllEquips().Contains(tue.SingleEntry)))
                        continue;
                    if (player.GetSlotCapacity(tue.Type) <= player.GetCurrentEquipCount(tue.Type))
                        continue;
                    return true;
                }
            }
            return false;
        }
        public void CZ04Action(Player player, string fuse, string argst)
        {
            string[] parts = argst.Split(',');
            ushort trove = ushort.Parse(parts[0]);
            ushort callUt = ushort.Parse(parts[1]);
            ushort asDbSerial = ushort.Parse(parts[2]);

            XI.RaiseGMessage("G0QZ," + player.Uid + "," + callUt);
            TuxEqiup tue = XI.LibTuple.TL.EncodeTuxDbSerial(asDbSerial) as TuxEqiup;
            Illusion ill = XI.LibTuple.TL.DecodeTux(trove) as Illusion;
            ill.ILAS = tue.Code; // TODO: change into a standard NGT(G0UL)
            XI.RaiseGMessage(new Artiad.EqImport()
            {
                SingleUnit = new Artiad.CardAsUnit()
                {
                    Who = player.Uid,
                    Card = ill.SingleEntry,
                    CardAs = ill.ILAS
                }
            }.ToMessage());
        }
        public string CZ04Input(Player player, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> troves = new List<ushort>();
                if (player.Trove != 0) { troves.Add(player.Trove); }
                if (player.ExEquip != 0 && ((player.ExMask & 0x4) != 0)) { troves.Add(player.ExEquip); }
                troves.RemoveAll(p => !XI.LibTuple.TL.DecodeTux(p).IsTuxEqiup() ||
                    !(XI.LibTuple.TL.DecodeTux(p) as TuxEqiup).IsIllusion());
                return "#执行幻化,/Q1(p" + string.Join("p", troves) + ")";
            }
            else if (prev.IndexOf(',') < 0)
            {
                ushort trove = ushort.Parse(prev);
                Illusion ill = XI.LibTuple.TL.DecodeTux(trove) as Illusion;
                string[] candidates = Artiad.ContentRule.GetIllusionResult(ill.Code);
                List<ushort> invs = new List<ushort>();
                foreach (string tuxCode in candidates)
                {
                    TuxEqiup tue = XI.LibTuple.TL.EncodeTuxCode(tuxCode) as TuxEqiup;
                    if (tue == null)
                        continue;
                    if (XI.Board.Garden.Values.Any(p => p.ListOutAllEquips().Contains(tue.SingleEntry)))
                        continue;
                    if (player.GetSlotCapacity(tue.Type) <= player.GetCurrentEquipCount(tue.Type))
                        continue;
                    invs.Add(tue.DBSerial);
                }
                return "#幻化弃置,/Q1(p" + string.Join("p", player.Tux) +
                    "),#幻化,/G1(p" + string.Join("p", invs) + ")";
            }
            else return "";
        }
        public void CZ05Action(Player player, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            Tux tux = XI.LibTuple.TL.DecodeTux(card);
            if (XI.Board.CsEqiups.Contains(player.Uid + "," + card))
                return;
            if (player.Fakeq.ContainsKey(card))
            {
                if (player.Fakeq[card] == "TPT2" || (player.Fakeq[card] == "0" && tux.Code.Equals("TPT2")))
                {
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                    ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "#战力增加,S", "CZ05", "0"));
                    XI.RaiseGMessage("G0IP," + side + ",2");
                }
            }
        }
        public string CZ05Input(Player player, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> uts = player.Fakeq.Keys.Where(p =>
                    !XI.Board.CsEqiups.Contains(player.Uid + "," + p)).ToList();
                return "/C1(p" + string.Join("p", uts) + ")";
            }
            else
                return "";
        }
        public bool CZ05Valid(Player player, string fuse)
        {
            List<ushort> uts = player.Fakeq.Keys.Where(p =>
                !XI.Board.CsEqiups.Contains(player.Uid + "," + p)).ToList();
            return uts.Any();
        }
    }
}
