using PSD.Base;
using PSD.Base.Flow;
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

        public delegate void ActionDelegate(Player player, Fuse fuse, string argst);

        public delegate bool ValidDelegate(Player player, Fuse fuse);

        public delegate string InputDelegate(Player player, Fuse fuse, string prev);

        private ActionDelegate mAction;
        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? ((p, f, a) => { }); }
        }

        private InputDelegate mInput;
        public InputDelegate Input
        {
            set { mInput = value; }
            get { return mInput ?? ((p, f, pr) => { return ""; }); }
        }

        private ValidDelegate mValid;
        public ValidDelegate Valid
        {
            set { mValid = value; }
            get { return mValid ?? ((p, f) => { return true; }); }
        }

        public NCAction(string name, string code, string intro)
        {
            Name = name; Code = code; Intro = intro;
        }
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