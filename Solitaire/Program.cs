using Solitaire.Scorpion;
using System;
using System.Collections.Generic;

namespace Solitaire
{
    class Program
    {
        static void Main(string[] args)
        {
            Shuffler shuffler = new Shuffler();
            int successCount = 0;
            int failCount = 0;
            for (int i = 0; i < 20000; ++i)
            {
                IList<Card> deck = shuffler.Deal(i);
                Board board = new Board(deck);
                Solver solver = new Solver(board);
                if (solver.Solve())
                {
                    Console.WriteLine(i);
                    ++successCount;
                }
                else
                {
                    ++failCount;
                }
            }
            Console.WriteLine(100 * successCount / (successCount + failCount) + "%");
        }
    }
}
