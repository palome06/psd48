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

        // public Delegate Type of Handling events
        public NPC(string code, int group, string name, ushort str, string[] skills, int hero)
        {
            this.Name = name; this.Group = group; this.Code = code;
            this.STR = this.STRb = str; this.Skills = skills;
            this.Hero = hero;
        }

        public bool IsMonster() { return false; }
        public bool IsNPC() { return true; }

        internal static NPC Parse(string line)
        {
            if (line != null && line.Length > 0)
            {
                string[] content = line.Split('\t');
                string code = content[0]; // code, e.g. (NC405)
                int group = int.Parse(content[1]);
                string name = content[2]; // name, e.g. (Xuanxiao)
                ushort str = ushort.Parse(content[3]);
                string[] npcskills = content[4].Split(',');
                int hero = int.Parse(content[5]);
                return new NPC(code, group, name, str, npcskills, hero);
            }
            else
                return null;
        }
    }

    public class NPCLib
    {
        private List<NPC> firsts;

        private IDictionary<ushort, NPC> dicts;

        private Utils.ReadonlySQL sql;

        public NPCLib(string path)
        {
            firsts = new List<NPC>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                NPC npc = NPC.Parse(line);
                if (npc != null)
                    firsts.Add(npc);
            }
            ushort cardx = 1;
            dicts = new Dictionary<ushort, NPC>();
            foreach (NPC npc in firsts)
                dicts.Add(cardx++, npc);
        }

        public NPCLib()
        {
            firsts = new List<NPC>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "VALID", "NAME", "STR", "ACTION", "ORG"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Npc");
            foreach (System.Data.DataRow data in datas)
            {
                string code = (string)data["CODE"];
                int valid = (int)((short)data["VALID"]);
                string name = (string)data["NAME"];
                ushort str = (ushort)((short)data["STR"]);
                string[] npcskills = ((string)data["ACTION"]).Split(',');
                int org = (int)((long)data["ORG"]);
                firsts.Add(new NPC(code, valid, name, str, npcskills, org));
            }
            ushort cardx = 1;
            dicts = new Dictionary<ushort, NPC>();
            foreach (NPC npc in firsts)
                dicts.Add(cardx++, npc);
        }

        public int Size { get { return dicts.Count; } }

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
            if (groups == 0)
                return dicts.Keys.ToList();
            else
                return dicts.Where(p => ((groups & (1 << (p.Value.Group - 1))) != 0)).Select(p => p.Key).ToList();
        }
    }
}
