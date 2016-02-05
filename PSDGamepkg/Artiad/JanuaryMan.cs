using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSD.Base.Card;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public class AnnouceCard
    {
        // Show: short term; Declare: long term; Discovery: only texture report
        public enum Type { NL = 0, SHOW = 1, DECLARE = 2, DISCOVERY = 3, FLASH = 4 };

        public Type Action { set; get; }

        public ushort Officer { set; get; }

        public Card.Genre Genre { set; get; }

        public ushort[] Cards { set; get; }
        public ushort SingleCard { set { Cards = new ushort[] { value }; } }

        public string ToMessage()
        {
            object[] values = { (int)Action, Officer, Genre.Genre2Char(), string.Join(",", Cards) };
            return "G2SW," + string.Join(",", values);
        }

        public AnnouceCard Parse(string line)
        {
            string[] g2fu = line.Split(',');
            Type action;
            switch (g2fu[1])
            {
                case "1": action = Type.SHOW; break;
                case "2": action = Type.DECLARE; break;
                case "3": action = Type.DISCOVERY; break;
                case "4": action = Type.FLASH; break;
                default: action = Type.NL; break;
            }
            return new AnnouceCard()
            {
                Action = action,
                Officer = ushort.Parse(g2fu[2]),
                Genre = g2fu[3][0].Char2Genre(),
                Cards = Algo.TakeRange(g2fu, 4, g2fu.Length).Select(p => ushort.Parse(p)).ToArray()
            };
        }
    }
}