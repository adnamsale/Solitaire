using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Scorpion
{
    class Board
    {
        class Slot
        {
            private int column;

            public Slot(int column)
            {
                this.column = column;
            }

            public Slot(Card card, bool isFaceUp, Slot parent)
            {
                Card = card;
                IsFaceUp = isFaceUp;
                Parent = parent;
                column = -1;
            }

            public Card Card
            {
                get;
                private set;
            }

            public bool IsFaceUp
            {
                get;
                set;
            }

            public Slot Parent
            {
                get;
                set;
            }

            public int Column
            {
                get
                {
                    if (column != -1)
                    {
                        return column;
                    }
                    return Parent.Column;
                }
            }

            public override string ToString()
            {
                return IsFaceUp ? Card.ToString() : "XXX";
            }
        }

        class SlotMove : IMove
        {
            private Board board;
            private Slot mover;
            private Slot newParent;
            private Slot oldParent;
            private bool oldIsFaceUp;

            public SlotMove(Board board, Slot mover, Slot newParent)
            {
                this.board = board;
                this.mover = mover;
                this.newParent = newParent;
                oldParent = null;
            }

            public void Apply()
            {
                oldParent = board.Move(mover, newParent);
                oldIsFaceUp = oldParent.IsFaceUp;
                if (!oldParent.IsFaceUp)
                {
                    oldParent.IsFaceUp = true;
                }
            }

            public void Undo()
            {
                oldParent.IsFaceUp = oldIsFaceUp;
                board.Move(mover, oldParent);
            }

            public override string ToString()
            {
                return mover.Card + " -> " + newParent.Card;
            }
        }

        class DealMove : IMove
        {
            private Board board;
            public DealMove(Board board)
            {
                this.board = board;
            }

            public void Apply()
            {
                board.DealTail();
            }

            public void Undo()
            {
                board.UndealTail();
            }

            public override string ToString()
            {
                return "DEAL TAIL";
            }
        }


        private Card[] tail;
        private Slot[] body;

        public Board(IList<Card> deck)
        {
            deck = new List<Card>(deck.Reverse());

            body = new Slot[7];
            for (int i = 0; i < 7; ++i)
            {
                body[i] = new Slot(i);
            }
            for (int i = 0; i < 49; ++i)
            {
                int col = i % 7;
                body[col] = new Slot(deck[i], !(col <= 3 && i < 21), body[col]);
            }
            tail = new Card[3];
            for (int i = 0; i < 3; ++i)
            {
                tail[i] = deck[i + 49];
            }
        }

        Slot Move(Slot mover, Slot newParent)
        {
            Slot oldParent = mover.Parent;
            mover.Parent = newParent;
            int oldColumn = oldParent.Column;
            int newColumn = newParent.Column;
            body[newColumn] = body[oldColumn];
            body[oldColumn] = oldParent;
            return oldParent;
        }

        void DealTail()
        {
            for (int i = 0; i < 3; ++i)
            {
                Slot newSlot = new Slot(tail[i], true, body[i]);
                body[i] = newSlot;
                tail[i] = null;
            }
        }

        void UndealTail()
        {
            for (int i = 0; i < 3; ++i)
            {
                tail[i] = body[i].Card;
                body[i] = body[i].Parent;
            }
        }

        public IList<IMove> GetMoves()
        {
            bool doneEmpty = false;
            IList<IMove> answer = new List<IMove>();
            for (int i = 0; i < 7; ++i)
            {
                Slot target = body[i];
                if (null == target.Card)
                {
                    if (doneEmpty)
                    {
                        continue;
                    }
                    doneEmpty = true;
                    for (int j = 0; j < 7; ++j)
                    {
                        Slot walk = body[j];
                        while (null != walk.Card)
                        {
                            if (walk.Card.Rank == 13 && walk.IsFaceUp && walk.Parent.Card != null)
                            {
                                answer.Add(new SlotMove(this, walk, target));
                            }
                            walk = walk.Parent;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < 7; ++j)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        Slot walk = body[j];
                        while (null != walk.Card)
                        {
                            if (walk.IsFaceUp && walk.Card.Suit == target.Card.Suit && walk.Card.Rank + 1 == target.Card.Rank)
                            {
                                answer.Add(new SlotMove(this, walk, target));
                                j = 7;
                                break;
                            }
                            walk = walk.Parent;
                        }
                    }
                }
            }
            if (null != tail[0] && 0 == answer.Count)
            {
                answer.Add(new DealMove(this));
            }
            return answer;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < 7; ++i)
            {
                if (body[i].Card == null)
                {
                    continue;
                }
                else if (body[i].Card.Rank != 1)
                {
                    return false;
                }
                else
                {
                    Slot walk = body[i];
                    while (null != walk.Parent.Card)
                    {
                        if (walk.Parent.Card.Suit != walk.Card.Suit)
                        {
                            return false;
                        }
                        if (walk.Parent.Card.Rank != walk.Card.Rank + 1)
                        {
                            return false;
                        }
                        walk = walk.Parent;
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
                for (int i = 0; i < 7; ++i)
                {
                    answer += body[i].Card == null ? "   " : body[i].Card.ToString();
                }
                return answer;
            }
        }

        public void Dump()
        {
            Console.WriteLine();
            for (int i = 0; i < 7; ++i)
            {
                Slot slot = body[i];
                while (null != slot)
                {
                    Console.Write(slot + " ");
                    slot = slot.Parent;
                }
                Console.WriteLine();
            }
            for (int i = 0; i < 3; ++i)
            {
                Console.Write(tail[i] + " ");
            }
            Console.WriteLine();
        }
    }
}
