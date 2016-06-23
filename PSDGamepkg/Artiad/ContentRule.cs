using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base;
using PSD.Base.Card;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public static class ContentRule
    {
        public static bool IsNPCJoinable(NPC npc, XI xi)
        {
            return npc.Skills.Contains("NJ01") &&
                IsHeroJoinable(xi.LibTuple.HL.InstanceHero(npc.Hero), xi);
        }
        // whether the hero can be called out directly
        public static bool IsHeroCallable(Hero hero, XI xi)
        {
            return hero != null && xi.PCS.ListAllSeleableHeros().Contains(hero) &&
                !IsHeroPhantomExist(hero, xi);
        }
        // whether the hero can join the team via NPC action
        public static bool IsHeroJoinable(Hero hero, XI xi)
        {
            return hero != null && xi.PCS.ListAllJoinableHeroes().Contains(hero) &&
                !IsHeroPhantomExist(hero, xi);
        }
        // Whether a hero's self/iso/banned exist in the field
        private static bool IsHeroPhantomExist(Hero hero, XI xi)
        {
            HeroLib hl = xi.LibTuple.HL;
            Board board = xi.Board;
            Hero hrc = hl.InstanceHero(hero.Archetype);
            foreach (Player py in board.Garden.Values)
            {
                if (py.SelectHero == 0)
                    continue;
                if (py.SelectHero == hero.Avatar && py.IsAlive)
                    return true;
                foreach (int isoId in hero.Isomorphic)
                {
                    if (isoId == py.SelectHero && py.IsAlive) // hero=10202,isoId=10203,py.Sel=10203
                        return true;
                }
                Hero hpy = hl.InstanceHero(py.SelectHero);
                if (hrc != null && hpy.Avatar == hrc.Avatar)
                    return true;
                else if (hpy.Archetype == hero.Avatar)
                    return true;
            }
            foreach (int ib in board.BannedHero)
            {
                if (ib == hero.Avatar)
                    return true;
                foreach (int isoId in hero.Isomorphic)
                {
                    if (isoId == ib)
                        return true;
                }
                Hero hpy = hl.InstanceHero(ib);
                if (hrc != null && hpy.Avatar == hrc.Avatar)
                    return true;
                else if (hpy.Archetype == hero.Avatar)
                    return true;
            }
            return false;
        }

        public static List<Hero> GetCallableHeroChain(Hero hero, XI xi)
        {
            List<Hero> heros = new List<Hero>();
            if (hero == null)
                return heros;
            if (hero.Pioneer != 0) // maybe sustitiude it with its pioneer
            {
                Console.WriteLine("Here comes a pioneer " + hero.Pioneer + "(" + hero.Avatar + ")");
                Hero pioneer = xi.LibTuple.HL.InstanceHero(hero.Pioneer);
                if (pioneer != null && xi.PCS.ListAllSeleableHeros().Contains(pioneer))
                    hero = pioneer;
            }
            if (IsHeroCallable(hero, xi))
                heros.Add(hero);
            foreach (int isoId in hero.Isomorphic)
            {
                Hero iso = xi.LibTuple.HL.InstanceHero(isoId);
                if (iso != null && IsHeroCallable(iso, xi))
                    heros.Add(iso);
            }
            return heros;
        }

        public static ILookup<ushort, ushort> GetPetOwnershipTable(IEnumerable<ushort> pets, XI xi)
        {
            return pets.ToList().ToLookup(p => GetPetOwnership(p, xi), p => p);
        }
        public static ushort GetPetOwnership(ushort pet, XI xi)
        {
            Monster mon = xi.LibTuple.ML.Decode(pet);
            int idx = mon.Element.Elem2Index();
            Player py = xi.Board.Garden.Values.SingleOrDefault(p => p.Pets[idx] == pet);
            return py != null ? py.Uid : (ushort)0;
        }
        public static ushort GetExspOwnership(ushort exsp, XI xi)
        {
            Player py = xi.Board.Garden.Values.SingleOrDefault(p => p.TokenExcl.Contains("I" + exsp));
            return py != null ? py.Uid : (ushort)0;
        }
        public static ushort GetEquipmentOwnership(string equipCode, XI xi)
        {
            ushort ut = (xi.LibTuple.TL.EncodeTuxCode(equipCode) as TuxEqiup).SingleEntry;
            Player py = xi.Board.Garden.Values.SingleOrDefault(p => p.ListOutAllEquips().Contains(ut));
            return py != null ? py.Uid : (ushort)0;
        }
        public static bool FindCardExistance(ushort ut, Card.Genre genre, XI xi, bool tared)
        {
            if (genre == Card.Genre.Tux)
            {
                if (xi.Board.TuxDises.Contains(ut))
                    return true;
                foreach (Player py in xi.Board.Garden.Values.Where(p => !tared || p.IsTared))
                {
                    foreach (ushort eq in py.ListOutAllEquips())
                    {
                        if (eq == ut)
                            return true;
                        Tux tux = xi.LibTuple.TL.DecodeTux(eq);
                        if (tux.IsTuxEqiup())
                        {
                            TuxEqiup tue = tux as TuxEqiup;
                            if (tue.IsLuggage())
                            {
                                Luggage lg = tue as Luggage;
                                if (lg.Capacities.Contains("C" + ut))
                                    return true;
                            }
                        }
                    }
                    if (py.TokenExcl.Contains("C" + ut))
                        return true;
                }
            }
            else if (genre == Card.Genre.NMB)
            {
                if (xi.Board.MonDises.Contains(ut))
                    return true;
                foreach (Player py in xi.Board.Garden.Values.Where(p => !tared || p.IsTared))
                {
                    if (py.Pets.Contains(ut)) return true;
                    if (py.Escue.Contains(ut)) return true;
                    foreach (ushort eq in py.ListOutAllBaseEquip())
                    {
                        TuxEqiup tue = xi.LibTuple.TL.DecodeTux(eq) as TuxEqiup;
                        if (tue.IsLuggage())
                        {
                            Luggage lg = tue as Luggage;
                            if (lg.Capacities.Contains("M" + ut))
                                return true;
                        }
                    }
                    if (py.TokenExcl.Contains("M" + ut))
                        return true;
                }
            }
            return false;
        }

        #region Sparse Base Rules

        public static bool IsTuxUsableEveryWhere(Tux tux)
        {
            return tux.Type != Tux.TuxType.ZP && !new string[] {
                "TP01", "TP03", "TPT1", "TPT3", "TPH2", "TPH3", "TPH4" }.Contains(tux.Code);
        }

        public static int GetLocustFreeType(string tuxCode, int oldType)
        {
            if (tuxCode == "JP05" && oldType == 1)
                return 0;
            else if (tuxCode == "TP02" && oldType == 1)
                return 0;
            else if (tuxCode == "TPT4" && oldType == 1)
                return 2;
            else
                return oldType;
        }

        public static void LoadDefaultPrice(Player player)
        {
            player.ClearPrice();
            player.AddToPrice("JP03", false, "0", '=', 1);
            player.AddToPrice("WQ04", false, "0", '=', 2);
            player.AddToPrice("WQ04", true, "0", '=', 2);
            player.AddToPrice("JPH4", false, "0", '=', 2);
        }

        public static bool IsTuxVestige(string tuxCode, int type)
        {
            if (tuxCode == "TPT2" && type == 0)
                return true;
            else if (tuxCode == "TPT3" && type == 0)
                return true;
            else
                return false;
        }
        // convert nmb to a virtual battle attender
        public static Player Lumberjack(NMB nmb, ushort orgCode, int team)
        {
            return Player.Warriors(nmb.Name, orgCode + 1000, team, nmb.STR, nmb.AGL);
        }
        // convert exsp to a virtual battle attender
        public static Player RobinHood(string exspName, ushort exspUt, int team)
        {
            int str = 0, dex = 0;
            if (exspUt == 25) { str = 2; dex = 4; }
            return Player.Warriors(exspName, exspUt + 3000, team, str, dex);
        }
        // convert ushort to a corresponding player/lumberjack/robinhood/...
        public static Player DecodePlayer(ushort ut, XI xi)
        {
            Player py;
            if (ut == 0)
                py = null;
            else if (ut > 0 && ut < 1000)
                py = xi.Board.Garden[ut];
            else if (ut < 2000) // Monster
            {
                ushort mut = (ushort)(ut - 1000);
                NMB nmb = NMBLib.Decode(mut, xi.LibTuple.ML, xi.LibTuple.NL);
                if (nmb != null)
                {
                    Player owner = xi.Board.Garden.Values.Single(p => p.Pets.Contains(mut));
                    py = Artiad.ContentRule.Lumberjack(nmb, mut, owner.Team);
                }
                else
                    py = null;
            }
            else if (ut < 3000) { py = null; } // Npc
            else // Exsp
            {
                ushort esut = (ushort)(ut - 3000);
                string escode = "I" + esut;
                Exsp exsp = xi.LibTuple.ESL.Encode(escode);
                if (exsp != null)
                {
                    Player owner = xi.Board.Garden.Values.Single(p => p.TokenExcl.Contains(escode));
                    py = Artiad.ContentRule.RobinHood(exsp.Name, esut, owner.Team);
                }
                else
                    py = null;
            }
            return py;
        }
        // judge whether the fuse is matched
        public static bool IsFuseMatch(string rawFuse, string fuse, Board board)
        {
            string r = board.Rounder.Uid.ToString();
            fuse = Algo.Substring(fuse, 0, fuse.IndexOf(','));
            return rawFuse == fuse || (rawFuse.Replace("#", r) == fuse) ||
                board.Garden.Keys.Any(p => p != board.Rounder.Uid && rawFuse.Replace("$", r) == fuse) ||
                board.Garden.Keys.Any(p => rawFuse.Replace("*", r) == fuse);
        }
        // check whether $player's $linkHead is suitable for $tux, not consider the pureFuse
        public static int GetTuxTypeFromLink(string linkFuse, Tux tux, Player player, Board board)
        {
            string pureFuse;
            return GetTuxTypeFromLink(linkFuse, tux, player, board, out pureFuse);
        }
        // check whether $player's $linkHead is suitable for $tux, not consider the pureFuse
        public static int GetTuxTypeFromLink(string linkFuse, Tux tux, Player provider, Player user, Board board)
        {
            string pureFuse;
            return GetTuxTypeFromLink(linkFuse, tux, provider, user, board, out pureFuse);
        }
        // check whether $player's $linkHead is suitable for $tux, then return the pureType
        public static int GetTuxTypeFromLink(string linkFuse, Tux tux,
            Player player, Board board, out string pureFuse)
        {
            return GetTuxTypeFromLink(linkFuse, tux, player, player, board, out pureFuse);
        }
        // check whether $player's $linkHead is suitable for $tux, then return the pureType
        public static int GetTuxTypeFromLink(string linkFuse, Tux tux,
            Player provider, Player user, Board board, out string pureFuse)
        {
            int idx = linkFuse.IndexOf(":");
            pureFuse = linkFuse.Substring(idx + 1);
            if (tux == null || idx <= 0)
                return -1;
            string[] linkHeads = Algo.Substring(linkFuse, 0, idx).Split('&');
            foreach (string linkHead in linkHeads)
            {
                string[] lh = linkHead.Split(',');
                string pureName = lh[0], pureTypeStr = lh[1], rawOc = lh[2];
                if (!pureTypeStr.Contains("!") && IsFuseMatch(rawOc, pureFuse, board))
                {
                    int pureType = int.Parse(pureTypeStr);
                    if (tux.Code == pureName && tux.Bribe(provider, pureType, pureFuse)
                                && tux.Valid(user, pureType, pureFuse))
                        return pureType;
                }
            }
            return -1;
        }

        public static string[] GetIllusionResult(string tuxCode)
        {
            if (tuxCode == "XBT5")
                return new string[] { "WQ01", "FJT1" };
            else if (tuxCode == "XBT6")
                return new string[] { "WQ05", "FJ01" };
            else if (tuxCode == "XBT7")
                return new string[] { "WQ03", "FJ05" };
            else
                return new string[] { };
        }

        #endregion Sparse Base Rules
    }
}