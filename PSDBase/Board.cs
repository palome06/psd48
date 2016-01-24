using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.Card;
using PSD.Base.Utils;

namespace PSD.Base
{
    public class Board
    {
        public IDictionary<ushort, Player> Garden { set; get; }
        private List<ushort> SortedPlayerList { set; get; }

        public Player Rounder
        {
            set { mRounder = value ?? ghost; }
            get { return mRounder ?? ghost; }
        }
        public Player Hinder
        {
            set { mHinder = value ?? ghost; }
            get { return mHinder ?? ghost; }
        }
        public Player Supporter
        {
            set { mSupporter = value ?? ghost; }
            get { return mSupporter ?? ghost; }
        }
        // Somebody that ordered to trigger a battle
        public Player Horn
        {
            set { mHorn = value ?? ghost; }
            get { return mHorn ?? ghost; }
        }
        public string RoundIN { set; get; }

        public bool SupportSucc { set; get; }
        public bool HinderSucc { set; get; }

        // Give up:0 ; player.Uid:1 ; NMB:GT03,NC150
        public List<string> PosHinders { private set; get; }
        public List<string> PosSupporters { private set; get; }
        public bool AllowNoSupport { set; get; }
        public bool AllowNoHinder { set; get; }
        // 0. fight; 1. Skip fight; 2. not enter the stage.
        //public int IsFight { set; get; }
        public bool ClockWised { set; get; }

        // Whether in a fight/battle, consequence excluded
        public bool InFight { set; get; }
        // Whether in a fight/battle, consequence included
        public bool InFightThrough { set; get; }
        // Whether the debut action of monster is valid
        public bool IsMonsterDebut { set; get; }
        // Bonus Pool Value of Rounder Side
        public int RPool { get; set; }
        // Bonus Pool Value of Opponent Side
        public int OPool { get; set; }
        // map of gain to pool, <host,reason:value>
        public IDictionary<string, int> RPoolGain { get; private set; }
        public IDictionary<string, int> OPoolGain { get; private set; }
        // Whether current battle is won
        public bool IsBattleWin { get; set; }
        // Final difference of pool in Battle
        public int PoolDelta { set; get; }
        // Whether monster can be caught as pet
        public bool Mon1Catchable { get; set; }
        public bool Mon2Catchable { get; set; }
        // nmb in the battle ring
        public ushort Monster1 { set; get; }
        public ushort Monster2 { set; get; }
        public ushort Mon1From { set; get; }
        //public bool IsTangled { set; get; }
        public ushort Neighborhood { set; get; } // NPC card in ex-NPC actions, cover the Monster1 here
        // Battler servers as a snapshot of Monster1, nothing to with the card
        public NMB Battler { set; get; }
        public ushort Eve { get; set; }
        // Round of using card, 0-Nobody, 1-Aka, 2-Ao
        public int UseCardRound { set; get; }
        // Whether the fight is tangled or not
        public bool FightTangled { set; get; }

        public List<ushort> PZone { private set; get; }
        // Consumed Pets list, to be removed later in ZD
        public List<string> CsPets { get; private set; }
        public List<string> CsEqiups { get; private set; }
        public int DiceValue { get; set; }

        public Utils.Rueue<ushort> TuxPiles { set; get; }
        public Utils.Rueue<ushort> EvePiles { set; get; }
        public Utils.Rueue<ushort> MonPiles { set; get; }
        public List<ushort> TuxDises { set; get; }
        public List<ushort> EveDises { set; get; }
        public List<ushort> MonDises { set; get; }

        public Utils.Rueue<int> HeroPiles { set; get; }
        public Utils.Rueue<ushort> RestNPCPiles { set; get; }
        public Utils.Rueue<ushort> RestMonPiles { set; get; }
        public List<int> HeroDises { set; get; }
        public List<ushort> RestNPCDises { set; get; }
        public List<ushort> RestMonDises { set; get; }

        public List<int> BannedHero { private set; get; }
        public List<ushort> ProtectedTux { private set; get; }
        // Vanish tuxes, format: PY.REASON,TUXES (e.g. 2,G0ZB,10)
        public Utils.Rueue<string> PendingTux { private set; get; }
        // Player that won't lose pets in battle
        public List<ushort> PetProtecedPlayer { private set; get; }
        // List of monsters that is disabled
        public ISet<ushort> NotActionPets { private set; get; }

        public IDictionary<string, string> JumpTable { private set; get; }

        // final score
        public int FinalAkaScore { set; get; }
        public int FinalAoScore { set; get; }

