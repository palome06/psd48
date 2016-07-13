using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.ClientAo
{
    public partial class XIVisi
    {
        #region Old Versions
        // return true if message is truncated and need no further processing
        private bool DealWithOldMessage(ref string readLine)
        {
            int version = (WI as VW.Eywi).Version;
            string[] args = readLine.Split(',');
            string header = args[0]; ushort rrounder = 0;
            if (header.StartsWith("R"))
            {
                rrounder = (ushort)(header[1] - '0');
                header = "R#" + header.Substring(2);
            }
            switch (args[0])
            {
                case "H09G":
                    if (version <= 101)
                    { // single target, no target count
                        Algo.LongMessageParse(readLine.Split(','), InitPlayerPositionFromLongMessage, InitPlayerFullFromLongMessage,
                           new string[] { "I,hero", "I,state", "U,hp", "U,hpa", "U,str", "U,stra", "U,dex", "U,dexa",
                                "I,tuxCount", "U,wp", "U,am", "U,exq", "LC5,pet", "LU,excard", "LU,fakeq",
                                "I,token", "LA,excl", "U,tar", "LU,escue" });
                        return true;
                    }
                    if (version <= 114)
                    { // no trove and lug around excard, no awake and folder, fakeq not contains tux code
                        Algo.LongMessageParse(readLine.Split(','), InitPlayerPositionFromLongMessage, InitPlayerFullFromLongMessage,
                           new string[] { "I,hero", "I,state", "U,hp", "U,hpa", "U,str", "U,stra", "U,dex", "U,dexa",
                                "I,tuxCount", "U,wp", "U,am", "U,exq", "LC5,pet", "LU,excard", "LU,fakeq",
                                "I,token", "LA,excl", "LU,tar", "LU,escue" });
                        return true;
                    }
                    if (version <= 121)
                    { // no guard and coss
                        Algo.LongMessageParse(readLine.Split(','), InitPlayerPositionFromLongMessage, InitPlayerFullFromLongMessage,
                            new string[] { "I,hero", "I,state", "U,hp", "U,hpa", "U,str", "U,stra", "U,dex", "U,dexa",
                                "I,tuxCount", "U,wp", "U,am", "U,tr", "U,exq", "LA,lug", "LC5,pet", "LU,excard", "LD,fakeq",
                                "I,token", "LA,excl", "LU,tar", "U,awake", "I,foldsz", "LU,escue" });
                        return true;
                    }
                    if (version <= 145)
                    { // no rune
                        Algo.LongMessageParse(readLine.Split(','), InitPlayerPositionFromLongMessage, InitPlayerFullFromLongMessage,
                            new string[] { "I,hero", "I,state", "U,hp", "U,hpa", "U,str", "U,stra", "U,dex", "U,dexa",
                                "I,tuxCount", "U,wp", "U,am", "U,tr", "U,exq", "LA,lug", "U,guard", "U,coss", "LC5,pet",
                                "LU,excard", "LD,fakeq", "I,token", "LA,excl", "LU,tar", "U,awake", "I,foldsz", "LU,escue" });
                        return true;
                    }
                    else if (version <= 149)
                    { // no fakeq for compability
                        Algo.LongMessageParse(readLine.Split(','), InitPlayerPositionFromLongMessage, InitPlayerFullFromLongMessage,
                            new string[] { "I,hero", "I,state", "U,hp", "U,hpa", "U,str", "U,stra", "U,dex", "U,dexa",
                                "I,tuxCount", "U,wp", "U,am", "U,tr", "U,exq", "LA,lug", "U,guard", "U,coss", "LC5,pet",
                                "LU,excard", "LU,rune", "I,token", "LA,excl", "LU,tar", "U,awake", "I,foldsz", "LU,escue" });
                        return true;
                    }
                    break;
                case "E0CC": // prepare to use card
                    if (version <= 105)
                    {
                        ushort ust = ushort.Parse(args[1]);
                        ushort pst = ushort.Parse(args[2]);
                        string tuxCode = args[3];
                        List<ushort> ravs = Algo.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (pst == 0)
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(tuxCode));
                        else
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}，为{3}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(tuxCode), zd.Player(pst));
                        if (!ravs.Contains(0))
                        {
                            List<string> cedcards = ravs.Select(p => "C" + p).ToList();
                            A0O.FlyingGet(cedcards, ust, 0);
                        }
                        return true;
                    }
                    else if (version <= 114)
                    {
                        ushort ust = ushort.Parse(args[1]);
                        ushort pst = ushort.Parse(args[2]);
                        string tuxCode = args[4];
                        List<ushort> ravs = Algo.TakeRange(args, 5, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (pst == 0)
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(tuxCode));
                        else
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}，为{3}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(tuxCode), zd.Player(pst));
                        if (!ravs.Contains(0))
                        {
                            List<string> cedcards = ravs.Select(p => "C" + p).ToList();
                            A0O.FlyingGet(cedcards, ust, 0);
                        }
                        return true;
                    }
                    break;
                case "E0HS":
                    if (version <= 114)
                    {
                        string msg = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            if (args[i] == "0")
                                msg += "不触发战斗.";
                            if (args[i] == "1")
                            {
                                ushort s = ushort.Parse(args[i + 1]);
                                string name = (s == 0 ? "无人" :
                                    (s < 1000 ? zd.Player(s) : zd.Monster((ushort)(s - 1000))));
                                msg += string.Format("{0}进行支援.", name);
                                if (A0F.Supporter != 0 && A0F.Supporter < 1000)
                                    A0P[A0F.Supporter].SetAsClear();
                                A0F.Supporter = s;
                                if (s != 0 && s < 1000)
                                    A0P[s].SetAsSpSucc();
                            }
                            else if (args[i] == "2")
                            {
                                ushort s = ushort.Parse(args[i + 1]);
                                string name = (s == 0 ? "无人" :
                                    (s < 1000 ? zd.Player(s) : zd.Monster((ushort)(s - 1000))));
                                msg += string.Format("{0}进行妨碍.", name);
                                if (A0F.Hinder != 0 && A0F.Hinder < 1000)
                                    A0P[A0F.Hinder].SetAsClear();
                                A0F.Hinder = s;
                                if (s != 0 && s < 1000)
                                    A0P[s].SetAsSpSucc();
                            }
                            if (msg.Length > 0)
                                VI.Cout(Uid, msg);
                        }
                        return true;
                    }
                    break;
                case "E0IJ":
                    if (version <= 114)
                    {
                        for (int idx = 1; idx < args.Length; )
                        {
                            ushort who = ushort.Parse(args[idx]);
                            ushort type = ushort.Parse(args[idx + 1]);
                            if (type == 0)
                            {
                                Base.Card.Hero hro = Tuple.HL.InstanceHero(A0P[who].SelectHero);
                                if (hro != null && !string.IsNullOrEmpty(hro.AwakeAlias))
                                {
                                    VI.Cout(Uid, "{0}已发动{1}.", zd.Player(who),
                                    zd.HeroAwakeAlias(A0P[who].SelectHero));
                                    A0P[who].Awake = false;
                                }
                                else
                                {
                                    ushort delta = ushort.Parse(args[idx + 2]);
                                    ushort cur = ushort.Parse(args[idx + 3]);
                                    VI.Cout(Uid, "{0}的{1}+{2}，现在为{3}.", zd.Player(who),
                                        zd.HeroTokenAlias(A0P[who].SelectHero), delta, cur);
                                    A0P[who].Token = cur;
                                }
                                idx += 4;
                            }
                            else if (type == 1)
                            {
                                int count1 = int.Parse(args[idx + 2]);
                                List<string> heros1 = Algo.TakeRange(args, idx + 3, idx + 3 + count1).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<string> heros2 = Algo.TakeRange(args, idx + 4 + count1,
                                    idx + 4 + count1 + count2).ToList();
                                VI.Cout(Uid, "{0}的{1}增加{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroPeopleAlias(A0P[who].SelectHero), zd.MixedCards(heros1), zd.MixedCards(heros2));
                                A0P[who].InsExSpCard(heros1);
                                //A0O.FlyingGet(heros1, 0, who);
                                idx += (4 + count1 + count2);
                            }
                            else if (type == 2)
                            {
                                int count1 = int.Parse(args[idx + 2]);
                                List<ushort> tars1 = Algo.TakeRange(args, idx + 3, idx + 3 + count1)
                                    .Select(p => ushort.Parse(p)).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<ushort> tars2 = Algo.TakeRange(args, idx + 4 + count1,
                                    idx + 4 + count1 + count2).Select(p => ushort.Parse(p)).ToList();
                                if (count1 == count2)
                                    VI.Cout(Uid, "{0}的{1}目标指定为{2}.", zd.Player(who),
                                        zd.HeroPlayerTarAlias(A0P[who].SelectHero), zd.Player(tars1));
                                else
                                    VI.Cout(Uid, "{0}的{1}目标增加{2}，现在为{3}.", zd.Player(who),
                                        zd.HeroPlayerTarAlias(A0P[who].SelectHero), zd.Player(tars1), zd.Player(tars2));
                                A0P[who].InsPlayerTar(tars1);
                                idx += (4 + count1 + count2);
                            }
                            else
                                break;
                        }
                        return true;
                    }
                    break;
                case "E0OJ":
                    if (version <= 114)
                    {
                        for (int idx = 1; idx < args.Length; )
                        {
                            ushort who = ushort.Parse(args[idx]);
                            ushort type = ushort.Parse(args[idx + 1]);
                            if (type == 0)
                            {
                                Base.Card.Hero hro = Tuple.HL.InstanceHero(A0P[who].SelectHero);
                                if (hro != null && !string.IsNullOrEmpty(hro.AwakeAlias))
                                {
                                    VI.Cout(Uid, "{0}已取消{1}.", zd.Player(who),
                                    zd.HeroAwakeAlias(A0P[who].SelectHero));
                                    A0P[who].Awake = false;
                                }
                                else
                                {
                                    ushort delta = ushort.Parse(args[idx + 2]);
                                    ushort cur = ushort.Parse(args[idx + 3]);
                                    VI.Cout(Uid, "{0}的{1}-{2}，现在为{3}.", zd.Player(who),
                                        zd.HeroTokenAlias(A0P[who].SelectHero), delta, cur);
                                    A0P[who].Token = cur;
                                }
                                idx += 4;
                            }
                            else if (type == 1)
                            {
                                int count1 = int.Parse(args[idx + 2]);
                                List<string> heros1 = Algo.TakeRange(args, idx + 3, idx + 3 + count1).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<string> heros2 = Algo.TakeRange(args, idx + 4 + count1,
                                    idx + 4 + count1 + count2).ToList();
                                VI.Cout(Uid, "{0}的{1}减少{2}，现在为{3}.", zd.Player(who),
                                    zd.HeroPeopleAlias(A0P[who].SelectHero), zd.MixedCards(heros1), zd.MixedCards(heros2));
                                A0P[who].DelExSpCard(heros1);
                                //A0O.FlyingGet(heros1, who, 0);
                                idx += (4 + count1 + count2);
                            }
                            else if (type == 2)
                            {
                                int count1 = int.Parse(args[idx + 2]);
                                List<ushort> tars1 = Algo.TakeRange(args, idx + 3, idx + 3 + count1)
                                    .Select(p => ushort.Parse(p)).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<ushort> tars2 = Algo.TakeRange(args, idx + 4 + count1,
                                    idx + 4 + count1 + count2).Select(p => ushort.Parse(p)).ToList();
                                if (count2 == 0)
                                    VI.Cout(Uid, "{0}失去{1}目标.", zd.Player(who),
                                        zd.HeroPlayerTarAlias(A0P[who].SelectHero));
                                else
                                    VI.Cout(Uid, "{0}的{1}目标减少{2}，现在为{3}.", zd.Player(who),
                                        zd.HeroPlayerTarAlias(A0P[who].SelectHero), zd.Player(tars1), zd.Player(tars2));
                                A0P[who].DelPlayerTar(tars1);
                                idx += (4 + count1 + count2);
                            }
                            else
                                break;
                        }
                        return true;
                    }
                    break;
                case "E0QC":
                    if (version <= 114) // JN40101
                    {
                        ushort cardType = ushort.Parse(args[1]);
                        List<ushort> mons = Algo.TakeRange(args, 2, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (mons.Count > 0)
                        {
                            if (cardType == 0)
                            {
                                VI.Cout(Uid, "{0}被弃置.", zd.Tux(mons));
                                List<string> cedcards = mons.Select(p => "C" + p).ToList();
                                A0O.FlyingGet(cedcards, 0, 0);
                            }
                            else if (cardType == 1)
                            {
                                VI.Cout(Uid, "{0}被弃置.", zd.Monster(mons));
                                List<string> cedcards = mons.Select(p => "M" + p).ToList();
                                A0O.FlyingGet(cedcards, 0, 0);
                            }
                            else if (cardType == 2)
                            {
                                VI.Cout(Uid, "{0}被弃置.", zd.Eve(mons));
                                List<string> cedcards = mons.Select(p => "E" + p).ToList();
                                A0O.FlyingGet(cedcards, 0, 0);
                            }
                        }
                        return true;
                    }
                    break;
                case "R#Z3":
                    if (version <= 137)
                    {
                        A0P.Where(p => p.Key != rrounder).ToList().ForEach((p) => p.Value.SetAsClear());
                        A0F.Supporter = 0; A0F.Hinder = 0;
                    }
                    break;
                case "E0LH":
                    if (version <= 152)
                    {
                        string[] e0lh = readLine.Split(',');
                        ushort ut = ushort.Parse(e0lh[2]);
                        ushort to = ushort.Parse(e0lh[3]);
                        readLine = "E0LA,H," + ut + "," + A0P[ut].HPa + "," + to;
                    }
                    break;
                case "E0ZS":
                case "E0ZL":
                    if (version <= 155)
                    {
                        string[] e0z = readLine.Split(',');
                        for (int i = 1; i < e0z.Length; i += 2)
                        {
                            ushort ut = ushort.Parse(e0z[i + 1]);
                            Base.Card.Tux tux = Tuple.TL.DecodeTux(ut);
                            e0z[i + 1] = ut + "," + tux.Code;
                        }
                        readLine = string.Join(",", e0z);
                    }
                    break;

            }
            return false;
        }
        #endregion Old Versions
    }
}