using PSD.Base;
using System.Collections.Generic;
using System.Linq;

namespace PSD.PSDGamepkg.Artiad
{
    // Sign the appearance of a attender
    public class CoachingSign
    {
        public List<CoachingSignUnit> List { set; get; }
        public CoachingSignUnit SingleUnit
        {
            set { List = new List<CoachingSignUnit>() { value }; }
        }

        public string ToMessage() { return "G17F," + string.Join(",", List.Select(p => p.ToRawMessage())); }
        public static CoachingSign Parse(string line)
        {
            return new CoachingSign() { List = CoachingSignUnit.ParseFromLine(line) };
        }
        public void Handle(XI XI)
        {
            if (List.Any(p => p.Role == CoachingHelper.PType.GIVEUP))
                XI.RaiseGMessage(new CoachingChange() { SingleUnit = new CoachingChangeUnit()
                {
                    Role = CoachingHelper.PType.GIVEUP
                } }.ToMessage());
            else if (List.Any(p => p.Role == CoachingHelper.PType.DONE)) // Just start the fight
            {
                XI.RaiseGMessage(new CoachingChange() { SingleUnit = new CoachingChangeUnit()
                {
                    Role = CoachingHelper.PType.DONE,
                    Elder = List.First().Coach
                } }.ToMessage());
            }
            else
            {
                ushort[] lists = new ushort[] { XI.Board.Rounder.Uid, XI.Board.Rounder.Uid,
                     XI.Board.Supporter.Uid, XI.Board.Supporter.Uid,
                     XI.Board.Hinder.Uid, XI.Board.Hinder.Uid, 0, 0 }; // TODO: Horn
                ISet<ushort> clist = new HashSet<ushort>();
                ISet<ushort> dlist = new HashSet<ushort>();
                ISet<ushort> rlist = new HashSet<ushort>();
                List.ForEach(p =>
                {
                    if (p.Role == CoachingHelper.PType.REFRESH)
                        rlist.Add(p.Coach);
                    else if (p.Coach == XI.Board.Rounder.Uid)
                        lists[1] = 0;
                    else if (p.Coach == XI.Board.Supporter.Uid)
                        lists[3] = 0;
                    else if (p.Coach == XI.Board.Hinder.Uid)
                        lists[5] = 0;
                    else if (p.Coach == XI.Board.Horn.Uid)
                        lists[7] = 0;
                    else if (XI.Board.DrumUts.Contains(p.Coach) && p.Role != CoachingHelper.PType.EX_ENTER)
                        dlist.Add(p.Coach);
                    else if (!XI.Board.DrumUts.Contains(p.Coach) && p.Role == CoachingHelper.PType.EX_ENTER)
                        clist.Add(p.Coach);

                    if (p.Role == CoachingHelper.PType.TRIGGER && lists[1] != p.Coach)
                        lists[1] = p.Coach;
                    else if (p.Role == CoachingHelper.PType.SUPPORTER && lists[3] != p.Coach)
                        lists[3] = p.Coach;
                    else if (p.Role == CoachingHelper.PType.HINDER && lists[5] != p.Coach)
                        lists[5] = p.Coach;
                    else if (p.Role == CoachingHelper.PType.HORN && lists[7] != p.Coach)
                        lists[7] = p.Coach;
                });
                List<CoachingChangeUnit> ccus = new List<CoachingChangeUnit>();
                if (lists[0] != lists[1])
                    ccus.Add(new CoachingChangeUnit()
                    {
                        Role = CoachingHelper.PType.TRIGGER,
                        Elder = lists[0],
                        Stepper = lists[1]
                    });
                if (lists[2] != lists[3])
                    ccus.Add(new CoachingChangeUnit()
                    {
                        Role = CoachingHelper.PType.SUPPORTER,
                        Elder = lists[2],
                        Stepper = lists[3]
                    });
                if (lists[4] != lists[5])
                    ccus.Add(new CoachingChangeUnit()
                    {
                        Role = CoachingHelper.PType.HINDER,
                        Elder = lists[4],
                        Stepper = lists[5]
                    });
                if (lists[6] != lists[7])
                    ccus.Add(new CoachingChangeUnit()
                    {
                        Role = CoachingHelper.PType.HORN,
                        Elder = lists[6],
                        Stepper = lists[7]
                    });
                ccus.AddRange(clist.Select(p => new CoachingChangeUnit()
                {
                    Role = CoachingHelper.PType.EX_ENTER,
                    Stepper = p
                }));
                ccus.AddRange(dlist.Select(p => new CoachingChangeUnit()
                {
                    Role = CoachingHelper.PType.EX_EXIT,
                    Elder = p
                }));
                ccus.AddRange(rlist.Select(p => new CoachingChangeUnit()
                {
                    Role = CoachingHelper.PType.REFRESH,
                    Elder = p
                }));

                if (ccus.Count > 0)
                    XI.RaiseGMessage(new CoachingChange() { List = ccus }.ToMessage());
            }
        }
    }

    public class CoachingSignUnit
    {
        public CoachingHelper.PType Role { set; get; }
        public ushort Coach { set; get; }

