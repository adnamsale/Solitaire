namespace Solitaire
{
    class Suit
    {
        public static readonly Suit HEART = new Suit("H");
        public static readonly Suit SPADE = new Suit("S");
        public static readonly Suit DIAMOND = new Suit("D");
        public static readonly Suit CLUB = new Suit("C");

        private string abbr;

        private Suit(string abbr)
        {
            this.abbr = abbr;
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
