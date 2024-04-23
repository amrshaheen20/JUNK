using System.ComponentModel;
using System.Diagnostics;

namespace Domino_Game.Lib.Core
{
    public enum eGamePlayers
    {
        [Description("1 to 1")]
        OneToOne = 0,
        [Description("1 to 2")]
        OneToTwo,
        [Description("1 to 3")]
        OneToThree
    }
    public enum eGameType
    {
        Regular = 0,
        AllFives = 1,//الأمريكي
    }
    public enum eDirection
    {
        auto,
        Left,
        Right,
        Top,
        Bottom
    }

    public class GameState : IDisposable
    {
        public List<Card> Cards { get; set; }

        public int MAX_POINTS { get; set; } = 100;

        protected void AddCards()
        {
            List<Card> allCards = new List<Card>();

            if (Players.Count != 3)
            {
                allCards.Add(new Card("0-0", "Double Zero", 0, 0)); // البلاطة
            }
            allCards.Add(new Card("0-1", "Zero One", 0, 1));
            allCards.Add(new Card("0-2", "Zero Two", 0, 2));
            allCards.Add(new Card("0-3", "Zero Three", 0, 3));
            allCards.Add(new Card("0-4", "Zero Four", 0, 4));
            allCards.Add(new Card("0-5", "Zero Five", 0, 5));
            allCards.Add(new Card("0-6", "Zero Six", 0, 6));
            allCards.Add(new Card("1-1", "One One", 1, 1)); // هابياك
            allCards.Add(new Card("1-2", "One Two", 1, 2));
            allCards.Add(new Card("1-3", "One Three", 1, 3));
            allCards.Add(new Card("1-4", "One Four", 1, 4));
            allCards.Add(new Card("1-5", "One Five", 1, 5));
            allCards.Add(new Card("1-6", "One Six", 1, 6));
            allCards.Add(new Card("2-2", "Two Two", 2, 2)); // دوبارة
            allCards.Add(new Card("2-3", "Two Three", 2, 3));
            allCards.Add(new Card("2-4", "Two Four", 2, 4));
            allCards.Add(new Card("2-5", "Two Five", 2, 5));
            allCards.Add(new Card("2-6", "Two Six", 2, 6));
            allCards.Add(new Card("3-3", "Three Three", 3, 3)); // دوسة
            allCards.Add(new Card("3-4", "Three Four", 3, 4));
            allCards.Add(new Card("3-5", "Three Five", 3, 5));
            allCards.Add(new Card("3-6", "Three Six", 3, 6));
            allCards.Add(new Card("4-4", "Four Four", 4, 4)); // دورجي
            allCards.Add(new Card("4-5", "Four Five", 4, 5));
            allCards.Add(new Card("4-6", "Four Six", 4, 6));
            allCards.Add(new Card("5-5", "Five Five", 5, 5)); // الدبش
            allCards.Add(new Card("5-6", "Five Six", 5, 6));
            allCards.Add(new Card("6-6", "Six Six", 6, 6)); // الدُش

            Random rng = new Random();
            allCards = allCards.OrderBy(card => rng.Next()).ToList();

            foreach (Card card in allCards)
            {
                Cards.Add(card);
            }
        }

        public List<Player> Players { get; set; }
        public Player CurrentPlayer { get; set; }
        public Board Bord { get; set; }
        public eGameType gameType { get; set; } = eGameType.Regular;
        public eAIMode GameMode { get; set; } = eAIMode.Normal;
        public eGamePlayers GamePlayers { get; set; } = eGamePlayers.OneToOne;
        protected Random Random { get; set; }
        private int _playerCount { get; set; } = 4;
        public int GameId { get; set; }
        public bool MustStartWithDouble { get; set; } = true;
        public bool IsFirstRound { get; set; } = true;
        public bool IsRoundOver
        {
            get
            {
                return Bord.GameOver() || ObjectDisposed;
            }
        }
        public bool IsGameOver
        {
            get
            {
                return TheWinner() != null || ObjectDisposed;
            }
        }
        public bool ObjectDisposed { get; private set; } = false;

        public GameState() => Init();
        public GameState(eGamePlayers gamePlayers, eGameType type, eAIMode mode)
        {
            GamePlayers = gamePlayers;
            gameType = type;
            GameMode = mode;
            Init();
        }

        private void Init()
        {
            Cards = new List<Card>();
            Players = new List<Player>();
            Bord = new Board();
            Bord.gamestate = this;
            Random = new Random();
            GameId = Random.Next(1000, 9999);
        }

        public void StartGame()
        {
            if (GamePlayers == eGamePlayers.OneToOne)
            {
                _playerCount = 2;
            }
            else if (GamePlayers == eGamePlayers.OneToTwo)
            {
                _playerCount = 3;
            }
            else
            {
                _playerCount = 4;
            }

            for (int i = Players.Count; i < _playerCount; i++)
            {
                Players.Add(new Player("Player " + (i + 1)));
                Players.Last().game = this;
            }

            HandOutCards();
        }

        public void AddPlayer(string name)
        {
            var player = new Player(name);
            player.game = this;
            player.IsHuman = true;

            if (Players.Count != 4)
                Players.Add(player);
        }

