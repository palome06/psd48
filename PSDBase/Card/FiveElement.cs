using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public enum FiveElement
    {
        GLOBAL = 0,
        AQUA = 1, AGNI = 2, THUNDER = 3, AERO = 4, SATURN = 5,
        YIN = 6, SOL = 7,
        A = 8,
        LOVE = 9,
        //DUEL = 10,
        //BONE = 11
    }

    public enum GiftMask
    {
        ALIVE_DUEL = 0x1, ALIVE_PIS = 0x2, // Choose

        ALIVE = 0x3,
        STABLE = 0x4, FROM_TUX = 0x8, TERMIN = 0x10, // Combined
        INCOUNTABLE = 0x20
    }
}
