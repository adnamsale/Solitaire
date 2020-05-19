using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Solitaire
{
    enum Color
    {
        RED,
        BLACK
    }

    class Suit
    {
        public static readonly Suit HEART = new Suit("H", 0, Color.RED);
        public static readonly Suit SPADE = new Suit("S", 1, Color.BLACK);
        public static readonly Suit DIAMOND = new Suit("D", 2, Color.RED);
        public static readonly Suit CLUB = new Suit("C", 3, Color.BLACK);

        public static readonly IEnumerable<Suit> AllValues =
            new ReadOnlyCollection<Suit>(new [] { HEART, SPADE, DIAMOND, CLUB });

        private string abbr;

        private Suit(string abbr, int ordinal, Color color)
        {
            this.abbr = abbr;
            Ordinal = ordinal;
            Color = color;
        }

        public int Ordinal
        {
            get;
            private set;
        }

        public Color Color
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return abbr;
        }
    }

    class Card
    {
        public Card(Suit suit, int rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public Suit Suit
        {
            get;
            private set;
        }

        public int Rank
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Suit + Rank.ToString("D2");
        }
    }
}
