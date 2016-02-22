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
        public void Handle(XI XI)
        {
            if (CrossStage)
            {
                foreach (Player player in XI.Board.Garden.Values)
                    player.ResetRAM();
            }
            XI.WI.RecvInfTermin();
            // Reset board information
            XI.Board.UseCardRound = 0;
            new GotoFire() { Terminal = Terminal }.Telegraph(XI.WI.BCast);
            // count how many players have received the F0JM message
            int count = XI.Board.Garden.Keys.Count;
            XI.WI.RecvInfStart();
            while (count > 0)
            {
                Base.VW.Msgs msg = XI.WI.RecvInfRecvPending();
                if (msg.Msg.StartsWith("F0JM"))
                    --count;
                else
                    new GotoFire() { Terminal = Terminal }.Telegraph(p => XI.WI.Send(p, 0, msg.From));
            }
            XI.WI.RecvInfEnd();
            new GotoFire() { Terminal = Terminal }.Telegraph(XI.WI.Live);
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
