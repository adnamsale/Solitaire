using System;
using System.Collections.Generic;

namespace Solitaire
{
    // Shuffling algorithm from cardgames.io so that we can compare games
    class Shuffler
    {
        private long e;

        public IList<Card> Deal(int seed)
        {
            e = seed;
            List<Card> deck = new List<Card>();
            for (int i = 1; i <= 13; ++i)
            {
                // HSDC
                deck.Add(new Card(Suit.HEART, i));
                deck.Add(new Card(Suit.SPADE, i));
                deck.Add(new Card(Suit.DIAMOND, i));
                deck.Add(new Card(Suit.CLUB, i));
            }

            for (int t = 52; t != 1; --t)
            {
                int n = (int)Math.Floor(nextRandom() * t);
                Card a = deck[t - 1];
                Card i = deck[n];
                deck[t - 1] = i;
                deck[n] = a;
            }
            return deck;
        }

        double nextRandom()
        {
            e = (9301 * e + 49297) % 233280;
            return (double)e / 233280;
        }
    }
}
