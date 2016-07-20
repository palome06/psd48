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
        // where the card comes from
        public ushort Source { set; get; }
        // trigger
        public ushort Trigger { set; get; }
        private ushort mCard;
        public ushort Card
        {
            set { mCard = value; if (value == 0) { IsReset = true; } }
            get { return IsReset ? (ushort)0 : mCard; } 
        }

        public ImperialLeft() { IsReset = false; Source = 0; Trigger = 0; }
        public string ToMessage()
        {
            return "G0YM," + Zone + "," + Trigger + "," +
                Source + "," + (IsReset ? 0 : Card);
        }
        public static ImperialLeft Parse(string line)
        {
            string[] g0ym = line.Split(',');
            ushort trigger = ushort.Parse(g0ym[2]);
            ushort source = ushort.Parse(g0ym[3]);
            ushort card = ushort.Parse(g0ym[4]);
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
                Trigger = trigger,
                Source = source,
                Card = card
            };
        }
        public bool Legal() { return IsReset || Card != 0; }
        public void Handle(XI XI, Base.VW.IWISV WI, int priority)
        {
            if (priority == 100)
                Handle100(XI, WI);
            else if (priority == 200)
                Handle200(XI);
        }
        private void Handle100(XI XI, Base.VW.IWISV WI)
        {
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
                case ZoneType.M1:
                    XI.Board.Monster1 = Card;
                    if (IsReset)
                        XI.Board.Mon1From = 0;
                    else
                        XI.Board.Mon1From = Source;
                    break;
                case ZoneType.M2: XI.Board.Monster2 = Card; break;
                case ZoneType.E: XI.Board.Eve = Card; break;
                case ZoneType.W:
                    if (IsReset && XI.Board.Wang.Count > 0)
                        XI.Board.Wang.Pop();
                    else
                        XI.Board.Wang.Push(Card);
                    break;
            }
            new Artiad.ImperialLeftSemaphore()
            {
                Zone = Zone,
                ShowText = !IsReset,
                Source = Source,
                Card = (Zone == ZoneType.W && IsReset &&
                    XI.Board.Wang.Count > 0) ? XI.Board.Wang.Peek() : Card
            }.Telegraph(WI.BCast);
        }
        private void Handle200(XI XI)
        {
            if (!IsReset && (Zone == ZoneType.M1 || Zone == ZoneType.M2 || Zone == ZoneType.W))
            {
                if (Base.Card.NMBLib.IsNPC(Card) && Trigger != 0)
                {
                    NPC npc = XI.LibTuple.NL.Decode(Base.Card.NMBLib.OriginalNPC(Card));
                    npc.Debut(XI.Board.Garden[Trigger]);
                }
            }
        }
    }

    public class ImperialCentre // YZ, Show only
    {
        public Card.Genre Genre { set; get; } // Genre Actually
        public bool Encrypted { set; get; }
        public ushort[] Cards { set; get; } // valid when Encrypted = false
        public ushort SingleCard { set { Cards = new ushort[] { value }; } }
        public int CardCount { set; get; } // valid when Encrypted = true

        public string ToMessage()
        {
            return "G2YZ," + Card.Genre2Char(Genre) + "," +
                (!Encrypted ? ("0," + string.Join(",", Cards)) : ("1," + CardCount));
        }
        public bool Legal() { return Cards != null && Cards.Length > 0; }

        public ImperialCentre() { Encrypted = false; }
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
    /// <summary>
    /// YB, put card back to piles, DO NOT consider the source opeartion
    /// </summary>
    public class ImperialRight
    {
        public bool Encrypted { set; get; }
        public Card.Genre Genre { set; get; }
        public int Offset { set; get; } // where to put the cards, 0 = peek, 1 = next
        public List<ImperialRightUnit> Items { set; get; }
        public ImperialRightUnit SingleItem
        {
            set { Items = new ImperialRightUnit[] { value }.ToList(); }
        }
        public string ToMessage()
        {
            return "G0YB," + Card.Genre2Char(Genre) + "," + Offset + "," + (Encrypted ? 1 : 0) +
                "," + string.Join(",", Items.Select(p => p.ToRawMessage()));
        }
        public bool Legal() { return Items != null && Items.Count > 0; }
        public static ImperialRight Parse(string line)
        {
            string[] piru = line.Split(',');
            return new ImperialRight()
            {
                Encrypted = piru[3] == "1",
                Offset = int.Parse(piru[2]),
                Genre = Card.Char2Genre(piru[1][0]),
                Items = ImperialRightUnit.ParseFromLine(line, 4)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            System.Action<Base.Utils.Rueue<ushort>, ushort[]> action = (piles, cards) =>
            {
                List<ushort> list = new List<ushort>();
                list.AddRange(piles.Dequeue(Offset));
                list.AddRange(cards);
                piles.PushBack(list);
            };

            foreach (ImperialRightUnit iri in Items)
            {
                if (Genre == Card.Genre.Tux)
                    action(XI.Board.TuxPiles, iri.Cards);
                else if (Genre == Card.Genre.NMB || Genre == Card.Genre.NPC)
                    action(XI.Board.MonPiles, iri.Cards);
                else if (Genre == Card.Genre.Eve)
                    action(XI.Board.EvePiles, iri.Cards);
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
                        Offset = Offset,
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
                        Offset = Offset,
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
        public ushort SingleCard { set { Cards = new ushort[] { value }; } }
        public string ToRawMessage()
        {
            return Source + "," + Algo.ListToString(Cards);
        }
        public static List<ImperialRightUnit> ParseFromLine(string line, int startIdx)
        {
            List<ImperialRightUnit> irus = new List<ImperialRightUnit>();
            string[] piru = line.Split(',');
            for (int idx = startIdx; idx < piru.Length;)
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
        public ushort Source { set; get; }
        public ushort Card { set; get; }
        public bool ShowText { set; get; }
        public void Telegraph(System.Action<string> send)
        {
            send("E0YM," + (ShowText ? 0 : 1) + "," + Zone + "," + Source + "," + Card);
        }
    }
    public class ImperialRightSemaphore
    {
        public Card.Genre Genre { set; get; }
        public int Offset { set; get; }
        public List<ImperialRightSemaphoreUnit> Items { set; get; }
        public static ImperialRightSemaphore Annex(IEnumerable<ImperialRightSemaphore> sources)
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
            send("E0YB," + Card.Genre2Char(Genre) + "," + Offset + "," +
                string.Join(",", Items.Select(p => p.ToRawMessage())));
        }
        public static void Send(Base.VW.IWISV WI,
            IDictionary<ushort, ImperialRightSemaphore> lookup, ImperialRightSemaphore live)
        {
            lookup.ToList().ForEach(p => p.Value.Telegraph(q => WI.Send(q, 0, p.Key)));
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
