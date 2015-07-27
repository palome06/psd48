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
        // spouses, (e.g.) {30501,!2}
        public List<string> Spouses { private set; get; }
        public List<int> Isomorphic { private set; get; }
        public int Archetype { private set; get; }

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
            List<string> spouses, List<int> isomorphic, int archetype, List<string> skills, string bio)
        {
            this.Name = name; this.Avatar = avatar;
            this.Group = group; this.Genre = genre;
            this.Gender = gender;
            this.HP = hp; this.STR = str; this.DEX = dex;
            this.Spouses = spouses; this.Skills = skills;
            this.Isomorphic = isomorphic; this.Archetype = archetype;
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
                if (group != 0)
                {
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
                    int archetype = 0;
                    List<int> isos = new List<int>();
                    foreach (string isosr in isoStr.Split(','))
                    {
                        if (isosr.StartsWith("@"))
                            archetype = int.Parse(isosr.Substring("@".Length));
                        else if (!string.IsNullOrEmpty(isosr))
                            isos.Add(int.Parse(isosr));
                    }
                    string skills = (string)data["SKILL"];
                    List<string> skill = string.IsNullOrEmpty(skills) ?
                        new List<string>() : skills.Split(',').ToList();
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
                    Hero hero = new Hero(name, code, group, genre, gender, hp, str, dex, spouse, isos, archetype, skill, bio)
                    {
                        Ofcode = data["OFCODE"] as string,
                        TokenAlias = alias[0],
                        PeopleAlias = alias[1],
                        PlayerTarAlias = alias[2],
                        ExCardsAlias = alias[3],
                        AwakeAlias = alias[4],
                        FolderAlias = alias[5],
                        GuestAlias = alias[6]
                    };
                    hero.SetAvailableParam(gs);
                    dicts.Add(code, hero);
                }
            }
        }

        public int Size { get { return dicts.Count; } }

        public List<Hero> ListAllSeleable(int groups)
        {
            List<Hero> first = ListAllHeros(groups).Where(p => p.Ofcode != "XJ103" &&
                p.Ofcode != "XJ207" && p.Ofcode != "XJ304" && p.Ofcode != "XJ507" &&
                p.Ofcode != "HL005" && p.Ofcode != "HL015").ToList();
            if (first.Any(p => p.Ofcode == "XJ505") && first.Any(p => p.Ofcode == "TR011"))
                first.RemoveAll(p => p.Ofcode == "XJ505");
            return first;
        }
        public List<Hero> ListAllHeros(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs != null)
                return dicts.Values.Where(p => pkgs.Contains(p.Group)).ToList();
            else
                return dicts.Values.ToList();
        }
        //public Hero[,] SRPickList(int cand, int people)
        //{
        //    int possi = dicts.Count / people;
        //    if (possi < cand)
        //        cand = possi;
        //    int total = cand * people;
        //    List<Hero> possibles = dicts.Values.Where(p => p.Avatar != 10103 &&
        //        p.Avatar != 10207 && p.Avatar != 10304 && p.Avatar != 10607).ToList();
        //    IEnumerable<Hero> selects = possibles.Take(total);
        //    int idx = 0, jdx = 0;
        //    Hero[,] heros = new Hero[people, cand];
        //    foreach (Hero hero in selects)
        //    {
        //        heros[idx, jdx] = hero;
        //        ++jdx;
        //        if (jdx >= cand) { ++idx; jdx = 0; }
        //    }
        //    //heros[0, 0] = dicts[10505];
        //    return heros;
        //}
        public List<Hero> ListHeroesInTest(int level)
        {
            return dicts.Values.Where(p => p.AvailableTestPkg == (level >> 1)
                && p.AvailableDay.Contains(DateTime.Now.DayOfWeek)).ToList();
        }
        public List<Hero> PurgeHeroesWithGivenTrainer(int level, string[] codes)
        {
            if (codes == null)
                return new List<Hero>();
            if (codes.Length > 6)
                codes = codes.Take(6).ToArray();
            List<Hero> list = ListAllHeros(level).Where(p => codes.Contains(p.Ofcode)).ToList();
            list.AddRange(ListHeroesInTest(level).Where(p => codes.Contains(p.Ofcode)));
            return list;
        }

        public Hero InstanceHero(int code) {
            Hero hero = null;
            dicts.TryGetValue(code, out hero);
            return hero;
        }
    }
}
