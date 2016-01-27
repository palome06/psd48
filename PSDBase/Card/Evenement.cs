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
        public ushort[] Range { private set; get; }

        public string Background { private set; get; }
        public string Description { private set; get; }
        // group, e.g. 1 for standard, 0 for test, 2 for SP, etc.
        public int Group { private set; get; }
        public int Genre { private set; get; }

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
        public bool IsSilence() { return (mSpi & 0x8) != 0; }

        internal Evenement(string name, string code, string range, int group, int genre,
            string background, string description, string spis)
        {
            this.Name = name; this.Code = code;
            Range = range.Split(',').Select(p => ushort.Parse(p)).ToArray();
            this.Background = background;
            this.Group = group; this.Genre = genre;
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
                else if (spis[i] == 'S')
                    mSpi |= 0x8;
            }
            Occurs = new string[] { }; Priorties = new int[] { };
            IsOnce = new bool[] { }; IsTermini = new bool[] { };
            Lock = new bool[] { };
        }

        public delegate void ActionDelegate(Player player);
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

        private static ActionDelegate DefAction = new ActionDelegate(delegate(Player player) { });
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
    }

    public class EvenementLib
    {
        private List<Evenement> firsts;

        private IDictionary<ushort, Evenement> dicts;

        private Utils.ReadonlySQL sql;
        
        public EvenementLib()
        {
            firsts = new List<Evenement>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "CODE", "VALID", "NAME", "RANGE", "BACKGROUND", "EFFECT", "SPI", "GENRE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Eve");
            foreach (System.Data.DataRow data in datas)
            {
                ushort valid = (ushort)((short)data["VALID"]);
                if (valid > 0)
                {
                    string code = (string)data["CODE"];
                    string name = (string)data["NAME"];
                    int genre = (int)((long)data["GENRE"]);
                    string range = (string)data["RANGE"];
                    string bg = (string)data["BACKGROUND"];
                    string effect = (string)data["EFFECT"];
                    string spis = (string)data["SPI"];
                    firsts.Add(new Evenement(name, code, range, valid, genre, bg, effect, spis));
                }
            }
            dicts = new Dictionary<ushort, Evenement>();
            Refresh();
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
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return dicts.Keys.ToList();
            else
                return dicts.Where(p => pkgs.Contains(p.Value.Group)).Select(p => p.Key).ToList();
        }
        public List<Evenement> ListAllEves(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return firsts;
            else
                return firsts.Where(p => pkgs.Contains(p.Group)).ToList();
        }
        public void Refresh()
        {
            dicts.Clear();
            foreach (Evenement eve in firsts)
            {
                for (ushort i = eve.Range[0]; i <= eve.Range[1]; ++i)
                    dicts.Add(i, eve);
            }
        }
    }
}
