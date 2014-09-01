using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PSD.ClientAo.VW
{
    public class Eywi : Base.VW.IWICL
    {
        private IEnumerator<string> iter;

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
            iter = File.ReadLines(fileName).GetEnumerator();
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
        }

        public string Recv(ushort me, ushort from)
        {
            if (mInProcess)
            {
                while (iter.MoveNext())
                {
                    string line = iter.Current;
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
                        return line;
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

        #region Version
        private void HandleWithVersion(ref string line, int Version)
        {
            if (Version <= 121)
            {
                if (line.StartsWith("E0IE") || line.StartsWith("E0OE"))
                    line = line.Substring(0, "E0IE".Length) + ",0," + line.Substring("E0IE,".Length);
                else if (line.StartsWith("H09G"))
                {
                    string[] blocks = line.Split(',');
                    for (int idx = 1; idx < blocks.Length; )
                    {
                        int lugsz = int.Parse(blocks[idx + 14]);
                        int nextIdx = idx + 15;
                        nextIdx += lugsz;
                        blocks[nextIdx] = "0,0," + blocks[nextIdx];
                        nextIdx += 5;
                        int excdsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        List<ushort> excards = Util.TakeRange(blocks, nextIdx,
                            nextIdx + excdsz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += excdsz;
                        int fakeqsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        List<string> fakeqpairs = Util.TakeRange(blocks, nextIdx,
                            nextIdx + fakeqsz * 2).ToList();
                        nextIdx += fakeqsz * 2;
                        int token = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        int peoplesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        List<string> peoples = Util.TakeRange(blocks, nextIdx,
                            nextIdx + peoplesz).ToList();
                        nextIdx += peoplesz;
                        int tarsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        List<ushort> tars = Util.TakeRange(blocks, nextIdx,
                            nextIdx + tarsz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += tarsz;
                        bool awake = blocks[nextIdx] == "1";
                        nextIdx += 1;
                        int foldsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        int escuesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        List<ushort> escues = Util.TakeRange(blocks, nextIdx,
                            nextIdx + escuesz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += escuesz;

                        idx = nextIdx;
                    }
                    line = string.Join(",", blocks);
                }
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
                    if (ch!= '\0')
                    {
                        line = "E0ON,10," + ch + "," + (args.Length - 2) + ","
                            + string.Join(",", Util.TakeRange(args, 2, args.Length));
                    }
                }
                else if (line.StartsWith("E0CC"))
                {
                    string[] args = line.Split(',');
                    if (args[2] == "0")
                        args[2] = args[1];
                    args[2] = ",0," + args[2];
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0CD"))
                {
                    string[] args = line.Split(',');
                    args[1] = args[1] + ",0,";
                    line = string.Join(",", args);
                }
                else if (line.StartsWith("E0CE"))
                {
                    string[] args = line.Split(',');
                    args[1] = args[1] + ",0,";
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
                else if (line.StartsWith("H09G"))
                {
                    string[] blocks = line.Split(',');
                    for (int idx = 1; idx < blocks.Length; )
                    {
                        blocks[idx + 12] = "0," + blocks[idx + 12] + ",0"; // insert trove and lug into exq;

                        int nextIdx = idx + 18;
                        int excdsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        nextIdx += excdsz;
                        int fakeqsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        for (int i = nextIdx; i < nextIdx + fakeqsz; ++i)
                            blocks[i] = blocks[i] + ",0";
                        nextIdx += fakeqsz;
                        int token = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        int peoplesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        nextIdx += peoplesz;
                        int tarsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        nextIdx += tarsz;
                        blocks[nextIdx] = "0,0," + blocks[nextIdx]; // insert awake and folder into escuesz
                        int escuesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        nextIdx += escuesz;
                        idx = nextIdx;
                    }
                    line = string.Join(",", blocks);
                }
                else if (line.StartsWith("H09F"))
                    line += ",0";
            }
            if (Version <= 112)
            {
                if (line.StartsWith("E0QC,"))
                    line = "E0QC,1," + line.Substring("E0QC,".Length);
            }
            if (Version <= 110)
            {
                if (line.StartsWith("H0SM"))
                    line += ",5";
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
            if (Version <= 101)
            {
                if (line.StartsWith("H09G"))
                {
                    string[] blocks = line.Split(',');
                    for (int idx = 1; idx < blocks.Length; )
                    {
                        int nextIdx = idx + 18;
                        int excdsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<ushort> excards = Util.TakeRange(blocks, nextIdx,
                        //    nextIdx + excdsz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += excdsz;
                        int fakeqsz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<ushort> fakeqs = Util.TakeRange(blocks, nextIdx,
                        //    nextIdx + fakeqsz).Select(p => ushort.Parse(p)).ToList();
                        nextIdx += fakeqsz;
                        int token = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        int peoplesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<string> peoples = Util.TakeRange(blocks, nextIdx,
                        //    nextIdx + peoplesz).ToList();
                        nextIdx += peoplesz;

                        int @int = int.Parse(blocks[nextIdx]); // target
                        blocks[nextIdx] = "1," + @int;

                        int escuesz = int.Parse(blocks[nextIdx]);
                        nextIdx += 1;
                        //List<ushort> escues = Util.TakeRange(blocks, nextIdx,
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
        }
        #endregion Version
    }
}
