using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Domino_Game.Lib.Core.Components
{
    public enum eMove
    {
        Right = 1,
        Bottom,
        LeftFilp,
        TopFilp,
        Left,
        Top,
        RightFilp,
        BottomFilp,
    }
    public class BoardDarw : UserControl
    {
        public UIElement BaseElement { get; set; }
        public UIElement LastRightElement { get; set; }
        public UIElement LastLeftElement { get; set; }
        public UIElement LastTopElement { get; set; }
        public UIElement LastBottomElement { get; set; }
        public double Scale { get; set; } = 1;
        public Board board { get; set; }
        public new int Padding { get; set; } = 2;
        private Point Center
        {
            get
            {
                double centerX = ActualWidth / 2;
                double centerY = ActualHeight / 2;
                return new Point(centerX, centerY);
            }
        }
        public Canvas canvas { get; set; }
        public BoardDarw()
        {
            canvas = new Canvas();
            AddChild(canvas);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (board?.BaseNode == null)
            {
                return;
            }

            Render();
        }

        private void Render()
        {
            if (board?.BaseNode == null)
            {
                return;
            }
            Scale = 1.0;
            while (true)
            {
                Image temp = GetImage(board.BaseNode.BaseCard, eRotation.Horizontal);
                if (temp.Source != null)
                {
                    double d = Center.X - temp.Source.Width * 7;
                    double d2 = Center.Y - temp.Source.Width * 3;
                    if (d < 0 || d2 < 0)
                    {
                        Scale -= 0.1;
                        continue;
                    }
                    break;
                }
                Scale = 0.1;
                break;
            }


            canvas.Children.Clear();
            Image image = GetImage(board.BaseNode.BaseCard, board.BaseNode.BaseCard.IsDouble ? eRotation.Vertical : eRotation.Horizontal);

            DrawCard(image, Center.X, Center.Y);

            BaseElement = canvas.Children[^1];


            UIElement GetLastChild(Func<Card, Card> nextNode)
            {

                if (nextNode(board.BaseNode.BaseCard) == null)
                {
                    return BaseElement;
                }
                else
                {
                    return canvas.Children[^1];
                }
            };

            DrawLine(board.BaseNode.BaseCard, image, Canvas.GetLeft(BaseElement), Center.Y, eMove.Left, board.GetLeftNode);
            LastLeftElement = GetLastChild(board.GetLeftNode);

            DrawLine(board.BaseNode.BaseCard, image, Canvas.GetLeft(BaseElement), Center.Y, eMove.Right, board.GetRightNode);
            LastRightElement = GetLastChild(board.GetRightNode);

            DrawLine(board.BaseNode.BaseCard, image, Canvas.GetLeft(BaseElement), Canvas.GetTop(BaseElement), eMove.Top, board.GetTopNode);
            LastTopElement = GetLastChild(board.GetTopNode);

            DrawLine(board.BaseNode.BaseCard, image, Canvas.GetLeft(BaseElement), Canvas.GetTop(BaseElement), eMove.Bottom, board.GetBottomNode);
            LastBottomElement = GetLastChild(board.GetBottomNode);
        }



        private void DrawCard(Image img, double startx, double starty)
        {
            double x = startx;
            double y = starty;
            Card card = img.Tag as Card;



            if (card.IsDouble)
            {
                if (card.rotation is eRotation.Vertical or eRotation.VerticalFilp)
                {
                    y -= img.Source.Width / 2;
                }
                else
                {
                    x -= img.Source.Height / 2;
                }

            }

            Canvas.SetLeft(img, x);
            Canvas.SetTop(img, y);
            canvas.Children.Add(img);
        }



        private bool DrawCardInLeft(bool IsFilpped, Card card, ref double startx, ref double starty, ref Image lastImg)
        {

            eRotation Direction = !IsFilpped ?
                card.IsDouble ? eRotation.Vertical : eRotation.Horizontal :
                card.IsDouble ? eRotation.VerticalFilp : eRotation.HorizontalFlip;
            Image img = GetImage(card, Direction);

            if (startx <= img.Source.Width + Padding + (img.Source.Width / 4))
            {
                return false;
            }

            startx -= img.Source.Width + Padding;

            lastImg = img;
            DrawCard(img, startx, starty);
            return true;
        }



        private bool DrawCardInRight(bool IsFilpped, Card card, ref double startx, ref double starty, ref Image lastImg)
        {
            eRotation Direction = !IsFilpped ?
                            card.IsDouble ? eRotation.Vertical : eRotation.Horizontal :
                            card.IsDouble ? eRotation.VerticalFilp : eRotation.HorizontalFlip;

            Image img = GetImage(card, Direction);

            double temp = startx + lastImg.Source.Width + img.Source.Width + Padding;

            temp += img.Source.Width / 4;


            if (temp >= ActualWidth)
            {
                return false;
            }

            startx += lastImg.Source.Width + Padding;

            lastImg = img;
            DrawCard(img, startx, starty);
            return true;
        }

        private bool DrawCardInTop(bool IsFilpped, Card card, ref double startx, ref double starty, ref Image lastImg)
        {
            eRotation Direction = !IsFilpped ?
                card.IsDouble ? eRotation.Horizontal : eRotation.Vertical :
                card.IsDouble ? eRotation.HorizontalFlip : eRotation.VerticalFilp;


            Image img = GetImage(card, Direction);



            if (starty <= img.Source.Height + Padding + (img.Source.Height / 4))
            {
                return false;
            }

            starty -= img.Source.Height + Padding;

            lastImg = img;
            DrawCard(img, startx, starty);
            return true;

        }

        private bool DrawCardInBottom(bool IsFilpped, Card card, ref double startx, ref double starty, ref Image lastImg)
        {
            eRotation Direction = !IsFilpped ?
                card.IsDouble ? eRotation.Horizontal : eRotation.Vertical :
                card.IsDouble ? eRotation.HorizontalFlip : eRotation.VerticalFilp;


            Image img = GetImage(card, Direction);


            double temp = starty + lastImg.Source.Height + Padding + img.Source.Height;


            temp += img.Source.Width / 2;

            if ((int)temp > ActualHeight)
            {
                return false;
            }

            starty += lastImg.Source.Height + Padding;

            lastImg = img;
            DrawCard(img, startx, starty);

            return true;
        }


        private Image GetImage(Card card, eRotation Direction)
        {
            return card.GetImage(Direction, new Card.CardStyle() { Scale = this.Scale });
        }

        private void DrawLine(Card StartCard, Image LastImg, double rightstartx, double rightstarty, eMove StartMove, Func<Card, Card> GetNode)
        {
            Card card = GetNode(StartCard);
            eMove BaseMaove = StartMove;
            bool IsDrawed = false;
            int ErrorNumber = 0;
            while (card != null)
            {
                if (StartMove is eMove.Left or eMove.LeftFilp)
                {
                    IsDrawed = DrawCardInLeft(StartMove == eMove.LeftFilp, card, ref rightstartx, ref rightstarty, ref LastImg);
                    if (!IsDrawed)
                    {
                        StartMove = NextMove(StartMove, BaseMaove);

                        Image lastImg = (Image)canvas.Children[^1];

                        rightstarty = Canvas.GetTop(lastImg);
                        rightstartx = Canvas.GetLeft(lastImg);
                    }
                }
                else if (StartMove is eMove.Right or eMove.RightFilp)
                {
                    IsDrawed = DrawCardInRight(StartMove == eMove.RightFilp, card, ref rightstartx, ref rightstarty, ref LastImg);
                    if (!IsDrawed)
                    {
                        StartMove = NextMove(StartMove, BaseMaove);
                        Image lastImg = (Image)canvas.Children[^1];

                        rightstarty = Canvas.GetTop(lastImg);

                        Card Card = (Card)lastImg.Tag;


                        if (!Card.IsDouble)
                        {
                            rightstartx += lastImg.Source.Width / 2;
                        }
                    }
                }
                else if (StartMove is eMove.Top or eMove.TopFilp)
                {
                    IsDrawed = DrawCardInTop(StartMove == eMove.TopFilp, card, ref rightstartx, ref rightstarty, ref LastImg);
                    if (!IsDrawed)
                    {
                        StartMove = NextMove(StartMove, BaseMaove);

                        Image lastImg = (Image)canvas.Children[^1];
                        rightstarty = Canvas.GetTop(lastImg);
                        rightstartx = Canvas.GetLeft(lastImg);
                    }
                }
                else if (StartMove is eMove.Bottom or eMove.BottomFilp)
                {
                    IsDrawed = DrawCardInBottom(StartMove == eMove.BottomFilp, card, ref rightstartx, ref rightstarty, ref LastImg);
                    if (!IsDrawed)
                    {
                        StartMove = NextMove(StartMove, BaseMaove);

                        Image lastImg = (Image)canvas.Children[^1];

                        Card Card = (Card)lastImg.Tag;

                        if (!Card.IsDouble)
                        {
                            rightstarty += lastImg.Source.Height / 2;
                        }

                        rightstartx = Canvas.GetLeft(lastImg);
                    }
                }

                if (IsDrawed || ++ErrorNumber == 3)//error number for avoid infinite loop
                {
                    card = GetNode(card);
                    ErrorNumber = 1;
                }
            }
        }

        private static eMove NextMove(eMove StartMove, eMove BaseMaove)
        {

            int num = (int)(StartMove + 1);

            return num % Enum.GetNames(typeof(eMove)).Length == 0 ? BaseMaove : ++StartMove;
        }

        public void Refresh()
        {
            Render();
        }

        public void Clear()
        {
            canvas.Children.Clear();
        }

        public bool IsBoardEmpty()
        {
            return canvas.Children.Count == 0;
        }

        public bool IsInPlace(UIElement elementOne, UIElement elementTwo, eMove DirectionThatMoveTo)
        {
            if (elementOne == null || elementTwo == null)
            {
                return true;
            }
            double leftElementOne = Canvas.GetLeft(elementOne);//x
            double topElementOne = Canvas.GetTop(elementOne);//y
            double leftElementTwo = Canvas.GetLeft(elementTwo);//x
            double topElementTwo = Canvas.GetTop(elementTwo);//y
            double elementTwoHeight = elementTwo.RenderSize.Height;
            double elementTwoWidth = elementTwo.RenderSize.Width;


            if (elementTwo == BaseElement)
            {
                switch (DirectionThatMoveTo)
                {
                    case eMove.Right:
                        return leftElementTwo < leftElementOne &&
                               topElementTwo < topElementOne &&
                               topElementTwo + elementTwoHeight >= topElementOne;

                    case eMove.Left:
                        return leftElementTwo > leftElementOne &&
                               topElementTwo < topElementOne &&
                               topElementTwo + elementTwoHeight >= topElementOne;


                    case eMove.Top:
                        return topElementTwo > topElementOne &&
                               leftElementTwo < leftElementOne &&
                               leftElementTwo + elementTwoWidth >= leftElementOne;

                    case eMove.Bottom:
                        return topElementTwo < topElementOne &&
                               leftElementTwo < leftElementOne &&
                               leftElementTwo + elementTwoWidth >= leftElementOne;

                    default:
                        return false;
                }
            }

            //Todo |:
            double distance1 = Math.Sqrt(Math.Pow(leftElementOne - leftElementTwo, 2) + Math.Pow(topElementOne - topElementTwo, 2));

            leftElementOne += elementOne.RenderSize.Width;
            leftElementTwo += elementTwo.RenderSize.Width;
            topElementOne += elementOne.RenderSize.Height;
            topElementTwo += elementTwo.RenderSize.Height;

            double distance2 = Math.Sqrt(Math.Pow(leftElementOne - leftElementTwo, 2) + Math.Pow(topElementOne - topElementTwo, 2));

            return distance1 < Math.Max(elementTwo.RenderSize.Height, elementTwo.RenderSize.Width) ||
                   distance2 < Math.Max(elementTwo.RenderSize.Height, elementTwo.RenderSize.Width);
        }

        //left->Head || right-> Tail 
        //Top->Head || Bottom->Tail
        private void HighlightCard(object element, bool highlightHead)
        {
            if (element is null)
            {
                return;
            }

            var img = (Image)element;
            Card card = (Card)img.Tag;

            if (highlightHead)
            {
                card.Style.HighlightHead = true;

                if (board.BaseNode != null)
                    card.Style.HighlightTail = card.IsDouble;
            }
            else
            {
                card.Style.HighlightTail = true;
                if (board.BaseNode != null)
                    card.Style.HighlightHead = card.IsDouble;
            }

            img.Source = card.GetBitmapImage();
        }

        public void HighlightLeftCard() => HighlightCard(LastLeftElement, true);// Head
        public void HighlightRightCard() => HighlightCard(LastRightElement, false);// Tail
        public void HighlightTopCard() => HighlightCard(LastTopElement, true);// Head
        public void HighlightBottomCard() => HighlightCard(LastBottomElement, false);// Tail


        private void SelectedCard(object element, bool highlightHead)
        {
            if (element is null)
            {
                return;
            }
            var img = (Image)element;
            Card card = (Card)img.Tag;

            if (card.Style.IsSelected)
            {
                return;
            }

            card.Style.IsSelected = true;

            img.Source = card.GetBitmapImage();
        }

        public void SelectLeftCard() => SelectedCard(LastLeftElement, true);// Head
        public void SelectRightCard() => SelectedCard(LastRightElement, false);// Tail
        public void SelectTopCard() => SelectedCard(LastTopElement, true);// Head
        public void SelectBottomCard() => SelectedCard(LastBottomElement, false);// Tail


        private void UnSelectCard(object element)
        {
            if (element is null)
            {
                return;
            }

            var img = (Image)element;
            Card card = (Card)img.Tag;
            if (!card.Style.IsSelected)
                return;


            card.Style.IsSelected = false;

            img.Source = card.GetBitmapImage();
        }

        public void UnSelectLeftCard() => UnSelectCard(LastLeftElement);
        public void UnSelectRightCard() => UnSelectCard(LastRightElement);
        public void UnSelectTopCard() => UnSelectCard(LastTopElement);
        public void UnSelectBottomCard() => UnSelectCard(LastBottomElement);
        public void UnSelectedAllCards()
        {
            UnSelectLeftCard();
            UnSelectRightCard();
            UnSelectTopCard();
            UnSelectBottomCard();
        }

        private void RestoreCard(object element)
        {
            if (element is null)
            {
                return;
            }

            var img = (Image)element;
            Card card = (Card)img.Tag;

            if (!card.Style.HighlightHead && !card.Style.HighlightTail && !card.Style.IsSelected)
                return;


            card.Style.HighlightHead = false;
            card.Style.HighlightTail = false;
            card.Style.IsSelected = false;
            img.Source = card.GetBitmapImage();
        }

        public void RestoreLeftCard() => RestoreCard(LastLeftElement);
        public void RestoreRightCard() => RestoreCard(LastRightElement);
        public void RestoreTopCard() => RestoreCard(LastTopElement);
        public void RestoreBottomCard() => RestoreCard(LastBottomElement);

        public void RestoreAllCards()
        {
            RestoreLeftCard();
            RestoreRightCard();
            RestoreTopCard();
            RestoreBottomCard();
        }

    }
}
