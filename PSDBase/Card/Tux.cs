using System;
using System.Collections.Generic;
using System.Linq;

namespace PSD.Base.Card
{
    public class Tux
    {
        public enum TuxType { HX, JP, ZP, TP, WQ, FJ, XB };

        public TuxType Type { private set; get; }
        // Tux hand name, (e.g.) Tianxuanwuyin
        public string Name { private set; get; }
        // avatar = serial, (e.g.) 50204
        //public int Avatar { private set; get; }
        public string Code { private set; get; }
        // Integer in data base to numbering code
        public ushort DBSerial { protected set; get; }

        //public int Count { private set; get; }
        // package contains all the tux involved in (e.g.) { 1, 4 }
        public int[] Package { protected set; get; }
        public int Genre { private set; get; }
        // Package contains the tux number range in given package
        // (e.g.) { 1, 2, 82, 84 } means 1~2 in package 1# an 82~84 in package 4#
        public ushort[] Range { protected set; get; }

        public string Description { private set; get; }
        public IDictionary<string, string> Special { private set; get; }

        public int[] Priorities { protected set; get; }
        public string[] Occurs { protected set; get; }
        public string[] Parasitism { get; protected set; }

        public char[] Targets { protected set; get; }
        public bool[] IsTermini { protected set; get; }
        // whether is equipped or used directly
        // 1-set-eqiup, 2-set-pending
        public ushort[] IsEq { protected set; get; }
        // whether only can work for the owner himself
        //public bool IsSelfType { private set; get; }

        public delegate void ActionDelegate(Player player, int type, string fuse, string argst);
        public delegate bool ValidDelegate(Player player, int type, string fuse);
        public delegate string InputDelegate(Player player, int type, string fuse, string prev);
        public delegate string InputHolderDelegate(Player provider, Player user, int type, string fuse, string prev);
        public delegate string EncryptDelegate(string args);
        public delegate void LocustActionDelegate(Player player, int type, string fuse,
            string cdFuse, Player locuster, Tux locust);

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
        private InputHolderDelegate mInputHolder;
        public InputHolderDelegate InputHolder
        {
            set { mInputHolder = value; }
            get { return mInputHolder ?? DefInputHolder; }
        }

        private ValidDelegate mValid, mBribe;
        public ValidDelegate Valid
        {
            set { mValid = value; }
            get { return mValid ?? DefValid; }
        }
        public ValidDelegate Bribe
        {
            set { mBribe = value; }
            get { return mBribe ?? DefValid; }
        }

        private EncryptDelegate mEncrypt;
        public EncryptDelegate Encrypt
        {
            set { mEncrypt = value; }
            get { return mEncrypt ?? DefEncrypt; }
        }

        private LocustActionDelegate mLocust;
        public LocustActionDelegate Locust
        {
            set { mLocust = value; }
            get { return mLocust ?? DefLocust; }
        }

        protected static ActionDelegate DefAction = (p, t, f, a) => { };
        protected static ValidDelegate DefValid = (p, t, f) => { return true; };
        protected static InputDelegate DefInput = (p, t, f, pr) => { return ""; };
        protected static InputHolderDelegate DefInputHolder = (p, u, t, f, pr) => { return ""; };
        protected static EncryptDelegate DefEncrypt = (a) => { return a; };
        protected static LocustActionDelegate DefLocust = (p, t, f, cd, lr, l) => { };

        // public Delegate Type of Handling events
        internal Tux(string name, string code, int genre, TuxType type, string description,
            IDictionary<string, string> special)
        {
            this.Name = name; this.Code = code;
            this.Genre = genre;
            //this.Count = count;
            this.Type = type;
            this.Description = description; this.Special = special;
        }

