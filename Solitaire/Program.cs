using Solitaire.TriPeaks;
using System;
using System.Collections.Generic;

namespace Solitaire
{
    class Program
    {
        static void Main(string[] args)
        {
            Shuffler shuffler = new Shuffler();
            int lo = 1;
            int hi = 200;
            int good = 0;
            for (int i = lo; i <= hi; ++i)
            {
                IList<Card> deck = shuffler.Deal(i);
                Board board = new Board(deck);
                Solver solver = new Solver(board);
                if (solver.Solve())
                {
                    ++good;
                }
            }
            Console.WriteLine($"Success rate: {(double)good / (hi - lo + 1)}");
        }
    }
}
