using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg
{
    #region SKTriple Declaration

    internal enum SKTType { SK, BK, TX, EQ, CZ, NJ, PT, EV }

    internal class SkTriple
    {
        internal string Name { set; get; }
        internal int Priorty { set; get; }
        // Owner = 0, public:Tux; otherwise, private:Skill
        internal ushort Owner { set; get; }
        // Type, which occur trigger the action, starts with 0
        internal int InType { set; get; }
        // whether an effect needs equipment or not.
        internal SKTType Type { set; get; }
        // consume type : 0->Substain, 1->Once, 2->Background
        internal int Consume { set; get; }
        // lock skill / card-effect, use card then false
        internal bool? Lock { set; get; }
        // whether is once or not
        internal bool IsOnce { get; set; }
        // which occur event trigger it
        internal string Occur { get; set; }
        // used for parasitism, indicates where it comes from
        internal string LinkFrom { set; get; }
        // whether it will terminate the event, otherwise continue with it
        internal bool IsTermini { get; set; }

        internal static int Cmp(SkTriple skt, SkTriple sku)
        {
            return skt.Priorty - sku.Priorty;
        }
    }
    // Skill Triple Element, contains Trigger Information (e.g. Fuse, Tick, Tg)
    internal class SKE
    {
        internal string Name { private set; get; }
        internal int Priorty { private set; get; }
        internal ushort Owner { private set; get; }
        internal int InType { private set; get; }
        internal SKTType Type { set; get; }
        internal int Consume { set; get; }
        internal bool? Lock { set; get; }
        internal bool IsOnce { get; set; }
        internal string LinkFrom { set; get; } // Parasitism 
        internal bool IsTermini { get; set; }

        // card code to distinguish which card, 0 if skill
        //internal ushort CardCode { set; get; }
        // Fuse, which R/G trigger the action
        internal string Fuse { set; get; }
        // Use Count
        internal int Tick { set; get; }
        // Actual Trigger
        //internal ushort Trigger { set; get; }
        internal ushort Tg { set; get; }

        internal SKE(SkTriple skt)
        {
            Name = skt.Name;
            Priorty = skt.Priorty;
            Owner = skt.Owner;
            InType = skt.InType;
            Type = skt.Type;
            Consume = skt.Consume;
            Lock = skt.Lock;
            IsOnce = skt.IsOnce;
            LinkFrom = skt.LinkFrom;
            IsTermini = skt.IsTermini;

            Fuse = ""; Tick = 0; Tg = 0;
        }

        internal static List<SKE> Generate(List<SkTriple> list)
        {
            List<SKE> result = new List<SKE>();
            foreach (SkTriple skt in list)
                result.Add(new SKE(skt));
            return result;
        }

        internal static SKE Find(string name, ushort tg, List<SKE> list)
        {
            foreach (SKE ske in list)
                if (ske.Name.Equals(name) && ske.Tg == tg)
                    return ske;
            return null;
        }

        internal string ToTagString()
        {
            if (Type == SKTType.EQ || Type == SKTType.PT)
                return Name + "," + Consume + "!" + InType;
            else
                return Name + "," + InType;
        }
    }

    #endregion SKTriple Declaration

    public enum UEchoCode
    {
        STRANGE = 0, RE_REQUEST = 1, 
        NO_OPTIONS = 2, // NPC case only now
        NEXT_STEP = 3, // U3 Message, need others
        END_CANCEL = 4, END_ACTION = 5,
        END_TERMIN = 7, // Terminate the event (G/R) at the same time, Blue Case
    }

    public partial class XI
    {
        /// <summary>
        /// The last U/V message to be handled, used for re-broadcasting in re-connection
        /// </summary>
        public IDictionary<ushort, string> LastUVs { private set; get; }

        private void PushIntoLastUV(ushort who, string msg)
        {
            // U5, U7, V3, V5 is not accepted
            //if (!LastUVs.ContainsKey(who))
            //    LastUVs.Add(who, new Stack<string>());
            //LastUVs[who].Push(msg);
            LastUVs[who] = msg;
        }
        private void PushIntoLastUV(IEnumerable<ushort> whos, string msg)
        {
            foreach (ushort ut in whos)
                PushIntoLastUV(ut, msg);
        }
        private bool MatchedPopFromLastUV(ushort who, string msg)
        {
            if (LastUVs.ContainsKey(who) && LastUVs[who] != null)
            {
                string lsg = LastUVs[who];
                bool u2 = msg.StartsWith("U2") && lsg.StartsWith("U1");
                bool u4 = msg.StartsWith("U4") && lsg.StartsWith("U3");
                bool u24 = msg.StartsWith("U24") && (lsg.StartsWith("U1") || lsg.StartsWith("U3"));
                bool u8 = msg.StartsWith("U8") && lsg.StartsWith("U7");
                bool v1 = msg.StartsWith("V1") && lsg.StartsWith("V0");
                bool v4 = msg.StartsWith("V4") && lsg.StartsWith("V2");
                bool v5 = msg.StartsWith("V5") && lsg.StartsWith("V2");
                if (u2 || u4 || u24 || u8 || v1 || v4 || v5)
                {
                    //LastUVs[who].Pop();
                    LastUVs[who] = null;
                    return true;
                }
            }
            return false;
        }
        private bool MatchedPopFromLastUV(IEnumerable<ushort> whos, string msg)
        {
            bool ans = true;
            foreach (ushort ut in whos)
                ans &= MatchedPopFromLastUV(ut, msg);
            return ans;
        }
        private void ResumeLostInputEvent()
        {
            foreach (var pair in LastUVs)
            {
                if (pair.Value != null)
                    WI.Send(pair.Value, 0, pair.Key);
            }
        }

        private UEchoCode HandleUMessage(string msg, List<SKE> purse,
            ushort from, bool[] involved, int sina)
        {
            return HandleUMessage(msg, purse, from, involved, Util.RepeatToArray(sina, involved.Length));
        }
        private UEchoCode HandleUMessage(string msg, List<SKE> purse,
            ushort from, bool[] involved, int[] sina)
        {
            if (msg.EndsWith(VI.CinSentinel))
                return UEchoCode.END_CANCEL;
            else
            {
                int idx = msg.IndexOf(',');
                string cop = msg.Substring(0, idx);
                string[] rests = Util.Splits(msg.Substring(idx + 1), ";;");
                UEchoCode code;
                if (cop == "U2")
                    code = HandleU2Message(from, involved, purse, rests[0], sina);
                else if (cop == "U4")
                    code = HandleU4Message(from, involved, purse, rests[0]);
                else
                    code = UEchoCode.STRANGE;
                return code;
            }
        }
        // return next stage: 1 - resend for invalid input, 3 - wait, 5 - OK, take action
        private UEchoCode HandleU2Message(ushort from, bool[] involved,
            List<SKE> pocket, string mai, int[] sina)
        {
            var garden = Board.Garden;
            if (mai == "0")
            {
                if (involved[from])
                {
                    VI.Cout(0, "{0}宣布放弃行动.", DisplayPlayer(from));
                    involved[from] = false;
                    if ((sina[from] & 2) == 0)
                        WI.BCast("UC," + from + ";;0");
                }
                MatchedPopFromLastUV(from, "U2");
                return UEchoCode.END_CANCEL;
            }
            else if (mai.StartsWith("0,"))
            {
                if (involved[from])
                {
                    VI.Cout(0, "{0}无法行动.", DisplayPlayer(from));
                    involved[from] = false;
                    if ((sina[from] & 2) == 0)
                        WI.BCast("UC," + from + ";;0," + sina[from]);
                }
                MatchedPopFromLastUV(from, "U2");
                return UEchoCode.END_CANCEL;
            }
            else
            {
                string skName;
                mai = DecodeSimplifiedCommand(mai, out skName);
                SKE ske = SKE.Find(skName, from, pocket);
                return HandleU24Message(from, involved, mai, ske);
            }
        }
        private UEchoCode HandleU4Message(ushort from, bool[] involved, List<SKE> pocket, string mai)
        {
            var garden = Board.Garden;
            if (mai.Equals("0"))
            { // Cancel the action
                MatchedPopFromLastUV(from, "U4");
                return UEchoCode.RE_REQUEST;
            }
            else
            {
                int idx = mai.IndexOf(',');
                string skName = mai.Substring(0, idx);
                SKE ske = SKE.Find(skName, from, pocket);
                return HandleU24Message(from, involved, mai, ske);
            }
        }
        private UEchoCode HandleU24Message(ushort from, bool[] involved, string mai, SKE ske)
        {
            var garden = Board.Garden;
            UEchoCode u5ed = UEchoCode.END_CANCEL; int idx = mai.IndexOf(',');
            MatchedPopFromLastUV(from, "U24");
            var skName = ske != null ? ske.Name : null;
            if (ske != null && sk01.ContainsKey(skName))
            {
                Skill skill = sk01[skName];
                string args = (idx < 0) ? "" : mai.Substring(idx + 1);
                // judge whether args is complete
                string lf = (skill.IsLinked(ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                string otherPara = skill.Input(garden[from], ske.InType, lf, args);
                if (otherPara == "")
                {
                    // OK, done.
                    string enc = skill.Encrypt(args);
                    string sTop = "U5," + from + ";;" + skName;
                    string sType = ";;" + ske.InType;

                    string mMsg = sTop + (args != "" ? "," + args : "") + sType;
                    string mEnc = sTop + (enc != "" ? "," + enc : "") + sType;

                    WI.Send(mMsg, 0, from);
                    WI.Send(mEnc, ExceptStaff(from));
                    WI.Live(mEnc + sType);
                    skill.Action(garden[from], ske.InType, lf, args);
                    ++ske.Tick;
                    u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                }
                else // need further support
                {
                    string mU3 = "U3," + otherPara + ";;" + mai + ";;" + ske.InType;
                    PushIntoLastUV(from, mU3);
                    WI.Send(mU3, 0, from);
                    u5ed = UEchoCode.NEXT_STEP;
                }
            }
            else if (ske != null && tx01.ContainsKey(skName))
            {
                Base.Card.Tux tux = tx01[skName];
                int jdx = mai.IndexOf(',', idx + 1);
                ushort ccode = ushort.Parse(Util.Substring(mai, idx + 1, jdx));
                string args = mai.Substring(idx + 1);
                //if (!tux.IsEq[ske.InType])
                if (!tux.IsTuxEqiup())
                {
                    string otherPara = tux.Input(garden[from], ske.InType, ske.Fuse, args);
                    if (otherPara == "")
                    {
                        // OK, done.
                        string enc = tux.Encrypt(args);
                        string sTop = "U5," + from + ";;" + skName;
                        string sType = ";;" + ske.InType;

                        string mMsg = sTop + (args != "" ? "," + args : "") + sType;
                        string mEnc = sTop + (enc != "" ? "," + enc : "") + sType;

                        string cargs = (args == "^") ? "" : "," + args;
                        if (tux.Type == Base.Card.Tux.TuxType.ZP)
                            RaiseGMessage("G0CZ,0," + from);
                        RaiseGMessage("G0CC," + from + ",0," + from + "," + skName +
                            cargs + ";" + ske.InType + "," + ske.Fuse);
                        ++ske.Tick;
                        u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    }
                    else // need further support
                    {
                        string mU3 = "U3," + otherPara + ";;" + mai + ";;" + ske.InType;
                        PushIntoLastUV(from, mU3);
                        WI.Send(mU3, 0, from);
                        u5ed = UEchoCode.NEXT_STEP;
                    }
                }
                else if (ske.Type == SKTType.TX)
                {
                    WI.BCast("U5," + from + ";;" + skName + "," + args + ";;" + ske.InType);
                    //RaiseGMessage("G0ZB," + from + ",0," + ccode);
                    if (tux.IsTuxEqiup())
                    {
                        Base.Card.TuxEqiup tue = tux as Base.Card.TuxEqiup;
                        tue.UseAction(ccode, Board.Garden[from]);
                        u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    }
                    //else
                    //{
                    //    RaiseGMessage("G0ZB," + from + ",3," + ccode);
                    //    u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    //}
                }
                else
                {
                    Base.Card.TuxEqiup tue = (Base.Card.TuxEqiup)tux;
                    // args include card code now.
                    int consumeCode = ske.Consume;
                    string prev = args.IndexOf(',') < 0 ? args : args.Substring(args.IndexOf(',') + 1);
                    string lf = (tue.IsLinked(consumeCode, ske.InType) ? ske.LinkFrom + ":" : "") + ske.Fuse;
                    string otherPara = tue.ConsumeInput(garden[from], consumeCode, ske.InType, lf, prev);
                    if (otherPara == "")
                    {
                        string enc = tue.Encrypt(args);
                        string sTop = "U5," + from + ";;" + skName;
                        string sType = ";;" + ske.InType + "!" + consumeCode;

                        string mMsg = sTop + "," + args + sType;
                        string mEnc = sTop + (enc != "" ? "," + enc : "") + sType;

                        WI.Send(mMsg, 0, from);
                        WI.Send(mEnc, ExceptStaff(from));
                        WI.Live(mEnc);
                        RaiseGMessage("G0ZC," + from + "," + consumeCode +
                               "," + args + ";" + ske.InType + "," + lf);
                        u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    }
                    else // need further support
                    {
                        string mU3 = "U3," + otherPara + ";;" + mai + ";;" + ske.InType + "!" + consumeCode;
                        PushIntoLastUV(from, mU3);
                        WI.Send(mU3, 0, from);
                        u5ed = UEchoCode.NEXT_STEP;
                    }
                }
            }
            else if (ske != null && cz01.ContainsKey(skName))
            {
                Operation cz = cz01[skName];
                string args = (idx < 0) ? "" : mai.Substring(idx + 1);
                // judge whether args is complete
                string otherPara = cz.Input(garden[from], ske.Fuse, args);
                if (otherPara == "")
                {
                    // OK, done.
                    //string enc = c.Encrypt(args);
                    string sTop = "U5," + from + ";;" + skName;
                    string sType = ";;" + ske.InType;
                    WI.BCast(sTop + (args != "" ? "," + args : "") + sType);
                    cz.Action(garden[from], ske.Fuse, args);
                    u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    ++ske.Tick;
                }
                else // need further support
                {
                    string mU3 = "U3," + otherPara + ";;" + mai + ";;" + ske.InType;
                    PushIntoLastUV(from, mU3);
                    WI.Send(mU3, 0, from);
                    u5ed = UEchoCode.NEXT_STEP;
                }
            }
            else if (ske != null && nj01.ContainsKey(skName))
            {
                NCAction na = nj01[skName];
                string args = (idx < 0) ? "" : mai.Substring(idx + 1);
                // judge whether args is complete
                string otherPara = na.Input(garden[from], ske.Fuse, args);
                if (otherPara == "")
                {
                    // OK, done.
                    //string enc = c.Encrypt(args);
                    string sTop = "U5," + from + ";;" + skName;
                    string sType = ";;" + ske.InType;
                    WI.BCast(sTop + (args != "" ? "," + args : "") + sType);
                    na.Action(garden[from], ske.Fuse, args);
                    u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    ++ske.Tick;
                }
                else // need further support
                {
                    string mU3 = "U3," + otherPara + ";;" + mai + ";;" + ske.InType;
                    PushIntoLastUV(from, mU3);
                    WI.Send(mU3, 0, from);
                    u5ed = UEchoCode.NEXT_STEP;
                }
            }
            else if (ske != null && mt01.ContainsKey(skName))
            {
                Base.Card.Monster mt = mt01[skName];
                int jdx = mai.IndexOf(',', idx + 1);
                ushort mcode = ushort.Parse(Util.Substring(mai, idx + 1, jdx));
                string args = mai.Substring(idx + 1);
                // args include card code now.
                int consumeCode = ske.Consume;
                string otherPara = mt.ConsumeInput(garden[from], consumeCode, ske.InType, ske.Fuse, args);
                if (otherPara == "")
                {
                    string sTop = "U5," + from + ";;" + skName;
                    string sType = ";;" + ske.InType + "!" + consumeCode;
                    WI.BCast(sTop + "," + args + sType);
                    RaiseGMessage("G0HH," + from + "," + consumeCode +
                           "," + args + ";" + ske.InType + "," + ske.Fuse);
                    u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                    ++ske.Tick;
                }
                else // need further support
                {
                    string mU3 = "U3," + otherPara + ";;" + mai + ";;" + ske.InType + "!" + consumeCode;
                    PushIntoLastUV(from, mU3);
                    WI.Send(mU3, 0, from);
                    u5ed = UEchoCode.NEXT_STEP;
                }
            }
            else if (ske != null && ev01.ContainsKey(skName))
            {
                Base.Card.Evenement ev = ev01[skName];
                string args = (idx < 0) ? "" : mai.Substring(idx + 1);
                // args include card code now.
                string sTop = "U5," + from + ";;" + skName;
                string sType = ";;" + ske.InType;
                WI.BCast(sTop + sType);
                ev.Pers(Board.Garden[from]);
                u5ed = ske.IsTermini ? UEchoCode.END_TERMIN : UEchoCode.END_ACTION;
                ++ske.Tick;
            }
            else // invalid input, repost U1
                u5ed = UEchoCode.RE_REQUEST;
            return u5ed;
        }
        // U7/9 Events
        public string AsyncInput(ushort who, string mai, string prev, string inType)
        {
            if (!string.IsNullOrEmpty(mai))
            {
                WI.Send("U9," + who + ";;" + prev + ";;" + inType, ExceptStaff(who));
                WI.Live("U9," + who + ";;" + prev + ";;" + inType);
                string mU7 = "U7," + who + ";;" + mai + ";;" + prev + ";;" + inType;
                PushIntoLastUV(who, mU7);
                WI.Send(mU7, 0, who);
                string input = WI.Recv(0, who);
                while (input == null || !input.StartsWith("U8,"))
                {
                    input = WI.Recv(0, who);
                    System.Threading.Thread.Sleep(100);
                }
                // Format: U8,K,x2,y2
                MatchedPopFromLastUV(who, input);
                string result = input.Substring("U8,".Length);
                return result.Substring(prev.Length + 1);
            } else
                return "";
        }

        public void SendOutU1Message(bool[] invs, string[] mais, int sina)
        {
            SendOutU1Message(invs, mais, Util.RepeatToArray(sina, invs.Length));
        }
        public void SendOutU1Message(bool[] invs, string[] mais, int[] sina)
        {
            string inv = string.Join(",", Board.Garden.Keys.Where(p => invs[p]));
            foreach (ushort ut in Board.Garden.Keys)
            {
                string mU1;
                if (mais[ut] != "" && mais[ut] != "0")
                    mU1 = "U1," + inv + ";;" + mais[ut];
                else if (Board.Garden[ut].IsAlive)
                    mU1 = "U1," + inv + ";;0," + sina[ut];
                else
                    mU1 = "U1," + inv + ";;0," + (sina[0] & (~1));
                PushIntoLastUV(ut, mU1);
                WI.Send(mU1, 0, ut);
            }
            WI.Live("U1," + inv + ";;0," + (sina[0] & (~1)));
        }
        public void ResendU1Message(ushort who, bool[] invs,
            string[] mais, bool critical, int sina)
        {
            ResendU1Message(who, invs, mais, critical, Util.RepeatToArray(sina, invs.Length));
        }
        public void ResendU1Message(ushort who, bool[] invs,
            string[] mais, bool critical, int[] sina)
        {
            if (critical && !invs[who])
            {
                invs[who] = true;
                WI.Send("UB,0", 0, who);
            }
            if (invs[who])
            {
                string inv = string.Join(",", Board.Garden.Keys.Where(p => invs[p]));
                string mU1;
                if (mais[who] != "" && mais[who] != "0")
                    mU1 = "U1," + inv + ";;" + mais[who];
                else
                    mU1 = "U1," + inv + ";;0," + sina[who];
                PushIntoLastUV(who, mU1);
                WI.Send(mU1, 0, who);
            }
        }
        public void SendOutU5Message(ushort who, string mai, string inType)
        {
            WI.BCast("U5," + who + ";;" + mai + ";;" + inType);
        }
        public void SendOutUAMessage(ushort who, string mai, string inType)
        {
            WI.BCast("UA," + who + ";;" + mai + ";;" + inType);
        }
        public IDictionary<ushort, string> MultiAsyncInput(IDictionary<ushort, string> dicts)
        {
            IDictionary<ushort, string> result = new Dictionary<ushort, string>();
            if (dicts != null && dicts.Count > 0)
            {
                foreach (var pair in dicts)
                {
                    string mV0 = "V0," + dicts.Count + "," + string.Join(",", dicts.Keys) + "," + pair.Value;
                    PushIntoLastUV(pair.Key, mV0);
                    WI.Send(mV0, 0, pair.Key);
                }
                // notify the others of waiting
                WI.Send("V3," + string.Join(",", dicts.Keys),
                    ExceptStaff(dicts.Keys.ToArray()));
                WI.Live("V3," + string.Join(",", dicts.Keys));
                WI.RecvInfStart();
                while (dicts.Count > 0)
                {
                    Base.VW.Msgs msg = WI.RecvInfRecvPending();
                    MatchedPopFromLastUV(msg.From, msg.Msg);
                    result.Add(msg.From, msg.Msg.Substring("V1,".Length));
                    dicts.Remove(msg.From);
                    RaiseGMessage("G2AS," + msg.From);
                }
                WI.RecvInfEnd();
            }
            return result;
        }
        // Major Query Case, $citizens contains who can vote for advice and its input format
        // $handleCitizenAdvices tell how to handle with advice.
        // return the major's decision.
        public string MajorAsyncInput(ushort major, string majorMsg,
            IDictionary<ushort, string> citizens, Action<ushort, string> handleCitizenAdvices)
        {
            string mV2 = "V2," + major + "," + majorMsg;
            PushIntoLastUV(major, mV2);
            WI.Send(mV2, 0, major);
            foreach (var pair in citizens)
            {
                mV2 = "V2," + major + "," + pair.Value;
                PushIntoLastUV(pair.Key, mV2);
                WI.Send(mV2, 0, pair.Key);
            }
            List<ushort> invs = citizens.Keys.ToList(); invs.Add(major);
            WI.Send("V3," + major, ExceptStaff(invs.ToArray()));
            WI.Live("V3," + major);
            WI.RecvInfStart();
            while (true)
            {
                Base.VW.Msgs msg = WI.RecvInfRecvPending();
                if (string.IsNullOrEmpty(msg.Msg))
                    break;
                if (msg.From == major)
                {
                    MatchedPopFromLastUV(msg.From, msg.Msg);
                    string decision = msg.Msg.Substring("V4,".Length);
                    MatchedPopFromLastUV(ExceptStaff(major), "V5,0");
                    WI.Send("V5,0", ExceptStaff(major));
                    WI.Live("V5,0");
                    return msg.Msg.Substring("V4,".Length);
                }
                else if (citizens.ContainsKey(msg.From))
                {
                    string advice = msg.Msg.Substring("V4,".Length);
                    handleCitizenAdvices(msg.From, advice);
                }
            }
            return "";
        }
        public string MajorAsyncInput(ushort major, string majorMsg, IEnumerable<ushort> citizens,
            string citizenMsg, Action<ushort, string> handleCitizenAdvices)
        {
        	IDictionary<ushort, string> citizenDict = new Dictionary<ushort, string>();
        	foreach (ushort ut in citizens)
        		citizenDict[ut] = citizenMsg;
    		return MajorAsyncInput(major, majorMsg, citizenDict, handleCitizenAdvices);
        }
        // public bool MultiSyncReply(IDictionary<ushort, string> dicts)
        // {
        //     try
        //     {
        //         foreach (var pair in dicts)
        //             WI.Send("V2," + pair.Value, 0, pair.Key);
        //         WI.RecvInfStart();
        //         while (dicts.Count > 0)
        //         {
        //             Base.VW.Msgs msg = WI.RecvInfRecvPending();
        //             dicts.Remove(msg.From);
        //         }
        //         WI.RecvInfEnd();
        //     }
        //     catch (Exception) { return false; }
        //     return true;
        // }
        private void HandleYMessage(string msg, ushort who)
        {
            if (msg.StartsWith("Y1,"))
            {
                string say = "Y2," + who + "," + msg.Substring("Y1,".Length);
                if ((WI as VW.Aywi).IsTalkSilence)
                    WI.Send(say, Board.Garden.Keys.Where(p => (p % 2 == who % 2)).ToArray());
                else
                    WI.BCast(say);
            }
            else if (msg.StartsWith("Y3,"))
            {
                ushort opt = ushort.Parse(msg.Substring("Y3,".Length));
                if (Board.Garden.ContainsKey(who))
                {
                    if (opt == 1) // SK_OPT
                        Board.Garden[who].IsSKOpt = true;
                    else if (opt == 2)
                        Board.Garden[who].IsSKOpt = false;
                    else if (opt == 3) // TP_OPT
                        Board.Garden[who].IsTPOpt = true;
                    else if (opt == 4)
                        Board.Garden[who].IsTPOpt = false;
                    WI.Send("Y4," + opt, 0, who);
                }
            }
        }

        private string DecodeSimplifiedCommand(string mai, out string skName)
        {
            int idx = mai.IndexOf(',');
            skName = idx < 0 ? mai : mai.Substring(0, idx);
            string comrest = idx < 0 ? "" : mai.Substring(idx);
            // BK: JN60102(2) => JN60102,2
            int jdx = mai.IndexOf('(');
            if (jdx >= 0)
            {
                int kdx = mai.IndexOf(')');
                ushort owner = ushort.Parse(Util.Substring(skName, jdx + 1, kdx));
                skName = skName.Substring(0, jdx);
                mai = skName + "," + owner + comrest;
            }
            // TX: TX2 => TP01,2
            if (skName.StartsWith("TX"))
            {
                ushort card = ushort.Parse(skName.Substring("TX".Length));
                skName = LibTuple.TL.DecodeTux(card).Code;
                mai = skName + "," + card + comrest;
            }
            // PT: PT16 => GF04,16
            else if (skName.StartsWith("PT"))
            {
                ushort card = ushort.Parse(skName.Substring("PT".Length));
                skName = LibTuple.ML.Decode(card).Code;
                mai = skName + "," + card + comrest;
            }
            return mai;
        }
    }
}
