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
                int price = 0;
                if (tux.Code.Equals("JP03"))
                    price = 1;
                else if (tux.Code.Equals("WQ04"))
                    price = 2;
                if (price > 0)
                {
                    //XI.RaiseGMessage("G0OT," + player.Uid + ",1," + card);
                    XI.RaiseGMessage("G2ZU,0," + player.Uid + "," + card);
                    //XI.RaiseGMessage("G0ON," + player.Uid + ",C,1," + card);
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0," + price);
                }
            }
            else if (player.Weapon == card || player.ExEquip == card)
            {
                int price = 0;
                if (tux.Code.Equals("WQ04"))
                    price = 2;
                if (price > 0)
                {
                    //XI.RaiseGMessage("G0OT," + player.Uid + ",1," + card);
                    XI.RaiseGMessage("G2ZU,0," + player.Uid + "," + card);
                    //XI.RaiseGMessage("G0ON," + player.Uid + ",C,1," + card);
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                    XI.RaiseGMessage("G0DH," + player.Uid + ",0," + price);
                }
            }
        }
        public string CZ01Input(Player player, string fuse, string prev)
        {
            if (prev != "")
                return "";
            else {
                var lt = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p).Code.Equals("JP03")
                    || XI.LibTuple.TL.DecodeTux(p).Code.Equals("WQ04")).ToList();
                if (player.Weapon != 0 && XI.LibTuple.TL.DecodeTux(player.Weapon).Code.Equals("WQ04"))
                    lt.Add(player.Weapon);
                if (player.ExEquip != 0 && XI.LibTuple.TL.DecodeTux(player.ExEquip).Code.Equals("WQ04"))
                    lt.Add(player.ExEquip);
                return "/Q1(p" + string.Join("p", lt) + ")";
            }
        }
        public bool CZ01Valid(Player player, string fuse)
        {
            ushort who = (ushort)(fuse[fuse.IndexOf('R') + 1] - '0');
            if (player.Uid == who)
            {
                bool c1 = player.Tux.Where(p => XI.LibTuple.TL.DecodeTux(p).Code.Equals("JP03")
                        || XI.LibTuple.TL.DecodeTux(p).Code.Equals("WQ04")).Any();
                bool c2 = player.Weapon != 0 && XI.LibTuple.TL.DecodeTux(player.Weapon).Code.Equals("WQ04");
                bool c3 = player.ExEquip != 0 && XI.LibTuple.TL.DecodeTux(player.ExEquip).Code.Equals("WQ04");
                return c1 || c2 || c3;
            }
            else return false;
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
                return XI.Board.Monster2 == 0 && XI.Board.MonPiles.Count > 0;
            else
                return false;
        }

        public void CZ03Action(Player player, string fuse, string args)
        {
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
                return "/M1(p" + string.Join("p", player.Escue) + ")";
            else
                return "";
        }
        public bool CZ03Valid(Player player, string fuse)
        {
            return player.Escue.Count > 0;
        }

        public void CZ05Action(Player player, string fuse, string args)
        {
            ushort card = ushort.Parse(args);
            Tux tux = XI.LibTuple.TL.DecodeTux(card);
            if (XI.Board.CsEqiups.Contains(player.Uid + "," + card))
                return;
            if (player.Fakeq.Contains(card))
            {
                if (tux.Code.Equals("TPT2"))
                {
                    //XI.RaiseGMessage("G0OT," + player.Uid + ",1," + card);
                    XI.RaiseGMessage("G2ZU,0," + player.Uid + "," + card);
                    //XI.RaiseGMessage("G0ON," + player.Uid + ",C,1," + card);
                    XI.RaiseGMessage("G0QZ," + player.Uid + "," + card);
                    ushort side = ushort.Parse(XI.AsyncInput(player.Uid, "S", "TPT2", "0"));
                    XI.RaiseGMessage("G0IP," + side + ",2");
                }
            }
        }
        public string CZ05Input(Player player, string fuse, string prev)
        {
            if (prev == "")
            {
                List<ushort> uts = player.Fakeq.Where(p =>
                    !XI.Board.CsEqiups.Contains(player.Uid + "," + p)).ToList();
                return "/C1(p" + string.Join("p", uts) + ")";
            }
            else
                return "";
        }
        public bool CZ05Valid(Player player, string fuse)
        {
            List<ushort> uts = player.Fakeq.Where(p =>
                !XI.Board.CsEqiups.Contains(player.Uid + "," + p)).ToList();
            return uts.Any();
        }
    }
}
