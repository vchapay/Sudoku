using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal class ScenesContainer : Control
    {
        private SudokuControlModel _exitBtn;
        private SudokuControlModel _collapseBtn;
        private SudokuControlModel _expandBtn;
        private SudokuControlModel _backBtn;
        private Rectangle _iconRect;
        private Rectangle _topPanelRect;
        private Rectangle _controlRect;
        private Rectangle _outlineRect;
        private Control _control;
        private HatchBrush _topPanelHatchBrush;
        private LinearGradientBrush _topPanelGradientBrush;
        private HatchBrush _outlineBrush;
        private Pen _panelsPen;
        private Pen _outlinePen;

        private BorderStyle _borderStyle;

        public ScenesContainer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
            _topPanelHatchBrush = new HatchBrush(HatchStyle.LargeGrid,
                Color.White, Color.FromArgb(50, 180, 160, 200));

            _backBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.UndoIcon,
                BackColor = Color.FromArgb(90, 200, 200, 200)
            };

            _collapseBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.CollapsingIcon,
                BackColor = Color.FromArgb(120, 200, 200, 230)
            };

            _expandBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.ExpandingIcon,
                BackColor = Color.FromArgb(120, 200, 200, 230)
            };

            _exitBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.CrossIcon,
                BackColor = Color.FromArgb(60, 250, 150, 150)
            };

            Width = 500;
            Height = 300;

            _panelsPen = new Pen(Color.Gray);

            _outlinePen = new Pen(Color.Gray)
            {
                Width = 2
            };

            _outlineBrush = new HatchBrush(HatchStyle.BackwardDiagonal,
                Color.FromArgb(150, 200, 200, 200), Color.FromArgb(140, 220, 220, 220));
        }

        public Control Control 
        {
            get
            {
                return _control;
            }
            set
            {
                if (value != null)
                {
                    _control = value;
                    _control.ClientSize = _controlRect.Size;
                    _control.Location = _controlRect.Location;
                    Controls.Clear();
                    Controls.Add(_control);
                    _control.Select();
                    OnResize(EventArgs.Empty);
                }
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

        public bool IsBackButtonVisible { get; set; }

        public event EventHandler BackButtonClicked;

        public event EventHandler ExitButtonClicked;

        public event EventHandler CollapseButtonClicked;

        public event EventHandler ExpandButtonClicked;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.White);
            g.FillRectangle(_outlineBrush, ClientRectangle);
            g.FillRectangle(Brushes.White, _outlineRect);
            g.FillRectangle(_topPanelHatchBrush, _topPanelRect);
            g.FillRectangle(_topPanelGradientBrush, _topPanelRect);
            g.DrawRectangle(_panelsPen, _topPanelRect);
            g.DrawRectangle(_panelsPen, _controlRect);
            g.DrawRectangle(_outlinePen, _outlineRect);

            if (IsBackButtonVisible)
                _backBtn.Draw(g);

            _exitBtn.Draw(g);
            _collapseBtn.Draw(g);
            _expandBtn.Draw(g);

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
            _outlineRect.X = 10;
            _outlineRect.Y = 10;
            _outlineRect.Width = Width - 20;
            _outlineRect.Height = Height - 20;

            int tWidth = _outlineRect.Width - 1 > 0 ? _outlineRect.Width - 1 : 1;
            int tHeight = 50 < Height ? 50 : 1;

            _topPanelRect.X = _outlineRect.X;
            _topPanelRect.Y = _outlineRect.Y;
            _topPanelRect.Width = tWidth;
            _topPanelRect.Height = tHeight;

            _backBtn.X = _topPanelRect.X + 5;
            _backBtn.Y = _topPanelRect.Y + 5;
            _backBtn.Height = _topPanelRect.Height - 10;
            _backBtn.Width = _backBtn.Height;

            _exitBtn.Y = _topPanelRect.Y + 5;
            _exitBtn.Height = _topPanelRect.Height - 10;
            _exitBtn.Width = _exitBtn.Height;
            _exitBtn.X = _outlineRect.Width - _exitBtn.Width - 5;

            _expandBtn.Y = _topPanelRect.Y + 5;
            _expandBtn.Height = _topPanelRect.Height - 10;
            _expandBtn.Width = _expandBtn.Height;
            _expandBtn.X = _exitBtn.X - _expandBtn.Width - 5;

            _collapseBtn.Y = _topPanelRect.Y + 5;
            _collapseBtn.Height = _topPanelRect.Height - 10;
            _collapseBtn.Width = _collapseBtn.Height;
            _collapseBtn.X = _expandBtn.X - _collapseBtn.Width - 5;

            _topPanelGradientBrush = new LinearGradientBrush(_topPanelRect,
                Color.FromArgb(120, 250, 250, 250),
                Color.FromArgb(70, 190, 190, 210), 180);

            _controlRect.X = _topPanelRect.X + 5;
            _controlRect.Y = _topPanelRect.Bottom + 5;
            _controlRect.Width = _outlineRect.Width - 10;
            _controlRect.Height = _outlineRect.Height - _topPanelRect.Height - 10;

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
            _collapseBtn.IsSelected = _collapseBtn.IsFocused(e.Location);
            _expandBtn.IsSelected = _expandBtn.IsFocused(e.Location);
            _backBtn.IsSelected = _backBtn.IsFocused(e.Location);
            _exitBtn.IsSelected = _exitBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _backBtn.IsPressed = _backBtn.IsFocused(e.Location);
            _collapseBtn.IsPressed = _collapseBtn.IsFocused(e.Location);
            _expandBtn.IsPressed = _expandBtn.IsFocused(e.Location);
            _exitBtn.IsPressed = _exitBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_backBtn.IsPressed && IsBackButtonVisible)
            {
                OnBackButtonClicked(EventArgs.Empty);
            }

            if (_exitBtn.IsPressed)
            {
                OnExitButtonClicked(EventArgs.Empty);
            }

            if (_collapseBtn.IsPressed)
            {
                OnCollapseButtonClicked(EventArgs.Empty);
            }

            if (_expandBtn.IsPressed)
            {
                OnExpandButtonClicked(EventArgs.Empty);
            }

            _collapseBtn.IsPressed = false;
            _expandBtn.IsPressed = false;
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

        private void OnCollapseButtonClicked(EventArgs e)
        {
            CollapseButtonClicked?.Invoke(this, e);
        }

        private void OnExpandButtonClicked(EventArgs e)
        {
            ExpandButtonClicked?.Invoke(this, e);
        }
    }
}
