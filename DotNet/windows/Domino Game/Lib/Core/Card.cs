using Domino_Game.Lib.Core.Components;
using Domino_Game.Lib.Core.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Domino_Game.Lib.Core
{

    public enum eRotation
    {
        Vertical = 1,
        VerticalFilp = 2,
        Horizontal = 3,
        HorizontalFlip = 4
    }

    public class Card
    {
        public string Name
        {
            get
            {
                return Head + "-" + Tail;
            }
        }
        public string Description { get; set; } = string.Empty;
        public int Head { get; set; }
        public int Tail { get; set; }
        public int Priority { get; set; } = 0;

        public eRotation rotation { get; set; }
        public eDirection direction { get; set; } = eDirection.auto;

        public CardStyle Style { get; set; } = new CardStyle();


        public Player Player { get; set; }

        public int Value
        {
            get
            {
                return Head + Tail;
            }
        }

        public bool IsDouble
        {
            get
            {
                return Head == Tail;
            }
        }
        public Card() { }

        public Card(string name, string description, int head, int tail)
        {
            Description = description;

            if (head > tail)
            {
                Head = tail;
                Tail = head;
            }
            else
            {
                Head = head;
                Tail = tail;
            }
        }

        public class CardStyle
        {
            public double Scale { get; set; } = 1;
            public bool HideCard { get; set; } = false;
            public bool HighlightHead { get; set; } = false;
            public bool HighlightTail { get; set; } = false;
            public bool IsSelected { get; set; } = false;
            public bool IsEnabled { get; set; } = true;

        }

        public System.Windows.Controls.Image GetImage() => GetImage(rotation, Style);
        public System.Windows.Controls.Image GetImage(eRotation direction, CardStyle style = null, bool StoreStyle = true)
        {
            var img = GetBitmapImage(direction, style, StoreStyle);

            var image = new System.Windows.Controls.Image();

            image.Stretch = System.Windows.Media.Stretch.None;
            image.Source = img;
            image.Tag = this;

            return image;
        }

        public BitmapImage GetBitmapImage() => GetBitmapImage(rotation, Style);
        public BitmapImage GetBitmapImage(eRotation direction, CardStyle style = null, bool StoreStyle = true)
        {
            if (style == null)
            {
                style = new CardStyle();
            }

            if (StoreStyle)
                Style = style;

            this.rotation = direction;
            var img = new Bitmap(50, 100);
            var g = Graphics.FromImage(img);

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //g.FillEllipse(new SolidBrush(System.Drawing.Color.Black), 0, 0, 10, 10);

            if (!style.HideCard)
            {
                g.FillRoundedRectangle(style.IsEnabled ? System.Drawing.Brushes.White : System.Drawing.Brushes.Gray, new Rectangle(0, 0, img.Width, img.Height), 8);


                if (style.HighlightHead)
                {
                    g.FillRoundedRectangle(!style.IsSelected ? System.Drawing.Brushes.Orange : System.Drawing.Brushes.DarkGreen, new Rectangle(0, 0, img.Width, img.Height / 2), 8);
                }

                if (style.HighlightTail)
                {
                    g.FillRoundedRectangle(!style.IsSelected ? System.Drawing.Brushes.Orange : System.Drawing.Brushes.DarkGreen, new Rectangle(0, img.Height / 2, img.Width, img.Height / 2), 8);
                }

                g.DrawImage(CardDraw.GetCardImage(Head, 50, img.Height / 2), 0, 0);



                g.DrawImage(CardDraw.GetCardImage(Tail, 50, img.Height / 2), 0, img.Height / 2);


                g.DrawLine(new Pen(System.Drawing.Color.Black, 2), new Point(5, img.Height / 2), new Point(img.Width - 5, img.Height / 2));


                //  g.FillEllipse(new SolidBrush(Color.Yellow), (img.Width / 2) - 4, (img.Height / 2) - 4, 7, 7);
            }
            else
            {
                g.FillRoundedRectangle(System.Drawing.Brushes.Orange, new Rectangle(0, 0, img.Width, img.Height), 8);
            }


            g.Save();

            try
            {
                img = new Bitmap(img, new Size((int)(img.Width * style.Scale), (int)(img.Height * style.Scale)));

            }
            catch
            {
                return null;
            }


            if (direction == eRotation.Horizontal)
            {
                img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            if (direction == eRotation.HorizontalFlip)
            {
                img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            if (direction == eRotation.VerticalFilp)
            {
                img.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }



            //var url = new Uri("pack://application:,,,/Lib/assets/images/" + Name + + ".png");
            //Debug.WriteLine(url.AbsolutePath);

            //image.Source = new System.Windows.Media.Imaging.BitmapImage(url);

            using (MemoryStream memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }


        }

        public void Flip()
        {
            var temp = Head;
            Head = Tail;
            Tail = temp;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
