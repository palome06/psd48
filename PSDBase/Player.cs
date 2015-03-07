using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IDictionary<string, int> cz01PriceDict { private set; get; }

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
        // TODO: consider to handle with non-equip, too.
        private IDictionary<string, int> cardDisabled;

        public bool IsAlive { set; get; }
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
        // Whether player is disabled to use ZP
        public bool ZPDisabled { set; get; }
        // Whether player can use tux from hand directly or not
        public bool DrTuxDisabled { set; get; }
        // Extend Equip Mask, 0 = disabled, 1 = weapon, 2 = armor, 4 = trove
        public int ExMask { set; get; }

        private void SetEquipDisabled(string tag, bool value, int maskCode)
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
        private int GetEquipDisabled()
        {
            int maskAll = 0;
            foreach (int mask in cardDisabled.Values)
                maskAll |= mask;
            return maskAll;
        }
        public bool EquipDisabled { get { return (GetEquipDisabled() & 7) != 0; } }
        public void SetEquipDisabled(string tag, bool value) { SetEquipDisabled(tag, value, 7); }
        public bool WeaponDisabled { get { return (GetEquipDisabled() & 1) != 0; } }
        public void SetWeaponDisabled(string tag, bool value) { SetEquipDisabled(tag, value, 1); }
        public bool ArmorDisabled { get { return (GetEquipDisabled() & 2) != 0; } }
        public void SetArmorDisabled(string tag, bool value) { SetEquipDisabled(tag, value, 2); }
        public bool LuggageDisabled { get { return (GetEquipDisabled() & 4) != 0; } }
        public void SetLuggageDisabled(string tag, bool value) { SetEquipDisabled(tag, value, 4); }
        // whether the player is rounder and dead to cause continuous procedure
        public bool IsRan { set; get; }
        // whether the player is added and needed push back to pocket
        public bool IsZhu { set; get; }

        #endregion Status

        #region Memory
        public bool TokenAwake { set; get; } // 0 (e.g. XJ401.2)
        public ushort TokenCount { set; get; } // 1 (e.g. XJ608.1)
        public List<ushort> TokenTars { set; get; } // 2 (e.g. XJ405,2)
        public List<string> TokenExcl { set; get; } // 3 (e.g. XJ606,1)
        public List<ushort> TokenFold { set; get; } // 4 (e.g. HL011.2)

        public ushort SingleTokenTar { get { return TokenTars.Count > 0 ? TokenTars[0] : (ushort)0; } }

        // TODO: to change memory into Diva format
        public Utils.Diva ROM { private set; get; }
        //public IDictionary<string, object> ROM { private set; get; }
        public IDictionary<string, object> RAM { private set; get; }
        public ushort ROMUshort { set; get; }
        public int ROMInt { set; get; }
        public ushort RAMUshort { set; get; }
        public int RAMInt { set; get; }
        public List<ushort> RAMUtList { private set; get; }

        // Cos players Stack, e.g. SP101-SP102-XJ404
        public Stack<int> Coss { private set; get; }
        // Guardian, e.g. TR007/TR020
        public ushort Guardian { set; get; }
        #endregion Memory

        #region PlayerOptions
        //public bool IsVip { set; get; }
        public bool IsTPOpt { set; get; }
        public bool IsSKOpt { set; get; }
        #endregion PlayerOptions

        public Player(string name, ushort avatar, ushort uid)
            : this(name, avatar, uid, true) { }

        internal Player(string name, ushort avatar, ushort uid, bool isReal)
        {
            this.Name = name; this.Avatar = avatar; this.Uid = uid;

            this.Tux = new List<ushort>();
            this.Armor = 0; this.Weapon = 0; this.Trove = 0;
            this.ExEquip = 0; this.ExMask = 0;

            this.Pets = new ushort[] { 0, 0, 0, 0, 0 };
            this.ExCards = new List<ushort>();
            Escue = new List<ushort>();
            Fakeq = new Dictionary<ushort, string>();

            TokenAwake = false;
            TokenCount = 0;
            TokenExcl = new List<string>();
            TokenTars = new List<ushort>();
            TokenFold = new List<ushort>();

            ROM = new Utils.Diva();
            RAM = new Dictionary<string, object>();
            RAMUtList = new List<ushort>();

            Coss = new Stack<int>();
            Guardian = 0;

            Immobilized = false;
            cardDisabled = new Dictionary<string, int>();
            PetDisabled = false;
            ZPDisabled = false;
            IsAlive = false;
            Loved = false;
            DrTuxDisabled = false;

            IsTPOpt = true;
            IsSKOpt = true;

            IsReal = isReal;
            Skills = new HashSet<string>();
            IsRan = false;
            IsZhu = false;

            cz01PriceDict = new Dictionary<string, int>();
        }
        public int GetPetCount() { return Pets.Count(p => p != 0); }
        public int GetActionPetCount(Board board)
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

        public void ResetRAM()
        {
            RAM.Clear();
            RAMUshort = 0;
            RAMInt = 0;
            RAMUtList.Clear();
            ZPDisabled = false;
            DrTuxDisabled = false;
        }

        public void ResetTokens()
        {
            TokenAwake = false;
            TokenCount = 0;
            TokenExcl.Clear();
            TokenTars.Clear();
            TokenFold.Clear();
        }

        public void ResetROM(Board board)
        {
            ResetTokens();
            foreach (string cd in TokenExcl)
            {
                if (cd.StartsWith("H"))
                {
                    int hero = int.Parse(cd.Substring("H".Length));
                    board.BannedHero.Remove(hero);
                }
            }
            ROM.Clear();
            ROMUshort = 0;
            ROMInt = 0;
            ResetRAM();
        }

        public void InitFromHero(Base.Card.Hero hero, bool reset, bool sdaset, bool sdcset)
        {
            Gender = hero.Gender;
            STRh = STRb = hero.STR;
            DEXh = DEXb = hero.DEX;
            SDaSet = sdaset; SDcSet = sdcset;
            STRi = 0; DEXi = 0;
            Skills.Clear();
            if (reset)
            {
                IsAlive = true;
                IsTared = true;
                PetDisabled = false;
                Immobilized = false;
                HP = HPb = hero.HP;
                Loved = false;
                TuxLimit = 3;

                cz01PriceDict.Clear();
                cz01PriceDict.Add("JP03", 1);
                cz01PriceDict.Add("WQ04", 2);
                cz01PriceDict.Add("{E}WQ04", 2);
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
        public List<ushort> ListOutAllCards()
        {
            List<ushort> result = new List<ushort>();
            if (Tux.Count > 0) result.AddRange(Tux);
            result.AddRange(ListOutAllEquips());
            return result;
        }
        public List<ushort> ListOutAllEquips()
        {
            List<ushort> result = new List<ushort>();
            if (Weapon != 0) result.Add(Weapon);
            if (Armor != 0) result.Add(Armor);
            if (Trove != 0) result.Add(Trove);
            if (ExEquip != 0) result.Add(ExEquip);
            if (ExCards.Count > 0) result.AddRange(ExCards);
            if (Fakeq.Count > 0) result.AddRange(Fakeq.Keys);
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
        public bool IsValidPlayer()
        {
            return Uid != 0 && IsAlive && IsReal;
        }
    }
}
