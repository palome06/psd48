using PSD.Base;
using PSD.Base.Utils;
using System.Linq;

// Token issue
namespace PSD.PSDGamepkg.Artiad
{
    public class IncrTokenCount
    {
        public ushort Who { set; get; }
        public int Delta { set; get; }

        public string ToMessage() { return "G0IJ," + Who + ",0," + Delta; }
        public static IncrTokenCount Parse(string line)
        {
            string[] g0ij = line.Split(',');
            return new IncrTokenCount()
            {
                Who = ushort.Parse(g0ij[1]),
                Delta = int.Parse(g0ij[3])
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenCount += Delta;
            WI.BCast("E0IJ," + Who + ",0," + Delta + "," + py.TokenCount);
        }
    }

    public class DecrTokenCount
    {
        public ushort Who { set; get; }
        public int Delta { set; get; }

        public string ToMessage() { return "G0OJ," + Who + ",0," + Delta; }
        public static DecrTokenCount Parse(string line)
        {
            string[] g0oj = line.Split(',');
            return new DecrTokenCount()
            {
                Who = ushort.Parse(g0oj[1]),
                Delta = int.Parse(g0oj[3])
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            if (Delta > py.TokenCount)
                Delta = py.TokenCount;
            py.TokenCount -= Delta;
            WI.BCast("E0OJ," + Who + ",0," + Delta + "," + py.TokenCount);
        }
    }

    public class IncrTokenExcl
    {
        public ushort Who { set; get; }
        public string[] Delta { set; get; }
        public string SingleDelta
        {
            set { Delta = new string[] { value }; }
            get { return Delta.Length == 1 ? Delta[0] : ""; }
        }
        // whether gain from public or not
        public bool Public { set; get; }

        public IncrTokenExcl() { Public = false; }
        public string ToMessage()
        {
            return "G0IJ," + Who + ",1," + Algo.ListToString(Delta) + "," + (Public ? "1" : "0");
        }
        public static IncrTokenExcl Parse(string line)
        {
            string[] g0ij = line.Split(',');
            int n = int.Parse(g0ij[3]);
            return new IncrTokenExcl()
            {
                Who = ushort.Parse(g0ij[1]),
                Delta = Algo.TakeRange(g0ij, 4, 4 + n),
                Public = g0ij[4 + n] == "1"
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenExcl.AddRange(Delta);
            WI.BCast("E0IJ," + Who + ",1," + Algo.ListToString(Delta) +
                "," + Algo.ListToString(py.TokenExcl));
            if (Public)
                XI.RaiseGMessage("G2TZ," + Who + ",0," + string.Join(",", Delta));
        }
    }

    public class DecrTokenExcl
    {
        public ushort Who { set; get; }
        public string[] Delta { set; get; }
        public string SingleDelta
        {
            set { Delta = new string[] { value }; }
            get { return Delta.Length == 1 ? Delta[0] : ""; }
        }
        // whether lose to public or not
        public bool Public { set; get; }

        public DecrTokenExcl() { Public = false; }
        public string ToMessage()
        {
            return "G0OJ," + Who + ",1," + Algo.ListToString(Delta) + "," + (Public ? "1" : "0");
        }
        public static DecrTokenExcl Parse(string line)
        {
            string[] g0oj = line.Split(',');
            int n = int.Parse(g0oj[3]);
            return new DecrTokenExcl()
            {
                Who = ushort.Parse(g0oj[1]),
                Delta = Algo.TakeRange(g0oj, 4, 4 + n),
                Public = g0oj[4 + n] == "1"
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenExcl.RemoveAll(p => Delta.Contains(p));
            WI.BCast("E0OJ," + Who + ",1," + Algo.ListToString(Delta) +
                "," + Algo.ListToString(py.TokenExcl));
            if (Public)
                XI.RaiseGMessage("G2TZ,0," + Who + "," + string.Join(",", Delta));
        }
    }

    public class IncrTokenTar
    {
        public ushort Who { set; get; }
        public ushort[] Tars { set; get; }
        public ushort SingleTar
        {
            set { Tars = new ushort[] { value }; }
            get { return Tars.Length == 1 ? Tars[0] : (ushort)0; }
        }

        public string ToMessage() { return "G0IJ," + Who + ",2," + Algo.ListToString(Tars); }
        public static IncrTokenTar Parse(string line)
        {
            string[] g0ij = line.Split(',');
            int next;
            return new IncrTokenTar()
            {
                Who = ushort.Parse(g0ij[1]),
                Tars = Algo.TakeArrayWithSize(g0ij, 3, out next)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenTars.AddRange(Tars);
            WI.BCast("E0IJ," + Who + ",2," + Algo.ListToString(Tars) +
                "," + Algo.ListToString(py.TokenTars));
        }
    }
        
