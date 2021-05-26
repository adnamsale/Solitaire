using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Solitaire.Pyramid
{
    class Solver
    {
        public Solver(Board board)
        {
            this.board = board;
        }

        private Board board;
        private Stack<IMove> path = new Stack<IMove>();
        private IDictionary<string, int> seenCache = new Dictionary<String, int>();

        public bool Solve()
        {
            var sw = Stopwatch.StartNew();
            int bound = board.Heuristic;
            while (true)
            {
                Console.WriteLine($"Here we go with bound {bound}");
                seenCache.Clear();
                int t = Search(0, bound);
                long time = sw.ElapsedMilliseconds;
                if (t == -1)
                {
                    foreach (IMove move in path)
                    {
                        Console.WriteLine(move);
                    }
                    Console.WriteLine($"Total nodes: {nodeCount}");
                    return true;
                }
                else if (t == int.MaxValue)
                {
                    return false;
                }
                bound = t;
            }
        }

        int nodeCount = 0;
        private int Search(int g, int bound)
        {
            ++nodeCount;
            int f = g + board.Heuristic;
            if (bound < f)
            {
                return f;
            }
            if (board.IsSolved())
            {
                return -1;
            }
            int min = int.MaxValue;
            foreach (IMove move in board.GetMoves())
            {
                path.Push(move);
                move.Apply();
//                Console.WriteLine("Trying: " + move);
                string cacheKey = board.CacheKey;
                if (!InCache(cacheKey, g))
                {
                    seenCache[cacheKey] = g;
                    if (1000000 == seenCache.Count)
                    {
                        seenCache.Clear();
                    }
                    int t = Search(g + 1, bound);
                    if (t == -1)
                    {
                        return -1;
                    }
                    else if (t < min)
                    {
                        min = t;
                    }
                }
                move.Undo();
//                Console.WriteLine("Backing out: " + move);
                path.Pop();
            }
            return min;
        }

        private bool InCache(string cacheKey, int g)
        {
            int existing;
            if (!seenCache.TryGetValue(cacheKey, out existing))
            {
                return false;
            }
            return existing < g;
        }
    }
}
