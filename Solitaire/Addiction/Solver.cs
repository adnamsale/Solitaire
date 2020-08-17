using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Addiction
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
            Console.WriteLine($"{answer.Count} moves");
            foreach (IMove move in answer.Reverse())
            {
                Console.WriteLine(move);
            }
        }

        int depth = 0;
        private bool SolveRecurse()
        {
            if (board.IsSolved())
            {
                return true;
            }
            ++depth;
            IList<IMove> moves = board.GetMoves();
            foreach (IMove move in moves)
            {
                move.Apply();
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
            if (cache.Count == 2000000) // Bail if we've looked quite hard already
            {
                return true;
            }
            return cache.Contains(board.CacheKey);
        }

        private void CacheSeen()
        {
            if (cache.Count == 2000000)
            {
                cache.Clear();
                Console.WriteLine("Dumping cache");
            }
            cache.Add(board.CacheKey);
        }
    }
}
