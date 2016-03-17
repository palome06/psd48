using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.ClientAo.VW
{
    public class Eywi : Base.VW.IWICL
    {
        // private IEnumerator<string> iter;
        private List<string> dixits;
        private int dixitCursor;

        public int Version { set; get; }
        public ushort Uid { set; get; }

        private float[] magnification;
        private string[] magnificationstr;
        private int mMagIndex;
        [STAThread]
        private void SetMagIndex(int value)
        {
            if (mMagIndex != value)
            {
                if (value >= magnification.Length)
                    mMagIndex = magnification.Length - 1;
                else if (value < 0)
                    mMagIndex = 0;
                else
                    mMagIndex = value;
            }
        }
        public int MagIndex
        {
            set { SetMagIndex(value); }
            get { return mMagIndex; }
        }
        private bool mInProcess;
        [STAThread]
        private void SetInProcess(bool value)
        {
            mInProcess = value;
        }
        public bool InProcess
        {
            set { SetInProcess(value); }
            get { return mInProcess; }
        }
        public int Duration { get { return (int)(200 / magnification[MagIndex]); } }
        public string CurrentMagi { get { return magnificationstr[MagIndex]; } }

        public Eywi(string fileName)
        {
            IEnumerator<string> iter = File.ReadLines(fileName).GetEnumerator();
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    Version = int.Parse(firsts[0].Substring("VERSION=".Length));
                if (firsts[1].StartsWith("UID="))
                    Uid = ushort.Parse(firsts[1].Substring("UID=".Length));
            }
            magnification = new float[] { 0.25f, 0.5f, 1, 2, 4, 8 };
            magnificationstr = new string[] { "0.25", "0.5", "1", "2", "4", "8" };
            mMagIndex = 2;
            mInProcess = true;
            yMessage = new Queue<string>();

            dixits = new List<string>();
            while (iter.MoveNext())
                dixits.Add(iter.Current);
            dixitCursor = 0;
        }

        public string Recv(ushort me, ushort from)
        {
            if (mInProcess)
            {
                while (dixitCursor < dixits.Count)
                {
                    string line = dixits[dixitCursor];
                    string prefix = (dixitCursor + 1 < dixits.Count &&
                         IsClogFreeUIEvent(dixits[dixitCursor + 1])) ? "<|>" : "";
                    ++dixitCursor;
                    if (Version >= 99)
                    {
                        line = Base.LogES.DESDecrypt(line, "AKB48Show!",
                            (Version * Version).ToString());
                    }
                    if (!string.IsNullOrEmpty(line))
                    {
                        if (line.StartsWith("<"))
                            line = line.Substring("<".Length);
                        else if (line.StartsWith(">"))
                            continue;
                    }
                    HandleWithVersion(ref line, Version);
                    if (line.StartsWith("Y"))
                    {
                        lock (yMessage)
                            yMessage.Enqueue(line);
                    }
                    else
                        return prefix + line;
                }
            }
            return null;
        }

        public void Send(string msg, ushort me, ushort to) { }
        public void SendDirect(string msg, ushort me) { }
        public void Close() { }

        private Queue<string> yMessage;

        public string Hear()
        {
            lock (yMessage)
            {
                if (yMessage.Count > 0)
                    return yMessage.Dequeue();
            }
            return null;
        }
        // to indicate whether $message is clog-free, then show it directly.
        // e.g. Target event
        public bool IsClogFreeUIEvent(string message)
        {
            string head = Algo.Substring(message, 0, message.IndexOf(','));
            string[] clogfrees = new string[] { "E0AS", "E0YS" };
            return clogfrees.Contains(head);
        }

        #region Version
        private void HandleWithVersion(ref string line, int Version)
        {
            if (Version <= 101)
            {
                if (line.StartsWith("H09G"))
                {
                    string[] blocks = line.Split(',');
                    for (int idx = 1; idx < blocks.Length;)
                    {
                        int nextIdx = idx + 18;
                        int excdsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<ushort> excards = Algo.TakeRange(blocks, nextIdx,
                        //    nextIdx + excdsz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += excdsz;
                        int fakeqsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<ushort> fakeqs = Algo.TakeRange(blocks, nextIdx,
                        //    nextIdx + fakeqsz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += fakeqsz;
                        int token = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        int peoplesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<string> peoples = Algo.TakeRange(blocks, nextIdx,
                        //    nextIdx + peoplesz).ToList();
                        nextIdx += peoplesz;

                        int @int = int.Parse(blocks[nextIdx]); // target
                        blocks[nextIdx] = "1," + @int;

                        int escuesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<ushort> escues = Algo.TakeRange(blocks, nextIdx,
                        //    nextIdx + escuesz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += escuesz;

                        idx = nextIdx;
                    }
                    line = string.Join(",", blocks);
                }
                if (line.StartsWith("E0IJ"))
                {
                    string[] e0ij = line.Split(',');
                    if (e0ij[2] == "1")
                        e0ij[2] = "1,1";
                    else if (e0ij[2] == "2")
                    {
                        string x = e0ij[3];
                        e0ij[3] = "1," + x + ",1," + x;
                    }
                    line = string.Join(",", e0ij);
                }
                else if (line.StartsWith("E0OJ"))
                {
                    string[] e0oj = line.Split(',');
                    if (e0oj[2] == "1")
                        e0oj[2] = "1,1";
                    else if (e0oj[2] == "2")
                        e0oj[2] = "2,0";
                    line = string.Join(",", e0oj);
                }
            }
            if (Version <= 107)
            {
                if (line.StartsWith("V0,"))
                {
                    string rest = line.Substring("V0,".Length);
                    line = "V0,1," + Uid + "," + rest;
                }
            }
            if (Version <= 110)
            {
                if (line.StartsWith("H0SM"))
                    line += ",4";
                else if (line[0] == 'R' && line.Substring(2, 3) == "ZW5")
                {
                    string[] blocks = line.Split(',');
                    if (blocks.Length >= 3)
                    {
                        if (Char.IsDigit(blocks[2][0]))
                            blocks[2] = "T" + blocks[2];
                        else if (blocks[2] == "0")
                            blocks[2] = "/0";
                        line = string.Join(",", blocks);
                    }
                }
            }
            if (Version <= 112)
            {
                if (line.StartsWith("E0QC,"))
                    line = "E0QC,1," + line.Substring("E0QC,".Length);
            }
            if (Version <= 114)
            {
                if (line.StartsWith("E0ON"))
                {
                    string[] args = line.Split(',');
                    ushort utype = ushort.Parse(args[1]);
                    char ch = '\0';
                    if (utype == 0)
                        ch = 'C';
                    else if (utype == 1)
                        ch = 'M';
                    else if (utype == 2)
                        ch = 'E';
                    if (ch != '\0')
                    {
                        line = "E0ON,10," + ch + "," + (args.Length - 2) + ","
                            + string.Join(",", Algo.TakeRange(args, 2, args.Length));
                    }
                }
                else if (line.StartsWith("E0CC"))
                {
                    string[] args = line.Split(',');
                    if (args[2] == "0")
                        args[2] = args[1];
                    args[2] = "0," + args[2];
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0CD"))
                {
                    string[] args = line.Split(',');
                    args[1] = args[1] + ",0";
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0CE"))
                {
                    string[] args = line.Split(',');
                    args[1] = args[1] + ",0";
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0ZB"))
                {
                    string[] args = line.Split(',');
                    ushort type = ushort.Parse(args[2]);
                    if (type == 0)
                    {
                        ushort where = ushort.Parse(args[3]);
                        if (where == 4)
                        {
                            args[4] += ",0";
                            line = string.Join(",", args);
                        }
                    }
                    else if (type == 1)
                    {
                        ushort where = ushort.Parse(args[4]);
                        if (where == 4)
                        {
                            args[4] += ",0";
                            line = string.Join(",", args);
                        }
                    }
                }
                else if (line.StartsWith("H09F"))
                    line += ",0";
            }
            if (Version <= 115)
            {
                if (line.StartsWith("E0IA") || line.StartsWith("E0IX") ||
                    line.StartsWith("E0OA") || line.StartsWith("E0OX"))
                {
                    string[] args = line.Split(',');
                    if (args[2] == "3")
                        args[2] = "2";
                    else if (args[2] == "2")
                        args[2] = "1";
                    line = string.Join(",", args);
                }
            }
            if (Version <= 121)
            {
                if (line.StartsWith("E0IE") || line.StartsWith("E0OE"))
                    line = line.Substring(0, "E0IE".Length) + ",0," + line.Substring("E0IE,".Length);
            }
            if (Version <= 131)
            {
                if (line[0] == 'R' && Algo.Substring(line, 2, 5) == "ZW7")
                {
                    string[] args = line.Substring("R#ZW7,".Length).Split(',');
                    if (args[0] == "1")
                        line = "E0FI,S,0," + args[1];
                    else if (args[0] == "2")
                        line = "E0FI,H,0," + args[1];
                }
                else if (line.StartsWith("E0AF"))
                {
                    string[] args = line.Split(',');
                    string e0fi = "";
                    for (int i = 1; i < args.Length; i += 2)
                    {
                        ushort type = ushort.Parse(args[i]);
                        if (type == 1) { e0fi += ",S,0," + args[i + 1]; }
                        else if (type == 2) { e0fi += ",H,0," + args[i + 1]; }
                        else if (type == 5) { e0fi += ",S," + args[i + 1] + "0"; }
                        else if (type == 6) { e0fi += ",H," + args[i + 1] + "0"; }
                    }
                    if (!string.IsNullOrEmpty(e0fi))
                        line = "E0FI" + e0fi;
                }
                else if (line.StartsWith("E0KI"))
                {
                    string[] args = line.Split(',');
                    IDictionary<int, int> values = new Dictionary<int, int>();
                    for (int i = 1; i < args.Length; i += 2)
                        values.Add(int.Parse(args[i]), int.Parse(args[i + 1]));
                    if (values.Count == 6)
                    {
                        foreach (var pair in values)
                        {
                            if (pair.Value == 1)
                            {
                                line = "E0FI,U," + pair.Key;
                                break;
                            }
                        }
                    }
                    else if (values.Any(p => p.Value == 4))
                    {
                        var pair = values.First(p => p.Value == 4);
                        line = "E0FI,W,0," + pair.Key;
                    }
                }
                else if (line.StartsWith("E0ON"))
                {
                    string[] args = line.Split(',');
                    for (int idx = 1; idx < args.Length;)
                    {
                        ushort fromZone = ushort.Parse(args[idx]);
                        string cardType = args[idx + 1];
                        int n = int.Parse(args[idx + 2]);
                        if (n > 0 && cardType == "E")
                        {
                            for (int i = 0; i < n; ++i)
                            {
                                ushort ut = ushort.Parse(args[idx + 3 + i]);
                                if (ut >= 7 && ut <= 30)
                                    args[idx + 3 + i] = (ut + 1).ToString();
                            }
                        }
                        idx += (3 + n);
                    }
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0YM"))
                {
                    string[] args = line.Split(',');
                    if (args[1] == "2")
                    {
                        ushort ut = ushort.Parse(args[2]);
                        if (ut >= 7 && ut <= 30)
                            args[2] = (ut + 1).ToString();
                    }
                    line = string.Join(",", args);
                }
            }
            if (Version <= 137)
            {
                if (line[0] == 'R' && Algo.Substring(line, 2, 4) == "Z3")
                    line = "E0FI,U,0";
            }
            if (Version <= 145)
            {
                if (line.StartsWith("E0FU"))
                {
                    string[] args = line.Split(',');
                    if (args[1] == "0" || args[1] == "1" || args[1] == "4")
                        args[1] += ",C";
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("H09F"))
                    line += ",0";
            }
            if (Version <= 146)
            {
                if (line.StartsWith("E0YM"))
                {
                    string[] args = line.Split(',');
                    if (args[1] == "5")
                    {
                        string pick = args[2];
                        line = "E0YM,5," + pick;
                    }
                }
            }
            if (Version <= 148)
            {
                if (line.StartsWith("E0ON"))
                {
                    string[] args = line.Split(',');
                    for (int idx = 1; idx < args.Length;)
                    {
                        ushort fromZone = ushort.Parse(args[idx]);
                        string cardType = args[idx + 1];
                        int n = int.Parse(args[idx + 2]);
                        if (n > 0 && cardType == "E")
                        {
                            for (int i = idx + 3; i < idx + 3 + n; ++i)
                            {
                                ushort ut = ushort.Parse(args[i]);
                                switch (ut)
                                {
                                    case 39: args[2] = "40"; break;
                                    case 40: args[2] = "41"; break;
                                    case 41: args[2] = "43"; break;
                                }
                            }
                        }
                        idx += (3 + n);
                    }
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0YM"))
                {
                    string[] args = line.Split(',');
                    if (args[1] == "2")
                    {
                        ushort ut = ushort.Parse(args[2]);
                        switch (ut)
                        {
                            case 39: args[2] = "40"; break;
                            case 40: args[2] = "41"; break;
                            case 41: args[2] = "43"; break;
                        }
                    }
                    line = string.Join(",", args);
                }
            }
            if (Version <= 149)
            {
                if (line.StartsWith("E0PH"))
                    line = "E0OH" + line.Substring(line.IndexOf(','));
                if (line.StartsWith("E0OH") || line.StartsWith("E0IH"))
                {
                    string[] e0h = line.Split(',');
                    for (int i = 1; i < e0h.Length; i += 4)
                    {
                        int elemCode = int.Parse(e0h[i + 1]);
                        // YIN = 6, SOL = 7, A = 8, LOVE = 9
                        if (elemCode == 6 || elemCode == 7 || elemCode == 8)
                            e0h[i + 1] = "0,0";
                        else if (elemCode == 9)
                            e0h[i + 1] = "1,0";
                        else
                            e0h[i + 1] = "0," + e0h[i + 1];
                    }
                    line = string.Join(",", e0h);
                }
                else if (line.StartsWith("E0FU"))
                {
                    string[] e0fu = line.Split(',');
                    if (e0fu[1] == "2")
                        e0fu[2] += ",C";
                    line = string.Join(",", e0fu);
                }

                int[] avatarBase = new int[] { 37, 39, 40, 41, 42, 43, 44, 45 };
                Func<int, int> offset = v => v == 45 ? 37 : (v + 1);
                string sline = line;
                string[] hin = { "IY", "OY", "IJ", "OJ", "IV", "OV", "YM" };
                if (line.StartsWith("H") || hin.Any(p => sline.StartsWith("E0" + p)))
                {
                    foreach (int ab in avatarBase)
                        line = line.Replace("170" + ab, "1^7^0" + ab);
                    foreach (int ab in avatarBase)
                        line = line.Replace("1^7^0" + ab, "170" + offset(ab));
                }
                string[] uis = { "IS", "OS" };
                if (line.StartsWith("H") || line.StartsWith("U") || line.StartsWith("V") || uis.Any(p => sline.StartsWith("E0" + p)))
                {
                    foreach (int ab in avatarBase)
                        line = line.Replace("JNT" + ab, "J^N^JNT" + ab);
                    foreach (int ab in avatarBase)
                        line = line.Replace("J^N^JNT" + ab, "JNT" + offset(ab));
                }
                Func<ushort, string> npcUpdate = v => v == 1079 ? "1071" : (v >= 1071 && v < 1079) ? (v + 1).ToString() : v.ToString();
                if (line.StartsWith("E0IL") || line.StartsWith("E0OL"))
                {
                    string[] e0ls = line.Split(',');
                    for (int idx = 1; idx < e0ls.Length; idx += 2)
                        e0ls[idx + 1] = npcUpdate(ushort.Parse(e0ls[idx + 1]));
                    line = string.Join(",", e0ls);
                }
                else if (line.StartsWith("E0HZ"))
                {
                    string[] e0hz = line.Split(',');
                    if (e0hz[1] != "0")
                        e0hz[3] = npcUpdate(ushort.Parse(e0hz[3]));
                    line = string.Join(",", e0hz);
                }
                else if (line.StartsWith("E0YM"))
                {
                    string[] args = line.Split(',');
                    ushort atype = ushort.Parse(args[1]);
                    if (atype == 0 || atype == 1 || atype == 3)
                    {
                        args[2] = npcUpdate(ushort.Parse(args[2]));
                        line = string.Join(",", args);
                    }
                    else if (atype == 5)
                    {
                        for (int i = 2; i < args.Length; ++i)
                            args[i] = npcUpdate(ushort.Parse(args[i]));
                        line = string.Join(",", args);
                    }
                    else if (args[1] == "6")
                    {
                        for (int i = 3; i < args.Length; ++i)
                            args[i] = npcUpdate(ushort.Parse(args[i]));
                        line = string.Join(",", args);
                    }
                }
            }
            if (Version <= 150)
            {
                if (line.StartsWith("E0ON"))
                {
                    string[] args = line.Split(',');
                    for (int idx = 1; idx < args.Length;)
                    {
                        ushort fromZone = ushort.Parse(args[idx]);
                        string cardType = args[idx + 1];
                        int n = int.Parse(args[idx + 2]);
                        if (n > 0 && cardType == "E")
                        {
                            for (int i = idx + 3; i < idx + 3 + n; ++i)
                            {
                                ushort ut = ushort.Parse(args[i]);
                                switch (ut)
                                {
                                    case 38: args[i] = "43"; break;
                                    case 39: args[i] = "44"; break;
                                    case 40: args[i] = "46"; break;
                                    case 41: args[i] = "47"; break;
                                    case 42: args[i] = "48"; break;
                                    case 43: args[i] = "49"; break;
                                }
                            }
                        }
                        idx += (3 + n);
                    }
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0YM"))
                {
                    string[] args = line.Split(',');
                    if (args[1] == "2")
                    {
                        ushort ut = ushort.Parse(args[2]);
                        switch (ut)
                        {
                            case 38: args[2] = "43"; break;
                            case 39: args[2] = "44"; break;
                            case 40: args[2] = "46"; break;
                            case 41: args[2] = "47"; break;
                            case 42: args[2] = "48"; break;
                            case 43: args[2] = "49"; break;
                        }
                        line = string.Join(",", args);
                    }
                    else if (args[1] == "3")
                        line = line.Substring(0, line.Length - ",0".Length);
                }
            }
            if (Version <= 151)
            {
                if (line.StartsWith("E0ZB"))
                {
                    // old where map to new slot
                    ushort[] slotMap = { 0, 1, 2, 5, 6, 4, 3 };
                    string[] e0zb = line.Split(',');
                    if (e0zb[2] == "0")
                    {
                        int where = int.Parse(e0zb[3]);
                        if (where == 4)
                            line = "E0ZB," + e0zb[1] + "," + e0zb[1] + ",6," + e0zb[4] + "," + e0zb[5];
                        else
                            line = "E0ZB," + e0zb[1] + "," + e0zb[1] + "," + slotMap[where] + "," + e0zb[4];
                    }
                    else if (e0zb[2] == "1")
                    {
                        ushort from = ushort.Parse(e0zb[3]);
                        int where = int.Parse(e0zb[4]);
                        if (where == 4)
                            line = "E0ZB," + e0zb[1] + "," + from + ",6," + e0zb[5] + "," + e0zb[6];
                        else
                            line = "E0ZB," + e0zb[1] + "," + from + "," + slotMap[where] + "," + e0zb[5];
                    }
                }
                else if (line.StartsWith("E0QU"))
                {
                    string[] e0qu = line.Split(',');
                    if (e0qu[1] == "0")
                    {
                        e0qu[1] = "0,C";
                        line = string.Join(",", e0qu);
                    }
                }
                else if (line.StartsWith("E0HC"))
                {
                    string[] e0hc = line.Split(',');
                    int type = int.Parse(e0hc[1]);
                    ushort who = ushort.Parse(e0hc[2]);
                    if (type == 0)
                        line = "E0HC," + who + "," + string.Join(",", Algo.TakeRange(e0hc, 4, e0hc.Length));
                    else if (type == 1)
                        line = "E0HC," + who + "," + string.Join(",", Algo.TakeRange(e0hc, 5, e0hc.Length));
                }
                else if (line.StartsWith("E0SW"))
                {
                    string[] e0sw = line.Split(',');
                    e0sw[0] += ",3";
                    if (e0sw[2] == "0")
                        e0sw[2] = "C";
                    else if (e0sw[2] == "1")
                        e0sw[2] = "M";
                    else if (e0sw[2] == "2")
                        e0sw[2] = "E";
                    line = string.Join(",", e0sw);
                }
                else if (line.StartsWith("E0FU"))
                {
                    string[] e0fu = line.Split(',');
                    if (e0fu[1] == "2")
                        line = "E0SW" + line.Substring("E0FU".Length);
                    else if (e0fu[1] == "5")
                    {
                        ushort who = ushort.Parse(e0fu[2]);
                        string[] cards = Algo.TakeRange(e0fu, 3, e0fu.Length);
                        line = "E0SW,2," + who + ",G," + cards;
                    }
                }
                else if (line.StartsWith("E0HH"))
                {
                    string[] e0hh = line.Split(',');
                    ushort consumeType = ushort.Parse(e0hh[2]);
                    if (consumeType == 1)
                    {
                        ushort me = ushort.Parse(e0hh[1]);
                        ushort mons = ushort.Parse(e0hh[3]);
                        line = "E0HI," + me + "," + mons;
                    }
                }
                else if (line.StartsWith("E0ZC"))
                {
                    string[] e0zc = line.Split(',');
                    ushort me = ushort.Parse(e0zc[1]);
                    ushort consumeType = ushort.Parse(e0zc[2]);
                    // ushort where = ushort.Parse(args[3]);
                    ushort card = ushort.Parse(e0zc[4]);
                    if (consumeType == 2)
                        line = "E0ZI," + me + "," + card;
                    else
                    {
                        string rest = string.Join(",", Algo.TakeRange(e0zc, 4, e0zc.Length));
                        line = "E0ZC," + me + "," + consumeType + "," + rest;
                    }
                }
            }
            if (Version <= 154)
            {
                if (line.StartsWith("H09G"))
                {
                    string[] h09g = line.Split(',');
                    h09g[1] = "H09G";
                    h09g[8] += ",1"; // Add SupportSucc after Supporter
                    h09g[9] += ",1,0,0"; // Add HinderSucc, Drums and Wang after Hinder
                    line = string.Join(",", Algo.TakeRange(h09g, 1, h09g.Length)); // remove [1]=Eve
                }
                else if (line.StartsWith("E0ZC"))
                {
                    string[] e0zc = line.Split(',');
                    e0zc[2] += ",0"; // Add target = 0 after consumeType
                    line = string.Join(",", e0zc);
                }
            }
            if (Version <= 156)
            {
                if (line.StartsWith("E0YM"))
                {
                    string[] e0ym = line.Split(',');
                    if (e0ym[1] == "3")
                    {
                        e0ym[2] = "0," + e0ym[2];
                        line = string.Join(",", e0ym);
                    }
                }
            }
        }
        #endregion Version
    }
}
