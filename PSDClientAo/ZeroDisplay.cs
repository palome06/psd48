using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base;

namespace PSD.ClientAo
{
    public class ZeroDisplay
    {
        private XIVisi xiv;

        private LibGroup tuple;

        public ZeroDisplay(XIVisi xiv) {
            this.xiv = xiv;
            this.tuple = xiv.Tuple;
        }

        #region Utils
        internal string SKTXCZ(string ops) { return SKTXCZ(ops, false, "0"); }
        internal string SKTXCZ(string ops, bool hind, string inType)
        {
            int idx = ops.IndexOf(',');
            string title = Util.Substring(ops, 0, idx);
            string result = "";
            if (title.StartsWith("JN"))
            {
                int jdx = title.IndexOf('('), kdx = title.IndexOf(')');
                string jnName = Util.Substring(title, 0, jdx);
                string arg = jdx < 0 ? "" : "(" + Util.Substring(title, jdx + 1, kdx) + ")";
                var skill = tuple.SL.EncodeSkill(jnName);
                int inTypeInt;
                if (!int.TryParse(inType, out inTypeInt))
                    inTypeInt = 0;
                if (skill != null && (!hind || !skill.IsHind[inTypeInt]))
                    result = title + ":" + skill.Name + (arg == "" ? "" : arg);
                else
                    return "";
            }
            else if (title.StartsWith("CZ"))
                result = title + ":" + tuple.ZL.EncodeOps(title).Name;
            else if (title.StartsWith("TX"))
                result = title + ":" + tuple.TL.DecodeTux(ushort.Parse(title.Substring("TX".Length))).Name;
            else if (title.StartsWith("NJ"))
                result = title + ":" + tuple.NJL.EncodeNCAction(title).Name;
            else if (title.StartsWith("PT"))
                result = title + ":" + tuple.ML.Decode(ushort.Parse(title.Substring("PT".Length))).Name;
            else if (title.StartsWith("SJ"))
                result = title + ":" + tuple.EL.GetEveFromName(title).Name;
            else
                result = title;
            if (idx >= 0)
                result += "(" + ops.Substring(idx + 1) + ")";
            return result;
        }
        internal string Tux(ushort card)
        {
            return card == 0 ? "0:无" : card + ":" + tuple.TL.DecodeTux(card).Name;
        }
        internal string Tux(IEnumerable<ushort> cards)
        {
            if (!cards.Any())
                return "{}";
            return "{" + string.Join(",", cards.Select(p => Tux(p))) + "}";
        }
        internal string Tux(string cardName)
        {
            return cardName + ":" + tuple.TL.Firsts.Find(p => p.Code == cardName).Name;
        }
        internal string PurePlayer(ushort player)
        {
            return player == 0 ? "取消" : (player < 1000 ? tuple.HL.InstanceHero(
                xiv.A0P[player].SelectHero).Name : Monster((ushort)(player - 1000)));
        }
        internal string Player(ushort player)
        {
            return player == 0 ? "0:天上" : (player < 1000 ? player + ":" + tuple.HL
                .InstanceHero(xiv.A0P[player].SelectHero).Name : Monster((ushort)(player - 1000)));
        }
        internal string Player(IEnumerable<ushort> players)
        {
            if (!players.Any())
                return "{}";
            return "{" + string.Join(",", players.Select(p => Player(p))) + "}";
        }
        internal string PlayerWithMonster(IEnumerable<string> strings) // used only for XJ107 and GT03
        {
            if (!strings.Any())
                return "{}";
            return "{" + string.Join(",", strings.Select(p => p.StartsWith("!") ?
                (p.Substring(1) + ":" + tuple.ML.Decode(ushort.Parse(p.Substring("!PT".Length))).Name)
                : Player(ushort.Parse(p)))) + "}";
        }
        internal string Monster(ushort p)
        {
            return (p == 0) ? "0:没" : p + ":" + Base.Card.NMBLib.Decode(p, tuple.ML, tuple.NL).Name;
        }
        internal string Monster(IEnumerable<ushort> mons)
        {
            if (!mons.Any())
                return "{}";
            var ma = mons.Select(p => Monster(p));
            return "{" + string.Join(",", ma) + "}";
            //return "{" + string.Join(",", mons.Select(p => Monster(p))) + "}";
        }
        internal string Eve(ushort eve)
        {
            return (eve == 0) ? "0:静" : eve + ":" + tuple.EL.DecodeEvenement(eve).Name;
        }
        internal string Eve(IEnumerable<ushort> eves)
        {
            if (!eves.Any())
                return "{}";
            return "{" + string.Join(",", eves.Select(p => Eve(p))) + "}";
        }
        internal string MixedCards(string code)
        {
            if (code.StartsWith("H"))
                return Hero(int.Parse(code.Substring("H".Length)));
            else if (code.StartsWith("I"))
                return ExspI(int.Parse(code.Substring("I".Length)));
            else
                return null;
        }
        internal string MixedCards(IEnumerable<string> codes)
        {
            if (!codes.Any())
                return "{}";
            var co = codes.Select(p => MixedCards(p));
            return "{" + string.Join(",", co) + "}";
        }
        internal string ExspI(int code)
        {
            Base.Card.Exsp exsp = tuple.ESL.Encode("I" + code);
            return (exsp != null) ? exsp.Name : "喵";
        }
        internal string ExspIWithCode(int code)
        {
            return code + ":" + ExspI(code);
        }
        internal string ExspIWithCode(IEnumerable<int> codes)
        {
            if (!codes.Any())
                return "{}";
            return "{" + string.Join(",", codes.Select(p => ExspIWithCode(p))) + "}";
        }
        internal string Hero(int hero)
        {
            return hero == 0 ? "姚仙" : tuple.HL.InstanceHero(hero).Name;
        }
        internal string Hero(IEnumerable<int> heros)
        {
            if (!heros.Any())
                return "{}";
            var ho = heros.Select(p => Hero(p));
            return "{" + string.Join(",", ho) + "}";
        }
        internal string HeroWithCode(int hero)
        {
            Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
            return (hero == 0 || hro == null) ? "0:姚仙" : hro.Avatar + ":" + hro.Name;
        }
        internal string HeroWithCode(IEnumerable<int> heros)
        {
            if (!heros.Any())
                return "{}";
            var ho = heros.Select(p => HeroWithCode(p)).ToList();
            StringBuilder sb = new StringBuilder();
            sb.Append("\n\r");
            for (int i = 0; i < ho.Count; ++i)
            {
                sb.Append(ho[i]);
                sb.Append(i % 4 == 3 ? "\n\r" : ",");
            }
            sb.Remove(sb.Length - 1, 1);
            return "{" + sb.ToString() + "}";
        }
        internal string HeroTokenAlias(int hero)
        {
            Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
            if (hro != null)
                return hro.TokenAlias ?? "标记";
            else
                return "标记";
        }
        internal string HeroPeopleAlias(int hero)
        {
            Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
            if (hro != null)
                return hro.PeopleAlias ?? "人物牌";
            else
                return "人物牌";
        }
        internal string HeroPlayerTarAlias(int hero)
        {
            Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
            if (hro != null)
                return hro.PlayerTarAlias ?? "目标角色";
            else
                return "目标角色";
        }
        internal string HeroExCardAlias(int hero)
        {
            Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
            if (hro != null)
                return hro.ExCardsAlias ?? "特殊手牌";
            else
                return "特殊手牌";
        }
        internal string Prop(ushort prop)
        {
            switch (prop)
            {
                case 1: return "水";
                case 2: return "火";
                case 3: return "雷";
                case 4: return "风";
                case 5: return "土";
                case 6: return "阴";
                case 7: return "阳";
                case 8: return "物";
                case 9: return "钦慕";
                case 10: return "阴·决";
                default: return "属性" + prop;
            }
        }
        internal string AnalysisAction(string mai, string typeStr)
        {
            if (mai.StartsWith("JN"))
                return "发动";
            else if (mai.StartsWith("CZ"))
                return "操作";
            else if (mai.StartsWith("NJ"))
                return "执行NPC效果";
            else if (mai.StartsWith("SJ"))
                return "触发";
            else
            {
                if (!typeStr.Contains('!'))
                    return "使用";
                else
                {
                    int idx = typeStr.IndexOf('!');
                    string ct = typeStr.Substring(idx + 1);
                    if (ct.Equals("0"))
                        return "利用";
                    else if (ct.Equals("1"))
                        return "爆发";
                    else if (ct.Equals("2"))
                        return "触发";
                }
            }
            return "南泥湾";
        }
        private static string Substring(string @string, int start, int end)
        {
            if (end >= 0)
                return @string.Substring(start, end - start);
            else
                return @string.Substring(start);
        }
        private static void Aps(StringBuilder sb, string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args));
        }

        #endregion Utils

        private string MonsterInfo(Base.Card.Monster mon)
        {
            StringBuilder sb = new StringBuilder();
            if (mon != null)
            {
                Aps(sb, "*******************");
                Aps(sb, "  {0} {1} 战力：{2} 闪避：{3}", mon.Name, mon.Code, mon.STR, mon.AGL);
                if (!string.IsNullOrEmpty(mon.DebutText))
                    Aps(sb, "  出场：{0}", mon.DebutText);
                if (!string.IsNullOrEmpty(mon.PetText))
                    Aps(sb, "  宠物：{0}", mon.PetText);
                if (!string.IsNullOrEmpty(mon.WinText))
                    Aps(sb, "  胜利：{0}", mon.WinText);
                if (!string.IsNullOrEmpty(mon.LoseText))
                    Aps(sb, "  失败：{0}", mon.LoseText);
                Aps(sb, "*******************");
            }
            return sb.ToString();
        }
        private string NPCInfo(Base.Card.NPC npc)
        {
            StringBuilder sb = new StringBuilder();
            if (npc != null)
            {
                Aps(sb, "*******************");
                Aps(sb, " {0} {1} 战力：{2}", npc.Name, npc.Code, npc.STR);
                if (npc.Skills.Length > 0)
                    Aps(sb, "  NPC效果：{0}", string.Join(" ", npc.Skills.Select(p => SKTXCZ(p))));
                Aps(sb, "*******************");
            }
            return sb.ToString();
        }
        private string EveInfo(Base.Card.Evenement eve)
        {
            StringBuilder sb = new StringBuilder();
            if (eve != null)
            {
                Aps(sb, "*******************");
                Aps(sb, "  {0} {1}", eve.Name, eve.Code);
                if (eve.Description.Length > 0)
                    Aps(sb, "  {0}", eve.Description);
                Aps(sb, "*******************");
            }
            return sb.ToString();
        }
        private string TuxInfo(Base.Card.Tux tux)
        {
            StringBuilder sb = new StringBuilder();
            if (tux != null)
            {
                Aps(sb, "*******************");
                Aps(sb, "  {0} {1}", tux.Name, tux.Code);
                Aps(sb, "  ");
                if (!string.IsNullOrEmpty(tux.Description))
                    Aps(sb, "{0}", tux.Description);
                if (!string.IsNullOrEmpty(tux.Special))
                    Aps(sb, "{0}", tux.Special);
                Aps(sb, "*******************");
            }
            return sb.ToString();
        }
        private string HeroInfo(Base.Card.Hero hero)
        {
            StringBuilder sb = new StringBuilder();
            if (hero != null)
            {
                Aps(sb, "*******************");
                Aps(sb, "  {0} {1} HP：{2} 战力：{3} 命中：{4} ",
                    hero.Name, hero.Ofcode, hero.HP, hero.STR, hero.DEX);
                List<string> spo = hero.Spouses.Where(p =>
                {
                    int hro;
                    if (int.TryParse(p, out hro))
                        return tuple.HL.InstanceHero(hro) != null;
                    else
                        return true;
                }).ToList();
                if (spo.Count > 0)
                {
                    Aps(sb, "  倾慕者：{0}", string.Join(" ", spo.Select(p =>
                    {
                        int hro;
                        if (int.TryParse(p, out hro))
                            return Hero(hro);
                        else if (p == "!1")
                            return "场上任意一人";
                        else if (p == "!2")
                            return "水魔兽";
                        else if (p == "!3")
                            return "全体正式蜀山弟子";
                        else if (p == "!4")
                            return "全体琼华弟子";
                        else if (p == "!5")
                            return "指定场上一名女性";
                        else if (p == "!6")
                            return "指定场上一人男性";
                        else
                            return "";
                    })));
                }
                List<string> skls = hero.Skills.Where(p => tuple.SL.EncodeSkill(p) != null).ToList();
                if (skls.Count > 0)
                    Aps(sb, "  技能：{0}", string.Join(" ", skls.Select(p => SKTXCZ(p))));
                Aps(sb, "*******************");
            }
            return sb.ToString();
        }
        private string SkillInfo(Base.Skill skill)
        {
            StringBuilder sb = new StringBuilder();
            if (skill != null)
            {
                Aps(sb, "*******************");
                Aps(sb, "  {0} {1}", skill.Name, skill.Code);
                if (skill.Descripe.Length > 0)
                    Aps(sb, "  {0}", skill.Descripe);
                Aps(sb, "*******************");
            }
            return sb.ToString();
        }

        public string MonsterInfo(ushort code)
        {
            if (Base.Card.NMBLib.IsMonster(code))
            {
                Base.Card.Monster mon = tuple.ML.Decode(
                    Base.Card.NMBLib.OriginalMonster(code));
                return MonsterInfo(mon);
            }
            else if (Base.Card.NMBLib.IsNPC(code))
            {
                Base.Card.NPC npc = tuple.NL.Decode(
                    Base.Card.NMBLib.OriginalNPC(code));
                return NPCInfo(npc);
            }
            else
                return "";
        }
        public string MonsterInfo(string name)
        {
            ushort code = Base.Card.NMBLib.Encode(name, tuple.ML, tuple.NL);
            if (code != 0)
                return MonsterInfo(code);
            else
                return "";
        }
        public string EveInfo(ushort code)
        {
            return EveInfo(tuple.EL.DecodeEvenement(code));
        }
        public string EveInfo(string name)
        {
            foreach (Base.Card.Evenement eve in tuple.EL.ListAllEves(0))
            {
                if (eve.Code.Equals(name))
                    return EveInfo(eve);
            }
            return "";
        }
        public string TuxInfo(ushort code)
        {
            return TuxInfo(tuple.TL.DecodeTux(code));
        }
        public string TuxInfo(string name)
        {
            return TuxInfo(tuple.TL.EncodeTuxCode(name));
        }
        public string HeroInfo(ushort code)
        {
            return HeroInfo(tuple.HL.InstanceHero(code));
        }
        public string HeroInfo(string name)
        {
            foreach (var hero in tuple.HL.ListAllHeros(0))
            {
                if (hero.Ofcode == name)
                    return HeroInfo(hero);
            }
            return "";
        }
        public string SkillInfo(string name)
        {
            return SkillInfo(tuple.SL.EncodeSkill(name));
        }
        public string GetHelp()
        {
            StringBuilder sb = new StringBuilder();
            Aps(sb, "*************");
            Aps(sb, "/A：(Ain)查看自己的手牌（旁观者无效）");
            Aps(sb, "/P：(Piles)查看牌堆剩余情况及双方战绩");
            Aps(sb, "/F：(Field)查看当前战斗双方战力及怪物等");
            Aps(sb, "/G：(Gamer)查看所有玩家公开信息");
            Aps(sb, "/G1：(Gamer1)查看1#玩家公开信息，其余1-6同");
            Aps(sb, "/H：(Help)查看命令格式");
            Aps(sb, "");
            Aps(sb, "/IM8：(Info-Monster)查看代码为8的怪物（熔岩兽王）情报");
            Aps(sb, "/IMGH04：(Info-Monster)查看卡牌代号为GH04的怪物（熔岩兽王）情报");
            Aps(sb, "/IM1021：(Info-Monster)查看代码为1021的NPC（韩菱纱）情报");
            Aps(sb, "/IMNC402：(Info-Monster)查看卡牌代号为NC402的NPC（韩菱纱）情报");
            Aps(sb, "/IT10：(Info-Tux)查看代码为19的卡牌（天雷破）情报");
            Aps(sb, "/ITJP05：(Info-Tux)查看卡牌代号为JP05的卡牌（天雷破）情报");
            Aps(sb, "/IE1：(Info-Eve)查看代码为1的事件牌（仙灵岛的邂逅）情报");
            Aps(sb, "/IESJ101：(Info-Eve)查看卡牌代号为SJ101的事件牌（仙灵岛的邂逅）情报");
            Aps(sb, "/IH10401：(Info-Hero)查看代码为10401的人物（南宫煌）情报");
            Aps(sb, "/IHX3W01：(Info-Hero)查看卡牌代号为X3W01的人物（南宫煌）情报");
            Aps(sb, "/ISJN10102：(Info-Skill)查看代码为JN10102的技能（飞龙探云手）情报");
            Aps(sb, "*************");
            return sb.ToString();
        }
    }
}
