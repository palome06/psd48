﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base;
using Algo = PSD.Base.Utils.Algo;

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
            return skill == null ? "大法" : skill.Name;
        }
        internal string SKTXCZ(string ops) { return SKTXCZ(ops, false, "0"); }
        internal string SKTXCZ(string ops, bool hind, string inType)
        {
            int idx = ops.IndexOf(',');
            string title = Algo.Substring(ops, 0, idx);
            string result = "";
            if (title.StartsWith("JN"))
            {
                int jdx = title.IndexOf('('), kdx = title.IndexOf(')');
                string jnName = Algo.Substring(title, 0, jdx);
                string arg = jdx < 0 ? "" : "(" + Algo.Substring(title, jdx + 1, kdx) + ")";
                var skill = tuple.SL.EncodeSkill(jnName);
                int inTypeInt;
                if (!int.TryParse(inType, out inTypeInt))
                    inTypeInt = 0;
                if (skill != null && (!hind || inTypeInt >= skill.IsHind.Length || !skill.IsHind[inTypeInt]))
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
            {
                int jdx = inType.IndexOf('!');
                ushort consumeType = ushort.Parse(inType.Substring(0, jdx));
                ushort innerType = ushort.Parse(inType.Substring(jdx + 1));

                var monster = tuple.ML.Decode(ushort.Parse(title.Substring("PT".Length)));
                if (monster != null && (!hind || Algo.Equals(monster.EAHinds, consumeType, innerType, false)))
                {
                    result = title + ":" + monster.Name;
                }
                else return "";
            }
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
            return "{" + string.Join(",", cards.Select(p => Tux(p))) + "}";
        }
        internal string Tux(string cardName)
        {
            return cardName + ":" + tuple.TL.Firsts.Find(p => p.Code == cardName).Name;
        }
        internal string Tux(IEnumerable<string> cardNames)
        {
            return "{" + string.Join(",", cardNames.Select(p => Tux(p))) + "}";
        }
        internal string TuxDbSerial(ushort dbSerial)
        {
            return dbSerial + ":" + tuple.TL.Firsts.Find(p => p.DBSerial == dbSerial).Name;
        }
        internal string TuxDbSerial(IEnumerable<ushort> dbSerials)
        {
            return "{" + string.Join(",", dbSerials.Select(p => TuxDbSerial(p))) + "}";
        }
        internal string PurePlayer(ushort player)
        {
            return player == 0 ? "取消" : (player < 1000 ? tuple.HL.InstanceHero(
                xiv.A0P[player].SelectHero).Name : Monster((ushort)(player - 1000)));
        }

        internal string Player(ushort player)
        {
            return player == 0 ? "0:天上" : (player + ":" + tuple.HL
                .InstanceHero(xiv.A0P[player].SelectHero).Name);
        }
        internal string Player(IEnumerable<ushort> players)
        {
            return "{" + string.Join(",", players.Select(p => Player(p))) + "}";
        }
        internal string Warrior(ushort extUid)
        {
            if (extUid == 0)
                return "无人";
            else if (extUid < 1000)
                return Player(extUid);
            else if (extUid < 3000)
                return Monster((ushort)(extUid - 1000));
            else if (extUid < 4000)
                return ExspI((ushort)(extUid - 3000));
            else
                return "0:天使";
        }
        internal string Warriors(IEnumerable<string> strings) // mix results of entites
        {
            // !PT19,!I25,1,3,5
            System.Func<string, string> format = (jname) =>
            {
                if (jname.StartsWith("!PT"))
                {
                    ushort ut = ushort.Parse(jname.Substring("!PT".Length));
                    return ("PT" + ut) + ":" + tuple.ML.Decode(ut).Name;
                }
                else if (jname.StartsWith("!I"))
                {
                    ushort ut = ushort.Parse(jname.Substring("!I".Length));
                    return ("I" + ut) + ":" + tuple.ESL.Encode("I" + ut).Name;
                }
                else if (jname.StartsWith("!WQ"))
                {
                    string uname = jname.Substring("!".Length);
                    return uname + ":" + tuple.TL.EncodeTuxCode(uname).Name;
                }
                else
                    return Player(ushort.Parse(jname));
            };
            return "{" + string.Join(",", strings.Select(p => format(p))) + "}";
        }
        internal string Monster(ushort p)
        {
            return (p == 0) ? "0:没" : p + ":" + Base.Card.NMBLib.Decode(p, tuple.ML, tuple.NL).Name;
        }
        internal string Monster(IEnumerable<ushort> mons)
        {
            return "{" + string.Join(",", mons.Select(p => Monster(p))) + "}";
        }
        internal string Eve(ushort eve)
        {
            return (eve == 0) ? "0:静" : eve + ":" + tuple.EL.DecodeEvenement(eve).Name;
        }
        internal string Eve(IEnumerable<ushort> eves)
        {
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
            else if (code.StartsWith("E"))
                return Eve(ushort.Parse(code.Substring("E".Length)));
            else if (code.StartsWith("V"))
                return Prop(ushort.Parse(code.Substring("V".Length)));
            else
                return null;
        }
        internal string MixedCards(IEnumerable<string> codes)
        {
            return "{" + string.Join(",", codes.Select(p => MixedCards(p))) + "}";
        }
        internal string ExspI(int code)
        {
            Base.Card.Exsp exsp = tuple.ESL.Encode("I" + code);
            return (exsp != null) ? (code + ":" + exsp.Name) : "0:喵";
        }
        internal string ExspI(IEnumerable<int> codes)
        {
            return "{" + string.Join(",", codes.Select(p => ExspI(p))) + "}";
        }
        internal string Guard(ushort code)
        {
            Base.Card.Exsp exsp = tuple.ESL.Encode("L" + code);
            return (exsp != null) ? (code + ":" + exsp.Name) : "0:喵";
        }
        internal string Rune(ushort code)
        {
            return code == 0 ? "秘籍" : tuple.RL.Decode(code).Name;
        }
        internal string Rune(IEnumerable<ushort> codes)
        {
            return "{" + string.Join(",", codes.Select(p => Rune(p))) + "}";
        }
        internal string RuneWithCode(ushort code)
        {
            return code + ":" + Rune(code);
        }
        internal string RuneWithCode(IEnumerable<ushort> codes)
        {
            return "{" + string.Join(",", codes.Select(p => RuneWithCode(p))) + "}";
        }
        internal string Hero(int hero)
        {
            return hero == 0 ? "姚仙" : tuple.HL.InstanceHero(hero).Name;
        }
        internal string Hero(IEnumerable<int> heros)
        {
            return "{" + string.Join(",", heros.Select(p => Hero(p))) + "}";
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
        internal string PropName(int prop)
        {
            switch (prop)
            {
                case 0: return "物";
                case 1: return "水";
                case 2: return "火";
                case 3: return "雷";
                case 4: return "风";
                case 5: return "土";
                case 6: return "阴";
                case 7: return "阳";
                default: return "属性" + prop;
            }
        }
        internal string Prop(int prop)
        {
            return prop + ":" + PropName(prop);
        }
        internal string Prop(IEnumerable<int> groups)
        {
            return "{" + string.Join(",", groups.Select(p => Prop(p))) + "}";
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
