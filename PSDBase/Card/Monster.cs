using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.Base.Card
{
    public class Monster : NMB
    {
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
        public int Genre { private set; get; }
        // whether put into piles or not
        public bool IsEx { private set; get; }
        public ushort DBSerial { set; get; }

        public int mSTR;
        public int STR { set { mSTR = value; } get { return mSTR >= 0 ? mSTR : 0; } }
        public ushort STRb { private set; get; }
        public int mAGL;
        public int AGL { set { mAGL = value; } get { return mAGL >= 0 ? mAGL : 0; } }
        public ushort AGLb { private set; get; }

        public string[][] EAOccurs { private set; get; }
        public int[][] EAProperties { private set; get; }
        public bool[][] EALocks { private set; get; }
        public bool[][] EAOnces { private set; get; }
        public bool[][] EAIsTermini { private set; get; }
        public bool[][] EAHinds { private set; get; }

        public ushort Padrone { set; get; }

        public string DebutText { set; get; }
        public string PetText { set; get; }
        public string WinText { set; get; }
        public string LoseText { set; get; }

        //public ushort ROMUshort { set; get; }
        //public int RAMInt { set; get; }
        //public List<ushort> RAMUtList { private set; get; }

        #region Memory
        public Utils.Diva ROM { private set; get; } // Alive during the entire game
        public Utils.Diva RFM { private set; get; } // Alive in a round
        public Utils.Diva RAM { private set; get; } // Alive in a period
        public bool TeamBursted { set; get; } // whether brust in the team, only reset when changing side
        #endregion Memory

        #region SPI Info
        private int mSpiHW, mSpiHL, mSpiHw, mSpiHl, mSpiHC, mSpiHc;
        private int mSpiTW, mSpiTL, mSpiTw, mSpiTl, mSpiTC, mSpiTc;
        private bool mSpiS;

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
        public bool IsSilence() { return mSpiS; }
        public void ParseSpi(string spis)
        {
            mSpiHW = mSpiHL = mSpiHw = mSpiHl = mSpiHC = mSpiHc = 0;
            mSpiTW = mSpiTL = mSpiTw = mSpiTl = mSpiTC = mSpiTc = 0;
            mSpiS = false;
            for (int i = 0; i < spis.Length;)
            {
                if (spis[i] == 'S')
                {
                    mSpiS = true; ++i;
                }
                else if (i + 2 < spis.Length && spis[i + 2] == '#')
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
            int consumeType, int type, string fuse, string argst);
        public delegate bool CsValidDelegate(Player player,
            int consumeType, int type, string fuse);
        public delegate string CsInputDelegate(Player player,
            int consumeType, int type, string fuse, string prev);

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

        public Monster(string name, string code, int group, int genre, FiveElement element, ushort strb,
            ushort agl, ClLevel level, string[][] eaoccurs, int[][] eaprops,
            bool[][] ealocks, bool[][] eaonces, bool[][] eaterminis, bool[][] eahinds, string spis)
        {
            this.Name = name; this.Code = code;
            this.Group = Math.Abs(group); this.Genre = genre;
            this.IsEx = (group < 0);
            this.Element = element; this.Level = level;
            this.STRb = strb; this.STR = this.STRb;
            this.AGLb = agl; this.AGL = this.AGLb;
            this.EAOccurs = eaoccurs;
            this.EAProperties = eaprops;
            this.EALocks = ealocks;
            this.EAOnces = eaonces;
            this.EAIsTermini = eaterminis;
            this.EAHinds = eahinds;
            this.Padrone = 0;
            ParseSpi(spis);

            ROM = new Utils.Diva();
            RFM = new Utils.Diva();
            RAM = new Utils.Diva();
            TeamBursted = false;
        }

        public bool IsMonster() { return true; }
        public bool IsNPC() { return false; }

        private DebutDelegate DefDebut = new DebutDelegate(delegate () { });
        private WLDelegate DefWL = new WLDelegate(delegate () { });
        private static CrActionDelegate DefCrAction = p => { };
        private static CsActionDelegate DefCsAction = (p, c, t, f, a) => { };
        private static CsValidDelegate DefCsValid = (p, c, t, f) => true;
        private static CsInputDelegate DefCsInput = (p, c, t, f, pv) => "";

        public void ResetRAM() { RAM.Clear(); }
        public void ResetRFM() { ResetRAM(); RFM.Clear(); }
        public void ResetROM() { ResetRFM(); ROM.Clear(); }

        public bool IsLinked(int consumeType, int inType)
        {
            return EAOccurs != null && EAOccurs.Length > consumeType && EAOccurs[consumeType] != null &&
                EAOccurs[consumeType].Length > inType && EAOccurs[consumeType][inType].Contains('%');
        }
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
                "ID", "CODE", "NAME", "VALID", "STR", "AGL", "LEVEL", "OCCURS", "PRIORS",
                "DEBUTTEXT", "PETTEXT", "WINTEXT", "LOSETEXT", "TERMINI", "SPI", "GENRE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Monster");
            foreach (System.Data.DataRow data in datas)
            {
                int mid = (int)((long)data["ID"]);
                string code = (string)data["CODE"];
                string name = (string)data["NAME"];
                int group = (int)((short)data["VALID"]);
                int genre = (int)((long)data["GENRE"]);
                FiveElement five;
                switch (code.Substring(0, 2))
                {
                    case "GS": five = FiveElement.AQUA; break;
                    case "GH": five = FiveElement.AGNI; break;
                    case "GL": five = FiveElement.THUNDER; break;
                    case "GF": five = FiveElement.AERO; break;
                    case "GT": five = FiveElement.SATURN; break;
                    case "GI": five = FiveElement.YINN; break;
                    case "GY": five = FiveElement.SOLARIS; break;
                    default: five = FiveElement.A; break;
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
                string[] occurs = ((string)data["OCCURS"]).Split(';');
                string[][] eaoccurs = new string[occurs.Length][];
                string[] propss = ((string)data["PRIORS"]).Split(';');
                int[][] eaprops = new int[occurs.Length][];
                bool[][] ealocks = new bool[occurs.Length][];
                bool[][] eaonces = new bool[occurs.Length][];
                string[] eatermis = ((string)data["TERMINI"]).Split(';');
                bool[][] eaterminies = new bool[occurs.Length][];
                bool[][] eahinds = new bool[occurs.Length][];
                for (int i = 0; i < occurs.Length; ++i)
                {
                    if (occurs[i] == "^")
                    {
                        eaoccurs[i] = null;
                        eaprops[i] = null;
                        ealocks[i] = null;
                        eaonces[i] = null;
                    }
                    else
                    {
                        string[] ods = occurs[i].Split(',');
                        string[] opss = propss[i].Split(',');
                        string[] omin = eatermis[i].Split(',');
                        eaoccurs[i] = new string[ods.Length];
                        eaprops[i] = new int[ods.Length];
                        ealocks[i] = new bool[ods.Length];
                        eaonces[i] = new bool[ods.Length];
                        eaterminies[i] = new bool[ods.Length];
                        eahinds[i] = new bool[ods.Length];
                        for (int j = 0; j < ods.Length; ++j)
                        {
                            if (ods[j].StartsWith("!"))
                            {
                                eaoccurs[i][j] = ods[j].Substring(1);
                                ealocks[i][j] = true;
                            }
                            else
                            {
                                eaoccurs[i][j] = ods[j];
                                ealocks[i][j] = false;
                            }
                            if (opss[j].StartsWith("!")) // Only once
                            {
                                eaprops[i][j] = int.Parse(opss[j].Substring("!".Length));
                                eaonces[i][j] = false;
                            }
                            else
                            {
                                eaprops[i][j] = int.Parse(opss[j]);
                                eaonces[i][j] = true;
                            }
                            if (omin[j].StartsWith("!")) // Hind
                            {
                                eaterminies[i][j] = (omin[j][1] == '1');
                                eahinds[i][j] = true;
                            }
                            else
                            {
                                eaterminies[i][j] = (omin[j][0] == '1');
                                eahinds[i][j] = false;
                            }
                        }
                    }
                }
                string spis = data["SPI"] as string;
                string debutText = data["DEBUTTEXT"] as string;
                string petText = data["PETTEXT"] as string;
                string winText = data["WINTEXT"] as string;
                string loseText = data["LOSETEXT"] as string;
                Monster monster = new Monster(name, code, group, genre, five, str, agl,
                    level, eaoccurs, eaprops, ealocks, eaonces, eaterminies, eahinds, spis)
                {
                    DebutText = debutText ?? "",
                    PetText = petText ?? "",
                    WinText = winText ?? "",
                    LoseText = loseText ?? "",
                    DBSerial = (ushort)mid
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
                return dicts.Where(p => !p.Value.IsEx).Select(p => p.Key).ToList();
            else
                return dicts.Where(p => pkgs.Contains(p.Value.Group) && !p.Value.IsEx).Select(p => p.Key).ToList();
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