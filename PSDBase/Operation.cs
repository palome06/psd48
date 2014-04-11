using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base
{
    public class Operation
    {
        public string Name { private set; get; }

        public string Code { private set; get; }

        public string Occur { private set; get; }

        public bool IsOnce { private set; get; }

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

        public Operation(string name, string code, string occur, bool isOnce)
        {
            Name = name; Code = code;
            Occur = occur; IsOnce = isOnce;
        }

        private static InputDelegate DefInput = delegate(Player p, string f, string pr) { return ""; };
        private static ActionDelegate DefAction = delegate(Player p, string f, string a) { };
        private static ValidDelegate DefValid = delegate(Player p, string f) { return true; };
    }

    public class OperationLib
    {
        public List<Operation> Firsts { private set; get; }

        //private IDictionary<string, Skill> dicts;
        private Utils.ReadonlySQL sql;

        public OperationLib(string path)
        {
            Firsts = new List<Operation>();
            //dicts = new Dictionary<string, Skill>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (line != null && line.Length > 0 && !line.StartsWith("#"))
                {
                    string[] content = line.Split('\t');
                    string code = content[0]; // code, e.g. (JN10102)
                    string name = content[1]; // name, e.g. (Feilongtanyunshou)
                    string occur = content[2];
                    bool once = content[3] == "1" ? true : false;
                    Firsts.Add(new Operation(name, code, occur, once));
                }
            }
        }

        public OperationLib()
        {
            Firsts = new List<Operation>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "NAME", "OCCUR", "ISONCE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Ops");
            foreach (System.Data.DataRow data in datas)
            {
                string code = (string)data["CODE"];
                string name = (string)data["NAME"];
                string occur = (string)data["OCCUR"];
                bool once = ((short)data["ISONCE"] == 1);
                Firsts.Add(new Operation(name, code, occur, once));
            }
        }

        public int Size { get { return Firsts.Count; } }

        public Operation EncodeOps(string code)
        {
            foreach (Operation op in Firsts)
                if (op.Code == code)
                    return op;
            return null;
        }
    }
}
