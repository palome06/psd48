using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base.Card;

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
        public string GenerateSerialGamerMessage(LibGroup tuple)
        {
            StringBuilder h09g = new StringBuilder();
            foreach (Player py in Garden.Values)
            {
                h09g.Append("," + py.Uid + "," + py.SelectHero);
                int state = 0;
                state |= !py.IsAlive ? 0 : 1;
                state |= !py.Loved ? 0 : 2;
                state |= !py.Immobilized ? 0 : 4;
                state |= !py.PetDisabled ? 0 : 8;
                h09g.Append("," + state);
                h09g.Append("," + py.HP + "," + py.HPb + "," + py.STR + ","
                    + (InFight ? py.STRa : py.STR) + "," + py.DEX + ","
                    + (InFight ? py.DEXa : py.DEX) + "," + py.Tux.Count
                    + "," + py.Weapon + "," + py.Armor + "," + py.Trove + "," + py.ExEquip);
                Card.Luggage lug = tuple.TL.DecodeTux(py.Trove) as Card.Luggage;
                if (lug != null && lug.Capacities.Count > 0)
                    h09g.Append("," + lug.Capacities.Count + "," + string.Join(",", lug.Capacities));
                else
                    h09g.Append(",0");
                h09g.Append("," + py.Guardian + "," + (py.Coss.Count > 0 ? py.Coss.Peek() : 0));
                h09g.Append("," + string.Join(",", py.Pets));
                h09g.Append("," + py.ExCards.Count);
                if (py.ExCards.Count > 0)
                    h09g.Append("," + string.Join(",", py.ExCards));
                //h09g.Append("," + py.Fakeq.Count);
                //if (py.Fakeq.Count > 0)
                //    h09g.Append("," + string.Join(",", py.Fakeq.Select(p => p.Key + "," + p.Value)));
                h09g.Append("," + py.Runes.Count);
                if (py.Runes.Count > 0)
                    h09g.Append("," + string.Join(",", py.Runes));
                h09g.Append("," + py.TokenCount);
                h09g.Append("," + py.TokenExcl.Count);
                if (py.TokenExcl.Count > 0)
                    h09g.Append("," + string.Join(",", py.TokenExcl));
                h09g.Append("," + py.TokenTars.Count);
                if (py.TokenTars.Count > 0)
                    h09g.Append("," + string.Join(",", py.TokenTars));
                h09g.Append("," + (py.TokenAwake ? "1" : "0"));
                h09g.Append("," + py.TokenFold.Count);
                h09g.Append("," + py.Escue.Count);
                if (py.Escue.Count > 0)
                    h09g.Append("," + string.Join(",", py.Escue));
            }
            return h09g.Length > 0 ? h09g.ToString().Substring(1) : "";
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
            h09f.Add(py.Tux.Count.ToString());
            if (py.Tux.Count > 0)
                h09f.Add(string.Join(",", py.Tux));
            h09f.Add(py.TokenFold.Count.ToString());
            if (py.TokenFold.Count > 0)
                h09f.Add(string.Join(",", py.TokenFold));
            h09f.Add(py.Skills.Count.ToString());
            if (py.Skills.Count > 0)
                h09f.Add(string.Join(",", py.Skills));
            return string.Join(",", h09f);
        }
        #endregion Serialize the Game Situation
    }
}
