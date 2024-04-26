namespace XO_Game.lib
{
    public class Player
    {
        public string Name { get; set; }//no need :D
        public BoardData Value { get; set; }
        public int Score { get; set; }
        public bool IsHuman { get; set; }

        GameState GameState { get; set; }

        public Player Opponent
        {
            get
            {
                return GameState.Players.FirstOrDefault(x => x != this);
            }
        }

        public Player(BoardData value, bool Ishuman, GameState game)
        {
            Value = value;
            IsHuman = Ishuman;
            GameState = game;
        }

        public Player(string name, BoardData value, bool Ishuman, GameState game)
        {
            Name = name;
            Value = value;
            GameState = game;
        }

        #region AI Methods
        private int evaluate()
        {
            if (GameState.IsPlayerWin(this))
            {
                return int.MaxValue;
            }
            else if (GameState.IsPlayerWin(Opponent))
            {
                return int.MinValue;
            }
            else
            {
                return 0;
            }
        }

        private int minmax(int depth, int alpha, int beta, bool isMax)
        {
            //Console.WriteLine($"Depth: {depth} Alpha: {alpha} Beta: {beta} IsMax: {isMax}");
            int score = evaluate();

            if (score != 0 || depth == 0)
                return score;

            if (!GameState.IsPlayerCanPlay())
            {
                return 0;
            }

            if (isMax)
            {
                int best = int.MinValue;

                for (int i = 0; i < GameState.RowCount; i++)
                {
                    for (int j = 0; j < GameState.ColumnCount; j++)
                    {
                        if (GameState.board[i * GameState.ColumnCount + j] == BoardData.None)
                        {
                            GameState.board[i * GameState.ColumnCount + j] = Value;

                            best = Math.Max(best, minmax(depth - 1, alpha, beta, !isMax));

                            alpha = Math.Max(alpha, best);

                            GameState.board[i * GameState.ColumnCount + j] = BoardData.None;

                            if (beta <= alpha)
                                break;
                        }
                    }
                }

                return best;
            }
            else
            {
                int best = int.MaxValue;

                for (int i = 0; i < GameState.RowCount; i++)
                {
                    for (int j = 0; j < GameState.ColumnCount; j++)
                    {
                        if (GameState.board[i * GameState.ColumnCount + j] == BoardData.None)
                        {
                            GameState.board[i * GameState.ColumnCount + j] = Opponent.Value;

                            best = Math.Min(best, minmax(depth - 1, alpha, beta, !isMax));

                            beta = Math.Min(beta, best);

                            GameState.board[i * GameState.ColumnCount + j] = BoardData.None;

                            if (beta <= alpha)
                                break;
                        }
                    }
                }

                return best;
            }
        }

        private Move FindBestMove(int depth)
        {
            var bestVal = int.MinValue;
            var alpha = int.MinValue;
            var beta = int.MaxValue;
            var bestMove = new Move();

            for (int i = 0; i < GameState.RowCount; i++)
            {
                for (int j = 0; j < GameState.ColumnCount; j++)
                {
                    if (GameState.board[i * GameState.ColumnCount + j] == BoardData.None)
                    {
                        GameState.board[i * GameState.ColumnCount + j] = Value;

                        int moveVal = minmax(depth, alpha, beta, false);
                        GameState.board[i * GameState.ColumnCount + j] = BoardData.None;

                        if (moveVal > bestVal)
                        {
                            bestMove.Startindex = i;
                            bestMove.Endindex = j;
                            bestVal = moveVal;
                        }
                    }
                }
            }

            return bestMove;
        }

        #endregion

        #region AI Modes
        public void AiPlayer()
        {
            if(GameState.IsObjectDisposed)
            {
                return;
            }

            Console.WriteLine("Ai is playing");
            Move move = new Move();

            if (GameState.Type == GameType.Easy)
            {
                move = FindBestMove(1);
            }
            else if (GameState.Type == GameType.Medium)
            {
                move = FindBestMove(3);
            }
            else if (GameState.Type == GameType.Impossible)
            {
                if (GameState.boardSize == BoardSize.Small)
                {
                    move = FindBestMove(-1); //get the best move
                }
                else
                {
#warning "ToDo: Find the best move for the big board"
                    move = FindBestMove(4);  //not the best move but it's good because the board is big and it will take a lot of time to get the best move
                }
            }

            if (!move.IsEmpty())
            {
                GameState.board[move.Startindex * GameState.ColumnCount + move.Endindex] = Value;
            }
            else
            {
                RandomAi();
            }



            GameState.MoveToNextPlayer();
            Console.WriteLine("Ai is played");
        }

        private void RandomAi()
        {
            var availablePositions = GameState.board.Select((x, i) => new { Data = x, Index = i }).Where(item => item.Data == BoardData.None).Select(item => item.Index).ToArray();
            var num = GameState.Random.Next(0, availablePositions.Count());
            GameState.board[availablePositions[num]] = Value;
        }

        #endregion
    }
}
