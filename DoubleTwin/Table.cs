using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleTwin
{
	public struct sTableCard
	{
		public sCard card;
		public bool enemy;
		public int x;
		public int y;
		public bool toBeFlipped;
		public sTableCard(sCard card, bool enemy, int x, int y)
		{
			this.card = card;
			this.enemy = enemy;
			this.x = x;
			this.y = y;
			this.toBeFlipped = false;
		}
    }

	class Table
	{
		private List<sTableCard> cards;

		public Table()
		{
			cards = new List<sTableCard>();
		}
		public void AddCard(sTableCard tcard)
		{
			ChangeOwnerNearCard(tcard);
            cards.Add(tcard);
		}

		public void AddCard(sCard card, int x, int y, bool enemy)
		{
			sTableCard tcard = new sTableCard(card, enemy, x, y);
			ChangeOwnerNearCard(tcard);
			cards.Add(tcard);
		}

		private void ChangeOwnerNearCard(sTableCard tCard)
		{
			for (int i = 0; i < cards.Count; ++i)
			{
				int deltaX = Math.Abs(cards[i].x - tCard.x);
				int deltaY = Math.Abs(cards[i].y - tCard.y);
				if (cards[i].enemy != tCard.enemy && ((deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1)))
				{
					if ((cards[i].x > tCard.x && cards[i].card.left < tCard.card.right)
						|| (cards[i].x < tCard.x && cards[i].card.right < tCard.card.left)
						|| (cards[i].y > tCard.y && cards[i].card.top < tCard.card.bottom)
						|| (cards[i].y < tCard.y && cards[i].card.bottom < tCard.card.top))
					{
						sTableCard tc = cards[i];
						tc.toBeFlipped = true;
						cards[i] = tc;
					}
				}
			}
		}

		public sTableCard GetTableCardAt(int x, int y)
		{
			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].x == x && cards[i].y == y)
				{
					return cards[i];
				}
			}
			return new sTableCard();
		}

		public bool PlaceIsFree(int x, int y)
		{
			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].x == x && cards[i].y == y)
				{
					return false;
				}
			}
			return true;
        }

		public void FlipCardsMarkedToBeFlipped()
		{
			for (int i = 0; i < cards.Count; ++i)
			{
				sTableCard tcard = cards[i];
				if (tcard.toBeFlipped)
				{
					tcard.enemy = !tcard.enemy;
					cards[i] = tcard;
				}
			}
		}

		public void EndFlippingAnimation()
		{
			for (int i = 0; i < cards.Count; ++i)
			{
				sTableCard tcard = cards[i];
				if (tcard.toBeFlipped)
				{
					tcard.toBeFlipped = false;
					cards[i] = tcard;
				}
			}
		}

		public int GetNumberOfCardsPlayerOwns(bool enemy)
		{
			int number = 0;
			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].enemy == enemy)
					number++;
			}
			return number;
		}

		public bool CardsNeedToBeFlipped()
		{
			for (int i = 0; i < cards.Count; ++i)
			{
				if (cards[i].toBeFlipped)
					return true;
			}
			return false;
		}

        public int GetNumberOfCards()
        {
            return cards.Count;
        }
	}
}
