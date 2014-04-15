using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.Card;

namespace PSD.PSDGamepkg.Artiad
{
    public class IntHelper
    {
        public static int Elem2Int(FiveElement element)
        {
            switch (element)
            {
                case FiveElement.AQUA: return 1;
                case FiveElement.AGNI: return 2;
                case FiveElement.THUNDER: return 3;
                case FiveElement.AERO: return 4;
                case FiveElement.SATURN: return 5;
                case FiveElement.YIN: return 6;
                case FiveElement.SOL: return 7;
                case FiveElement.A: return 8;
                case FiveElement.LOVE: return 9;
                //case FiveElement.DUEL: return 10;
                //case FiveElement.BONE: return 11;
                default: return 0;
            }
        }

        public static FiveElement Int2Elem(int code)
        {
            switch (code)
            {
                case 1: return FiveElement.AQUA;
                case 2: return FiveElement.AGNI;
                case 3: return FiveElement.THUNDER;
                case 4: return FiveElement.AERO;
                case 5: return FiveElement.SATURN;
                case 6: return FiveElement.YIN;
                case 7: return FiveElement.SOL;
                case 8: return FiveElement.A;
                case 9: return FiveElement.LOVE;
                //case 10: return FiveElement.DUEL;
                //case 11: return FiveElement.BONE;
                default: return FiveElement.GLOBAL;
            }
        }

        public static bool IsMaskSet(int code, GiftMask mask)
        {
            int maskCode = 0;
            switch (mask)
            {
                case GiftMask.ALIVE_DUEL: maskCode = 0x1; break;
                case GiftMask.ALIVE_PIS: maskCode = 0x2; break;
                case GiftMask.ALIVE: maskCode = 0x3; break;
                case GiftMask.STABLE: maskCode = 0x4; break;                
                case GiftMask.FROM_TUX: maskCode = 0x8; break;
                case GiftMask.TERMIN: maskCode = 0x10; break;
                case GiftMask.INCOUNTABLE: maskCode = 0x20; break;
            }
            return (code & maskCode) != 0;
        }

        public static int SetMask(int code, GiftMask mask, bool isSet)
        {
            int maskCode = 0;
            switch (mask)
            {
                case GiftMask.ALIVE_DUEL: maskCode = 0x1; break;
                case GiftMask.ALIVE_PIS: maskCode = 0x2; break;

                case GiftMask.STABLE: maskCode = 0x4; break;                
                case GiftMask.FROM_TUX: maskCode = 0x8; break;
                case GiftMask.TERMIN: maskCode = 0x10; break;
                case GiftMask.INCOUNTABLE: maskCode = 0x20; break;
            }
            if (isSet)
                code |= maskCode;
            else
                code &= (~maskCode);
            return code;
        }
    }

    public class Harm
    {
        public ushort Who { set; get; }

        public FiveElement Element { set; get; }

        public int N { set; get; }
        // Nowadays, set 0 = unknown source, 1~6 = player, 1001~ = monster/NPC(2000+)
        public int Source { set; get; }
        // Attached Property
        public int Mask { set; get; }

        public Harm(ushort who, int source, FiveElement elem, int n, int mask)
        {
            Who = who; Element = elem;
            N = n; Source = source; Mask = mask;
        }

        public static FiveElement[] GetPropedElement()
        {
            return new FiveElement[] { FiveElement.AQUA, FiveElement.AGNI,
                FiveElement.THUNDER, FiveElement.AERO, FiveElement.SATURN };
        }

        public static string ToMessage(Harm harm)
        {
            return "G0OH," + harm.Who + "," + harm.Source + "," + IntHelper.Elem2Int(harm.Element)
                + "," + harm.N + "," + harm.Mask;
        }
        public static string ToMessage(IEnumerable<Harm> harms)
        {
            string op = string.Join(",", harms.Select(p => p.Who + "," +
                    p.Source + "," + IntHelper.Elem2Int(p.Element) + "," + p.N + "," + p.Mask));
            if (!string.IsNullOrEmpty(op))
                return "G0OH," + op;
            else
                return "";
        }

        public static List<Harm> Parse(string line)
        {
            List<Harm> list = new List<Harm>();
            string[] blocks = line.Split(',');
            for (int i = 1; i < blocks.Length; i += 5)
            {
                ushort who = ushort.Parse(blocks[i]);
                int src = int.Parse(blocks[i + 1]);
                FiveElement elem = IntHelper.Int2Elem(int.Parse(blocks[i + 2]));
                int n = int.Parse(blocks[i + 3]);
                int mask = int.Parse(blocks[i + 4]);
                list.Add(new Harm(who, src, elem, n, mask));
            }
            return list;
        }
    }

    public class Cure
    {
        public ushort Who { set; get; }

        public FiveElement Element { set; get; }

        public int N { set; get; }
        // Nowadays, set 0 = unknown source, 1~6 = player, 1001~ = monster/NPC(2000+)
        public int Source { set; get; }

        public Cure(ushort who, int source, FiveElement elem, int n)
        {
            Who = who; Element = elem;
            N = n; Source = source;
        }

        public static string ToMessage(Cure cure)
        {
            return "G0IH," + cure.Who + "," + cure.Source + "," + IntHelper.Elem2Int(cure.Element) + "," + cure.N;
        }
        public static string ToMessage(IEnumerable<Cure> cures)
        {
            string op = string.Join(",", cures.Select(p => p.Who + "," +
                    p.Source + "," + IntHelper.Elem2Int(p.Element) + "," + p.N));
            if (!string.IsNullOrEmpty(op))
                return "G0IH," + op;
            else
                return "";
        }

        public static List<Cure> Parse(string line)
        {
            List<Cure> list = new List<Cure>();
            string[] blocks = line.Split(',');
            for (int i = 1; i < blocks.Length; i += 4)
            {
                ushort who = ushort.Parse(blocks[i]);
                int src = int.Parse(blocks[i + 1]);
                FiveElement elem = IntHelper.Int2Elem(int.Parse(blocks[i + 2]));
                int n = int.Parse(blocks[i + 3]);
                list.Add(new Cure(who, src, elem, n));
            }
            return list;
        }
    }
}