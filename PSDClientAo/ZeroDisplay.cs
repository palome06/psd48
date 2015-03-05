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
        internal string SkillName(string code)
        {
            var skill = tuple.SL.EncodeSkill(code);
            return skill.Name;
        }
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
                if (skill != null && (!hind || !skill.Branches[inTypeInt].Hind))
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
        internal string Tux(IEnumerable<string> cardNames)
        {
            if (!cardNames.Any())
                return "{}";
            return "{" + string.Join(",", cardNames.Select(p => Tux(p))) + "}";
        }
        internal string TuxDbSerial(ushort dbSerial)
        {
            return dbSerial + ":" + tuple.TL.Firsts.Find(p => p.DBSerial == dbSerial).Name;
        }
        internal string TuxDbSerial(IEnumerable<ushort> dbSerials)
        {
            if (!dbSerials.Any())
                return "{}";
            return "{" + string.Join(",", dbSerials.Select(p => TuxDbSerial(p))) + "}";
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
            else if (code.StartsWith("C"))
                return Tux(ushort.Parse(code.Substring("C".Length)));
            else if (code.StartsWith("M"))
                return Monster(ushort.Parse(code.Substring("M".Length)));
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
        internal object Guard(ushort code)
        {
            Base.Card.Exsp exsp = tuple.ESL.Encode("L" + code);
            return (exsp != null) ? (code + ":" + exsp.Name) : "0:喵";
        }
        internal string GuardWithCode(int code)
        {
            return code + ":" + ExspI(code);
        }
        internal string GuardWithCode(IEnumerable<int> codes)
        {
            if (!codes.Any())
                return "{}";
            return "{" + string.Join(",", codes.Select(p => GuardWithCode(p))) + "}";
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
        internal string HeroTokenAlias(params int[] heroes)
        {
            foreach (int hero in heroes)
            {
                if (hero != 0)
                {
                    Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
                    if (hro != null && hro.TokenAlias != null)
                        return hro.TokenAlias;
                }
            }
            return "标记";
        }
        internal string HeroPeopleAlias(params int[] heroes)
        {
            foreach (int hero in heroes)
            {
                if (hero != 0)
                {
                    Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
                    if (hro != null && hro.PeopleAlias != null)
                        return hro.PeopleAlias;
                }
            }
            return "人物牌";
        }
        internal string HeroPlayerTarAlias(params int[] heroes)
        {
            foreach (int hero in heroes)
            {
                if (hero != 0)
                {
                    Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
                    if (hro != null && hro.PlayerTarAlias != null)
                        return hro.PlayerTarAlias;
                }
            }
            return "目标角色";
        }

        internal string HeroExCardAlias(params int[] heroes)
        {
            foreach (int hero in heroes)
            {
                if (hero != 0)
                {
                    Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
                    if (hro != null && hro.ExCardsAlias != null)
                        return hro.ExCardsAlias;
                }
            }
            return "特殊装备";
        }
        internal string HeroAwakeAlias(params int[] heros)
        {
            foreach (int hero in heros)
            {
                if (hero != 0)
                {
                    if (hero != 0)
                    {
                        Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
                        if (hro != null && hro.AwakeAlias != null)
                            return hro.AwakeAlias;
                    }
                }
            }
            return "特殊状态";
        }
        internal string HeroFolderAlias(params int[] heros)
        {
            foreach (int hero in heros)
            {
                if (hero != 0)
                {
                    if (hero != 0)
                    {
                        Base.Card.Hero hro = tuple.HL.InstanceHero(hero);
                        if (hro != null && hro.FolderAlias != null)
                            return hro.FolderAlias;
                    }
                }
            }
            return "盖牌";
        }
        internal string GuardAlias(params int[] heros)
        {
            return HeroPeopleAlias(heros);
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
    }
}
