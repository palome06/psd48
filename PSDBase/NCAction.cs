using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base
{
    public class NCAction
    {
        public string Name { set; get; }

        public string Code { set; get; }

        public string Intro { set; get; }

        public SKBranch[] Branches { private set; get; }

        private ActionDelegate mAction;
        private EscueActionDelegate mEscueAction;
        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? DefAction; }
        }
        public EscueActionDelegate EscueAction
        {
            set { mEscueAction = value; }
            get { return mEscueAction ?? DefEscueAction; }
        }

        private InputDelegate mInput;
        private EscueInputDelegate mEscueInput;
        public InputDelegate Input
        {
            set { mInput = value; }
            get { return mInput ?? DefInput; }
        }
        public EscueInputDelegate EscueInput
        {
            set { mEscueInput = value; }
            get { return mEscueInput ?? DefEscueInput; }
        }

        private ValidDelegate mValid;
        private EscueValidDelegate mEscueValid;
        public ValidDelegate Valid
        {
            set { mValid = value; }
            get { return mValid ?? DefValid; }
        }
        public EscueValidDelegate EscueValid
        {
            set { mEscueValid = value; }
            get { return mEscueValid ?? DefEscueValid; }
        }

        public NCAction(string name, string code, string intro, string escue)
        {
            Name = name; Code = code; Intro = intro;
            Branches = SKBranch.ParseFromString(escue);
        }

        public delegate void ActionDelegate(Player player, string fuse, string argst);
        public delegate bool ValidDelegate(Player player, string fuse);
        public delegate string InputDelegate(Player player, string fuse, string prev);
        public delegate void EscueActionDelegate(Player player, ushort npcUt, int type, string fuse, string argst);
        public delegate bool EscueValidDelegate(Player player, ushort npcUt, int type, string fuse);
        public delegate string EscueInputDelegate(Player player, ushort npcUt, int type, string fuse, string prev);
        private static ActionDelegate DefAction = (p, f ,a) => { };
        private static ValidDelegate DefValid = (p, f) => { return true; };
        private static InputDelegate DefInput = (p, f, pv) => { return ""; };
        private static EscueActionDelegate DefEscueAction = (p, u, t, f, a) => { };
        private static EscueValidDelegate DefEscueValid = (p, u, t, f) => { return true; };
        private static EscueInputDelegate DefEscueInput = (p, u, t, f, pv) => { return ""; };
    }

    public class NCActionLib
    {
        public List<NCAction> Firsts { private set; get; }

        //private IDictionary<string, Skill> dicts;

        private Utils.ReadonlySQL sql;

        public NCActionLib()
        {
            Firsts = new List<NCAction>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] { "CODE", "NAME", "INTRO", "ESCUE" }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "NJ");
            foreach (System.Data.DataRow data in datas)
            {
                string code = (string)data["CODE"];
                string name = (string)data["NAME"];
                string intro = (string)data["INTRO"];
                string escue = (string)data["ESCUE"];
                Firsts.Add(new NCAction(name, code, intro, escue));
            }
        }

        public int Size { get { return Firsts.Count; } }

        public NCAction EncodeNCAction(string code)
        {
            return Firsts.FirstOrDefault(p => p.Code == code);
        }
    }
}