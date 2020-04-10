using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Scorpion
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
            if (depth < 15)
            {
                //Console.WriteLine("Depth: " + depth + ", Moves: " + moves.Count);
            }
            if (moves.Count == 0 && board.IsSolved())
            {
                return true;
            }
            foreach (IMove move in moves)
            {
                move.Apply();
//                Console.WriteLine(move);
//                board.Dump();
                if (InFailureCache())
                {
                    move.Undo();
                }
                else if (SolveRecurse())
                {
                    answer.Add(move);
                    --depth;
                    return true;
                }
                else
                {
                    CacheFailure();
                    move.Undo();
                }
            }
            --depth;
            return false;
        }

        private ISet<string> cache = new HashSet<string>();

        private bool InFailureCache()
        {
            return cache.Contains(board.CacheKey);
        }

        private void CacheFailure()
        {
            if (cache.Count == 1000000)
            {
                cache.Clear();
            }
            cache.Add(board.CacheKey);
        }
    }
}
