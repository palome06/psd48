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

        public List<string> Skills{private set;get;}
        public IDictionary<string, string> Description{private set;get;}

        private ExspLib lib;

        internal Exsp(string name, string code, List<string> skills,
            IDictionary<string, string> description, ExspLib lib)
        {
            this.Name = name; this.Code = code;
            this.lib = lib;
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
                "NAME", "CODE", "SKILL", "DESC"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Exsp");
            foreach (System.Data.DataRow data in datas)
            {
                //short type = (ushort)((short)data["TYPE"]);
                string name = (string)data["NAME"];
                string code = (string)data["CODE"];
                string skillstr = (string)data["SKILL"];
                List<string> skill = string.IsNullOrEmpty(skillstr) ?
                        new List<string>() : skillstr.Split(',').ToList();
                string descstr = (string)data["DESC"];
                IDictionary<string, string> id = new Dictionary<string, string>();
                string[] descSpt = string.IsNullOrEmpty(descstr) ?
                        new string[] { } : descstr.Split('|');
                for (int i = 1; i < descSpt.Length; i += 2)
                    id.Add(descSpt[i], descSpt[i + 1]);
                firsts.Add(new Exsp(name, code, skill, id, this));
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
    }
}