    public class DecrTokenTar
    {
        public ushort Who { set; get; }
        public ushort[] Tars { set; get; }
        public ushort SingleTar
        {
            set { Tars = new ushort[] { value }; }
            get { return Tars.Length == 1 ? Tars[0] : (ushort)0; }
        }

        public string ToMessage() { return "G0OJ," + Who + ",2," + Algo.ListToString(Tars); }
        public static DecrTokenTar Parse(string line)
        {
            string[] g0oj = line.Split(',');
            int next;
            return new DecrTokenTar()
            {
                Who = ushort.Parse(g0oj[1]),
                Tars = Algo.TakeArrayWithSize(g0oj, 3, out next)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenTars.RemoveAll(p => Tars.Contains(p));
            WI.BCast("E0OJ," + Who + ",2," + Algo.ListToString(Tars) +
                "," + Algo.ListToString(py.TokenTars));
        }
    }

    public class IncrTokenAwake
    {
        public ushort Who { set; get; }

        public string ToMessage() { return "G0IJ," + Who + ",3"; }
        public static IncrTokenAwake Parse(string line)
        {
            string[] g0ij = line.Split(',');
            return new IncrTokenAwake()
            {
                Who = ushort.Parse(g0ij[1]),
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenAwake = true;
            WI.BCast("E0IJ," + Who + ",3");
        }
    }

    public class DecrTokenAwake
    {
        public ushort Who { set; get; }

        public string ToMessage() { return "G0OJ," + Who + ",3"; }
        public static DecrTokenAwake Parse(string line)
        {
            string[] g0oj = line.Split(',');
            return new DecrTokenAwake()
            {
                Who = ushort.Parse(g0oj[1]),
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenAwake = false;
            WI.BCast("E0OJ," + Who + ",3");
        }
    }

    public class IncrTokenFold
    {
        public ushort Who { set; get; }
        public ushort[] Delta { set; get; }
        public ushort SingleDelta
        {
            set { Delta = new ushort[] { value }; }
            get { return Delta.Length == 1 ? Delta[0] : (ushort)0; }
        }

        public string ToMessage() { return "G0IJ," + Who + ",4," + Algo.ListToString(Delta); }
        public static IncrTokenFold Parse(string line)
        {
            string[] g0ij = line.Split(',');
            int next;
            return new IncrTokenFold()
            {
                Who = ushort.Parse(g0ij[1]),
                Delta = Algo.TakeArrayWithSize(g0ij, 3, out next)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenFold.AddRange(Delta);
            WI.Focus(Who, "E0IJ," + Who + ",4,0," + Algo.ListToString(Delta) + "," +
                Algo.ListToString(py.TokenFold), "E0IJ," + Who + ",4,1," + Delta.Length + "," + py.TokenFold.Count);
        }
    }

    public class DecrTokenFold
    {
        public ushort Who { set; get; }
        public ushort[] Delta { set; get; }
        public ushort SingleDelta
        {
            set { Delta = new ushort[] { value }; }
            get { return Delta.Length == 1 ? Delta[0] : (ushort)0; }
        }
        // whether lose to public or not
        public bool Public { set; get; }

        public string ToMessage() { return "G0OJ," + Who + ",4," + Algo.ListToString(Delta) + "," + (Public ? 1 : 0); }
        public static DecrTokenFold Parse(string line)
        {
            string[] g0oj = line.Split(',');
            int next;
            return new DecrTokenFold()
            {
                Who = ushort.Parse(g0oj[1]),
                Delta = Algo.TakeArrayWithSize(g0oj, 3, out next),
                Public = g0oj[next] == "1"
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player py = XI.Board.Garden[Who];
            py.TokenFold.RemoveAll(p => Delta.Contains(p));
            WI.Focus(Who, "E0OJ," + Who + ",4,0," + Algo.ListToString(Delta) + "," +
                Algo.ListToString(py.TokenFold), "E0OJ," + Who + ",4,1," + Delta.Length + "," + py.TokenFold.Count);
            if (Public)
                XI.RaiseGMessage("G2TZ,0," + Who + "," + string.Join(",", Delta));
        }
    }

    public static class LittleHelper
    {
        private static bool Is(string line, int type)
        {
            string[] g0ij = line.Split(',');
            return g0ij.Length >= 3 && g0ij[2] == type.ToString();
        }
        public static bool IsCount(string line) { return Is(line, 0); }
        public static bool IsExcl(string line) { return Is(line, 1); }
        public static bool IsTars(string line) { return Is(line, 2); }
        public static bool IsAwake(string line) { return Is(line, 3); }
        public static bool IsFold(string line) { return Is(line, 4); }
    }
}
