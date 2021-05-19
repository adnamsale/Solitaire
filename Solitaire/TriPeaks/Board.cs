using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.TriPeaks
{
    class Board
    {
        public class TableauPos
        {
            private TableauPos leftChild;
            private TableauPos rightChild;

            public TableauPos(Card card, TableauPos leftChild, TableauPos rightChild)
            {
                Card = card;
                this.leftChild = leftChild;
                this.rightChild = rightChild;
            }

            public Card Card
            {
                get;
                set;
            }

            public bool IsFaceUp
            {
                get => null != Card && leftChild?.Card == null && rightChild?.Card == null;
            }
        }

        private List<Card> waste = new List<Card>();
        private List<Card> stock = new List<Card>();
        private TableauPos[][] tableau = new TableauPos[4][];

        public Board(IList<Card> deck)
        {
            tableau[0] = new TableauPos[10];
            for (int i = 0; i < 10; ++i)
            {
                tableau[0][i] = new TableauPos(deck[51 - i], null, null);
                ++Heuristic;
            }
            tableau[1] = new TableauPos[9];
            tableau[2] = new TableauPos[6];
            tableau[3] = new TableauPos[3];
            for (int peak = 0; peak < 3; ++peak)
            {
                for (int i = 0; i < 3; ++i)
                {
                    tableau[1][3 * peak + i] = new TableauPos(deck[51 - 10 - 6 * peak - i], tableau[0][3 * peak + i], tableau[0][3 * peak + i + 1]);
                    ++Heuristic;
                }
                for (int i = 0; i < 2; ++i)
                {
                    tableau[2][2 * peak + i] = new TableauPos(deck[51 - 10 - 6 * peak - 3 - i], tableau[1][3 * peak + i], tableau[1][3 * peak + i + 1]);
                    ++Heuristic;
                }
                tableau[3][peak] = new TableauPos(deck[51 - 10 - 6 * peak - 5], tableau[2][2 * peak], tableau[2][2 * peak + 1]);
                ++Heuristic;
            }
            for (int i = 0; i < 52 - 10 - 9 - 6 - 3; ++i)
            {
                stock.Add(deck[i]);
            }
        }

        public class StockMove : IMove
        {
            private Board board;

            public StockMove(Board board)
            {
                this.board = board;
            }

            public void Apply()
            {
                board.ApplyStockMove();
            }

            public void Undo()
            {
                board.UndoStockMove();
            }

            public override string ToString()
            {
                return $"Stock";
            }
        }

        public class TableauMove : IMove
        {
            private Board board;
            private TableauPos pos;
            private Card card;

            public TableauMove(Board board, TableauPos pos)
            {
                this.board = board;
                this.pos = pos;
                card = pos.Card;
            }

            public void Apply()
            {
                board.ApplyTableauMove(pos);
            }

            public void Undo()
            {
                board.UndoTableauMove(pos);
            }

            public override string ToString()
            {
                string answer = $"Move {card}";
                return answer;
            }
        }

        public void ApplyStockMove()
        {
            waste.Add(stock.Last());
            stock.RemoveAt(stock.Count - 1);
        }

        public void UndoStockMove()
        {
            stock.Add(waste.Last());
            waste.RemoveAt(waste.Count - 1);
        }

        public void ApplyTableauMove(TableauPos pos)
        {
            waste.Add(pos.Card);
            pos.Card = null;
            --Heuristic;
        }

        public void UndoTableauMove(TableauPos pos)
        {
            pos.Card = waste.Last();
            waste.RemoveAt(waste.Count - 1);
            ++Heuristic;
        }

        public IList<IMove> GetMoves()
        {
            List<IMove> answer = new List<IMove>();
            foreach (TableauPos[] line in tableau)
            {
                foreach (TableauPos pos in line)
                {
                    if (pos.IsFaceUp)
                    {
                        Card card = pos.Card;
                        if (waste.Count == 0)
                        {
                            answer.Add(new TableauMove(this, pos));
                        }
                        else
                        {
                            int diff = Math.Abs(card.Rank - waste.Last().Rank);
                            if (1 == diff || 12 == diff)
                            {
                                answer.Add(new TableauMove(this, pos));
                            }
                        }
                    }
                }
            }
            if (stock.Count != 0 && answer.Count == 0)
            {
                answer.Add(new StockMove(this));
            }
            return answer;
        }

        public bool IsSolved()
        {
            return 0 == Heuristic;
        }

        public int Heuristic { get; private set; } = 0;

        public string CacheKey
        {
            get
            {
                string answer = "";
                answer += stock.Count == 0 ? "   " : stock.Last().ToString();
                foreach (TableauPos[] line in tableau)
                {
                    foreach (TableauPos pos in line)
                    {
                        answer += pos.Card == null ? " " : "X";
                    }
                }

                return answer;
            }
        }
    }
}
