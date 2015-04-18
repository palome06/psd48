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

        // public Delegate Type of Handling events
        public NPC(string code, int group, int genre, string name, ushort str, string[] skills, int hero)
        {
            this.Name = name; this.Code = code;
            this.Group = group; this.Genre = genre;
            this.STR = this.STRb = str; this.Skills = skills;
            this.Hero = hero;
        }

        public bool IsMonster() { return false; }
        public bool IsNPC() { return true; }
    }

    public class NPCLib
    {
        private List<NPC> firsts;

        private IDictionary<ushort, NPC> dicts;

        private Utils.ReadonlySQL sql;

        public NPCLib()
        {
            firsts = new List<NPC>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "VALID", "NAME", "STR", "ACTION", "ORG", "GENRE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Npc");
            foreach (System.Data.DataRow data in datas)
            {
                string code = (string)data["CODE"];
                int valid = (int)((short)data["VALID"]);
                int genre = (int)((long)data["GENRE"]);
                string name = (string)data["NAME"];
                ushort str = (ushort)((short)data["STR"]);
                string[] npcskills = ((string)data["ACTION"]).Split(',');
                int org = (int)((long)data["ORG"]);
                firsts.Add(new NPC(code, valid, genre, name, str, npcskills, org));
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
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return dicts.Keys.ToList();
            else
                return dicts.Where(p => pkgs.Contains(p.Value.Group)).Select(p => p.Key).ToList();
        }
    }
}
