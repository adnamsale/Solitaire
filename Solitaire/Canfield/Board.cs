using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Solitaire.Canfield
{
    class Board
    {
        class Slot
        {
            public Slot()
            {
                Card = null;
                Parent = null;
            }

            public Slot(Card card, Slot parent)
            {
                Card = card;
                Parent = parent;
            }

            public Card Card
            {
                get;
                private set;
            }

            public Slot Parent
            {
                get;
                set;
            }

            // Does a tableau slot accept the card?
            public bool Accepts(Card other)
            {
                if (Card == null)
                {
                    return true;
                }
                return Card.Suit.Color != other.Suit.Color && ((Card.Rank - other.Rank - 1) % 13 == 0);
            }

            // Does a foundation slot accept the card?
            public bool Accepts(Card other, int lead)
            {
                if (Card == null)
                {
                    return other.Rank == lead;
                }
                else
                {
                    if (other.Rank == lead)
                    {
                        throw new InvalidOperationException("Foundation should be empty for lead");
                    }
                    if (other.Suit != Card.Suit)
                    {
                        throw new InvalidOperationException("Adding card to wrong foundation!");
                    }
                    return ((other.Rank - Card.Rank - 1) % 13 == 0);
                }
            }

            public override string ToString()
            {
                return Card?.ToString() ?? "   ";
            }
        }

        private class SlotMove : IMove
        {
            private Board board;
            private int from;
            private int to;
            private Slot mover;
            private Slot oldFlipChainStart;

            public SlotMove(Board board, int from, int to) : this(board, from, to, board.board[from])
            {
            }

            public SlotMove(Board board, int from, int to, Slot mover)
            {
                this.board = board;
                this.from = from;
                this.to = to;
                this.mover = mover;
            }

            public void Apply()
            {
                board.Move(from, to, mover);
                oldFlipChainStart = board.flipChainStart;
                board.flipChainStart = null;
            }

            public void Undo()
            {
                board.flipChainStart = oldFlipChainStart;
                board.Move(to, from, mover);
            }

            public override string ToString()
            {
                return $"Move {mover} from {board.Describe(from)} to {board.Describe(to)}";
            }
        }

        private class FlipMove : IMove
        {
            public FlipMove(Board board)
            {
                this.board = board;
            }

            private Board board;
            private Slot oldFlipChainStart = null;
            private Card mover = null;

            public void Apply()
            {
                Flip();
            }

            public void Undo()
            {
                UnFlip();
            }

            private void Flip()
            {
                oldFlipChainStart = board.flipChainStart;
                if (null == board.flipChainStart)
                {
                    board.flipChainStart = board.board[STOCK];
                }
                mover = board.board[STOCK].Card; // Just needed for ToString
                board.Move(STOCK, WASTE);
            }

            private void UnFlip()
            {
                board.Move(WASTE, STOCK);
                board.flipChainStart = oldFlipChainStart;
            }

            public override string ToString()
            {
                return $"Flip {mover} to WASTE";
            }
        }

        private class ResetMove: IMove
        {
            public ResetMove(Board board)
            {
                this.board = board;
            }

            private Board board;

            public void Apply()
            {
                while (null != board.board[WASTE].Card)
                {
                    board.Move(WASTE, STOCK);
                }
            }

            public void Undo()
            {
                while (null != board.board[STOCK].Card)
                {
                    board.Move(STOCK, WASTE);
                }
            }

            public override string ToString()
            {
                return "Reset";
            }
        }

        private Slot[] board = new Slot[11];
        private int lead;
        private Slot flipChainStart;

        const int STOCK = 0;
        const int WASTE = 1;
        const int RESERVE = 2;
        const int FOUNDATION = 3;
        const int TABLEAU = 7;

        public Board(IList<Card> deck)
        {
            deck = new List<Card>(deck.Reverse());

            for (int i = 0; i < 11; ++i)
            {
                board[i] = new Slot();
            }
            for (int i = 0; i < 13; ++i)
            {
                board[RESERVE] = new Slot(deck[i], board[RESERVE]);
            }
            for (int i = 0; i < 4; ++i)
            {
                board[TABLEAU + i] = new Slot(deck[13 + i], board[TABLEAU + i]);
            }
            board[FOUNDATION + deck[17].Suit.Ordinal] =
                new Slot(deck[17], board[FOUNDATION + deck[17].Suit.Ordinal]);
            lead = deck[17].Rank;
            for (int i = 51; 18 <= i; --i)
            {
                board[STOCK] = new Slot(deck[i], board[STOCK]);
            }
        }


        public IList<IMove> GetMoves()
        {
            IList<IMove> answer = new List<IMove>();
            // We don't need to consider any of these moves if the last move was a flip
            if (null == flipChainStart)
            {
                // Special case - if there is an open tableau space and the reserve is not empty
                // then filling the tableau is the only allowed move
                if (null != board[RESERVE].Card)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        if (board[TABLEAU + i].Card == null)
                        {
                            answer.Add(new SlotMove(this, RESERVE, TABLEAU + i));
                            return answer;
                        }
                    }
                }
                if (null != board[RESERVE].Card)
                {
                    Card reserve = board[RESERVE].Card;
                    // Move reserve to foundation?
                    if (board[FOUNDATION + reserve.Suit.Ordinal].Accepts(reserve, lead))
                    {
                        answer.Add(new SlotMove(this, RESERVE, FOUNDATION + reserve.Suit.Ordinal));
                    }
                    // Move reserve to tableau?
                    for (int i = 0; i < 4; ++i)
                    {
                        if (board[TABLEAU + i].Accepts(board[RESERVE].Card))
                        {
                            answer.Add(new SlotMove(this, RESERVE, TABLEAU + i));
                        }
                    }
                }
                // Move tableau to foundation
                for (int i = 0; i < 4; ++i)
                {
                    if (null != board[TABLEAU + i].Card)
                    {
                        Card tableau = board[TABLEAU + i].Card;
                        if (board[FOUNDATION + tableau.Suit.Ordinal].Accepts(tableau, lead))
                        {
                            answer.Add(new SlotMove(this, TABLEAU + i, FOUNDATION + tableau.Suit.Ordinal));
                        }
                    }
                }
                // Move tableau to tableau
                for (int i = 0; i < 4; ++i)
                {
                    if (null == board[TABLEAU + i].Card)
                    {
                        continue;
                    }
                    Slot tableau = FindTopSlot(TABLEAU + i);
                    for (int j = 0; j < 4; ++j)
                    {
                        if (i != j && null != board[TABLEAU + j].Card && board[TABLEAU + j].Accepts(tableau.Card))
                        {
                            answer.Add(new SlotMove(this, TABLEAU + i, TABLEAU + j, tableau));
                        }
                    }
                }
            }
            if (null != board[WASTE].Card)
            {
                Card waste = board[WASTE].Card;
                // Move waste to foundation
                if (board[FOUNDATION + waste.Suit.Ordinal].Accepts(waste, lead))
                {
                    answer.Add(new SlotMove(this, WASTE, FOUNDATION + waste.Suit.Ordinal));
                }
                // Move waste to tableau
                for (int i = 0; i < 4; ++i)
                {
                    if (board[TABLEAU + i].Accepts(waste) && IsCardUseful(waste))
                    {
                        answer.Add(new SlotMove(this, WASTE, TABLEAU + i));
                    }
                }
            }
            if (null != board[STOCK].Card && board[STOCK] != flipChainStart)
            {
                answer.Add(new FlipMove(this));
            }
            if (null == board[STOCK].Card && null != board[WASTE].Card)
            {
                answer.Add(new ResetMove(this));
            }
            return answer;
        }

        void Move(int from, int to)
        {
            Move(from, to, board[from]);
        }

        void Move(int from, int to, Slot mover)
        {
            Slot oldFrom = board[from];
            board[from] = mover.Parent;
            mover.Parent = board[to];
            board[to] = oldFrom;
        }

        private bool IsCardUseful(Card card)
        {
            // A card is only useful (for placing on the tableau) if it is an
            // ancestor of either the reserve card or a tableau pile
            if (null != board[RESERVE].Card && IsAncestor(card, board[RESERVE].Card)) {
                return true;
            }
            for (int i = 0; i < 4; ++i)
            {
                if (null == board[TABLEAU + i].Card)
                {
                    continue;
                }
                Slot tableau = FindTopSlot(TABLEAU + i);
                if (IsAncestor(card, board[TABLEAU + i].Card))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAncestor(Card parent, Card child)
        {
            // We are an ancestor if lead is not between parent and child
            // and the colors match/differ depending on whether the cards are an odd/even
            // distance apart.
            int parentRank = parent.Rank;
            int childRank = child.Rank;
            if (parentRank < childRank)
            {
                parentRank += 13;
            }
            if (childRank <= lead && lead <= parentRank)
            {
                return false;
            }
            if ((parentRank - childRank) % 2 == 0)
            {
                return parent.Suit.Color == child.Suit.Color;
            }
            else
            {
                return parent.Suit.Color != child.Suit.Color;
            }
        }

        string Describe(int col)
        {
            if (col == STOCK)
            {
                return "STOCK";
            }
            else if (col == WASTE)
            {
                return "WASTE";
            }
            else if (col == RESERVE)
            {
                return "RESERVE";
            }
            else if (col < TABLEAU)
            {
                return "FOUNDATION";
            }
            else
            {
                return $"TABLEAU {col - TABLEAU}";
            }
        }

        private Slot FindTopSlot(int col)
        {
            Slot walk = board[col];
            while (walk.Parent.Card != null)
            {
                walk = walk.Parent;
            }
            return walk;
        }

        public bool IsSolved()
        {
            return board[RESERVE].Parent.Card == null;
            for (int i = 0; i < 4; ++i)
            {
                Card foundation = board[FOUNDATION + i].Card;
                if (null == foundation || ((lead - foundation.Rank - 1) % 13) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public string CacheKey
        {
            get
            {
                string answer = board[RESERVE].ToString();
                answer += board[WASTE].ToString();
                for (int i = 0; i < 4; ++i)
                {
                    answer += board[FOUNDATION + i];
                }
                for (int i = 0; i < 4; ++i)
                {
                    Slot walk = board[TABLEAU + i];
                    while (null != walk.Card)
                    {
                        answer += walk;
                        walk = walk.Parent;
                    }
                    answer += "   ";
                }
                return answer;
            }
        }

        public void Dump()
        {
            Console.Write($"({board[STOCK]}, {board[WASTE]}) {board[RESERVE]} [");
            for (int i = 0; i < 4; ++i)
            {
                Console.Write(board[TABLEAU + i] + " ");
            }
            Console.Write("]  [");
            foreach (Suit s in Suit.AllValues)
            {
                Console.Write(board[FOUNDATION + s.Ordinal] + " ");
            }
            Console.WriteLine("]");
        }
    }
}
