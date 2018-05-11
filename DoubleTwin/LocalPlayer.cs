using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace DoubleTwin
{
    class LocalPlayer : PlayerInterface
    {
        private List<sCard> cards;

        public LocalPlayer(List<sCard> cardList)
        {
            cards = cardList;
        }
        
        public sTableCard MakeMove(Table table) 
        {
            while (!moveHasBeenMade) Thread.Sleep(10);
            sTableCard card = new sTableCard();
            card.card = cards[moveCardIndex];
            card.enemy = false;
            card.toBeFlipped = false;
            card.x = moveX;
            card.y = moveY;
			moveHasBeenMade = false;
            cards.RemoveAt(moveCardIndex);
            return card; 
        }

        public bool IsBot() 
        { 
            return false; 
        }

        private bool moveHasBeenMade;
        int moveCardIndex;
        int moveX;
        int moveY;

        public void PlayerMove(int cardIndex, int x, int y)
        {
			if (cardIndex >= cards.Count) return;
			moveHasBeenMade = true;
            moveCardIndex = cardIndex;
            moveX = x;
            moveY = y;
        }
		public List<sCard> GetCards()
		{
			return cards;
		}
	}
}
