using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.PSDGamepkg.Mint
{
	public enum MintType
	{
		NONE, 
		NORMAL, // G0
		INNER, // G1, won't cause E communication directly
		UI_ONLY // G2, trigger E communcation directly
	};

	public class Mint
	{
		private IDictionary<string, object> dict = new Dictionary<string, object>();

		public virtual string Head { get { return null; } }
		public virtual MintType MintType { get { return MintType.NONE; } }

		public void Set(string key, object value) { dict[key] = value; }
		// public void SetUshort(string key, ushort value) { dict[key] = value; }
		// public void SetInt(string key, int value) { dict[key] = value; }
		// public void SetFloat(String key, float value) { dict[key] = value; }
		// public void SetString(string key, string value) { dict[key] = value; }
		// public void SetBool(string key, bool value) { dict[key] = value; }
		// public void SetMint(string key, Mint value) { dict[key] = value; }
		// public void SetArray<Type>(String key, List<Type> value) { dict[key] = value; }

		public ushort GetUshort(string key) { return dict.ContainsKey(key) ? (ushort)dict[key] : (ushort)0; }
		public int? GetInt(string key) { return dict.ContainsKey(key) ? (int?) dict[key] : null; }
		//public float? GetFloat(string key) { return dict.ContainsKey(key) ? (float) dict[key] : null; }
		public string GetString(string key) { return dict.ContainsKey(key) ? (string) dict[key] : null; }
		public bool? GetBool(string key) { return dict.ContainsKey(key) ? (bool?) dict[key] : null; }
		public Mint GetMint(string key) { return dict.ContainsKey(key) ? (Mint) dict[key] : null; }

		public List<ushort> GetUshortArray(String key)
		{
			return dict.ContainsKey(key) ? (List<ushort>) dict[key] : null;
		}
		public List<int> GetIntArray(String key)
		{
			return dict.ContainsKey(key) ? (List<int>) dict[key] : null;
		}
		// public List<float> GetFloatArray(String key)
		// {
		// 	return dict.ContainsKey(key) ? (List<float>) dict[key] : null;
		// }
		public List<string> GetStringArray(String key)
		{
			return dict.ContainsKey(key) ? (List<string>) dict[key] : null;
		}
		public List<bool> GetBoolArray(String key)
		{
			return dict.ContainsKey(key) ? (List<bool>) dict[key] : null;
		}
		public List<Mint> GetMintArray(String key)
		{
			return dict.ContainsKey(key) ? (List<Mint>) dict[key] : null;
		}

		public Mint() { }
		public Mint(params object[] pairs)
		{
			for (int i = 0; i < pairs.Length; i += 2)
			{
				string key = pairs[i] as string;
				object value = pairs[i + 1];
				Set(key, value);
			}
		}
		public virtual string ToMessage() { return "****"; }
		public virtual void Handle(XI xi)
		{
			if (MintType == MintType.UI_ONLY)
				xi.WI.BCast("E0" + ToMessage().Substring("G2".Length));
			else
				xi.RaiseGMessage(ToMessage()); // TODO: Gradually refurbish it
		}
	}

	public class Target : Mint
	{
		public override string Head { get { return "G2SY"; } }
		public override MintType MintType { get { return MintType.UI_ONLY; } }

        public Target(char fromChar, ushort fromUt, char toChar, ushort toUt)
            : base()
        {
            Set("head", Head);
            Set("from", new Mint("type", fromChar.ToString(), "ut", fromUt));
            Set("to", new Mint[] { new Mint("type", toChar.ToString(), "ut", toUt) }.ToList());
        }
        public Target(char fromChar, ushort fromUt, IEnumerable<ushort> toTargetUts)
            : base()
        {
            Set("head", Head);
            Set("from", new Mint("type", fromChar.ToString(), "ut", fromUt));
            Set("to", toTargetUts.Select(p => new Mint("type", "T", "ut", p)).ToList());
        }
        private Target(char fromChar, ushort fromUt, object[] tos)
            : base()
        {
            Set("head", Head);
            Set("from", new Mint("type", fromChar.ToString(), "ut", fromUt));
            List<Mint> toMints = new List<Mint>();
            for (int i = 0; i < tos.Length; i += 2)
                toMints.Add(new Mint("type", tos[i], "ut", tos[i + 1]));
            Set("to", toMints);
        }
		public override string ToMessage()
		{
			return Head + "," + GetMint("from").GetString("type") + GetMint("from").GetString("ut")
				+ "," + string.Join(",", GetMintArray("to").Select(p => p.GetString("type") + p.GetString("ut")));
		}
		public static Target Parse(string message)
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
		public override MintType MintType { get { return MintType.UI_ONLY; }}
		private Stargazer() : base() { }
		// case 1: standard
		public static Stargazer NewStandard(ushort operater,
			IEnumerable<ushort> views, char cardType, IEnumerable<ushort> cards)
		{
			Stargazer sg = new Stargazer();
			sg.Set("show", 0);
			sg.Set("operator", operater);
            if (views != null)
			    sg.Set("views", views.ToList());
			sg.Set("cardType", cardType.ToString());
			sg.Set("cards", cards.ToList());
			return sg;
		}
		// case 2: show
		public static Stargazer NewShow(ushort owner, char cardType, IEnumerable<ushort> cards)
		{
			Stargazer sg = new Stargazer();
			sg.Set("show", 1);
			sg.Set("owner", owner);
			sg.Set("cardType", cardType.ToString());
			sg.Set("cards", cards.ToList());
			return sg;
		}
        public static Mint NewShow(ushort owner, char cardType, ushort card)
        {
            return NewShow(owner, cardType, new ushort[] { card });
        }
		// case 3: close
		public static Stargazer NewClose()
		{
			Stargazer sg = new Stargazer();
			sg.Set("show", 2);
			return sg;
		}
		// case 4: take away
		public static Stargazer NewTakeAway(IEnumerable<ushort> views, char cardType, IEnumerable<ushort> cards)
		{
			Stargazer sg = new Stargazer();
            sg.Set("show", -1);
            if (views != null)
			    sg.Set("views", views.ToList());
			sg.Set("cardType", cardType.ToString());
			sg.Set("cards", cards.ToList());
			return sg;
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
		public static Stargazer Parse(string message)
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
}