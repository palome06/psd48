using PSD.Base.Card;
using System.Linq;
using System.Collections.Generic;
using PSD.Base.Utils;

namespace PSD.PSDGamepkg.Artiad
{
    public class Abandon
    {
        // card genre, asume only one type of genre exists in the same abadon object
        public Card.Genre Genre { set; get; }
        // zone
        public CustomsHelper.ZoneType Zone { set; get; }
        // unit list
        public List<CustomsUnit> List { set; get; }
        public CustomsUnit SingleUnit
        {
            set { List = new List<CustomsUnit>() { value }; }
        }
        public string ToMessage()
        {
            return "G0ON," + Card.Genre2Char(Genre) + "," + CustomsHelper.Zone2Char(Zone) + "," +
                string.Join(",", List.Select(p => p.ToRawMessage()));
        }
        public static Abandon Parse(string line)
        {
            char chg = line["G0ON,".Length];
            char chz = line["G0ON,X,".Length];
            return new Abandon()
            {
                Genre = Card.Char2Genre(chg),
                Zone = CustomsHelper.Char2Zone(chz),
                List = CustomsUnit.ParseFromLine(line)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            foreach (CustomsUnit cunit in List)
            {
                if (Genre == Card.Genre.Tux)
                    XI.Board.TuxDises.AddRange(cunit.Cards);
                else if (Genre == Card.Genre.NMB)
                    XI.Board.MonDises.AddRange(cunit.Cards);
                else if (Genre == Card.Genre.Eve)
                    XI.Board.EveDises.AddRange(cunit.Cards);
            }
            new AbadonSemaphore()
            {
                Genre = Genre,
                Zone = Zone,
                Cards = List.SelectMany(p => p.Cards).ToArray()
            }.Telegraph(WI.BCast);
        }
    }

    public class CustomsUnit
    { 
        // source of player, only matters if Zone == PLAYER
        public ushort Source { set; get; }
        // cards
        public ushort[] Cards { set; get; }
        // single entry
        public ushort SingleCard
        {
            set { Cards = new ushort[] { value }; }
            get { return (Cards != null && Cards.Length == 1) ? Cards[0] : (ushort)0; }
        }

        internal string ToRawMessage() { return Source + "," + Cards.ListToString(); }
        internal static List<CustomsUnit> ParseFromLine(string line)
        {
            string[] g0on = line.Split(',');
            List<CustomsUnit> cus = new List<CustomsUnit>();
            int idx = 3;
            while (idx < g0on.Length)
                cus.Add(new CustomsUnit()
                {
                    Source = ushort.Parse(g0on[idx]),
                    Cards = Algo.TakeArrayWithSize(g0on, idx + 1, out idx)
                });
            return cus;
        }
    }

    public class AbadonSemaphore
    {
        // card genre, asume only one type of genre exists in the same abadon object
        public Card.Genre Genre { set; get; }
        // zone
        public CustomsHelper.ZoneType Zone { set; get; }
        // cards
        public ushort[] Cards { set; get; }

        public void Telegraph(System.Action<string> send)
        {
            send("E0ON," + Card.Genre2Char(Genre) + "," +
                CustomsHelper.Zone2Char(Zone) + "," + string.Join(",", Cards));
        }
    }

    public static class CustomsHelper
    {
        // showboard -> the card is on display now
        // explicit -> from other places and need full notification
        // implicit -> from other places and need no notification
        public enum ZoneType { NIL, PLAYER, SHOWBOARD, EXPLICIT, IMPLICIT };
        private static readonly char[] ZoneChar = { '/', 'P', 'S', 'E', 'I' };

        internal static char Zone2Char(ZoneType zone)
        {
            return ZoneChar[(int)zone];
        }
        internal static ZoneType Char2Zone(char ch)
        {
            switch (ch)
            {
                case 'P': return ZoneType.PLAYER;
                case 'S': return ZoneType.SHOWBOARD;
                case 'E': return ZoneType.EXPLICIT;
                case 'I': return ZoneType.IMPLICIT;
                default: return ZoneType.NIL;
            }
        }
        internal static bool RemoveCards(Abandon abandon, params ushort[] cards)
        {
            abandon.List.ForEach(p =>
            {
                p.Cards = p.Cards.ToList().Where(q => !cards.Contains(q)).ToArray();
            });
            abandon.List.RemoveAll(p => p.Cards.Length == 0);
            return abandon.List.Count > 0;
        }
    }
}
