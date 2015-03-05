using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public class Monster : NMB
    {
        //public enum FiveElement { GLOBAL, AQUA, AGNI, THUNDER, AERO, SATURN };
        public enum ClLevel { WOODEN, WEAK, STRONG, BOSS };

        public FiveElement Element { private set; get; }
        public ClLevel Level { private set; get; }

        // Monster name, (e.g.) Caiyi
        public string Name { private set; get; }
        // avatar = serial, (e.g.) 40404
        //public int Avatar { private set; get; }
        public string Code { private set; get; }
        // group, e.g. 1 for standard, 0 for test, 2 for SP, etc.
        public int Group { private set; get; }

        public int mSTR;
        public int STR
        {
            set { mSTR = value; }
            get { return mSTR >= 0 ? mSTR : 0; }
        }
        public ushort STRb { private set; get; }
        public int AGL { set; get; }
        public ushort AGLb { private set; get; }
        // first dimension: 0->Normal, 1->Outbrust, 2->Debut
        public SKBranch[][] EA { private set; get; }

        public ushort Padrone { set; get; }

        public string DebutText { set; get; }
        public string PetText { set; get; }
        public string WinText { set; get; }
        public string LoseText { set; get; }

        public ushort RAMUshort { set; get; }

        #region SPI Info
        private int mSpiHW, mSpiHL, mSpiHw, mSpiHl, mSpiHC, mSpiHc;
        private int mSpiTW, mSpiTL, mSpiTw, mSpiTl, mSpiTC, mSpiTc;

        // whether self/attend, whether debut result
        // whether win or not or rounder, whether teammates of rounder
        public bool IsHarmInvolved(bool self, bool attend, bool debut, bool win, bool round)
        {
            int value;
            if (debut && round) value = mSpiHC;
            else if (debut && !round) value = mSpiHc;
            else
            {
                if (win && round) value = mSpiHW;
                else if (!win && round) value = mSpiHL;
                else if (win && !round) value = mSpiHw;
                else value = mSpiHl;
            }
            if ((value & 0x1) != 0) return true;
            else if ((value & 0x2) != 0 && attend) return true;
            else if ((value & 0x4) != 0 && self) return true;
            else return false;
        }
        public bool IsHarmInvolvedTeam(bool debut, bool win, bool round)
        {
            int value;
            if (debut && round) value = mSpiHC;
            else if (debut && !round) value = mSpiHc;
            else
            {
                if (win && round) value = mSpiHW;
                else if (!win && round) value = mSpiHL;
                else if (win && !round) value = mSpiHw;
                else value = mSpiHl;
            }
            if ((value & 0x7) != 0) return true;
            else return false;
        }
        // whether self/attend, whether debut result
        // whether win or not or rounder, whether teammates of rounder
        public bool IsTuxInvolved(bool self, bool attend, bool debut, bool win, bool round)
        {
            int value;
            if (debut && round) value = mSpiTC;
            else if (debut && !round) value = mSpiTc;
            else
            {
                if (win && round) value = mSpiTW;
                else if (!win && round) value = mSpiTL;
                else if (win && !round) value = mSpiTw;
                else value = mSpiTl;
            }
            if ((value & 0x1) != 0) return true;
            else if ((value & 0x2) != 0 && attend) return true;
            else if ((value & 0x4) != 0 && self) return true;
            else return false;
        }
        public void ParseSpi(string spis)
        {
            mSpiHW = mSpiHL = mSpiHw = mSpiHl = mSpiHC = mSpiHc = 0;
            mSpiTW = mSpiTL = mSpiTw = mSpiTl = mSpiTC = mSpiTc = 0;
            for (int i = 0; i < spis.Length; )
            {
                if (i + 2 < spis.Length && spis[i + 2] == '#')
                {
                    BitOr(spis[i], spis[i + 1], 0x4);
                    i += 3;
                }
                else if (i + 2 < spis.Length && spis[i + 2] == '+')
                {
                    BitOr(spis[i], spis[i + 1], 0x2);
                    i += 3;
                }
                else
                {
                    BitOr(spis[i], spis[i + 1], 0x1);
                    i += 2;
                }
            }
        }
        private void BitOr(char ichi, char ni, int mask)
        {
            if (ichi == 'H' && ni == 'W')
                mSpiHW |= mask;
            else if (ichi == 'H' && ni == 'L')
                mSpiHL |= mask;
            else if (ichi == 'H' && ni == 'w')
                mSpiHw |= mask;
            else if (ichi == 'H' && ni == 'l')
                mSpiHl |= mask;
            else if (ichi == 'H' && ni == 'C')
                mSpiHC |= mask;
            else if (ichi == 'H' && ni == 'c')
                mSpiHc |= mask;
            else if (ichi == 'T' && ni == 'W')
                mSpiTW |= mask;
            else if (ichi == 'T' && ni == 'L')
                mSpiTL |= mask;
            else if (ichi == 'T' && ni == 'w')
                mSpiTw |= mask;
            else if (ichi == 'T' && ni == 'l')
                mSpiTl |= mask;
            else if (ichi == 'T' && ni == 'C')
                mSpiTC |= mask;
            else if (ichi == 'T' && ni == 'c')
                mSpiTc |= mask;
        }
        #endregion SPI Info

        // public Delegate Type of Handling events
        public delegate void DebutDelegate();
        public delegate void WLDelegate();
        public delegate void CrActionDelegate(Player player);
        public delegate void CsActionDelegate(Player player,
            int consumeType, int type, Fuse fuse, string argst);
        public delegate bool CsValidDelegate(Player player,
            int consumeType, int type, Fuse fuse);
        public delegate string CsInputDelegate(Player player,
            int consumeType, int type, Fuse fuse, string prev);

        private DebutDelegate mDebut, mCurtain;
        public DebutDelegate Debut
        {
            set { mDebut = value; }
            get { return mDebut ?? DefDebut; }
        }
        public DebutDelegate Curtain
        {
            set { mCurtain = value; }
            get { return mCurtain ?? DefDebut; }
        }

        private WLDelegate mWinEff, mLoseEff;
        public WLDelegate WinEff
        {
            set { mWinEff = value; }
            get { return mWinEff ?? DefWL; }
        }
        public WLDelegate LoseEff
        {
            set { mLoseEff = value; }
            get { return mLoseEff ?? DefWL; }
        }

        private CrActionDelegate mIncrAction, mDecrAction;
        public CrActionDelegate IncrAction
        {
            set { mIncrAction = value; }
            get { return mIncrAction ?? DefCrAction; }
        }
        public CrActionDelegate DecrAction
        {
            set { mDecrAction = value; }
            get { return mDecrAction ?? DefCrAction; }
        }

        private CsActionDelegate mConsumeAction;
        public CsActionDelegate ConsumeAction
        {
            set { mConsumeAction = value; }
            get { return mConsumeAction ?? DefCsAction; }
        }

        private CsInputDelegate mConsumeInput;
        public CsInputDelegate ConsumeInput
        {
            set { mConsumeInput = value; }
            get { return mConsumeInput ?? DefCsInput; }
        }

        private CsValidDelegate mConsumeValid;
        public CsValidDelegate ConsumeValid
        {
            set { mConsumeValid = value; }
            get { return mConsumeValid ?? DefCsValid; }
        }

        public Monster(string name, string code, int group, FiveElement element, ushort str, ushort agl,
             ClLevel level, string eaoccurs, string eaprops, string eamixcodes, string spis)
        {
            this.Name = name; this.Code = code; this.Group = group;
            this.Element = element; this.Level = level;
            this.STRb = strb; this.STR = this.STRb;
            this.AGLb = agl; this.AGL = this.AGLb;
            string[] eaos = eaoccurs.Split(';');
            string[] eaps = eaprops.Split(';');
            string[] eams = eamixcodes.Split(';');
            int sz = eaos.Length;
            EA = new SKBranch[sz][];
            for (int i = 0; i < sz; ++i)
                EA[i] = SKBranch.ParseFromStrings(eaos[i], eaps[i], eams[i]);
            this.Padrone = 0;
            ParseSpi(spis);
        }

        public bool IsMonster() { return true; }
        public bool IsNPC() { return false; }

        private DebutDelegate DefDebut = new DebutDelegate(delegate() { });
        private WLDelegate DefWL = new WLDelegate(delegate() { });
        private static CrActionDelegate DefCrAction = new CrActionDelegate(
            delegate(Player player) { });
        private static CsActionDelegate DefCsAction = new CsActionDelegate(
            delegate(Player player, int consumeType, int type, Fuse fuse, string argst) { });
        private static CsValidDelegate DefCsValid = new CsValidDelegate(
            delegate(Player player, int consumeType, int type, Fuse fuse) { return true; });
        private static CsInputDelegate DefCsInput = new CsInputDelegate(
            delegate(Player player, int consumeType, int type, Fuse fuse, string prev) { return ""; });
    }

    public class MonsterLib
    {
        public List<Monster> Firsts { private set; get; }

        private IDictionary<ushort, Monster> dicts;

        private Utils.ReadonlySQL sql;

        public MonsterLib()
        {
            Firsts = new List<Monster>();
            dicts = new Dictionary<ushort, Monster>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "ID", "CODE", "NAME", "VALID", "FIVE", "STR", "AGL", "LEVEL", "OCCURS",
                "PRIORS", "DEBUTTEXT", "PETTEXT", "WINTEXT", "LOSETEXT", "MIXCODE", "SPI"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Monster");
            foreach (System.Data.DataRow data in datas)
            {
                int mid = (int)((long)data["ID"]);
                string code = (string)data["CODE"];
                string name = (string)data["NAME"];
                int group = (int)((short)data["VALID"]);
                short five = (short)data["FIVE"];
                FiveElement element;
                switch (five)
                {
                    case 1: element = FiveElement.AQUA; break;
                    case 2: element = FiveElement.AGNI; break;
                    case 3: element = FiveElement.THUNDER; break;
                    case 4: element = FiveElement.AERO; break;
                    case 5: element = FiveElement.SATURN; break;
                    default: element = FiveElement.GLOBAL; break;
                }
                ushort str = (ushort)((short)data["STR"]);
                ushort agl = (ushort)((short)data["AGL"]);
                short levelCode = (short)data["LEVEL"];
                Monster.ClLevel level;
                switch (levelCode)
                {
                    case 1: level = Monster.ClLevel.WEAK; break;
                    case 2: level = Monster.ClLevel.STRONG; break;
                    case 3: level = Monster.ClLevel.BOSS; break;
                    default: level = Monster.ClLevel.WOODEN; break;
                }
                string occurs = (string)data["OCCURS"];
                string propss = (string)data["PRIORS"];
                string mixess = (string)data["MIXCODE"];

                string spis = data["SPI"] as string;
                string debutText = data["DEBUTTEXT"] as string;
                string petText = data["PETTEXT"] as string;
                string winText = data["WINTEXT"] as string;
                string loseText = data["LOSETEXT"] as string;
                Monster monster = new Monster(name, code, group, element, str, agl,
                    level, occurs, propss, mixess, spis)
                    {
                        DebutText = debutText ?? "",
                        PetText = petText ?? "",
                        WinText = winText ?? "",
                        LoseText = loseText ?? ""
                    };
                Firsts.Add(monster);
                dicts.Add((ushort)mid, monster);
            }
        }

        public int Size { get { return dicts.Count; } }

        public Monster Decode(ushort code)
        {
            Monster monster;
            if (dicts.TryGetValue(code, out monster))
                return monster;
            else return null;
        }
        public ushort Encode(string code)
        {
            var any = dicts.Where(p => p.Value.Code.Equals(code));
            if (any.Count() == 1)
                return any.Select(p => p.Key).Single();
            else
                return 0;
        }

        public List<ushort> ListAllSeleable(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return dicts.Where(p => p.Value.Group > 0).Select(p => p.Key).ToList();
            else
                return dicts.Where(p => pkgs.Contains(p.Value.Group)).Select(p => p.Key).ToList();
        }

        public List<Monster> ListAllMonster(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return dicts.Values.ToList();
            else
                return dicts.Values.Where(p => pkgs.Contains(p.Group)).ToList();
        }
    }
}
