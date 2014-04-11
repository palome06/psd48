using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.Base.Card
{
    public class Evenement
    {
        // Evenment name, (e.g.) Xialingdao's Encounter
        public string Name { private set; get; }
        // avatar = serial, (e.g.) 30502
        //public int Avatar { private set; get; }
        public string Code { private set; get; }
        public int Count { private set; get; }

        public string Background { private set; get; }
        public string Description { private set; get; }
        // group, e.g. 1 for standard, 0 for test, 2 for SP, etc.
        public int Group { private set; get; }

        public string[] Occurs { set; get; }
        public int[] Priorties { set; get; }
        public bool[] IsOnce { set; get; }
        public bool[] IsTermini { set; get; }
        public bool[] Lock { set; get; }
        //public delegate void ActionDelegate(Player player, Board board);

        //public ActionDelegate Action { private set; get; }

        private int mSpi;
        public bool IsHarmInvolved() { return (mSpi & 0x1) != 0; }
        public bool IsTuxInvolved(bool self)
        {
            if ((mSpi & 0x2) != 0) return true;
            return (mSpi & 0x4) != 0;
        }

        internal Evenement(string name, string code, int count, int group,
            string background, string description, string spis)
        {
            this.Name = name; this.Code = code;
            this.Count = count; this.Background = background;
            this.Group = group;
            this.Description = description;
            //this.Action += new ActionDelegate();
            mSpi = 0;
            for (int i = 0; i < spis.Length; ++i)
            {
                if (spis[i] == 'H')
                    mSpi |= 0x1;
                else if (spis[i] == 'T')
                {
                    if (i + 1 < spis.Length && spis[i + 1] == '#')
                    {
                        mSpi |= 0x4;
                        ++i;
                    }
                    else
                        mSpi |= 0x2;
                }
            }
            Occurs = new string[] { };Priorties = new int [] { };
            IsOnce = new bool [] { }; IsTermini = new bool [] { };
            Lock = new bool[] { };
        }

        public delegate void ActionDelegate();
        private ActionDelegate mAction;
        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? DefAction; }
        }
        private ActionDelegate mPers;
        public ActionDelegate Pers
        {
            set { mPers = value; }
            get { return mPers ?? DefAction; }
        }

        public delegate bool ValidDelegate();
        private ValidDelegate mPersValid;
        public ValidDelegate PersValid
        {
            set { mPersValid = value; }
            get { return mPersValid ?? DefValid; }
        }

        //private InputDelegate mInput;

        //public InputDelegate Input
        //{
        //    set { mInput = value; }
        //    get
        //    {
        //        if (mInput != null)
        //            return mInput;
        //        else
        //            return DefInput;
        //    }
        //}

        private static ActionDelegate DefAction = new ActionDelegate(delegate() { });
        private static ValidDelegate DefValid = new ValidDelegate(delegate() { return true; });

        //private static string DefaultInput(Board board) { return ""; }
        //private static InputDelegate DefInput = new InputDelegate(DefaultInput);

        //public void Action(VW.IXI xi, Board board)
        //{
        //    string cardCode = string.Format("SJ{0:D3}", Avatar - 30000);
        //    lib.GetType().GetMethod(cardCode).Invoke(lib, new object[] { xi, board });
        //}

        //public string Input(VW.IXI xi, Board board)
        //{
        //    var method = lib.GetType().GetMethod("SJ{0:D3}Input");
        //    if (method != null)
        //        return (string)(method.Invoke(lib, new object[] { xi, board }));
        //    else
        //        return null;
        //}

        internal static Evenement Parse(string line)
        {
            if (line != null && line.Length > 0 && !line.StartsWith("#"))
            {
                string[] content = line.Split('\t');
                string code = content[0]; // code, e.g. (SJ402)
                string name = content[1]; // name, e.g. (Shufuhuanmingjie)
                ushort count = ushort.Parse(content[2]);
                string background = content[3];
                string description = content[4];
                string spis = content[5];
                int group = int.Parse(content[5]);
                return new Evenement(name, code, count, group, background, description, spis);
            }
            else
                return null;
        }
    }

    public class EvenementLib
    {
        private List<Evenement> firsts;

        private IDictionary<ushort, Evenement> dicts;

        private Utils.ReadonlySQL sql;

        public EvenementLib(string path)
        {
            firsts = new List<Evenement>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                Evenement eve = Evenement.Parse(line);
                if (eve != null)
                    firsts.Add(eve);
            }
            ushort cardx = 1;
            dicts = new Dictionary<ushort, Evenement>();
            foreach (Evenement eve in firsts)
            {
                for (int i = 0; i < eve.Count; ++i)
                    dicts.Add(cardx++, eve);
            }
        }

        public EvenementLib()
        {
            firsts = new List<Evenement>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "VALID", "NAME", "COUNT", "BACKGROUND", "EFFECT", "SPI",
                "OCCURS", "PRIORS", "ONCES", "TERMINS"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Eve");
            foreach (System.Data.DataRow data in datas)
            {
                ushort valid = (ushort)((short)data["VALID"]);
                if (valid > 0)
                {
                    string code = (string)data["CODE"];
                    string name = (string)data["NAME"];
                    ushort count = (ushort)((short)data["COUNT"]);
                    string bg = (string)data["BACKGROUND"];
                    string effect = (string)data["EFFECT"];
                    string spis = (string)data["SPI"];

                    string occurss = (string)data["OCCURS"];
                    string[] occurs = string.IsNullOrEmpty(occurss) ? new string[] { }
                        : occurss.Split(',').Select(p => p.StartsWith("!") ? p.Substring(1) : p).ToArray();
                    bool[] lks = string.IsNullOrEmpty(occurss) ? new bool[] { }
                        : occurss.Split(',').Select(p => p.StartsWith("!")).ToArray();
                    string priorss = (string)data["PRIORS"];
                    int[] priors = string.IsNullOrEmpty(priorss) ? new int[] { }
                        : priorss.Split(',').Select(p => int.Parse(p)).ToArray();
                    string oncess = (string)data["ONCES"];
                    bool[] onces = string.IsNullOrEmpty(oncess) ? new bool[] { }
                        : oncess.Split(',').Select(p => p != "0").ToArray();
                    string terminss = (string)data["TERMINS"];
                    bool[] termins = string.IsNullOrEmpty(terminss) ? new bool[] { }
                        : terminss.Split(',').Select(p => p != "0").ToArray();
                    firsts.Add(new Evenement(name, code, count, valid, bg, effect, spis)
                    {
                        Occurs = occurs,
                        Priorties = priors,
                        IsOnce = onces,
                        IsTermini = termins,
                        Lock = lks
                    });
                }
            }
            ushort cardx = 1;
            dicts = new Dictionary<ushort, Evenement>();
            foreach (Evenement eve in firsts)
            {
                for (int i = 0; i < eve.Count; ++i)
                    dicts.Add(cardx++, eve);
            }
        }

        //public IEnumerable<Evenement> Firsts { get { return firsts; } }

        //public int Size { get { return dicts.Count; } }

        public Evenement DecodeEvenement(ushort code)
        {
            Evenement evenement;
            if (dicts.TryGetValue(code, out evenement))
                return evenement;
            else return null;
        }

        public Evenement GetEveFromName(string code)
        {
            foreach (var pair in dicts)
                if (pair.Value.Code == code)
                    return pair.Value;
            return null;
        }

        public List<ushort> ListAllSeleable(int groups)
        {
            if (groups == 0)
                return dicts.Keys.ToList();
            else
                return dicts.Where(p => ((groups & (1 << (p.Value.Group - 1))) != 0)).Select(p => p.Key).ToList();
        }

        public List<Evenement> ListAllEves(int groups)
        {
            if (groups == 0)
                return firsts;
            else
                return firsts.Where(p => ((groups & (1 << (p.Group - 1))) != 0)).ToList();
        }
    }
}
