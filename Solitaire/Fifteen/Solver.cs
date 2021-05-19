using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Solitaire.Fifteen
{
    class Solver
    {
        public Solver()
        {
        }

        public bool Solve(ulong board)
        {
            var sw = Stopwatch.StartNew();
            int bound = Board.Heuristic(board);
            Stack<ulong> path = new Stack<ulong>();
            path.Push(board);
            while (true)
            {
                Console.WriteLine($"Here we go with bound {bound}");
                int t = Search(path, 0, bound);
                long time = sw.ElapsedMilliseconds;
                if (t == -1)
                {
                    foreach (ulong node in path)
                    {
                        Board.Dump(node);
                        Console.WriteLine();
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
        private int Search(Stack<ulong> path, int g, int bound)
        {
            ++nodeCount;
            ulong node = path.Peek();
            int f = g + Board.Heuristic(node);
            if (bound < f)
            {
                return f;
            }
            if (Board.IsSolved(node))
            {
                return -1;
            }
            int min = int.MaxValue;
            foreach (ulong move in Board.GetMoves(node))
            {
                if (!path.Contains(move))
                {
                    path.Push(move);
                    int t = Search(path, g + 1, bound);
                    if (t == -1)
                    {
                        return -1;
                    }
                    else if (t < min)
                    {
                        min = t;
                    }
                    path.Pop();
                }
            }
            return min;
        }
/*
        public bool Solve()
        {
            ulong saved = board.Save();
            int maxDepth = 1;
            while (true)
            {
                board.Restore(saved);
                board.Dump();
                ResetCache();
                depth = 0;
                nodes = 0;
                if (SolveRecurse(maxDepth))
                {
                    return true;
                }
                Console.WriteLine($"Failed at depth {maxDepth} after {nodes} nodes");
                ++maxDepth;
            }
        }
        */
        /*
        int nodes = 0;
        int depth = 0;
        private bool SolveRecurse(int maxDepth)
        {
            if (board.IsSolved())
            {
                return true;
            }
            if (maxDepth == depth)
            {
                return false;
            }
            ++depth;
            ++nodes;
            IList<IMove> moves = board.GetMoves();
            foreach (IMove move in moves)
            {
                move.Apply();
                if (InSeenCache(depth))
                {
                    move.Undo();
                }
                else
                {
                    CacheSeen(depth);
                    if (SolveRecurse(maxDepth))
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

        private IDictionary<ulong, int> cache = new Dictionary<ulong, int>();

        private bool InSeenCache(int depth)
        {
            int foundDepth;
            if (cache.TryGetValue(board.CacheKey, out foundDepth))
            {
                return foundDepth < depth;
            }
            return false;
        }

        private void CacheSeen(int depth)
        {
            if (cache.Count == 2000000)
            {
                ResetCache();
                Console.WriteLine("Dumping cache");
            }
            cache[board.CacheKey] = depth;
        }

        private void ResetCache()
        {
            cache.Clear();
        }
        */
    }
}
