using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.KitC
{
    class Solver
    {
        private Board board;
        private IList<IMove> answer;

        public Solver(Board board)
        {
            this.board = board;
            answer = new List<IMove>();
            BestCount = 52;
        }

        public bool Solve()
        {
            return SolveRecurse();
        }

        public void DumpSolution()
        {
            foreach (IMove move in answer.Reverse())
            {
                Console.WriteLine(move);
            }
        }

        int depth = 0;
        public int BestCount
        {
            get;
            private set;
        }

        private bool SolveRecurse()
        {
            ++depth;
            if (board.IsSolved())
            {
                return true;
            }
            if (InSeenCache())
            {
                return false;
            }
            CacheSeen();
            if (board.Remaining < BestCount)
            {
                BestCount = board.Remaining;
            }
            IList<IMove> moves = board.GetMoves();
            //if (depth < 5)
            //{
            //    Console.WriteLine("Depth: " + depth + ", Moves: " + moves.Count);
            //    board.Dump();
            //}
            foreach (IMove move in moves)
            {
                move.Apply();
                //Console.WriteLine(move);
                //board.Dump();
                //Console.WriteLine();
                if (SolveRecurse())
                {
                    answer.Add(move);
                    --depth;
                    return true;
                }
                else
                {
                    move.Undo();
                }
            }
            --depth;
            return false;
        }

        private ISet<string> cache = new HashSet<string>();

        private bool InSeenCache()
        {
            return cache.Contains(board.CacheKey) || cache.Count == 1000000;
        }

        private void CacheSeen()
        {
            if (cache.Count == 1000000)
            {
                return;
//                cache.Clear();
//                Console.WriteLine("Dumping cache");
            }
            cache.Add(board.CacheKey);
        }
    }
}
