﻿using System;
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

        public Skill[] Skills { set; get; }

        private int mSTR, mDEX, mSTRa, mDEXa, mSTRb, mDEXb;
        public char Gender { set; get; }

        public int HP { set; get; }
        public int STR
        {
            set { mSTR = value; }
            get { return mSTR >= 0 ? mSTR : 0; }
        }
        public int DEX
        {
            set { mDEX = value; }
            get { return mDEX >= 0 ? mDEX : 0; }
        }
        public int STRa
        {
            set { mSTRa = (value >= 0 ? value : 0); }
            get { return mSTRa; }
        }
        public int DEXa
        {
            set { mDEXa = (value >= 0 ? value : 0); }
            get { return mDEXa; }
        }

        public int HPb { set; get; }
        public int STRb
        {
            set { mSTRb = (value >= 0 ? value : 0); }
            get { return mSTRb; }
        }
        public int DEXb
        {
            set { mDEXb = (value >= 0 ? value : 0); }
            get { return mDEXb; }
        }

        public int STRi { set; get; } // -1, -inf; 0, normal; 1, inf
        public int DEXi { set; get; } // -1, -inf; 0, normal; 1, inf
        public int TuxLimit { set; get; }

        #endregion Property

        #region Cards
        public List<ushort> Tux { private set; get; }
        public ushort Armor { set; get; }
        public ushort Weapon { set; get; }
        public ushort Luggage { set; get; }

        // Another extra Equip, also view as Equip
        public ushort ExEquip { set; get; }
        public ushort[] Pets { private set; get; }
        // special zones
        public List<ushort> ExCards { private set; get; }
        public List<ushort> Escue { private set; get; }
        // Other card as Equip, but not set
        public List<ushort> Fakeq { private set; get; }

        #endregion Cards

        #region Status
        // cause reason (GL04) / mask code (3)
        private IDictionary<string, int> cardDisabled;

        public bool IsAlive { set; get; }
        // whether can become target for special Targets or Tuxes
        public bool IsTared
        {
            set { mIsTared = value; }
            get { return mIsTared && Uid != 0 && IsAlive && IsReal; }
        }
        private bool mIsTared;
        public bool Immobilized { set; get; }
        public int RestZP { set; get; }
        public bool Loved { set; get; }
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
        public bool PetDisabled { set; get; }
        public bool ZPDisabled { set; get; }

        #endregion Status

        #region Memory
        //public ushort ArgUshort { set; get; }
        //public string ArgString { set; get; }
        //public List<ushort> ArgUShorts { private set; get; }
        public ushort ROMUshort { set; get; } // 0
        public List<string> ROMCards { private set; get; } // 1
        public int ROMToken { set; get; } // 2
        public List<ushort> ROMPlayerTar { private set; get; } // 3

        public ushort RAMUshort { set; get; }
        public int RAMInt { set; get; }
        public List<ushort> RAMPeoples { private set; get; }
        public string RAMString { set; get; }

        public Stack<int> Coss { private set; get; } // Cos players Stack, e.g. SP101-SP102-XJ404
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
            this.Armor = 0; this.Weapon = 0; this.Luggage = 0; this.ExEquip = 0;
            this.Pets = new ushort[5];
            for (int i = 0; i < Pets.Length; ++i)
                Pets[i] = 0;
            this.ExCards = new List<ushort>();
            Escue = new List<ushort>();
            Fakeq = new List<ushort>();

            this.ROMUshort = 0;
            ROMToken = 0;
            ROMCards = new List<string>();
            ROMPlayerTar = new List<ushort>();

            RAMUshort = 0; RAMInt = 0;
            RAMPeoples = new List<ushort>();
            Coss = new Stack<int>();

            Immobilized = false;
            cardDisabled = new Dictionary<string, int>();
            PetDisabled = false;
            ZPDisabled = false;
            IsAlive = false;
            Loved = false;

            IsTPOpt = true;
            IsSKOpt = true;

            IsReal = isReal;
        }
        public int GetPetCount() { return Pets.Count(p => p != 0); }
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
            RAMUshort = 0;
            RAMInt = 0;
            RAMPeoples.Clear();
            RAMString = "";
            ZPDisabled = false;
        }

        public void ResetROM(Board board)
        {
            ROMUshort = 0;
            ROMToken = 0;
            ROMPlayerTar.Clear();
            foreach (string cd in ROMCards)
            {
                if (cd.StartsWith("H"))
                {
                    int hero = int.Parse(cd.Substring("H".Length));
                    board.BannedHero.Remove(hero);
                }
            }
            ROMCards.Clear();
            ResetRAM();
        }

        public void InitFromHero(Base.Card.Hero hero) { InitFromHero(hero, true); }
        public void InitFromHero(Base.Card.Hero hero, bool reset)
        {
            Gender = hero.Gender;
            STR = STRa = STRb = hero.STR;
            DEX = DEXa = DEXb = hero.DEX;
            STRi = 0; DEXi = 0;
            if (reset)
            {
                IsAlive = true;
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
            else if (Luggage == card) { Luggage = 0; discards.Add(card); return true; }
            else if (ExEquip == card) { ExEquip = 0; discards.Add(card); return true; }
            else if (ExCards.Contains(card)) { ExCards.Remove(card); discards.Add(card); return true; }
            else if (Fakeq.Contains(card)) { Fakeq.Remove(card); discards.Add(card); return true; }
            else return false;
        }
        public bool HasAnyCards()
        {
            return Tux.Count > 0 || HasAnyEquips();
        }
        public bool HasAnyEquips()
        {
            return Weapon != 0 || Armor != 0 || Luggage != 0 || ExEquip != 0 ||
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
            if (Luggage != 0) result.Add(Luggage);
            if (ExEquip != 0) result.Add(ExEquip);
            if (ExCards.Count > 0) result.AddRange(ExCards);
            if (Fakeq.Count > 0) result.AddRange(Fakeq);
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
        public int GetEquipCount()
        {
            return (Weapon != 0 ? 1 : 0) + (Armor != 0 ? 1 : 0) + (ExEquip != 0 ? 1 : 0) +
                ExCards.Count + Fakeq.Count;
        }
        public bool IsValidPlayer()
        {
            return Uid != 0 && IsAlive && IsReal;
        }
    }
}
