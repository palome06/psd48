using System.Collections.Generic;

namespace PSD.Base.VW
{
    //public interface IWI
    //{
    //    // Standard receive from $from to $me
    //    string Recv(ushort me, ushort from);
    //    //// Receive multiple raw message from multiple $from, need gather
    //    //string[] RecvAll(ushort[] from);
    //    // infinite process starts
    //    void RecvInfStart();
    //    // receive each message during the process
    //    Msgs RecvInfRecv();
    //    // infinite process ends
    //    void RecvInfEnd();
    //    // reset the terminate flag to 0, start new stage
    //    void RecvInfTermin();
    //    // Send raw message from $me to $to
    //    void Send(string msg, ushort me, ushort to);
    //    // Send raw message to multiple $to
    //    void Send(string msg, ushort[] tos);
    //    // Send raw message to the whole
    //    void BCast(string msg);
    //    // Send direct message that won't be caught by RecvInfRecv from $me to 0
    //    void SendDirect(string msg, ushort me);

    //    // Ask for Input with tag/param from $to, return only data
    //    //string AsyncInput(string tag, string param, ushort to);
    //}
    public interface IWISV : System.IDisposable
    {
        // Standard receive from $from to $me
        string Recv(ushort me, ushort from);
        // receive each message during the process
        Msgs RecvInfRecv();
        // Send raw message from $me to $to
        void Send(string msg, ushort me, ushort to);
        // Send raw message to multiple $to
        void Send(string msg, IEnumerable<ushort> tos);
        // live message to all watchers
        void Live(string msg);
        // Send raw message to the whole
        void BCast(string msg);
        // send in general, might get combined results
        void Send(IDictionary<ushort, string> table, string live);
        // send $msg to who and nofify the others with live
        void Focus(ushort who, string msg, string live);
    }

    public interface IWICL : System.IDisposable
    {
        // Standard receive from $from to $me
        string Recv(ushort me, ushort from);
        // Send raw message from $me to $to
        void Send(string msg, ushort me, ushort to);
        // Close the socket for recycling
        void Shutdown();
        // Hear any text message from others
        string Hear();
    }

    public class Msgs
    {
        public string Msg { private set; get; }
        public ushort From { private set; get; }
        public ushort To { private set; get; }
        public bool Direct { private set; get; } // TODO: maybe remove the property
        // public int Serial { private set; get; }
        public Msgs(string msg, ushort from, ushort to)
        {
            Msg = msg; From = from; To = to; Direct = false;
        }
        public Msgs(string msg, ushort from, ushort to, bool direct)
        {
            Msg = msg; From = from; To = to; Direct = direct;
        }
        public override string ToString()
        {
            return "<" + From + "-" + To + (Direct ? ">>" : ">") + Msg;
        }
        //public static Msgs Parse() { }
    }
}
