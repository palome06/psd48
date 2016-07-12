using PSD.Base;
using System.Collections.Generic;

namespace PSD.PSDGamepkg.Artiad
{
    public class Goto
    {
        public bool CrossStage { set; get; }
        public string Terminal { set; get; }

        public string ToMessage()
        {
            return "G0JM," + (CrossStage ? 1 : 0) + "," + Terminal;
        }
        public Goto() { CrossStage = true; }
        public static Goto Parse(string line)
        {
            string[] g0jm = line.Split(',');
            return new Goto() { CrossStage = g0jm[1] == "1", Terminal = g0jm[2] };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            if (CrossStage)
                XI.ResetAllPlayerRAM();
            XI.ClearLeftPendingTux();
            // TODO: raise new ON event (e.g. Use WQ04 as ZP01 and then jump to R*Z2}), check the risk
            // WI.RecvInfTermin();
            // Reset board information
            XI.Board.UseCardRound = 0;
            GotoFire gotoFire = new GotoFire() { Terminal = Terminal };
            gotoFire.Telegraph((p) => XI.PushIntoLastUV(XI.Board.Garden.Keys, p));
            gotoFire.Telegraph(WI.BCast);
            // count how many players have received the F0JM message
            int count = XI.Board.Garden.Keys.Count;
            while (count > 0)
            {
                Base.VW.Msgs msg = WI.RecvInfRecv();
                if (msg.Msg.StartsWith("F0JM") && XI.MatchedPopFromLastUV(msg.From, "F0JM"))
                    --count;
                else
                    gotoFire.Telegraph(p => WI.Send(p, 0, msg.From));
            }
            XI.BlockSetJumpTable(Terminal, "H0TM");
            System.Threading.Thread.CurrentThread.Abort();
        }
    }
    
    public class GotoFire
    {
        public string Terminal { set; get; }
        public void Telegraph(System.Action<string> send)
        {
            send("F0JM," + Terminal);
        }
    }
}
