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

        public Hero(string name, int avatar, int group, char gender, ushort hp, ushort str, ushort dex,
            List<string> spouses, List<int> isomorphic, int archetype, List<string> skills, string bio)
        {
            this.Name = name; this.Avatar = avatar; this.Group = group;
            this.Gender = gender;
            this.HP = hp; this.STR = str; this.DEX = dex;
            this.Spouses = spouses; this.Skills = skills;
            this.Isomorphic = isomorphic; this.Archetype = archetype;
            this.Bio = bio;
        }

        internal static Hero Parse(string line)
        {
            if (line != null && line.Length > 0 && !line.StartsWith("#"))
            {
                string[] content = line.Split(new char[] { '\t' });
                int code = int.Parse(content[0]); // code, e.g. (01004)
                string name = content[1]; // name, e.g. (Mugongxia)
                ushort hp = ushort.Parse(content[2]);
                ushort str = ushort.Parse(content[3]);
                ushort dex = ushort.Parse(content[4]);
                char gender = content[5][0];
                string spousesStr = content[6];
                List<string> spouses = spousesStr.Equals("^") ?
                    new List<string>() : spousesStr.Split(',').ToList();
                string isoStr = content[7];
                int archetype = 0;
                List<int> isos = new List<int>();
                foreach (string isosr in isoStr.Split(','))
                {
                    if (isosr.StartsWith("@"))
                        archetype = int.Parse(isosr.Substring("@".Length));
                    else if (!string.IsNullOrEmpty(isosr))
                        isos.Add(int.Parse(isosr));
                }
                string skillStr = content[8];
                List<string> skills = skillStr.Equals("^") ?
                    new List<string>() : skillStr.Split(',').ToList();
                string bio = content[9];
                return new Hero(name, code, 1, gender, hp, str, dex, spouses, isos, archetype, skills, bio)
                {
                    Ofcode = "XJ" + (code - 10000)
                };
            }
            else
                return null;
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

        public HeroLib(string path)
        {
            dicts = new Dictionary<int, Hero>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                Hero hero = Hero.Parse(line);
                if (hero != null)
                    dicts.Add(hero.Avatar, hero);
            }
        }

        public HeroLib()
        {
            dicts = new Dictionary<int, Hero>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "ID", "VALID", "OFCODE", "NAME", "HP", "STR", "DEX", "GENDER", "SPOUSE",
                "ISO", "SKILL", "ALIAS", "BIO"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Hero");
            foreach (System.Data.DataRow data in datas)
            {
                int group = (int)((long)data["VALID"]);
                if (group != 0)
                {
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
                    Hero hero = new Hero(name, code, group, gender, hp, str, dex, spouse, isos, archetype, skill, bio)
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
                    dicts.Add(code, hero);
                }
            }
        }

        public int Size { get { return dicts.Count; } }

        public Hero[,] RMPickList(int cand, int people, int groups)
        {
            int possi = dicts.Count / people;
            if (possi < cand)
                cand = possi;
            int total = cand * people;
            // TODO: Remove others from HeroLib
            List<Hero> possibles = ListAllSeleable(groups).ToList();
            IEnumerable<Hero> selects = Card.PickSomeInRandomOrder(possibles, total);
            int idx = 0, jdx = 0;
            Hero[,] heros = new Hero[people, cand];
            foreach (Hero hero in selects)
            {
                heros[idx, jdx] = hero;
                ++jdx;
                if (jdx >= cand) { ++idx; jdx = 0; }
            }
            return heros;
        }
        public Hero[] RMPickList(int sz, int groups)
        {
            IEnumerable<Hero> selects = Card.PickSomeInRandomOrder(
                ListAllSeleable(groups).ToList(), sz);
            return selects.ToArray();
        }
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
            if (groups == 0)
                return dicts.Values.ToList();
            else
                return dicts.Values.Where(p => ((groups & (1 << (p.Group - 1))) != 0)).ToList();
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

        public Hero InstanceHero(int code) {
            Hero hero = null;
            dicts.TryGetValue(code, out hero);
            return hero;
        }

        public static void GenerateSQ3(string path)
        {
            List<Hero> heros = new List<Hero>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                Hero hero = Hero.Parse(line);
                if (hero != null)
                    heros.Add(hero);
            }
            Utils.ReadonlySQL sql = new Utils.ReadonlySQL("psd.db3");
            foreach (Hero hero in heros)
            {
                Dictionary<string, int> dc = new Dictionary<string, int>();
                dc.Add("ID", hero.Avatar);
                dc.Add("HP", hero.HP);
                dc.Add("STR", hero.STR);
                dc.Add("DEX", hero.DEX);

                Dictionary<string, string> ds = new Dictionary<string, string>();
                ds.Add("VALID", "");
                ds.Add("NAME", hero.Name);
                ds.Add("GENDER", hero.Gender.ToString());
                ds.Add("SPOUSE", string.Join(",", hero.Spouses));
                ds.Add("ISO", string.Join(",", hero.Isomorphic));
                ds.Add("SKILL", string.Join(",", hero.Skills));

                sql.Insert(dc, ds, "Hero");
            }
        }
    }
}
