using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base
{
    public class Rune
    {
        public string Name { private set; get; }

        public string Code { private set; get; }

        public string Occur { private set; get; }
        public int Priority { private set; get; }
        public bool? IsLock { private set; get; }
        public bool IsOnce { private set; get; }
        public bool IsTermin{private set;get;}
        public bool IsConsume{private set;get;}
        public string Description { private set; get; }

        public delegate string InputDelegate(Player player, string fuse, string prev);
        private InputDelegate mInput;
        public InputDelegate Input
        {
            set { mInput = value; }
            get { return mInput ?? DefInput; }
        }

        public delegate void ActionDelegate(Player player, string fuse, string args);
        private ActionDelegate mAction;
        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? DefAction; }
        }

        public delegate bool ValidDelegate(Player player, string fuse);
        private ValidDelegate mValid;
        public ValidDelegate Valid
        {
            set { mValid = value; }
            get { return mValid ?? DefValid; }
        }

        public Rune(string name, string code, string occur, int priority, bool? isLock,
            bool isOnce, bool isTermin, bool isConsume, string desc)
        {
            Name = name; Code = code;
            Occur = occur; Priority = priority; IsLock = isLock;
            IsOnce = isOnce; IsTermin = isTermin; IsConsume = isConsume;
            Description = desc;
        }

        private static InputDelegate DefInput = delegate(Player p, string f, string pr) { return ""; };
        private static ActionDelegate DefAction = delegate(Player p, string f, string a) { };
        private static ValidDelegate DefValid = delegate(Player p, string f) { return true; };
    }

    public class RuneLib
    {
        public List<Rune> Firsts { private set; get; }

        //private IDictionary<string, Skill> dicts;
        private Utils.ReadonlySQL sql;

        public RuneLib()
        {
            Firsts = new List<Rune>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "NAME", "OCCURS", "PRIORS", "ONCES", "TERMINS", "CONSUME", "DESC"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Rune");
            foreach (System.Data.DataRow data in datas)
            {
                string code = (string)data["CODE"];
                string name = (string)data["NAME"];
                string occur = (string)data["OCCURS"];
                bool? @lock = false;
                if (occur.StartsWith("!") || occur.StartsWith("?"))
                {
                    if (occur.StartsWith("!")) @lock = true;
                    else @lock = null;
                    occur = occur.Substring(1);
                }
                int prior = int.Parse((string)data["PRIORS"]);
                bool once = ((short)data["ONCES"] == 1);
                bool termin = ((short)data["TERMINS"] == 1);
                bool consume = ((short)data["CONSUME"] == 1);
                string desc = (string)data["DESC"];
                Firsts.Add(new Rune(name, code, occur, prior, @lock, once, termin, consume, desc));
            }
        }

        public int Size { get { return Firsts.Count; } }

        public Rune Encode(string code)
        {
            return Firsts.Find(p => p.Code == code);
        }
        public Rune Decode(ushort ut)
        {
            if (ut == 0 || ut > Firsts.Count)
                return null;
            return Firsts[ut - 1];
        }
        public ushort GetSingleIndex(Rune rune)
        {
            return (ushort)(Firsts.IndexOf(rune) + 1);
        }
        public ushort[] GetFullAppendableList()
        {
            return new ushort[] { 1, 2, 3, 4, 5, 6 };
        }
        public ushort[] GetFullPositive()
        {
            return new ushort[] { 1, 2, 3, 4 };
        }
        public ushort[] GetFullNegative()
        {
            return new ushort[] { 5, 6 };
        }
        public ushort[] GetFullAdvanced()
        {
            return new ushort[] { 7, 8 };
        }
    }
}
