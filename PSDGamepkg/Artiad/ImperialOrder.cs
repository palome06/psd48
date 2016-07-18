using PSD.Base;
using PSD.Base.Card;
using System.Collections.Generic;
using System.Linq;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public class ImperialLeft // YM, load into field
    {
        public enum ZoneType { NIL, M1, M2, E, W };
        public ZoneType Zone { set; get; }
        public bool IsReset { set; get; }
        private ushort mCard;
        public ushort Card
        {
            set { mCard = value; if (value == 0) { IsReset = true; } }
            get { return IsReset ? (ushort)0 : mCard; } 
        }

        public string ToMessage()
        {
            return "G0YM," + Zone + "," + (IsReset ? 0 : Card);
        }
        public static ImperialLeft Parse(string line)
        {
            string[] g0ym = line.Split(',');
            ushort card = ushort.Parse(g0ym[2]);
            ZoneType zone;
            switch (g0ym[1])
            {
                case "M1": zone = ZoneType.M1; break;
                case "M2": zone = ZoneType.M2; break;
                case "E": zone = ZoneType.E; break;
                case "W": zone = ZoneType.W; break;
                default: zone = ZoneType.NIL; break;
            }
            return new ImperialLeft()
            {
                Zone = zone,
                IsReset = card == 0,
                Card = card
            };
        }
        public bool Legal()
        {
            return IsReset || Card != 0;
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            new Artiad.ImperialLeftSemaphore()
            {
                Zone = Zone,
                Card = Card
            }.Telegraph(WI.BCast);
            if (Zone == ZoneType.W && !IsReset)
            {
                XI.RaiseGMessage(new Artiad.ImperialCentre()
                {
                    Genre = Base.Card.Card.Genre.NMB,
                    Cards = new ushort[] { Card }
                }.ToMessage());
            }
            switch (Zone)
            {
                case ZoneType.M1: XI.Board.Monster1 = Card; break;
                case ZoneType.M2: XI.Board.Monster2 = Card; break;
                case ZoneType.E: XI.Board.Eve = Card; break;
                case ZoneType.W:
                {
                    if (IsReset && XI.Board.Wang.Count > 0)
                        XI.Board.Wang.Pop();
                    else
                        XI.Board.Wang.Push(Card);
                    break;
                }
            }
        }
    }

    public class ImperialCentre // YZ, Show only
    {
        public Card.Genre Genre { set; get; } // Genre Actually
        public bool Encrypted { set; get; }
        public ushort[] Cards { set; get; } // valid when Encrypted = false
        public int CardCount { set; get; } // valid when Encrypted = true

        public string ToMessage()
        {
            return "G2YZ," + Card.Genre2Char(Genre) + "," +
                (!Encrypted ? ("0," + string.Join(",", Cards)) : ("1," + CardCount));
        }
        public static ImperialCentre Parse(string line)
        {
            string[] g0yz = line.Split(',');
            Card.Genre genre = Card.Char2Genre(g0yz[1][0]);
            bool encrypted = g0yz[2] == "1";
            if (!encrypted)
            {
                return new ImperialCentre()
                {
                    Genre = genre,
                    Encrypted = false,
                    Cards = Algo.TakeRange(g0yz, 3, g0yz.Length).Select(p => ushort.Parse(p)).ToArray()
                };
            }
            else
                return new ImperialCentre() { Genre = genre, Encrypted = false, CardCount = int.Parse(g0yz[3]) };
        }
    }

    public class ImperialRight // YB, Push back
    {
        public bool Encrypted { set; get; }
        public Card.Genre Genre { set; get; }
        public List<ImperialRightUnit> Items { set; get; }
        public string ToMessage()
        {
            return "G0YB," + (Encrypted ? 1 : 0) + "," + Card.Genre2Char(Genre) + "," +
                string.Join(",", Items.Select(p => p.ToRawMessage()));
        }
        public static ImperialRight Parse(string line)
        {
            string[] piru = line.Split(',');
            return new ImperialRight()
            {
                Encrypted = piru[1] == "1",
                Genre = Card.Char2Genre(piru[2][0]),
                Items = ImperialRightUnit.ParseFromLine(line)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            foreach (ImperialRightUnit iri in Items)
            {
                if (Genre == Card.Genre.Tux)
                    XI.Board.TuxPiles.PushBack(iri.Cards);
                else if (Genre == Card.Genre.NMB || Genre == Card.Genre.NPC)
                    XI.Board.MonPiles.PushBack(iri.Cards);
                else if (Genre == Card.Genre.Eve)
                    XI.Board.EvePiles.PushBack(iri.Cards);
            }
            if (!Encrypted)
            {
                new ImperialRightSemaphore()
                {
                    Genre = Genre,
                    Items = Items.Select(p => new ImperialRightSemaphoreUnit()
                    {
                        Encrypted = false,
                        Source = p.Source,
                        Cards = p.Cards
                    }).ToList()
                }.Telegraph(WI.BCast);
                XI.RaiseGMessage(new Artiad.ImperialCentre()
                {
                    Genre = Genre,
                    Encrypted = false,
                    Cards = Items.SelectMany(p => p.Cards).ToArray()
                }.ToMessage());
            }
            else
            {
                ImperialRightSemaphore.Send(WI, XI.Board.Garden.Keys.ToDictionary(p => p,
                    p => new ImperialRightSemaphore()
                    {
                        Genre = Genre,
                        Items = Items.Select(q => new ImperialRightSemaphoreUnit()
                        {
                            Encrypted = q.Source != p,
                            Source = q.Source,
                            Cards = q.Cards,
                            Count = q.Cards.Length
                        }).ToList()
                    }), new ImperialRightSemaphore()
                    {
                        Genre = Genre,
                        Items = Items.Select(p => new ImperialRightSemaphoreUnit()
                        {
                            Encrypted = true,
                            Source = p.Source,
                            Count = p.Cards.Length
                        }).ToList()
                    }
                );
                XI.RaiseGMessage(new Artiad.ImperialCentre()
                {
                    Genre = Genre,
                    Encrypted = true,
                    CardCount = Items.Sum(p => p.Cards.Length)
                }.ToMessage());
            }
        }
    }

    public class ImperialRightUnit
    {
        public ushort Source { set; get; }
        public ushort[] Cards { set; get; }
        public string ToRawMessage()
        {
            return Source + "," + Algo.ListToString(Cards);
        }
        public static List<ImperialRightUnit> ParseFromLine(string line)
        {
            List<ImperialRightUnit> irus = new List<ImperialRightUnit>();
            string[] piru = line.Split(',');
            for (int idx = 3; idx < piru.Length;)
            {
                ushort source = ushort.Parse(piru[idx++]);
                ushort[] cards = Algo.TakeArrayWithSize(piru, idx, out idx);
                irus.Add(new ImperialRightUnit()
                {
                    Source = source,
                    Cards = cards
                });
            }
            return irus;
        }
    }

    public class ImperialLeftSemaphore
    {
        public ImperialLeft.ZoneType Zone { set; get; }
        public ushort Card { set; get; }
        public bool ShowText { set; get; }
        public void Telegraph(System.Action<string> send)
        {
            send("E0YM," + (ShowText ? 0 : 1) + "," + Zone + "," + Card);
        }
    }
    public class ImperialRightSemaphore
    {
        public Card.Genre Genre { set; get; }
        public List<ImperialRightSemaphoreUnit> Items { set; get; }
        public static ImperialRightSemaphore Merge(IEnumerable<ImperialRightSemaphore> sources)
        {
            if (!sources.Any()) return null;
            Card.Genre genre = sources.First().Genre;
            List<ImperialRightSemaphoreUnit> items = sources.SelectMany(p => p.Items).ToLookup(
                p => p.Source).Select(p => new ImperialRightSemaphoreUnit()
                {
                    Source = p.Key,
                    Encrypted = !p.Any(q => !q.Encrypted),
                    Cards = p.Any(q => !q.Encrypted) ? p.Where(q => !q.Encrypted).First().Cards : null,
                    Count = !p.Any(q => !q.Encrypted) ? p.Where(q => q.Encrypted).First().Count : 0
                }).ToList();
            return new ImperialRightSemaphore() { Genre = genre, Items = items };
        }
        public void Telegraph(System.Action<string> send)
        {
            send("E0YB," + Card.Genre2Char(Genre) + "," +
                string.Join(",", Items.Select(p => p.ToRawMessage())));
        }
        public static void Send(Base.VW.IWISV WI,
            IDictionary<ushort, ImperialRightSemaphore> lookup, ImperialRightSemaphore live)
        {
            lookup.ToList().ForEach(p => p.Value.Telegraph(q => WI.Send(q, p.Key, 0)));
            live.Telegraph(WI.Live);
        }
    }

    public class ImperialRightSemaphoreUnit
    {
        public ushort Source { set; get; }
        public bool Encrypted { set; get; }
        public ushort[] Cards { set; get; }
        public int Count { set; get; }
        public string ToRawMessage()
        {
            return Source + "," + (Encrypted ? ("1," + Count) : ("0," + Algo.ListToString(Cards))); 
        }
    }
}
