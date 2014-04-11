using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.VW
{
    public interface IVI
    {
        // Initial listening receive
        void Init();
        // Set whether in game mode, whether operations can be accepted
        void SetInGame(bool value);
        // display $msg on $me screen
        void Cout(ushort me, string msgFormat, params object[] args);
        // require input from $me, with hint as $hint
        string Cin(ushort me, string hintFormat, params object[] args);
        //// TODO:DEBUG: only display hidden message
        //void Cout0(ushort me, string msg);

        // Open Cin Tunnel
        void OpenCinTunnel(ushort me);
        // Close Cin Tunnel
        void CloseCinTunnel(ushort me);
        // Terminate Cin Tunnel, give pending Cin CinSentinel as result
        void TerminCinTunnel(ushort me);
        // Reset Cin Tunnel, clean all pending input request
        void ResetCinTunnel(ushort me);
        // sentinel value that pending close Cin would return
        string CinSentinel { get; }

        // Request in Client
        string Request(ushort me);
        // Talk in Client
        string RequestTalk(ushort me);
        //// Display the chat message send from the others after room
        //void Chat(Msgs msg);
        // Display the chat message send from the others before room
        void Chat(string msg, string nick);
    }
}
