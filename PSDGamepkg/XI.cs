using System;
using System.Collections.Generic;
using System.Linq;
using PSD.Base;
using PSD.Base.Rules;
using PSD.Base.Utils;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg
{
    public class PilesConstruct
    {
        private LibGroup libTuple;
        public int Groups { private set; get; }
        public bool IsTrain { private set; get; }
        public int Level { get { return (Groups << 1) | (IsTrain ? 1 : 0); } }
        public string[] Trainer { private set; get; }

        public PilesConstruct(LibGroup libTuple, int levelCode, string[] trainer)
        {
            //this.xi = xi; 
            this.libTuple = libTuple;
            IsTrain = (levelCode % 2 != 0);
            Groups = (levelCode >> 1);
            this.Trainer = trainer;
        }

        //public Base.Card.Hero[,] AllocateHerosRM(int szCand, int szPlayer)
        //{
        //    return libTuple.HL.RMPickList(szCand, szPlayer);
        //}
        public Base.Card.Hero[] AllocateHerosRM(int sz)
        {
            List<Base.Card.Hero> list = libTuple.HL.PurgeHeroesWithGivenTrainer(Level, Trainer);
            //var test = libTuple.HL.ListHeroesInTest(Level).Except(list).ToList();
            //var sels = ListAllSeleableHeros().Except(list).ToArray();
            //double probablity = (IsTrain ? 0.48 : Math.Max(0.04, test.Count * 1.0 / (test.Count + sels.Length)));

            //list.AddRange(Base.Card.Card.PickSomeInGivenProbability(
            //    libTuple.HL.ListHeroesInTest(Level), probablity).ToList());
            //if (list.Count >= sz)
            //    return Base.Card.Card.PickSomeInRandomOrder(list, sz).ToArray();
            //else
            //{
            //    list.AddRange(Base.Card.Card.PickSomeInRandomOrder(sels, sz - list.Count));
            //    list.Shuffle();
            //    return list.ToArray();
            //}
            if (list.Count < sz)
            {
                list.AddRange(Base.Card.Card.PickSomeInRandomOrder(
                    ListAllSeleableAndTestedHeros().Except(list).ToList(), sz - list.Count));
                list.Shuffle();
                return list.ToArray();
            }
            else
                return Base.Card.Card.PickSomeInRandomOrder(list, sz).ToArray();
        }
        public List<Base.Card.Hero> ListAllSeleableAndTestedHeros()
        {
            return libTuple.HL.ListAllSeleable(Level).ToList().Union(
                libTuple.HL.ListHeroesInTest(Level)).ToList();
        }
        // List out all heros that can be in the selection pool directly
        public Base.Card.Hero[] ListAllSeleableHeros()
        {
            return libTuple.HL.ListAllSeleable(Level).ToArray();
        }
        // List out all heros that can be joined directly, including conflict one like XJ505
        public Base.Card.Hero[] ListAllJoinableHeroes()
        {
            return libTuple.HL.ListAllJoinableHeroes(Level).ToArray();
        }
        public List<Base.Card.Hero> ListAllHeros()
        {
            List<Base.Card.Hero> list = libTuple.HL.ListAllHeros(Level).ToList();
            list.AddRange(libTuple.HL.ListHeroesInTest(Level));
            return list;
        }
    }

    public partial class XI
    {
        #region Memeber Declaration & Constructor
        public Base.VW.IVI VI { private set; get; }
        public Base.VW.IWISV WI { private set; get; }

        //private IDictionary<ushort, Player> garden;

        // sk01 for mapping from skill name to skill object
        private IDictionary<string, Skill> sk01;
        // sk02 for mapping from occur to "holder,skill/..."
        private IDictionary<string, List<SkTriple>> sk02;
        // sk03 for getting the parasitism link list
        private IDictionary<string, List<string>> sk03;
        public IDictionary<string, List<string>> Sk03 { get { return sk03; } }
        // tx01 for mapping from tux name to tux object
        private IDictionary<string, Base.Card.Tux> tx01;
        // mt01 for mapping from tux name to tux object
        private IDictionary<string, Base.Card.Monster> mt01;
        // cz01 for mapping from operation name to object
        private IDictionary<string, Operation> cz01;
        // nj01 for mapping from npc effect name to object
        private IDictionary<string, Base.NCAction> nj01;
        // ev01 for mapping from eve code pers effect name to object
        private IDictionary<string, Base.Card.Evenement> ev01;
        // sf01 for mapping from rune name to object
        private IDictionary<string, Base.Rune> sf01;

        public LibGroup LibTuple { private set; get; }

        //private Queue<ushort> Board.TuxPiles, Board.EvePiles, Board.MonPiles;

        //private Stack<string> clMsgStack;
        //private int clMsgCount;

        public Board Board { private set; get; }
        public PilesConstruct PCS { set; get; }

        public Log Log { private set; get; }

        // Hero selection issue
        private int SelCode { set; get; }
        private Casting Casting { set; get; }

        public XI()
        {
            Log = new Log();
            LibTuple = new LibGroup();
            Board = new Board();
            Casting = null; SelCode = 0;
            randomSeed = new Random();
        }

        #endregion Memeber Declaration & Constructor

        #region Piles Operation

        private static void Riffle(Base.Utils.Rueue<ushort> piles, List<ushort> dises)
        {
            while (piles.Count > 0)
                dises.Add(piles.Dequeue());
            dises.Shuffle();
            foreach (ushort id in dises)
                piles.Enqueue(id);
            dises.Clear();
        }
        private void ConstructPiles(int levelCode)
        {
            List<ushort> tuxLst = LibTuple.TL.ListAllTuxCodes(levelCode);
            Algo.Shuffle(tuxLst);
            Board.TuxPiles = new Rueue<ushort>(tuxLst);
            List<ushort> eveLst = LibTuple.EL.ListAllSeleable(levelCode);
            Algo.Shuffle(eveLst);
            Board.EvePiles = new Rueue<ushort>(eveLst);
            List<ushort> monLst = LibTuple.ML.ListAllSeleable(levelCode)
                .Select(p => Base.Card.NMBLib.CodeOfMonster(p)).ToList();
            List<ushort> npcLst = LibTuple.NL.ListAllSeleable(levelCode)
                .Select(p => Base.Card.NMBLib.CodeOfNPC(p)).ToList();
            monLst.Shuffle(); npcLst.Shuffle();
            List<ushort> nmbLst = new List<ushort>();
            for (int i = 0; i < 10; ++i)
            {
                nmbLst.Add(monLst[2 * i]);
                nmbLst.Add(monLst[2 * i + 1]);
                nmbLst.Add(npcLst[i]);
            }
            nmbLst.Shuffle();
            Board.MonPiles = new Base.Utils.Rueue<ushort>(nmbLst);

            Board.TuxDises = new List<ushort>();
            Board.EveDises = new List<ushort>();
            Board.MonDises = new List<ushort>();

            List<int> heros = PCS.ListAllHeros().Select(p => p.Avatar).ToList();
            foreach (Player py in Board.Garden.Values)
                heros.Remove(py.SelectHero);
            heros.Shuffle();
            Board.HeroPiles = new Base.Utils.Rueue<int>(heros);
            Board.HeroDises = new List<int>();

            List<ushort> restNPC = Algo.TakeRange(npcLst.ToArray(), 11, npcLst.Count).ToList();
            restNPC.Shuffle();
            Board.RestNPCPiles = new Base.Utils.Rueue<ushort>(restNPC);
            Board.RestNPCDises = new List<ushort>();

            List<ushort> restMon = Algo.TakeRange(monLst.ToArray(), 21, monLst.Count).ToList();
            restMon.Shuffle();
            Board.RestMonPiles = new Base.Utils.Rueue<ushort>(restMon);
            Board.RestMonDises = new List<ushort>();
        }
        public ushort[] DequeueOfPile(Base.Utils.Rueue<ushort> queue, int count)
        {
            if (queue.Count >= count)
            {
                ushort[] ret = new ushort[count];
                for (int i = 0; i < count; ++i)
                    ret[i] = queue.Dequeue();
                return ret;
            }
            else
            {
                if (queue.Equals(Board.TuxPiles) || queue.Equals(Board.EvePiles)
                     || queue.Equals(Board.RestNPCPiles) || queue.Equals(Board.RestMonPiles))
                {
                    ushort[] ret = new ushort[count];
                    int qCount = queue.Count;
                    for (int i = 0; i < qCount; ++i)
                        ret[i] = queue.Dequeue();
                    if (queue.Equals(Board.TuxPiles))
                        Riffle(Board.TuxPiles, Board.TuxDises);
                    else if (queue.Equals(Board.EvePiles))
                        Riffle(Board.EvePiles, Board.EveDises);
                    else if (queue.Equals(Board.RestNPCPiles))
                        Riffle(Board.RestNPCPiles, Board.RestNPCDises);
                    else if (queue.Equals(Board.RestMonPiles))
                        Riffle(Board.RestMonPiles, Board.RestMonDises);
                    for (int i = qCount; i < count; ++i)
                        ret[i] = queue.Dequeue();
                    return ret;
                }
                else if (queue.Equals(Board.MonPiles))
                {
                    ushort[] ret = new ushort[queue.Count];
                    int qCount = queue.Count;
                    for (int i = 0; i < qCount; ++i)
                        ret[i] = queue.Dequeue();
                    return ret;
                }
                else
                    return null;
            }
        }
        public ushort DequeueOfPile(Base.Utils.Rueue<ushort> queue)
        {
            if (queue.Count > 0)
                return queue.Dequeue();
            else
            {
                if (queue.Equals(Board.TuxPiles))
                {
                    Riffle(Board.TuxPiles, Board.TuxDises);
                    return queue.Dequeue();
                }
                else if (queue.Equals(Board.EvePiles))
                {
                    Riffle(Board.EvePiles, Board.EveDises);
                    return queue.Dequeue();
                }
                else if (queue.Equals(Board.RestNPCPiles))
                {
                    Riffle(Board.RestNPCPiles, Board.RestNPCDises);
                    return queue.Dequeue();
                }
                else if (queue.Equals(Board.RestMonPiles))
                {
                    Riffle(Board.RestMonPiles, Board.RestMonDises);
                    return queue.Dequeue();
                }
                else if (queue.Equals(Board.MonPiles))
                    return 0;
                else
                    return 0;
            }
        }
        private IEnumerable<ushort> WatchFromPile(Base.Utils.Rueue<ushort> queue, int count)
        {
            return queue.Watch(count);
        }

        #endregion Piles Operation

        #region Skill Inv-Register
        private void PutSKTIntoDict(SkTriple skt, string occur, IDictionary<string, List<SkTriple>> dict,
            IDictionary<string, List<string>> links, List<SkTriple> parasitisms)
        {
            if (occur.StartsWith("%"))
            {
                int aliasSerial = int.Parse(occur.Substring("%".Length));
                string theAlias = SKBranch.GetAas(aliasSerial);
                foreach (string theA in theAlias.Split('&'))
                    Algo.AddToMultiMap(links, theA, skt.ToHeader());
                skt.Occur = theAlias;
                parasitisms.Add(skt);
            }
            else if (occur.StartsWith("&"))
            {
                occur = occur.Substring("&".Length);
                string[] theBlias = occur.Split('&');
                string[] theAlias = new string[theBlias.Length / 2];
                for (int i = 0; i < theAlias.Length; ++i)
                    theAlias[i] = theBlias[2 * i] + "," + theBlias[2 * i + 1];
                foreach (string theA in theAlias)
                    Algo.AddToMultiMap(links, theA, skt.ToHeader());
                skt.Occur = string.Join("&", theAlias);
                parasitisms.Add(skt);
            }
            else if (occur.Contains('#') || occur.Contains('$') || occur.Contains('*'))
            {
                Board.Garden.Keys.Select(p => p.ToString()).ToList().ForEach(p =>
                    Algo.AddToMultiMap(dict, occur.Replace("#", p).Replace("$", p).Replace("*", p), skt));
            }
            else
                Algo.AddToMultiMap(dict, occur, skt);
        }
        private void PutParamsIntoDict(IDictionary<string, List<SkTriple>> dict,
            List<SkTriple> parasitisms, bool resort)
        {
            IDictionary<string, List<Diva>> invTable = new Dictionary<string, List<Diva>>();
            foreach (var pair in dict)
            {
                pair.Value.ForEach(p => Algo.AddToMultiMap(invTable, p.ToHeader(),
                    new Diva().Set("oc", pair.Key).Set("prior", p.Priorty).Set("rawoc", p.Occur)));
            }
            foreach (SkTriple mskt in parasitisms)
            {
                Dictionary<string, List<Diva>> linkChain = new Dictionary<string, List<Diva>>();
                string[] paras = Algo.Splits(mskt.Occur, "&");
                foreach (string para in paras.Where(p => invTable.ContainsKey(p)))
                {
                    foreach (Diva diva in invTable[para])
                    {
                        Algo.AddToMultiMap(linkChain, diva.GetString("oc") + "," + diva.GetInt("prior"),
                            new Diva().Set("rawoc", diva.GetString("rawoc")).Set("header", para));
                    }
                }
                foreach (var pair in linkChain)
                {
                    int lidx = pair.Key.IndexOf(',');
                    string loc = pair.Key.Substring(0, lidx);
                    int lprior = int.Parse(pair.Key.Substring(lidx + 1));
                    SkTriple skt = new SkTriple()
                    {
                        Name = mskt.Name,
                        Priorty = lprior,
                        Owner = mskt.Owner, // Owner would always be 0
                        InType = mskt.InType,
                        Type = mskt.Type,
                        Consume = mskt.Consume,
                        Lock = mskt.Lock,
                        IsOnce = mskt.IsOnce,
                        Occur = loc, // needs #/*/$
                        LinkFrom = string.Join("&", pair.Value.Select(p =>
                            p.GetString("header") + "," + p.GetString("rawoc"))),
                        IsTermini = mskt.IsTermini
                    };
                    if (pair.Value.GroupBy(p => p.GetString("rawoc")).Count() == 1)
                        skt.Occur = pair.Value[0].GetString("rawoc");
                    Algo.AddToMultiMap(dict, loc, skt);
                    if (resort)
                        dict[loc].Sort(SkTriple.Cmp);
                }
            }
        }
        // $dict is the main sk02 dictionary mapping from Occur to SKTriple
        // $links is the dictionary mapping from one SKTriple to all its parasitisms
        private void MappingSksp(out IDictionary<string, List<SkTriple>> dict,
            out IDictionary<string, List<string>> links, int levelCode)
        {
            dict = new Dictionary<string, List<SkTriple>>();
            links = new Dictionary<string, List<string>>();
            //IDictionary<string, List<SkTriple>> par = new Dictionary<string, List<SkTriple>>();
            List<SkTriple> parasitism = new List<SkTriple>();
            foreach (Base.Card.Tux tux in LibTuple.TL.ListAllTuxs(levelCode))
            {
                //string[] blocks = tux.Occur.Split(';');
                //string[] occurs = blocks[0].Split(',');
                for (int i = 0; i < tux.Occurs.Length; ++i)
                {
                    string oc = tux.Occurs[i];
                    if (oc != "")
                    {
                        SkTriple skt = new SkTriple()
                        {
                            Name = tux.Code,
                            Priorty = tux.Priorities[i],
                            Owner = 0,
                            InType = i,
                            Type = SKTType.TX,
                            Consume = 0,
                            Lock = false,
                            IsOnce = (tux.Type == Base.Card.Tux.TuxType.ZP),
                            Occur = oc,
                            IsTermini = tux.IsTermini[i]
                        };
                        PutSKTIntoDict(skt, oc, dict, links, parasitism);
                    }
                }
                if (tux.IsTuxEqiup())
                {
                    Base.Card.TuxEqiup tue = (Base.Card.TuxEqiup)tux;
                    for (int i = 0; i < tue.CsOccur.Length; ++i)
                    {
                        if (tue.CsOccur[i] != null)
                        {
                            for (int j = 0; j < tue.CsOccur[i].Length; ++j)
                            {
                                string oc = tue.CsOccur[i][j];
                                if (oc != "")
                                {
                                    SkTriple skt = new SkTriple()
                                    {
                                        Name = tux.Code,
                                        Priorty = tue.CsPriorites[i][j],
                                        Owner = 0,
                                        InType = j,
                                        Type = SKTType.EQ,
                                        Consume = i,
                                        Lock = tue.CsLock[i][j],
                                        IsOnce = tue.CsOnce[i][j],
                                        Occur = oc,
                                        IsTermini = tue.CsIsTermini[i][j],
                                    };
                                    PutSKTIntoDict(skt, oc, dict, links, parasitism);
                                }
                            }
                        }
                    }
                }
            }
            foreach (Base.Operation cz in cz01.Values)
            {
                SkTriple skt = new SkTriple()
                {
                    Name = cz.Code,
                    Priorty = 0,
                    Owner = 0,
                    InType = 0,
                    Type = SKTType.CZ,
                    Consume = 0,
                    Lock = false,
                    IsOnce = cz.IsOnce,
                    Occur = cz.Occur,
                    IsTermini = false
                };
                PutSKTIntoDict(skt, cz.Occur, dict, links, parasitism);
            }
            foreach (Base.Rune sf in sf01.Values)
            {
                SkTriple skt = new SkTriple()
                {
                    Name = sf.Code,
                    Priorty = sf.Priority,
                    Owner = 0,
                    InType = 0,
                    Type = SKTType.SF,
                    Consume = sf.IsConsume ? 1 : 0,
                    Lock = sf.IsLock,
                    IsOnce = sf.IsOnce,
                    Occur = sf.Occur,
                    IsTermini = sf.IsTermin
                };
                PutSKTIntoDict(skt, sf.Occur, dict, links, parasitism);
            }
            foreach (Base.Card.Monster mt in LibTuple.ML.ListAllMonster(levelCode))
            {
                for (int i = 0; i < mt.EAOccurs.Length; ++i)
                    if (mt.EAOccurs[i] != null)
                    {
                        for (int j = 0; j < mt.EAOccurs[i].Length; ++j)
                        {
                            string oc = mt.EAOccurs[i][j];
                            if (oc != "")
                            {
                                SkTriple skt = new SkTriple()
                                {
                                    Name = mt.Code,
                                    Priorty = mt.EAProperties[i][j],
                                    Owner = 0,
                                    InType = j,
                                    Type = SKTType.PT,
                                    Consume = i,
                                    Lock = mt.EALocks[i][j],
                                    IsOnce = ((i == 1) || mt.EAOnces[i][j]),
                                    Occur = oc,
                                    IsTermini = mt.EAIsTermini[i][j]
                                };
                                PutSKTIntoDict(skt, oc, dict, links, parasitism);
                            }
                        }
                    }
            }
            foreach (NCAction nca in LibTuple.NJL.Firsts)
            {
                for (int i = 0; i < nca.Branches.Length; ++i)
                {
                    SKBranch skb = nca.Branches[i];
                    string oc = skb.Occur;
                    SkTriple skt = new SkTriple()
                    {
                        Name = nca.Code,
                        Priorty = skb.Priority,
                        Owner = 0,
                        InType = i,
                        Type = SKTType.YJ,
                        Consume = 0, // Consume is handled within Action
                        Lock = skb.Lock,
                        IsOnce = skb.Once,
                        Occur = oc,
                        IsTermini = skb.Demiurgic
                    };
                    PutSKTIntoDict(skt, oc, dict, links, parasitism);
                }
            }
            foreach (Base.Card.Evenement eve in LibTuple.EL.ListAllEves(levelCode))
            {
                for (int i = 0; i < eve.Occurs.Length; ++i)
                {
                    string oc = eve.Occurs[i];
                    SkTriple skt = new SkTriple()
                    {
                        Name = eve.Code,
                        Priorty = eve.Priorties[i],
                        Owner = 0,
                        InType = i,
                        Type = SKTType.EV,
                        Consume = 0,
                        Lock = eve.Lock[i],
                        IsOnce = eve.IsOnce[i],
                        Occur = oc,
                        IsTermini = eve.IsTermini[i]
                    };
                    PutSKTIntoDict(skt, oc, dict, links, parasitism);
                }
            }
            if (parasitism.Count > 0)
                PutParamsIntoDict(dict, parasitism, false);

            string[] g0 = { "IT", "OT", "HQ", "QZ", "DH", "IH", "OH", "ZH", "LV", "LU", "ZW", "IY",
                "OY", "DS", "CC", "CD", "CE", "XZ", "ZB", "ZC", "ZI", "ZS", "ZL", "IA", "OA", "IX",
                "OX", "AX", "IB", "OB", "IW", "OW", "WB", "9P", "IP", "OP", "CZ", "HC", "HD", "HH",
                "HI", "HL", "IC", "OC", "HT", "HG", "QR", "HZ", "TT", "T7", "JM", "WN", "IJ", "OJ",
                "IE", "OE", "IS", "OS", "LA", "IV", "OV", "PB", "YM", "HR", "FI", "ON", "SN", "MA",
                "ZJ", "IF", "OF", "PQ" };
            string[] g1 = { "CH", "DI", "IU", "OU", "TH", "CW", "ZK", "IZ", "OZ", "WP", "SG", "HK",
                "WJ", "XR", "EV", "CK", "7F", "YP", "NI", "GE", "LY", "UE" };
            string[] g2 = { "IN", "RN", "CN", "QC", "FU", "QU", "CL", "WK", "AK", "IL", "OL", "SW",
                "AS", "SY", "UL", "ZZ" };
            foreach (string g0event in g0)
                RegisterBasicSKTs(dict, "G0" + g0event, 100);
            foreach (string g1event in g1)
                RegisterBasicSKTs(dict, "G1" + g1event, 100);
            foreach (string g2event in g2)
                RegisterBasicSKTs(dict, "G2" + g2event, 100);

            RegisterBasicSKTs(dict, "G1TH", 200);
            RegisterBasicSKTs(dict, "G0ZW", 0);
            RegisterBasicSKTs(dict, "G0ZW", 200);
            RegisterBasicSKTs(dict, "G0ZW", 300);
            RegisterBasicSKTs(dict, "G0ZW", 400);
            RegisterBasicSKTs(dict, "G0OY", 200);
            RegisterBasicSKTs(dict, "G0OY", 300);
            RegisterBasicSKTs(dict, "G0CC", 200);
            RegisterBasicSKTs(dict, "G0CC", 300);
            RegisterBasicSKTs(dict, "G0CC", 400);
            RegisterBasicSKTs(dict, "G0HZ", 200);
            RegisterBasicSKTs(dict, "G0HZ", 300);
            RegisterBasicSKTs(dict, "G1EV", 200);
            RegisterBasicSKTs(dict, "G1WJ", 200);
            
            foreach (string key in dict.Keys)
            {
                List<SkTriple> value = dict[key];
                if (value.Count > 0)
                    value.Sort(SkTriple.Cmp);
            }
            //return dict;
        }

        private void RegisterBasicSKTs(IDictionary<string, List<SkTriple>> dict, string name, int priorty)
        {
            Algo.AddToMultiMap(dict, name, new SkTriple() { Name = "~" + priorty, Priorty = priorty, Occur = name });
        }

        private List<SKE> ParseSingleFromSKTriples(List<SkTriple> list,
            string zero, bool ucr, ushort puid, Skill skill)
        {
            List<SKE> result = new List<SKE>();
            ucr &= (Board.UseCardRound != 0);
            if (ucr && Board.Garden[puid].Team != Board.UseCardRound)
                return result;

            foreach (SkTriple skt in list)
            {
                if (skt.Name == skill.Code)
                {
                    if (skt.Type == SKTType.BK)
                        result.Add(new SKE(skt) { Tg = puid });
                    else if (skt.Type == SKTType.SK && puid == skt.Owner)
                        result.Add(new SKE(skt) { Tg = puid });
                }
            }
            return result;
        }
        // add newcomers skill back to pocket to register it in runquad
        private void AddZhuSkillBackward(List<SKE> pocket, string zero, bool cond)
        {
            foreach (Player player in Board.Garden.Values.Where(p => p.IsZhu))
            {
                foreach (string skillStr in player.Skills)
                {
                    Skill skill = LibTuple.SL.EncodeSkill(skillStr);
                    if (!pocket.Any(p => p.Name == skill.Code))
                    {
                        pocket.AddRange(ParseSingleFromSKTriples(
                            sk02[zero], zero, cond, player.Uid, skill));
                    }
                }
                player.IsZhu = false;
            }
        }

        // Parse from SKTriple list to SKT list
        // $ucr: user's round, old controller for R*ZD.
        private List<SKE> ParseFromSKTriples(List<SkTriple> list, string zero, bool ucr)
        {
            List<SKE> result = new List<SKE>();
            ucr &= (Board.UseCardRound != 0);
            List<ushort> pys = ucr ? Board.Garden.Values.Where(p => p.Team == Board.UseCardRound)
                .Select(p => p.Uid).ToList() : Board.Garden.Keys.ToList();

            foreach (SkTriple skt in list)
                switch (skt.Type)
                {
                    case SKTType.BK:
                    case SKTType.TX:
                    case SKTType.EQ:
                    case SKTType.CZ:
                    case SKTType.EV:
                    case SKTType.SF:
                    case SKTType.YJ:
                        if (skt.Occur.Contains('#'))
                        {
                            if (!ucr || Board.Rounder.Team == Board.UseCardRound)
                                result.Add(new SKE(skt) { Tg = Board.Rounder.Uid });
                        }
                        else if (skt.Occur.Contains('$'))
                            result.AddRange(pys.Where(p => p != Board.Rounder.Uid)
                                .Select(p => new SKE(skt) { Tg = p }));
                        else if (skt.Occur.Contains('*'))
                            result.AddRange(pys.Select(p => new SKE(skt) { Tg = p }));
                        else
                            result.AddRange(pys.Select(p => new SKE(skt) { Tg = p }));
                        break;
                    case SKTType.PT:
                        if (skt.Consume != 2)
                        {
                            if (skt.Occur.Contains('#'))
                            {
                                if (!ucr || Board.Rounder.Team == Board.UseCardRound)
                                    result.Add(new SKE(skt) { Tg = Board.Rounder.Uid });
                            }
                            else if (skt.Occur.Contains('$'))
                                result.AddRange(pys.Where(p => p != Board.Rounder.Uid)
                                    .Select(p => new SKE(skt) { Tg = p }));
                            else if (skt.Occur.Contains('*'))
                                result.AddRange(pys.Select(p => new SKE(skt) { Tg = p }));
                            else
                                result.AddRange(pys.Select(p => new SKE(skt) { Tg = p }));
                        }
                        else
                            result.Add(new SKE(skt) { Tg = Board.Rounder.Uid });
                        break;
                    case SKTType.SK:
                    default:
                        if (!ucr || skt.Owner == 0 || Board.Garden[skt.Owner].Team == Board.UseCardRound)
                        {
                            bool b1 = skt.Occur.Contains('#') && skt.Owner != Board.Rounder.Uid;
                            bool b2 = skt.Occur.Contains('$') && skt.Owner == Board.Rounder.Uid;
                            if (!b1 && !b2)
                                result.Add(new SKE(skt) { Tg = skt.Owner });
                        }
                        break;
                }
            return result;
        }
        private bool SKE2Message(SKE ske, string zero, bool[] involved, string[] pris, List<string> locks)
        {
            var garden = Board.Garden;

            bool isAnySet = false;
            ske.Fuse = zero;
            bool sheid = (ske.Lock != true && Board.Silence.Count > 0);
            if (ske.Type == SKTType.SK && sk01.ContainsKey(ske.Name) && !sheid)
            {
                //skt.CardCode = 0;
                Skill skill = sk01[ske.Name];
                string lf = (skill.IsLinked(ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                if ((!ske.IsOnce || ske.Tick == 0) && skill.Valid(garden[ske.Tg], ske.InType, lf))
                {
                    if (ske.Lock == false || (ske.Lock == null && !garden[ske.Tg].IsSKOpt))
                    {
                        string msg = ske.Name;
                        // Only report the first param
                        string req = skill.Input(garden[ske.Tg], ske.InType, lf, "");
                        if (req != "")
                            msg += "," + req;
                        pris[ske.Tg] += ";" + msg;
                        //iTypes[skt.Tg] += "," + skt.InType;
                        involved[ske.Tg] |= true;
                    }
                    else
                        locks.Add(ske.Tg + "," + ske.Name + ";" + ske.InType);
                    isAnySet |= true;
                }
            }
            else if (ske.Type == SKTType.BK && sk01.ContainsKey(ske.Name) && !sheid)
            {
                //skt.CardCode = 0;
                Bless skill = (Bless)sk01[ske.Name];
                string lf = (skill.IsLinked(ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                if ((!ske.IsOnce || ske.Tick == 0) && skill.BKValid(garden[ske.Tg], ske.InType, lf, ske.Owner))
                {
                    if (ske.Lock == false || (ske.Lock == null && !garden[ske.Tg].IsSKOpt))
                    {
                        string msg = ske.Name + "(" + ske.Owner + ")";
                        // Only report the first param
                        string req = skill.Input(garden[ske.Tg], ske.InType, lf, ske.Owner.ToString());
                        if (req != "")
                            msg += "," + req;
                        pris[ske.Tg] += ";" + msg;
                        //iTypes[skt.Tg] += "," + skt.InType;
                        involved[ske.Tg] |= true;
                    }
                    else
                        locks.Add(ske.Tg + "," + ske.Name + "(" + ske.Owner + ")" + ";" + ske.InType);
                    isAnySet |= true;
                }
            }
            else if ((ske.Type == SKTType.TX || ske.Type == SKTType.EQ) && tx01.ContainsKey(ske.Name))
            {
                Base.Card.Tux tux = tx01[ske.Name];
                //IEnumerable<Player> detects;
                Player player = Board.Garden[ske.Tg];
                string lf = (tux.IsLinked(ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                if (tux.Valid(player, ske.InType, lf) && tux.Bribe(player, ske.InType, lf))
                {
                    //if (g && !tux.IsSelfType)
                    //if (!tux.IsSelfType)
                    //{
                    //    involved[0] |= true;  // All involved.
                    //    isAnySet |= true;
                    //}
                    //else // TODO: compete mode only
                    //{
                    if (ske.Type == SKTType.TX && player.Tux.Count > 0)
                    {
                        involved[ske.Tg] |= true;
                        if (!player.IsTPOpt)
                            isAnySet |= true;
                        //isAnySet |= true;
                    }
                    //}
                    foreach (ushort handCode in player.Tux)
                    {
                        Base.Card.Tux hand = LibTuple.TL.DecodeTux(handCode);
                        if (hand.Code.Equals(tux.Code))
                        {
                            if (!hand.IsTuxEqiup() || ske.Type == SKTType.TX)
                            {
                                //skt.CardCode = handCode;
                                //pris[player.Uid] += (";" + skt.Name + "," + handCode);
                                pris[player.Uid] += (";TX" + handCode);
                                //iTypes[skt.Tg] += "," + skt.InType;
                                involved[player.Uid] |= true;
                                isAnySet |= true;
                            }
                            //string inp = tux.Input(player, "");
                            //if (inp != "")
                            //    pris[player.Uid] += "," + inp;
                        }
                    }
                    foreach (ushort card in new ushort[] { player.Weapon,
                        player.Armor, player.Trove, player.ExEquip })
                    {
                        if (card != 0)
                        {
                            Base.Card.Tux hand = LibTuple.TL.DecodeTux(card);
                            if (hand.IsTuxEqiup() && hand.Code.Equals(tux.Code) && ske.Type == SKTType.EQ)
                            {
                                int consumeType = ske.Consume;
                                if (consumeType == 1 && Board.CsEqiups.Contains(player.Uid + "," + card))
                                    continue;
                                Base.Card.TuxEqiup tue = (Base.Card.TuxEqiup)hand;
                                bool vi = (!ske.IsOnce || ske.Tick == 0);
                                vi &= (tue.Type == Base.Card.Tux.TuxType.FJ && !player.ArmorDisabled)
                                    || (tue.Type == Base.Card.Tux.TuxType.WQ && !player.WeaponDisabled)
                                    || (tue.Type == Base.Card.Tux.TuxType.XB && !player.TroveDisabled);
                                string elf = (tue.IsLinked(consumeType, ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                                if (vi && tue.ConsumeValid(player, consumeType, ske.InType, elf))
                                {
                                    if (ske.Lock == false)
                                    {
                                        // Actually should be the same as skill case
                                        // Only report the first param
                                        string req = tue.ConsumeInput(garden[player.Uid], consumeType, ske.InType, elf, "");
                                        string msg = string.IsNullOrEmpty(req) ? "" : "," + req;
                                        //pris[player.Uid] += ";" + skt.Name + "," + card + msg;
                                        pris[player.Uid] += ";TX" + card + msg;
                                        //iTypes[skt.Tg] += "," + skt.InType + (skt.IsConsume ? "!" : "");
                                        involved[player.Uid] |= true;
                                    }
                                    else
                                        //locks.Add(player.Uid + "," + skt.Name + "," + card +
                                        //    ";" + skt.InType + (skt.IsConsume ? "!" : ""));
                                        locks.Add(player.Uid + ",TX" + card +
                                            ";" + ske.InType + (ske.Consume == 1 ? "!" : ""));
                                    isAnySet |= true;
                                }
                            }
                        }
                    }
                }
            }
            else if (ske.Type == SKTType.CZ && cz01.ContainsKey(ske.Name))
            {
                Base.Operation cz = cz01[ske.Name];
                Player player = Board.Garden[ske.Tg];
                if ((!ske.IsOnce || ske.Tick == 0) && cz.Valid(player, ske.Fuse))
                {
                    if (ske.Lock == false)
                    {
                        string msg = ske.Name;
                        // Only report the first param
                        string req = cz.Input(player, ske.Fuse, "");
                        if (req != "")
                            msg += "," + req;
                        pris[player.Uid] += ";" + msg;
                        //iTypes[skt.Tg] += "," + skt.InType;
                        involved[player.Uid] |= true;
                    }
                    else
                        locks.Add(player.Uid + "," + ske.Name + ";" + ske.InType);
                    isAnySet |= true;
                }
            }
            else if ((!ske.IsOnce || ske.Tick == 0) && ske.Type == SKTType.SF && sf01.ContainsKey(ske.Name))
            {
                Base.Rune sf = sf01[ske.Name];
                Player player = Board.Garden[ske.Tg];
                ushort rnCode = LibTuple.RL.GetSingleIndex(sf);
                if (player.Runes.Contains(rnCode) && sf.Valid(player, ske.Fuse))
                {
                    if (ske.Lock == false)
                    {
                        string msg = "FW" + rnCode;
                        string req = sf.Input(player, ske.Fuse, "");
                        if (req != "")
                            msg += "," + req;
                        pris[player.Uid] += ";" + msg;
                        involved[player.Uid] |= true;
                    }
                    else
                        locks.Add(player.Uid + ",FW" + rnCode + ";" + ske.InType);
                    isAnySet |= true;
                }
            }
            else if ((!ske.IsOnce || ske.Tick == 0) && ske.Type == SKTType.YJ &&
                nj01.ContainsKey(ske.Name) && Board.EscueBanned.Count == 0)
            {
                Base.NCAction na = nj01[ske.Name];
                Player player = Board.Garden[ske.Tg];
                foreach (ushort escue in player.Escue)
                {
                    ushort orgEscue = Base.Card.NMBLib.OriginalNPC(escue);
                    Base.Card.NPC npc = LibTuple.NL.Decode(orgEscue);
                    if (npc.Skills.Contains(na.Code) && na.EscueValid(player, escue, ske.InType, ske.Fuse))
                    {
                        if (ske.Lock == false)
                        {
                            string msg = "YJ" + orgEscue;
                            string req = na.EscueInput(player, escue, ske.InType, ske.Fuse, "");
                            if (req != "")
                                msg += "," + req;
                            pris[player.Uid] += ";" + msg;
                            involved[player.Uid] |= true;
                        }
                        else
                            locks.Add(player.Uid + ",YJ" + orgEscue + ";" + ske.InType);
                        isAnySet |= true;
                    }
                }
            }
            else if ((!ske.IsOnce || ske.Tick == 0) && ske.Type == SKTType.PT && mt01.ContainsKey(ske.Name))
            {
                Base.Card.Monster mt = mt01[ske.Name];
                int consumeType = ske.Consume;

                if (ske.Consume != 2)
                {
                    Player player = Board.Garden[ske.Tg];
                    if (!player.PetDisabled) // If Disabled, then do nothing.
                    {
                        foreach (ushort petCode in player.Pets)
                        {
                            if (petCode == 0 || Board.NotActionPets.Contains(petCode)) continue;
                            if (petCode == Board.Monster1 && consumeType == 1) continue;
                            Base.Card.Monster pet = LibTuple.ML.Decode(petCode);
                            string lf = (pet.IsLinked(ske.Consume, ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                            if (pet.Code.Equals(mt.Code) && mt.ConsumeValid(player, consumeType, ske.InType, lf))
                            {
                                if (consumeType == 1 && Board.CsPets.Contains(player.Uid + "," + petCode))
                                    continue;
                                if (ske.Lock == false)
                                {
                                    string req = pet.ConsumeInput(player, consumeType, ske.InType, lf, "");
                                    string msg = string.IsNullOrEmpty(req) ? "" : "," + req;
                                    pris[player.Uid] += ";PT" + petCode + msg;
                                    //iTypes[skt.Tg] += "," + skt.InType;
                                    involved[player.Uid] |= true;
                                }
                                else
                                    locks.Add(player.Uid + ",PT" + petCode + ";"
                                        + ske.InType + (ske.Consume == 1 ? "!" : ""));
                                isAnySet |= true;
                            }
                        }
                    }
                }
                else
                {
                    Player player = Board.Rounder;
                    ushort mon1 = Board.Monster1;
                    if (mon1 != 0 && Board.InCampaign && Board.IsMonsterDebut)
                    {
                        Base.Card.Monster mon = LibTuple.ML.Decode(mon1);
                        string lf = (mon.IsLinked(ske.Consume, ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                        if (mon != null && mon == mt && mt.ConsumeValid(player, consumeType, ske.InType, lf))
                        {
                            if (ske.Lock == false)
                            {
                                string req = mon.ConsumeInput(player, consumeType, ske.InType, lf, "");
                                string msg = string.IsNullOrEmpty(req) ? "" : "," + req;
                                pris[player.Uid] += ";PT" + mon1 + msg;
                                //iTypes[skt.Tg] += "," + skt.InType;
                                involved[player.Uid] |= true;
                            }
                            else
                                locks.Add(player.Uid + ",PT" + mon1 + ";" + ske.InType + "!!");
                            isAnySet |= true;
                        }
                    }
                }
            }
            else if (ske.Type == SKTType.EV && ev01.ContainsKey(ske.Name))
            {
                //skt.CardCode = 0;
                Base.Card.Evenement eve = ev01[ske.Name];
                if (Board.Eve != 0 && LibTuple.EL.DecodeEvenement(Board.Eve) == eve)
                {
                    if ((!ske.IsOnce || ske.Tick == 0) && eve.PersValid())
                    {
                        if (ske.Lock == false)
                        {
                            string msg = ske.Name;
                            // Ignore param now.
                            //string req = eve.Input("");
                            //if (req != "")
                            //    msg += "," + req;
                            pris[ske.Tg] += ";" + msg;
                            //iTypes[skt.Tg] += "," + skt.InType;
                            involved[ske.Tg] |= true;
                        }
                        else
                            locks.Add(ske.Tg + "," + ske.Name + ";" + ske.InType);
                        isAnySet |= true;
                    }
                }
            }
            return isAnySet;
        }

        private int LockSkillCompare(string sk1, string sk2)
        {
            int me1 = ushort.Parse(sk1.Substring(0, sk1.IndexOf(',')));
            int me2 = ushort.Parse(sk2.Substring(0, sk2.IndexOf(',')));
            if (me1 == me2)
                return 0;
            foreach (Player player in new Player[] { Board.Rounder, Board.Supporter, Board.Hinder })
            {
                if (player != null && player.Uid != 0)
                {
                    if (me1 == player.Uid)
                        return me1;
                    else if (me2 == player.Uid)
                        return me2;
                }
            }
            if (Board.Rounder == null)
                return me1 - me2;
            int dme1 = me1 - Board.Rounder.Uid, dme2 = me2 - Board.Rounder.Uid;
            if (dme1 * dme2 > 0)
                return dme1 - dme2;
            else
                return dme2 - dme1;
        }

        internal bool IsOccurIncluded(string occur, ushort who, string skName,
            SKTType type, int inType, int consumeType, int owner, out int priority)
        {
            if (sk02.ContainsKey(occur))
            {
                List<SkTriple> list = sk02[occur];
                foreach (SkTriple skt in list)
                {
                    if (skt.Name == skName && skt.Type == type &&
                        inType == skt.InType && (skt.Consume == consumeType) &&
                        (skt.Owner == owner || skt.Owner == 0 || owner == 0))
                    {
                        priority = skt.Priorty;
                        if (skt.Occur == occur)
                            return true;
                        if (skt.Occur.Replace("#", who.ToString()) == occur)
                            return true;
                        else if (Board.Garden.Keys.Where(p => p != who &&
                            (skt.Occur.Replace("$", p.ToString()) == occur)).Any())
                            return true;
                        else if (Board.Garden.Keys.Where(p =>
                            (skt.Occur.Replace("*", p.ToString()) == occur)).Any())
                            return true;
                        //return true;
                    }
                }
            }
            priority = 0;
            return false;
        }
        // Handle with single skill, occurs at hero enters
        private void AddSingleSkill(ushort ut, Skill skill, IDictionary<string, List<SkTriple>> dict,
            IDictionary<string, List<string>> links)
        {
            List<SkTriple> parasitism = new List<SkTriple>();
            for (int i = 0; i < skill.Occurs.Length; ++i)
            {
                string occur = skill.Occurs[i];
                SkTriple skt = new SkTriple()
                {
                    Name = skill.Code,
                    Priorty = skill.Priorities[i],
                    Owner = ut,
                    InType = i,
                    Type = skill.IsBK ? SKTType.BK : SKTType.SK,
                    Lock = skill.Lock[i],
                    IsOnce = skill.IsOnce[i],
                    Occur = occur,
                    IsTermini = skill.IsTermini[i]
                };
                if (occur.StartsWith("%"))
                {
                    int aliasSerial = int.Parse(occur.Substring("%".Length));
                    string theAlias = SKBranch.GetAas(aliasSerial);
                    foreach (string theA in theAlias.Split('&'))
                        Algo.AddToMultiMap(links, theA, skt.ToHeader());
                    skt.Occur = theAlias;
                    parasitism.Add(skt);
                }
                else if (occur.StartsWith("&"))
                {
                    occur = occur.Substring("&".Length);
                    string[] theBlias = occur.Split('&');
                    string[] theAlias = new string[theBlias.Length / 2];
                    for (int j = 0; j < theAlias.Length; ++j)
                        theAlias[j] = theBlias[2 * j] + "," + theBlias[2 * j + 1];
                    foreach (string theA in theAlias)
                        Algo.AddToMultiMap(links, theA, skt.ToHeader());
                    skt.Occur = string.Join("&", theAlias);
                    parasitism.Add(skt);
                }
                else
                {
                    if (skill.IsBK)
                        occur = occur.Replace('#', '*').Replace('$', '*');
                    if (occur.Contains('#'))
                    {
                        string oc = occur.Replace("#", ut.ToString());
                        Algo.AddToMultiMap(dict, oc, skt);
                        dict[oc].Sort(SkTriple.Cmp);
                    }
                    else if (occur.Contains('$'))
                    {
                        foreach (ushort p in Board.Garden.Keys)
                            if (p != ut)
                            {
                                string oc = occur.Replace("$", p.ToString());
                                Algo.AddToMultiMap(dict, oc, skt);
                                dict[oc].Sort(SkTriple.Cmp);
                            }
                    }
                    else if (occur.Contains('*'))
                    {
                        foreach (ushort p in Board.Garden.Keys)
                        {
                            string oc = occur.Replace("*", p.ToString());
                            Algo.AddToMultiMap(dict, oc, skt);
                            dict[oc].Sort(SkTriple.Cmp);
                        }
                    }
                    else
                    {
                        Algo.AddToMultiMap(dict, occur, skt);
                        dict[occur].Sort(SkTriple.Cmp);
                    }
                }
            }
            if (parasitism.Count > 0)
                PutParamsIntoDict(dict, parasitism, true);
        }

        // Handle with single skill when removed
        private void RemoveSingleSkill(ushort ut, Skill skill, IDictionary<string, List<SkTriple>> dict,
            IDictionary<string, List<string>> links)
        {
            List<int> paraInTypes = new List<int>();
            // maybe need check parasitism chain list from others?
            for (int i = 0; i < skill.Occurs.Length; ++i)
            {
                string occur = skill.Occurs[i];
                if (occur.StartsWith("%") || occur.StartsWith("&"))
                    paraInTypes.Add(i);
                else if (occur.Contains('#'))
                {
                    string oc = occur.Replace("#", ut.ToString());
                    if (dict.ContainsKey(oc))
                        dict[oc].RemoveAll(p => (p.Name == skill.Code && p.Owner == ut));
                }
                else if (occur.Contains('$'))
                {
                    foreach (ushort ky in Board.Garden.Keys)
                        if (ky != ut)
                        {
                            string oc = occur.Replace("$", ky.ToString());
                            if (dict.ContainsKey(oc))
                                dict[oc].RemoveAll(p => (p.Name == skill.Code && p.Owner == ut));
                        }
                }
                else if (occur.Contains('*'))
                {
                    foreach (ushort ky in Board.Garden.Keys)
                    {
                        string oc = occur.Replace("*", ky.ToString());
                        if (dict.ContainsKey(oc))
                            dict[oc].RemoveAll(p => (p.Name == skill.Code && p.Owner == ut));
                    }
                }
                else
                {
                    if (dict.ContainsKey(occur))
                        dict[occur].RemoveAll(p => (p.Name == skill.Code && p.Owner == ut));
                }
            }
            if (paraInTypes.Count > 0)
            {
                IDictionary<string, List<string>> invTable = new Dictionary<string, List<string>>();
                foreach (var pair in dict)
                    pair.Value.ForEach(p => Algo.AddToMultiMap(invTable, p.ToHeader(), pair.Key));
                foreach (int inType in paraInTypes)
                {
                    string[] theAlias = null;
                    string occur = skill.Occurs[inType];
                    if (occur.StartsWith("%"))
                    {
                        int aliasSerial = int.Parse(occur.Substring("%".Length));
                        theAlias = SKBranch.GetAas(aliasSerial).Split('&');
                    }
                    else if (occur.StartsWith("&"))
                    {
                        occur = occur.Substring("&".Length);
                        string[] theBlias = occur.Split('&');
                        theAlias = new string[theBlias.Length / 2];
                        for (int i = 0; i < theAlias.Length; ++i)
                            theAlias[i] = theBlias[2 * i] + "," + theBlias[2 * i + 1];
                    }
                    foreach (string term in theAlias)
                    {
                        if (links.ContainsKey(term))
                        {
                            links[term].Remove(skill.Code + "," + inType);
                            if (links[term].Count == 0)
                                links.Remove(term);
                        }
                        if (invTable.ContainsKey(term))
                        {
                            List<string> ocs = invTable[term];
                            foreach (string oc in ocs)
                            {
                                if (dict.ContainsKey(oc))
                                {
                                    dict[oc].RemoveAll(p => p.Name == skill.Code &&
                                        p.InType == inType && p.Owner == ut);
                                    if (dict[oc].Count == 0)
                                        dict.Remove(oc);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion Skill Inv-Register

        #region Util Methods

        private ushort NextRounder(int rounder)
        {
            ++rounder;
            int total = Board.Garden.Count;
            rounder = rounder % total;
            return (ushort)(rounder == 0 ? total : rounder);
        }

        public IDictionary<int, int> CalculatePetsScore()
        {
            IDictionary<int, int> dicts = new Dictionary<int, int>();
            foreach (Player player in Board.Garden.Values)
            {
                if (!dicts.ContainsKey(player.Team))
                    dicts.Add(player.Team, 0);
            }
            foreach (Player player in Board.Garden.Values)
            {
                if (player.IsAlive)
                {
                    foreach (ushort ut in player.Pets)
                        if (ut != 0)
                        {
                            int value = LibTuple.ML.Decode(ut).STR;
                            if (value > 0)
                                dicts[player.Team] += value;
                        }
                }
            }
            return dicts;
        }
        private static void Fill<Type>(Type[] arrays, Type value)
        {
            for (int i = 0; i < arrays.Length; ++i)
                arrays[i] = value;
        }
        private static bool IsAllClear<Type>(IEnumerable<Type> arrays, Type value)
        {
            foreach (Type val in arrays)
            {
                if (!val.Equals(value))
                    return false;
            }
            return true;
        }
        private ushort[] ExceptStaff(ushort to)
        {
            return ExceptStaff(new ushort[] { to });
        }
        private ushort[] ExceptStaff(ushort[] exc)
        {
            List<ushort> list = new List<ushort>(Board.Garden.Keys);
            foreach (ushort us in exc)
                list.Remove(us);
            return list.ToArray();
        }

        public string DisplayTux(ushort card)
        {
            return card == 0 ? "0:无" : card + ":" + LibTuple.TL.DecodeTux(card).Name;
        }
        public string DisplayTux(IEnumerable<ushort> cards)
        {
            if (!cards.Any())
                return "{}";
            return "{" + string.Join(",", cards.Select(p => DisplayTux(p))) + "}";
        }
        public string DisplayTux(string cardName)
        {
            return cardName + ":" + LibTuple.TL.Firsts.Find(p => p.Code == cardName).Name;
        }
        public string DisplayPlayer(ushort player)
        {
            if (player == 0)
                return "0:牌堆";
            else
            {
                int selHero = Board.Garden[player].SelectHero;
                if (selHero != 0)
                    return player + ":" + LibTuple.HL.InstanceHero(selHero).Name;
                else
                    return player + "#";
            }
        }
        public string DisplayPlayer(IEnumerable<ushort> players)
        {
            if (!players.Any())
                return "{}";
            return "{" + string.Join(",", players.Select(p => DisplayPlayer(p))) + "}";
        }
        private string DisplayMonster(ushort p)
        {
            return (p == 0) ? "0:没" : p + ":" + PSD.Base.Card.NMBLib.Decode(p, LibTuple.ML, LibTuple.NL).Name;
        }
        public string DisplayMonster(IEnumerable<ushort> mons)
        {
            if (!mons.Any())
                return "{}";
            return "{" + string.Join(",", mons.Select(p => DisplayMonster(p)) + "}");
        }
        public string DisplayEve(ushort eve)
        {
            return (eve == 0) ? "0:静" : LibTuple.EL.DecodeEvenement(eve).Name;
        }
        public string DisplayEve(IEnumerable<ushort> eves)
        {
            if (!eves.Any())
                return "{}";
            return "{" + string.Join(",", eves.Select(p => DisplayEve(p)) + "}");
        }
        public string DisplayProp(ushort prop)
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

        #endregion Util Methods

        #region Hero Selection
        public void SelectHero(int selCode, int levelCode, string[] trainer)
        {
            PCS = new PilesConstruct(LibTuple, levelCode, trainer);
            //int prpr = -1; // Params Specification Closed now. (e.g. 31->41, prpr = 4)
            //if (!int.TryParse(Algo.Substring(mode, 2, -1), out prpr))
            //    prpr = -1;
            int prpr = -1;
            var garden = Board.Garden;
            List<ushort> staff = garden.Keys.ToList(); staff.Sort();
            SelCode = selCode;
            WI.BCast("H0SM," + selCode + "," + levelCode);
            if (selCode == RuleCode.MODE_00)
            {
                for (ushort i = 1; i <= 6; ++i)
                    garden[i].SelectHero = mode00heroes[i - 1];
            }
            else if (selCode == RuleCode.MODE_31 && PCS.ListAllSeleableAndTestedHeros().Count >= 24)
            {
                if (prpr < 0)
                    prpr = 3;
                if (PCS.ListAllSeleableAndTestedHeros().Count >= 72)
                    ++prpr;
                CastingPick cp = new CastingPick(); Casting = cp;
                Base.Card.Hero[] heros = PCS.AllocateHerosRM((prpr + 1) * garden.Count);
                for (int i = 0; i < garden.Count; ++i)
                {
                    cp.Init(staff[i], Algo.TakeRange(heros, i * prpr, (i + 1) * prpr).Select(p => p.Avatar).ToList(),
                        new int[] { heros[garden.Count * prpr + i].Avatar }.ToList());
                }
                WI.BCast("H0RT,0");
                for (int i = 0; i < garden.Count; ++i)
                    WI.Send("H0RM," + cp.ToMessage(staff[i]), 0, staff[i]);
                List<ushort> replied = staff.ToList();
                WI.RecvInfStart();
                while (replied.Count > 0)
                {
                    bool suc = false;
                    Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                    if (msgs.Msg.StartsWith("H0RN,"))
                    {
                        int selectAva = int.Parse(msgs.Msg.Substring("H0RN,".Length));
                        if (selectAva == 0)
                        {
                            int dels = new Random().Next(3);
                            int olds = cp.Xuan[msgs.From][dels];
                            int news = cp.SwitchAt(msgs.From, dels);
                            if (news != 0)
                            {
                                WI.Send("H0RS," + olds + "," + news, 0, msgs.From);
                                suc = true;
                            }
                        }
                        else if (cp.Xuan[msgs.From].Contains(selectAva))
                        {
                            if (cp.Pick(msgs.From, selectAva))
                            {
                                replied.Remove(msgs.From);
                                garden[msgs.From].SelectHero = selectAva;
                                VI.Cout(0, "The selection of " + msgs.From + "# is: " + selectAva);
                                WI.Send("H0RO,1," + msgs.From + "," + selectAva, 0, msgs.From);
                                WI.Send("H0RO,0," + msgs.From, ExceptStaff(msgs.From));
                                WI.Live("H0RO,0," + msgs.From);
                                suc = true;
                            }
                        }
                        if (!suc)
                            WI.Send("H0RM," + cp.ToMessage(msgs.From), 0, msgs.From);
                    }
                }
                WI.RecvInfEnd();
            }
            else if ((selCode == RuleCode.MODE_31 && PCS.ListAllSeleableAndTestedHeros().Count < 24) || selCode == RuleCode.MODE_NM)
            {
                if (prpr < 0)
                    prpr = 3;
                CastingPick cp = new CastingPick(); Casting = cp;
                Base.Card.Hero[] heros = PCS.AllocateHerosRM(prpr * garden.Count);
                for (int i = 0; i < garden.Count; ++i)
                {
                    cp.Init(staff[i], Algo.TakeRange(heros, i * prpr,
                        (i + 1) * prpr).Select(p => p.Avatar).ToList());
                }
                WI.BCast("H0RT,0");
                for (int i = 0; i < garden.Count; ++i)
                    WI.Send("H0RM," + cp.ToMessage(staff[i]), 0, staff[i]);
                // $staff[i] reply their selections
                List<ushort> replied = staff.ToList();
                WI.RecvInfStart();
                while (replied.Count > 0)
                {
                    bool suc = false;
                    Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                    if (msgs.Msg.StartsWith("H0RN,"))
                    {
                        int selectAva = int.Parse(msgs.Msg.Substring("H0RN,".Length));
                        if (cp.Pick(msgs.From, selectAva))
                        {
                            replied.Remove(msgs.From);
                            garden[msgs.From].SelectHero = selectAva;
                            VI.Cout(0, "The selection of " + msgs.From + "# is: " + selectAva);
                            WI.Send("H0RO,1," + msgs.From + "," + selectAva, 0, msgs.From);
                            WI.Send("H0RO,0," + msgs.From, ExceptStaff(msgs.From));
                            WI.Live("H0RO,0," + msgs.From);
                            suc = true;
                        }
                    }
                    if (!suc)
                        WI.Send("H0RM," + cp.ToMessage(msgs.From), 0, msgs.From);
                }
                WI.RecvInfEnd();
            }
            else if (selCode == RuleCode.MODE_CJ) // No Casting in Mode of CJ
            {
                List<Base.Card.Hero> heros = PCS.ListAllSeleableAndTestedHeros();
                List<int> hts = heros.Select(p => p.Avatar).ToList();
                WI.BCast("H0RT,0");
                foreach (ushort ut in staff)
                    WI.Send("H0RM," + string.Join(",", hts), 0, ut);
                List<ushort> replied = staff.ToList();
                IDictionary<int, List<ushort>> table = new Dictionary<int, List<ushort>>();
                WI.RecvInfStart();
                while (replied.Count > 0)
                {
                    Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                    if (msgs.Msg.StartsWith("H0RN,"))
                    {
                        int selectAva = int.Parse(msgs.Msg.Substring("H0RN,".Length));
                        replied.Remove(msgs.From);
                        Algo.AddToMultiMap(table, selectAva, msgs.From);
                        hts.Remove(selectAva);
                        VI.Cout(0, "The selection of " + msgs.From + "# is: " + selectAva);
                        WI.Send("H0RO,1," + msgs.From + "," + selectAva, 0, msgs.From);
                        WI.Send("H0RO,0," + msgs.From, ExceptStaff(msgs.From));
                        WI.Live("H0RO,0," + msgs.From);
                    }
                }
                WI.RecvInfEnd();
                hts.Shuffle();
                foreach (var pair in table)
                {
                    if (pair.Value.Count == 1)
                        garden[pair.Value.First()].SelectHero = pair.Key;
                    else if (pair.Value.Count > 1)
                    {
                        int idxKeep = randomSeed.Next(pair.Value.Count);
                        for (int i = 0; i < pair.Value.Count; ++i)
                        {
                            if (idxKeep == i)
                                garden[pair.Value[i]].SelectHero = pair.Key;
                            else
                            {
                                int selectHero = hts.Last();
                                garden[pair.Value[i]].SelectHero = selectHero;
                                hts.RemoveAt(hts.Count - 1);
                            }
                        }
                    }
                }
            }
            else if (selCode == RuleCode.MODE_BP)
            {
                if (prpr < 0) prpr = 18;
                List<Base.Card.Hero> heros = PCS.AllocateHerosRM(prpr).ToList();
                CastingTable ct = new CastingTable(heros.Select(p => p.Avatar).ToList());
                Casting = ct;
                ushort round = 0; ushort side = 1; bool ban = true;
                WI.BCast("H0TT," + ct.ToMessage());
                do
                {
                    ushort ut = staff[(ushort)(round + side)];
                    if (ban)
                    {
                        WI.Send("H0TA," + string.Join(",", ct.Xuan), 0, ut);
                        WI.Send("H0SW,1," + ut, ExceptStaff(ut));
                        WI.Live("H0SW,1," + ut);
                        while (true)
                        {
                            string msg = WI.Recv(0, ut);
                            if (msg != null && msg.StartsWith("H0TB"))
                            {
                                int selAva = int.Parse(msg.Substring("H0TB,".Length));
                                if (ct.Ban(ut, selAva))
                                {
                                    WI.BCast("H0TC," + ut + "," + selAva); break;
                                }
                            }
                            WI.Send("H0TA," + string.Join(",", ct.Xuan), 0, ut);
                        }
                    }
                    else
                    {
                        WI.Send("H0TX," + string.Join(",", ct.Xuan), 0, ut);
                        WI.Send("H0SW,0," + ut, ExceptStaff(ut));
                        WI.Live("H0SW,0," + ut);
                        while (true)
                        {
                            string msg = WI.Recv(0, ut);
                            if (msg != null && msg.StartsWith("H0TN"))
                            {
                                int selAva = int.Parse(msg.Substring("H0TN,".Length));
                                if (ct.Pick(ut, selAva))
                                {
                                    WI.BCast("H0TO," + ut + "," + selAva); break;
                                }
                            }
                            WI.Send("H0TX," + string.Join(",", ct.Xuan), 0, ut);
                        }
                    }
                    if (side == 1) { side = 0; }
                    else if (side == 0 && ban) { side = 1; ban = false; }
                    else { side = 1; ban = true; round += 2; }
                } while (round < staff.Count);
                foreach (var pair in ct.Ding)
                    garden[pair.Key].SelectHero = pair.Value;
            }
            else if (selCode == RuleCode.MODE_RD)
            {
                if (prpr < 0)
                    prpr = 12;
                List<Base.Card.Hero> heros = PCS.AllocateHerosRM(prpr).ToList();
                CastingTable ct = new CastingTable(heros.Select(p => p.Avatar).ToList());
                Casting = ct;
                ushort ut = 2;
                WI.BCast("H0TT," + ct.ToMessage());
                do
                {
                    WI.Send("H0TX," + ct.ToMessage(), 0, ut);
                    WI.Send("H0SW,0," + ut, ExceptStaff(ut));
                    WI.Live("H0SW,0," + ut);
                    while (true)
                    {
                        string msg = WI.Recv(0, ut);
                        if (msg != null && msg.StartsWith("H0TN"))
                        {
                            int selAva = int.Parse(msg.Substring("H0TN,".Length));
                            if (ct.Pick(ut, selAva))
                            {
                                garden[ut].SelectHero = selAva;
                                WI.BCast("H0TO," + ut + "," + selAva);
                                break;
                            }
                            else
                                WI.Send("H0TX," + ct.ToMessage(), 0, ut);
                        }
                    }
                    if (ut % 2 == 0)
                        --ut;
                    else
                        ut += 3;
                } while (ut <= staff.Count);
                foreach (var pair in ct.Ding)
                    garden[pair.Key].SelectHero = pair.Value;
            }
            else if (selCode == RuleCode.MODE_ZY)
            {
                Base.Card.Hero[] heros = PCS.AllocateHerosRM(24).ToArray();
                int hi = 0, hr = 0; int[] icr = new int[] { 3, 4, 5 };
                CastingPick cp = new CastingPick();
                Casting = cp;
                for (int i = 0; i < staff.Count; i += 2)
                {
                    cp.Init(staff[i], Algo.TakeRange(heros, hi, hi + icr[hr])
                        .Select(p => p.Avatar).ToList());
                    hi += icr[hr];
                    cp.Init(staff[i + 1], Algo.TakeRange(heros, hi, hi + icr[hr])
                        .Select(p => p.Avatar).ToList());
                    hi += icr[hr];
                    ++hr;
                }
                WI.BCast("H0RT,0");
                for (int i = 0; i < staff.Count; i += 2)
                {
                    ushort ud = staff[i], ut = staff[i + 1];
                    WI.RecvInfStart();
                    WI.Send("H0RM," + cp.ToMessage(ud), 0, ud);
                    WI.Send("H0RM," + cp.ToMessage(ut), 0, ut);
                    WI.BCast("H0SW,0," + ud + "," + ut);
                    List<ushort> replied = (new ushort[] { ud, ut }).ToList();
                    IDictionary<ushort, string> selAnswer = new Dictionary<ushort, string>();
                    while (replied.Count > 0)
                    {
                        bool suc = false;
                        Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                        if (replied.Contains(msgs.From) && msgs.Msg.StartsWith("H0RN,"))
                        {
                            int selectAva = int.Parse(msgs.Msg.Substring("H0RN,".Length));
                            if (cp.Pick(msgs.From, selectAva))
                            {
                                replied.Remove(msgs.From);
                                garden[msgs.From].SelectHero = selectAva;
                                VI.Cout(0, "The selection of " + msgs.From + "# is: " + selectAva);
                                WI.Send("H0RO,1," + msgs.From + "," + selectAva, 0, msgs.From);
                                WI.Send("H0RO,0," + msgs.From, ExceptStaff(msgs.From));
                                WI.Live("H0RO,0," + msgs.From);
                                selAnswer.Add(msgs.From, "H0RO,1," + msgs.From + "," + selectAva);
                                suc = true;
                            }
                        }
                        if (!suc)
                            WI.Send("H0RM," + cp.ToMessage(msgs.From), 0, msgs.From);
                    }
                    foreach (var pair in selAnswer)
                    {
                        WI.Send(pair.Value, ExceptStaff(pair.Key));
                        WI.Live(pair.Value);
                    }
                    WI.RecvInfEnd();
                }
            }
            else if (selCode == RuleCode.MODE_CP)
            {
                if (prpr < 0)
                    prpr = 16;
                else if (prpr % 2 != 0)
                    ++prpr;
                int half = prpr / 2;
                Base.Card.Hero[] heros = PCS.AllocateHerosRM(prpr).ToArray();

                Base.Rules.CastingCongress cc = new Base.Rules.CastingCongress(
                    Algo.TakeRange(heros, 0, half).Select(p => p.Avatar).ToList(),
                    Algo.TakeRange(heros, half, prpr).Select(p => p.Avatar).ToList(),
                    Algo.TakeRange(heros, 0, prpr).Select(p => p.Avatar).ToList());
                //cc.Viewable = false;
                Casting = cc;
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).IsTalkSilence = true;

                WI.Send("H0CT," + half + "," + string.Join(",", cc.XuanAka) + "," + half + "," +
                    string.Join(",", Enumerable.Repeat(0, half)) + "," +
                    string.Join(",", staff.Select(p => p + ",0")), staff.Where(p => p % 2 == 1).ToArray());
                WI.Send("H0CT," + half + "," + string.Join(",", Enumerable.Repeat(0, half)) +
                     "," + half + "," + string.Join(",", cc.XuanAo) + "," +
                    string.Join(",", staff.Select(p => p + ",0")), staff.Where(p => p % 2 == 0).ToArray());

                List<ushort> decided = new ushort[] { 3, 4 }.ToList();
                WI.RecvInfStart();
                while (decided.Count > 0)
                {
                    Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                    ushort[] teamates = staff.Where(p => (p % 2 == msgs.From % 2)).ToArray();
                    if (msgs.Msg.StartsWith("H0CN,"))
                    {
                        string[] parts = msgs.Msg.Split(',');
                        //ushort who = ushort.Parse(parts[1]);
                        int selectAva = int.Parse(parts[2]);
                        int putBack = cc.Set(msgs.From, selectAva);
                        if (putBack >= 0)
                            WI.Send("H0CO," + msgs.From + "," + selectAva + "," + putBack + ",0", teamates);
                    }
                    else if (msgs.Msg.StartsWith("H0CB,"))
                    {
                        int backAva = int.Parse(msgs.Msg.Substring("H0CB,".Length));
                        if (cc.Set(0, backAva) >= 0)
                            WI.Send("H0CC," + msgs.From + "," + backAva, teamates);
                    }
                    else if (msgs.Msg.StartsWith("H0CD,"))
                    {
                        if (cc.IsDecide(msgs.From))
                        {
                            WI.Send("H0CE,2," + string.Join(",", teamates.Select(
                                p => p + "," + cc.Ding[p])), teamates);
                            WI.Send("H0CE,1," + (msgs.From % 2 == 1 ? 2 : 1), ExceptStaff(teamates));
                            WI.Live("H0CE,1," + (msgs.From % 2 == 1 ? 2 : 1));
                            decided.Remove(msgs.From);
                        }
                        else
                            WI.Send("H0CE,0", 0, msgs.From);
                    }
                }
                WI.RecvInfEnd();
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).IsTalkSilence = false;
                foreach (var pair in cc.Ding)
                    garden[pair.Key].SelectHero = pair.Value;
            }
            //else if (mode.StartsWith("IN"))
            //{
            //    if (prpr < 0)
            //        prpr = 5;
            //    Base.Card.Hero[] heros = PCS.AllocateHerosRM(13 + prpr);
            //    List<int> cas = Algo.TakeRange(heros, 0, prpr).Select(p => p.Avatar).ToList();
            //    int next = prpr;
            //    bool randomed1 = false, randomed2 = false;
            //    for (int i = 0; i < staff.Count; i += 2)
            //    {
            //        ushort ut = staff[i];
            //        ushort ud = staff[i + 1];
            //        int newAva = 0;

            //        WI.BCast("H0CX," + string.Join(",", cas));
            //        WI.BCast("H0SB,0," + ut);
            //        while (true)
            //        {
            //            string msg = WI.Recv(0, ut);
            //            if (msg != "" && msg.StartsWith("H0RB,"))
            //            {
            //                int banAva = int.Parse(msg.Substring("H0RB,".Length));
            //                if (cas.Contains(banAva))
            //                {
            //                    cas.Remove(banAva);
            //                    WI.BCast("H0SB," + ut + "," + banAva);
            //                    break;
            //                }
            //                else
            //                    WI.Send("H0SB,0," + ut, 0, ut);
            //            }
            //        }
            //        cas.Add(newAva = heros[next++].Avatar);
            //        WI.BCast("H0CI," + newAva);
            //        WI.BCast("H0CX," + string.Join(",", cas));
            //        WI.BCast("H0SB,0," + ud);
            //        while (true)
            //        {
            //            string msg = WI.Recv(0, ud);
            //            if (msg != "" && msg.StartsWith("H0RB,"))
            //            {
            //                int banAva = int.Parse(msg.Substring("H0RB,".Length));
            //                if (cas.Contains(banAva))
            //                {
            //                    cas.Remove(banAva);
            //                    WI.BCast("H0SB," + ud + "," + banAva);
            //                    break;
            //                }
            //                else
            //                    WI.Send("H0SB,0," + ud, 0, ud);
            //            }
            //        }
            //        cas.Add(newAva = heros[next++].Avatar);
            //        WI.BCast("H0CI," + newAva);
            //        WI.BCast("H0CX," + string.Join(",", cas));
            //        WI.BCast("H0SP,0," + ut + (randomed1 ? "" : "!"));
            //        while (true)
            //        {
            //            string msg = WI.Recv(0, ut);
            //            if (msg != "" && msg.StartsWith("H0RP,"))
            //            {
            //                int selAva = int.Parse(msg.Substring("H0RP,".Length));
            //                if (cas.Contains(selAva))
            //                {
            //                    cas.Remove(selAva);
            //                    garden[ut].SelectHero = selAva;
            //                    WI.BCast("H0SP," + ut + "," + selAva);
            //                    int newAva1 = heros[next++].Avatar;
            //                    int newAva2 = heros[next++].Avatar;
            //                    cas.Add(newAva1); cas.Add(newAva2);
            //                    WI.BCast("H0CI," + newAva1 + "," + newAva2);
            //                    break;
            //                }
            //                else if (selAva == 0 && !randomed1)
            //                {
            //                    randomed1 = true;
            //                    selAva = heros[next++].Avatar;
            //                    garden[ut].SelectHero = selAva;
            //                    WI.BCast("H0SP," + ut + "," + selAva);
            //                    cas.Add(newAva = heros[next++].Avatar);
            //                    WI.BCast("H0CI," + newAva);
            //                    break;
            //                } else
            //                    WI.Send("H0SP,0," + ut + (randomed1 ? "" : "!"), 0, ut);
            //            }
            //        }
            //        WI.BCast("H0CX," + string.Join(",", cas));
            //        WI.BCast("H0SP,0," + ud + (randomed2 ? "" : "!"));
            //        while (true)
            //        {
            //            string msg = WI.Recv(0, ud);
            //            if (msg != "" && msg.StartsWith("H0RP,"))
            //            {
            //                int selAva = int.Parse(msg.Substring("H0RP,".Length));
            //                if (cas.Contains(selAva))
            //                {
            //                    cas.Remove(selAva);
            //                    garden[ud].SelectHero = selAva;
            //                    WI.BCast("H0SP," + ud + "," + selAva);
            //                    break;
            //                }
            //                else if (selAva == 0 && !randomed2)
            //                {
            //                    randomed2 = true;
            //                    selAva = heros[next++].Avatar;
            //                    garden[ud].SelectHero = selAva;
            //                    WI.BCast("H0SP," + ud + "," + selAva);
            //                    // ban another one
            //                    WI.BCast("H0CX," + string.Join(",", cas));
            //                    WI.BCast("H0SB,0," + ud);
            //                    while (true)
            //                    {
            //                        string bmsg = WI.Recv(0, ud);
            //                        if (bmsg != "" && bmsg.StartsWith("H0RB,"))
            //                        {
            //                            int banAva = int.Parse(bmsg.Substring("H0RB,".Length));
            //                            if (cas.Contains(banAva))
            //                            {
            //                                cas.Remove(banAva);
            //                                WI.BCast("H0SB," + ud + "," + banAva);
            //                                break;
            //                            }
            //                        }
            //                    }
            //                    break;
            //                } else
            //                    WI.Send("H0SP,0," + ud + (randomed2 ? "" : "!"), 0, ud);
            //            }
            //        }
            //    }
            //}
            else if (selCode == RuleCode.MODE_CM)
            {
                if (prpr <= 0) prpr = 16;
                else if (prpr < 11) prpr = 11;
                List<Base.Card.Hero> heros = PCS.AllocateHerosRM(prpr).ToList();
                CastingTable ct = new CastingTable(heros.Select(p => p.Avatar).ToList());
                Casting = ct;
                WI.BCast("H0TT," + ct.ToMessage());
                int startRank = randomSeed.Next(3);
                int iRank = startRank;

                ushort[] banUt = new ushort[] { (ushort)(iRank * 2 + 2),
                    (ushort)(iRank * 2 + 1), (ushort)(iRank * 2 + 1) };
                foreach (ushort ut in banUt)
                {
                    WI.Send("H0TA," + string.Join(",", ct.Xuan), 0, ut);
                    WI.Send("H0SW,1," + ut, ExceptStaff(ut));
                    WI.Live("H0SW,1," + ut);
                    while (true)
                    {
                        string msg = WI.Recv(0, ut);
                        if (msg != null && msg.StartsWith("H0TB"))
                        {
                            int selAva = int.Parse(msg.Substring("H0TB,".Length));
                            if (ct.Ban(ut, selAva))
                            {
                                WI.BCast("H0TC," + ut + "," + selAva); break;
                            }
                        }
                        WI.Send("H0TA," + string.Join(",", ct.Xuan), 0, ut);
                    }
                }
                do
                {
                    banUt = new ushort[] { (ushort)(iRank * 2 + 2), (ushort)(iRank * 2 + 1) };
                    foreach (ushort ut in banUt)
                    {
                        WI.Send("H0TX," + string.Join(",", ct.Xuan), 0, ut);
                        WI.Send("H0SW,0," + ut, ExceptStaff(ut));
                        WI.Live("H0SW,0," + ut);
                        while (true)
                        {
                            string msg = WI.Recv(0, ut);
                            if (msg != null && msg.StartsWith("H0TN"))
                            {
                                int selAva = int.Parse(msg.Substring("H0TN,".Length));
                                if (ct.Pick(ut, selAva))
                                {
                                    WI.BCast("H0TO," + ut + "," + selAva); break;
                                }
                            }
                            WI.Send("H0TX," + string.Join(",", ct.Xuan), 0, ut);
                        }
                        // Insert Ban
                        if ((ut % 2 == 0 && ct.BanAo.Count < 2) || (ut % 2 == 1 && ct.BanAka.Count < 3))
                        {
                            WI.Send("H0TA," + string.Join(",", ct.Xuan) + ",0", 0, ut);
                            WI.Send("H0SW,1," + ut, ExceptStaff(ut));
                            WI.Live("H0SW,1," + ut);
                            while (true)
                            {
                                string msg = WI.Recv(0, ut);
                                if (msg != null && msg.StartsWith("H0TB"))
                                {
                                    int selAva = int.Parse(msg.Substring("H0TB,".Length));
                                    if (selAva == 0) // give up ban chance
                                    {
                                        WI.BCast("H0TC," + ut + ",0"); break;
                                    }
                                    else if (ct.Ban(ut, selAva))
                                    {
                                        WI.BCast("H0TC," + ut + "," + selAva); break;
                                    }
                                }
                                WI.Send("H0TA," + string.Join(",", ct.Xuan) + ",0", 0, ut);
                            }
                        }
                    }
                    ++iRank;
                    if (iRank > 2) iRank = 0;
                } while (iRank != startRank);
                foreach (var pair in ct.Ding)
                    garden[pair.Key].SelectHero = pair.Value;
            }
            else if (selCode == RuleCode.MODE_SS)
            {
                if (prpr <= 0) prpr = 16;
                if (prpr % 2 != 0) ++prpr;
                if (PCS.ListAllSeleableHeros().Length > 72)
                    prpr -= 2;
                List<Base.Card.Hero> heros = PCS.AllocateHerosRM(prpr).ToList();
                CastingPublic cp = new CastingPublic(heros.Select(p => p.Avatar).ToList());
                Casting = cp;
                WI.BCast("H0PT," + cp.ToMessage());
                ushort[] captain = new ushort[] { 4, 3, 3, 4 };
                for (int i = 0; i < 2; ++i)
                {
                    ushort ut = captain[i];
                    int team = ut % 2 == 0 ? 2 : 1;
                    WI.BCast("H0PA," + ut + "," + string.Join(",", cp.Xuan));
                    while (true)
                    {
                        string msg = WI.Recv(0, ut);
                        if (msg != null && msg.StartsWith("H0PB,"))
                        {
                            int selAva = int.Parse(msg.Substring("H0PB,".Length));
                            if (cp.Ban(team == 1, selAva))
                            {
                                WI.BCast("H0PC," + team + "," + selAva); break;
                            }
                            else
                                WI.BCast("H0PA," + ut + "," + string.Join(",", cp.Xuan));
                        }
                    }
                }
                WI.BCast("H0PT," + cp.ToMessage());
                int cidx = 0;
                do
                {
                    ushort ut = captain[cidx % 4];
                    int team = ut % 2 == 0 ? 2 : 1;
                    ushort[] teamates = staff.Where(p => (p % 2 == ut % 2)).ToArray();
                    WI.BCast("H0PM," + ut + "," + string.Join(",", cp.Xuan));
                    while (true)
                    {
                        string msg = WI.Recv(0, ut);
                        if (msg != null && msg.StartsWith("H0PN,"))
                        {
                            int selAva = int.Parse(msg.Substring("H0PN,".Length));
                            int resAva = cp.Pick(team == 1, selAva);
                            if (resAva == selAva)
                            {
                                WI.BCast("H0PO," + team + "," + selAva); break;
                            }
                            else if (resAva != 0)
                            {
                                WI.Send("H0PO," + team + "," + resAva, teamates);
                                WI.Send("H0PO," + team + ",0", ExceptStaff(teamates));
                                WI.Live("H0PO," + team + ",0"); break;
                            }
                            else
                                WI.BCast("H0PM," + ut + "," + string.Join(",", cp.Xuan));
                        }
                    }
                    ++cidx;
                    if (cidx % 4 == 0 && cp.Xuan.Count > 0)
                        WI.BCast("H0PT," + cp.ToMessage());
                } while (cp.Xuan.Count > 0);
                WI.BCast("H0SN,0");
                int half = cp.DingAka.Count;
                Base.Rules.CastingCongress cc = new Base.Rules.CastingCongress(
                    cp.DingAka, cp.DingAo, new List<int>());
                //cc.Viewable = true;
                Casting = cc;
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).IsTalkSilence = true;

                //WI.BCast("H0CI," + half + "," + string.Join(",", cc.XuanAka) + "," + half + "," +
                //    string.Join(",", cc.XuanAo) + "," + string.Join(",", staff.Select(p => p + ",0")));
                foreach (var pair in Board.Garden)
                    WI.Send("H0CI," + cc.ToMessage(pair.Value.Team == 1, false), 0, pair.Key);
                WI.Live("H0CI," + cc.ToMessage(true, true));

                List<ushort> decided = new ushort[] { 3, 4 }.ToList();
                WI.RecvInfStart();
                while (decided.Count > 0)
                {
                    Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                    ushort[] teamates = staff.Where(p => (p % 2 == msgs.From % 2)).ToArray();
                    if (msgs.Msg.StartsWith("H0CN,"))
                    {
                        string[] parts = msgs.Msg.Split(',');
                        ushort who = ushort.Parse(parts[1]);
                        int ava = int.Parse(parts[2]);
                        ushort putBackTo;
                        int putBack = cc.Set(who, ava, out putBackTo);
                        if (putBack >= 0)
                            WI.Send("H0CO," + who + "," + ava + "," + putBack + "," + putBackTo, teamates);
                    }
                    else if (msgs.Msg.StartsWith("H0CB,"))
                    {
                        int backAva = int.Parse(msgs.Msg.Substring("H0CB,".Length));
                        if (cc.Set(0, backAva) >= 0)
                            WI.Send("H0CC," + msgs.From + "," + backAva, teamates);
                    }
                    else if (msgs.Msg.StartsWith("H0CD,"))
                    {
                        if (cc.IsDecide(msgs.From))
                        {
                            WI.Send("H0CE,2," + string.Join(",", teamates.Select(
                                p => p + "," + cc.Ding[p])), teamates);
                            WI.Send("H0CE,1," + (msgs.From % 2 == 1 ? 2 : 1), ExceptStaff(teamates));
                            WI.Live("H0CE,1," + (msgs.From % 2 == 1 ? 2 : 1));
                            decided.Remove(msgs.From);
                        }
                        else
                            WI.Send("H0CE,0", 0, msgs.From);
                    }
                }
                WI.RecvInfEnd();
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).IsTalkSilence = false;
                foreach (var pair in cc.Ding)
                    garden[pair.Key].SelectHero = pair.Value;
            }
            else if (selCode == RuleCode.MODE_TC)
            {
                if (prpr <= 0) prpr = 12;
                else if (prpr % 2 != 0) ++prpr;
                List<Base.Card.Hero> heros = PCS.AllocateHerosRM(prpr).ToList();
                CastingPublic cp = new CastingPublic(heros.Select(p => p.Avatar).ToList(), null, null,
                    null, null, heros.Take(prpr / 2).Select(p => p.Avatar).ToList());
                cp.Xuan.Shuffle();
                Casting = cp;
                WI.BCast("H0PT," + cp.ToMessage());
                ushort[] captain = new ushort[] { 4, 3, 3, 4 };
                int cidx = 0;
                do
                {
                    ushort ut = captain[cidx % 4];
                    int team = ut % 2 == 0 ? 2 : 1;
                    ushort[] teamates = staff.Where(p => (p % 2 == ut % 2)).ToArray();
                    WI.BCast("H0PM," + ut + "," + string.Join(",", cp.Xuan));
                    while (true)
                    {
                        string msg = WI.Recv(0, ut);
                        if (msg != null && msg.StartsWith("H0PN,"))
                        {
                            int selAva = int.Parse(msg.Substring("H0PN,".Length));
                            int resAva = cp.Pick(team == 1, selAva);
                            if (resAva == selAva)
                            {
                                WI.BCast("H0PO," + team + "," + selAva); break;
                            }
                            else if (resAva != 0)
                            {
                                WI.Send("H0PO," + team + "," + resAva, teamates);
                                WI.Send("H0PO," + team + ",0", ExceptStaff(teamates));
                                WI.Live("H0PO," + team + ",0"); break;
                            }
                            else
                                WI.BCast("H0PM," + ut + "," + string.Join(",", cp.Xuan));
                        }
                    }
                    ++cidx;
                    if (cidx % 4 == 0 && cp.Xuan.Count > 0)
                        WI.BCast("H0PT," + cp.ToMessage());
                } while (cp.Xuan.Count > 0);
                WI.BCast("H0SN,0");
                int half = cp.DingAka.Count;
                Base.Rules.CastingCongress cc = new Base.Rules.CastingCongress(
                    cp.DingAka, cp.DingAo, cp.Secrets);
                //cc.Viewable = true;
                Casting = cc;
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).IsTalkSilence = true;

                //WI.BCast("H0CI," + half + "," + string.Join(",", cc.XuanAka) + "," + half + "," +
                //    string.Join(",", cc.XuanAo) + "," + string.Join(",", staff.Select(p => p + ",0")));
                foreach (var pair in Board.Garden)
                    WI.Send("H0CI," + cc.ToMessage(pair.Value.Team == 1, false), 0, pair.Key);
                WI.Live("H0CI," + cc.ToMessage(true, true));

                List<ushort> decided = new ushort[] { 3, 4 }.ToList();
                WI.RecvInfStart();
                while (decided.Count > 0)
                {
                    Base.VW.Msgs msgs = WI.RecvInfRecvPending();
                    ushort[] teamates = staff.Where(p => (p % 2 == msgs.From % 2)).ToArray();
                    if (msgs.Msg.StartsWith("H0CN,"))
                    {
                        string[] parts = msgs.Msg.Split(',');
                        ushort who = ushort.Parse(parts[1]);
                        int ava = int.Parse(parts[2]);
                        ushort putBackTo;
                        int putBack = cc.Set(who, ava, out putBackTo);
                        if (putBack >= 0)
                            WI.Send("H0CO," + who + "," + ava + "," + putBack + "," + putBackTo, teamates);
                    }
                    else if (msgs.Msg.StartsWith("H0CB,"))
                    {
                        int backAva = int.Parse(msgs.Msg.Substring("H0CB,".Length));
                        if (cc.Set(0, backAva) >= 0)
                            WI.Send("H0CC," + msgs.From + "," + backAva, teamates);
                    }
                    else if (msgs.Msg.StartsWith("H0CD,"))
                    {
                        if (cc.IsDecide(msgs.From))
                        {
                            WI.Send("H0CE,2," + string.Join(",", teamates.Select(
                                p => p + "," + cc.Ding[p])), teamates);
                            WI.Send("H0CE,1," + (msgs.From % 2 == 1 ? 2 : 1), ExceptStaff(teamates));
                            WI.Live("H0CE,1," + (msgs.From % 2 == 1 ? 2 : 1));
                            decided.Remove(msgs.From);
                        }
                        else
                            WI.Send("H0CE,0", 0, msgs.From);
                    }
                }
                WI.RecvInfEnd();
                if (WI is VW.Aywi)
                    (WI as VW.Aywi).IsTalkSilence = false;
                foreach (var pair in cc.Ding)
                    garden[pair.Key].SelectHero = pair.Value;
            }
            else // if (mode == "RM")
            {
                Base.Card.Hero[] heros = PCS.AllocateHerosRM(garden.Count);
                for (int i = 0; i < heros.GetLength(0); ++i)
                    garden[staff[i]].SelectHero = heros[i].Avatar;
            }
            string selAll = string.Join(",", garden.Values.Select(p => p.Uid + "," + p.SelectHero));
            VI.Cout(0, "Selection: " + selAll);
            WI.BCast("H0SL," + selAll);
            WI.BCast("H0SN,0");
        }

        #endregion Hero Selection

        public static void SafeExecute(Action action, Action<Exception> handler)
        {
            try { action(); }
            catch (System.Threading.ThreadAbortException) { }
            catch (System.OperationCanceledException) { throw; }
            catch (Exception ex) { handler(ex); Console.WriteLine(ex.ToString()); }
        }

        public static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                int argType = int.Parse(args[0]);
                if (argType == 1 && args.Length >= 5)
                {
                    int room = int.Parse(args[1]);
                    int[] opts = args[2].Split(',').Select(p => int.Parse(p)).ToArray();
                    string[] trainer = (args[3] == "^") ? null : args[3].Split(',');
                    ushort[] invs = args[4].Split(',').Select(p => ushort.Parse(p)).ToArray();
                    new XI().StartRoom(room, opts, invs, trainer);
                }
                if (argType == 0)
                    new XI().StartRoom(args);
            }
            else
                new XI().StartRoom(null);
            int 雷杀 = 9, 火杀 = 4;
            if (雷杀 == 火杀)
                Console.WriteLine();
        }
    }
}
