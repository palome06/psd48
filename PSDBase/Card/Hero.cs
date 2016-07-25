using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.Base.Card
{
    public class Hero
    {
        // Game name, (e.g.) Han Lingsha
        public string Name { private set; get; }
        // avatar = serial, (e.g.) 10502
        public int Avatar { private set; get; }
        // group, e.g. 1 for standard, 0 for test, 2 for SP, etc.
        public int Group { private set; get; }
        public int Genre { private set; get; }
        // available day of tested hero
        public DayOfWeek[] AvailableDay { set; get; }
        // available test level, e.g. HL012 in Group 6 is tested in RCM Room 3
        public int AvailableTestPkg { set; get; }

        // HP, EP, CP; STR, DEF, ATS, ADF, SPD, DEX, AGL, MOV, RNG
        public ushort HP { private set; get; }

        public ushort STR { private set; get; }

        public ushort DEX { private set; get; }
        
        public List<string> Skills { private set; get; }
        // skills that hero could generated. e.g. JNT0701->[JNT0703] contained
        public List<string> RelatedSkills { set; get; }
        // spouses, (e.g.) {30501,!2}
        public List<string> Spouses { private set; get; }
        // isomorphic list, e.g. XJ304.Isomorphic = XJ303
        public List<int> Isomorphic { private set; get; }
        // private values
        private int archetype;
        // archetype, e.g. SP001.Archetype = XJ101
        public int Archetype
        {
            private set { archetype = value; }
            get { return archetype != 0 ? archetype : Antecessor; }
        }
        // antecessor, e.g. RM202.Antecessor = XJ202
        public int Antecessor { private set; get; }
        // pioneer, e.g. XJ202.Pioneer = RM202
        public int Pioneer { internal set; get; }

        public char Gender { private set; get; }
        public string Bio { private set; get; }

        public string Ofcode { get; set; }
        public string TokenAlias { set; get; }
        public string PeopleAlias { set; get; }
        public string PlayerTarAlias { set; get; }
        public string ExCardsAlias { set; get; }
        public string AwakeAlias { set; get; }
        public string FolderAlias { set; get; }
        public string GuestAlias { set; get; }

        public Hero(string name, int avatar, int group, int genre, char gender, ushort hp, ushort str, ushort dex,
            List<string> spouses, List<int> isomorphic, int archetype, int antecessor, List<string> skills, string bio)
        {
            this.Name = name; this.Avatar = avatar;
            this.Group = group; this.Genre = genre;
            this.Gender = gender;
            this.HP = hp; this.STR = str; this.DEX = dex;
            this.Spouses = spouses; this.Skills = skills;
            this.Isomorphic = isomorphic;
            this.Archetype = archetype;
            this.Antecessor = antecessor;
            this.Bio = bio;
        }
        internal void SetAvailableParam(string groupString)
        {
            List<DayOfWeek> list = new List<DayOfWeek>();
            int apkg = 0;
            string[] parts = groupString.Split(',');
            foreach (string part in parts)
            {
                switch (part)
                {
                    case "L1": list.Add(DayOfWeek.Monday); break;
                    case "L2": list.Add(DayOfWeek.Tuesday); break;
                    case "L3": list.Add(DayOfWeek.Wednesday); break;
                    case "L4": list.Add(DayOfWeek.Thursday); break;
                    case "L5": list.Add(DayOfWeek.Friday); break;
                    case "L6": list.Add(DayOfWeek.Saturday); break;
                    case "L7": list.Add(DayOfWeek.Sunday); break;
                    default: if (part.StartsWith("R")) apkg = int.Parse(part.Substring("R".Length)); break;
                }
            }
            AvailableDay = list.ToArray();
            AvailableTestPkg = apkg;
        }
        /// <summary>
        /// Force Change Attribute of a hero, used mainly for capability for older version
        /// </summary>
        /// <param name="field">field name, caps sensetive</param>
        /// <param name="value">file value</param>
        public void ForceChange(string field, object value)
        {
            if (field == "HP" && value is ushort)
                HP = (ushort)value;
            else if (field == "STR" && value is ushort)
                STR = (ushort)value;
            else if (field == "DEX" && value is ushort)
                DEX = (ushort)value;
            else if (field == "Avatar" && value is int)
                Avatar = (int)value;
            else if (field == "OfCode" && value is string)
                Ofcode = (string)value;
        }
    }

    public class HeroLib
    {
        //private List<Hero> firsts;
        private IDictionary<int, Hero> dicts;
        private Utils.ReadonlySQL sql;

        public HeroLib()
        {
            dicts = new Dictionary<int, Hero>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "ID", "GENRE", "VALID", "OFCODE", "NAME", "HP", "STR", "DEX",
                "GENDER", "SPOUSE", "ISO", "SKILL", "ALIAS", "BIO"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Hero");
            foreach (System.Data.DataRow data in datas)
            {
                string gs = (string)data["VALID"];
                int group = int.Parse(gs.Contains(",") ? gs.Substring(0, gs.IndexOf(',')) : gs);

                int genre = (int)((long)data["GENRE"]);
                int code = (int)((long)data["ID"]);
                string name = (string)data["NAME"];
                ushort hp = (ushort)((short)data["HP"]);
                ushort str = (ushort)((short)data["STR"]);
                ushort dex = (ushort)((short)data["DEX"]);
                char gender = ((string)data["GENDER"])[0];
                string spouses = (string)data["SPOUSE"];
                List<string> spouse = string.IsNullOrEmpty(spouses) ?
                    new List<string>() : spouses.Split(',').ToList();
                string isoStr = (string)data["ISO"];
                int archetype = 0, antecessor = 0;
                List<int> isos = new List<int>();
                foreach (string isosr in isoStr.Split(','))
                {
                    if (isosr.StartsWith("@"))
                        archetype = int.Parse(isosr.Substring("@".Length));
                    else if (isosr.StartsWith("^"))
                        antecessor = int.Parse(isosr.Substring("^".Length));
                    else if (!string.IsNullOrEmpty(isosr))
                        isos.Add(int.Parse(isosr));
                }
                string skills = (string)data["SKILL"];
                List<string> skill, relatedSkill;
                if (string.IsNullOrEmpty(skills))
                {
                    skill = new List<string>();
                    relatedSkill = new List<string>();
                }
                else if (skills.IndexOf('|') < 0)
                {
                    skill = skills.Split(',').ToList();
                    relatedSkill = new List<string>();
                }
                else
                {
                    int idx = skills.IndexOf('|');
                    skill = skills.Substring(0, idx).Split(',').ToList();
                    relatedSkill = skills.Substring(idx + 1).Split(',').ToList();
                }
                string[] aliass = (data["ALIAS"] as string ?? "").Split(',');
                string[] alias = new string[7];
                for (int i = 0; i < aliass.Length; i += 2)
                {
                    switch (aliass[i])
                    {
                        case "K": alias[0] = aliass[i + 1]; break;
                        case "C": alias[1] = aliass[i + 1]; break;
                        case "T": alias[2] = aliass[i + 1]; break;
                        case "E": alias[3] = aliass[i + 1]; break;
                        case "A": alias[4] = aliass[i + 1]; break;
                        case "F": alias[5] = aliass[i + 1]; break;
                        case "V": alias[6] = aliass[i + 1]; break;
                    }
                }
                string bio = data["BIO"] as string ?? "";
                Hero hero = new Hero(name, code, group, genre, gender, hp, str, dex,
                    spouse, isos, archetype, antecessor, skill, bio)
                {
                    Ofcode = data["OFCODE"] as string,
                    TokenAlias = alias[0],
                    PeopleAlias = alias[1],
                    PlayerTarAlias = alias[2],
                    ExCardsAlias = alias[3],
                    AwakeAlias = alias[4],
                    FolderAlias = alias[5],
                    GuestAlias = alias[6],
                    RelatedSkills = relatedSkill
                };
                hero.SetAvailableParam(gs);
                dicts.Add(code, hero);
            }
            foreach (Hero hero in dicts.Values)
            {
                if (hero.Antecessor != 0 && dicts.ContainsKey(hero.Antecessor))
                    dicts[hero.Antecessor].Pioneer = hero.Avatar;
            }
        }

        public List<Hero> ListAllJoinableHeroes(int groups)
        {
            return ListAllHeros(groups).Where(p => p.Ofcode != "XJ103" &&
                p.Ofcode != "XJ207" && p.Ofcode != "XJ304" && p.Ofcode != "XJ507" &&
                p.Ofcode != "TR031" && p.Ofcode != "TR033" &&
                p.Ofcode != "HL005" && p.Ofcode != "HL015").ToList();
        }
        public List<Hero> ListAllSeleable(int groups)
        {
            List<Hero> first = ListAllJoinableHeroes(groups);
            string[] pair = { "XJ505", "TR011", "TR012", "R5Q05", "XJ302", "RM302", "XJ202", "RM202",
                "X3W01", "R3W01", "TR004", "RM509" };
            for (int i = 0; i < pair.Length; i += 2)
            {
                if (first.Any(p => p.Ofcode == pair[i + 1]))
                    first.RemoveAll(p => p.Ofcode == pair[i]);
            }
            return first;
        }
        public List<Hero> ListAllHeros(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs != null)
                return dicts.Values.Where(p => p.Group != 0 && pkgs.Contains(p.Group)).ToList();
            else
                return dicts.Values.Where(p => p.Group != 0).ToList();
        }
        public List<Hero> ListHeroesInTest(int level)
        {
            int[] lv = Card.Level2Pkg(level) ?? new int[0];
            return dicts.Values.Where(p => p.Group != 0 && lv.Contains(p.AvailableTestPkg)
                && p.AvailableDay.Contains(DateTime.Now.DayOfWeek)).ToList();
        }
        public List<Hero> PurgeHeroesWithGivenTrainer(int level, string[] codes)
        {
            if (codes == null)
                return new List<Hero>();
            List<Hero> list = ListAllSeleable(level).Where(p => codes.Contains(p.Ofcode)).ToList();
            list.AddRange(ListHeroesInTest(level).Where(p => codes.Contains(p.Ofcode)));
            if (list.Count > 6)
                list = list.Take(6).ToList();
            return list;
        }

        public Hero InstanceHero(int code) {
            Hero hero = null;
            dicts.TryGetValue(code, out hero);
            return hero;
        }

        public void ForceChange(Hero hero, int newAvatar, string newCode)
        {
            if (hero != null)
            {
                dicts.Remove(hero.Avatar);
                dicts[newAvatar] = hero;
                hero.ForceChange("Avatar", newAvatar);
                hero.ForceChange("OfCode", newCode);
            }
        }
    }
}
