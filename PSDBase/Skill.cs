using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSD.Base.Flow;

namespace PSD.Base
{
    public class Skill
    {
        public string Name { private set; get; }

        public string Code { private set; get; }

        public SKBranch[] Branches { private set; get; }
        // host message is attached to fuse in Calling
        public string[] Parasitism { private set; get; }

        public string Descripe { private set; get; }
        // whether the skill is to change the hero
        public bool IsChange { private set; get; }

        public Skill(string name, string code, string skillType, string occurStr,
            string priortyStr, string mixCodeStr, string parasitismStr, string desc)
        {
            Name = name; Code = code;
            Branches = SKBranch.ParseFromStrings(occurStr, priortyStr, mixedCodeStr);
            Parasitism = string.IsNullOrEmpty(parasitismStr) ? new string[0] : parasitismStr.Split('&');
            IsChange = skillType.Contains("C");
        }

        public virtual bool IsBK { get { return false; } }

        public delegate void ActionDelegate(Player player, int type, Fuse fuse, string argst);

        public delegate bool ValidDelegate(Player player, int type, Fuse fuse);

        public delegate string InputDelegate(Player player, int type, Fuse fuse, string prev);

        public delegate string EncryptDelegate(string args);

        private ActionDelegate mAction;
        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? ((p, t, f, a) => { }); }
        }

        private InputDelegate mInput;
        public InputDelegate Input
        {
            set { mInput = value; }
            get { return mInput ?? ((p, t, f, pr) => { return ""; }); }
        }

        private ValidDelegate mValid;
        public ValidDelegate Valid
        {
            set { mValid = value; }
            get { return mValid ?? ((p, t, f) => { return true; }); }
        }

        private EncryptDelegate mEncrypt;
        public EncryptDelegate Encrypt
        {
            set { mEncrypt = value; }
            get { return mEncrypt ?? ((a) => { return a; }); }
        }
        /// <summary>
        /// Force Change Attribute of a skill, used mainly for capability for older version
        /// </summary>
        /// <param name="field">field name, caps sensetive</param>
        /// <param name="value">file value</param>
        public void ForceChange(string field, object value)
        {
            if (field == "Branches" && value is SKBranch[])
                Branches = value as SKBranch[];
            else if (field == "Parasitism" && value is string[])
                Parasitism = value as string[];
        }
    }

    public class Bless : Skill
    {
        public override bool IsBK { get { return true; } }

        public delegate bool BKValidDelegate(Player player, int type, Fuse fuse, ushort owner);

        private BKValidDelegate mBKValid;

        public BKValidDelegate BKValid
        {
            set { mBKValid = value; }
            get { return mBKValid ?? ((p, t, f, o) => { return true; }); }
        }

        public Bless(string name, string code, string skillType, string occurStr,
            string priortyStr, string mixCodeStr, string parasitismStr, string desc)
            : base(name, code, skillType, occurStr, priortyStr, mixCodeStr, parasitismStr, desc) { }
    }

    public class SkillLib
    {
        public List<Skill> Firsts { private set; get; }

        private Utils.ReadonlySQL sql;

        public SkillLib()
        {
            Firsts = new List<Skill>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "TYPE", "NAME", "OCCURS", "PRIORS", "MIXCODES",
                "PARASITISM", "DESCRIPE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Skill");
            foreach (System.Data.DataRow data in datas)
            {
                string type = (string)data["TYPE"];
                if (type != "^")
                {
                    string code = (string)data["CODE"];
                    string name = (string)data["NAME"];
                    string occurs = (string)data["OCCURS"];
                    string priors = (string)data["PRIORS"];
                    string mixcodes = (string)data["MIXCODES"];
                    string para = (string)data["PARASITISM"];
                    string descripe = data["DESCRIPE"] as string;

                    bool isBless = type.Contains("B");
                    Firsts.Add(isBless ? new Bless(name, code, type, occurs, priors, mixcodes, para,
                         descripe) : new Skill(name, code, type, occurs, priors, mixcodes, para, descripe));
                }
            }
        }

        public int Size { get { return Firsts.Count; } }

        public Skill EncodeSkill(string code)
        {
            foreach (Skill sk in Firsts)
                if (sk.Code == code)
                    return sk;
            return null;
        }
    }
}
