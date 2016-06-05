using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.Base
{
    public class Player
    {
        #region Account Info
        public string Name { private set; get; }
        public ushort Avatar { private set; get; }
        public ushort Uid { private set; get; } // Here Uid is actually seat position

        public ushort AUid { set; get; } // AUid is the uid of a given account
        public int HopeTeam { set; get; }
        #endregion Account Info

        #region Property
        // select Hero code, e.g. 10502 for Han Lingsha
        public int SelectHero { get; set; }
        public int Team { get; set; }
        // whether is real or lumberjack and etc
        public bool IsReal { private set; get; }
        // Set of skills the player has obtained
        public ISet<string> Skills { private set; get; }
        // 'M' for male and 'F' for female
        public char Gender { set; get; }

        public int HP { set; get; }
        public int HPb { set; get; }

        // whether the STRa/DEXa/STRc/DEXc is set, return STR/DEX according to them.
        public bool SDaSet { set; get; }
        public bool SDcSet { set; get; }
        public int mSTRa, mSTRb, mDEXa, mDEXb;
        private int mSTRc, mDEXc;

        public int STRa { set { mSTRa = value; } get { return mSTRa >= 0 ? mSTRa : 0; }}
        public int STRb { set { mSTRb = value; } get { return mSTRb >= 0 ? mSTRb : 0; }}
        public int STRc { set { mSTRc = (value >= 0 ? value : 0); } get { return mSTRc; } }
        // STR value got from the hero card, might decrease because of skill
        public int STRh { set; get; }

        public int DEXa { set { mDEXa = value; } get { return mDEXa >= 0 ? mDEXa : 0; }}
        public int DEXb { set { mDEXb = value; } get { return mDEXb >= 0 ? mDEXb : 0; }}
        public int DEXc { set { mDEXc = (value >= 0 ? value : 0); } get { return mDEXc; } }
        public int DEXh { set; get; }

        public int STR { get { return SDcSet ? STRc : (SDaSet ? STRa : STRb ); } }
        public int DEX { get { return SDcSet ? DEXc : (SDaSet ? DEXa : DEXb ); } }

        public int STRi { set; get; } // -1, -inf; 0, normal; 1, inf
        public int DEXi { set; get; } // -1, -inf; 0, normal; 1, inf
        public int TuxLimit { set; get; }

        public IDictionary<string, List<string>> cz01PriceDict { private set; get; }

        #endregion Property

        #region Cards
        public List<ushort> Tux { private set; get; }
        public ushort Armor { set; get; }
        public ushort Weapon { set; get; }
        public ushort Trove { set; get; }

        // Another extend Equip, also view as Equip
        public ushort ExEquip { set; get; }
        // extra Equip Slot, not the same as the ExEquip
        public List<ushort> ExCards { private set; get; }
        // special time-lapse cards, the substitution is set in Board
        public IDictionary<ushort, string> Fakeq { private set; get; }

        public ushort[] Pets { private set; get; }
        // Escue Npcs
        public List<ushort> Escue { private set; get; }

        #endregion Cards

        #region Status
        // cause reason (GL04) / mask code (3)
        private IDictionary<string, int> cardDisabled;

        public bool IsAlive { set; get; }
        // indicate whether a player is in processing of leaving
        // it might be interrupted by another join
        public bool Nineteen { set; get; }
        // whether can become target for special Targets or Tuxes
        private bool mIsTared;
        public bool IsTared
        {
            set { mIsTared = value; }
            get { return mIsTared && Uid != 0 && IsAlive && IsReal; }
        }
        public bool Immobilized { set; get; }
        public bool Loved { set; get; }
        public bool PetDisabled { set; get; }
        // How many ZPs the player can still use
        public int RestZP { set; get; }
        // Extend Equip Mask, 0 = disabled, 1 = weapon, 2 = armor, 4 = trove
        public int ExMask { set; get; }
        // Lost Basic Equip Mask, 0 = full, 1 = weapon, 2 = armor, 4 = trove
        public int FyMask { set; get; }
        public List<ushort> Runes { private set; get; }
        public List<string> ExSpouses { private set; get; }

        private void SetTuxDisabledLevel(string tag, bool value, int maskCode)
        {
            if (value)
            {
                if (cardDisabled.ContainsKey(tag))
                    cardDisabled[tag] |= maskCode;
                else
                    cardDisabled.Add(tag, maskCode);
            }
            else
            {
                if (cardDisabled.ContainsKey(tag))
                {
                    cardDisabled[tag] &= (~maskCode);
                    if (cardDisabled[tag] == 0)
                        cardDisabled.Remove(tag);
                }
            }
        }
        private int GetTuxDisabledLevel()
        {
            return cardDisabled.Values.Aggregate(0, (acc, x) => acc | x);
        }
        public bool EquipDisabled { get { return (GetTuxDisabledLevel() & 7) != 0; } }
        public void SetEquipDisabled(string tag, bool value) { SetTuxDisabledLevel(tag, value, 7); }
        public bool WeaponDisabled { get { return (GetTuxDisabledLevel() & 1) != 0; } }
        public void SetWeaponDisabled(string tag, bool value) { SetTuxDisabledLevel(tag, value, 1); }
        public bool ArmorDisabled { get { return (GetTuxDisabledLevel() & 2) != 0; } }
        public void SetArmorDisabled(string tag, bool value) { SetTuxDisabledLevel(tag, value, 2); }
        public bool TroveDisabled { get { return (GetTuxDisabledLevel() & 4) != 0; } }
        public void SetTroveDisabled(string tag, bool value) { SetTuxDisabledLevel(tag, value, 4); }

        public bool ZPDisabled { get { return (GetTuxDisabledLevel() & 0x8) != 0; } }
        public void SetZPDisabled(string tag, bool value) { SetTuxDisabledLevel(tag, value, 0x8); }
        public bool AllTuxDisabled { get { return (GetTuxDisabledLevel() & 0x10) != 0; } }
        public void SetAllTuxDisabled(string tag, bool value) { SetTuxDisabledLevel(tag, value, 0x10); }
        // whether the player is rounder and dead to cause continuous procedure
        public bool IsRan { set; get; }
        // whether the player is added and needed push back to pocket
        public bool IsZhu { set; get; }

        #endregion Status

        #region Memory
        // TODO: formalize and classify memories
        public bool TokenAwake { set; get; } // 0 (e.g. XJ401.2)
        public ushort TokenCount { set; get; } // 1 (e.g. XJ608.1)
        public List<ushort> TokenTars { set; get; } // 2 (e.g. XJ405,2)
        public List<string> TokenExcl { set; get; } // 3 (e.g. XJ606,1)
        public List<ushort> TokenFold { set; get; } // 4 (e.g. HL011.2)

        public ushort SingleTokenTar { get { return TokenTars.Count > 0 ? TokenTars[0] : (ushort)0; } }

        public Utils.Diva ROM { private set; get; } // Alive during the entire game
        public Utils.Diva RFM { private set; get; } // Alive in a round
        public Utils.Diva RAM { private set; get; } // Alive in a period

        // Cos players Stack, e.g. SP101-SP102-XJ404
        public Stack<int> Coss { private set; get; }
        // Guardian, e.g. TR007/TR020
        public ushort Guardian { set; get; }
        #endregion Memory

        #region PlayerOptions
        //public bool IsVip { set; get; }
        public bool IsTPOpt { set; get; }
        public bool IsSKOpt { set; get; }
        public bool IsMyOpt { set; get; }
        #endregion PlayerOptions

        public Player(string name, ushort avatar, ushort uid)
            : this(name, avatar, uid, true) { }

        internal Player(string name, ushort avatar, ushort uid, bool isReal)
        {
            this.Name = name; this.Avatar = avatar; this.Uid = uid;

            this.Tux = new List<ushort>();
            this.Armor = 0; this.Weapon = 0; this.Trove = 0;
            this.ExEquip = 0; this.ExMask = 0; this.FyMask = 0;

            Pets = Enumerable.Repeat<ushort>(0, Card.FiveElementHelper.PropCount).ToArray();
            this.ExCards = new List<ushort>();
            Escue = new List<ushort>();
            Fakeq = new Dictionary<ushort, string>();
            this.Runes = new List<ushort>();
            this.ExSpouses = new List<string>();

            TokenAwake = false;
            TokenCount = 0;
            TokenExcl = new List<string>();
            TokenTars = new List<ushort>();
            TokenFold = new List<ushort>();

            ROM = new Utils.Diva();
            RFM = new Utils.Diva();
            RAM = new Utils.Diva();

            Coss = new Stack<int>();
            Guardian = 0;

            Immobilized = false;
            cardDisabled = new Dictionary<string, int>();
            PetDisabled = false;
            IsAlive = false;
            Nineteen = false;
            Loved = false;

            IsTPOpt = true;
            IsSKOpt = true;
            IsMyOpt = true;

            IsReal = isReal;
            Skills = new HashSet<string>();
            IsRan = false;
            IsZhu = false;

            cz01PriceDict = new Dictionary<string, List<string>>();
        }
        public int GetPetCount() { return Pets.Count(p => p != 0); }
        public int GetActivePetCount(Board board)
        {
            return PetDisabled ? 0 : Pets.Count(p => p != 0 && !board.NotActionPets.Contains(p));
        }
        public int OppTeam { get { return 3 - Team; } }

        public void ResetStatus()
        {
            Immobilized = false;
            cardDisabled.Clear();
            PetDisabled = false;
            Loved = false;
            DEXi = 0; STRi = 0;
        }

        public void ResetRAM(int hero = 0)
        {
            Utils.Diva newDiva = new Utils.Diva();
            if (hero != 0)
            {
                foreach (string key in RAM.GetKeys())
                {
                    if (key.StartsWith("@") && !key.StartsWith("@" + hero))
                        newDiva.Set(key, RAM.GetObject(key));
                }
                RAM = newDiva;
            }
            else
                RAM.Clear();
        }

        public void ResetRFM(int hero = 0)
        {
            ResetRAM(hero);
            Utils.Diva newDiva = new Utils.Diva();
            if (hero != 0)
            {
                foreach (string key in RFM.GetKeys())
                {
                    if (key.StartsWith("@") && !key.StartsWith("@" + hero))
                        newDiva.Set(key, RFM.GetObject(key));
                }
                RFM = newDiva;
            }
            else
                RFM.Clear();
        }

        public void ResetTokens()
        {
            TokenAwake = false;
            TokenCount = 0;
            TokenExcl.Clear();
            TokenTars.Clear();
            TokenFold.Clear();
        }

        public void ResetROM(Board board, int hero = 0)
        {
            ResetTokens();
            foreach (string cd in TokenExcl)
            {
                if (cd.StartsWith("H"))
                {
                    int heroSwal = int.Parse(cd.Substring("H".Length));
                    board.BannedHero.Remove(heroSwal);
                }
            }
            ResetRFM(hero);
            Utils.Diva newDiva = new Utils.Diva();
            if (hero != 0)
            {
                foreach (string key in ROM.GetKeys())
                {
                    if (key.StartsWith("@") && !key.StartsWith("@" + hero))
                        newDiva.Set(key, ROM.GetObject(key));
                }
                ROM = newDiva;
            }
            else
                ROM.Clear();
        }

        public void InitFromHero(Base.Card.Hero hero, bool reset, bool sdaset, bool sdcset)
        {
            Gender = hero.Gender;
            STRh = STRb = hero.STR;
            DEXh = DEXb = hero.DEX;
            SDaSet = sdaset; SDcSet = sdcset;
            STRi = 0; DEXi = 0;
            if (reset)
            {
                Skills.Clear();
                IsAlive = true;
                Nineteen = false;
                IsTared = true;
                PetDisabled = false;
                Immobilized = false;
                HP = HPb = hero.HP;
                Loved = false;
                TuxLimit = 3;
            }
        }
        public bool RemoveCard(ushort card, List<ushort> discards)
        {
            if (Tux.Contains(card)) { Tux.Remove(card); discards.Add(card); return true; }
            else if (Armor == card) { Armor = 0; discards.Add(card); return true; }
            else if (Weapon == card) { Weapon = 0; discards.Add(card); return true; }
            else if (Trove == card) { Trove = 0; discards.Add(card); return true; }
            else if (ExEquip == card) { ExEquip = 0; discards.Add(card); return true; }
            else if (ExCards.Contains(card)) { ExCards.Remove(card); discards.Add(card); return true; }
            else if (Fakeq.ContainsKey(card)) { Fakeq.Remove(card); discards.Add(card); return true; }
            else return false;
        }
        public bool HasAnyCards()
        {
            return Tux.Count > 0 || HasAnyEquips();
        }
        public bool HasAnyEquips()
        {
            return Weapon != 0 || Armor != 0 || Trove != 0 || ExEquip != 0 ||
                ExCards.Count > 0 || Fakeq.Count > 0;
        }
        public bool HasCard(ushort ut)
        {
            return Tux.Contains(ut) || Weapon == ut || Armor == ut || Trove == ut ||
                ExEquip == ut || ExCards.Contains(ut) || Fakeq.ContainsKey(ut);
        }
        public bool HasCards(IEnumerable<ushort> uts)
        {
            return !uts.Any(p => !HasCard(p));
        }
        public List<ushort> ListOutAllCards()
        {
            List<ushort> result = new List<ushort>();
            if (Tux.Count > 0) result.AddRange(Tux);
            result.AddRange(ListOutAllEquips());
            return result;
        }
        public List<ushort> ListOutAllEquips()
        {
            List<ushort> result = ListOutAllBaseEquip();
            if (ExCards.Count > 0) result.AddRange(ExCards);
            if (Fakeq.Count > 0) result.AddRange(Fakeq.Keys);
            return result;
        }
        public List<ushort> ListOutAllBaseEquip()
        {
            List<ushort> result = new List<ushort>();
            if (Weapon != 0) result.Add(Weapon);
            if (Armor != 0) result.Add(Armor);
            if (Trove != 0) result.Add(Trove);
            if (ExEquip != 0) result.Add(ExEquip);
            return result;
        }
        public List<ushort> ListOutAllTuxsWithEncrypt()
        {
            List<ushort> result = new List<ushort>();
            for (int i = 0; i < Tux.Count; ++i)
                result.Add(0);
            return result;
        }
        public List<ushort> ListOutAllCardsWithEncrypt()
        {
            List<ushort> result = ListOutAllTuxsWithEncrypt();
            result.AddRange(ListOutAllEquips());
            return result;
        }
        public List<ushort> ListOutAllCardsWithEncrypt(List<ushort> except)
        {
            List<ushort> result = new List<ushort>();
            for (int i = 0; i < Tux.Count; ++i)
                if (!except.Contains(Tux[i]))
                    result.Add(0);
            foreach (ushort ut in ListOutAllEquips())
                if (!except.Contains(ut))
                    result.Add(ut);
            return result;
        }
        public int GetBaseEquipCount()
        {
            return (Weapon != 0 ? 1 : 0) + (Armor != 0 ? 1 : 0) +
                 (Trove != 0 ? 1 : 0) + (ExEquip != 0 ? 1 : 0);
        }
        public int GetEquipCount()
        {
            return GetBaseEquipCount() + ExCards.Count + Fakeq.Count;
        }
        public int GetAllCardsCount()
        {
            return Tux.Count + GetEquipCount();
        }
        public bool IsValidPlayer()
        {
            return Uid != 0 && IsAlive && IsReal;
        }
        public int GetSlotCapacity(Card.Tux.TuxType tuxType)
        {
            int mask = 0;
            if (tuxType == Card.Tux.TuxType.WQ)
                mask = 0x1;
            else if (tuxType == Card.Tux.TuxType.FJ)
                mask = 0x2;
            else if (tuxType == Card.Tux.TuxType.XB)
                mask = 0x4;
            return 1 + ((ExMask & mask) == 0 ? 0 : 1) + ((FyMask & mask) == 0 ? 0 : -1);
        }
        public int GetCurrentEquipCount(Card.Tux.TuxType tuxType)
        {
            int cap = GetSlotCapacity(tuxType);
            if (cap == 0)
                return 0;
            int ext = (cap == 2 && ExEquip != 0) ? 1 : 0;
            if (tuxType == Card.Tux.TuxType.WQ)
                return (Weapon != 0 ? 1 : 0) + ext;
            else if (tuxType == Card.Tux.TuxType.FJ)
                return (Armor != 0 ? 1 : 0) + ext;
            else if (tuxType == Card.Tux.TuxType.XB)
                return (Trove != 0 ? 1 : 0) + ext;
            else return 0;
        }
        #region TuxPrice
        public void ClearPrice()
        {
            cz01PriceDict.Clear();
        }
        public void AddToPrice(string tuxCode, bool inEq, string reason, char type, int price)
        {
            string prefix = (inEq ? "{E}" : "") + tuxCode;
            if (!cz01PriceDict.ContainsKey(prefix))
                cz01PriceDict[prefix] = new List<string>();
            cz01PriceDict[prefix].Add(reason + "," + type + "," + price);
        }
        public void RemoveFromPrice(string tuxCode, bool inEq, string reason)
        {
            string prefix = (inEq ? "{E}" : "") + tuxCode;
            if (cz01PriceDict.ContainsKey(prefix))
            {
                cz01PriceDict[prefix].RemoveAll(p => p.StartsWith(reason + ","));
                if (cz01PriceDict[prefix].Count == 0)
                    cz01PriceDict.Remove(prefix);
            }
        }
        public int GetPrice(string tuxCode, bool inEq)
        {
            string prefix = (inEq ? "{E}" : "") + tuxCode;
            int zero = 0;
            if (cz01PriceDict.ContainsKey(prefix))
            {
                foreach (string line in cz01PriceDict[prefix])
                {
                    string[] lines = line.Split(',');
                    int value = int.Parse(lines[2]);
                    if (lines[1] == "!")
                        return value;
                    else if (lines[1] == "=")
                        zero = value > zero ? value : zero;
                    else if (lines[1] == "+")
                        zero += value;
                    else if (lines[1] == "-")
                        zero -= value;
                }
            }
            return zero < 0 ? 0 : zero;
        }
        #endregion TuxPrice
        // Create a ext warriors, act as normal battle attenders
        public static Player Warriors(string name, int extUid, int team, int str, int agl)
        {
            return new Player(name, 0, (ushort)extUid, false)
            {
                STRb = str,
                DEXb = agl,
                IsAlive = false,
                Team = team
            };
        }

        internal class PlayerCompare : System.Collections.Generic.IEqualityComparer<Player>
        {
            public bool Equals(Player p, Player q) { return p == q; }
            public int GetHashCode(Player p) { return p.Uid.GetHashCode(); }
        }
    }
}
