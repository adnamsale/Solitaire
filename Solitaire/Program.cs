using Solitaire.Addiction;
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
            for (int i = 21072; i < 21073; ++i)
            {
                IList<Card> deck = shuffler.Deal(i);
                Board board = new Board(deck, shuffler);
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
