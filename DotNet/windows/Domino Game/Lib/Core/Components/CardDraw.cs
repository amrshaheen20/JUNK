using Domino_Game.Lib.Core.Exceptions;
using System.Drawing;

namespace Domino_Game.Lib.Core.Components
{
    public class CardDraw
    {
        [Flags]
        protected enum ePostion
        {
            TopLeft = 1 << 0,
            TopCenter = 1 << 1,
            TopRight = 1 << 2,
            CenterLeft = 1 << 3,
            Center = 1 << 4,
            CenterRight = 1 << 5,
            BottomLeft = 1 << 6,
            BottomCenter = 1 << 7,
            BottomRight = 1 << 8
        }

        public int Value { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        private Point Center
        {
            get
            {
                return new Point(Width / 2, Height / 2);
            }
        }

        public Color Background { get; set; } = Color.White;
        public Color Foreground { get; set; } = Color.Black;

        public int DotSize { get; set; } = 10;
        public int Padding { get; set; } = 5;



        public static Image GetCardImage(int value, int width, int height)
        {
            var card = new CardDraw();
            card.Value = value;
            card.Width = width;
            card.Height = height;

            return card.Draw();
        }




        public Image Draw()
        {
            var img = new Bitmap(Width, Height);

            var g = Graphics.FromImage(img);
            // g.FillRectangle(new SolidBrush(Background), 0, 0, Width, Height);

            //draw borders
            // var pen = new Pen(new SolidBrush(Foreground));
            //  pen.Width = 5;
            //g.DrawRectangle(pen, 0, 0, Width, Height);

            switch (Value)
            {
                case 0:
                    return img;
                case 1:
                    DrawShape(g, ePostion.Center);
                    break;
                case 2:
                    DrawShape(g, ePostion.TopRight | ePostion.BottomLeft);
                    break;
                case 3:
                    DrawShape(g, ePostion.TopRight | ePostion.BottomLeft | ePostion.Center);
                    break;
                case 4:
                    DrawShape(g, ePostion.TopLeft | ePostion.TopRight | ePostion.BottomLeft | ePostion.BottomRight);
                    break;
                case 5:
                    DrawShape(g, ePostion.TopLeft | ePostion.TopRight | ePostion.BottomLeft | ePostion.BottomRight | ePostion.Center);
                    break;
                case 6:
                    DrawShape(g, ePostion.TopLeft | ePostion.TopRight | ePostion.BottomLeft | ePostion.BottomRight | ePostion.CenterLeft | ePostion.CenterRight);
                    break;
                default:
                    throw new InvalidCardValue("Invalid Card Value");
            }

            return img;
        }


        protected void DrawShape(Graphics g, ePostion postion)
        {
            Point TopLeft = new Point(Padding, Padding);
            Point TopRight = new Point((Width - Padding) - DotSize, Padding);
            Point BottomLeft = new Point(Padding, Height - Padding - DotSize);
            Point BottomRight = new Point(Width - Padding - DotSize, Height - Padding - DotSize);

            int centerX = (TopLeft.X + TopRight.X + BottomLeft.X + BottomRight.X) / 4;
            int centerY = (TopLeft.Y + TopRight.Y + BottomLeft.Y + BottomRight.Y) / 4;

            Point CenterPoint = new Point(centerX, centerY);

            if (postion.HasFlag(ePostion.TopLeft))
            {

                g.FillEllipse(new SolidBrush(Foreground), TopLeft.X, TopLeft.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.TopCenter))
            {

                g.FillEllipse(new SolidBrush(Foreground), CenterPoint.X, TopRight.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.TopRight))
            {

                g.FillEllipse(new SolidBrush(Foreground), TopRight.X, TopRight.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.CenterLeft))
            {
                g.FillEllipse(new SolidBrush(Foreground), TopRight.X, CenterPoint.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.Center))
            {
                g.FillEllipse(new SolidBrush(Foreground), CenterPoint.X, CenterPoint.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.CenterRight))
            {
                g.FillEllipse(new SolidBrush(Foreground), TopLeft.X, CenterPoint.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.BottomLeft))
            {
                g.FillEllipse(new SolidBrush(Foreground), BottomLeft.X, BottomLeft.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.BottomCenter))
            {
                g.FillEllipse(new SolidBrush(Foreground), CenterPoint.X, BottomLeft.Y, DotSize, DotSize);
            }

            if (postion.HasFlag(ePostion.BottomRight))
            {
                g.FillEllipse(new SolidBrush(Foreground), BottomRight.X, BottomRight.Y, DotSize, DotSize);
            }
        }

    }
}
