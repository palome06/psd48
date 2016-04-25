using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public static class Procedure
    {
        // remove hero-spec RIM/RTM Diva, hero = 0 means removing all
        public static void ErasePlayerToken(Player player, Board board, Action<string> raiseG, int hero = 0)
        {
            if (player.TokenCount != 0)
                raiseG("G0OJ," + player.Uid + ",0," + player.TokenCount);
            if (player.TokenExcl.Count > 0)
            {
                char exclType = player.TokenExcl[0][0];
                raiseG("G2TZ,0," + player.Uid + "," + string.Join(",", player.TokenExcl));
                raiseG("G0OJ," + player.Uid + ",1," + Algo.ListToString(player.TokenExcl));
                if (exclType != 'H')
                    raiseG(new Abandon()
                    {
                        Zone = CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Char2Genre(exclType),
                        SingleUnit = new CustomsUnit()
                        {
                            Source = player.Uid,
                            Cards = player.TokenExcl.Select(p => ushort.Parse(p.Substring(1))).ToArray()
                        }
                    }.ToMessage());
            }
            if (player.TokenTars.Count > 0)
                raiseG("G0OJ," + player.Uid + ",2," + player.TokenTars.Count
                    + "," + string.Join(",", player.TokenTars));
            if (player.TokenAwake)
                raiseG("G0OJ," + player.Uid + ",3");
            if (player.TokenFold.Count > 0)
            {
                List<ushort> folds = player.TokenFold.ToList();
                raiseG("G2TZ,0," + player.Uid + "," + string.Join(",", folds.Select(p => "C" + p)));
                raiseG("G0OJ," + player.Uid + ",4," + folds.Count + "," + string.Join(",", folds));
                raiseG(new Abandon()
                {
                    Zone = CustomsHelper.ZoneType.PLAYER,
                    Genre = Card.Genre.Tux,
                    SingleUnit = new CustomsUnit() { Source = player.Uid, Cards = folds.ToArray() }
                }.ToMessage());
            }
            player.ResetROM(board, hero);
            // Remove others' tar token on the player
            foreach (Player py in board.Garden.Values)
            {
                if (py.IsAlive && py != player && py.TokenTars.Contains(player.Uid))
                    raiseG("G0OJ," + py.Uid + ",2,1," + player.Uid);
            }
        }

        public static List<string> IncrGuestPlayer(Player player, Hero guest, XI xi, int limit)
        {
            List<string> skills = new List<string>();
            List<string> source = guest.Skills.ToList();
            foreach (string skstr in source)
            {
                Skill skill = xi.LibTuple.SL.EncodeSkill(skstr);
                if (skill.IsChange && (limit & 0x1) != 0)
                    continue;
                if (skill.IsRestrict && (limit & 0x2) != 0)
                    continue;
                skills.Add(skill.Code);
            }
            if (skills.Count > 0)
                xi.RaiseGMessage("G0IS," + player.Uid + ",1," + string.Join(",", skills));
            return skills;
        }

        public static void DecrGuestPlayer(Player player, Hero guest, XI xi, int limit)
        {
            List<ushort> excds = new List<ushort>();
            excds.AddRange(player.ExCards);
            if (excds.Count > 0)
                xi.RaiseGMessage("G0QZ," + player.Uid + "," + string.Join(",", excds));
            ErasePlayerToken(player, xi.Board, xi.RaiseGMessage, guest.Avatar);

            List<string> skills = new List<string>();
            List<string> source = guest.Skills.ToList(); source.AddRange(guest.RelatedSkills);
            foreach (string skstr in source)
            {
                Skill skill = xi.LibTuple.SL.EncodeSkill(skstr);
                if (skill.IsChange && (limit & 0x1) != 0)
                    continue;
                if (skill.IsRestrict && (limit & 0x2) != 0)
                    continue;
                skills.Add(skill.Code);
            }
            if (skills.Count > 0)
                xi.RaiseGMessage("G0OS," + player.Uid + ",1," + string.Join(",", skills));
        }

        public static void LoopOfNPCUntilJoinable(XI XI, Player player)
        {
            ushort pop = CardHunter(XI, Card.Genre.NMB, (p) => ContentRule.IsNPCJoinable(
                XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(p)), XI), (a, r) => a.Count == 1, true).Single();
            NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
            XI.RaiseGMessage("G0OY,0," + player.Uid);
            int hp = (XI.LibTuple.HL.InstanceHero(npc.Hero).HP + 1) / 2;
            XI.RaiseGMessage("G0IY,2," + player.Uid + "," + npc.Hero + "," + hp);
            XI.RaiseGMessage(new Abandon()
            {
                Zone = CustomsHelper.ZoneType.SHOWBOARD,
                Genre = Card.Genre.NMB,
                SingleUnit = new CustomsUnit() { SingleCard = pop }
            }.ToMessage());
        }

        public static void AssignCurePoint(XI XI, Player decider, int total,
            string reason, List<Player> invs, Action<IDictionary<Player, int>> cureAction)
        {
            IDictionary<Player, int> sch = new Dictionary<Player, int>();
            while (total > 0)
            {
                if (invs.Count == 0)
                    break;
                else if (invs.Count == 1)
                {
                    string word = "#HP回复,T1(p" + invs[0].Uid + "),#回复数值,D" + total;
                    XI.AsyncInput(decider.Uid, word, reason, "0");
                    sch[invs[0]] = total;
                    total = 0; invs.Clear();
                }
                else
                {
                    string ichi = total == 1 ? "/D1" : ("/D1~" + total);
                    string word = "#HP回复,T1(p" + string.Join("p",
                        invs.Select(p => p.Uid)) + "),#回复数值," + ichi;
                    string input = XI.AsyncInput(decider.Uid, word, reason, "0");
                    if (!input.Contains("/"))
                    {
                        string[] ips = input.Split(',');
                        ushort ut = ushort.Parse(ips[0]);
                        int zn = int.Parse(ips[1]);
                        Player py = XI.Board.Garden[ut];
                        sch[py] = zn;
                        total -= zn;
                        invs.Remove(py);
                    }
                }
            }
            cureAction(sch);
        }
        public static void AssignCurePointToTeam(XI XI, Player decider, int total,
            string reason, Action<IDictionary<Player, int>> cureAction)
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == decider.Team && p.HP < p.HPb).ToList();
            AssignCurePoint(XI, decider, total, reason, invs, cureAction);
        }

        public static bool UseCardDirectly(Player player, ushort cardUt,
            string fuse, XI xi, ushort provider)
        {
            Tux tux = xi.LibTuple.TL.DecodeTux(cardUt);
            if (!ContentRule.IsTuxUsableEveryWhere(tux))
                return false;
            if (tux.IsTuxEqiup())
                xi.RaiseGMessage("G1UE," + player.Uid + "," + provider + "," + cardUt);
            else
            {
                ushort who = player.Uid;
                xi.RaiseGMessage("G0CC," + provider + ",0," + who +
                    "," + tux.Code + "," + cardUt + ";0," + fuse);
            }
            return true;
        }

        public static void ArticuloMortis(XI xi, Base.VW.IWISV wi, bool notify)
        {
            List<ushort> zeros = xi.Board.Garden.Values.Where(
                p => p.IsAlive && p.HP == 0).Select(p => p.Uid).ToList();
            if (zeros.Count > 0)
            {
                if (notify)
                    wi.BCast("E0ZH," + string.Join(",", zeros));
                xi.RaiseGMessage("G0ZH,0");
            }
        }

        public static void RotateHarm(Player from, Player to,
            bool flatten, Func<int, int> valFunc, ref List<Artiad.Harm> harms)
        {
            Artiad.Harm rotation = null;
            foreach (Artiad.Harm harm in harms)
            {
                if (harm.Who == from.Uid && harm.N > 0 && !HPEvoMask.IMMUNE_INVAO
                    .IsSet(harm.Mask) && !HPEvoMask.DECR_INVAO.IsSet(harm.Mask))
                {
                    rotation = harm;
                }
            }
            if (rotation != null)
            {
                harms.Remove(rotation);
                if (flatten && HPEvoMask.TERMIN_AT.IsSet(rotation.Mask))
                    rotation.Mask = HPEvoMask.TERMIN_AT.Reset(rotation.Mask);
                rotation.N = valFunc(rotation.N);
                foreach (Artiad.Harm harm in harms)
                {
                    if (harm.Who == to.Uid)
                    {
                        bool ta1 = HPEvoMask.TERMIN_AT.IsSet(harm.Mask);
                        bool ta2 = HPEvoMask.TERMIN_AT.IsSet(rotation.Mask);
                        // Swallowed A with the termin_at harm
                        if (ta1 && ta2)
                        {
                            harm.N = Math.Max(harm.N, rotation.N);
                            rotation = null;
                        }
                        else if (ta2)
                        {
                            HPEvoMask.TERMIN_AT.Set(harm.N);
                            harm.N = rotation.N;
                            rotation = null;
                        }
                        else if (ta1) { rotation = null; }
                        else if (harm.Element == rotation.Element)
                        {
                            harm.N += rotation.N;
                            rotation = null;
                        }
                    }
                    if (rotation == null) { break; }
                }
            }
            if (rotation != null)
            {
                rotation.Who = to.Uid;
                harms.Add(rotation);
            }
        }

        public static void ObtainAndAllocateTux(XI XI, Base.VW.IVI vi, Player py,
            int count, string reason, string inType)
        {
            if (count == 0)
                return;
            List<ushort> rps = XI.Board.Garden.Values.Where(p => p.IsAlive
                && p.Team == py.Team).Select(p => p.Uid).ToList();
            ushort[] rpsa = XI.Board.Garden.Values.Where(p => p.Team == py.Team).Select(p => p.Uid).ToArray();
            string rg = string.Join(",", rps), rf = "(p" + string.Join("p", rps) + ")";

            List<ushort> pops = XI.DequeueOfPile(XI.Board.TuxPiles, count).ToList();
            XI.RaiseGMessage("G2IN,0," + count);
            XI.RaiseGMessage("G1IU," + string.Join(",", pops));
            do
            {
                XI.RaiseGMessage("G2FU,0," + py.Uid + "," + rps.Count + "," + rg + ",C," + string.Join(",", pops));
                int pubSz = XI.Board.PZone.Count;
                string pubDig = (pubSz > 1) ? ("+Z1~" + pubSz) : "+Z1";
                string input = XI.AsyncInput(py.Uid, pubDig + "(p" +
                    string.Join("p", XI.Board.PZone) + "),#获得卡牌的,/T1" + rf, reason, inType);
                if (!input.StartsWith("/") && input != vi.CinSentinel)
                {
                    string[] ips = input.Split(',');
                    List<ushort> getxs = Algo.TakeRange(ips, 0, ips.Length - 1).Select(p => ushort.Parse(p))
                        .Where(p => XI.Board.PZone.Contains(p)).ToList();
                    ushort to = ushort.Parse(ips[ips.Length - 1]);
                    if (getxs.Count > 0)
                    {
                        XI.RaiseGMessage("G1OU," + string.Join(",", getxs));
                        XI.RaiseGMessage("G2QU,0,C," + rpsa.Length + "," +
                             string.Join(",", rpsa) + "," + string.Join(",", getxs));
                        XI.RaiseGMessage("G0HQ,2," + to + ",0," + rpsa.Length + "," +
                            string.Join(",", rpsa) + "," + string.Join(",", getxs));
                        foreach (ushort getx in getxs)
                            pops.Remove(getx);
                    }
                }
                XI.RaiseGMessage("G2FU,3");
            } while (pops.Count > 0);
        }

        public static bool LocustChangePendingTux(XI XI, ushort provider, ushort locuster, ushort locustee)
        {
            if (XI.Board.PendingTux.Contains(provider + ",G0CC," + locustee))
            {
                XI.Board.PendingTux.Remove(provider + ",G0CC," + locustee);
                XI.Board.PendingTux.Enqueue(locuster + ",G0CC," + locustee);
                return true;
            }
            else
                return false;
        }

        public static void SetPlayerAllEqDisable(XI XI, IEnumerable<Player> pys, int eqTypeMask, string reason)
        {
            SetPlayerAllEqAblity(XI, pys.ToList(), eqTypeMask, reason, false);
        }
        public static void SetPlayerAllEqEnable(XI XI, IEnumerable<Player> pys, int eqTypeMask, string reason)
        {
            SetPlayerAllEqAblity(XI, pys.ToList(), eqTypeMask, reason, true);
        }
        private static void SetPlayerAllEqAblity(XI XI, List<Player> pys,
             int eqTypeMask, string reason, bool enabled)
        {
            List<Artiad.CardAsUnit> caus = new List<Artiad.CardAsUnit>();
            foreach (Player py in pys)
            {
                if ((eqTypeMask & 0x1) != 0 && py.WeaponDisabled == enabled)
                {
                    if (py.Weapon != 0)
                        caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.Weapon });
                    if ((py.ExMask & 0x1) != 0 && py.ExEquip != 0)
                        caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.ExEquip });
                    if (py.Trove != 0)
                    {
                        TuxEqiup trove = XI.LibTuple.TL.DecodeTux(py.Trove) as TuxEqiup;
                        if (trove.IsIllusion())
                        {
                            Tux ilas = XI.LibTuple.TL.EncodeTuxCode((trove as Illusion).ILAS);
                            if (ilas != null && ilas.Type == Tux.TuxType.WQ)
                                caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.Trove, CardAs = ilas.Code });
                        }
                    }
                }
                if ((eqTypeMask & 0x2) != 0 && py.ArmorDisabled == enabled)
                {
                    if (py.Armor != 0)
                        caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.Armor });
                    if ((py.ExMask & 0x2) != 0 && py.ExEquip != 0)
                        caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.ExEquip });
                    if (py.Trove != 0)
                    {
                        TuxEqiup trove = XI.LibTuple.TL.DecodeTux(py.Trove) as TuxEqiup;
                        if (trove.IsIllusion())
                        {
                            Tux ilas = XI.LibTuple.TL.EncodeTuxCode((trove as Illusion).ILAS);
                            if (ilas != null && ilas.Type == Tux.TuxType.FJ)
                                caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.Trove, CardAs = ilas.Code });
                        }
                    }
                }
                if ((eqTypeMask & 0x4) != 0 && py.WeaponDisabled == enabled)
                {
                    if (py.Trove != 0)
                        caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.Trove });
                    if ((py.ExMask & 0x4) != 0 && py.ExEquip != 0)
                        caus.Add(new Artiad.CardAsUnit() { Who = py.Uid, Card = py.ExEquip });
                }
            }
            if (caus.Count > 0)
            {
                if (enabled)
                    XI.RaiseGMessage(new Artiad.EquipIntoForce() { Imports = caus.ToArray() }.ToMessage());
                else
                    XI.RaiseGMessage(new Artiad.EquipOutofForce() { Exports = caus.ToArray() }.ToMessage());
            }
            if ((eqTypeMask & 0x1) != 0)
                pys.ForEach(p => p.SetWeaponDisabled(reason, !enabled));
            if ((eqTypeMask & 0x2) != 0)
                pys.ForEach(p => p.SetArmorDisabled(reason, !enabled));
            if ((eqTypeMask & 0x4) != 0)
                pys.ForEach(p => p.SetTroveDisabled(reason, !enabled));
        }
        /// <summary>
        /// hunt for specificed cards from piles
        /// </summary>
        /// <param name="XI">XI</param>
        /// <param name="genre">card genre, enum of Card.Genre</param>
        /// <param name="acceptRule">the rule func(tux-ut) for accepting a card or not</param>
        /// <param name="finishRule">the rule func(acceptList, rejectList) for finishing the hunting</param>
        /// <param name="acceptIncomplete">whether accept results even through finishes forcibly</param>
        /// <return>the accepted list</return>
        public static List<ushort> CardHunter(XI XI, Card.Genre genre, Func<ushort, bool> acceptRule,
            Func<List<ushort>, List<ushort>, bool> finishRule, bool acceptIncomplete)
        {
            Base.Utils.Rueue<ushort> pile = null; // maybe get from the args
            int pileSerial = 0; int ymSerial = 0;
            if (genre == Card.Genre.Tux)
            {
                pile = XI.Board.TuxPiles; pileSerial = 0; ymSerial = 8;
            }
            else if (genre == Card.Genre.NMB)
            {
                pile = XI.Board.MonPiles; pileSerial = 1; ymSerial = 5;
            }
            else return new List<ushort>();
            
            List<ushort> accepts = new List<ushort>();
            List<ushort> rejects = new List<ushort>();
            while (pile.Count > 0 && !finishRule(accepts, rejects))
            {
                ushort ut = XI.DequeueOfPile(pile);
                XI.RaiseGMessage("G2IN," + pileSerial + ",1");
                XI.RaiseGMessage("G0YM," + ymSerial + "," + ut);
                if (acceptRule(ut)) { accepts.Add(ut); }
                else { rejects.Add(ut); }
                XI.RaiseGMessage("G2ZZ,0");
            }
            if (!acceptIncomplete && !finishRule(accepts, rejects))
            {
                rejects.AddRange(accepts);
                accepts.Clear();
            }
            if (rejects.Count > 0)
                XI.RaiseGMessage(new Abandon()
                {
                    Zone = CustomsHelper.ZoneType.SHOWBOARD,
                    Genre = genre,
                    SingleUnit = new CustomsUnit() { Cards = rejects.ToArray() }
                }.ToMessage());
            return accepts;
        }
    }
}
