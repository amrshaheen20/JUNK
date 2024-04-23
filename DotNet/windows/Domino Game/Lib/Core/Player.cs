using System.Diagnostics;

namespace Domino_Game.Lib.Core
{

    public enum eAIMode
    {
        Easy,
        Normal,
        Hard
    }

    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; }
        public Card LastCardPlayed { get; set; }
        public GameState game { get; set; }
        public int Points { get; set; }
        public bool IsTurn { get; set; } = false;

        public bool IsWinner => Hand.Count == 0;


        public bool IsHuman { get; set; }

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            Points = 0;
        }



        public bool PlayerHaveCard(Card card)
        {
            return Hand.Contains(card);
        }

        public bool PlayerHasHead(int head)
        {
            return Hand.Any(c => c.Head == head);
        }

        public bool PlayerHasTail(int tail)
        {
            return Hand.Any(c => c.Tail == tail);
        }

        public bool PlayerHasFourDouble()
        {
            return Hand.Count(c => c.IsDouble) >= 4;
        }

        public int PlayerPoints()
        {
            return Hand.Sum(c => c.Value);
        }


        public Card AIPlayer(ref eDirection direction)
        {

            if (game.ObjectDisposed)
            {
                return null;
            }

            if (Hand.Count == 0 || game.IsRoundOver)
            {
                return null;
            }

            if (game.GameMode == eAIMode.Easy)
            {
                return AIEasyModePlayer(ref direction);
            }
            else if (game.GameMode == eAIMode.Normal)
            {
                return AINormalModePlayer(ref direction);
            }
            else if (game.GameMode == eAIMode.Hard)
            {
                return AIHardModePlayer(ref direction);
            }


            return null;
        }

  
        protected Card AIEasyModePlayer(ref eDirection direction)
        {
            direction = eDirection.auto;

            foreach (var card in Hand)
            {
                if (game.Bord.CardCanSet(card))
                {
                    return card;
                }
            }

            return null;
        }


        protected Card AINormalModePlayer(ref eDirection direction)
        {
            var AvailableCardsThatCanPlayed = new List<Card>();
            int Doubles = 0;

            foreach (var card in Hand)
            {
                if (game.Bord.CardCanSet(card))
                {
                    AvailableCardsThatCanPlayed.Add(card);

                    if (card.IsDouble)
                    {
                        Doubles++;
                        int temp = game.Bord.GetCardCount(card.Head);

                        if (temp >= 3 && temp < 5)
                        {
                            card.Priority = 2;
                        }
                        else if (temp >= 5)
                        {
                            card.Priority = 3;
                        }
                    }

                }

                Debug.WriteLine($"Card {card} can be played: {game.Bord.CardCanSet(card)}");
                Debug.WriteLine($"Card {card} is double: {card.IsDouble}");
                Debug.WriteLine($"Card {card} priority: {card.Priority}");
                Debug.Write($"{AvailableCardsThatCanPlayed.Count} Available Cards That Can Played: ");
                foreach (var c in AvailableCardsThatCanPlayed)
                {
                    Debug.Write(c + " ");
                }
                Debug.WriteLine("");

            }

            if (AvailableCardsThatCanPlayed.Count == 1)
            {
                direction = eDirection.auto;
                return AvailableCardsThatCanPlayed[0];
            }
            else if (Doubles > 0)
            {
                var MaxPriorityCard = AvailableCardsThatCanPlayed.Where(c => c.IsDouble).Max(x => x.Priority);
                direction = eDirection.auto;
                return AvailableCardsThatCanPlayed[0];
            }

            return AIEasyModePlayer(ref direction);
        }


        protected Card AIHardModePlayer(ref eDirection direction)
        {

            var validCards = new List<Card>();
            foreach (var card in Hand)
            {
                if (game.Bord.CardCanSet(card))
                {
                    validCards.Add(card);
                }
            }

            if (validCards.Count == 0)
                return null;


            Card maxCard = validCards[0];

            foreach (var card in validCards)
            {
                if (card.Value > maxCard.Value || card.Tail == 0)
                    maxCard = card;
            }


            if (game.Bord.CardCanSetInLeft(maxCard) && game.Bord.CardCanSetInLeft(maxCard) &&
                game.Bord.CardCanSetInTop(maxCard) && game.Bord.CardCanSetInBottom(maxCard)
                && game.gameType == eGameType.AllFives)
            {
                var random = new Random();
                var randomDirection = random.Next(1, 5);//avoid 0 auto (1=>4)

                direction = (eDirection)randomDirection;

                return maxCard;
            }
            else if (game.Bord.CardCanSetInLeft(maxCard) && game.Bord.CardCanSetInLeft(maxCard))
            {

                var random = new Random();
                var randomDirection = random.Next(1, 3);//avoid 0 auto (1=>2)

                direction = (eDirection)randomDirection;
                return maxCard;
            }
            else
            {
                direction = eDirection.auto;  //random
                return maxCard;
            }
        }

    }

}