        public virtual bool IsTuxEqiup() { return false; }
        internal virtual void Parse(string countStr, string occurStr, string parasitismStr,
            string priorStr, string isEqStr, string tarStr, string terminiStr, long dbSerial)
        {
            if (countStr != "")
            {
                string[] counts = countStr.Split(',');
                Package = new int[counts.Length / 3];
                Range = new ushort[(counts.Length / 3) * 2];
                for (int i = 0; i < counts.Length; i += 3)
                {
                    Package[i / 3] = int.Parse(counts[i]);
                    Range[(i / 3) * 2] = ushort.Parse(counts[i + 1]);
                    Range[(i / 3) * 2 + 1] = ushort.Parse(counts[i + 2]);
                }
            }
            int sz;
            if (occurStr != "" && occurStr != "^")
            {
                string[] occurs = occurStr.Split(',');
                sz = occurs.Length;
                Occurs = new string[sz];
                for (int i = 0; i < sz; ++i)
                    Occurs[i] = occurs[i];
            }
            else
                sz = 0;
            if (parasitismStr != null && parasitismStr != "" && parasitismStr != "^")
                Parasitism = parasitismStr.Split('&');
            else
                Parasitism = new string[0];
            if (priorStr != "" && priorStr != "^")
            {
                string[] priors = priorStr.Split(',');
                Priorities = new int[sz];
                for (int i = 0; i < sz; ++i)
                    Priorities[i] = int.Parse(priors[i]);
            }
            if (isEqStr != "" && isEqStr != "^")
            {
                string[] eqs = isEqStr.Split(',');
                IsEq = new ushort[sz];
                for (int i = 0; i < sz; ++i)
                    IsEq[i] = ushort.Parse(eqs[i]);
            }
            if (tarStr != "")
            {
                string[] tars = tarStr.Split(',');
                Targets = new char[sz];
                for (int i = 0; i < sz; ++i)
                    Targets[i] = tars[i][0];
            }
            if (terminiStr != "")
            {
                string[] ters = terminiStr.Split(',');
                IsTermini = new bool[sz];
                for (int i = 0; i < sz; ++i)
                    IsTermini[i] = (ters[i][0] == '1');
            }
            else
            {
                IsTermini = new bool[sz];
                for (int i = 0; i < sz; ++i)
                    IsTermini[i] = false;
            }
            DBSerial = (ushort)dbSerial;
        }

        public virtual string OccurString()
        {
            if (Occurs != null)
                return string.Join(",", Occurs);
            else return "";
        }

        public bool IsLinked(int inType)
        {
            return Occurs != null && Occurs.Length > inType && Occurs[inType].Contains('&');
        }
    }

    public class TuxLib
    {
        public List<Tux> Firsts { private set; get; }

        private IDictionary<ushort, Tux> dicts;

        private Utils.ReadonlySQL sql;

        //public TuxLib(string path)
        //{
        //    Firsts = new List<Tux>();
        //    string[] lines = System.IO.File.ReadAllLines(path);
        //    foreach (string line in lines)
        //    {
        //        if (line != null && line.Length > 0)
        //        {
        //            string[] content = line.Split('\t');
        //            string code = content[0]; // code, e.g. (ZP04)
        //            string name = content[1]; // name, e.g. (Tianxuanwuyin)
        //            //ushort count = ushort.Parse(content[2]);
        //            Tux.TuxType type;
        //            switch (code.Substring(0, 2))
        //            {
        //                case "JP": type = Tux.TuxType.JP; break;
        //                case "ZP": type = Tux.TuxType.ZP; break;
        //                case "TP": type = Tux.TuxType.TP; break;
        //                case "WQ": type = Tux.TuxType.WQ; break;
        //                case "FJ": type = Tux.TuxType.FJ; break;
        //                case "XB": type = Tux.TuxType.XB; break;
        //                default: type = Tux.TuxType.HX; break;
        //            }
        //            string countStr = content[2];
        //            string occur = content[3];
        //            string priority = content[4];
        //            string parasitismStr = content[5];
        //            string description = content[6];
        //            string descstr = content[7];
        //            IDictionary<string, string> special = new Dictionary<string, string>();
        //            string[] descSpt = string.IsNullOrEmpty(descstr) ?
        //                    new string[] { } : descstr.Split('|');
        //            for (int i = 1; i < descSpt.Length; i += 2)
        //                special.Add(descSpt[i], descSpt[i + 1]);
        //            string isEqs = content[8];
        //            string targets = content[9];
        //            string growup = content[10];
        //            string terminiStr = content[11];
        //            if (type == Tux.TuxType.WQ || type == Tux.TuxType.FJ || type == Tux.TuxType.XB)
        //            {
        //                var tux = new TuxEqiup(name, code, type, description, special, growup);
        //                tux.Parse(countStr, occur, parasitismStr, priority, isEqs, targets, terminiStr);
        //                Firsts.Add(tux);
        //            }
        //            else
        //            {
        //                var tux = new Tux(name, code, type, description, special);
        //                tux.Parse(countStr, occur, parasitismStr, priority, isEqs, targets, terminiStr);
        //                Firsts.Add(tux);
        //            }
        //        }
        //    }
        //    //ushort cardx = 1;
        //    dicts = new Dictionary<ushort, Tux>();
        //    foreach (Tux tux in Firsts)
        //    {
        //        for (int i = 0; i < tux.Range.Length; i += 2)
        //        {
        //            for (ushort j = tux.Range[i]; j <= tux.Range[i + 1]; ++j)
        //                dicts.Add(j, tux);
        //        }
        //    }
        //}

