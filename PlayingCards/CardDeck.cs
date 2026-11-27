using StardewValley;
using System;
using System.Collections.Generic;

namespace PlayingCards
{
    public class CardDeck
    {
        public List<PlayingCard> cards = new List<PlayingCard>();
        public CardDeck(string deckString)
        {
            string[] carda = deckString.Split(',');
            foreach (string cardString in carda)
            {
                cards.Add(new PlayingCard(cardString));
            }
        }
        public CardDeck()
        {
            Guid guid = new Guid();
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 13; j++)
                {
                    cards.Add(new PlayingCard()
                    {
                        index = j,
                        suit = i,
                        facing = "down",
                        deckID = guid.ToString()
                    });
                }
            }
        }
        public string GetDeckString()
        {

            List<string> cardList = new List<string>();
            foreach (PlayingCard card in cards)
            {
                cardList.Add(card.GetString());
            }
            return string.Join(",", cardList);
        }
        public void ShuffleCards()
        {
            int n = cards.Count;
            while (n >= 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }
    }

    public class PlayingCard
    {
        public int index;
        public int suit;
        public string facing;
        public string deckID;

        public PlayingCard()
        {
        }

        public PlayingCard(string cardString)
        {
            string[] card = cardString.Split(':');
            index = int.Parse(card[0]);
            suit = int.Parse(card[1]);
            facing = card[2];
            deckID = card[3];
        }

        public string GetString()
        {
            return index + ":" + suit + ":" + facing + ":" + deckID;
        }
    }
}