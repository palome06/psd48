using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.ClientZero
{
    public class ZeroBase
    {
        protected XIClient xic;

        protected ZeroBase(XIClient xic)
        {
            this.xic = xic;
        }

        protected static void Aps(StringBuilder sb, string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args));
        }
    }

    public class ZeroPiles : ZeroBase
    {
        private int mTuxCount;

        public int TuxCount
        {
            set
            {
                mTuxCount = value;
                if (mTuxCount < 0)
                {
                    mTuxCount += TuxDises;
                    TuxDises = 0;
                }
            }
            get { return mTuxCount; }
        }

        public int MonCount { set; get; }

        private int mEveCount;

        public int EveCount
        {
            set
            {
                mEveCount = value;
                if (mEveCount < 0)
                {
                    mEveCount += EveDises;
                    EveDises = 0;
                }
            }
            get { return mEveCount; }
        }

        public int TuxDises { set; get; }

        public int EveDises { set; get; }

        public int MonDises { set; get; }

        public IDictionary<int, int> Score { private set; get; }

        public ZeroPiles(XIClient xic) : base(xic)
        {
            TuxCount = 0; MonCount = 0; EveCount = 0;
            TuxDises = 0; MonDises = 0; EveDises = 0;
            Score = new Dictionary<int, int>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Aps(sb, "***************");
            Aps(sb, "剩余牌数 - 手牌: {0}，怪物牌：{1}，事件牌：{2}",
                TuxCount, MonCount, EveCount);
            Aps(sb, "弃牌堆数 - 手牌: {0}，怪物牌：{1}，事件牌：{2}",
                TuxDises, MonDises, EveDises);
            if (Score.Count > 0)
            {
                List<int> sides = Score.Keys.ToList();
                sides.Sort();
                string scores = string.Join("  ", sides.Select(p => p + "方-" + Score[p]));
                Aps(sb, "宠物战绩榜：{0}", scores);
            }
            Aps(sb, "***************");
            return sb.ToString();
        }
    }

    public class ZeroField : ZeroBase
    {
        public ushort Monster1 { set; get; }
        public ushort Monster2 { set; get; }

        public int RPool { set; get; }
        public int OPool { set; get; }

        public ushort Supporter { set; get; }
        public ushort Hinder { set; get; }

        public ushort Eve1 { set; get; }

        public ZeroField(XIClient xic) : base(xic)
        {
            Monster1 = 0; Monster2 = 0;
            RPool = 0; OPool = 0;
            Supporter = 0; Hinder = 0;
            Eve1 = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Aps(sb, "***************");
            if (Eve1 > 0)
                Aps(sb, "本回合响应事件牌：{0}", xic.zd.Eve(Eve1));
            Aps(sb, "触发方灵力池：{0} 妨碍方灵力池：{1}", RPool, OPool);
            Aps(sb, "支援者: {0} 妨碍者: {1}", (Supporter == 0 ? "无" : (Supporter < 1000 ?
                xic.zd.Player(Supporter) : xic.zd.Monster((ushort)(Supporter - 1000)))),
                (Hinder == 0 ? "无" : (Hinder < 1000 ? xic.zd.Player(Hinder) :
                xic.zd.Monster((ushort)(Hinder - 1000)))));
            Base.Card.NMB nmb1 = Base.Card.NMBLib.Decode(Monster1, xic.Tuple.ML, xic.Tuple.NL);
            Aps(sb, "当前第一怪物/NPC: {0}. 战力：{1} 闪避：{2}", xic.zd.Monster(Monster1),
                nmb1 == null ? 0 : nmb1.STR, nmb1 == null ? 0 : nmb1.AGL);
            Base.Card.NMB nmb2 = Base.Card.NMBLib.Decode(Monster2, xic.Tuple.ML, xic.Tuple.NL);
            Aps(sb, "当前第二怪物/NPC: {0}. 战力：{1} 闪避：{2}", xic.zd.Monster(Monster2),
                nmb2 == null ? 0 : nmb2.STR, nmb2 == null ? 0 : nmb2.AGL);
            Aps(sb, "***************");
            return sb.ToString();
        }
    }

    public class ZeroMe : ZeroBase
    {
        public int SelectHero { set; get; }

        public List<ushort> Tux { set; get; }
        public List<ushort> Folder { set; get; }

        public ZeroMe(XIClient xic): base(xic)
        {
            Tux = new List<ushort>();
            Folder = new List<ushort>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Aps(sb, "***************");
            Aps(sb, "您的手牌: {0}", xic.zd.Tux(Tux));
            if (Folder.Count > 0)
            {
                Aps(sb, "    {0}: {1}",
                 xic.zd.HeroFolderAlias(SelectHero), xic.zd.Tux(Tux));
            }
            Aps(sb, "***************");
            return sb.ToString();
        }
    }

    public class ZeroPlayer : ZeroBase
    {
        public string Name { private set; get; }
        public ushort Uid { set; get; }

        public int SelectHero { set; get; }
        public int Team { set; get; }

        public int HP { set; get; }
        public int HPa { set; get; }

        public int STR { set; get; }
        public int STRa { set; get; }

        public int DEX { set; get; }
        public int DEXa { set; get; }

        public bool IsLoved { set; get; }
        public bool IsAlive { set; get; }
        public bool Immobilized { set; get; }
        public bool PetDisabled { get; set; }

        public int TuxCount { set; get; }
        public ushort Weapon { set; get; }
        public ushort Armor { set; get; }
        public ushort Trove { set; get; }

        public ushort ExEquip { get; set; }
        public List<ushort> ExCards { set; get; }

        public ushort[] Pets { set; get; }
        public List<ushort> Escue { private set; get; }
        public IDictionary<ushort, string> Fakeq { get; set; }

        public IDictionary<ushort, List<string>> Treasures { private set; get; }
        public int Coss { set; get; }
        public ushort Guardian { set; get; }

        public int Token { set; get; }
        public List<string> SpecialCards { private set; get; }
        public List<ushort> PlayerTars { private set; get; }
        public bool AwakeSignal { set; get; }
        public int FolderCount { set; get; }

        public ZeroPlayer(string name, XIClient xic) : base(xic)
        {
            Name = name;
            Pets = new ushort[5];

            TuxCount = 0;
            Weapon = 0;
            Armor = 0;
            Trove = 0;
            ExCards = new List<ushort>();
            Escue = new List<ushort>();
            Fakeq = new Dictionary<ushort, string>();

            Treasures = new Dictionary<ushort, List<string>>();

            Token = 0;
            SpecialCards = new List<string>();
            PlayerTars = new List<ushort>();
            AwakeSignal = false;
            FolderCount = 0;

            Guardian = 0; Coss = 0;
        }

        public void ParseFromHeroLib()
        {
            Base.Card.Hero hero = xic.Tuple.HL.InstanceHero(SelectHero);
            if (hero != null)
            {
                Team = 4;
                HP = HPa = hero.HP;
                STR = STRa = hero.STR;
                DEX = DEXa = hero.DEX;
                IsAlive = true;
                IsLoved = false;
                Immobilized = false;
                PetDisabled = false;
            }
        }
        public void ClearStatus()
        {
            IsLoved = false;
            Immobilized = false;
            PetDisabled = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Aps(sb, "{0}", "***************");
            Aps(sb, "{0}", ToStringSingleMask());
            return sb.ToString();
        }

        public string ToStringSingleMask()
        {
            StringBuilder sb = new StringBuilder();
            Aps(sb, "{0}P: {1}({2}) {3}{4}{5}{6} {7}", Uid, xic.zd.Hero(SelectHero), Name,
                IsAlive ? "" : "已阵亡 ", IsLoved ? "已倾慕 " : "", Immobilized ? "定 " : "",
                PetDisabled ? "禁宠 " : "", Guardian != 0 ? xic.zd.GuardAlias(SelectHero, Coss) : "");
            Aps(sb, "手牌数:{0} HP:{1}/{2} STR:{3}/{4} DEX:{5}/{6}",
                TuxCount, HP, HPa, STR, STRa, DEX, DEXa);
            string equipBase = "装备: {0} {1}" + (((xic.LevelCode >> 1) >= 3) ? " {2}  " : "  ") +
                 ((ExCards.Count > 0 || ExEquip != 0) ? "{3}: {4}" : "");
            Aps(sb, equipBase, xic.zd.Tux(Weapon), xic.zd.Tux(Armor), xic.zd.Tux(Trove),
                xic.zd.HeroExCardAlias(SelectHero, Coss),
                (ExCards.Count > 0 ? xic.zd.Tux(ExCards) : xic.zd.Tux(ExEquip)));
            Aps(sb, "宠物: {0}", xic.zd.Monster(Pets.Where(p => p != 0)));
            if (Escue.Count > 0)
                Aps(sb, "可助战NPC：{0}", xic.zd.Monster(Escue));

            if (Trove != 0 && Treasures.ContainsKey(Trove) && Treasures[Trove].Count > 0)
                Aps(sb, "行囊中：{0}", xic.zd.MixedCards(Treasures[Trove]));

            string special = "";
            if (Token > 0)
                special += " {0}：{1}";
            if (SpecialCards.Count > 0)
                special += " {2}：{3}";
            if (PlayerTars.Count > 0)
                special += " {4}：{5}";
            if (AwakeSignal)
                special += " {6} 已发动.";
            if (FolderCount > 0)
                special += " {7}数：{8}";
            if (special != "")
                Aps(sb, special, xic.zd.HeroTokenAlias(SelectHero, Coss), Token,
                    xic.zd.HeroPeopleAlias(SelectHero, Coss), xic.zd.MixedCards(SpecialCards),
                    xic.zd.HeroPlayerTarAlias(SelectHero, Coss), xic.zd.Player(PlayerTars),
                    xic.zd.HeroAwakeAlias(SelectHero, Coss),
                    xic.zd.HeroFolderAlias(SelectHero, Coss), FolderCount);
            if (Fakeq.Count > 0)
                Aps(sb, "其它配饰：{0}", xic.zd.TuxAs(Fakeq));
            Aps(sb, "{0}", "***************");
            return sb.ToString();
        }
    }
}
