using Solitaire.Canfield;
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
            for (int i = 1; i < 200; ++i)
            {
                IList<Card> deck = shuffler.Deal(i);
                Board board = new Board(deck);
                board.Dump();
                Solver solver = new Solver(board);
                if (solver.Solve())
                {
                    Console.WriteLine(i);
                    solver.DumpSolution();
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