        public TuxLib()
        {
            Firsts = new List<Tux>();
            sql = new Utils.ReadonlySQL("psd.db3");
            List<string> list = new string[] {
                "ID", "CODE", "NAME", "COUNT", "OCCURS", "PRIORS", "PARASITISM",
                "DESCRIPTION", "SPECIAL", "ISEQ", "TARGET", "GROWUP", "TERMHIND", "GENRE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Tux");
            foreach (System.Data.DataRow data in datas)
            {
                long lid = (long)data["ID"];
                string code = (string)data["CODE"];
                int genre = (ushort)((long)data["GENRE"]);
                string name = (string)data["NAME"];
                //ushort count = (ushort)((short)data["COUNT"]);
                Tux.TuxType type;
                switch (code.Substring(0, 2))
                {
                    case "JP": type = Tux.TuxType.JP; break;
                    case "ZP": type = Tux.TuxType.ZP; break;
                    case "TP": type = Tux.TuxType.TP; break;
                    case "WQ": type = Tux.TuxType.WQ; break;
                    case "FJ": type = Tux.TuxType.FJ; break;
                    case "XB": type = Tux.TuxType.XB; break;
                    default: type = Tux.TuxType.HX; break;
                }
                string countStr = (string)data["COUNT"];
                string occur = (string)data["OCCURS"];
                string priority = (string)data["PRIORS"];
                string parasitismStr = (string)data["PARASITISM"];
                string description = (string)data["DESCRIPTION"];
                string descstr = (string)data["SPECIAL"];
                IDictionary<string, string> special = new Dictionary<string, string>();
                string[] descSpt = string.IsNullOrEmpty(descstr) ?
                        new string[] { } : descstr.Split('|');
                for (int i = 1; i < descSpt.Length; i += 2)
                    special.Add(descSpt[i], descSpt[i + 1]);
                string isEqs = (string)data["ISEQ"];
                string targets = (string)data["TARGET"];
                string terministr = (string)data["TERMHIND"];
                if (type == Tux.TuxType.XB && isEqs == "5")
                {
                    string growup = (string)data["GROWUP"];
                    isEqs = "1";
                    var tux = new Luggage(name, code, genre, type, description, special, growup);
                    tux.Parse(countStr, occur, parasitismStr,
                        priority, isEqs, targets, terministr, (ushort)lid);
                    Firsts.Add(tux);
                }
                else if (type == Tux.TuxType.WQ || type == Tux.TuxType.FJ || type == Tux.TuxType.XB)
                {
                    string growup = (string)data["GROWUP"];
                    var tux = new TuxEqiup(name, code, genre, type, description, special, growup);
                    tux.Parse(countStr, occur, parasitismStr,
                        priority, isEqs, targets, terministr, (ushort)lid);
                    Firsts.Add(tux);
                }
                else
                {
                    var tux = new Tux(name, code, genre, type, description, special);
                    tux.Parse(countStr, occur, parasitismStr,
                        priority, isEqs, targets, terministr, (ushort)lid);
                    Firsts.Add(tux);
                }
            }
            //ushort cardx = 1;
            dicts = new Dictionary<ushort, Tux>();
            foreach (Tux tux in Firsts)
            {
                for (int i = 0; i < tux.Range.Length; i += 2)
                {
                    for (ushort j = tux.Range[i]; j <= tux.Range[i + 1]; ++j)
                        dicts.Add(j, tux);
                }
                if (tux.IsTuxEqiup())
                {
                    TuxEqiup te = tux as TuxEqiup;
                    te.SingleEntry = tux.Range[0];
                }
            }
        }

        public int Size { get { return dicts.Count; } }

        public const int DEF_PRIORTY = 110;

