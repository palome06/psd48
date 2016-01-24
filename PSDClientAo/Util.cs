using System;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.ClientAo
{
    public static class Util
    {
        public delegate bool InputMessageHandler(string input);

        public delegate void RubanMoveHandler(int oI, int oJ, int idx, int jdx);
    }

    public class UCounter
    {
        public UCounter(int value) { Value = value; }
        public int Value { set; get; }
        public void Incr() { ++Value; }
    }
}
