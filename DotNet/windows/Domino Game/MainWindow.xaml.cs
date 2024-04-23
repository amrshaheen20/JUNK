using Domino_Game.Lib.Core;
using Domino_Game.Lib.Core.Components;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Domino_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string PlayerName { get; set; }
        public eGameType Gametype { get; set; } = eGameType.Regular;
        public eGamePlayers GamePlayers { get; set; } = eGamePlayers.OneToOne;
        public eAIMode GameMode { get; set; } = eAIMode.Easy;
        public int MaxPoint { get; set; } = 100;

        private GameState gameState { get; set; }
        private bool CanStartNewGame = true;
        private bool IsGameEnded = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MainMenu.Visibility = Visibility.Visible;
            PlayerName = Settings.Default.PLayerName;
            Gametype = (eGameType)Settings.Default.Gametype;
            GamePlayers = (eGamePlayers)Settings.Default.GamePlayers;
            GameMode = (eAIMode)Settings.Default.GameMode;
            MaxPoint = Settings.Default.MaxPoint;
        }



        private async Task NewGame()
        {
            IsGameEnded = true;

            if (gameState != null)
            {   
                gameState.Dispose();
                gameState = null;
            }

            await WaitGameToEnd();

            gameState = new GameState(GamePlayers, Gametype, GameMode);
            gameState.MAX_POINTS = MaxPoint;
#if !TESTGAME
            gameState.AddPlayer(PlayerName);
#endif

            gameState.StartGame();

            Debug.WriteLine("Game Start");

            Board.board = gameState.Bord;
            Board.Clear();
            RefreshGame();

            PrepareGui();


            IsGameEnded = false;
            CanStartNewGame = false;
            await AIPlayer();
            CanStartNewGame = true;
        }

        private async Task WaitGameToEnd()
        {
            while (!CanStartNewGame)
            {
                await Task.Delay(1);
            }
        }

        private void PrepareGui()
        {
            GameOverMenu.Visibility = Visibility.Hidden;
            NextRound.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Hidden;
            SelectGame.Visibility = Visibility.Hidden;
        }


        Dictionary<Player, StackPanel> players;
        private void RefreshGame()
        {
            players = new Dictionary<Player, StackPanel>();

            var stack = new List<StackPanel>
            {
                Player1,
                Player2,
                Player3,
                Player4
            };

            foreach (var s in stack)
            {
                s.Visibility = Visibility.Collapsed;
            }

            #region Remaining cards

            Remaining_Cards.Visibility = Visibility.Collapsed;
            if (gameState.Cards.Count != 0)
            {
                Remaining_Cards.Visibility = Visibility.Visible;
                Remaining_Cards.Children.Clear();

                foreach (var card in gameState.Cards)
                {
                    var style = new Card.CardStyle();
                    style.Scale = 0.4;
#if !TESTGAME
                    style.HideCard = true;
#endif
                    var cardimg = card.GetImage(eRotation.Horizontal, style, false);
                    cardimg.Margin = new Thickness(2);
                    Remaining_Cards.Children.Add(cardimg);
                }
            }

            #endregion

            #region Players Info Card
            var Stackindex = -1;
            for (int i = 0; i < gameState.Players.Count; i++)
            {
                if (gameState.Players.Count < 4 && i == 1)
                {
                    ++Stackindex;
                }


                players.Add(gameState.Players[i], stack[++Stackindex]);
                stack[Stackindex].Children.Clear();
                stack[Stackindex].Visibility = Visibility.Visible;

                var player = players.ElementAt(i).Value;
                stack[Stackindex].Name = "Player" + i;

                var userpanel = new UserPanel();
                userpanel.PlayerName.Text = gameState.Players[i].Name;
                userpanel.PlayerScore.Text = $"{gameState.Players[i].Points}/{gameState.MAX_POINTS}";

                player.Children.Add(userpanel);

#if !TESTGAME
                if (!gameState.Players[i].IsHuman)
                {
                    var border = new Border();
                    border.CornerRadius = new CornerRadius(7);
                    border.Background = new SolidColorBrush(Colors.Orange);
                    border.Margin = new Thickness(5);
                    border.Width = 30;
                    border.Height = 50;
                    border.HorizontalAlignment = HorizontalAlignment.Center;
                    border.VerticalAlignment = VerticalAlignment.Center;
                    border.Child = new TextBlock()
                    {
                        Text = gameState.Players[i].Hand.Count.ToString(),
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    player.Children.Add(border);
                    continue;
                }

#endif
                #endregion

            #region Player Cards
                // player.Children.Add(new TextBlock() { Text = gameState.Players[i].Name + "\n score: " + gameState.Players[i].Points });
                foreach (var card in gameState.Players[i].Hand)
                {

#if !TESTGAME
                    card.Style.HideCard = !gameState.Players[i].IsHuman;
#endif

                    if (!gameState.Bord.CardCanSet(card) || gameState.CurrentPlayer != gameState.Players[i])
                    {
                        card.Style.IsEnabled = false;
#if !TESTGAME
                        if (!gameState.Players[i].IsHuman)
                        {
                            card.Style.IsEnabled = true;

                        }
#endif
                    }
                    else
                    {
                        card.Style.IsEnabled = true;
                    }


                    var cardimg = card.GetImage(player.Orientation != Orientation.Vertical ? eRotation.Vertical : eRotation.Horizontal, card.Style);

                    cardimg.Name = "Card";
                    if (!card.Style.IsEnabled)
                    {
                        cardimg.IsEnabled = false;
                    }

                    if (gameState.Players[i].IsHuman)
                    {
                        cardimg.MouseDown += (x, y) => OnCardClick(x, y);
                        cardimg.MouseMove += (x, y) =>
                        {
                            if (y.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                            {
                                Image newControl = new Image() { DataContext = cardimg.DataContext };
                                newControl.Tag = (player, card, cardimg);

                                newControl.Source = card.GetBitmapImage(eRotation.Vertical, new Card.CardStyle() { Scale = Board.Scale }, false);
                                DragDrop.DoDragDrop(newControl, new DataObject(DataFormats.Serializable, newControl), DragDropEffects.Move | DragDropEffects.Copy);

                            }
                        };


                    }
                    cardimg.Tag = (player, card);
                    cardimg.Margin = new Thickness(5);




                    player.Children.Add(cardimg);
                }
                #endregion
            }

        }

        private void Board_DragEnter(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is Image element)
            {
                var (Player, card, Soruce) = ((StackPanel, Card, Image))element.Tag;
                Soruce.Visibility = Visibility.Hidden;

                if (gameState.Bord.CardCanSetInLeft(card)) Board.HighlightLeftCard();

                if (gameState.Bord.CardCanSetInRight(card)) Board.HighlightRightCard();

                if (gameState.Bord.CardCanSetInTop(card)) Board.HighlightTopCard();

                if (gameState.Bord.CardCanSetInBottom(card)) Board.HighlightBottomCard();

            }
        }


        private void Board_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is Image element)
            {
                var (Player, card, Soruce) = ((StackPanel, Card, Image))element.Tag;

                if (!Board.canvas.Children.Contains(element))
                {
                    Board.canvas.Children.Add(element);
                }


                var droppoint = e.GetPosition(Board);
                Canvas.SetLeft(element, droppoint.X);
                Canvas.SetTop(element, droppoint.Y);


                Board.UnSelectedAllCards();

                if (Board.IsInPlace(element, Board.LastRightElement, eMove.Right))
                {
                    Board.SelectRightCard();
                }
                else if (Board.IsInPlace(element, Board.LastLeftElement, eMove.Left))
                {
                    Board.SelectLeftCard();
                }
                else if (Board.IsInPlace(element, Board.LastTopElement, eMove.Top))
                {
                    Board.SelectTopCard();
                }
                else if (Board.IsInPlace(element, Board.LastBottomElement, eMove.Bottom))
                {
                    Board.SelectBottomCard();
                }
            }
        }

        private void Board_DragLeave(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is FrameworkElement element)
            {
                var (Player, card, Soruce) = ((StackPanel, Card, Image))element.Tag;
                Board.canvas.Children.Remove(element);
                Soruce.Visibility = Visibility.Visible;
            }

            Board.RestoreAllCards();
        }

        private void Board_Drop(object sender, DragEventArgs e)
        {

            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is FrameworkElement element)
            {
                var (Player, card, Soruce) = ((StackPanel, Card, Image))element.Tag;


                if (Board.IsInPlace(element, Board.LastRightElement, eMove.Right))
                {
                    AddCard(card, eDirection.Right);
                }
                else if (Board.IsInPlace(element, Board.LastLeftElement, eMove.Left))
                {
                    AddCard(card, eDirection.Left);
                }
                else if (Board.IsInPlace(element, Board.LastTopElement, eMove.Top))
                {
                    AddCard(card, eDirection.Top);
                }
                else if (Board.IsInPlace(element, Board.LastBottomElement, eMove.Bottom))
                {
                    AddCard(card, eDirection.Bottom);
                }
                else
                {
                    AddCard(card, eDirection.auto);
                }
            }
        }

        private void OnCardClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (e.ClickCount != 2)
            {
                return;
            }

            var item = sender as FrameworkElement;

            var (Player, card) = ((StackPanel, Card))item.Tag;


            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                AddCard(card, eDirection.Left);
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                AddCard(card, eDirection.Right);
            }
            else
            {
                return;
            }

            Player.Children.Remove(item);
        }


        async void AddCard(Card card, eDirection direction)
        {
            //Todo: async make some problem here
            if (IsGameEnded)
            {
                return;
            }
            new SoundPlayer(Application.GetResourceStream(new Uri("pack://application:,,,/Lib/assets/Audio/DominoSetDown.wav")).Stream).Play();
            gameState.Add(card, direction);
            await MoveToNextPlayer();
            if (!IsGameEnded)
            {
                Board.Refresh();
                RefreshGame();
                await ShowNextRoundScreen();
                if (!IsGameEnded && !IsGameOver())
                {
                    await AIPlayer();
                }
            }
            //Debug.WriteLine("Game id: " + gameState.GameId + "\n" +
            //    "Next Player: " + gameState.CurrentPlayer.Name);
        }

        private bool IsGameOver()
        {
            if (gameState.IsGameOver)
            {
                GameOverMenu.Visibility = Visibility.Visible;
                WinnerPlayer.Text = string.Format("Player \"{0}\" win the game and his soure is: {1}", gameState.TheWinner().Name, gameState.TheWinner().Points);
                return true;
            }
            return false;
        }

        private async Task ShowNextRoundScreen()
        {
            if (gameState.IsRoundOver)
            {
                RoundPlayersCards.Children.Clear();
                foreach (var player in gameState.Players)
                {

                    if (player.IsWinner || player.Hand.Count == 0)
                    {
                        continue;
                    }

                    var stackpanel = new StackPanel();
                    stackpanel.Orientation = Orientation.Vertical;
                    stackpanel.HorizontalAlignment = HorizontalAlignment.Center;
                    stackpanel.VerticalAlignment = VerticalAlignment.Center;
                    stackpanel.Margin = new Thickness(2);

                    stackpanel.Children.Add(new TextBlock()
                    {
                        Text = player.Name,
                        FontSize = 20,
                        Foreground = new SolidColorBrush(Colors.White),
                        Padding = new Thickness(10)
                    });

                    var stackpanel2 = new StackPanel();
                    stackpanel2.Orientation = Orientation.Horizontal;
                    stackpanel2.HorizontalAlignment = HorizontalAlignment.Center;
                    stackpanel2.VerticalAlignment = VerticalAlignment.Center;
                    stackpanel2.Margin = new Thickness(2);
                    stackpanel.Children.Add(stackpanel2);


                    foreach (var card in player.Hand)
                    {
                        card.Style = new Domino_Game.Lib.Core.Card.CardStyle();
                        card.Style.Scale = 0.5;
                        var cardimg = card.GetImage(eRotation.Vertical, card.Style);
                        cardimg.Margin = new Thickness(2);
                        stackpanel2.Children.Add(cardimg);
                    }

                    RoundPlayersCards.Children.Add(stackpanel);
                }



                var winner = gameState.NextLevel();

                if (gameState.IsGameOver|| IsGameEnded)
                {
                    return;
                }

                await Task.Delay(1000);
                NextRound.Visibility = Visibility.Visible;
                RoundWinnerPlayer.Text = string.Format("Player \"{0}\" win the round and his soure is: {1}", winner.Name, winner.Points);

                await Task.Delay(4000);
                NextRound.Visibility = Visibility.Hidden;


                RefreshGame();
                Board.Clear();
            }

        }



        private async Task AIPlayer()
        {
            if (IsGameEnded || gameState.CurrentPlayer.IsHuman)
            {
                return;
            }
            await Task.Delay(1000);
            if (!IsGameEnded)
            {
                eDirection direction = eDirection.auto;
                Card aicard = gameState.CurrentPlayer.AIPlayer(ref direction);
                if (aicard != null)
                {
                    AddCard(aicard, direction);
                }
            }

        }


        public async Task MoveToNextPlayer()
        {
            gameState.GetNextPlayer();
            do
            {
                if (!gameState.PlayerCanPlay() && !gameState.IsRoundOver)
                {
                    Debug.WriteLine("Next Player: " + gameState.CurrentPlayer.Name);
                    Debug.WriteLine("Player Can play: " + gameState.PlayerCanPlay());
                    if (gameState.IsNoCardsOutSide())
                    {
                        Board.Refresh();
                        RefreshGame();
                        players[gameState.CurrentPlayer].Children.Add(new TextBlock
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Text = "PASS",
                            FontSize = 30.0,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(Colors.Yellow)
                        });
                        await Task.Delay(2000);
                        gameState.GetNextPlayer();
                    }
                    else
                    {
                        gameState.GetCardsFromOutSide();
                        Board.Refresh();
                        RefreshGame();
                    }
                }
            }
            while (!gameState.PlayerCanPlay() && !gameState.IsRoundOver);
        }

        private void mainMenubtn_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            SelectGame.Visibility = Visibility.Visible;
        }

        private async void StartGame_Click(object sender, RoutedEventArgs e)
        {
            //stupid way i know ;)
            if (string.IsNullOrEmpty(PlayerName))
            {
                MessageBox.Show("Please enter your name");
                return;
            }

            //GameType
            Gametype = ((ComboBoxItem)GameType.SelectedItem).Content.ToString() switch
            {
                "Regular" => eGameType.Regular,
                _ => throw new Exception("Unknown game type")
            };

            //PlayersCount
            GamePlayers = ((ComboBoxItem)PlayersCount.SelectedItem).Content.ToString() switch
            {
                "1 To 1" => eGamePlayers.OneToOne,
                "1 To 2" => eGamePlayers.OneToTwo,
                "1 To 3" => eGamePlayers.OneToThree,
                _ => throw new Exception("Unknown players count")
            };

            //GameDifficulty
            GameMode = ((ComboBoxItem)GameDifficulty.SelectedItem).Content.ToString() switch
            {
                "Easy" => eAIMode.Easy,
                "Normal" => eAIMode.Normal,
                "Hard" => eAIMode.Hard,
                _ => throw new Exception("Unknown difficulty value")
            };


            Settings.Default.PLayerName = PlayerName;
            Settings.Default.Gametype = (int)Gametype;
            Settings.Default.GamePlayers = (int)GamePlayers;
            Settings.Default.GameMode = (int)GameMode;

            Settings.Default.MaxPoint = MaxPoint;
            Settings.Default.Save();

            await NewGame();
        }

        private async void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            IsGameEnded = true;
            await WaitGameToEnd();
            SelectGame.Visibility = Visibility.Visible;
        }
    }
}