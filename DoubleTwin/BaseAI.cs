using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleTwin
{
    class BaseAI : PlayerInterface
    {
        private List<sCard> cards;
		private bool iAmEnemy;

        public BaseAI(List<sCard> cardList, bool enemy)
		{
			cards = cardList;
			iAmEnemy = enemy;
            cards.Sort(Comparer<sCard>.Create((a, b) => (a.bottom + a.top + a.left + a.right) >
                (b.bottom + b.top + b.left + b.right) ? 1 : (a.bottom + a.top + a.left + a.right) <
                (b.bottom + b.top + b.left + b.right) ? -1 : 0));
        }

		public sTableCard MakeMove(Table table)
		{
            int bestCardPos = -1;
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
                            double score = Calculate(i, x, y, table);
                            if (score > bestScore)
                            {
                                bestCardPos = i;
                                bestX = x;
                                bestY = y;
                                bestScore = score;
                            }
                        }
                    }
                }
            }
            sTableCard card = new sTableCard();
            card.card = cards[bestCardPos];
            card.enemy = iAmEnemy;
            card.toBeFlipped = false;
            card.x = bestX;
            card.y = bestY;
            cards.RemoveAt(bestCardPos);
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
            return (double)count / CalculateFreeCells(x, y, table);
        }

        private int CalculateFreeCells(int x, int y, Table table)
        {
            int count = 1;

            //check up
            if (y - 1 >= 0 && table.PlaceIsFree(x, y - 1))
            {
                count++;
            }

            //check down
            if (y + 1 < 3 && table.PlaceIsFree(x, y + 1))
            {
                count++;
            }

            //check left
            if (x - 1 >= 0 && table.PlaceIsFree(x - 1, y))
            {
                count++;
            }

            //check right
            if (x + 1 < 3 && table.PlaceIsFree(x + 1, y))
            {
                count++;
            }
            return count;
        }
	}
}
