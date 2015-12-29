using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base;
using PSD.Base.Card;

namespace PSD.PSDGamepkg.Artiad
{
    public class ContentRule
    {
        public static bool IsNPCJoinable(NPC npc, HeroLib hl, Board board)
        {
            return npc.Skills.Contains("NJ01") && IsHeroJoinable(hl.InstanceHero(npc.Hero), hl, board);
        }

        public static bool IsHeroJoinable(Hero hero, HeroLib hl, Board board)
        {
            if (hero == null || hero.Group == 0)
                return false;
            Hero hrc = hl.InstanceHero(hero.Archetype);
            foreach (Player py in board.Garden.Values)
            {
                if (py.SelectHero == 0)
                    continue;
                if (py.SelectHero == hero.Avatar && py.IsAlive)
                    return false;
                foreach (int isoId in hero.Isomorphic)
                {
                    if (isoId == py.SelectHero && py.IsAlive) // hero=10202,isoId=10203,py.Sel=10203
                        return false;
                }
                Hero hpy = hl.InstanceHero(py.SelectHero);
                if (hrc != null && hpy.Avatar == hrc.Avatar)
                    return false;
                else if (hpy.Archetype == hero.Avatar)
                    return false;
            }
            foreach (int ib in board.BannedHero)
            {
                if (ib == hero.Avatar)
                    return false;
                foreach (int isoId in hero.Isomorphic)
                {
                    if (isoId == ib)
                        return false;
                }
                Hero hpy = hl.InstanceHero(ib);
                if (hrc != null && hpy.Avatar == hrc.Avatar)
                    return false;
                else if (hpy.Archetype == hero.Avatar)
                    return false;
            }
            return true;
        }
        // remove hero-spec RIM/RTM Diva, hero = 0 means removing all
        public static void ErasePlayerToken(Player player, Board board, Action<string> raiseG, int hero = 0)
        {
            if (player.TokenCount != 0)
                raiseG("G0OJ," + player.Uid + ",0," + player.TokenCount);
            if (player.TokenExcl.Count > 0)
            {
                raiseG("G2TZ,0," + player.Uid + "," + string.Join(",", player.TokenExcl));
                raiseG("G0OJ," + player.Uid + ",1," + player.TokenExcl.Count
                    + "," + string.Join(",", player.TokenExcl));
                IDictionary<char, List<ushort>> allKinds = new Dictionary<char, List<ushort>>();
                foreach (string cd in player.TokenExcl)
                {
                    if (cd[0] != 'H')
                        Util.AddToMultiMap(allKinds, cd[0], ushort.Parse(cd.Substring(1)));
                }
                if (allKinds.Count > 0)
                    raiseG("G0ON," + string.Join(",", allKinds.Select(p => player.Uid +
                        "," + p.Key + "," + p.Value.Count + "," + string.Join(",", p.Value))));
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
                raiseG("G0ON," + player.Uid + ",C," + folds.Count + "," + string.Join(",", folds));
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
            if (player.ExEquip != 0)
                excds.Add(player.ExEquip);
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
    }
}