        public Tux DecodeTux(ushort code)
        {
            Tux tux;
            if (dicts.TryGetValue(code, out tux))
                return tux;
            else return null;
        }
        public Tux EncodeTuxCode(string code)
        {
            foreach (Tux tux in Firsts)
            {
                if (tux.Code.Equals(code))
                    return tux;
            }
            return null;
        }
        public Tux EncodeTuxDbSerial(ushort dbSerial)
        {
            List<Tux> tuxes = Firsts.Where(
                p => p.DBSerial == dbSerial).ToList();
            return tuxes.Count > 0 ? tuxes.First() : null;
        }
        public List<Tux> ListAllTuxs(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            if (pkgs == null)
                return Firsts.ToList();
            else
                return Firsts.Where(p => p.Package.Any(q => pkgs.Contains(q))).ToList();
        }
        public List<Tux> ListAllTuxSeleable(int groups)
        {
            List<Tux> first = ListAllTuxs(groups);
            string[] duplicated = { "TPT2", "JPT1", "JPT4" };
            string[] keeppace = { "TPR1", "JPR1", "JPR2" };
            for (int i = 0; i < duplicated.Length; ++i)
            {
                if (first.Any(p => p.Code == duplicated[i]) && first.Any(p => p.Code == keeppace[i]))
                    first.RemoveAll(p => p.Code == duplicated[i]);
            }
            return first;
        }
        public List<ushort> ListAllTuxCodes(int groups)
        {
            int[] pkgs = Card.Level2Pkg(groups);
            List<Tux> txs = ListAllTuxSeleable(groups);
            List<ushort> us = new List<ushort>();
            foreach (Tux tux in txs)
            {
                for (int i = 0; i < tux.Package.Length; ++i)
                {
                    if (pkgs == null || pkgs.Contains(tux.Package[i]))
                    {
                        for (ushort j = tux.Range[i * 2]; j <= tux.Range[i * 2 + 1]; ++j)
                            us.Add(j);
                    }
                }
            }
            return us;
        }
        // Find the unique equip code number, return 0 when no or duplicated found
        public ushort UniqueEquipSerial(string code)
        {
            ushort ans = 0;
            foreach (var pair in dicts) {
                if (pair.Value.Code.Equals(code)) {
                    if (ans == 0)
                        ans = pair.Key;
                    else
                        return 0;
                }
            }
            return ans;
        }
    }

    public class TuxEqiup : Tux
    {
        public int IncrOfSTR { set; get; }
        public int IncrOfDEX { set; get; }

        public ushort SingleEntry { set; get; }

        public int[][] CsPriorites { private set; get; }
        public string[][] CsOccur { private set; get; }
        public bool[][] CsLock { private set; get; }
        public bool[][] CsIsTermini { private set; get; }
        public bool[][] CsHind { private set; get; }

        public delegate void CrActionDelegate(Player player);
        public delegate void CsActionDelegate(Player player,
            int consumeType, int type, string fuse, string argst);
        public delegate void CsActionHolderDelegate(Player provider, Player user,
            int consumeType, int type, string fuse, string argst);
        public delegate bool CsValidDelegate(Player player,
            int consumeType, int type, string fuse);
        public delegate bool CsValidHolderDelegate(Player provider, Player user,
            int consumeType, int type, string fuse);
        public delegate string CsInputDelegate(Player player,
            int consumeType, int type, string fuse, string prev);
        public delegate string CsInputHolderDelegate(Player provider, Player user,
            int consumeType, int type, string fuse, string prev);
        public delegate void CsUseActionDelegate(ushort cardUt, Player player);

        private CrActionDelegate mIncrAction, mDecrAction;
        private CsActionDelegate mConsumeAction;
        private CsActionHolderDelegate mConsumeActionHolder;
        private CrActionDelegate mInsAction, mDelAction;

        public CrActionDelegate IncrAction
        {
            set { mIncrAction = value; }
            get { return mIncrAction ?? DefCrAction; }
        }
        public CrActionDelegate DecrAction
        {
            set { mDecrAction = value; }
            get { return mDecrAction ?? DefCrAction; }
        }
        public CrActionDelegate InsAction
        {
            set { mInsAction = value; }
            get { return mInsAction ?? DefCrAction; }
        }
        public CrActionDelegate DelAction
        {
            set { mDelAction = value; }
            get { return mDelAction ?? DefCrAction; }
        }
        public CsActionDelegate ConsumeAction
        {
            set { mConsumeAction = value; }
            get { return mConsumeAction ?? DefCsAction; }
        }
        public CsActionHolderDelegate ConsumeActionHolder
        {
            set { mConsumeActionHolder = value; }
            get { return mConsumeActionHolder ?? DefCsActionHolder; }
        }

