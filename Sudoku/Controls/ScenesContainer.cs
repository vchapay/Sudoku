using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal class ScenesContainer : Control
    {
        private SudokuControlModel _exitBtn;
        private SudokuControlModel _backBtn;
        private Rectangle _iconRect;
        private Rectangle _topPanelRect;
        private Rectangle _controlRect;
        private Control _control;
        private HatchBrush _topPanelHatchBrush;
        private LinearGradientBrush _topPanelGradientBrush;
        private Pen _panelsPen;

        private BorderStyle _borderStyle;

        public ScenesContainer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
            BackColor = Color.White;
            _topPanelHatchBrush = new HatchBrush(HatchStyle.LargeGrid,
                Color.White, Color.FromArgb(50, 180, 160, 200));

            _backBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.UndoIcon,
                BackColor = Color.FromArgb(90, 200, 200, 200)
            };

            _exitBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.CrossIcon,
                BackColor = Color.FromArgb(60, 250, 150, 150)
            };

            Width = 500;
            Height = 300;

            _panelsPen = new Pen(Color.Gray);
        }

        public Control Control 
        {
            get
            {
                return _control;
            }
            set
            {
                _control = value;
                Controls.Clear();
                Controls.Add(_control);
                OnResize(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Стиль границ контрола
        /// </summary>
        public BorderStyle BorderStyle
        {
            get
            {
                return _borderStyle;
            }
            set
            {
                _borderStyle = value;
                Invalidate();
            }
        }

        public event EventHandler BackButtonClicked;

        public event EventHandler ExitButtonClicked;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(BackColor);

            g.FillRectangle(_topPanelHatchBrush, _topPanelRect);
            g.FillRectangle(_topPanelGradientBrush, _topPanelRect);
            g.DrawRectangle(_panelsPen, _topPanelRect);
            g.DrawRectangle(_panelsPen, _controlRect);

            _backBtn.Draw(g);
            _exitBtn.Draw(g);

            switch (_borderStyle)
            {
                case BorderStyle.None:
                    break;
                case BorderStyle.FixedSingle:
                    Rectangle outline = new Rectangle()
                    {
                        X = 0,
                        Y = 0,
                        Width = Width - 1,
                        Height = Height - 1
                    };

                    g.DrawRectangle(Pens.Black, outline);
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            int tWidth = Width - 1 > 0 ? Width - 1 : 1;
            int tHeight = Height / 13 > 0 ? Height / 13 : 1;
            _topPanelRect.X = 0;
            _topPanelRect.Y = 0;
            _topPanelRect.Width = tWidth;
            _topPanelRect.Height = tHeight;

            _backBtn.X = 5;
            _backBtn.Y = 5;
            _backBtn.Height = _topPanelRect.Height - 10;
            _backBtn.Width = _backBtn.Height;

            _exitBtn.Y = 5;
            _exitBtn.Height = _topPanelRect.Height - 10;
            _exitBtn.Width = _exitBtn.Height;
            _exitBtn.X = Width - _exitBtn.Width - 5;

            _topPanelGradientBrush = new LinearGradientBrush(_topPanelRect,
                Color.FromArgb(120, 250, 250, 250),
                Color.FromArgb(70, 190, 190, 210), 180);

            _controlRect.X = 5;
            _controlRect.Y = _topPanelRect.Bottom + 5;
            _controlRect.Width = Width - 10;
            _controlRect.Height = Height - _topPanelRect.Height - 10;

            if (Control != null)
            {
                Control.Location = 
                    new Point(_controlRect.X + 1, _controlRect.Y + 1);
                Control.Width = _controlRect.Width - 2;
                Control.Height = _controlRect.Height - 2;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _backBtn.IsSelected = _backBtn.IsFocused(e.Location);
            _exitBtn.IsSelected = _exitBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _backBtn.IsPressed = _backBtn.IsFocused(e.Location);
            _exitBtn.IsPressed = _exitBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_backBtn.IsPressed)
            {
                OnBackButtonClicked(EventArgs.Empty);
            }

            if (_exitBtn.IsPressed)
            {
                OnExitButtonClicked(EventArgs.Empty);
            }

            _backBtn.IsPressed = false;
            _exitBtn.IsPressed = false;
        }

        private void OnBackButtonClicked(EventArgs e)
        {
            BackButtonClicked?.Invoke(this, e);
        }

        private void OnExitButtonClicked(EventArgs e)
        {
            ExitButtonClicked?.Invoke(this, e);
        }
    }
}
