using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.Base.Rules
{
    public class RuleCode
    {
        public const int DEF_CODE = 0x0;

        #region Team Selection
        public const int HOPE_NO = 0x2;

        public const int HOPE_YES = 0x4;
        public const int HOPE_NOTCARE = 0x5;
        public const int HOPE_AKA = 0x6;
        public const int HOPE_AO = 0x7;

        public const int HOPE_IP = 0xC;
         // Normal case
        #endregion Team Selection

        #region Mode Selection
        public const int MODE_00 = 0x1; // Supervised
        public const int MODE_CJ = 0x2; // Called
        public const int MODE_31 = 0x3; // 31
        public const int MODE_RM = 0x4; // Random
        public const int MODE_BP = 0x5; // Ban & Pick
        public const int MODE_RD = 0x6; // Round Pick
        public const int MODE_ZY = 0x7; // 3-4-4 Mode
        public const int MODE_CP = 0x8; // Couple Pick
        public const int MODE_IN = 0x9; // Inn Mode
        public const int MODE_SS = 0xA; // Official Pick/Ban Mode
        public const int MODE_NM = 0xB; // Normal 41 without change card
        public const int MODE_TC = 0xC; // 6 known and 6 unknown SS mode
        public const int MODE_CM = 0xD; // AS Captain Mode

        public static int CastMode(string name)
        {
            switch (name)
            {
                case "00": return MODE_00;
                case "CJ": return MODE_CJ;
                case "31": return MODE_31;
                case "RM": return MODE_RM;
                case "BP": return MODE_BP;
                case "RD": return MODE_RD;
                case "ZY": return MODE_ZY;
                case "CP": return MODE_CP;
                case "IN": return MODE_IN;
                case "SS": return MODE_SS;
                case "NM": return MODE_NM;
                case "TC": return MODE_TC;
                case "CM": return MODE_CM;
                default: return DEF_CODE;
            }
        }
        public static string CastMode(int mode)
        {
            switch (mode)
            {
                case MODE_00: return "00";
                case MODE_CJ: return "CJ";
                case MODE_31: return "31";
                case MODE_RM: return "RM";
                case MODE_BP: return "BP";
                case MODE_RD: return "RD";
                case MODE_ZY: return "ZY";
                case MODE_CP: return "CP";
                case MODE_IN: return "IN";
                case MODE_SS: return "SS";
                case MODE_NM: return "NM";
                case MODE_TC: return "TC";
                case MODE_CM: return "CM";
                default: return "RM";
            }
        }
        #endregion Mode Selection

        #region Package Selection
        //public const int XJ_STANDARD = 0x1; // [1] Standard
        //public const int XJ_EXT1 = 0x2; // [2] Fengmingyushi
        //public const int XJ_SP = 0x4; // [3] SP
        //public const int XJ_TR2 = 0x8; // [4] Sanshilunhui
        //public const int XJ_TR3 = 0x10; // [5] Yunlaiqiyuan
        //public const int XJ_HL = 0x20; // [6] Holiday Serial
        //public const int XJ_TR4 = 0x40; // [7] Xiaoyaohuanjing

        //public static int PKG_ALL
        //{
        //    get { return XJ_STANDARD | XJ_EXT1 | XJ_SP | XJ_TR2 | XJ_TR3 | XJ_HL | XJ_TR4; }
        //}
        //public static int PKG_STD
        //{
        //    get { return XJ_STANDARD | XJ_EXT1 | XJ_TR2 | XJ_TR3; }
        //}
        public const int LEVEL_NEW = 0x1 << 1;
        public const int LEVEL_STD = 0x2 << 1;
        public const int LEVEL_RCM = 0x3 << 1;
        public const int LEVEL_ALL = 0x4 << 1;
        public const int LEVEL_IPV = 0x5 << 1;

        public const int LEVEL_TRAIN_MASK = 0x1;
        #endregion Package Selection
    }
}
