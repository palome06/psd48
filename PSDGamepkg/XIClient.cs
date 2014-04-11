using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg
{
    public class XIClient
    {
        #region Basic Members
        public Base.VW.IVI VI { private set; get; }
        public Base.VW.IWI WI { private set; get; }

        private LibTuple tuple;

        private readonly ushort uid;

        public IDictionary<ushort, int> Heros { set; private get; }
        // list of running threads handling events
        private List<Thread> listOfThreads;

        public XIClient(ushort uid, int count, Base.VW.IWI wi, Base.VW.IVI vi, LibTuple tuple)
        {
            this.uid = uid;
            WI = wi;
            VI = vi;
            this.tuple = tuple;
            listOfThreads = new List<Thread>();
            sk01 = new JNS.SkillCottage(null, null).Sample();
            cz01 = new JNS.OperationCottage(null, null).Sample();
            nj01 = new JNS.NPCCottage(null, null).Sample();
        }

        // Hero selection proceeding
        public void RunAsync()
        {
            new Thread(delegate()
            {
                while (true)
                {
                    string readLine = WI.Recv(uid, 0);
                    if (!string.IsNullOrEmpty(readLine))
                    {
                        // VI.Cout(uid, "★●▲■" + readLine + "★●▲■");
                        if (readLine.StartsWith("E0"))
                            HMMain(readLine);
                        else
                        {
                            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(HMMain);
                            Thread myThread = new Thread(ParStart);
                            lock (listOfThreads)
                            {
                                if (listOfThreads.Count > 100)
                                {
                                    List<Thread> nt = listOfThreads.Where(p => p.IsAlive).ToList();
                                    listOfThreads.Clear();
                                    listOfThreads.AddRange(nt);
                                }
                                listOfThreads.Add(myThread);
                            }
                            myThread.Start(readLine);
                        }
                    }
                    Thread.Sleep(100);
                }
            }).Start();
        }
        #endregion Basic Members

        private void HMMain(object pararu)
        {
            string readLine = (string)pararu;
            // start a new thread to handle with the message
            int cdx = readLine.IndexOf(',');            
            string cop = Util.Substring(readLine, 0, cdx);
            if (cop.StartsWith("E0"))
                HandleE0Message(readLine);
            if (cop.StartsWith("F0"))
                HandleF0Message(readLine);
            else if (cop.StartsWith("U"))
            {
                char rank = cop[1];
                string[] blocks = Util.Splits(readLine.Substring("U1,".Length), ";;");
                switch (rank)
                {
                    case '1':
                        HandleU1Message(blocks[0], blocks[1]); break;
                    case '3':
                        HandleU3Message(blocks[0], blocks[1], blocks[2]); break;
                    case '5':
                        HandleU5Message(blocks[0], blocks[1], blocks[2]); break;
                    case '7':
                        HandleU7Message(blocks[0], blocks[1], blocks[2], blocks[3]); break;
                    case '9':
                        HandleU9Message(blocks[0], blocks[1], blocks[2]); break;
                    case 'A':
                        HandleUAMessage(blocks[0], blocks[1], blocks[2]); break;
                    case 'B':
                        VI.Cout(uid, "您不可取消行动."); break;
                }
            }
            else if (cop.StartsWith("R"))
                HandleRMessage(readLine);
            else if (cop.StartsWith("V0"))
            {
                VI.OpenCinTunnel(uid);
                string input = FormattedInputWithCancelFlag(
                    readLine.Substring("V0,".Length));
                VI.CloseCinTunnel(uid);
                WI.Send("V1," + input, uid, 0);
            }
            else
            {
                switch (cop)
                {
                    case "H0RM":
                        OnNotifyHero(readLine.Substring(cdx + 1)); break;
                    case "H0SL":
                        OnDecideHero(readLine.Substring(cdx + 1)); break;
                    case "H0ST":
                        if (readLine["H0ST,".Length] == '0')
                        {
                            VI.Cout(uid, "游戏开始阶段开始...");
                            //VI.ReleaseCin(uid);
                        }
                        else if (readLine["H0ST,".Length] == '1')
                        {
                            VI.Cout(uid, "游戏开始阶段结束...");
                            //VI.ReleaseCin(uid);
                        }
                        break;
                }
            }
        }

        #region Format Input

        private string FormattedInputWithCancelFlag(string line)
        {
            if (string.IsNullOrEmpty(line))
                return "";
            string output = "";
            string prevComment = "";
            foreach (string block in Util.Splits(line, ","))
            {
                string arg = block;
                string cancel = "";
                string roundInput = "";
                if (block.StartsWith("/"))
                {
                    if (block.Equals("//"))
                    {
                        VI.Cin(uid, "请按任意键继续.");
                        roundInput = "0";
                    }
                    else if (block.Length > 1)
                    {
                        arg = block.Substring(1);
                        cancel = "(0为取消发动)";
                    }
                    else
                    {
                        VI.Cin(uid, "不能指定合法目标.");
                        roundInput = "0";
                    }
                }
                if (arg.StartsWith("#"))
                {
                    prevComment = arg.Substring(1); continue;
                }
                if (arg[0] == 'T')
                {
                    // format T1~2(p1p3p5),T1(p1p3),#Text
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    // TODO: handle with AND of multiple condition
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            // TODO: consider of empty bracket
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.Cin(uid, "请选择{0}至{1}名角色为{2}目标，可选{3}{4}.", r1, r2, prevComment, DisplayPlayer(uss), cancel);
                        }
                        else
                            input = VI.Cin(uid, "请选择{0}至{1}名角色为{2}目标{3}.", r1, r2, prevComment, cancel);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.Cin(uid, "请选择{0}名角色为{1}目标，可选{2}{3}.", r, prevComment, DisplayPlayer(uss), cancel);
                        }
                        else
                            input = VI.Cin(uid, "请选择{0}名角色为{1}目标{2}.", r, prevComment, cancel);
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'C')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.Cin(uid, "请选择{0}至{1}张卡牌为{2}目标，可选{3}{4}.", r1, r2, prevComment, DisplayTux(uss), cancel);
                        }
                        else
                            input = VI.Cin(uid, "请选择{0}至{1}张卡牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.Cin(uid, "请选择{0}张卡牌为{1}目标，可选{2}{3}.", r, prevComment, DisplayTux(uss), cancel);
                        }
                        else
                            input = VI.Cin(uid, "请选择{0}张卡牌为{1}目标{2}.", r, prevComment, cancel);
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'Z')
                {
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    int r = int.Parse(Substring(arg, 1, jdx));
                    if (jdx >= 0)
                    {
                        string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                        var uss = argv.Select(p => ushort.Parse(p));
                        input = VI.Cin(uid, "请选择{0}张公共卡牌为{1}目标，可选{2}{3}.", r, prevComment, DisplayTux(uss), cancel);
                    }
                    else
                        input = VI.Cin(uid, "请选择{0}张公共卡牌为{1}目标{2}.", r, prevComment, cancel);
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'M')
                {
                    int idx = arg.IndexOf('~');
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string input;
                    if (idx >= 1)
                    {
                        int r1 = int.Parse(Substring(arg, 1, idx));
                        int r2 = int.Parse(Substring(arg, idx + 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.Cin(uid, "请选择{0}至{1}张怪物牌为{2}目标，可选{3}{4}.", r1, r2, prevComment, DisplayMonster(uss), cancel);
                        }
                        else
                            input = VI.Cin(uid, "请选择{0}至{1}张怪物牌为{2}目标{3}.", r1, r2, prevComment, cancel);
                    }
                    else
                    {
                        int r = int.Parse(Substring(arg, 1, jdx));
                        if (jdx >= 0)
                        {
                            string[] argv = Substring(arg, jdx + "(p".Length, kdx).Split('p');
                            var uss = argv.Select(p => ushort.Parse(p));
                            input = VI.Cin(uid, "请选择{0}张怪物牌为{1}目标，可选{2}{3}.", r, prevComment, DisplayMonster(uss), cancel);
                        }
                        else
                            input = VI.Cin(uid, "请选择{0}张怪物牌为{1}目标{2}.", r, prevComment, cancel);
                    }
                    prevComment = ""; cancel = "";
                    roundInput = input;
                }
                else if (arg[0] == 'Y') // Yes or not selection
                {
                    int posCan = (int)(arg[1] - '0');
                    string[] coms;
                    if (prevComment == "")
                        coms = Enumerable.Repeat("", posCan).ToArray();
                    else
                    {
                        string[] prevs = Util.Splits(prevComment, "##");
                        if (posCan >= prevs.Length)
                        {
                            IEnumerable<string> v1 = prevs.Select(p => ":" + p);
                            IEnumerable<string> v2 = Enumerable.Repeat("", posCan - prevs.Length);
                            coms = v1.Concat(v2).ToArray();
                        }
                        else
                            coms = prevs.Take(posCan).Select(p => ":" + p).ToArray();
                    }
                    string input = "请执行选项{0}—" + string.Join(",", Enumerable.Range(0, posCan).Select(p => (p + 1) + coms[p]));
                    roundInput = VI.Cin(uid, input, cancel);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'X') // Arrangement
                {
                    // format X(p1p3p5)
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string[] argv = Util.Substring(arg, jdx + "(p".Length, kdx).Split('p');
                    var uss = argv.Select(p => ushort.Parse(p));
                    roundInput = VI.Cin(uid, "请重排以下{0}怪物{1}{2}.", prevComment, DisplayMonster(uss), cancel);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'W') // Arrangement
                {
                    int jdx = arg.IndexOf('(');
                    int kdx = arg.IndexOf(')');
                    string[] argv = Util.Substring(arg, jdx + "(p".Length, kdx).Split('p');
                    var uss = argv.Select(p => ushort.Parse(p));
                    roundInput = VI.Cin(uid, "请重排以下{0}卡牌{1}{2}.", prevComment, DisplayTux(uss), cancel);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == 'S')
                {
                    roundInput = VI.Cin(uid, "请选择{0}一方{1}.", prevComment, cancel);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == '!')
                {
                    roundInput = arg.Substring(1);
                    prevComment = ""; cancel = "";
                }
                else if (arg[0] == '^')
                {

                }
                if (roundInput == "0" && block.StartsWith("/"))
                {
                    output += ",0"; return "/" + output.Substring(1);
                }
                else
                    output += "," + roundInput;
            }
            return output == "" ? "" : output.Substring(1);
        }
        private string FormattedInput(string line)
        {
            string result = FormattedInputWithCancelFlag(line);
            if (result.StartsWith("/"))
                result = result.Substring(1);
            return result;
        }
        #endregion Format Input

        #region E
        private void HandleE0Message(string readLine)
        {
            string[] args = readLine.Split(',');
            switch (args[0])
            {
                //case "E0IN":
                //    {
                //        string[] names = new string[] { "手牌", "怪物牌", "事件牌" };
                //        VI.Cout(uid, "{0}剩余牌数-{1}.", names[int.Parse(args[1])], args[2]);
                //    }
                //    break;
                //case "E0ON":
                //    {
                //        ushort type = ushort.Parse(args[1]);
                //        IEnumerable<ushort> ics = Util.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p));
                //        if (type == 0)
                //            VI.Cout(uid, "{0}进入弃牌堆.", DisplayTux(ics));
                //        else if (type == 1)
                //            VI.Cout(uid, "{0}进入弃牌堆.", DisplayMonster(ics));
                //        else if (type == 2)
                //            VI.Cout(uid, "{0}进入弃牌堆.", DisplayEve(ics));
                //    }
                //    break;
                //case "E0RN":
                //case "E0CN": break;
                case "E0HQ":
                    {
                        ushort type = ushort.Parse(args[1]);
                        if (type == 0)
                        {
                            ushort to = ushort.Parse(args[2]);
                            ushort from = ushort.Parse(args[3]);
                            int utype = int.Parse(args[4]);
                            if (utype == 0)
                            {
                                int n = int.Parse(args[5]);
                                var cards = Util.TakeRange(args, 6, args.Length).Select(p => ushort.Parse(p));
                                VI.Cout(uid, "{0}从{1}获得了{2}.", DisplayPlayer(to), DisplayPlayer(from), DisplayTux(cards));
                            }
                            else if (type == 1)
                            {
                                int n = int.Parse(args[5]);
                                VI.Cout(uid, "{0}从{1}获得了{2}张牌.", DisplayPlayer(to), DisplayPlayer(from), n);
                            }
                        }
                        else if (type == 2)
                        {
                            ushort to = ushort.Parse(args[2]);
                            var cards = Util.TakeRange(args, 3, args.Length).Select(p => ushort.Parse(p));
                            VI.Cout(uid, "{0}摸取了{1}.", DisplayPlayer(to), DisplayTux(cards));
                        }
                        else if (type == 3)
                        {
                            ushort to = ushort.Parse(args[2]);
                            int n = int.Parse(args[3]);
                            VI.Cout(uid, "{0}摸取了{1}张牌.", DisplayPlayer(to), n);
                        }
                        break;
                    }
                case "E0QZ":
                    {
                        ushort from = ushort.Parse(args[1]);
                        var cards = Util.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p));
                        VI.Cout(uid, "{0}弃置卡牌{1}.", DisplayPlayer(from), DisplayTux(cards));
                        break;
                    }
                case "E0IH":
                    {
                        string result = "", revive = "";
                        for (int i = 1; i < args.Length; i += 4)
                        {
                            ushort from = ushort.Parse(args[i]);
                            ushort prop = ushort.Parse(args[i + 1]);
                            ushort n = ushort.Parse(args[i + 2]);
                            ushort now = ushort.Parse(args[i + 3]);
                            result += string.Format("\n{0}HP+{1}({2})，当前HP={3}.", DisplayPlayer(from), n, DisplayProp(prop), now);
                            if (now == n)
                                revive += ("," + DisplayPlayer(from));
                        }
                        if (result != "")
                            VI.Cout(uid, result.Substring(1));
                        if (revive != "")
                            VI.Cout(uid, "以下角色脱离濒死状态：{0}.", revive.Substring(1));
                        break;
                    }
                case "E0OH":
                    {
                        string result = "", death = "";
                        for (int i = 1; i < args.Length; i += 4)
                        {
                            ushort from = ushort.Parse(args[i]);
                            ushort prop = ushort.Parse(args[i + 1]);
                            ushort n = ushort.Parse(args[i + 2]);
                            ushort now = ushort.Parse(args[i + 3]);
                            result += string.Format("\n{0}HP-{1}({2})，当前HP={3}.", DisplayPlayer(from), n, DisplayProp(prop), now);
                            if (now == 0)
                                death += ("," + DisplayPlayer(from));
                        }
                        if (result != "")
                            VI.Cout(uid, result.Substring(1));
                        if (death != "")
                            VI.Cout(uid, "以下角色处于濒死状态：{0}.", death.Substring(1));
                        break;
                    }
                //case "E0ZH":
                //    {
                //        string result = "";
                //        for (int i = 1; i < args.Length; ++i)
                //        {
                //            ushort py = ushort.Parse(args[i]);
                //            result += "," + DisplayPlayer(py);
                //        }
                //        if (result != "")
                //            VI.Cout(uid, "{0}阵亡.", result.Substring(1));
                //        break;
                //    }
                case "E0LV":
                    {
                        int idx = 1;
                        while (idx < args.Length)
                        {
                            ushort who = ushort.Parse(args[idx]);
                            int count = int.Parse(args[idx + 1]);
                            VI.Cout(uid, "{0}对{1}发动了倾慕.", DisplayPlayerWithMonster(
                                Util.TakeRange(args, idx + 2, idx + 2 + count)), DisplayPlayer(who));
                            idx += (2 + count);
                        }
                        break;
                    }
                case "E0ZW":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; ++i)
                        {
                            ushort py = ushort.Parse(args[i]);
                            result += "," + DisplayPlayer(py);
                        }
                        if (result != "")
                            VI.Cout(uid, "{0}因阵亡退场.", result.Substring(1));
                        break;
                    }
                case "E0IY":
                    {
                        ushort who = ushort.Parse(args[2]);
                        int hero = int.Parse(args[3]);
                        if (Heros.ContainsKey(who))
                            Heros[who] = hero;
                        string ops = args[1] == "0" ? "转化" : "变身";
                        VI.Cout(uid, "{0}#玩家{1}为{2}角色.", who,
                            ops, tuple.HL.InstanceHero(hero).Name);
                    }
                    break;
                case "E0OY":
                    if (args[1] == "0" || args[1] == "2")
                    {
                        ushort who = ushort.Parse(args[2]);
                        VI.Cout(uid, "{0}退场.", DisplayPlayer(who));
                    }
                    break;
                case "E0WN":
                    VI.Cout(uid, "{0}方获胜.", args[1]); break;
                case "E0DS":
                    {
                        ushort from = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            int n = int.Parse(args[3]);
                            VI.Cout(uid, "{0}被定身{1}回合.", DisplayPlayer(from), n);
                        }
                        else if (type == 1)
                            VI.Cout(uid, "{0}解除定身.", DisplayPlayer(from));
                        break;
                    }
                case "E0FU":
                    if (args[1].Equals("0"))
                    {
                        //ushort from = ushort.Parse(args[2]);
                        var ravs = Util.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p));
                        VI.Cout(uid, "你观看了{0}.", DisplayTux(ravs));
                    }
                    else if (args[1].Equals("1"))
                    {
                        ushort n = ushort.Parse(args[2]);
                        VI.Cout(uid, "{0}张卡牌正被观看.", n);
                    }
                    break;
                case "E0QU":
                    if (args[1].Equals("0"))
                    {
                        var ravs = Util.TakeRange(args, 2, args.Length).Select(p => ushort.Parse(p));
                        VI.Cout(uid, "{0}被移离观看区.", DisplayTux(ravs));
                    }
                    else if (args[1].Equals("1"))
                        VI.Cout(uid, "{0}张牌被移离观看区.", args[2]);
                    else if (args[1].Equals("2"))
                        VI.Cout(uid, "观看区被清空.");
                    break;
                case "E0CC": // prepare to use card
                    {
                        // E0CC,A,TP02,17,36
                        ushort ust = ushort.Parse(args[1]);
                        ushort[] ravs = new ushort[args.Length - 3];
                        for (int i = 3; i < args.Length; ++i)
                            ravs[i - 3] = ushort.Parse(args[i]);
                        VI.Cout(uid, "{0}将卡牌{1}当作卡牌{2}使用.", DisplayPlayer(ust), DisplayTux(ravs), DisplayTux(args[2]));
                        break;
                    }
                case "E0CD": // use card and want a target
                    {
                        // E0CD,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        ushort[] ravs = new ushort[args.Length - 3];
                        for (int i = 3; i < args.Length; ++i)
                            ravs[i - 3] = ushort.Parse(args[i]);
                        VI.Cout(uid, "{0}预定使用{1}({2}).", DisplayPlayer(ust), DisplayTux(args[2]), Util.SatoString(ravs));
                        break;
                    }
                case "E0CE": // use card and take action
                    {
                        // E0CE,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        ushort[] ravs = new ushort[args.Length - 3];
                        for (int i = 3; i < args.Length; ++i)
                            ravs[i - 3] = ushort.Parse(args[i]);
                        VI.Cout(uid, "{0}成功使用{1}({2}).", DisplayPlayer(ust), DisplayTux(args[2]), Util.SatoString(ravs));
                        break;
                    }
                case "E0CL": // cancel card
                    {
                        // E0CL,A,JP04,3,1
                        ushort ust = ushort.Parse(args[1]);
                        ushort[] ravs = new ushort[args.Length - 3];
                        for (int i = 3; i < args.Length; ++i)
                            ravs[i - 3] = ushort.Parse(args[i]);
                        VI.Cout(uid, "{0}的{1}({2})被抵消.", DisplayPlayer(ust), DisplayTux(args[2]), Util.SatoString(ravs));
                        break;
                    }
                case "E0XZ":
                    {
                        ushort py = ushort.Parse(args[1]);
                        ushort type = ushort.Parse(args[2]);
                        IDictionary<string, string> dd = new Dictionary<string, string>();
                        IDictionary<string, Func<IEnumerable<ushort>, string>> df =
                            new Dictionary<string, Func<IEnumerable<ushort>, string>>();
                        dd.Add("1", "手牌堆"); dd.Add("2", "怪物牌堆"); dd.Add("3", "事件牌堆");
                        df.Add("1", DisplayTux); df.Add("2", DisplayMonster); df.Add("3", DisplayEve);
                        if (type == 0)
                        {
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(uid, "您观看{0}结果为{1}.", dd[args[3]], df[args[3]](ravs));
                        }
                        else if (type == 1)
                            VI.Cout(uid, "{0}观看{1}上方{2}张牌.", DisplayPlayer(py), dd[args[3]], args[4]);
                        else if (type == 2)
                        {
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(uid, "您调整{0}结果为{1}.", dd[args[3]], df[args[3]](ravs));
                        }
                        else if (type == 3)
                        {
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(uid, "{0}调整{1}的新顺序为{2}.", DisplayPlayer(py), dd[args[3]], Util.SatoString(ravs));
                        }
                        else if (type == 4)
                            VI.Cout(uid, "{0}不调整牌堆顺序.", DisplayPlayer(py));
                        else if (type == 5)
                        {
                            ushort who = ushort.Parse(args[3]);
                            ushort[] ravs = new ushort[args.Length - 4];
                            for (int i = 4; i < args.Length; ++i)
                                ravs[i - 4] = ushort.Parse(args[i]);
                            VI.Cout(uid, "您观看{0}手牌结果为{1}.", DisplayPlayer(who), DisplayTux(ravs));
                        }
                        else if (type == 6)
                            VI.Cout(uid, "{0}观看了{1}的手牌.", DisplayPlayer(py), DisplayPlayer(ushort.Parse(args[3])));
                        break;
                    }
                case "E0ZB":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort where = ushort.Parse(args[2]);
                        ushort card = ushort.Parse(args[3]);
                        if (where == 1)
                            VI.Cout(uid, "{0}装备了卡牌{1}到武器区.", DisplayPlayer(me), DisplayTux(card));
                        else if (where == 2)
                            VI.Cout(uid, "{0}装备了卡牌{1}到防具区.", DisplayPlayer(me), DisplayTux(card));
                        else if (where == 3)
                            VI.Cout(uid, "{0}装备了卡牌{1}到特殊区.", DisplayPlayer(me), DisplayTux(card));
                        break;
                    }
                case "E0ZC":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort consumeType = ushort.Parse(args[2]);
                        ushort where = ushort.Parse(args[3]);
                        ushort card = ushort.Parse(args[4]);
                        int type = int.Parse(args[5]);

                        string argvs = "";
                        for (int i = 6; i < args.Length; ++i)
                            argvs += "," + args[i];
                        if (argvs != "")
                            argvs = "(" + argvs.Substring(1) + ")";

                        if (where == 1 && consumeType == 0)
                            VI.Cout(uid, "{0}发动了武器区卡牌{1}[{2}]特效{3}.", DisplayPlayer(me), DisplayTux(card), type, argvs);
                        else if (where == 1 && consumeType == 1)
                            VI.Cout(uid, "{0}爆发了武器区卡牌{1}[{2}]{3}.", DisplayPlayer(me), DisplayTux(card), type, argvs);
                        else if (where == 2 && consumeType == 0)
                            VI.Cout(uid, "{0}发动了武器区卡牌{1}[{2}]特效{3}.", DisplayPlayer(me), DisplayTux(card), type, argvs);
                        else if (where == 2 && consumeType == 1)
                            VI.Cout(uid, "{0}爆发了防具区卡牌{1}[{2}]{3}.", DisplayPlayer(me), DisplayTux(card), type, argvs);
                        break;
                    }
                case "E0ZL":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort card = ushort.Parse(args[i + 1]);
                            result += string.Format(",{0}的{1}", DisplayPlayer(who), DisplayTux(card));
                        }
                        if (result != "")
                            VI.Cout(uid, "{0}装备特效无效化.", result.Substring(1));
                    }
                    break;
                case "E0ZS":
                    {
                        string result = "";
                        for (int i = 1; i < args.Length; i += 2)
                        {
                            ushort who = ushort.Parse(args[i]);
                            ushort card = ushort.Parse(args[i + 1]);
                            result += string.Format(",{0}的{1}", DisplayPlayer(who), DisplayTux(card));
                        }
                        if (result != "")
                            VI.Cout(uid, "{0}装备特效开始生效.", result.Substring(1));
                    }
                    break;
                case "E0IA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        int n = int.Parse(args[3]);
                        int bs = int.Parse(args[4]);
                        int tp = int.Parse(args[5]);
                        if (type == 0)
                            VI.Cout(uid, "{0}基础战力+{1},当前战力为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 1)
                            VI.Cout(uid, "{0}本场战力+{1},当前战力为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 2)
                            VI.Cout(uid, "{0}临时战力+{1},当前战力为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        break;
                    }
                case "E0OA":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        int n = int.Parse(args[3]);
                        int bs = int.Parse(args[4]);
                        int tp = int.Parse(args[5]);
                        if (type == 0)
                            VI.Cout(uid, "{0}基础战力-{1},当前战力为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 1)
                            VI.Cout(uid, "{0}本场战力-{1},当前战力为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 2)
                            VI.Cout(uid, "{0}临时战力-{1},当前战力为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        break;
                    }
                case "E0IX":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        int n = int.Parse(args[3]);
                        int bs = int.Parse(args[4]);
                        int tp = int.Parse(args[5]);
                        if (type == 0)
                            VI.Cout(uid, "{0}基础命中+{1},当前命中为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 1)
                            VI.Cout(uid, "{0}本场命中+{1},当前命中为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 2)
                            VI.Cout(uid, "{0}临时命中+{1},当前命中为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        break;
                    }
                case "E0OX":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int type = int.Parse(args[2]);
                        int n = int.Parse(args[3]);
                        int bs = int.Parse(args[4]);
                        int tp = int.Parse(args[5]);
                        if (type == 0)
                            VI.Cout(uid, "{0}基础命中-{1},当前命中为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 1)
                            VI.Cout(uid, "{0}本场命中-{1},当前命中为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        else if (type == 2)
                            VI.Cout(uid, "{0}临时命中-{1},当前命中为{2}/{3}.", DisplayPlayer(who), n, tp, bs);
                        break;
                    }
                case "E0AX":
                    {
                        ushort who = ushort.Parse(args[1]);
                        int str = int.Parse(args[2]);
                        int dex = int.Parse(args[3]);
                        VI.Cout(uid, "{0}战力恢复为{1},命中恢复为{2}.", DisplayPlayer(who), str, dex);
                        break;
                    }
                case "E0IB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int delta = ushort.Parse(args[2]);
                        int cur = int.Parse(args[3]);
                        VI.Cout(uid, "{0}战力+{1},现在为{2}.", DisplayMonster(x), delta, cur);
                        break;
                    }
                case "E0OB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int delta = int.Parse(args[2]);
                        int cur = int.Parse(args[3]);
                        VI.Cout(uid, "{0}战力-{1},现在为{2}.", DisplayMonster(x), delta, cur);
                        break;
                    }
                case "E0WB":
                    {
                        ushort x = ushort.Parse(args[1]);
                        int cur = int.Parse(args[2]);
                        VI.Cout(uid, "{0}战力恢复为{1}.", DisplayMonster(x), cur);
                    }
                    break;
                case "E09P":
                    if (args[1] == "0")
                    {
                        ushort s = ushort.Parse(args[2]);
                        bool sy = args[3].Equals("1");
                        ushort h = ushort.Parse(args[4]);
                        bool hy = args[5].Equals("1");
                        string comp1 = (s != 0) ? "{0}支援" + (sy ? "成功" : "失败") : "无支援";
                        string comp2 = (h != 0) ? "{1}妨碍" + (hy ? "成功" : "失败") : "无妨碍";
                        VI.Cout(uid, comp1 + "，" + comp2 + "。", DisplayPlayer(s), DisplayPlayer(h));
                    }
                    else if (args[1] == "1") {
                        ushort rside = ushort.Parse(args[2]);
                        int rpool = ushort.Parse(args[3]);
                        ushort oside = ushort.Parse(args[4]);
                        int opool = ushort.Parse(args[5]);
                        VI.Cout(uid, "{0}方战力={1}，{2}方战力={3}.", rside, rpool, oside, opool);
                    }
                    break;
                case "E0IP":
                    {
                        ushort side = ushort.Parse(args[1]);
                        ushort delta = ushort.Parse(args[2]);
                        VI.Cout(uid, "{0}方战力+{1}.", side, delta);
                        break;
                    }
                case "E0OP":
                    {
                        ushort side = ushort.Parse(args[1]);
                        ushort delta = ushort.Parse(args[2]);
                        VI.Cout(uid, "{0}方战力-{1}.", side, delta);
                        break;
                    }
                case "E0CZ":
                    if (args[1] == "0")
                        VI.Cout(uid, "{0}禁止使用战牌.", DisplayPlayer(ushort.Parse(args[2])));
                    else if (args[1] == "1")
                        VI.Cout(uid, "{0}恢复使用战牌权.", DisplayPlayer(ushort.Parse(args[2])));
                    else if (args[1] == "2")
                        VI.Cout(uid, "全体恢复使用战牌权.");
                    break;
                case "E0HC":
                    {
                        int type = int.Parse(args[1]);
                        ushort who = ushort.Parse(args[2]);
                        if (type == 0)
                        {
                            var cards = Util.TakeRange(args, 3, args.Length).Select(p => ushort.Parse(p));
                            VI.Cout(uid, "{0}可以获得宠物{1}.", DisplayPlayer(who), DisplayMonster(cards));
                        }
                    }
                    break;
                case "E0HH":
                    {
                        ushort me = ushort.Parse(args[1]);
                        ushort consumeType = ushort.Parse(args[2]);
                        ushort mons = ushort.Parse(args[3]);
                        int type = int.Parse(args[4]);

                        string argvs = "";
                        for (int i = 5; i < args.Length; ++i)
                            argvs += "," + args[i];
                        if (argvs != "")
                            argvs = "(" + argvs.Substring(1) + ")";

                        if (consumeType == 0)
                            VI.Cout(uid, "{0}发动了宠物{1}[{2}]特效{3}.", DisplayPlayer(me), DisplayMonster(mons), type, argvs);
                        else if (consumeType == 1)
                            VI.Cout(uid, "{0}爆发了宠物{1}[{2}]{3}.", DisplayPlayer(me), DisplayMonster(mons), type, argvs);
                        break;
                    }
                case "E0HD":
                    VI.Cout(uid, "{0}获得了宠物{1}.", DisplayPlayer(ushort.Parse(args[1])),
                        DisplayMonster(ushort.Parse(args[2])));
                    break;
                case "E0HL":
                    VI.Cout(uid, "{0}失去了宠物{1}.", DisplayPlayer(ushort.Parse(args[1])),
                        DisplayMonster(ushort.Parse(args[2])));
                    break;
                case "E0HZ":
                    if (args[1] == "0") {
                        ushort who = ushort.Parse(args[2]);
                        VI.Cout(uid, "{0}放弃触发混战.", DisplayPlayer(who));
                    } else if (args[1] == "1") {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        VI.Cout(uid, "{0}触发混战的结果为{1}.", DisplayPlayer(who), DisplayMonster(mon));
                    } else if (args[1] == "2") {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        VI.Cout(uid, "{0}触发混战的结果为{1}.", DisplayPlayer(who), DisplayMonster(mon));
                    } else if (args[1] == "3") {
                        ushort who = ushort.Parse(args[2]);
                        ushort mon = ushort.Parse(args[3]);
                        VI.Cout(uid, "{0}触发混战的结果为{1}，钦慕效果生效.", DisplayPlayer(who), DisplayMonster(mon));
                    }
                    break;
                case "E0TT":
                    VI.Cout(uid, "{0}掷骰的结果为{1}.", DisplayPlayer(ushort.Parse(args[1])), args[2]);
                    break;
                case "E0IJ":
                    for (int idx = 1; idx < args.Length;) {
                        ushort who = ushort.Parse(args[idx]);
                        ushort type = ushort.Parse(args[idx + 1]);
                        if (type == 0) {
                            ushort delta = ushort.Parse(args[idx + 2]);
                            ushort cur = ushort.Parse(args[idx + 3]);
                            VI.Cout(uid, "{0}的标记数+{1}，现在为{2}.", DisplayPlayer(who), delta, cur);
                            idx += 4;
                        }
                        else if (type == 1)
                        {
                            int hero = int.Parse(args[idx + 2]);
                            int count = int.Parse(args[idx + 3]);
                            List<int> heros = Util.TakeRange(args, idx + 4, idx + 4 + count)
                                .Select(p => int.Parse(p)).ToList();
                            VI.Cout(uid, "{0}的武将牌增加{1}，现在为{2}.", DisplayPlayer(who),
                                DisplayHero(hero), DisplayHero(heros));
                            idx += (4 + count);
                        }
                        else
                            break;
                    }
                    break;
                case "E0OJ":
                    for (int idx = 1; idx < args.Length; )
                    {
                        ushort who = ushort.Parse(args[idx]);
                        ushort type = ushort.Parse(args[idx + 1]);
                        if (type == 0)
                        {
                            ushort delta = ushort.Parse(args[idx + 2]);
                            ushort cur = ushort.Parse(args[idx + 3]);
                            VI.Cout(uid, "{0}的标记数-{1}，现在为{2}.", DisplayPlayer(who), delta, cur);
                            idx += 4;
                        }
                        else if (type == 1)
                        {
                            int hero = int.Parse(args[idx + 2]);
                            int count = int.Parse(args[idx + 3]);
                            List<int> heros = Util.TakeRange(args, idx + 4, idx + 4 + count)
                                .Select(p => int.Parse(p)).ToList();
                            VI.Cout(uid, "{0}的武将牌减少{1}，现在为{2}.", DisplayPlayer(who),
                                DisplayHero(hero), DisplayHero(heros));
                            idx += (4 + count);
                        }
                        else
                            break;
                    }
                    break;
            }
        }
        #endregion E
        #region F
        private void HandleF0Message(string readLine)
        {
            lock (listOfThreads)
            {
                foreach (Thread td in listOfThreads)
                {
                    if (td != Thread.CurrentThread && td.IsAlive)
                        td.Abort();
                }
                listOfThreads.Clear();
                listOfThreads.Add(Thread.CurrentThread);
            }
            WI.Send(readLine, uid, 0);
            string[] args = readLine.Split(',');
            switch (args[0])
            {
                case "F0JM":
                    VI.Cout(uid, "强制跳转至阶段{0}.", args[1]);
                    break;
                case "F0WN":
                    if (args[1] == "0")
                        VI.Cout(uid, "游戏结束，平局.");
                    else
                        VI.Cout(uid, "游戏结束，{0}方获胜.", args[1]);
                    break;
            }
            
        }
        #endregion F
        #region U
        private void HandleU1Message(string inv, string mai)
        {
            ushort[] invs = inv.Split(',').Select(p => ushort.Parse(p)).ToArray();
            if (string.IsNullOrEmpty(mai) || mai == "0")
            {
                VI.Cout(uid, "等待下列玩家行动:{0}...", DisplayPlayer(invs));
                WI.Send("U2,0", uid, 0);
                VI.CloseCinTunnel(uid);
                return;
            }
            VI.Cout(uid, "下列玩家与你均可行动:{0}.", DisplayPlayer(invs));
            bool decided = false;
            while (!decided)
            {
                IDictionary<string, string> skTable = new Dictionary<string, string>();
                VI.OpenCinTunnel(uid);
                string[] blocks = mai.Split(';');
                string opt = "您可以发动";
                foreach (string block in blocks)
                {
                    //int jdx = -1;
                    //if (block.StartsWith("JN") || block.StartsWith("CZ") || block.StartsWith("NJ"))
                    //    jdx = block.IndexOf(',');
                    //else
                    //{
                    //    int kdx = block.IndexOf(',');
                    //    if (kdx >= 0)
                    //        jdx = block.IndexOf(',', kdx + 1);
                    //}
                    int jdx = block.IndexOf(',');
                    if (jdx < 0) {
                        opt += DisplaySKTXCZ(block) + ";";
                        skTable.Add(block, "^");
                    }
                    else
                    {
                        string name = block.Substring(0, jdx);
                        string rest = block.Substring(jdx + 1);
                        opt += DisplaySKTXCZ(name) + ";";
                        skTable.Add(name, rest);
                    }
                }
                VI.Cout(uid, opt.Substring(0, opt.Length - 1));
                string inputBase = VI.Cin(uid, "请做出您的选择，0为放弃行动:");
                if (inputBase == VI.CinSentinel)
                    decided = true;
                else if (inputBase == "0")
                {
                    decided = true;
                    VI.Cout(uid, "您决定放弃行动.");
                    WI.Send("U2,0", uid, 0);
                }
                else if (skTable.ContainsKey(inputBase))
                {
                    if (skTable[inputBase] == "^")
                    {
                        decided = true;
                        WI.Send("U2," + inputBase, uid, 0);
                    }
                    else
                    {
                        string input = FormattedInputWithCancelFlag(skTable[inputBase]);
                        if (!input.StartsWith("/"))
                        {
                            decided = true;
                            if (input != "")
                                input = "," + input;
                            WI.Send("U2," + inputBase + input, uid, 0);
                        }
                    }
                    // otherwise, cancel and not action immediately, still wait
                }
            }
        }
        private void HandleU3Message(string mai, string prev, string inType) {
            VI.OpenCinTunnel(uid);
            string action = AnalysisAction(mai, inType);
            VI.Cout(uid, "已尝试{0}{1}，请继续：", action, DisplaySKTXCZ(prev));
            string input = FormattedInputWithCancelFlag(mai);
            VI.CloseCinTunnel(uid);
            if (!input.StartsWith("/") && input != "")
                WI.Send("U4," + prev + "," + input, uid, 0);
            else
                WI.Send("U4,0", uid, 0);
        }
        private void HandleU5Message(string involved, string mai, string inType)
        {
            VI.CloseCinTunnel(uid);
            ushort owner = ushort.Parse(involved);
            string action = AnalysisAction(mai, inType);
            if (owner != uid)
                VI.Cout(uid, "{0}{1}了{2}.", DisplayPlayer(owner), action, DisplaySKTXCZ(mai));
            else
                VI.Cout(uid, "您{0}了{1}.", action, DisplaySKTXCZ(mai));
        }
        private void HandleU7Message(string inv, string mai, string prev, string inType)
        {
            ushort owner = ushort.Parse(inv);
            VI.OpenCinTunnel(uid);
            string action = AnalysisAction(mai, inType);
            VI.Cout(uid, "{0}{1}过程中，请继续：", action, DisplaySKTXCZ(prev));
            string input = FormattedInputWithCancelFlag(mai);
            VI.CloseCinTunnel(uid);
            WI.SendDirect("U8," + prev + "," + input, uid);
        }
        private void HandleU9Message(string inv, string prev, string inType)
        {
            ushort owner = ushort.Parse(inv);
            VI.Cout(uid, "等待{0}响应中:{1}...", DisplayPlayer(owner), DisplaySKTXCZ(prev));
            VI.CloseCinTunnel(uid);
            return;
        }
        private void HandleUAMessage(string inv, string mai, string inType) {
            ushort owner = ushort.Parse(inv);
            string action = inType.Contains('!') ? "爆发" : "执行";
            VI.Cout(uid, "{0}{1}{2}完毕.", owner, action, DisplaySKTXCZ(mai));
        }
        #endregion U
        #region R
        private void HandleRMessage(string readLine)
        {
            int idx = readLine.IndexOf(',');
            ushort rounder = (ushort)(readLine[1] - '0');
            string cop = Substring(readLine, "R0".Length, idx);
            string para = idx >= 0 ? readLine.Substring(idx + 1) : "";

            switch (cop)
            {
                case "001":
                    {
                        ushort type = ushort.Parse(para);
                        if (type == 0)
                            VI.Cout(uid, "{0}回合开始.", DisplayPlayer(rounder));
                        else if (type == 1)
                            VI.Cout(uid, "{0}回合跳过.", DisplayPlayer(rounder));
                        else if (type == 2)
                            VI.Cout(uid, "{0}回合跳过，恢复正常.", DisplayPlayer(rounder));
                        break;
                    }
                case "EV1":
                    {
                        if (rounder == uid)
                        {
                            VI.OpenCinTunnel(uid);
                            string reply = VI.Cin(uid, "是否翻看事件牌？0/1：");
                            if (!reply.Equals("1"))
                                reply = "0";
                            WI.Send("R" + rounder + "EV2," + reply, uid, 0);
                            VI.CloseCinTunnel(uid);
                        }
                        else
                        {
                            VI.Cout(uid, "等待{0}是否翻看事件牌...", DisplayPlayer(rounder));
                            VI.CloseCinTunnel(uid);
                        }
                    }
                    break;
                case "EV3":
                    {
                        ushort no = ushort.Parse(para);
                        if (para == "0")
                            VI.Cout(uid, "决定不翻看事件牌.");
                        else
                            VI.Cout(uid, "翻看事件牌{0}.", DisplayEve(no));
                    }
                    break;
                case "GR":
                    if (para == "0")
                        VI.Cout(uid, "{0}技牌阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}技牌阶段结束.", DisplayPlayer(rounder));
                    break;
                case "GE":
                    if (para == "0")
                        VI.Cout(uid, "{0}技牌结束阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}技牌结束阶段结束.", DisplayPlayer(rounder));
                    break;
                case "Z0":
                    if (para == "0")
                        VI.Cout(uid, "{0}战牌开始阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}战牌开始阶段结束.", DisplayPlayer(rounder));
                    break;
                case "ZW1":
                    {
                        string[] args = para.Split(',');
                        ushort isSupport = ushort.Parse(args[0]);
                        ushort isDecider = ushort.Parse(args[1]);
                        string[] candidates = new string[args.Length - 2];
                        for (int i = 2; i < args.Length; ++i)
                        {
                            ushort ut;
                            if (ushort.TryParse(args[i], out ut))
                                candidates[i - 2] = args[i];
                            else
                                candidates[i - 2] = "!" + args[i];
                        }
                        string identity = isSupport == 0 ? "支援者{1}—{0}则不支援，0则不打怪."
                            : "妨碍者{1}—{0}则不妨碍.";
                        VI.OpenCinTunnel(uid);
                        if (isDecider == 0) // decider
                        {
                            string select = VI.Cin(uid, "{0}战斗阶段开始，请决定" + identity,
                                DisplayPlayer(rounder), DisplayPlayerWithMonster(candidates));
                            if (select != VI.CinSentinel)
                            {
                                WI.Send("R" + rounder + "ZW4," + select, uid, 0);
                                VI.CloseCinTunnel(uid);
                            }
                        }
                        else if (isDecider == 1)
                        {
                            string select = VI.Cin(uid, "{0}战斗阶段开始，请推选" + identity,
                                DisplayPlayer(rounder), DisplayPlayerWithMonster(candidates));
                            if (select != VI.CinSentinel)
                            {
                                WI.Send("R" + rounder + "ZW2," + select, uid, 0);
                                //VI.CloseCinTunnel(uid);
                            }
                        }
                        break;
                    }
                case "ZW3":
                case "ZW5":
                    {
                        int jdx = para.IndexOf(',');
                        string suggest = cop[2] == '3' ? "建议" : "决定";
                        if (jdx >= 0)
                        {
                            ushort from = ushort.Parse(para.Substring(0, jdx));
                            string advice = para.Substring(jdx + 1);
                            ushort who;
                            if (ushort.TryParse(advice, out who))
                            {
                                if (who == 0)
                                    VI.Cout(uid, "{0}{1}不打怪.", DisplayPlayer(from), suggest);
                                else if (who == rounder)
                                    VI.Cout(uid, "{0}{1}不让其它人参与战斗.", DisplayPlayer(from), suggest);
                                else
                                    VI.Cout(uid, "{0}{1}{2}参与战斗.", DisplayPlayer(from), suggest, DisplayPlayer(who));
                            }
                            else if (advice.StartsWith("G"))
                            {
                                ushort monCode = tuple.ML.Encode(advice);
                                if (monCode > 0)
                                    VI.Cout(uid, "{0}{1}{2}参与战斗.", DisplayPlayer(from), suggest, DisplayMonster(monCode));
                            }
                        }
                        else
                        {
                            ushort from = ushort.Parse(para);
                            VI.Cout(uid, "{0}已经做出了{1}.", DisplayPlayer(from), suggest);
                        }
                    }
                    break;
                case "ZW7":
                    {
                        string[] args = para.Split(',');
                        if (args[0] == "0")
                        {
                            ushort mons = ushort.Parse(args[1]);
                            VI.Cout(uid, "{0}决定不打怪，放弃怪物{1}.", DisplayPlayer(rounder), DisplayMonster(mons));
                        }
                        else if (args[0] == "1")
                        {
                            ushort s = ushort.Parse(args[1]);
                            ushort h = ushort.Parse(args[2]);
                            string ss = (s == 0) ? "不支援" : "{1}进行支援";
                            string sh = (h == 0) ? "不妨碍" : "{2}进行妨碍";
                            VI.Cout(uid, "{0}决定打怪，" + ss + "，" + sh + ".", DisplayPlayer(rounder),
                                s < 1000 ? DisplayPlayer(s) : DisplayMonster((ushort)(s - 1000)),
                                h < 1000 ? DisplayPlayer(h) : DisplayMonster((ushort)(h - 1000)));
                        }
                    }
                    VI.CloseCinTunnel(uid);
                    break;
                case "ZM1":
                    {
                        ushort mons = ushort.Parse(para);
                        VI.Cout(uid, "翻出的怪物为{0}.", DisplayMonster(mons));
                        break;
                    }
                case "NP1":
                    if (para == "0")
                        VI.Cout(uid, "{0}NPC响应开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}NPC响应结束.", DisplayPlayer(rounder));
                    break;
                case "NP2":
                    VI.Cout(uid, "{0}跳过NPC，继续翻看怪物牌，结果为{1}.", DisplayPlayer(rounder),
                        DisplayMonster(ushort.Parse(para))); break;
                case "Z1":
                    if (para == "0")
                        VI.Cout(uid, "{0}战斗开始阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}战斗开始阶段结束.", DisplayPlayer(rounder));
                    break;
                //case "ZC1":
                //    {
                //        string[] args = para.Split(',');
                //        ushort s = ushort.Parse(args[0]);
                //        bool sy = args[1].Equals("1");
                //        ushort h = ushort.Parse(args[2]);
                //        bool hy = args[3].Equals("1");
                //        string comp1 = (s != 0) ? "{0}支援" + (sy ? "成功" : "失败") : "无支援";
                //        string comp2 = (h != 0) ? "{1}妨碍" + (hy ? "成功" : "失败") : "无妨碍";
                //        VI.Cout(uid, comp1 + "，" + comp2 + "。", DisplayPlayer(s), DisplayPlayer(h));
                //        break;
                //    }
                case "ZD":
                    if (para == "0")
                        VI.Cout(uid, "{0}战牌阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}战牌阶段结束.", DisplayPlayer(rounder));
                    break;
                case "VS1":
                    VI.Cout(uid, "判断胜负阶段中..."); break;
                case "VS2":
                    VI.Cout(uid, "判断胜负结束,{0}方胜利.", ushort.Parse(para));
                    break;
                case "ZF":
                    if (para == "0")
                        VI.Cout(uid, "{0}战牌结束阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}战牌结束阶段结束.", DisplayPlayer(rounder));
                    break;
                case "BC":
                    if (para == "0")
                        VI.Cout(uid, "{0}补牌阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}补牌阶段结束.", DisplayPlayer(rounder));
                    break;
                case "TM":
                    if (para == "0")
                        VI.Cout(uid, "{0}回合结束阶段开始.", DisplayPlayer(rounder));
                    else if (para == "1")
                        VI.Cout(uid, "{0}回合结束阶段结束.", DisplayPlayer(rounder));
                    break;
            }
        }
        #endregion R

        private void OnNotifyHero(string line)
        {
            string[] args = line.Split(',');
            string msg = "Candidates: ";
            for (int i = 1; i < args.Length; ++i)
            {
                int heroCode = int.Parse(args[i]);
                msg += heroCode + ",";
            }
            VI.Cout(uid, msg);
        }
        private void OnDecideHero(string line)
        {
            string[] args = line.Split(',');
            int code = int.Parse(args[0]);
            if (code == 0)
            {
                ushort puid = ushort.Parse(args[1]);
                VI.Cout(uid, "Player " + puid + "# has made the decision.");
            }
            else if (code == 1)
            {
                int heroCode = int.Parse(args[2]);
                VI.Cout(uid, "Confirmation of selection to (" + heroCode + ").");
            }
            else if (code == 2)
            {
                string msg = "Selection: ";
                for (int i = 0; i < (args.Length - 1) / 2; ++i)
                {
                    ushort puid = ushort.Parse(args[2 * i + 1]);
                    int heroCode = ushort.Parse(args[2 * i + 2]);
                    msg += (puid + ":" + heroCode + ",");
                }
                VI.Cout(uid, msg);
            }
        }

        #region Utils
        // TODO:DEBUG: Libized?
        private IDictionary<string, Base.Skill> sk01;
        private IDictionary<string, Base.Operation> cz01;
        private IDictionary<string, Base.NCAction> nj01;

        private string DisplaySKTXCZ(string ops)
        {
            int idx = ops.IndexOf(',');
            string title = Util.Substring(ops, 0, idx);
            string result = "";
            if (title.StartsWith("JN"))
            {
                int jdx = title.IndexOf('('), kdx = title.IndexOf(')');
                if (jdx < 0)
                    result = title + ":" + sk01[title].Name;
                else
                    result = title + ":" + sk01[Util.Substring(title, 0, jdx)].Name + "(" + Util.Substring(title, jdx + 1, kdx) + ")";
            }
            else if (title.StartsWith("CZ"))
                result = title + ":" + cz01[title].Name;
            else if (title.StartsWith("TX"))
                result = title + ":" + tuple.TL.DecodeTux(ushort.Parse(title.Substring("TX".Length))).Name;
            else if (title.StartsWith("NJ"))
                result = title + ":" + nj01[title].Name;
            else if (title.StartsWith("PT"))
                result = title + ":" + tuple.ML.Decode(ushort.Parse(title.Substring("PT".Length))).Name;
            else
                result = title;
            if (idx >= 0)
                result += "(" + ops.Substring(idx + 1) + ")";
            return result;
        }
        private string DisplayTux(ushort card)
        {
            return card == 0 ? "0:无" : card + ":" + tuple.TL.DecodeTux(card).Name;
        }
        private string DisplayTux(IEnumerable<ushort> cards)
        {
            if (!cards.Any())
                return "{}";
            return "{" + string.Join(",", cards.Select(p => DisplayTux(p))) + "}";
        }
        private string DisplayTux(string cardName)
        {
            return cardName + ":" + tuple.TL.Firsts.Find(p => p.Code == cardName).Name;
        }
        private string DisplayPlayer(ushort player)
        {
            return player == 0 ? "0:天上" : (player < 1000 ? player + ":" +
                tuple.HL.InstanceHero(Heros[player]).Name : DisplayMonster((ushort)(player - 1000)));
        }
        private string DisplayPlayer(IEnumerable<ushort> players)
        {
            if (!players.Any())
                return "{}";
            return "{" + string.Join(",", players.Select(p => DisplayPlayer(p))) + "}";
        }
        private string DisplayPlayerWithMonster(IEnumerable<string> strings) // used only for Baiyue
        {
            if (!strings.Any())
                return "{}";
            return "{" + string.Join(",", strings.Select(p => p.StartsWith("!") ?
                (p.Substring(1) + ":" + tuple.ML.Decode(tuple.ML.Encode(p.Substring(1))).Name)
                : DisplayPlayer(ushort.Parse(p)))) + "}";
        }
        private string DisplayMonster(ushort p)
        {
            return (p == 0) ? "0:没" : p + ":" + Base.Card.NMBLib.Decode(p, tuple.ML,tuple.NL).Name;
        }
        private string DisplayMonster(IEnumerable<ushort> mons)
        {
            if (!mons.Any())
                return "{}";
            var ma = mons.Select(p => DisplayMonster(p));
            return "{" + string.Join(",", ma) + "}";
            //return "{" + string.Join(",", mons.Select(p => DisplayMonster(p)) + "}");
        }
        private string DisplayEve(ushort eve)
        {
            return (eve == 0) ? "0:静" : tuple.EL.DecodeEvenement(eve).Name;
        }
        private string DisplayEve(IEnumerable<ushort> eves)
        {
            if (!eves.Any())
                return "{}";
            return "{" + string.Join(",", eves.Select(p => DisplayEve(p)) + "}");
        }
        private string DisplayHero(int hero)
        {
            return hero == 0 ? "0:姚仙" : tuple.HL.InstanceHero(hero).Name;
        }
        private string DisplayHero(IEnumerable<int> heros)
        {
            if (!heros.Any())
                return "{}";
            return "{" + string.Join(",", heros.Select(p => DisplayHero(p)) + "}");
        }
        private string DisplayProp(ushort prop)
        {
            switch (prop)
            {
                case 1: return "水";
                case 2: return "火";
                case 3: return "雷";
                case 4: return "风";
                case 5: return "土";
                case 6: return "阴";
                case 7: return "阳";
                case 8: return "物";
                case 9: return "钦慕";
                default: return "属性" + prop;
            }
        }
        private string AnalysisAction(string mai, string typeStr)
        {
            if (mai.StartsWith("JN"))
                return "发动";
            else if (mai.StartsWith("CZ"))
                return "操作";
            else if (mai.StartsWith("NJ"))
                return "执行NPC效果";
            else
            {
                if (!typeStr.Contains('!'))
                    return "使用";
                else
                {
                    int idx = typeStr.IndexOf('!');
                    string ct = typeStr.Substring(idx + 1);
                    if (ct.Equals("0"))
                        return "利用";
                    else if (ct.Equals("1"))
                        return "爆发";
                }
            }
            return "南泥湾";
        }
        private static string Substring(string @string, int start, int end)
        {
            if (end >= 0)
                return @string.Substring(start, end - start);
            else
                return @string.Substring(start);
        }

        #endregion Utils
    }
}
