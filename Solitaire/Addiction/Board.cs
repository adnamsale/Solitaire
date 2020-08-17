using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Solitaire.Addiction
{
    class Board
    {

        private class CardMove : IMove
        {
            private Board board;
            private int iFrom;
            private int jFrom;
            private int iTo;
            private int jTo;
            private string description;

            public CardMove(Board board, int iFrom, int jFrom, int iTo, int jTo)
            {
                this.board = board;
                this.iFrom = iFrom;
                this.jFrom = jFrom;
                this.iTo = iTo;
                this.jTo = jTo;
                description = $"Move {board.Card(iFrom, jFrom)} to ({iTo}, {jTo})";
            }

            public void Apply()
            {
                board.Move(iFrom, jFrom, iTo, jTo);
            }

            public void Undo()
            {
                board.Move(iTo, jTo, iFrom, jFrom);
            }

            public override string ToString()
            {
                return description;
            }
        }

        // The official game just uses a random shuffle, but we use one based on the game number so that
        // games are reproducible
        private class ShuffleMove : IMove
        {
            Card[][] savedBoard;
            long savedSeed;

            public ShuffleMove(Board board)
            {
                this.board = board;
            }

            private Board board;

            public void Apply()
            {
                // Save the current position
                savedSeed = board.Shuffler.Seed;
                savedBoard = new Card[4][];
                for (int i = 0; i < 4; ++i)
                {
                    savedBoard[i] = new Card[13];
                    for (int j = 0; j < 13; ++j)
                    {
                        savedBoard[i][j] = board.Card(i, j);
                    }
                }
                // Collect the out-of-position cards
                IList<Card> shufflers = new List<Card>();
                for (int i = 0; i < 4; ++i)
                {
                    bool shuffle = false;
                    for (int j = 0; j < 13; ++j)
                    {
                        if (!shuffle) {
                            if (board.Card(i, j) == null)
                            {
                                shuffle = true;
                            }
                            else if (j == 0)
                            {
                                if (board.Card(i, j).Rank != 2)
                                {
                                    shuffle = true;
                                }
                            }
                            else
                            {
                                if (board.Card(i, j).Rank != j + 2 || board.Card(i, j).Suit != board.Card(i, j - 1).Suit)
                                {
                                    shuffle = true;
                                }
                            }
                        }
                        if (shuffle)
                        {
                            shufflers.Add(board.Card(i, j));
                        }
                    }
                }
                // Shuffle the out-of-position cards
                board.Shuffler.Shuffle(shufflers);
                // And write them back
                int take = 0;
                for (int i = 0; i < 4; ++i)
                {
                    bool shuffle = false;
                    for (int j = 0; j < 13; ++j)
                    {
                        if (!shuffle)
                        {
                            if (board.Card(i, j) == null)
                            {
                                shuffle = true;
                            }
                            else if (j == 0)
                            {
                                if (board.Card(i, j).Rank != 2)
                                {
                                    shuffle = true;
                                }
                            }
                            else
                            {
                                if (board.Card(i, j).Rank != j + 2 || board.Card(i, j).Suit != board.Card(i, j - 1).Suit)
                                {
                                    shuffle = true;
                                }
                            }
                        }
                        if (shuffle)
                        {
                            board.SetCard(i, j, shufflers[take++]);
                        }
                    }
                }
                ++board.ShuffleCount;
            }

            public void Undo()
            {
                --board.ShuffleCount;
                board.Shuffler.Seed = savedSeed;
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 13; ++j)
                    {
                        board.SetCard(i, j, savedBoard[i][j]);
                    }
                }
            }

            public override string ToString()
            {
                return $"Shuffle";
            }
        }


        private Card[][] board = new Card[4][];

        public Board(IList<Card> deck, Shuffler shuffler)
        {
            for (int i = 0; i < 4; ++i)
            {
                board[i] = new Card[13];
                for (int j = 0; j < 13; ++j)
                {
                    board[i][j] = deck[13 * i + j];
                    if (board[i][j].Rank == 1)
                    {
                        board[i][j] = null;
                    }
                }
            }
            Shuffler = shuffler;
            ShuffleCount = 0;
        }


        public IList<IMove> GetMoves()
        {
            IList<IMove> answer = new List<IMove>();
            for (int iTo = 0; iTo < 4; ++iTo)
            {
                for (int jTo = 0; jTo <13; ++jTo)
                {
                    Card to = board[iTo][jTo];
                    if (null != to)
                    {
                        continue;
                    }
                    if (0 == jTo)
                    {
                        for (int iFrom = 0; iFrom < 4; ++iFrom)
                        {
                            for (int jFrom = 1; jFrom < 13; ++jFrom)
                            {
                                if ((board[iFrom][jFrom]?.Rank ?? 0) == 2)
                                {
                                    answer.Add(new CardMove(this, iFrom, jFrom, iTo, jTo));
                                    //Prioritizing 'good' moves can lead to a shorter solution,
                                    //but it can also cause us to fill the cache before we find a valid solution
                                    //answer.Insert(0, new CardMove(this, iFrom, jFrom, iTo, jTo));
                                }
                            }
                        }
                    }
                    else
                    {
                        Card predecessor = board[iTo][jTo - 1];
                        if (predecessor == null || predecessor.Rank == 13)
                        {
                            continue;
                        }
                        for (int iFrom = 0; iFrom < 4; ++iFrom)
                        {
                            for (int jFrom = 0; jFrom < 13; ++jFrom)
                            {
                                if (board[iFrom][jFrom]?.Suit == predecessor.Suit && board[iFrom][jFrom].Rank == predecessor.Rank + 1)
                                {
                                    bool preferred = board[iTo][0] != null && board[iTo][0].Rank == 2 &&
                                        board[iFrom][jFrom].Suit == board[iTo][0].Suit && board[iFrom][jFrom].Rank == jTo + 2;
                                    if (preferred)
                                    {
                                        //answer.Insert(0, new CardMove(this, iFrom, jFrom, iTo, jTo));
                                        answer.Add(new CardMove(this, iFrom, jFrom, iTo, jTo));
                                    }
                                    else
                                    {
                                        answer.Add(new CardMove(this, iFrom, jFrom, iTo, jTo));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (0 == answer.Count && 3 < ShuffleCount)
            {
                answer.Add(new ShuffleMove(this));
            }
            return answer;
        }

        void Move(int iFrom, int jFrom, int iTo, int jTo)
        {
            board[iTo][jTo] = board[iFrom][jFrom];
            board[iFrom][jFrom] = null;
        }

        Card Card(int i, int j)
        {
            return board[i][j];
        }

        void SetCard(int i, int j, Card card)
        {
            board[i][j] = card;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (board[i][12] != null)
                {
                    return false;
                }
                if (board[i][0] == null || board[i][0].Rank != 2)
                {
                    return false;
                }
                for (int j = 1; j < 12; ++j)
                {
                    if (board[i][j] == null || board[i][j].Rank != j + 2 || board[i][j].Suit != board[i][j - 1].Suit)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Shuffler Shuffler
        {
            get;
            private set;
        }

        public int ShuffleCount
        {
            get;
            set;
        }

        public string CacheKey
        {
            get
            {
                string answer = "";
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 13; ++j)
                    {
                        answer += board[i][j] == null ? "   " : board[i][j].ToString();
                    }
                }
                return answer;
            }
        }

        public void Dump()
        {
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 13; ++j)
                {
                    Console.Write(board[i][j] == null ? "    " : board[i][j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