        private void HandOutCards()
        {
            Bord.Clear();
            Cards.Clear();
            AddCards();
            foreach (var player in Players)
            {
                Debug.WriteLine("Player: " + player.Name);
                player.Hand.Clear();

                int CardsCount = 7;
                if (Players.Count == 3)
                {
                    CardsCount = 9;
                }

                for (int i = 0; i < CardsCount; i++)
                {
                    var temp = Cards[Random.Next(Cards.Count)];
                    temp.Player = player;
                    temp.Player.IsTurn = false;
                    player.Hand.Add(temp);
                    Cards.Remove(temp);
                    Debug.WriteLine("Card: " + player.Hand.Last().Name);
                }
            }


            CurrentPlayer = Players[Random.Next(Players.Count)];

            var list = Players.Where(x => x.Hand.Any(y => y.IsDouble && y.Head == 6)).FirstOrDefault();
            if (list != null)
            {
                CurrentPlayer = list;
            }
            else
            {
                var list2 = Players.Where(x => x.Hand.Any(y => y.IsDouble)).ToList();
                if (list2.Count != 0)
                    CurrentPlayer = list2[Random.Next(list2.Count())];
            }


            CurrentPlayer.IsTurn = true;
            Debug.WriteLine("remaining cards: " + Cards.Count);
            Debug.WriteLine("CurrentPlayer: " + CurrentPlayer.Name);
        }


        public Player NextLevel()
        {
            var winner = WinnerPlayer();
            winner.Points += Players.Where(x => x != winner).Sum(x => x.PlayerPoints());

            HandOutCards();
            CurrentPlayer.IsTurn = false;
            CurrentPlayer = winner;
            CurrentPlayer.IsTurn = true;

            if (gameType == eGameType.AllFives)
            {
                while (!CurrentPlayer.Hand.Any(x => x.IsDouble))
                {
                    CurrentPlayer = GetNextPlayer();
                }
            }
            IsFirstRound = false;

            if (gameType != eGameType.AllFives)
                MustStartWithDouble = false;

            return winner;
        }



        public void Add(Card card, eDirection direction)
        {
            Debug.WriteLine("remaining cards: " + Cards.Count);
            if (direction == eDirection.auto)
            {
                direction = (eDirection)Random.Next(1, 3);

                Debug.WriteLine(direction);
            }

            if (direction == eDirection.Right)
            {
                Bord.AddToRight(card);
            }
            else if (direction == eDirection.Left)
            {
                Bord.AddToLeft(card);
            }
            else if (direction == eDirection.Top && gameType == eGameType.AllFives)
            {
                Bord.AddToTop(card);
            }
            else if (direction == eDirection.Bottom && gameType == eGameType.AllFives)
            {
                Bord.AddToBottom(card);
            }
            else
            {
                Bord.Add(card);
            }

            card.Player.Hand.Remove(card);
            card.Player.LastCardPlayed = card;
        }

        public void MoveToNextPlayer()
        {
            GetNextPlayer();

            if (!Bord.PlayerCanPlay(CurrentPlayer) && !IsRoundOver)
            {
                if (Cards.Count == 0)
                {
                    CurrentPlayer = GetNextPlayer(true);
                }
                else
                {
                    GetCardsFromOutSide();

                }
            }
            //just to make sure
            if (!Bord.PlayerCanPlay(CurrentPlayer) && !IsRoundOver)
            {
                CurrentPlayer = GetNextPlayer(true);
            }
        }

        public void GetCardsFromOutSide()
        {
            while (!Bord.PlayerCanPlay(CurrentPlayer) && Cards.Count != 0)
            {
                var temp = Cards.Last();
                temp.Player = CurrentPlayer;
                CurrentPlayer.Hand.Add(temp);
                Cards.Remove(temp);
            }
        }

        public Player GetNextPlayer(bool loop = false)
        {
            CurrentPlayer.IsTurn = false;
            var index = Players.IndexOf(CurrentPlayer) + 1;
            CurrentPlayer = Players[index % Players.Count];
            CurrentPlayer.IsTurn = true;

            if (!Bord.PlayerCanPlay(CurrentPlayer) && !IsRoundOver && loop)
            {
                CurrentPlayer = GetNextPlayer();
            }

            return CurrentPlayer;
        }


        public bool PlayerCanPlay()
        {
            return Bord.PlayerCanPlay(CurrentPlayer);
        }




        public Player WinnerPlayer()
        {
            var winner = Players.FirstOrDefault(p => p.IsWinner);
            if (winner == null)
            {
                winner = Players.MinBy(p => p.PlayerPoints());
            }

            return winner;
        }

        public Player TheWinner()
        {
            var player = Players.FirstOrDefault(x => x.Points >= MAX_POINTS);

            if (player != null)
            {
                return player;
            }

            return null;
        }

        public void HighlightLeft()
        {

        }

        public void Dispose()
        {
            ObjectDisposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!ObjectDisposed)
            {
                ObjectDisposed = true;
            }
        }

        ~GameState()
        {
            Dispose(false);
        }


        public bool IsNoCardsOutSide()
        {
            return Cards.Count == 0;
        }




    }
}
