using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Fifteen
{
    class Board
    {
        private static readonly ulong[] masks = new ulong[] {
            0xF000000000000000, 0x0F00000000000000, 0x00F0000000000000, 0x000F000000000000,
            0x0000F00000000000, 0x00000F0000000000, 0x000000F000000000, 0x0000000F00000000,
            0x00000000F0000000, 0x000000000F000000, 0x0000000000F00000, 0x00000000000F0000,
            0x000000000000F000, 0x0000000000000F00, 0x00000000000000F0, 0x000000000000000F};

        private ulong grid;

        public class Move
        {
            public int Heuristic
            {
                get;
                private set;
            }

            public ulong Board
            {
                get;
                private set;
            }

            public Move(ulong board, int from, int to)
            {
                Board = MoveZero(board, from, to);
                Heuristic = Heuristic(Board);
            }
        }

        public static bool IsSolved(ulong board)
        {
            return board == 0x0123456789ABCDEF;
        }

        public static ulong Create(string desc)
        {
            string[] elems = desc.Split(' ');
            if (16 != elems.Length)
            {
                throw new InvalidOperationException("Board description must have 16 elements");
            }
            ulong answer = 0;
            for (int i = 0; i < 16; ++i)
            {
                answer = Set(answer, i, int.Parse(elems[i]));
            }
            return answer;
        }

        public ulong CacheKey => grid;

        public static IEnumerable<ulong> GetMoves(ulong board)
        {
            List<Move> answer = new List<Move>();
            int spacePos = 0;
            for (int i = 0; i < 16; ++i)
            {
                if (0 == Get(board, i))
                {
                    spacePos = i;
                    break;
                }
            }
            if (3 < spacePos)
            {
                answer.Add(new Move(board, spacePos, spacePos - 4));
            }
            if (spacePos < 12)
            {
                answer.Add(new Move(board, spacePos, spacePos + 4));
            }
            if (0 < spacePos % 4)
            {
                answer.Add(new Move(board, spacePos, spacePos - 1));
            }
            if (spacePos % 4 < 3)
            {
                answer.Add(new Move(board, spacePos, spacePos + 1));
            }

            //return answer.OrderBy(x => x.Heuristic).Select(x => x.Board);
            return answer.Select(x => x.Board);
        }

        public static int Heuristic(ulong board)
        {
            int answer = 0;
            for (int i = 0; i < 16; ++i)
            {
                int val = Get(board, i);
                if (0 == val)
                {
                    continue;
                }
                int xDelta = Math.Abs(val % 4 - i % 4);
                int yDelta = Math.Abs((val / 4) - (i / 4));
                answer += xDelta + yDelta;
            }
            return answer;
        }

        public static void Dump(ulong board)
        {
            for (int i = 0; i < 16; ++i)
            {
                Console.Write(string.Format("{0,2} ", Get(board, i)));
                if (3 == i % 4)
                {
                    Console.WriteLine();
                }
            }
        }

        internal static ulong MoveZero(ulong board, int from, int to)
        {
            ulong answer = Set(board, from, Get(board, to));
            answer &= ~masks[to];
            return answer;
        }

        private static ulong Set(ulong board, int pos, int val)
        {
            return (board & ~masks[pos]) | ((ulong)val << 4 * (15 - pos));
        }

        private static int Get(ulong board, int pos)
        {
            return (int)((board & masks[pos]) >> 4 * (15 - pos));
        }
    }
}
