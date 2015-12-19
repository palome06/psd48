using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.Artiad
{
    public class Procedure
    {
        public static void LoopOfNPCUntilJoinable(XI XI, Player player)
        {
            bool done = false;
            do
            {
                ushort pop = XI.Board.RestNPCPiles.Dequeue();
                NPC npc = XI.LibTuple.NL.Decode(NMBLib.OriginalNPC(pop));
                XI.RaiseGMessage("G1NI," + player.Uid + "," + pop);
                if (Artiad.ContentRule.IsNPCJoinable(npc, XI.LibTuple.HL, XI.Board))
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
            string reason, List<Player> invs, Action<IDictionary<Player, int>> action)
        {
            IDictionary<Player, int> sch = new Dictionary<Player, int>();
            while (total > 0)
            {
                if (invs.Count == 0)
                    break;
                else if (invs.Count == 1)
                {
                    string word = "#HP回复,T1(p" + invs[0].Uid + "),#回复数值,D" + total;
                    XI.AsyncInput(XI.Board.Rounder.Uid, word, reason, "0");
                    sch[invs[0]] = total;
                    total = 0; invs.Clear();
                }
                else
                {
                    string ichi = total == 1 ? "/D1" : ("/D1~" + total);
                    string word = "#HP回复,T1(p" + string.Join("p",
                        invs.Select(p => p.Uid)) + "),#回复数值," + ichi;
                    string input = XI.AsyncInput(XI.Board.Rounder.Uid, word, reason, "0");
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
            action(sch);
        }
        public static void AssignCurePointToTeam(XI XI, Player decider, int total,
            string reason, Action<IDictionary<Player, int>> action)
        {
            List<Player> invs = XI.Board.Garden.Values.Where(p => p.IsAlive &&
                    p.Team == decider.Team && p.HP < p.HPb).ToList();
            AssignCurePoint(XI, decider, total, reason, invs, action);
        }

    }
}
