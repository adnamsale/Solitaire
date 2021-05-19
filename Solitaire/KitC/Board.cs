using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.KitC
{
    enum Mode
    {
        Adding,
        Removing
    }

    class Board
    {
        private List<Card>[] board = new List<Card>[4];
        private List<Card> deck = new List<Card>();
        private int count = 0;
        private Mode mode = Mode.Adding;

        const int JACK = 0;
        const int QUEEN = 1;
        const int KING = 2;
        const int MISC = 3;

        public Board(IList<Card> deck)
        {
            this.deck.AddRange(deck);
            board[KING] = new List<Card>();
            board[QUEEN] = new List<Card>();
            board[JACK] = new List<Card>();
            board[MISC] = new List<Card>();
        }

        public class PlaceMove : IMove
        {
            private Board board;
            private int row;
            private int insertPos;

            public PlaceMove(Board board, int row)
            {
                this.board = board;
                this.row = row;
            }

            public void Apply()
            {
                insertPos = board.Place(row);
            }

            public void Undo()
            {
                board.Unplace(row, insertPos);
            }

            public override string ToString()
            {
                return $"Place in row {row}";
            }
        }

        public class RemoveMove : IMove
        {
            private Board board;
            private int row1;
            private int col1;
            private int row2;
            private int col2;
            private Card save1;
            private Card save2;

            public RemoveMove(Board board, int row1, int col1, int row2 = 0, int col2 = 0)
            {
                this.board = board;
                this.row1 = row1;
                this.col1 = col1;
                this.row2 = row2;
                this.col2 = col2;
            }

            public void Apply()
            {
                if (0 != row2 || 0 != col2)
                {
                    save2 = board.Remove(row2, col2);
                }
                save1 = board.Remove(row1, col1);
                if (save1.Rank + (null != save2 ? save2.Rank : 0) != 10)
                {
                    throw new InvalidOperationException("Huh");
                }
            }

            public void Undo()
            {
                board.Replace(row1, col1, save1);
                if (null != save2)
                {
                    board.Replace(row2, col2, save2);
                }
            }

            public override string ToString()
            {
                string answer = $"Remove at [{row1}][{col1}]" + (0 != row2 || 0 != col2 ? $" and [{row2}][{col2}]" : "");
                return answer;
            }
        }

        public IList<IMove> GetMoves()
        {
            if (mode == Mode.Adding && (deck.Count == 0 || count == 16))
            {
                mode = Mode.Removing;
            }
            if (mode == Mode.Adding)
            {
                return GetAddMoves();
            }
            else
            {
                IList<IMove> answer = GetRemoveMoves();
                if (answer.Count == 0)
                {
                    mode = Mode.Adding;
                    answer = GetAddMoves();
                }
                return answer;
            }
        }

        public IList<IMove> GetAddMoves()
        {
            IList<IMove> answer = new List<IMove>();
            if (count < 16 && deck.Count != 0)
            {
                Card top = deck.Last();
                if (11 <= top.Rank)
                {
                    if (board[top.Rank - 11].Count < 4)
                    {
                        answer.Add(new PlaceMove(this, top.Rank - 11));
                    }
                }
                else
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        if (board[i].Count < 4)
                        {
                            answer.Add(new PlaceMove(this, i));
                        }
                    }
                }
            }
            return answer;
        }

        public IList<IMove> GetRemoveMoves()
        {
            IList<IMove> answer = new List<IMove>();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < board[i].Count; ++j)
                {
                    if (board[i][j].Rank == 10)
                    {
                        answer.Add(new RemoveMove(this, i, j));
                    }
                    else if (board[i][j].Rank < 10)
                    {
                        for (int k = i; k < 4; ++k)
                        {
                            for (int l = (k == i ? j + 1 : 0); l < board[k].Count; ++l)
                            {
                                if (board[i][j].Rank + board[k][l].Rank == 10)
                                {
                                    answer.Add(new RemoveMove(this, i, j, k, l));
                                }
                            }
                        }
                    }
                }
            }
            return answer;
        }

        public int Remaining => deck.Count + board[0].Count + board[1].Count + board[2].Count + board[3].Count;

        int Place(int row)
        {
            Card top = deck.Last();
            int i = 0;
            while (i < board[row].Count && board[row][i].Rank < top.Rank)
            {
                ++i;
            }
            board[row].Insert(i, top);
            deck.RemoveAt(deck.Count - 1);
            ++count;
            return i;
        }

        void Unplace(int row, int pos)
        {
            Card card = board[row][pos];
            deck.Add(card);
            board[row].RemoveAt(pos);
            --count;
            mode = Mode.Adding;
        }

        Card Remove(int row, int col)
        {
            Card card = board[row][col];
            board[row].RemoveAt(col);
            --count;
            return card;
        }

        void Replace(int row, int col, Card card)
        {
            board[row].Insert(col, card);
            ++count;
            mode = Mode.Removing;
        }

        public bool IsSolved()
        {
            if (deck.Count != 0)
            {
                return false;
            }
            if (board[MISC].Count != 0)
            {
                return false;
            }
            for (int i = 0; i < 3; ++i)
            {
                if (board[i].Count != 4)
                {
                    return false;
                }
                for (int j = 0; j < 4; ++j)
                {
                    if (board[i][j].Rank != i + 11)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string CacheKey
        {
            get
            {
                string answer = "";
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < board[i].Count; ++j)
                    {
                        answer += board[i][j].Rank.ToString("D2");
                    }
                    answer = answer.PadRight(8 + (i + 1));
                }
                answer += deck.Count.ToString("D2");
                return answer;
            }
        }


        private void DumpRow(int row)
        {
            Console.Write(row + ": ");
            for (int i = 0; i < board[row].Count; ++i)
            {
                Console.Write(board[row][i] + " ");
            }
            Console.WriteLine();
        }
        public void Dump()
        {
            DumpRow(JACK);
            DumpRow(QUEEN);
            DumpRow(KING);
            DumpRow(MISC);
            Console.WriteLine(deck.Count == 0 ? "<Empty>" : deck.Last().ToString());
            Console.WriteLine();
        }
    }
}
