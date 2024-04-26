using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XO_Game.lib;

namespace XO_Game.GuiComponents
{
    public class DrawBoard : Control
    {
        public Brush CellColor { get; set; } = Brushes.White;
        public Brush XColor { get; set; } = Brushes.Red;
        public Brush OColor { get; set; } = Brushes.Blue;
        public Brush GameOverLineColor { get; set; } = Brushes.White;
        public int CellLinesWidth { get; set; } = 8;
        public int CellWidth { get; set; } = 100;
        public int CellHeight { get; set; } = 100;
        public int CellPadding { get; set; } = 10;
        public GameState GameState { get; set; }



        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (GameState == null)
                GameState = new GameState(BoardSize.Small);

            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Background, null, new System.Windows.Rect(0, 0, this.ActualWidth, this.ActualHeight));
            if (GameState.RowCount > 0 && GameState.ColumnCount > 0)
            {
                Width = GameState.ColumnCount * CellWidth;
                Height = GameState.RowCount * CellHeight;

                double cellWidth = this.ActualWidth / GameState.ColumnCount;
                double cellHeight = this.ActualHeight / GameState.RowCount;
                for (int i = 1; i < GameState.ColumnCount; i++)
                {
                    drawingContext.DrawLine(new System.Windows.Media.Pen(CellColor, CellLinesWidth), new System.Windows.Point(i * cellWidth, CellPadding), new System.Windows.Point(i * cellWidth, this.ActualHeight - CellPadding));
                }
                for (int i = 1; i < GameState.RowCount; i++)
                {
                    drawingContext.DrawLine(new System.Windows.Media.Pen(CellColor, CellLinesWidth), new System.Windows.Point(CellPadding, i * cellHeight), new System.Windows.Point(this.ActualWidth - CellPadding, i * cellHeight));
                }


                if (GameState.Players.Count == 0)
                {
                    return;
                }


                DrawPlayers(drawingContext);

                if (GameState.IsGameOver())
                {
                    var winner = GameState.WinnerPlayer();
                    if (winner == null)
                    {
                        return;
                    }

                    var points = GameState.WinPoints(winner);
                    DrawLine(drawingContext, points.Startindex, points.Endindex);

                }
            }
        }



        protected void DrawPlayers(System.Windows.Media.DrawingContext drawingContext)
        {
            if (GameState.board != null)
            {
                for (int i = 0; i < GameState.board.Length; i++)
                {
                    int column = i % GameState.ColumnCount;
                    int row = i / GameState.ColumnCount;
                    double x = column * CellWidth;
                    double y = row * CellHeight;

                    if (GameState.board[i] == BoardData.X || i == MouseHoverIndex && GameState.CurrentPlayer.Value == BoardData.X)
                    {
#pragma warning disable CS0618 

                        var text = new FormattedText("X",
                            System.Globalization.CultureInfo.CurrentCulture,
                            System.Windows.FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            70,
                            XColor);

                        text.SetFontWeight(FontWeights.Bold);

                        double textWidth = text.WidthIncludingTrailingWhitespace;
                        double textHeight = text.Height;

                        drawingContext.DrawText(text, new System.Windows.Point(x + (CellWidth - textWidth) / 2, y + (CellHeight - textHeight) / 2));
                    }
                    else if (GameState.board[i] == BoardData.O || i == MouseHoverIndex && GameState.CurrentPlayer.Value == BoardData.O)
                    {
                        var text = new FormattedText("O",
                            System.Globalization.CultureInfo.CurrentCulture,
                            System.Windows.FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            70,
                            OColor);

                        text.SetFontWeight(FontWeights.Bold);
#pragma warning restore CS0618 

                        double textWidth = text.WidthIncludingTrailingWhitespace;
                        double textHeight = text.Height;

                        drawingContext.DrawText(text, new System.Windows.Point(x + (CellWidth - textWidth) / 2, y + (CellHeight - textHeight) / 2));
                    }

                }
            }
        }


        protected void DrawLine(System.Windows.Media.DrawingContext drawingContext, int index1, int index2)
        {
            int column1 = index1 % GameState.ColumnCount;
            int row1 = index1 / GameState.ColumnCount;
            int column2 = index2 % GameState.ColumnCount;
            int row2 = index2 / GameState.ColumnCount;

            double x1 = column1 * CellWidth + CellWidth / 2;
            double y1 = row1 * CellHeight + CellHeight / 2;
            double x2 = column2 * CellWidth + CellWidth / 2;
            double y2 = row2 * CellHeight + CellHeight / 2;

            if (y1 == y2)
            {
                x1 = column1 * CellWidth + CellWidth / 4;
                x2 = column2 * CellWidth + CellWidth / 4 * 3;
            }
            else if (x1 == x2)
            {
                y1 = row1 * CellHeight + CellHeight / 4;
                y2 = row2 * CellHeight + CellHeight / 4 * 3;
            }
            else
            {
                bool x1LessThanx2 = x1 < x2;
                bool y1LessThany2 = y1 < y2;

                x1 = column1 * CellWidth + (x1LessThanx2 ? CellWidth / 4 : CellWidth / 4 * 3);
                y1 = row1 * CellHeight + (y1LessThany2 ? CellHeight / 4 : CellHeight / 4 * 3);
                x2 = column2 * CellWidth + (x1LessThanx2 ? CellWidth / 4 * 3 : CellWidth / 4);
                y2 = row2 * CellHeight + (y1LessThany2 ? CellHeight / 4 * 3 : CellHeight / 4);
            }

            drawingContext.DrawLine(new System.Windows.Media.Pen(GameOverLineColor, 15), new Point(x1, y1), new System.Windows.Point(x2, y2));
        }





        int MouseHoverIndex = -1;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (GameState.board != null && GameState.CurrentPlayer != null
                && GameState.CurrentPlayer.IsHuman)
            {
                int column = (int)(e.GetPosition(this).X / CellWidth);
                int row = (int)(e.GetPosition(this).Y / CellHeight);
                if (column >= 0 && column < GameState.ColumnCount && row >= 0 && row < GameState.RowCount)
                {
                    int index = row * GameState.ColumnCount + column;

                    if (GameState.board[index] == BoardData.None && GameState.CurrentPlayer != null)
                    {
                        MouseHoverIndex = index;
                    }
                    else
                    {
                        MouseHoverIndex = -1;
                    }

                    InvalidateVisual();

                }
            }
        }

        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (GameState.board != null && GameState.CurrentPlayer != null && GameState.CurrentPlayer.IsHuman && MouseHoverIndex != -1)
            {

                GameState.board[MouseHoverIndex] = GameState.CurrentPlayer.Value;
                MouseHoverIndex = -1;
                GameState.CurrentPlayer = GameState.GetNextPlayer();

                InvalidateVisual();

                if (GameState.IsGameOver())
                {
                    OnGameover();
                }
                else
                {
                    OnPlayerChanged();

                    if (!GameState.CurrentPlayer.IsHuman)
                    {
                        OnAITurn();
                    }
                }

                Console.WriteLine("IsGameOver: " + GameState.IsGameOver());
                Console.WriteLine("IsPlayerWin: " + GameState.IsPlayerWin(GameState.CurrentPlayer));
            }
        }


        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (GameState.board != null && GameState.CurrentPlayer != null
                && GameState.CurrentPlayer.IsHuman)
            {
                if (MouseHoverIndex != -1)
                {
                    MouseHoverIndex = -1;
                }
                InvalidateVisual();
            }
        }


        public void Rrfresh()
        {
            InvalidateVisual();
        }


        [Browsable(true)]
        public event EventHandler Gameover;
        protected virtual void OnGameover()
        {
            Gameover?.Invoke(this, EventArgs.Empty);
        }

        [Browsable(true)]
        public event EventHandler PlayerChanged;

        protected virtual void OnPlayerChanged()
        {
            PlayerChanged?.Invoke(this, EventArgs.Empty);
        }


        [Browsable(true)]
        public event EventHandler AiTurn;
        protected virtual void OnAITurn()
        {
            AiTurn?.Invoke(this, EventArgs.Empty);
        }
    }
}
