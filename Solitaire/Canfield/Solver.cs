using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Canfield
{
    class Solver
    {
        private Board board;
        private IList<IMove> answer;

        public Solver(Board board)
        {
            this.board = board;
            answer = new List<IMove>();
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
        private bool SolveRecurse()
        {
            ++depth;
            IList<IMove> moves = board.GetMoves();
            //if (depth < 15)
            //{
            //    Console.WriteLine("Depth: " + depth + ", Moves: " + moves.Count);
            //    board.Dump();
            //}
            if (board.IsSolved())
            {
                return true;
            }
            foreach (IMove move in moves)
            {
                move.Apply();
//                Console.WriteLine(move);
//                board.Dump();
//                Console.WriteLine();
                if (InSeenCache())
                {
                    move.Undo();
                }
                else
                {
                    CacheSeen();
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
            }
            --depth;
            return false;
        }

        private ISet<string> cache = new HashSet<string>();

        private bool InSeenCache()
        {
            if (cache.Count == 1000000) // Bail if we've looked quite hard already
            {
                return true;
            }
            return cache.Contains(board.CacheKey);
        }

        private void CacheSeen()
        {
            if (cache.Count == 1000000)
            {
                cache.Clear();
                Console.WriteLine("Dumping cache");
            }
            cache.Add(board.CacheKey);
        }
    }
}
