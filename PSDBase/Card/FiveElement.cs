using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public enum FiveElement
    {
        A = 0,
        AQUA = 1, AGNI = 2, THUNDER = 3, AERO = 4, SATURN = 5,
        YINN = 6, SOLARIS = 7,
        // HIKARI = 8, KAGE = 9
    }

    public enum HPEvoMask
    {
        AVO_MASK = 0xF,
        TUX_INAVO = 0x1, // Tux Free
        IMMUNE_INVAO = 0x2, // Cannot Avoid Directly
        DECR_INVAO = 0x4, // Cannot get decreased
        CHAIN_INVAO = 0x8, // Cannot get increased or trigger new harms
        // SEL: 0001
        // YIN: 1101
        // SOL: 0111

        FINAL_MASK = 0x7 << 4,
        ALIVE = 0x1 << 4, ALIVE_HARD = 0x2 << 4, TERMIN_AT = 0x4 << 4,

        SRC_MASK = 0x3 << 7,
        FROM_JP = 0x1 << 7, FROM_SK = 0x2 << 7, FROM_NMB = 0x3 << 7,

        RESERVED_MASK = 0x3 << 9,
        RSV_DUEL = 0x1 << 9, RSV_WORM = 0x2 << 9,
    }

    public static class FiveElementHelper
    {
        public const int PropCount = 7;

        public const int StandardPropCount = 5;

        public static int Elem2Index(this FiveElement element)
        {
            return Elem2Int(element) - 1;
        }

        public static int Elem2Int(this FiveElement element)
        {
            return (int)element;
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
                case 6: return FiveElement.YINN;
                case 7: return FiveElement.SOLARIS;
                //case 8: return FiveElement.HIKARI;
                //case 9: return FiveElement.KAGE;
                default: return FiveElement.A;
            }
        }

        public static bool IsSet(this HPEvoMask mask, long code)
        {
            switch (mask)
            {
                case HPEvoMask.TUX_INAVO:
                case HPEvoMask.IMMUNE_INVAO:
                case HPEvoMask.DECR_INVAO:
                case HPEvoMask.CHAIN_INVAO:
                    return (code & (long)mask) == (long)mask;
                case HPEvoMask.ALIVE:
                case HPEvoMask.TERMIN_AT:
                    return (code & (long)HPEvoMask.FINAL_MASK) == (long)mask;
                case HPEvoMask.FROM_JP:
                case HPEvoMask.FROM_SK:
                case HPEvoMask.FROM_NMB:
                    return (code & (long)HPEvoMask.SRC_MASK) == (long)mask;
                case HPEvoMask.RSV_DUEL:
                case HPEvoMask.RSV_WORM:
                    return (code & (long)HPEvoMask.RESERVED_MASK) == (long)mask;
                default: return false;
            }
        }

        public static long Set(this HPEvoMask mask, long code)
        {
            long preMask = 0;
            switch (mask)
            {
                case HPEvoMask.TUX_INAVO:
                case HPEvoMask.IMMUNE_INVAO:
                case HPEvoMask.DECR_INVAO:
                case HPEvoMask.CHAIN_INVAO:
                    preMask = (long)mask; break;
                case HPEvoMask.ALIVE:
                case HPEvoMask.TERMIN_AT:
                    preMask = (long)HPEvoMask.FINAL_MASK; break;
                case HPEvoMask.FROM_JP:
                case HPEvoMask.FROM_SK:
                case HPEvoMask.FROM_NMB:
                    preMask = (long)HPEvoMask.SRC_MASK; break;
                case HPEvoMask.RSV_DUEL:
                case HPEvoMask.RSV_WORM:
                    preMask = (long)HPEvoMask.RESERVED_MASK; break;
            }
            return (code & ~preMask) | (long)mask;
        }

        public static long Reset(this HPEvoMask mask, long code)
        {
            switch (mask)
            {
                case HPEvoMask.AVO_MASK:
                case HPEvoMask.FINAL_MASK:
                case HPEvoMask.SRC_MASK:
                case HPEvoMask.RESERVED_MASK:
                    return code & ~(long)mask;
            }
            return code;
        }

        public static FiveElement[] GetStandardPropedElements()
        {
            return new FiveElement[] { FiveElement.AQUA, FiveElement.AGNI,
                FiveElement.THUNDER, FiveElement.AERO, FiveElement.SATURN };
        }
        public static bool IsStandardPropedElement(this FiveElement element)
        {
            return element == FiveElement.AQUA || element == FiveElement.AGNI ||
                element == FiveElement.THUNDER || element == FiveElement.AERO ||
                element == FiveElement.SATURN;
        }
        public static FiveElement[] GetPropedElements()
        {
            return new FiveElement[] { FiveElement.AQUA, FiveElement.AGNI,
                FiveElement.THUNDER, FiveElement.AERO, FiveElement.SATURN,
                FiveElement.YINN, FiveElement.SOLARIS };
        }
        public static bool IsPropedElement(this FiveElement element)
        {
            return element.IsStandardPropedElement() || element == FiveElement.YINN ||
                element == FiveElement.SOLARIS;
        }
    }
}
