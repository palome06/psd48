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
        public static void LoopOfNPCUntilJoinable(XI XI, Player player)
        {
            bool done = false;
            do
            {
                ushort pop = XI.Board.RestNPCPiles.Dequeue();
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
                XI.RaiseGMessage("G1NI," + player.Uid + "," + pop);
                if (Artiad.ContentRule.IsNPCJoinable(npc, XI))
                {
                    XI.RaiseGMessage("G0OY,0," + player.Uid);
                    int hp = (XI.LibTuple.HL.InstanceHero(npc.Hero).HP + 1) / 2;
                    XI.RaiseGMessage("G0IY,2," + player.Uid + "," + npc.Hero + "," + hp);
                    done = true;
                }
                XI.RaiseGMessage("G0ON,0,M,1," + pop);
            } while (XI.Board.RestNPCPiles.Count > 0 && !done);
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
                        XI.RaiseGMessage("G2QU,0," + rpsa.Length + "," +
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
    }
}
