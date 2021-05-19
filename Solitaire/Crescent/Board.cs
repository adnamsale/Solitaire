using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Crescent
{
    class Board
    {
        private List<Card>[] foundationAsc;
        private List<Card>[] foundationDesc;
        private List<Card>[] tableau;
        private int shufflesRemaining = 9;
 
        public Board(IList<Card> deck)
        {
            foundationAsc = new List<Card>[4];
            foundationDesc = new List<Card>[4];
            tableau = new List<Card>[16];
            for (int i = 0; i < 4; ++i)
            {
                foundationAsc[i] = new List<Card>();
                foundationDesc[i] = new List<Card>();
            }
            for (int i = 0; i < 16; ++i)
            {
                tableau[i] = new List<Card>();
            }
            int tableauCount = 0;
            for (int i = 0; i < deck.Count; ++i)
            {
                Card c = deck[i];
                if (c.Rank == 1 && foundationAsc[c.Suit.Ordinal].Count == 0)
                {
                    foundationAsc[c.Suit.Ordinal].Add(c);
                }
                else if (c.Rank == 13 && foundationDesc[c.Suit.Ordinal].Count == 0)
                {
                    foundationDesc[c.Suit.Ordinal].Add(c);
                }
                else
                {
                    tableau[15 - (tableauCount++ / 6)].Insert(0, c);
                }
            }
            TableauMovesRemaining = 4;
        }

        public int Heuristic
        {
            get
            {
                int answer = 0;
                for (int i = 0; i < 4; ++i)
                {
                    answer += (13 - foundationAsc[i].Count) + (13 - foundationDesc[i].Count);
                }
                return answer;
            }
        }

        public int TableauMovesRemaining
        {
            get;
            set;
        }

        public class MoveMove : IMove
        {
            private List<Card> from;
            private List<Card> to;
            private string desc;
            private Board board;

            public MoveMove(List<Card> from, List<Card> to, string desc, Board board = null)
            {
                this.from = from;
                this.to = to;
                this.desc = desc;
                this.board = board;
            }

            public void Apply()
            {
                Card card = from[from.Count - 1];
                from.RemoveAt(from.Count - 1);
                to.Add(card);
                if (null != board)
                {
                    board.TableauMovesRemaining--;
                }
            }

            public void Undo()
            {
                Card card = to[to.Count - 1];
                to.RemoveAt(to.Count - 1);
                from.Add(card);
                if (null != board)
                {
                    board.TableauMovesRemaining++;
                }
            }

            public override string ToString()
            {
                return desc;
            }
        }

        public class ShuffleMove : IMove
        {
            private Board board;

            public ShuffleMove(Board board)
            {
                this.board = board;
            }

            public void Apply()
            {
                board.Shuffle();
            }

            public void Undo()
            {
                board.UndoShuffle();
            }

            public override string ToString()
            {
                return "Shuffle";
            }
        }

        public IList<IMove> GetMoves()
        {
            List<IMove> answer = new List<IMove>();
            AddAscMoves(answer, tableau, "T", true);
            AddDescMoves(answer, tableau, "T", true);
            if (answer.Count != 0)
            {
                if (1 < answer.Count)
                {
                    answer.RemoveRange(1, answer.Count - 1);
                }
                return answer;
            }
            if (0 != TableauMovesRemaining)
            {
                AddAscMoves(answer, foundationDesc, "D", false);
                AddDescMoves(answer, foundationAsc, "A", false);
                for (int i = 0; i < 16; ++i)
                {
                    AddTableauMoves(answer, tableau[i], tableau, $"T[{i}]", "T", true);
                    //AddTableauMoves(answer, tableau[i], foundationDesc, $"T[{i}]", "D", false);
                    //AddTableauMoves(answer, tableau[i], foundationAsc, $"T[{i}]", "A", false);
                }
            }
            if (0 != shufflesRemaining)
            {
                answer.Add(new ShuffleMove(this));
            }
            return answer;
        }

        private void AddAscMoves(List<IMove> answer, List<Card>[] from, string fromName, bool canLeaveEmpty)
        {
            for (int i = 0; i < from.Length; ++i)
            {
                if (from[i].Count != 0)
                {
                    Card card = from[i].Last();
                    if (foundationAsc[card.Suit.Ordinal].Count == card.Rank - 1)
                    {
                        if (1 < from[i].Count || canLeaveEmpty)
                        {
                            answer.Add(new MoveMove(from[i], foundationAsc[card.Suit.Ordinal], $"Move from {fromName}[{i}] to A"));
                        }
                    }
                }
            }
        }

        private void AddDescMoves(List<IMove> answer, List<Card>[] from, string fromName, bool canLeaveEmpty)
        {
            for (int i = 0; i < from.Length; ++i)
            {
                if (from[i].Count != 0)
                {
                    Card card = from[i].Last();
                    if (foundationDesc[card.Suit.Ordinal].Last().Rank == card.Rank + 1)
                    {
                        if (1 < from[i].Count || canLeaveEmpty)
                        {
                            answer.Add(new MoveMove(from[i], foundationDesc[card.Suit.Ordinal], $"Move from {fromName}[{i}] to D"));
                        }
                    }
                }
            }
        }

        private void AddTableauMoves(List<IMove> answer, List<Card> to, List<Card>[] from, string toName, string fromName, bool canLeaveEmpty)
        {
            if (to.Count == 0)
            {
                return;
            }
            Card cardTo = to.Last();
            for (int i = 0; i < from.Length; ++i)
            {
                if (from[i].Count != 0)
                {
                    Card cardFrom = from[i].Last();
                    int diff = Math.Abs(cardTo.Rank - cardFrom.Rank);
                    if (cardTo.Suit == cardFrom.Suit && (diff == 12 || diff == 1))
                    {
                        if (1 < from[i].Count || canLeaveEmpty)
                        {
                            answer.Add(new MoveMove(from[i], to, $"Move from {fromName}[{i}] to {toName}", this));
                        }
                    }
                }
            }
        }

        private void Shuffle()
        {
            foreach (List<Card> t in tableau)
            {
                if (1 < t.Count)
                {
                    Card card = t.First();
                    t.RemoveAt(0);
                    t.Add(card);
                }
            }
            --shufflesRemaining;
        }

        private void UndoShuffle()
        {
            foreach (List<Card> t in tableau)
            {
                if (1 < t.Count)
                {
                    Card card = t.Last();
                    t.RemoveAt(t.Count - 1);
                    t.Insert(0, card);
                }
            }
            ++shufflesRemaining;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (foundationAsc[i].Count != 13 || foundationDesc[i].Count != 13)
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
                string answer = "";
                for (int i = 0; i < 4; ++i)
                {
                    answer += foundationAsc[i].Count.ToString("D2") + foundationDesc[i].Count.ToString("D2");
                }
                for (int i = 0; i < 16; ++i)
                {
                    foreach(Card c in tableau[i])
                    {
                        answer += c;
                    }
                    answer += "|";
                }
                return answer;
            }
        }

        public void Dump()
        {
            Dump("A", foundationAsc);
            Dump("D", foundationDesc);
            Dump("T", tableau);
        }

        private void Dump(string caption, List<Card>[] lists)
        {
            Console.Write(caption);
            Console.Write(": ");
            foreach(List<Card> list in lists)
            {
                if (list.Count == 0)
                {
                    Console.Write("  ");
                }
                else
                {
                    Console.Write(list.Last());
                }
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}
