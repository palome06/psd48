using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public class NPC : NMB
    {
        // Npc name, (e.g.) Xuanxiao
        public string Name { private set; get; }
        // avatar = serial, (e.g.) 20505
        public string Code { private set; get; }
        // group, e.g. 1 for standard, 0 for test, 2 for SP, etc.
        public int Group { private set; get; }
        // M or F
        public char Gender { private set; get; }
        public int Genre { private set; get; }

        private int mSTR;
        public int STR
        {
            set { mSTR = value; }
            get { return mSTR >= 0 ? mSTR : 0; }
        }
        public ushort STRb { private set; get; }
        public int AGL { get { return 0; } }

        public string[] Skills { private set; get; }
        // original hero code, (e.g.) 10505, and 0 if not exists
        public int Hero { private set; get; }

        public string DebutText { set; get; }
        public delegate void DebutDelegate(Player trigger);
        private DebutDelegate mDebut;
        public DebutDelegate Debut
        {
            set { mDebut = value; }
            get { return mDebut ?? DefDebut; }
        }

        private DebutDelegate DefDebut = (t) => { };

        public ushort ROMUshort { set; get; }
        // public Delegate Type of Handling events
        public NPC(string code, int group, int genre, string name, ushort str,
            string[] skills, int hero, char gender)
        {
            this.Name = name; this.Code = code;
            this.Group = group; this.Genre = genre;
            this.STR = this.STRb = str; this.Skills = skills;
            this.Hero = hero; this.Gender = gender;
            ROMUshort = 0;
        }

        public bool IsMonster() { return false; }
        public bool IsNPC() { return true; }

        /// <summary>
        /// Force Change Attribute of a hero, used mainly for capability for older version
        /// </summary>
        /// <param name="field">field name, caps sensetive</param>
        /// <param name="value">file value</param>
        public void ForceChange(string field, object value)
        {
            if (field == "Code" && value is string)
                Code = (string)value;
        }
    }

    public class NPCLib
    {
        private IDictionary<ushort, NPC> dicts;

        private Utils.ReadonlySQL sql;

        public NPCLib()
        {
            dicts = new Dictionary<ushort, NPC>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "ID", "CODE", "VALID", "NAME", "STR", "ACTION", "ORG", "GENDER",
                "DEBUTTEXT", "GENRE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Npc");
            foreach (System.Data.DataRow data in datas)
            {
                ushort id = (ushort)((long)data["ID"]);
                string code = (string)data["CODE"];
                int valid = (int)((short)data["VALID"]);
                int genre = (int)((long)data["GENRE"]);
                string name = (string)data["NAME"];
                ushort str = (ushort)((short)data["STR"]);
                string action = (string)data["ACTION"];
                string[] npcskills = !string.IsNullOrEmpty(action) ? action.Split(',') : new string[0];
                int org = (int)((long)data["ORG"]);
                char gender = ((string)data["GENDER"])[0];
                string debutText = data["DEBUTTEXT"] as string;
                dicts.Add(id, new NPC(code, valid, genre, name,
                    str, npcskills, org, gender) { DebutText = debutText ?? "" });
            }
        }

        public int Size { get { return dicts.Count; } }

        public List<NPC> First { get { return dicts.Values.ToList(); } }

        public NPC Decode(ushort code)
        {
            NPC npc;
            if (dicts.TryGetValue(code, out npc))
                return npc;
            else return null;
        }
        public ushort Encode(string code)
        {
            var any = dicts.Where(p => p.Value.Code.Equals(code));
            if (any.Count() == 1)
                return any.Select(p => p.Key).Single();
            else
                return 0;
        }

        public List<ushort> ListAllSeleable(int groups)
        {
            List<ushort> all = ListAllNPC(groups);
            string[] pair = { "NCR01", "NCT12", "NCR02", "NC302", "NCR03", "NC202", "NCR04", "N3W01",
                "NCR05", "NCT04" };
            for (int i = 0; i < pair.Length; i += 2)
            {
                if (all.Any(p => dicts[p].Code == pair[i]))
                    all.RemoveAll(p => dicts[p].Code == pair[i + 1]);
            }
            return all;
        }
        public List<ushort> ListAllNPC(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return dicts.Keys.ToList();
            else
                return dicts.Where(p => pkgs.Contains(p.Value.Group)).Select(p => p.Key).ToList();
        }
    }
}
