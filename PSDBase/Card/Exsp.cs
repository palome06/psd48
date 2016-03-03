using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.Base.Card
{
    public class Exsp
    {
        // Exsp name, (e.g.) Leiling 
        public string Name { private set; get; }
        // Code, (e.g.) TRXJ608
        public string Code { private set; get; }
        // type of Exsp: 0-General, 1-Target, 2-Token, 3-ICard, 4-Mark
        public ushort Type { private set; get; }
        // Hero holding the Exsp, (e.g.) TR004
        public int Hero { private set; get; }

        public List<string> Skills { private set; get; }
        public IDictionary<string, string> Description { private set; get; }

        internal Exsp(string name, ushort type, int hero, string code,
            List<string> skills, IDictionary<string, string> description)
        {
            Name = name; Code = code;
            Type = type; Hero = hero;
            Skills = skills;
            Description = description;
        }
    }

    public class ExspLib
    {
        private List<Exsp> firsts;
        private IDictionary<string, Exsp> dicts;

        private Utils.ReadonlySQL sql;

        public ExspLib()
        {
            firsts = new List<Exsp>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "TYPE", "NAME", "HERO", "CODE", "SKILL", "DESC"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Exsp");
            foreach (System.Data.DataRow data in datas)
            {
                ushort type = (ushort)((short)data["TYPE"]);
                string name = (string)data["NAME"];
                //string heroStr = (string)data["HERO"];
                //int hero = string.IsNullOrEmpty(heroStr) ? 0 : int.Parse(heroStr);
                int hero = (int)((long)data["HERO"]);
                string codeGroup = (string)data["CODE"];
                string skillstr = (string)data["SKILL"];
                List<string> skill = string.IsNullOrEmpty(skillstr) ?
                        new List<string>() : skillstr.Split(',').ToList();
                string descstr = (string)data["DESC"];
                IDictionary<string, string> id = new Dictionary<string, string>();
                string[] descSpt = string.IsNullOrEmpty(descstr) ?
                        new string[] { } : descstr.Split('|');
                for (int i = 1; i < descSpt.Length; i += 2)
                    id.Add(descSpt[i], descSpt[i + 1]);
                string[] codes = codeGroup.Split(',');
                foreach (string code in codes)
                    firsts.Add(new Exsp(name, type, hero, code, skill, id));
            }
            dicts = new Dictionary<string, Exsp>();
            foreach (Exsp exsp in firsts)
                dicts.Add(exsp.Code, exsp);
        }

        public Exsp Encode(string code)
        {
            Exsp exsp;
            if (dicts.TryGetValue(code, out exsp))
                return exsp;
            else return null;
        }

        public List<Exsp> Firsts
        {
            get { return dicts.Values.ToList(); }
        }
    }
}