        private CsInputDelegate mConsumeInput;
        public CsInputDelegate ConsumeInput
        {
            set { mConsumeInput = value; }
            get { return mConsumeInput ?? DefCsInput; }
        }
        private CsInputHolderDelegate mConsumeInputHolder;
        public CsInputHolderDelegate ConsumeInputHolder
        {
            set { mConsumeInputHolder = value; }
            get { return mConsumeInputHolder ?? DefCsInputHolder; }
        }

        private CsValidDelegate mConsumeValid;
        public CsValidDelegate ConsumeValid
        {
            set { mConsumeValid = value; }
            get { return mConsumeValid ?? DefCsValid; }
        }
        private CsValidHolderDelegate mConsumeValidHolder;
        public CsValidHolderDelegate ConsumeValidHolder
        {
            set { mConsumeValidHolder = value; }
            get { return mConsumeValidHolder ?? DefCsValidHolder; }
        }

        private CsUseActionDelegate mUseAction; // use/equip the card
        public CsUseActionDelegate UseAction
        {
            set { mUseAction = value; }
            get { return mUseAction ?? DefCsUseAction; }
        }

        protected static CrActionDelegate DefCrAction = new CrActionDelegate(delegate(Player player) { });
        protected static CsActionDelegate DefCsAction = new CsActionDelegate(
            delegate(Player player, int consumeType, int type, string fuse, string argst) { });
        protected static CsActionHolderDelegate DefCsActionHolder = new CsActionHolderDelegate(
            delegate(Player provider, Player user, int consumeType, int type, string fuse, string argst) { });
        protected static CsValidDelegate DefCsValid = new CsValidDelegate(
            delegate(Player player, int consumeType, int type, string fuse) { return true; });
        protected static CsValidHolderDelegate DefCsValidHolder = new CsValidHolderDelegate(
            delegate(Player provider, Player user, int consumeType, int type, string fuse) { return true; });
        protected static CsInputDelegate DefCsInput = new CsInputDelegate(
            delegate(Player player, int consumeType, int type, string fuse, string prev) { return ""; });
        protected static CsInputHolderDelegate DefCsInputHolder = new CsInputHolderDelegate(
            delegate(Player provider, Player user, int consumeType, int type, string fuse, string prev) { return ""; });
        protected static CsUseActionDelegate DefCsUseAction = new CsUseActionDelegate(
            delegate(ushort cardUt, Player player) { });

        public override bool IsTuxEqiup() { return true; }

        public virtual bool IsLuggage() { return false; }

