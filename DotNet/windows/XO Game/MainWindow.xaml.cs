using System.Windows;
using XO_Game.lib;

namespace XO_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        GameState gameState;

        public MainWindow()
        {
            InitializeComponent();
            Cboardsize.ItemsSource = Enum.GetValues(typeof(BoardSize));
            Cboardsize.SelectedIndex = 0;
            Cgamedifficulty.ItemsSource = Enum.GetValues(typeof(GameType));
            Cgamedifficulty.SelectedIndex = 0;

# warning "Settings need to be implemented"
        }

        public async void NewGame()
        {
            if(gameState != null)
            {
                gameState.Dispose();
            }


            gameState = new GameState((BoardSize)Cboardsize.SelectedItem);
            gameState.Type = (GameType)Cgamedifficulty.SelectedItem;

            var player = (BoardData)new Random().Next(1, 3);

            if (PlayasX.IsChecked == true)
            {
                gameState.AddPlayer(BoardData.X, true);
            }
            else if (PlayasO.IsChecked == true)
            {
                gameState.AddPlayer(BoardData.O, true);
            }

            gameState.StartGame();
            gameState.CurrentPlayer = gameState.Players[0];
            Board.GameState = gameState;
            UpdateScorePanel();
            Rrfresh();


            await AIPlay();
        }

        private void Rrfresh()
        {
            Board.Rrfresh();
            SetPlayerturn();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }


        private void SetPlayerturn()
        {
            var player = gameState.CurrentPlayer;

            if (player.Value == BoardData.X)
            {
                PlayerXTrun.Visibility = Visibility.Visible;
                PlayerOTrun.Visibility = Visibility.Hidden;
            }
            else
            {
                PlayerXTrun.Visibility = Visibility.Hidden;
                PlayerOTrun.Visibility = Visibility.Visible;
            }
            GameState.Text = $"{player.Value} Turn";
        }


        private async void Board_Gameover(object sender, EventArgs e)
        {
            var winner = gameState.WinnerPlayer();

            if (winner != null)
            {
                GameState.Text = $"{winner.Value} Wins!";
            }
            else
            {
                GameState.Text = "Draw!";
            }

            await Task.Delay(1000);

            GameState.Text = "";



            gameState.NextRound();

            UpdateScorePanel();

            Rrfresh();

            await AIPlay();
        }

        private void UpdateScorePanel()
        {
            XScore.Text = gameState.Players.FirstOrDefault(p => p.Value == BoardData.X).Score.ToString();
            OScore.Text = gameState.Players.FirstOrDefault(p => p.Value == BoardData.O).Score.ToString();
        }

        private void Board_PlayerChanged(object sender, EventArgs e)
        {
            SetPlayerturn();
        }

        private async Task AIPlay()
        {
            if (!gameState.CurrentPlayer.IsHuman && !gameState.IsGameOver())
            {
                await Task.Delay(1000);
                gameState.CurrentPlayer.AiPlayer();
                Rrfresh();

                if (gameState.IsGameOver())
                {
                    Board_Gameover(null, null);
                }

                if (!gameState.CurrentPlayer.IsHuman)
                {
                    await AIPlay();
                }

            }
        }

        private async void Board_AiTurn(object sender, EventArgs e)
        {
            await AIPlay();
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
            SelectGameMenu.Visibility = Visibility.Collapsed;
            GameBoard.Visibility = Visibility.Visible;

        }

        private void btnShowSelectGameMenu_Click(object sender, RoutedEventArgs e)
        {
            SelectGameMenu.Visibility = Visibility.Visible;
            GameBoard.Visibility = Visibility.Collapsed;
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Collapsed;
            SelectGameMenu.Visibility = Visibility.Visible;
        }
    }
}