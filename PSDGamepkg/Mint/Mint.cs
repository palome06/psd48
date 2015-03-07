using PSD.Base.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.PSDGamepkg.Mint
{
    public enum MintType
    {
        NONE,
        GENERAL, // G0
        INNER, // G1, won't cause E communication directly
        UI_ONLY, // G2, trigger E communcation directly

        ROUND // R, rounder
    };

    public class Mint : PSD.Base.Flow.MintBase
    {
        private Diva diva;
        public virtual string Head { get { return "****"; } }
        public virtual MintType MintType { get { return MintType.NONE; } }

        public Mint Set(string key, object value) { diva.Set(key, value); return this; }

        public ushort GetUshort(string key) { return diva.GetUshort(key); }
        public int GetInt(string key) { return diva.GetInt(key); }
        public string GetString(string key) { return diva.GetString(key); }
        public bool? GetBool(string key) { return diva.GetBool(key); }
        public Diva GetDiva(string key) { return diva.GetDiva(key); }
        public Mint GetMint(string key) { return diva.GetObject(key) as Mint; }
        public List<ushort> GetUshortArray(String key) { return NullizeList(diva.GetUshortArray(key)); }
        public List<int> GetIntArray(String key) { return NullizeList(diva.GetIntArray(key)); }
        public List<string> GetStringArray(String key) { return NullizeList(diva.GetStringArray(key)); }
        public List<bool> GetBoolArray(String key) { return NullizeList(diva.GetBoolArray(key)); }
        public List<Diva> GetDivaArray(String key) { return NullizeList(diva.GetDivaArray(key)); }
        private static List<T> NullizeList<T>(List<T> list) { return list.Count > 0 ? list : null; }

        public Mint() { diva = new Diva(); }
        public Mint(params object[] pairs) { diva = new Diva(pairs); }
        public override string ToMessage() { return Head; }
        public virtual void Handle(XI xi)
        {
            if (MintType == MintType.UI_ONLY)
                xi.WI.BCast("E0" + ToMessage().Substring("G2".Length));
            else
                xi.RaiseGMessage(ToMessage(), false); // TODO: Gradually refurbish it
        }
        public virtual void Handle(XI xi, int priority) { if (priority == 100) Handle(xi); }
        public override string ToString() { return Head + ":" + diva.ToString(); }
        public static Mint Parse(string message)
        {
            if (message.StartsWith("G0HR"))
                return HeavyRotation.Parse(message);
            else if (message.StartsWith("G2IN"))
                return CardOutOfPile.Parse(message);
            else if (message.StartsWith("G2CN"))
                return CardOutOfDise.Parse(message);
            else if (message.StartsWith("G2FU"))
                return Stargazer.Parse(message);
            else if (message.StartsWith("G2SY"))
                return Target.Parse(message);

            else if (message.StartsWith("G0"))
                return DefG0.Parse(message);
            else if (message.StartsWith("G1"))
                return DefG1.Parse(message);
            else return new Mint();
        }
    }
    #region Capability
    public class InRound : Mint
    {
        private readonly string rd;
        public override string Head { get { return rd; } }
        public override MintType MintType { get { return MintType.ROUND; } }
        public InRound(string rd) { this.rd = rd; }
        public static new InRound Parse(string message) { return new InRound(message); }
        public override void Handle(XI xi) { }
    }
    public class DefG0 : Mint
    {
        private readonly string msg;
        public override string Head { get { return msg.Substring(0, 4); } }
        public override MintType MintType { get { return MintType.GENERAL; } }
        public DefG0(string message) { msg = message; }
        public static new DefG0 Parse(string message) { return new DefG0(message); }
    }
    public class DefG1 : Mint
    {
        private readonly string msg;
        public override string Head { get { return msg.Substring(0, 4); } }
        public override MintType MintType { get { return MintType.INNER; } }
        public DefG1(string message) { msg = message; }
        public static new DefG1 Parse(string message) { return new DefG1(message); }
    }
    #endregion Capability
    public class CardOutOfPile : Mint
    {
        public override string Head { get { return "G2IN"; } }
        public override MintType MintType { get { return MintType.UI_ONLY; } }
        public CardOutOfPile(char pile, int count)
        {
            Set("pile", pile.ToString()).Set("count", count);
        }
        public override string ToMessage()
        {
            string pile = GetString("pile");
            int typeCode = (pile == "M") ? 1 : ((pile == "E") ? 2 : 0);
            return Head + "," + typeCode + "," + GetInt("count");
        }
        public static new CardOutOfPile Parse(string message)
        {
            string[] g2in = message.Split(',');
            char[] typeChars = new char[] { 'C', 'M', 'E' };
            return new CardOutOfPile(typeChars[int.Parse(g2in[1])], int.Parse(g2in[2]));
        }
    }

    public class CardOutOfDise : Mint
    {
        public override string Head { get { return "G2CN"; } }
        public override MintType MintType { get { return MintType.UI_ONLY; } }
        public CardOutOfDise(char pile, int count)
        {
            Set("pile", pile.ToString());
            Set("count", count);
        }
        public override string ToMessage()
        {
            string pile = GetString("pile");
            int typeCode = (pile == "M") ? 1 : ((pile == "E") ? 2 : 0);
            return Head + "," + typeCode + "," + GetInt("count");
        }
        public static new CardOutOfDise Parse(string message)
        {
            string[] g2cn = message.Split(',');
            char[] typeChars = new char[] { 'C', 'M', 'E' };
            return new CardOutOfDise(typeChars[int.Parse(g2cn[1])], int.Parse(g2cn[2]));
        }
    }

    public class Target : Mint
    {
        public override string Head { get { return "G2SY"; } }
        public override MintType MintType { get { return MintType.UI_ONLY; } }

        public Target(char fromChar, ushort fromUt, char toChar, ushort toUt)
            : base()
        {
            Set("head", Head).Set("from", new Diva("type", fromChar.ToString(), "ut", fromUt))
                .Set("to", new Diva[] { new Diva("type", toChar.ToString(), "ut", toUt) }.ToList());
        }
        public Target(char fromChar, ushort fromUt, IEnumerable<ushort> toTargetUts)
            : base()
        {
            Set("head", Head).Set("from", new Diva("type", fromChar.ToString(), "ut", fromUt))
                .Set("to", toTargetUts.Select(p => new Diva("type", "T", "ut", p)).ToList());
        }
        private Target(char fromChar, ushort fromUt, object[] tos)
            : base()
        {
            Set("head", Head).Set("from", new Diva("type", fromChar.ToString(), "ut", fromUt));
            List<Diva> toDivas = new List<Diva>();
            for (int i = 0; i < tos.Length; i += 2)
                toDivas.Add(new Diva("type", tos[i], "ut", tos[i + 1]));
            Set("to", toDivas);
        }
        public override string ToMessage()
        {
            return Head + "," + GetDiva("from").GetString("type") + GetDiva("from").GetString("ut")
                + "," + string.Join(",", GetDivaArray("to").Select(p => p.GetString("type") + p.GetString("ut")));
        }
        public static new Target Parse(string message)
        {
            string[] g2sy = message.Split(',');
            char fromChar = g2sy[1][0]; ushort fromUt = ushort.Parse(g2sy[1].Substring(1));
            List<object> tos = new List<object>();
            for (int i = 2; i < g2sy.Length; ++i)
            {
                tos.Add(g2sy[i][0]);
                tos.Add(ushort.Parse(g2sy[i].Substring(1)));
            }
            return new Target(fromChar, fromUt, tos.ToArray());
        }
    }

    public class Stargazer : Mint
    {
        public override string Head { get { return "G2FU"; } }
        public override MintType MintType { get { return MintType.UI_ONLY; } }
        private Stargazer() : base() { }

        public static Stargazer NewStandard(ushort operater,
            IEnumerable<ushort> views, char cardType, IEnumerable<ushort> cards)
        {
            List<ushort> viewList = (views == null ? null : views.ToList());
            return new Stargazer().Set("show", 0).Set("operator", operater).Set("views", viewList)
                .Set("cardType", cardType.ToString()).Set("cards", cards.ToList()) as Stargazer;
        }
        public static Stargazer NewShow(ushort owner, char cardType, IEnumerable<ushort> cards)
        {
            return new Stargazer().Set("show", 1).Set("owner", owner)
                .Set("cardType", cardType.ToString()).Set("cards", cards.ToList()) as Stargazer;
        }
        public static Stargazer NewShow(ushort owner, char cardType, ushort card)
        {
            return NewShow(owner, cardType, new ushort[] { card });
        }
        public static Stargazer NewClose()
        {
            return new Stargazer().Set("show", 2) as Stargazer;
        }
        public static Stargazer NewTakeAway(IEnumerable<ushort> views, char cardType, IEnumerable<ushort> cards)
        {
            List<ushort> viewList = (views == null ? null : views.ToList());
            return new Stargazer().Set("show", -1).Set("views", viewList)
                .Set("cardType", cardType.ToString()).Set("cards", cards.ToList()) as Stargazer;
        }
        public override string ToMessage()
        {
            int? show = GetInt("show");
            if (show == 0)
            {
                List<ushort> views = GetUshortArray("views");
                return Head + ",0," + GetUshort("operater") + "," + (views == null ?
                    "0" : (views.Count + "," + string.Join(",", views))) + "," +
                    GetString("cardType") + "," + string.Join(",", GetUshortArray("cards"));
            }
            else if (show == 1)
            {
                return Head + ",1," + GetUshort("owner") + "," + GetString("cardType") +
                    "," + string.Join(",", GetUshortArray("cards"));
            }
            else if (show == 2)
                return Head + ",2";
            else if (show == -1)
            {
                List<ushort> views = GetUshortArray("views");
                return Head + ",3," + (views == null ?
                    "0" : (views.Count + "," + string.Join(",", views))) + "," +
                    GetString("cardType") + "," + string.Join(",", GetUshortArray("cards"));
            }
            else return "";
        }
        public static new Stargazer Parse(string message)
        {
            string[] g2fu = message.Split(',');
            int show = int.Parse(g2fu[1]);
            if (show == 0)
            {
                ushort op = ushort.Parse(g2fu[2]);
                int viewCount = int.Parse(g2fu[3]);
                ushort[] views = Util.TakeRange(g2fu, 4, 4 + viewCount)
                    .Select(p => ushort.Parse(p)).ToArray();
                char cardType = g2fu[4 + viewCount][0];
                ushort[] cards = Util.TakeRange(g2fu, 5, g2fu.Length)
                    .Select(p => ushort.Parse(p)).ToArray();
                return Stargazer.NewStandard(op, views, cardType, cards);
            }
            else if (show == 1)
            {
                ushort owner = ushort.Parse(g2fu[2]);
                char cardType = g2fu[3][0];
                ushort[] cards = Util.TakeRange(g2fu, 4, g2fu.Length)
                    .Select(p => ushort.Parse(p)).ToArray();
                return Stargazer.NewShow(owner, cardType, cards);
            }
            else if (show == 2)
                return Stargazer.NewClose();
            else if (show == 3)
            {
                int viewCount = int.Parse(g2fu[2]);
                ushort[] views = Util.TakeRange(g2fu, 3, 3 + viewCount)
                    .Select(p => ushort.Parse(p)).ToArray();
                char cardType = g2fu[3 + viewCount][0];
                ushort[] cards = Util.TakeRange(g2fu, 4, g2fu.Length)
                    .Select(p => ushort.Parse(p)).ToArray();
                return Stargazer.NewTakeAway(views, cardType, cards);
            }
            else return null;
        }
        public override void Handle(XI xi)
        {
            int? show = GetInt("show");
            if (show == 0)
            {
                ushort op = GetUshort("operater");
                char cardType = GetString("cardType")[0];
                List<ushort> cards = GetUshortArray("cards");
                ushort[] invs = (GetUshortArray("views") ?? xi.Board.Garden.Keys).ToArray();
                if (op != 0)
                    xi.WI.Send("E0FU,0,2," + cardType + "," + string.Join(",", cards), 0, op);
                xi.WI.Send("E0FU,0,0," + cardType + "," + string.Join(",", cards),
                    invs.Except(new ushort[] { op }).ToArray());
                xi.WI.Send("E0FU,0,1," + cardType + "," + cards.Count, xi.ExceptStaff(invs));
                xi.WI.Live("E0FU,0,1," + cardType + "," + cards.Count);
            }
            else if (show == 1)
            {
                ushort owner = GetUshort("owner");
                char cardType = GetString("cardType")[0];
                List<ushort> cards = GetUshortArray("cards");
                xi.WI.BCast("E0FU,1," + owner + "," + cardType + "," + string.Join(",", cards));
            }
            else if (show == 2)
                xi.WI.BCast("E0FU,2");
            else if (show == 3)
            {
                List<ushort> views = GetUshortArray("views");
                char cardType = GetString("cardType")[0];
                List<ushort> cards = GetUshortArray("cards");
                ushort[] invs = (GetUshortArray("views") ?? xi.Board.Garden.Keys).ToArray();
                xi.WI.Send("E0FU,3,0," + cardType + "," + string.Join(",", cards), invs);
                xi.WI.Send("E0FU,3,1," + cardType + "," + cards.Count, xi.ExceptStaff(invs));
                xi.WI.Live("E0FU,3,1," + cardType + "," + cards.Count);
            }
        }
    }

    public class MoonlightFade : Mint
    {
    	public override string Head { get { return "G2AS"; } }
    	public override MintType MintType { get { return MintType.UI_ONLY; }}

    	public override string ToMessage() { return Head + ",0"; }
        public static new MoonlightFade Parse(string message) { return new MoonlightFade(); }
    }

    public class HeavyRotation : Mint
    {
        public override string Head { get { return "G0HR"; } }
        public override MintType MintType { get { return MintType.GENERAL; } }

        private HeavyRotation() : base() { }
        public static HeavyRotation NewReset()
        {
            return new HeavyRotation().Set("op", "reset") as HeavyRotation;
        }
        public static HeavyRotation NewRotate()
        {
            return new HeavyRotation().Set("op", "rotate") as HeavyRotation;
        }
        public static HeavyRotation NewSet(bool isClockWised)
        {
            return new HeavyRotation().Set("op", "set").Set("cwval", isClockWised) as HeavyRotation;
        }
        public override string ToMessage()
        {
            string op = GetString("op");
            if (op == "reset")
                return Head + ",0,0";
            else if (op == "rotate")
                return Head + ",0,1";
            else if (op == "set")
                return Head + ",1," + (GetBool("cwval") != false ? 1 : 0);
            return "";
        }
        public static new HeavyRotation Parse(string message)
        {
            string[] g0hr = message.Split(',');
            if (g0hr[1] == "0")
            {
                if (g0hr[2] == "0")
                    return HeavyRotation.NewReset();
                else if (g0hr[2] == "1")
                    return HeavyRotation.NewRotate();
            }
            else if (g0hr[1] == "1")
                return HeavyRotation.NewSet(g0hr[2] == "0");
            return null;
        }
        public override void Handle(XI xi)
        {
            string op = GetString("op");
            bool oldIsCW = xi.Board.ClockWised;
            if (op == "reset")
                xi.Board.ClockWised = true;
            else if (op == "rotate")
                xi.Board.ClockWised = !xi.Board.ClockWised;
            else if (op == "set")
                xi.Board.ClockWised = (GetBool("cwval") != false);
            if (oldIsCW != xi.Board.ClockWised)
                xi.WI.BCast("E0HR," + (xi.Board.ClockWised ? 0 : 1));
        }
    }
}
