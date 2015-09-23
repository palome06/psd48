﻿using System;
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
        public int Genre { private set; get; }

        public SKBranch[] Branches { private set; get; }
        //public delegate void ActionDelegate(Player player, Board board);
        //public ActionDelegate Action { private set; get; }

        private int mSpi;
        public bool IsHarmInvolved() { return (mSpi & 0x1) != 0; }
        public bool IsTuxInvolved(bool self)
        {
            if ((mSpi & 0x2) != 0) return true;
            return (mSpi & 0x4) != 0 && self;
        }

<<<<<<< HEAD
        internal Evenement(string name, string code, int count, int group, int genre,
            string background, string description, string spis)
=======
        internal Evenement(string name, string code, int count, int group, string occurStr,
            string priortyStr, string mixCodeStr, string background, string description, string spis)
>>>>>>> breaking into reconstruction of SKBranches and then sk02
        {
            this.Name = name; this.Code = code;
            this.Count = count; this.Background = background;
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
            }
            Branches = SKBranch.ParseFromStrings(occurStr, priortyStr, mixedCodeStr);
        }

        public delegate void ActionDelegate(Player player);
        private ActionDelegate mAction;
        public ActionDelegate Action
        {
            set { mAction = value; }
            get { return mAction ?? ((p) => { }); }
        }
        private ActionDelegate mPers;
        public ActionDelegate Pers
        {
            set { mPers = value; }
            get { return mPers ?? ((p) => { }); }
        }

        public delegate bool ValidDelegate();
        private ValidDelegate mPersValid;
        public ValidDelegate PersValid
        {
            set { mPersValid = value; }
            get { return mPersValid ?? (() => { return true; }); }
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
        public void ForceChange(string field, object value)
        {
            if (field == "Count" && value is ushort)
                Count = (ushort)value;
        }
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
                "CODE", "VALID", "NAME", "COUNT", "BACKGROUND", "EFFECT", "SPI", "GENRE"
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
                    ushort count = (ushort)((short)data["COUNT"]);
                    string bg = (string)data["BACKGROUND"];
                    string effect = (string)data["EFFECT"];
                    string spis = (string)data["SPI"];

                    string occurss = (string)data["OCCURS"];
                    string priorss = (string)data["PRIORS"];
<<<<<<< HEAD
                    int[] priors = string.IsNullOrEmpty(priorss) ? new int[] { }
                        : priorss.Split(',').Select(p => int.Parse(p)).ToArray();
                    string oncess = (string)data["ONCES"];
                    bool[] onces = string.IsNullOrEmpty(oncess) ? new bool[] { }
                        : oncess.Split(',').Select(p => p != "0").ToArray();
                    string terminss = (string)data["TERMINS"];
                    bool[] termins = string.IsNullOrEmpty(terminss) ? new bool[] { }
                        : terminss.Split(',').Select(p => p != "0").ToArray();
                    firsts.Add(new Evenement(name, code, count, valid, genre, bg, effect, spis)
                    {
                        Occurs = occurs,
                        Priorties = priors,
                        IsOnce = onces,
                        IsTermini = termins,
                        Lock = lks
                    });
=======
                    string mixcodess = (string)data["MIXCODES"];
                    firsts.Add(new Evenement(name, code, count, valid, occurss, priorss,
                         mixcodess, bg, effect, spis));
>>>>>>> breaking into reconstruction of SKBranches and then sk02
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
            dicts.Clear(); ushort cardx = 1;
            foreach (Evenement eve in firsts)
            {
                for (int i = 0; i < eve.Count; ++i)
                    dicts.Add(cardx++, eve);
            }
        }
    }
}
