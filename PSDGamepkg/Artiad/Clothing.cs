using System;
using System.Linq;
using PSD.Base;
using PSD.Base.Card;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public class EquipStandard
    {
        public ushort Who { set; get; }
        // the provider, source = 0 means from sky
        private ushort mSource;
        public ushort Source
        {
            set { mSource = value; }
            get { return FromSky ? (ushort)0 : mSource; }
        }
        // who is decide the put strategy
        private ushort mCoach;
        public ushort Coach
        {
            set { mCoach = value; }
            get { return mCoach == 0 ? Who : mCoach; }
        }
        // slot assign mode:
        // if empty slot exists, assign it; otherwise, remove the redundants to fullfill it
        public bool SlotAssign { set; get; }
        // Exquipments List
        public ushort[] Cards { set; get; }
        // single card case are used more common, so...
        public ushort SingleCard
        {
            set { Cards = new ushort[] { value }; }
            get { return (Cards != null && Cards.Length == 1) ? Cards[0] : (ushort)0; }
        }
        // set whether it's from sky
        public bool FromSky { set; get; }
        public EquipStandard()
        {
            SlotAssign = false;
            FromSky = false; Coach = 0;
        }
        public string ToMessage()
        {
            return "G0ZB,0," + Who + "," + Source + "," + Coach +
                "," + (SlotAssign ? 1 : 0) + "," + string.Join(",", Cards);
        }
        public static EquipStandard Parse(string line)
        {
            string[] args = line.Split(',');
            ushort[] g0zb = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToArray();
            return new EquipStandard()
            {
                Who = g0zb[0],
                Source = g0zb[1],
                Coach = g0zb[2],
                SlotAssign = g0zb[3] == 1,
                Cards = Algo.TakeRange(g0zb, 4, g0zb.Length)
            };
        }
    }

    public class EquipExCards
    {
        // the user
        public ushort Who { set; get; }
        // the provider
        private ushort mSource;
        public ushort Source
        {
            set { mSource = value; }
            get { return FromSky ? (ushort)0 : mSource; }
        }
        // Exquipments List
        public ushort[] Cards { set; get; }
        // single card, only support set operator
        public ushort SingleCard
        {
            set { Cards = new ushort[] { value }; }
        }
        // set whether it's from sky
        public bool FromSky { set; get; }
        public string ToMessage()
        {
            return "G0ZB,1," + Who + "," + Source + "," + string.Join(",", Cards);
        }
        public static EquipExCards Parse(string line)
        {
            string[] args = line.Split(',');
            ushort[] g0zb = Algo.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p)).ToArray();
            return new EquipExCards()
            {
                Who = g0zb[0],
                Source = g0zb[1],
                Cards = Algo.TakeRange(g0zb, 2, g0zb.Length)
            };
        }
    }
    // Insert into Fakeq, legecy logic only supporting single entry
    public class EquipFakeq
    {
        // the user
        public ushort Who { set; get; }
        // the provider
        public ushort Source { set; get; }
        // the card itself
        public ushort Card { set; get; }
        // what the fakeq serves as
        public string CardAs { set; get; }

        public string ToMessage()
        {
            return "G0ZB,2," + Who + "," + Source + "," + Card + "," + CardAs;
        }
        public static EquipFakeq Parse(string line)
        {
            string[] args = line.Split(',');
            string[] g0zb = Algo.TakeRange(args, 2, args.Length);
            return new EquipFakeq()
            {
                Who = ushort.Parse(g0zb[0]),
                Source = ushort.Parse(g0zb[1]),
                Card = ushort.Parse(g0zb[2]),
                CardAs = g0zb[3]
            };
        }
    }

    public class EquipSemaphore
    {
        public ushort Who { set; get; }

        public ushort Source { set; get; }

        public ClothingHelper.SlotType Slot { set; get; }

        public ushort[] Cards { set; get; }

        public ushort SingleCard
        {
            set { Cards = new ushort[] { value }; }
            get { return Cards == null || Cards.Length != 1 ? (ushort)0 : Cards[0]; }
        }
        // Optional, only called when Slot = FQ
        public string CardAs { set; get; }

        public void Telegraph(Action<string> send)
        {
            if (Slot != ClothingHelper.SlotType.FQ)
                send("E0ZB," + Who + "," + Source + "," + (long)Slot + "," + string.Join(",", Cards));
            else
                send("E0ZB," + Who + "," + Source + "," + (long)Slot + "," + Cards.Single() + "," + CardAs);
        }
    }

    public class EqSlotVariation
    {
        public ushort Who { set; get; }

        public ClothingHelper.SlotType Slot { set; get; }

        public bool Increase { set; get; }

        public string ToMessage()
        {
            return "G0ZJ," + Who + "," + (int)Slot + "," + (Increase ? 1 : 0);
        }
        public static EqSlotVariation Parse(string line)
        {
            string[] g0zj = line.Split(',');
            return new EqSlotVariation()
            {
                Who = ushort.Parse(g0zj[1]),
                Increase = g0zj[3] == "1",
                Slot = ClothingHelper.ParseSlot(ushort.Parse(g0zj[2]))
            };
        }
    }

    public class EqSlotMoveSemaphore
    {
        public ushort Who { set; get; }
        public ClothingHelper.SlotType Slot { set; get; }
        public ushort Card { set; get; }
        public void Telegraph(Action<string> send)
        {
            send("E0ZJ," + Who + "," + (int)Slot + "," + Card);
        }
    }

    public static class ClothingHelper
    {
        public enum SlotType { NL = 0, WQ = 1, FJ = 2, XB = 3, EE = 4, EX = 5, FQ = 6 };

        public static SlotType ParseSlot(ushort value)
        {
            switch (value)
            {
                case 1: return SlotType.WQ;
                case 2: return SlotType.FJ;
                case 3: return SlotType.XB;
                case 4: return SlotType.EE;
                case 5: return SlotType.EX;
                case 6: return SlotType.FQ;
                default: return SlotType.NL;
            }
        }

        public static bool IsStandard(string line) { return line.StartsWith("G0ZB,0"); }

        public static bool IsEx(string line) { return line.StartsWith("G0ZB,1"); }

        public static bool IsFakeq(string line) { return line.StartsWith("G0ZB,2"); }

        public static ushort GetWho(string line)
        {
            if (IsStandard(line)) { return EquipStandard.Parse(line).Who; }
            else if (IsEx(line)) { return EquipExCards.Parse(line).Who; }
            else if (IsFakeq(line)) { return EquipFakeq.Parse(line).Who; }
            else return 0;
        }
        public static ushort GetSource(string line)
        {
            if (IsStandard(line)) { return EquipStandard.Parse(line).Source; }
            else if (IsEx(line)) { return EquipExCards.Parse(line).Source; }
            else if (IsFakeq(line)) { return EquipFakeq.Parse(line).Source; }
            else return 0;
        }

        public static bool IsEquipable(Player player, Tux.TuxType tuxType)
        {
            if (!player.DrTuxDisabled)
            {
                if (tuxType == Tux.TuxType.WQ)
                    return (player.FyMask & 0x1) == 0;
                else if (tuxType == Tux.TuxType.FJ)
                    return (player.FyMask & 0x2) == 0;
                else if (tuxType == Tux.TuxType.XB)
                    return (player.FyMask & 0x4) == 0;
            }
            return false;
        }
        public static int GetSubstitude(Player player, Tux.TuxType tuxType,
            bool isSlotAssign, Func<string, string> input)
        {
            if (tuxType == Tux.TuxType.WQ)
                return GetSubstitude(player.Weapon, player.ExEquip, player.ExMask, player.FyMask, 0x1, isSlotAssign, input);
            else if (tuxType == Tux.TuxType.FJ)
                return GetSubstitude(player.Armor, player.ExEquip, player.ExMask, player.FyMask, 0x2, isSlotAssign, input);
            else if (tuxType == Tux.TuxType.XB)
                return GetSubstitude(player.Trove, player.ExEquip, player.ExMask, player.FyMask, 0x4, isSlotAssign, input);
            else return -1;
        }
        private static int GetSubstitude(ushort baseSlot, ushort exSlot, int exMask, int fyMask,
            int windowMask, bool isSlotAssign, Func<string, string> input)
        {
            if ((fyMask & windowMask) != 0)
                return -1;
            if ((exMask & windowMask) == 0)
                return baseSlot;

            if (isSlotAssign)
            {
                if (baseSlot == 0 || exSlot == 0)
                    return 0;
                else
                    return -1;
            }
            else
            {
                ushort[] cs = { baseSlot, exSlot };
                int cnt = cs.Count(p => p != 0);
                if (cnt == 0)
                    return 0;
                else
                {
                    string mi = cnt == 1 ? ("(取消不替换),/C1(p" + cs.First(p => p != 0) +
                        ")") : (",C1(p" + cs[0] + "p" + cs[1] + ")");
                    string sel = input("#替换" + mi);
                    if (sel.StartsWith("/"))
                        return 0;
                    else if (baseSlot.ToString() == sel)
                        return baseSlot;
                    else if (exSlot.ToString() == sel)
                        return exSlot;
                    else
                        return baseSlot;
                }
            }
        }
    }
}