        public Player GetOpponenet(Player player)
        {
            int mycount = 0, opcount = 0;
            List<ushort> list = SortedPlayerList.ToList();
            if (!ClockWised)
                list.Reverse();
            foreach (ushort ut in list)
            {
                Player py = Garden[ut];
                if (py.Team == player.Team)
                {
                    ++mycount;
                    if (py.Uid == player.Uid)
                        break;
                }
            }
            bool next = false;
            foreach (ushort ut in list)
            {
                Player py = Garden[ut];
                if (py.Team == player.OppTeam)
                {
                    ++opcount;
                    if (next && py.IsAlive)
                        return py;
                    if (opcount == mycount)
                    {
                        if (py.IsAlive)
                            return py;
                        else
                            next = true;
                    }
                }
            }
            foreach (ushort ut in list)
            {
                Player py = Garden[ut];
                if (py.IsAlive && py.Team == player.OppTeam)
                    return py;
            }
            return ghost;
        }
        public Player Opponent { get { return GetOpponenet(Rounder); } }
        // get the one face to the player
        public Player Facer(Player player)
        {
            ushort uhd = (ushort)(((player.Uid + 1) / 2 * 4) - 1 - player.Uid);
            return uhd > 6 ? ghost : Garden[uhd];
        }
        public void SortGarden()
        {
            SortedPlayerList = Garden.Keys.ToList();
            SortedPlayerList.Sort();
        }

        public bool IsAttendWar(Player player)
        {
            return player.Uid == Rounder.Uid ||
                player.Uid == Hinder.Uid ||
                player.Uid == Supporter.Uid;
        }
        public bool IsAttendWarSucc(Player player)
        {
            return player.Uid == Rounder.Uid ||
                (player.Uid == Hinder.Uid && HinderSucc) ||
                (player.Uid == Supporter.Uid && SupportSucc);
        }

        public int CalculateRPool()
        {
            return Math.Max(Rounder.STR + RPool + (SupportSucc ? Supporter.STR : 0), 0);
        }
        public int CalculateOPool()
        {
            return Math.Max(Battler.STR + OPool + (HinderSucc ? Hinder.STR : 0), 0);
        }
        public bool IsRounderBattleWin()
        {
            foreach (Player py in Garden.Values)
            {
                if (py.IsAlive && py.STRi < 0)
                    return py.Team == Rounder.OppTeam;
                else if (py.IsAlive && py.STRi > 0)
                    return py.Team == Rounder.Team;
            }
            return CalculateRPool() >= CalculateOPool();
        }
        public void CleanBattler()
        {
            Supporter = null; Hinder = null;
            SupportSucc = false; HinderSucc = false;
        }
        public List<ushort> OrderedPlayer() { return OrderedPlayer(Rounder.Uid); }
        public List<ushort> OrderedPlayer(ushort start)
        {
            List<ushort> list = new List<ushort>();
            if (Garden.Count >= 1)
            {
                int idx = start;
                do
                {
                    list.Add((ushort)idx);
                    if (ClockWised)
                        ++idx;
                    else
                        --idx;
                    while (idx > Garden.Count)
                        idx -= Garden.Count;
                    while (idx <= 0)
                        idx += Garden.Count;
                } while (idx != start);
            }
            return list;
        }
        public ushort? GetNextPlayer(ushort rounder)
        {
            List<ushort> ordered = OrderedPlayer();
            foreach (ushort ut in ordered)
            {
                if (ut != rounder && Garden[ut].IsAlive)
                    return ut;
            }
            return null;
        }

        public Board()
        {
            PZone = new List<ushort>();
            PosHinders = new List<string>();
            PosSupporters = new List<string>();
            CsPets = new List<string>();
            CsEqiups = new List<string>();
            BannedHero = new List<int>();
            PendingTux = new Utils.Rueue<string>();
            ProtectedTux = new List<ushort>();
            PetProtecedPlayer = new List<ushort>();
            NotActionPets = new HashSet<ushort>();
            UseCardRound = 0; ClockWised = true;
            JumpTable = new Dictionary<string, string>();
            FinalAkaScore = 0; FinalAoScore = 0;
            RPoolGain = new Dictionary<string, int>();
            OPoolGain = new Dictionary<string, int>();
            IsMonsterDebut = false;
            FightTangled = false;
        }
        // Create a lumberjack of monster/NPC, act as normal humans
        public static Player Lumberjack(Card.NMB nmb, ushort orgCode, int team)
        {
            return new Player(nmb.Name, 0, (ushort)(orgCode + 1000), false)
            {
                STRb = nmb.STR,
                DEXb = nmb.AGL,
                IsAlive = false,
                Team = team
            };
        }

