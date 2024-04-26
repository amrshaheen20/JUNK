namespace XO_Game.lib
{
    public enum BoardData
    {
        None = 0,
        X,
        O
    }

    public enum GameType
    {
        Easy = 0,
        Medium,
        Impossible,
        TwoPlayers
    }


    public enum BoardSize
    {
        Small = 0,       //3x3
        Medium,          //5x5
        Large,           //7x7
        //Ultimate       //9x9  no plan to implement
    }

    public struct Move
    {
        public int Startindex { get; set; }
        public int Endindex { get; set; }

        public Move()
        {
            Startindex = -1;
            Endindex = -1;
        }

        public Move(int row, int column)
        {
            Startindex = row;
            Endindex = column;
        }


        public bool IsEmpty()
        {
            return Startindex == -1 && Endindex == -1;
        }
    }


    public class GameState: IDisposable
    {
        public BoardData[] board { get; set; }
        public Player CurrentPlayer { get; set; }
        public GameType Type { get; set; } = GameType.Impossible;
        public BoardSize boardSize { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public Random Random { get; set; } = new Random();
        public List<Player> Players { get; set; }

        public bool IsObjectDisposed { get; set; } = false;

        public int MaxVerifyRowCount
        {
            get
            {
                if (boardSize != BoardSize.Small)
                {
                    return 4;
                }
                else
                {
                    return RowCount;
                }
            }
        }

        public int MaxVerifyColumnCount
        {
            get
            {
                if (boardSize != BoardSize.Small)
                {
                    return 4;
                }
                else
                {
                    return ColumnCount;
                }
            }
        }


        public GameState(BoardSize size)
        {
            switch (size)
            {
                case BoardSize.Small:
                    RowCount = 3;
                    ColumnCount = 3;
                    break;
                case BoardSize.Medium:
                    RowCount = 5;
                    ColumnCount = 5;
                    break;
                case BoardSize.Large:
                    RowCount = 7;
                    ColumnCount = 7;
                    break;
            }

            boardSize = size;
            board = new BoardData[RowCount * ColumnCount];
            Players = new List<Player>();
        }

        public GameState(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            board = new BoardData[RowCount * ColumnCount];
            Players = new List<Player>();
        }

        public void AddPlayer(BoardData PlayerValue, bool IsHuman)
        {
            var player = new Player(PlayerValue, IsHuman, this);
            Players.Add(player);
        }

        private BoardData GetSecondPlayer(BoardData player)
        {
            if (player == BoardData.X)
            {
                return BoardData.O;
            }
            else
            {
                return BoardData.X;
            }
        }

        public void StartGame()
        {
            board = new BoardData[RowCount * ColumnCount];

            if (Players.Count == 0)
            {
                Players.Add(new Player((BoardData)Random.Next(1, 3), true, this));
                Players.Add(new Player(GetSecondPlayer(Players[0].Value), Type == GameType.TwoPlayers, this));
            }
            else if (Players.Count == 1)
            {
                Players.Add(new Player(GetSecondPlayer(Players[0].Value), Type == GameType.TwoPlayers, this));
            }

            CurrentPlayer = Players[Random.Next(0, 2)];
        }

        public void NextRound()
        {
            CurrentPlayer = WinnerPlayer();

            if (CurrentPlayer != null) //not draw
            {
                CurrentPlayer.Score++;
            }
            else
            {
                CurrentPlayer = Players[Random.Next(0, 2)];
            }

            board = new BoardData[RowCount * ColumnCount];
        }


        public Player GetNextPlayer()
        {
            if (CurrentPlayer == Players[0])
            {
                return Players[1];
            }
            else
            {
                return Players[0];
            }
        }

        public void MoveToNextPlayer()
        {
            CurrentPlayer = GetNextPlayer();
        }


        public bool IsPlayerCanPlay()
        {
            return board.Contains(BoardData.None);
        }

        public bool IsBoardEmpty()
        {
            return !board.Contains(BoardData.X) && !board.Contains(BoardData.O);
        }

        public bool IsGameOver()
        {
            return WinnerPlayer() != null || !IsPlayerCanPlay()|| IsObjectDisposed;
        }

        public Player WinnerPlayer()
        {
            if (Players.Count < 2)
            {
                return null;
            }

            if (IsPlayerWin(Players[0]))
            {
                return Players[0];
            }
            else if (IsPlayerWin(Players[1]))
            {
                return Players[1];
            }
            else
            {
                return null;
            }
        }


        public Move WinPoints(Player player)
        {
            // Check rows
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount - MaxVerifyColumnCount + 1; j++)
                {
                    bool IsWin = true;
                    int start = i * ColumnCount + j;
                    int end = (i * ColumnCount + j + MaxVerifyColumnCount - 1);


                    for (int k = 0; k < MaxVerifyColumnCount; k++)
                    {
                        if (board[i * ColumnCount + j + k] != player.Value)
                        {
                            IsWin = false;
                            break;
                        }
                    }


                    if (IsWin)
                    {
                        return new Move(start, end);
                    }
                }
            }

            // Check columns
            for (int i = 0; i < ColumnCount; i++)
            {
                for (int j = 0; j < RowCount - MaxVerifyRowCount + 1; j++)
                {
                    bool IsWin = true;
                    int start = j * ColumnCount + i;
                    int end = (j + MaxVerifyRowCount - 1) * ColumnCount + i;
                    for (int k = 0; k < MaxVerifyRowCount; k++)
                    {
                        if (board[(j + k) * ColumnCount + i] != player.Value)
                        {
                            IsWin = false;
                            break;
                        }
                    }

                    if (IsWin)
                    {
                        return new Move(start, end);
                    }
                }
            }

            // Check diagonals
            for (int i = 0; i < RowCount - MaxVerifyRowCount + 1; i++)
            {
                for (int j = 0; j < ColumnCount - MaxVerifyColumnCount + 1; j++)
                {
                    bool IsWin = true;
                    int start = i * ColumnCount + j;
                    int end = (i + MaxVerifyRowCount - 1) * ColumnCount + j + MaxVerifyRowCount - 1;

                    for (int k = 0; k < MaxVerifyRowCount; k++)
                    {
                        if (board[(i + k) * ColumnCount + j + k] != player.Value)
                        {
                            IsWin = false;
                            break;
                        }
                    }

                    if (IsWin)
                    {
                        return new Move(start, end);
                    }
                }
            }

            // Check anti-diagonals
            for (int i = 0; i < RowCount - MaxVerifyRowCount + 1; i++)
            {
                for (int j = MaxVerifyColumnCount - 1; j < ColumnCount; j++)
                {
                    bool IsWin = true;
                    int start = i * ColumnCount + j;
                    int end = (i + MaxVerifyRowCount - 1) * ColumnCount + j - (MaxVerifyRowCount - 1);

                    for (int k = 0; k < MaxVerifyRowCount; k++)
                    {
                        if (board[(i + k) * ColumnCount + j - k] != player.Value)
                        {
                            IsWin = false;
                            break;
                        }
                    }
                    if (IsWin)
                    {
                        return new Move(start, end);
                    }
                }
            }



            return new Move(-1, -1);
        }


        public bool IsPlayerWin(Player player)
        {
            var winpoint = WinPoints(player);

            if (winpoint.IsEmpty())
                return false;
            else
                return true;

        }

        public void Dispose()
        {
            IsObjectDisposed= true;
        }

        ~GameState()
        {
            Dispose();
        }
    }
}