        internal string ToRawMessage() { return CoachingHelper.PType2Char(Role) + "," + Coach; }
        internal static List<CoachingSignUnit> ParseFromLine(string line)
        {
            string[] g17f = line.Split(',');
            List<CoachingSignUnit> ccus = new List<CoachingSignUnit>();
            for (int i = 1; i < g17f.Length; i += 2)
                ccus.Add(new CoachingSignUnit()
                {
                    Role = CoachingHelper.Char2PType(g17f[i][0]),
                    Coach = ushort.Parse(g17f[i + 1])
                });
            return ccus;
        }
    }
    // Report the actual change/substitution in each position
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
                XI.RaiseGMessage(new Artiad.PondRefresh() { CheckHit = true }.ToMessage());
        }
        // return positive->attend;negative->leave;0->nothing
        public int AttendOrLeave(ushort ut)
        {
            int result = 0;
            List.ForEach(p =>
            {
                if (p.Role == CoachingHelper.PType.SUPPORTER || p.Role == CoachingHelper.PType.HINDER)
                {
                    if (p.Elder == ut) { --result; }
                    else if (p.Stepper == ut) { ++result; }
                }
                else if (p.Role == CoachingHelper.PType.EX_ENTER)
                {
                    if (p.Stepper == ut) { ++result; }
                }
                else if (p.Role == CoachingHelper.PType.EX_EXIT)
                {
                    if (p.Elder == ut) { --result; }
                }
            });
            return result;
        }
    }

    public class CoachingChangeUnit
    {
        // The change position type
        public CoachingHelper.PType Role { set; get; }
        // The one original in the position
        public ushort Elder { set; get; }
        // The one new in the position
        public ushort Stepper { set; get; }

        internal string ToRawMessage()
        {
            string ch = CoachingHelper.PType2Char(Role).ToString();
            switch (Role)
            {
                case CoachingHelper.PType.GIVEUP: return ch.ToString();
                case CoachingHelper.PType.DONE: case CoachingHelper.PType.EX_EXIT: case CoachingHelper.PType.REFRESH:
                    return ch + "," + Elder;
                case CoachingHelper.PType.TRIGGER: case CoachingHelper.PType.SUPPORTER:
                case CoachingHelper.PType.HINDER: case CoachingHelper.PType.HORN:
                    return ch + "," + Elder + "," + Stepper;
                case CoachingHelper.PType.EX_ENTER:
                    return ch + "," + Stepper;
                default: return "";
            }
        }
        internal static List<CoachingChangeUnit> ParseFromLine(string line)
        {
            List<CoachingChangeUnit> ccs = new List<CoachingChangeUnit>();
            string[] g0fi = line.Split(',');
            for (int idx = 1; idx < g0fi.Length;)
            {
                CoachingHelper.PType role = CoachingHelper.Char2PType(g0fi[idx++][0]);
                switch (role)
                {
                    case CoachingHelper.PType.GIVEUP: ccs.Add(new CoachingChangeUnit() { Role = role }); break;
                    case CoachingHelper.PType.DONE: case CoachingHelper.PType.EX_EXIT: case CoachingHelper.PType.REFRESH:
                        ccs.Add(new CoachingChangeUnit() { Role = role, Elder = ushort.Parse(g0fi[idx++]) }); break;
                    case CoachingHelper.PType.TRIGGER: case CoachingHelper.PType.SUPPORTER:
                    case CoachingHelper.PType.HINDER: case CoachingHelper.PType.HORN:
                        ccs.Add(new CoachingChangeUnit()
                        {
                            Role = role,
                            Elder = ushort.Parse(g0fi[idx++]),
                            Stepper = ushort.Parse(g0fi[idx++])
                        }); break;
                    case CoachingHelper.PType.EX_ENTER:
                        ccs.Add(new CoachingChangeUnit() { Role = role, Stepper = ushort.Parse(g0fi[idx++]) }); break;
                }
            }
            return ccs;
        }
        internal void Solve(XI XI)
        {
            // TRIGGER/HORN/REFRESH
            if (Role == CoachingHelper.PType.SUPPORTER)
                XI.Board.Supporter = Artiad.ContentRule.DecodePlayer(Stepper, XI);
            else if (Role == CoachingHelper.PType.HINDER)
                XI.Board.Hinder = Artiad.ContentRule.DecodePlayer(Stepper, XI);
            else if (Role == CoachingHelper.PType.EX_ENTER)
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
            else if (Role == CoachingHelper.PType.EX_EXIT)
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

    public class CoachingHelper
    {
        public enum PType { NIL, GIVEUP, DONE, TRIGGER, SUPPORTER, HINDER, HORN, EX_ENTER, EX_EXIT, REFRESH };
        private static readonly char[] PTypeChar = { '/', 'O', 'U', 'T', 'S', 'H', 'W', 'C', 'D', 'R' };

        internal static char PType2Char(PType role)
        {
            return PTypeChar[(int)role];
        }
        internal static PType Char2PType(char ch)
        {
            switch (ch)
            {
                case 'O': return PType.GIVEUP;
                case 'U': return PType.DONE;
                case 'T': return PType.TRIGGER;
                case 'S': return PType.SUPPORTER;
                case 'H': return PType.HINDER;
                case 'W': return PType.HORN;
                case 'C': return PType.EX_ENTER;
                case 'D': return PType.EX_EXIT;
                case 'R': return PType.REFRESH;
                default: return PType.NIL;
            }
        }
    }
}
