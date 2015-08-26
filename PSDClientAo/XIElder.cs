using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PSD.ClientAo
{
    public partial class XIVisi
    {
        #region Old Versions
        // return true if message is truncated and need no further processing
        private bool DealWithOldMessage(string readLine)
        {
            int version = (WI as VW.Eywi).Version;
            string[] args = readLine.Split(',');
            string header = args[0]; ushort rrounder = 0;
            if (header.StartsWith("R")) { rrounder = (ushort)(header[1] - '0'); header = "R#" + header.Substring(2); }
            switch (args[0])
            {
                case "H09G":
                    if (version <= 145)
                    {
                        string[] blocks = readLine.Split(',');
                        for (int idx = 1; idx < blocks.Length; )
                        {
                            ushort who = ushort.Parse(blocks[idx]);
                            int hero = int.Parse(blocks[idx + 1]);
                            int state = int.Parse(blocks[idx + 2]);
                            ushort hp = ushort.Parse(blocks[idx + 3]);
                            ushort hpa = ushort.Parse(blocks[idx + 4]);
                            ushort str = ushort.Parse(blocks[idx + 5]);
                            ushort stra = ushort.Parse(blocks[idx + 6]);
                            ushort dex = ushort.Parse(blocks[idx + 7]);
                            ushort dexa = ushort.Parse(blocks[idx + 8]);
                            int tuxCount = int.Parse(blocks[idx + 9]);
                            ushort wp = ushort.Parse(blocks[idx + 10]);
                            ushort am = ushort.Parse(blocks[idx + 11]);
                            ushort tr = ushort.Parse(blocks[idx + 12]);
                            ushort exq = ushort.Parse(blocks[idx + 13]);

                            int lugsz = int.Parse(blocks[idx + 14]);
                            int nextIdx = idx + 15;
                            List<string> lugs = Util.TakeRange(blocks, nextIdx,
                                nextIdx + lugsz).ToList();
                            nextIdx += lugsz;
                            ushort guard = ushort.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            ushort coss = ushort.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            ushort[] pets = Util.TakeRange(blocks, nextIdx,
                                nextIdx + 5).Select(p => ushort.Parse(p)).ToArray();
                            nextIdx += 5;
                            int excdsz = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            List<ushort> excards = Util.TakeRange(blocks, nextIdx,
                                nextIdx + excdsz).Select(p => ushort.Parse(p)).ToList();
                            nextIdx += excdsz;
                            int fakeqsz = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            List<string> fakeqpairs = Util.TakeRange(blocks, nextIdx,
                                nextIdx + fakeqsz * 2).ToList();
                            nextIdx += fakeqsz * 2;
                            int token = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            int peoplesz = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            List<string> peoples = Util.TakeRange(blocks, nextIdx,
                                nextIdx + peoplesz).ToList();
                            nextIdx += peoplesz;
                            int tarsz = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            List<ushort> tars = Util.TakeRange(blocks, nextIdx,
                                nextIdx + tarsz).Select(p => ushort.Parse(p)).ToList();
                            nextIdx += tarsz;
                            bool awake = blocks[nextIdx] == "1";
                            nextIdx += 1;
                            int foldsz = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            int escuesz = int.Parse(blocks[nextIdx]);
                            nextIdx += 1;
                            List<ushort> escues = Util.TakeRange(blocks, nextIdx,
                                nextIdx + escuesz).Select(p => ushort.Parse(p)).ToList();
                            nextIdx += escuesz;

                            idx = nextIdx;
                            if (A0P.ContainsKey(who))
                            {
                                AoPlayer ap = A0P[who];
                                ap.Rank = who;
                                ap.Team = (ap.Rank == 0 ? 0 : (ap.Rank % 2 == 1 ? 1 : 2));
                                ap.SelectHero = hero;
                                ap.HP = hp; ap.HPa = hpa;
                                ap.STR = str; ap.STRa = str;
                                ap.DEX = dex; ap.DEXa = dexa;
                                ap.TuxCount = tuxCount;
                                ap.Weapon = wp; ap.Armor = am; ap.Trove = tr; ap.ExEquip = exq;
                                ap.InitToLuggage(lugs);
                                ap.Guardian = guard; ap.Coss = coss;
                                for (int i = 0; i < pets.Length; ++i)
                                    ap.SetPet(i, pets[i]);
                                foreach (ushort ut in excards)
                                    ap.InsExCards(ut);
                                ap.Token = token;
                                for (int i = 0; i < fakeqpairs.Count; i += 2)
                                    ap.InsFakeq(ushort.Parse(fakeqpairs[i]), fakeqpairs[i + 1]);
                                ap.InsExSpCard(peoples);
                                ap.InsPlayerTar(tars);
                                ap.Awake = awake;
                                ap.FolderCount = foldsz;
                                foreach (ushort ut in escues)
                                    ap.InsEscue(ut);

                                ap.IsAlive = ((state & 1) != 0);
                                ap.IsLoved = ((state & 2) != 0);
                                ap.Immobilized = ((state & 4) != 0);
                                ap.PetDisabled = ((state & 8) != 0);

                                if (Uid >= 1000 && who == WATCHER_1ST_PERSPECT)
                                    A0M.insTux(Enumerable.Repeat((ushort)0, tuxCount).ToList());
                            }
                        }
                        return true;
                    }
                    break;
                case "E0CC": // prepare to use card
                    if (version <= 114)
                    {
                        ushort ust = ushort.Parse(args[1]);
                        ushort pst = ushort.Parse(args[2]);
                        List<ushort> ravs = Util.TakeRange(args, 4, args.Length)
                            .Select(p => ushort.Parse(p)).ToList();
                        if (pst == 0)
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(args[3]));
                        else
                            VI.Cout(Uid, "{0}将卡牌{1}当作卡牌{2}，为{3}使用.", zd.Player(ust),
                                zd.Tux(ravs), zd.Tux(args[3]), zd.Player(pst));
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
                                List<string> heros1 = Util.TakeRange(args, idx + 3, idx + 3 + count1).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<string> heros2 = Util.TakeRange(args, idx + 4 + count1,
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
                                List<ushort> tars1 = Util.TakeRange(args, idx + 3, idx + 3 + count1)
                                    .Select(p => ushort.Parse(p)).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<ushort> tars2 = Util.TakeRange(args, idx + 4 + count1,
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
                                List<string> heros1 = Util.TakeRange(args, idx + 3, idx + 3 + count1).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<string> heros2 = Util.TakeRange(args, idx + 4 + count1,
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
                                List<ushort> tars1 = Util.TakeRange(args, idx + 3, idx + 3 + count1)
                                    .Select(p => ushort.Parse(p)).ToList();
                                int count2 = int.Parse(args[idx + 3 + count1]);
                                List<ushort> tars2 = Util.TakeRange(args, idx + 4 + count1,
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
                        List<ushort> mons = Util.TakeRange(args, 2, args.Length)
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
            }
            return false;
        }
        #endregion Old Versions
    }
}