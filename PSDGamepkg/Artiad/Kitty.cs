using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.Card;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public class HarvestPet
    {
        // KOKAN:return the old one; ACTIVE:farmer make chose; PASSIVE: erase the old one directly
        public enum Treaty { NL = 0, KOKAN = 1, ACTIVE = 2, PASSIVE = 3 };
        // the one to harvest the pet
        public ushort Farmer { set; get; }
        // the one to provide the pet 
        public ushort Farmland { set; get; }
        // pets
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        // whether gain from battle, used only for trigger
        public bool Trophy { set; get; }
        // whether put the card back to whether it froms if not pick up, only valid in ACTIVE
        public bool Reposit { set; get; }
        // whether plow the farmland: trigger pet lose or put into dices action, only valid in ACTIVE
        public bool Plow { set; get; }
        // Treaty
        public Treaty TreatyAct { set; get; }

        public HarvestPet()
        {
            Trophy = false; TreatyAct = Treaty.ACTIVE;
            Farmland = 0; Reposit = false; Plow = true;
        }
        public string ToMessage()
        {
            int mask = (int)TreatyAct;
            if (Trophy) { mask |= 0x4; }
            if (Reposit) { mask |= 0x8; }
            if (Plow) { mask |= 0x10; }
            return "G0HC,0," + Farmer + "," + Farmland + "," + mask + "," + string.Join(",", Pets);
        }
        public static HarvestPet Parse(string line)
        {
            ushort[] g0hc = line.Substring("G0HC,0,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            int mask = (int)g0hc[2];
            return new HarvestPet()
            {
                Farmer = g0hc[0],
                Farmland = g0hc[1],
                Pets = Algo.TakeRange(g0hc, 3, g0hc.Length),
                Trophy = (mask & 0x4) != 0,
                Reposit = (mask & 0x8) != 0,
                Plow = (mask & 0x10) != 0,
                TreatyAct = (mask & 0x3) == 1 ? Treaty.KOKAN : ((mask & 0x3) == 2 ? Treaty.ACTIVE : Treaty.PASSIVE)
            };
        }
    }
    // trade the pet between two players
    public class TradePet
    {
        public ushort A { set; get; }
        public ushort[] AGoods { set; get; }
        public ushort ASinglePet { set { AGoods = new ushort[] { value }; } }
        public ushort B { set; get; }
        public ushort[] BGoods { set; get; }
        public ushort BSinglePet { set { BGoods = new ushort[] { value }; } }
        public string ToMessage()
        {
            return "G0HC,1," + A + "," + Algo.ListToString(AGoods.ToList()) +
                "," + B + "," + Algo.ListToString(BGoods.ToList());
        }
        public static TradePet Parse(string line)
        {
            ushort[] g0hc = line.Substring("G0HC,1,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            int index = 0;
            return new TradePet()
            {
                A = g0hc[index],
                AGoods = Algo.TakeArrayWithSize(g0hc, index + 1, out index),
                B = g0hc[index],
                BGoods = Algo.TakeArrayWithSize(g0hc, index + 1, out index)
            };
        }
    }
    // actually obtain the pet
    public class ObtainPet
    {
        // the one to harvest the pet
        public ushort Farmer { set; get; }
        // the one to provide the pet 
        public ushort Farmland { set; get; }
        // whether gain from battle
        public bool Trophy { set; get; }
        // pets
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        public ObtainPet() { Farmland = 0; Trophy = false; }
        public string ToMessage()
        {
            return "G0HD," + Farmer + "," + Farmland + "," +
                (Trophy ? 1 : 0) + "," + string.Join(",", Pets);
        }
        public static ObtainPet Parse(string line)
        {
            ushort[] g0hc = line.Substring("G0HD,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            return new ObtainPet()
            {
                Farmer = g0hc[0],
                Farmland = g0hc[1],
                Trophy = (g0hc[2] == 1),
                Pets = Algo.TakeRange(g0hc, 3, g0hc.Length)
            };
        }
    }
    // lose pet
    public class LosePet
    {
        public ushort Owner { set; get; }
        // pets
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet { set { Pets = new ushort[] { value }; } }
        // whether need to put it into piles
        public bool Recycle { set; get; }
        public LosePet() { Recycle = true; }
        public string ToMessage()
        {
            return "G0HL," + (Recycle ? 1 : 0) + "," + Owner + "," + string.Join(",", Pets);
        }
        public static LosePet Parse(string line)
        {
            ushort[] g0hl = line.Substring("G0HL,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            return new LosePet()
            {
                Owner = g0hl[1],
                Pets = Algo.TakeRange(g0hl, 2, g0hl.Length),
                Recycle = (g0hl[0] == 1)
            };
        }
    }

    public class HarvestPetSemaphore
    {
        public ushort Farmer { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        public void Telegraph(Action<string> send)
        {
            send("E0HC," + Farmer + "," + string.Join(",", Pets));
        }
    }

    public class ObtainPetSemaphore
    {
        public ushort Farmer { set; get; }
        public ushort Farmland { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        public void Telegraph(Action<string> send)
        {
            send("E0HD," + Farmer + "," + Farmland + "," + string.Join(",", Pets));
        }
    }

    public class LosePetSemaphore
    {
        public ushort Owner { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet { set { Pets = new ushort[] { value }; } }
        public void Telegraph(Action<string> send)
        {
            send("E0HL," + Owner + "," + string.Join(",", Pets));
        }
    }

    public static class KittyHelper
    {
        public static bool IsHarvest(string line) { return line.StartsWith("G0HC,0"); }
        public static bool IsTrade(string line) { return line.StartsWith("G0HC,1"); }
    }
}