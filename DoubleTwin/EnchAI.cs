using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleTwin
{
    class EnchAI : PlayerInterface
    {
        private List<sCard> cards;
        private bool iAmEnemy;

        private List<List<int>> cardsInfo = new List<List<int>>{
            new List<int> {22, 21, 18, 12, 9, 5, 2, 2, 1, 0}, //top
            new List<int> {21, 20, 18, 15, 7, 4, 3, 2, 1, 0}, //right
            new List<int> {23, 20, 16, 12, 7, 5, 4, 1, 1, 0}, //bottom
            new List<int> {21, 20, 16, 14, 8, 5, 3, 3, 1, 0}  //left
        };
        private int cardsNumber = 23;

        public EnchAI(List<sCard> cardList, bool enemy)
        {
            cards = cardList;
            iAmEnemy = enemy;
        }

        public sTableCard MakeMove(Table table)
        {
            sCard bestCard = new sCard();
            double bestScore = -1;
            int bestX = -1;
            int bestY = -1;

            for (int i = 0; i < cards.Count; i++)
            {
                for (int x = 0; x < 3; ++x)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        if (table.PlaceIsFree(x, y))
                        {
                            Sort(x, y);
                            double score = Calculate(i, x, y, table);
                            if (score > bestScore)
                            {
                                bestCard = cards[i];
                                bestX = x;
                                bestY = y;
                                bestScore = score;
                            }
                        }
                    }
                }
            }
            sTableCard card = new sTableCard();
            card.card = bestCard;
            card.enemy = iAmEnemy;
            card.toBeFlipped = false;
            card.x = bestX;
            card.y = bestY;
            cards.Remove(bestCard);
            return card;
        }

        public bool IsBot()
        {
            return true;
        }

        public List<sCard> GetCards()
        {
            return cards;
        }

        private double Calculate(int cardPos, int x, int y, Table table)
        {
            int count = 0;

            //check up
            if (y - 1 >= 0 && !table.PlaceIsFree(x, y - 1))
            {
                var upCard = table.GetTableCardAt(x, y - 1);
                if (upCard.enemy != iAmEnemy && upCard.card.bottom < cards[cardPos].top)
                {
                    count++;
                }
            }

            //check down
            if (y + 1 < 3 && !table.PlaceIsFree(x, y + 1))
            {
                var downCard = table.GetTableCardAt(x, y + 1);
                if (downCard.enemy != iAmEnemy && downCard.card.top < cards[cardPos].bottom)
                {
                    count++;
                }
            }

            //check left
            if (x - 1 >= 0 && !table.PlaceIsFree(x - 1, y))
            {
                var leftCard = table.GetTableCardAt(x - 1, y);
                if (leftCard.enemy != iAmEnemy && leftCard.card.right < cards[cardPos].left)
                {
                    count++;
                }
            }

            //check right
            if (x + 1 < 3 && !table.PlaceIsFree(x + 1, y))
            {
                var rightCard = table.GetTableCardAt(x + 1, y);
                if (rightCard.enemy != iAmEnemy && rightCard.card.left < cards[cardPos].right)
                {
                    count++;
                }
            }
            return (double)count / CalculateFreeCellsCoef(x, y, cards[cardPos], table);
        }

        private double CalculateFreeCellsCoef(int x, int y, sCard card, Table table)
        {
            double count = 1;

            //check up
            if (y - 1 >= 0 && table.PlaceIsFree(x, y - 1))
            {
                count += (double)cardsInfo[2][card.top - 1] / cardsNumber;
            }

            //check down
            if (y + 1 < 3 && table.PlaceIsFree(x, y + 1))
            {
                count += (double)cardsInfo[0][card.bottom - 1] / cardsNumber;
            }

            //check left
            if (x - 1 >= 0 && table.PlaceIsFree(x - 1, y))
            {
                count += (double)cardsInfo[1][card.left - 1] / cardsNumber;
            }

            //check right
            if (x + 1 < 3 && table.PlaceIsFree(x + 1, y))
            {
                count += (double)cardsInfo[3][card.right - 1] / cardsNumber;
            }
            return count;
        }

        private void Sort(int x, int y)
        {
            cards.Sort(Comparer<sCard>.Create((a, b) => CardWeight(x, y, a) >
                CardWeight(x, y, b) ? 1 : CardWeight(x, y, a) <
                CardWeight(x, y, b) ? -1 : 0));
        }

        private double CardWeight(int x, int y, sCard card)
        {
            int pow = 2;
            //if (x == 0 && y == 0) //left top
            //{
            //    return Math.Pow(card.bottom, pow) + Math.Pow(card.right, pow);
            //}
            //else if (x == 2 && y == 0) //right top
            //{
            //    return Math.Pow(card.bottom, pow) + Math.Pow(card.left, pow);
            //}
            //else if (x == 0 && y == 2) //left bottom
            //{
            //    return Math.Pow(card.top, pow) + Math.Pow(card.right, pow);
            //}
            //else if (x == 2 && y == 2) //right bottom
            //{
            //    return Math.Pow(card.top, pow) + Math.Pow(card.left, pow);
            //}
            //else if (y == 0) //top
            //{
            //    return Math.Pow(card.bottom, pow) + Math.Pow(card.left, pow) + Math.Pow(card.right, pow);
            //}
            //else if (x == 2) //right
            //{
            //    return Math.Pow(card.bottom, pow) + Math.Pow(card.left, pow) + Math.Pow(card.top, pow);
            //}
            //else if (y == 2) //bottom
            //{
            //    return Math.Pow(card.top, pow) + Math.Pow(card.left, pow) + Math.Pow(card.right, pow);
            //}
            //else if (x == 0) //left
            //{
            //    return Math.Pow(card.bottom, pow) + Math.Pow(card.top, pow) + Math.Pow(card.right, pow);
            //}
            //else //middle
            //{
                return Math.Pow(card.top, pow) + Math.Pow(card.bottom, pow) +
                    Math.Pow(card.left, pow) + Math.Pow(card.right, pow);
            //}
        }
    }
}