        private Player mRounder, mHinder, mSupporter;
        private Player mHorn;
        private readonly Player ghost = new Player("鬼", 0, 0, false);

        #region Serialize the Game Situation
        public static readonly string[] StatusKey = new string[] { "I,hero", "I,state", "U,hp", "U,hpa", "U,str",
            "U,stra", "U,dex", "U,dexa", "I,tuxCount", "U,wp", "U,am", "U,tr", "U,exq", "LA,lug", "U,guard",
            "U,coss", "LC5,pet", "LU,excard", "LU,rune", "LD,fakeq", "I,token", "LA,excl", "LU,tar", "U,awake",
            "I,foldsz", "LU,escue" };

        public string ToSerialMessage(LibGroup tuple)
        {
            List<object> uList = new List<object>();
            foreach (Player py in Garden.Values)
            {
                uList.Add(py.Uid);
                foreach (string keyPair in StatusKey)
                {
                    int serp = keyPair.IndexOf(',');
                    string key = keyPair.Substring(serp + 1);
                    switch (key)
                    {
                        case "hero": uList.Add(py.SelectHero); break;
                        case "state":
                            int state = 0;
                            state |= !py.IsAlive ? 0 : 1;
                            state |= !py.Loved ? 0 : 2;
                            state |= !py.Immobilized ? 0 : 4;
                            state |= !py.PetDisabled ? 0 : 8;
                            uList.Add(state); break;
                        case "hp": uList.Add(py.HP); break;
                        case "hpa": uList.Add(py.HPb); break;
                        case "str": uList.Add(py.STR); break;
                        case "stra": uList.Add(InFight ? py.STRa : py.STR); break;
                        case "dex": uList.Add(py.DEX); break;
                        case "dexa": uList.Add(InFight ? py.DEXa : py.DEX); break;
                        case "tuxCount": uList.Add(py.Tux.Count); break;
                        case "wp": uList.Add(py.Weapon); break;
                        case "am": uList.Add(py.Armor); break;
                        case "tr": uList.Add(py.Trove); break;
                        case "exq": uList.Add(py.ExEquip); break;
                        case "lug":
                            Luggage lug = tuple.TL.DecodeTux(py.Trove) as Luggage;
                            uList.Add(lug != null ? lug.Capacities.ListToString() : "0");
                            break;
                        case "guard": uList.Add(py.Guardian); break;
                        case "coss": uList.Add(py.Coss); break;
                        case "pet":
                            uList.Add(py.Pets.Where(p => p != 0).ToList().ListToString());
                            break;
                        case "excard": uList.Add(py.ExCards.ListToString()); break;
                        case "token": uList.Add(py.TokenCount); break;
                        case "fakeq":
                            uList.Add(py.Fakeq.Count);
                            if (uList.Count > 0)
                                uList.Add(py.Fakeq.Select(p => p.Key + "," + p.Value));
                            break;
                        case "rune": uList.Add(py.Runes.ListToString()); break;
                        case "excl": uList.Add(py.TokenExcl.ListToString()); break;
                        case "tar": uList.Add(py.TokenTars.ListToString()); break;
                        case "awake": uList.Add(py.TokenAwake ? 1 : 0); break;
                        case "foldsz": uList.Add(py.TokenFold.Count); break;
                        case "escue": uList.Add(py.Escue.ListToString()); break;
                    }
                }
            }
            return uList.Count > 0 ? ("H09G" + uList.ToString()) : "";
        }
        public string GenerateSerialFieldMessage()
        {
            StringBuilder h09p = new StringBuilder();
            h09p.Append(Eve + "," + TuxPiles.Count + "," + MonPiles.Count + "," +
                EvePiles.Count + "," + TuxDises.Count + "," + MonDises.Count + "," + EveDises.Count);
            h09p.Append("," + (Rounder != null ? Rounder.Uid : 0));
            h09p.Append("," + Supporter.Uid + "," + Hinder.Uid);
            h09p.Append("," + (Monster1 + "," + Monster2 + "," + Eve));
            if (InFight)
                h09p.Append("," + Rounder.Team + "," + CalculateRPool() + "," +
                    Rounder.OppTeam + "," + CalculateOPool());
            else
                h09p.Append("," + Rounder.Team + ",0," + Rounder.OppTeam + ",0");
            return h09p.ToString();
        }
        public string GeneratePrivateMessage(ushort ut)
        {
            Player py = Garden[ut];
            List<string> h09f = new List<string>();
            h09f.Add(py.Tux.ListToString());
            h09f.Add(py.TokenFold.ListToString());
            h09f.Add(py.Skills.ListToString());
            return string.Join(",", h09f);
        }
        #endregion Serialize the Game Situation
    }
}
