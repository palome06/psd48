using PSD.Base;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.Artiad
{
    public class CoachingChange
    {
        public List<CoachingChangeUnit> List { set; get; }
        public CoachingChangeUnit SingleUnit
        {
            set { List = new List<CoachingChangeUnit>() { value }; }
        }
        public string ToMessage() { return "G0FI," + string.Join(",", List.Select(p => p.ToRawMessage())); }
        public static CoachingChange Parse(string line)
        {
            return new CoachingChange() { List = CoachingChangeUnit.ParseFromLine(line) };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            List.ForEach(p => p.Solve(XI));
            new CoachingChangeSemaphore() { List = List }.Telegraph(WI.BCast);
            if (XI.Board.PoolEnabled)
                XI.RaiseGMessage("G09P,0");
        }
        // return positive->attend;negative->leave;0->nothing
        public int AttendOrLeave(ushort ut)
        {
            int result = 0;
            List.ForEach(p =>
            {
                if (p.Role == CoachingChangeUnit.PType.SUPPORTER || p.Role == CoachingChangeUnit.PType.HINDER)
                {
                    if (p.Elder == ut) { --result; }
                    else if (p.Stepper == ut) { ++result; }
                }
                else if (p.Role == CoachingChangeUnit.PType.EX_ENTER)
                {
                    if (p.Stepper == ut) { ++result; }
                }
                else if (p.Role == CoachingChangeUnit.PType.EX_EXIT)
                {
                    if (p.Elder == ut) { --result; }
                }
            });
            return result;
        }
    }

    public class CoachingChangeUnit
    {
        public enum PType { GIVEUP, DONE, TRIGGER, SUPPORTER, HINDER, HORN, EX_ENTER, EX_EXIT, REFRESH };
        // The change position type
        public PType Role { set; get; }
        // The one original in the position
        public ushort Elder { set; get; }
        // The one new in the position
        public ushort Stepper { set; get; }

        internal string ToRawMessage()
        {
            switch (Role)
            {
                case PType.GIVEUP: return "O";
                case PType.DONE: return "U," + Elder;
                case PType.TRIGGER: return "T," + Elder + "," + Stepper;
                case PType.SUPPORTER: return "S," + Elder + "," + Stepper;
                case PType.HINDER: return "H," + Elder + "," + Stepper;
                case PType.HORN: return "W," + Elder + "," + Stepper;
                case PType.EX_ENTER: return "C," + Stepper;
                case PType.EX_EXIT: return "D," + Elder;
                case PType.REFRESH: return "R," + Elder;
                default: return "";
            }
        }
        internal static List<CoachingChangeUnit> ParseFromLine(string line)
        {
            List<CoachingChangeUnit> ccs = new List<CoachingChangeUnit>();
            string[] g0fi = line.Split(',');
            for (int idx = 1; idx < g0fi.Length;)
            {
                string role = g0fi[idx++];
                switch (role)
                {
                    case "O":
                        ccs.Add(new CoachingChangeUnit() { Role = PType.GIVEUP }); break;
                    case "U":
                        ccs.Add(new CoachingChangeUnit() { Role = PType.DONE, Elder = ushort.Parse(g0fi[idx++]) }); break;
                    case "T":
                        ccs.Add(new CoachingChangeUnit()
                        {
                            Role = PType.TRIGGER,
                            Elder = ushort.Parse(g0fi[idx++]),
                            Stepper = ushort.Parse(g0fi[idx++])
                        }); break;
                    case "S":
                        ccs.Add(new CoachingChangeUnit()
                        {
                            Role = PType.SUPPORTER,
                            Elder = ushort.Parse(g0fi[idx++]),
                            Stepper = ushort.Parse(g0fi[idx++])
                        }); break;
                    case "H":
                        ccs.Add(new CoachingChangeUnit()
                        {
                            Role = PType.HINDER,
                            Elder = ushort.Parse(g0fi[idx++]),
                            Stepper = ushort.Parse(g0fi[idx++])
                        }); break;
                    case "W":
                        ccs.Add(new CoachingChangeUnit()
                        {
                            Role = PType.HORN,
                            Elder = ushort.Parse(g0fi[idx++]),
                            Stepper = ushort.Parse(g0fi[idx++])
                        }); break;
                    case "C":
                        ccs.Add(new CoachingChangeUnit() { Role = PType.EX_ENTER, Stepper = ushort.Parse(g0fi[idx++]) }); break;
                    case "D":
                        ccs.Add(new CoachingChangeUnit() { Role = PType.EX_EXIT, Elder = ushort.Parse(g0fi[idx++]) }); break;
                    case "R":
                        ccs.Add(new CoachingChangeUnit() { Role = PType.REFRESH, Elder = ushort.Parse(g0fi[idx++]) }); break;
                    default: break;
                }
            }
            return ccs;
        }
        internal void Solve(XI XI)
        {
            // TRIGGER/HORN/REFRESH
            if (Role == PType.SUPPORTER)
                XI.Board.Supporter = Artiad.ContentRule.DecodePlayer(Stepper, XI);
            else if (Role == PType.HINDER)
                XI.Board.Hinder = Artiad.ContentRule.DecodePlayer(Stepper, XI);
            else if (Role == PType.EX_ENTER)
            {
                Player py = Artiad.ContentRule.DecodePlayer(Stepper, XI);
                if (py != null)
                {
                    if (py.Team == XI.Board.Rounder.Team && !XI.Board.RDrums.ContainsKey(py))
                        XI.Board.RDrums[py] = false;
                    else if (py.Team == XI.Board.Rounder.OppTeam && !XI.Board.ODrums.ContainsKey(py))
                        XI.Board.ODrums[py] = false;
                }
            }
            else if (Role == PType.EX_EXIT)
            {
                Player py = Artiad.ContentRule.DecodePlayer(Elder, XI);
                if (py != null)
                {
                    if (py.Team == XI.Board.Rounder.Team && XI.Board.RDrums.ContainsKey(py))
                        XI.Board.RDrums.Remove(py);
                    else if (py.Team == XI.Board.Rounder.OppTeam && XI.Board.ODrums.ContainsKey(py))
                        XI.Board.ODrums.Remove(py);
                }
            }
        }
    }
    
    public class CoachingChangeSemaphore
    {
        public List<CoachingChangeUnit> List { set; get; }

        public void Telegraph(System.Action<string> send)
        {
            if (List != null && List.Count > 0)
            send("E0FI," + string.Join(",", List.Select(p => p.ToRawMessage())));
        }
    }
}