        internal override void Parse(string countStr, string occurStr, string parasitismStr,
            string priorStr, string isEqStr, string tarStr, string tmhdstr, long dbSerial)
        {
            if (countStr != "")
            {
                string[] counts = countStr.Split(',');
                Package = new int[counts.Length / 3];
                Range = new ushort[(counts.Length / 3) * 2];
                for (int i = 0; i < counts.Length; i += 3)
                {
                    Package[i / 3] = int.Parse(counts[i]);
                    Range[(i / 3) * 2] = ushort.Parse(counts[i + 1]);
                    Range[(i / 3) * 2 + 1] = ushort.Parse(counts[i + 2]);
                }
            }
            string[] occurStrss = occurStr.Split(';');
            if (occurStrss.Length > 1)
            {
                CsOccur = new string[occurStrss.Length - 1][];
                CsLock = new bool[occurStrss.Length - 1][];
            }
            for (int i = 0; i < occurStrss.Length; ++i)
            {
                if (occurStrss[i] != "" && occurStrss[i] != "^")
                {
                    if (i == 0)
                    {
                        string[] occurs = occurStrss[i].Split(',');
                        Occurs = new string[occurs.Length];
                        for (int j = 0; j < occurs.Length; ++j)
                            Occurs[j] = occurs[j];
                    }
                    else
                    {
                        string[] occurs = occurStrss[i].Split(',');
                        CsOccur[i - 1] = new string[occurs.Length];
                        CsLock[i - 1] = new bool[occurs.Length];
                        for (int j = 0; j < occurs.Length; ++j)
                        {
                            if (occurs[j].StartsWith("!"))
                            {
                                CsOccur[i - 1][j] = occurs[j].Substring(1);
                                CsLock[i - 1][j] = true;
                            }
                            else
                            {
                                CsOccur[i - 1][j] = occurs[j];
                                CsLock[i - 1][j] = false;
                            }
                        }
                    }
                }
            }
            if (parasitismStr != null && parasitismStr != "" && parasitismStr != "^")
                Parasitism = parasitismStr.Split('&');
            else
                Parasitism = new string[0];
            string[] priorStrss = priorStr.Split(';');
            if (priorStrss.Length > 1)
                CsPriorites = new int[priorStrss.Length - 1][];
            for (int i = 0; i < priorStrss.Length; ++i)
            {
                if (priorStrss[i] != "" && priorStrss[i] != "^")
                {
                    if (i == 0)
                    {
                        string[] priors = priorStrss[i].Split(',');
                        Priorities = new int[priors.Length];
                        for (int j = 0; j < priors.Length; ++j)
                            Priorities[j] = int.Parse(priors[j]);
                    }
                    else
                    {
                        string[] priors = priorStrss[i].Split(',');
                        CsPriorites[i - 1] = new int[priors.Length];
                        for (int j = 0; j < priors.Length; ++j)
                            CsPriorites[i - 1][j] = int.Parse(priors[j]);
                    }
                }
            }
            string[] tars = tarStr.Split(',');
            Targets = new char[tars.Length];
            for (int i = 0; i < tars.Length; ++i)
                Targets[i] = tars[i][0];
            string[] isEqs = isEqStr.Split(',');
            IsEq = new ushort[isEqs.Length];
            for (int i = 0; i < isEqs.Length; ++i)
                IsEq[i] = ushort.Parse(isEqs[i]);

            string[] tmhd = tmhdstr.Split(';');
            if (tmhd.Length > 1)
            {
                CsIsTermini = new bool[tmhd.Length - 1][];
                CsHind = new bool[tmhd.Length - 1][];
            }
            for (int i = 0; i < tmhd.Length; ++i)
            {
                if (tmhd[i] != "" && tmhd[i] != "^")
                {
                    if (i == 0)
                    {
                        string[] trs = tmhd[i].Split(',');
                        IsTermini = new bool[trs.Length];
                        for (int j = 0; j < trs.Length; ++j)
                            IsTermini[j] = (trs[j][0] == '1');
                    }
                    else
                    {
                        string[] trs = tmhd[i].Split(',');
                        CsIsTermini[i - 1] = new bool[trs.Length];
                        CsHind[i - 1] = new bool[trs.Length];
                        for (int j = 0; j < trs.Length; ++j)
                        {
                            int val = int.Parse(trs[j]);
                            CsIsTermini[i - 1][j] = (val & 1) != 0;
                            CsHind[i - 1][j] = (val & 2) != 0;
                        }
                    }
                }
            }
            this.DBSerial = (ushort)dbSerial;
        }

        public override string OccurString()
        {
            string ocr = "";
            if (Occurs != null)
                ocr += string.Join(",", Occurs);
            else
                ocr += "^";
            if (CsOccur != null)
                ocr += ";" + string.Join(";", CsOccur.Select(p => (p != null) ? string.Join(",", p) : "^"));
            else
                ocr += "^";
            return ocr;
        }

        public bool IsLinked(int consumeType, int inType)
        {
            return CsOccur != null && CsOccur.Length > consumeType &&
                CsOccur[consumeType].Length > inType && CsOccur[consumeType][inType].Contains('&');
        }

        public TuxEqiup(string name, string code, int genre, TuxType type,
                string description, IDictionary<string, string> special, string growup) :
            base(name, code, genre, type, description, special)
        {
            //int idxh = growup.IndexOf('H');
            int idxa = growup.IndexOf('A');
            int idxx = growup.IndexOf('X');
            IncrOfSTR = idxa < 0 ? 0 : int.Parse(Substring(growup, idxa + 1, idxx));
            IncrOfDEX = idxx < 0 ? 0 : int.Parse(Substring(growup, idxx + 1, -1));
        }

        private static string Substring(string @string, int idx, int jdx)
        {
            if (idx < 0)
                return "";
            else if (jdx == -1)
                return @string.Substring(idx);
            else
                return @string.Substring(idx, jdx - idx);
        }
    }

    public class Luggage : TuxEqiup
    {
        // Cards Containing in the luggage (C/M/E...)
        public List<string> Capacities { private set; get; }

        public override bool IsLuggage() { return true; }
        // Indicate whether in processing of pulling goods, avoid recycle when erase luggage itself.
        public bool Pull { set; get; }

        public Luggage(string name, string code, int genre, TuxType type,
                string description, IDictionary<string, string> special, string growup) :
            base(name, code, genre, type, description, special, growup)
        {
            Capacities = new List<string>();
            Pull = false;
        }
    }
}
