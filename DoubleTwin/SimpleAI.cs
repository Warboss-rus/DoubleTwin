using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleTwin
{
	class SimpleAI : PlayerInterface
	{
		private List<sCard> cards;
		private bool iAmEnemy;

		public SimpleAI(List<sCard> cardList, bool enemy)
		{
			cards = cardList;
			iAmEnemy = enemy;
        }

		public sTableCard MakeMove(Table table)
		{
			for (int x = 0; x < 3; ++x)
			{
				for (int y = 0; y < 3; ++y)
				{
					if (table.PlaceIsFree(x, y))
					{
						sTableCard card = new sTableCard();
						card.card = cards[0];
						card.enemy = iAmEnemy;
						card.toBeFlipped = false;
						card.x = x;
						card.y = y;
						cards.RemoveAt(0);
						return card;
					}
				}
			}
			return new sTableCard();
		}

		public bool IsBot()
		{
			return true;
		}

		public List<sCard> GetCards()
		{
			return cards;
		}
	}
}
