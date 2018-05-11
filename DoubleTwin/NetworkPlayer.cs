using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace DoubleTwin
{
    class NetworkPlayer : PlayerInterface
    {
        private List<sCard> cards;
        TcpClient client;
        int cardsLeft;

        public NetworkPlayer(List<sCard> cardList, TcpClient client)
        {
            cards = cardList;
            this.client = client;
            cardsLeft = 5;
        }

        ~NetworkPlayer()
        {
            client.Close();
        }

        public sTableCard MakeMove(Table table)
        {
            sTableCard card = new sTableCard();
            if(!client.Connected)
            {
                return card;
            }
            NetworkStream stream = client.GetStream();
            byte[] inStream = new byte[13];
            stream.Read(inStream, 0, 13);
            int index = Convert.ToInt32(inStream[0]);
            card.card = cards[index];
            card.x = Convert.ToInt32(inStream[4]);
            card.y = Convert.ToInt32(inStream[8]);
            card.enemy = !Convert.ToBoolean(inStream[12]);
            cardsLeft--;
            return card;
        }

        public bool IsBot()
        {
            return true;
        }

        public List<sCard> GetCards()
        {
            List<sCard> emptyCards = new List<sCard>();
            for (int i = 0; i < cardsLeft; ++i )
            {
                emptyCards.Add(new sCard());
            }
            return emptyCards;
        }
    }
}
