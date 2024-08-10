using System.Drawing;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal class SudokuControlModel
    {
        private Pen _pen;
        private Pen _selectionPen;
        private SolidBrush _selectionBrush;
        private SolidBrush _bgBrush;
        private StringFormat _format;
        private SolidBrush _textBrush;
        private Font _font;
        private string _text;
        private int _textTrimming;
        private int _textMax;

        public SudokuControlModel()
        {
            _pen = new Pen(Color.LightGray);
            _selectionPen = new Pen(Color.Black);
            _selectionBrush = new SolidBrush(Color.FromArgb(70, 150, 150, 150));
            _bgBrush = new SolidBrush(Color.FromArgb(100, Color.White));
            _textBrush = new SolidBrush(Color.Black);
            _format = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            _font = new Font("Times New Roman", 22);
            _textTrimming = 10;
            _textMax = 50;
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Right => X + Width;

        public int Left => X;

        public int Top => Y;

        public int Bottom => Y + Height;

        public bool IsSelected { get; set; }

        public bool IsPressed { get; set; }

        public string Text 
        {
            get 
            {
                return _text;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > _textMax)
                        value = value.Substring(0, _textMax);

                    _text = value;
                }
            }
        }

        public int TextMaxLenght
        {
            get
            {
                return _textMax;
            }
            set
            {
                if (value > 0)
                    _textMax = value;
            }
        }

        public int TextTrimming 
        {
            get 
            { 
                return _textTrimming; 
            }
            set
            {
                if (value > 0)
                    _textTrimming = value; 
            }
        }

        public StringAlignment TextHorAligment
        {
            get 
            { 
                return _format.Alignment; 
            }
            set
            {
                _format.Alignment = value;
            }
        }

        public StringAlignment TextVertAligment
        {
            get
            {
                return _format.LineAlignment;
            }
            set
            {
                _format.LineAlignment = value;
            }
        }

        public Color BackColor
        {
            get
            {
                return _bgBrush.Color;
            }
            set
            {
                _bgBrush.Color = value;
            }
        }

        public Color TextColor
        {
            get
            {
                return _textBrush.Color;
            }
            set
            {
                _textBrush.Color = value;
            }
        }

        public Font Font
        {
            get
            {
                return _font;
            }
            set
            {
                if (value != null)
                    _font = value;
            }
        }

        public Image Image { get; set; }

        public void Draw(Graphics g)
        {
            Rectangle bounds = new Rectangle(X, Y, Width, Height);

            g.FillRectangle(_bgBrush, bounds);

            if (Image != null)
            {
                g.DrawImage(Image, bounds);
            }
            
            if (Text != null)
            {
                string tr = Text;
                if (tr.Length > _textTrimming)
                {
                    tr = tr.Remove(_textTrimming);
                    tr += "...";
                }

                g.DrawString(tr, _font, _textBrush, bounds, _format);
            }

            Pen pen = _pen;
            if (IsSelected)
                pen = _selectionPen;

            g.DrawRectangle(pen, bounds);

            if (IsPressed)
            {
                g.FillRectangle(_selectionBrush, bounds);
            }
        }

        public bool IsFocused(Point point)
        {
            Rectangle rect = new Rectangle()
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height
            };

            return rect.Contains(point);
        }
    }
}
