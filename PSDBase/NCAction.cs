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

        public delegate void ActionDelegate(Player player, string fuse, string argst);

        public delegate bool ValidDelegate(Player player, string fuse);

        public delegate string InputDelegate(Player player, string fuse, string prev);

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

        public NCAction(string name, string code, string intro)
        {
            Name = name; Code = code; Intro = intro;
        }

        private static ActionDelegate DefAction = delegate(
            Player player, string fuse, string argst) { };
        private static ValidDelegate DefValid = delegate(
            Player player, string fuse) { return true; };
        private static InputDelegate DefInput = delegate(
            Player player, string fuse, string prev) { return ""; };
    }

    public class NCActionLib
    {
        public List<NCAction> Firsts { private set; get; }

        //private IDictionary<string, Skill> dicts;

        private Utils.ReadonlySQL sql;

        public NCActionLib(string path)
        {
            Firsts = new List<NCAction>();
            //dicts = new Dictionary<string, Skill>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line != null && line.Length > 0 && !line.StartsWith("#"))
                {
                    string[] content = line.Split('\t');
                    string code = content[0];
                    string name = content[1];
                    string intro = content[2];

                    NCAction nj = new NCAction(name, code, intro);
                    Firsts.Add(nj);
                }
            }
        }

        public NCActionLib()
        {
            Firsts = new List<NCAction>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] { "CODE", "NAME", "INTRO" }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "NJ");
            foreach (System.Data.DataRow data in datas)
            {
                string code = (string)data["CODE"];
                string name = (string)data["NAME"];
                string intro = (string)data["INTRO"];
                Firsts.Add(new NCAction(name, code, intro));
            }
        }

        public int Size { get { return Firsts.Count; } }

        public NCAction EncodeNCAction(string code)
        {
            foreach (NCAction nj in Firsts)
                if (nj.Code == code)
                    return nj;
            return null;
        }
    }
}