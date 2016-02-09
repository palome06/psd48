using PSD.Base;
using System.Collections.Generic;

namespace PSD.PSDGamepkg.Artiad
{
    public class InnateChange
    {
        public enum Prop { STR, DEX, HP };
        public ushort Who { set; get; }
        public Prop Item { set; get; }
        public int NewValue { set; get; }
        public string ToMessage()
        {
            string propValue = Item == Prop.STR ? "S" : (Item == Prop.DEX ? "D" : "H");
            return "G0LA," + propValue + "," + Who + "," + NewValue;
        }
        public static InnateChange Parse(string line)
        {
            string[] g0la = line.Split(',');
            return new InnateChange()
            {
                Item = g0la[1] == "S" ? Prop.STR : (g0la[1] == "D" ? Prop.DEX : Prop.HP),
                Who = ushort.Parse(g0la[2]),
                NewValue = int.Parse(g0la[3])
            };
        }
        public void Handle(XI XI)
        {
            if (NewValue <= 0)
                NewValue = 0;
            Player py = XI.Board.Garden[Who];
            if (Item == Prop.STR)
            {
                int oldValue = py.STRh;
                py.STRh = NewValue;
                if (oldValue > NewValue)
                    XI.RaiseGMessage("G0OA," + Who + ",0," + (oldValue - NewValue));
                else if (oldValue < NewValue)
                    XI.RaiseGMessage("G0IA," + Who + ",0," + (NewValue - oldValue));
            }
            else if (Item == Prop.DEX)
            {
                int oldValue = py.DEXh;
                py.DEXh = NewValue;
                if (oldValue > NewValue)
                    XI.RaiseGMessage("G0OX," + Who + ",0," + (oldValue - NewValue));
                else if (oldValue < NewValue)
                    XI.RaiseGMessage("G0IX," + Who + ",0," + (NewValue - oldValue));
            }
            else if (Item == Prop.HP)
            {
                int oldValue = py.HPb;
                if (oldValue != NewValue)
                {
                    py.HPb = NewValue;
                    if (py.HP > py.HPb)
                        py.HP = py.HPb;
                    new Artiad.InnateSemaphore()
                    {
                        Item = Prop.HP,
                        Who = Who,
                        OldValue = oldValue,
                        Newvalue = NewValue
                    }.Telegraph(XI.WI.BCast);
                    if (py.HPb == 0)
                        XI.WI.BCast("E0ZW," + Who);
                }
            }
        }
    }

    public class ResetAX
    {
        public ushort Who { set; get; }
        public string ToMessage() { return "G0AX," + Who; }
        public static ResetAX Parse(string line)
        {
            ushort who = ushort.Parse(line.Substring("G0AX,".Length));
            return new ResetAX() { Who = who };
        }
        public void Handle(XI XI)
        {
            Player player = XI.Board.Garden[Who];
            if (player.IsAlive)
            {
                if (player.DEXc != player.DEXb || player.DEXa != player.DEXb
                    || player.STRc != player.STRb || player.STRa != player.STRb)
                {
                    new Artiad.ResetAXSemaphore()
                    {
                        Who = Who,
                        STR = player.STRb,
                        DEX = player.DEXb
                    }.Telegraph(XI.WI.BCast);
                }
                player.SDaSet = player.SDcSet = false;
                player.DEXi = 0; player.STRi = 0;
                player.RestZP = 1;
            } // JN50302 to override the G0AX events
        }
    }
    
    public class InnateSemaphore
    {
        public InnateChange.Prop Item { set; get; }
        public ushort Who { set; get; }
        public int OldValue { set; get; }
        public int Newvalue { set; get; }

        public void Telegraph(System.Action<string> send)
        {
            char ch = Item == InnateChange.Prop.STR ? 'S' :
                (Item == InnateChange.Prop.DEX ? 'D' : 'H');
            send("E0LA," + ch + "," + Who + "," + OldValue + "," + Newvalue);
        }
    }
    public class ResetAXSemaphore
    {
        public ushort Who { set; get; }
        public int STR { set; get; }
        public int DEX { set; get; }
        public void Telegraph(System.Action<string> send)
        {
            send("E0AX," + Who + "," + STR + "," + DEX);
        }
    }
}
