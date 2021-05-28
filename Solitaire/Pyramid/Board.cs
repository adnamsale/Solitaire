using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Pyramid

{
    class Board
    {
        public class TableauPos
        {
            public TableauPos(Card card)
            {
                Card = card;
            }

            public Card Card
            {
                get;
                set;
            }

            public TableauPos LeftChild { get; set; }
            public TableauPos RightChild { get; set; }

            public bool IsAvailable => null != Card && LeftChild?.Card == null && RightChild?.Card == null;

            public bool IsPairableWith(TableauPos child)
            {
                return null != Card && (LeftChild?.Card == child.Card || RightChild?.Card == child.Card) &&
                    (LeftChild?.Card == null || RightChild?.Card == null);
            }
        }

        private List<Card> foundation = new List<Card>();
        private List<Card> stock = new List<Card>();
        private List<Card> waste = new List<Card>();
        private TableauPos[][] tableau = new TableauPos[7][];
        private int tableauRank = 0;
        // We don't allow moves that only affect the tableau after a flip. Since flips and tableau-only moves don't
        // affect each other we can do them in any order so we require the tableau-only moves first. That prevents
        // an exponential explosion of move paths.
        private bool canPerformTableauMove = true;

        public Board(IList<Card> deck)
        {
            int deckPos = 51;
            for (int i = 0; i < 7; ++i)
            {
                tableau[i] = new TableauPos[i + 1];
                for (int j = 0; j < i + 1; ++j)
                {
                    Card card = deck[deckPos--];
                    tableauRank += card.Rank;
                    tableau[i][j] = new TableauPos(card);
                }
            }
            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j <= i; ++j)
                {
                    tableau[i][j].LeftChild = tableau[i + 1][j];
                    tableau[i][j].RightChild = tableau[i + 1][j + 1];
                }
            }
            for (int i = 0; i < 52 - 7 - 6 - 5 - 4 - 3 - 2 - 1; ++i)
            {
                stock.Add(deck[i]);
            }
        }

        public class FlipMove : IMove
        {
            private Board board;
            private bool restoreTableauMove;

            public FlipMove(Board board)
            {
                this.board = board;
            }

            public void Apply()
            {
                restoreTableauMove = board.ApplyFlipMove();
            }

            public void Undo()
            {
                board.UndoFlipMove(restoreTableauMove);
            }

            public override string ToString()
            {
                return "Deal";
            }
        }

        public class WasteMove : IMove
        {
            private Board board;

            public WasteMove(Board board)
            {
                this.board = board;
            }

            public void Apply()
            {
                board.ApplyWasteMove();
            }

            public void Undo()
            {
                board.UndoWasteMove();
            }

            public override string ToString()
            {
                return $"Stock";
            }
        }

        public class WasteTableau: IMove
        {
            private Board board;
            private TableauPos pos;
            private Card card;
            private bool restoreTableauMove;

            public WasteTableau(Board board, TableauPos pos)
            {
                this.board = board;
                this.pos = pos;
                card = pos.Card;
            }

            public void Apply()
            {
                board.ApplyWasteMove();
                restoreTableauMove = board.ApplyTableauMove(pos);
            }

            public void Undo()
            {
                board.UndoTableauMove(pos, restoreTableauMove);
                board.UndoWasteMove();
            }

            public override string ToString()
            {
                return $"Stock + {card}";
            }
        }

        public class TableauMove : IMove
        {
            private Board board;
            private TableauPos pos;
            private Card card;
            private bool restoreTableauMove;

            public TableauMove(Board board, TableauPos pos)
            {
                this.board = board;
                this.pos = pos;
                card = pos.Card;
            }

            public void Apply()
            {
                restoreTableauMove = board.ApplyTableauMove(pos);
            }

            public void Undo()
            {
                board.UndoTableauMove(pos, restoreTableauMove);
            }

            public override string ToString()
            {
                return $"Move {card}";
            }
        }

        public class TableauTableauMove : IMove
        {
            private Board board;
            private TableauPos pos1;
            private TableauPos pos2;
            private Card card1;
            private Card card2;
            private bool restoreTableauMove;

            public TableauTableauMove(Board board, TableauPos pos1, TableauPos pos2)
            {
                this.board = board;
                this.pos1 = pos1;
                this.pos2 = pos2;
                card1 = pos1.Card;
                card2 = pos2.Card;
            }

            public void Apply()
            {
                restoreTableauMove = board.ApplyTableauMove(pos1);
                board.ApplyTableauMove(pos2);
            }

            public void Undo()
            {
                board.UndoTableauMove(pos2, restoreTableauMove);
                board.UndoTableauMove(pos1, restoreTableauMove);
            }

            public override string ToString()
            {
                return $"Move {card1} + {card2}";
            }
        }

        public void ApplyWasteMove()
        {
            foundation.Add(waste.Last());
            waste.RemoveAt(waste.Count - 1);
        }

        public void UndoWasteMove()
        {
            waste.Add(foundation.Last());
            foundation.RemoveAt(foundation.Count - 1);
        }

        public bool ApplyTableauMove(TableauPos pos)
        {
            tableauRank -= pos.Card.Rank;
            foundation.Add(pos.Card);
            pos.Card = null;
            bool answer = canPerformTableauMove;
            canPerformTableauMove = true;
            return answer;
        }

        public void UndoTableauMove(TableauPos pos, bool restoreTableauMove)
        {
            pos.Card = foundation.Last();
            foundation.RemoveAt(foundation.Count - 1);
            tableauRank += pos.Card.Rank;
            canPerformTableauMove = restoreTableauMove;
        }

        public bool ApplyFlipMove()
        {
            if (stock.Count != 0)
            {
                waste.Add(stock.Last());
                stock.RemoveAt(stock.Count - 1);
            }
            else
            {
                stock.AddRange(waste);
                stock.Reverse();
                waste.Clear();
            }
            bool answer = canPerformTableauMove;
            canPerformTableauMove = false;
            return answer;
        }

        public void UndoFlipMove(bool restoreTableauMove)
        {
            if (waste.Count != 0)
            {
                stock.Add(waste.Last());
                waste.RemoveAt(waste.Count - 1);
            }
            else
            {
                waste.AddRange(stock);
                waste.Reverse();
                stock.Clear();
            }
            canPerformTableauMove = restoreTableauMove;
        }

        public IList<IMove> GetMoves()
        {
            List<IMove> answer = new List<IMove>();
            Card stockTop = waste.LastOrDefault();
            foreach (TableauPos[] line in tableau)
            {
                foreach (TableauPos pos in line)
                {
                    if (pos.IsAvailable)
                    {
                        if (pos.Card.Rank == 13 && canPerformTableauMove)
                        {
                            answer.Clear();
                            answer.Add(new TableauMove(this, pos));
                            return answer;
                        }
                        if (null != stockTop && stockTop.Rank + pos.Card.Rank == 13)
                        {
                            answer.Add(new WasteTableau(this, pos));
                        }
                        if (canPerformTableauMove)
                        {
                            foreach (TableauPos[] line2 in tableau)
                            {
                                foreach (TableauPos pos2 in line2)
                                {
                                    if (pos.IsAvailable && pos2.IsAvailable && pos.Card.Rank + pos2.Card.Rank == 13 && pos.Card.Rank < pos2.Card.Rank)
                                    {
                                        answer.Add(new TableauTableauMove(this, pos, pos2));
                                    }
                                    else if (pos2.IsPairableWith(pos) && pos.Card.Rank + pos2.Card.Rank == 13)
                                    {
                                        answer.Add(new TableauTableauMove(this, pos, pos2));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (waste.Count != 0 && waste.Last().Rank == 13)
            {
                answer.Add(new WasteMove(this));
            }
            if (stock.Count + waste.Count != 0)
            {
                answer.Add(new FlipMove(this));
            }
            return answer;
        }

        public bool IsSolved()
        {
            return 0 == tableauRank;
        }

        public int Heuristic
        {
            get
            {
                int miniHeuristic = (tableauRank + 12) / 13;
                int[] counts = new int[14];
                foreach (TableauPos[] line in tableau)
                {
                    foreach (TableauPos pos in line)
                    {
                        if (null != pos.Card)
                        {
                            ++counts[pos.Card.Rank];
                        }
                    }
                }
                int answer = 0;
                for (int i = 1; i <= 6; ++i)
                {
                    int pairs = Math.Min(counts[i], counts[13 - i]);
                    answer += pairs;
                    counts[i] -= pairs;
                    counts[13 - i] -= pairs;
                }
                for (int i = 1; i <= 13; ++i)
                {
                    answer += counts[i];
                }
                if (answer < miniHeuristic)
                {
                    throw new InvalidOperationException("Heuristic mismatch");
                }
                return answer;
            }
        }

        public string CacheKey
        {
            get
            {
                string answer = "";
                foreach (Card card in stock)
                {
                    answer += card;
                }
                answer += "|";
                foreach (Card card in waste)
                {
                    answer += card;
                }
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
