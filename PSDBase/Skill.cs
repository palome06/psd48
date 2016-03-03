using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base
{
    public class Skill
    {
        public string Name { private set; get; }

        public string Code { private set; get; }

        public string[] Occurs { private set; get; }

        public int[] Priorities { private set; get; }

        public bool[] IsOnce { private set; get; }

        public bool[] IsTermini { private set; get; }

        public bool?[] Lock { private set; get; }

        public bool[] IsHind { private set; get; }

        public bool IsChange { set; get; }
        public bool IsRestrict { set; get; }
        // whether linked, to tell whether a host message is attached to fuse in Calling
        //public bool IsLinked { set; get; }

        public string[] Parasitism { private set; get; }

        public string Descripe { set; get; }

        public Skill(string name, string code, string occurStr, string priortyStr,
            string isOnceStr, string isHindStr, string parasitismStr, string terminiStr)
        {
            this.Name = name; this.Code = code;

            string[] occurs = occurStr.Split(',');
            string[] priorties = priortyStr.Split(',');
            string[] onces = isOnceStr.Split(',');
            int sz = occurs.Length;
            Occurs = new string[sz]; Priorities = new int[sz];
            IsOnce = new bool[sz]; Lock = new bool?[sz];
            for (int i = 0; i < sz; ++i)
            {
                if (occurs[i].StartsWith("!"))
                {
                    Occurs[i] = occurs[i].Substring(1);
                    Lock[i] = true;
                }
                else if (occurs[i].StartsWith("?"))
                {
                    Occurs[i] = occurs[i].Substring(1);
                    Lock[i] = null;
                }
                else
                {
                    Occurs[i] = occurs[i];
                    Lock[i] = false;
                }
                Priorities[i] = int.Parse(priorties[i]);
                IsOnce[i] = (onces[i] == "1") ? true : false;
            }
            if (!string.IsNullOrEmpty(parasitismStr))
                Parasitism = parasitismStr.Split('&');
            else
                Parasitism = new string[0];
            if (!string.IsNullOrEmpty(isHindStr))
                IsHind = isHindStr.Split(',').Select(p => (p == "1")).ToArray();
            else
            {
                IsHind = new bool[sz];
                for (int i = 0; i < sz; ++i)
                    IsHind[i] = false;
            }
            if (!string.IsNullOrEmpty(terminiStr))
                IsTermini = terminiStr.Split(',').Select(p => (p == "1")).ToArray();
            else
            {
                IsTermini = new bool[sz];
                for (int i = 0; i < sz; ++i)
                    IsTermini[i] = false;
            }
            //IsLinked = isLinked;
        }

        public virtual bool IsBK { get { return false; } }

        public delegate void ActionDelegate(Player player, int type, string fuse, string argst);

        public delegate bool ValidDelegate(Player player, int type, string fuse);

        public delegate string InputDelegate(Player player, int type, string fuse, string prev);

        public delegate string EncryptDelegate(string args);

        private ActionDelegate mAction;

        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? DefAction; }
        }

        private InputDelegate mInput;

        public InputDelegate Input
        {
            set { mInput = value; }
            get { return mInput ?? DefInput; }
        }

        private ValidDelegate mValid;

        public ValidDelegate Valid
        {
            set { mValid = value; }
            get { return mValid ?? DefValid; }
        }

        private EncryptDelegate mEncrypt;

        public EncryptDelegate Encrypt
        {
            set { mEncrypt = value; }
            get { return mEncrypt ?? DefEncrypt; }
        }

        private static ActionDelegate DefAction =
            delegate(Player player, int type, string fuse, string argst) { };
        private static ValidDelegate DefValid =
            delegate(Player player, int type, string fuse) { return true; };
        private static InputDelegate DefInput =
            delegate(Player player, int type, string fuse, string prev) { return ""; };
        private static EncryptDelegate DefEncrypt =
            delegate(string args) { return args; };

        public bool IsLinked(int inType)
        {
            return Occurs != null && Occurs.Length > inType &&
                Occurs[inType].Contains('%');
        }
        /// <summary>
        /// Force Change Attribute of a skill, used mainly for capability for older version
        /// </summary>
        /// <param name="field">field name, caps sensetive</param>
        /// <param name="value">file value</param>
        public void ForceChange(string field, object value)
        {
            if (field == "Occurs" && value is string[])
                Occurs = value as string[];
            else if (field == "Priorities" && value is int[])
                Priorities = value as int[];
            else if (field == "IsOnce" && value is bool[])
                IsOnce = value as bool[];
            else if (field == "IsTermini" && value is bool[])
                IsTermini = value as bool[];
            else if (field == "Lock" && value is bool?[])
                Lock = value as bool?[];
            else if (field == "IsHind" && value is bool[])
                IsHind = value as bool[];
            else if (field == "Name" && value is string)
                Name = value as string;
            else if (field == "Code" && value is string)
                Code = value as string;
        }
    }

    public class Bless : Skill
    {
        public override bool IsBK { get { return true; } }

        public delegate bool BKValidDelegate(Player player, int type, string fuse, ushort owner);

        private BKValidDelegate mBKValid;

        public BKValidDelegate BKValid
        {
            set { mBKValid = value; }
            get { return mBKValid ?? DefBKValid; }
        }

        public Bless(string name, string code, string occurStr, string priortyStr,
            string isOnceStr, string isHindStr, string parasitismStr, string terminiStr)
            : base(name, code, occurStr, priortyStr, isOnceStr, isHindStr, parasitismStr, terminiStr) { }

        private static BKValidDelegate DefBKValid =
            delegate(Player p, int t, string f, ushort o) { return true; };
    }

    public class SkillLib
    {
        public List<Skill> Firsts { private set; get; }

        //private IDictionary<string, Skill> dicts;
        private Utils.ReadonlySQL sql;

        public SkillLib(string path)
        {
            Firsts = new List<Skill>();
            //dicts = new Dictionary<string, Skill>();
            string[] lines = System.IO.File.ReadAllLines("..\\..\\data\\SkillDict.txt");
            foreach (string line in lines)
            {
                if (line != null && line.Length > 0 && !line.StartsWith("#"))
                {
                    bool isBless = false;
                    string[] content = line.Split('\t');
                    string code = content[0]; // code, e.g. (JN10102)
                    if (code.StartsWith("o"))
                    {
                        code = code.Substring(1);
                        isBless = true;
                    }
                    string name = content[1]; // name, e.g. (Feilongtanyunshou)
                    string occurs = content[2];
                    string priorites = content[3];
                    string onces = content[4];
                    string parasitismStr = content[5];
                    string terminiStr = content[6];
                    if (isBless)
                    {
                        var skill = new Bless(name, code, occurs,
                            priorites, onces, null, parasitismStr, terminiStr);
                        Firsts.Add(skill);
                        //dicts.Add(code, skill);
                    }
                    else
                    {
                        var skill = new Skill(name, code, occurs,
                            priorites, onces, null, parasitismStr, terminiStr);
                        Firsts.Add(skill);
                    }
                }
            }
        }

        public SkillLib()
        {
            Firsts = new List<Skill>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "TYPE", "NAME", "OCCURS", "PRIORS", "ONCE",
                "PARASITISM", "DESCRIPE", "HIND", "TERMINI"
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
                    string onces = (string)data["ONCE"];
                    //string parasitismStr = data["PARASITISM"] == DBNull.Value ? "" : (string)data["PARASITISM"];
                    string parasitismStr = (string)data["PARASITISM"];
                    string descripe = data["DESCRIPE"] as string;
                    string hinds = data["HIND"] as string;
                    string terminiStr = data["TERMINI"] as string;

                    bool isChange = type.Contains("C");
                    bool isBless = type.Contains("B");
                    bool isRestrict = type.Contains("R");
                    if (isBless)
                        Firsts.Add(
                            new Bless(name, code, occurs, priors, onces, hinds,
                                parasitismStr, terminiStr)
                            {
                                Descripe = descripe, IsChange = isChange, IsRestrict = isRestrict
                            });
                    else
                        Firsts.Add(
                            new Skill(name, code, occurs, priors, onces, hinds,
                                parasitismStr, terminiStr)
                            {
                                Descripe = descripe, IsChange = isChange, IsRestrict = isRestrict
                            });
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
