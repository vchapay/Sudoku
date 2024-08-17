using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sudoku.MapLogic;

namespace Sudoku.Controls
{
    internal class SudokuPreviewPage : Control
    {
        private HatchBrush _bgHatchBrush;
        private LinearGradientBrush _bgGradientBrush;
        private SudokuControlModel _nameBox;
        private SudokuControlModel _descriptionBox;
        private SudokuControlModel _playBtn;
        private SudokuControlModel _editBtn;
        private SudokuControlModel _copyBtn;
        private Rectangle _bottomPanelRect;
        private Rectangle _idRect;
        private Rectangle _sizeRect;
        private Font _font;
        private Brush _textBrush;
        private StringFormat _textFormat;
        private List<SudokuControlModel> _boxes;
        private Map _map;

        public SudokuPreviewPage() 
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;

            _map = new Map() 
            {
                Name = "map1",
                Description = string.Empty
            };

            _nameBox = new SudokuControlModel()
            {
                Text = "map1",
                Font = new Font("Times New Roman", 36),
                TextTrimming = 30
            };

            _descriptionBox = new SudokuControlModel()
            {
                TextVertAligment = StringAlignment.Near,
                TextMaxLenght = 400,
                TextTrimming = 410
            };

            _playBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.PlayingIcon
            };

            _editBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.ChangingIcon
            };

            _copyBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.CopyingIcon
            };

            _boxes = new List<SudokuControlModel>()
            {
                _nameBox,
                _descriptionBox,
            };

            _font = new Font("Times New Roman", 18);
            _textBrush = Brushes.Black;
            _textFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            Width = 500;
            Height = 300;

            _bgHatchBrush = new HatchBrush(HatchStyle.Sphere,
               Color.White, Color.FromArgb(50, 180, 160, 200));

            _bgGradientBrush = new LinearGradientBrush(ClientRectangle,
                Color.FromArgb(120, 250, 250, 250),
                Color.FromArgb(70, 190, 190, 210), 72);
        }

        /// <summary>
        /// Карта, используемая контролом.
        /// </summary>
        public Map Map 
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
                _nameBox.Text = _map.Name;
                _descriptionBox.Text = _map.Description;
            }
        }

        /// <summary>
        /// Происходит при нажатии кнопки "редактировать карту".
        /// </summary>
        public event MapActionClickHandler EditingClicked;

        /// <summary>
        /// Происходит при нажатии кнопки "сыграть карту".
        /// </summary>
        public event MapActionClickHandler PlayingClicked;

        /// <summary>
        /// Происходит при изменении названия карты.
        /// </summary>
        public event MapActionClickHandler NameChanged;

        /// <summary>
        /// Происходит при изменении описания карты.
        /// </summary>
        public event MapActionClickHandler DescriptionChanged;

        /// <summary>
        /// Происходит при нажатии на кнопку "копировать".
        /// </summary>
        public event MapActionClickHandler CopyingClicked;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(_bgHatchBrush, ClientRectangle);
            g.FillRectangle(_bgGradientBrush, ClientRectangle);
            _nameBox.Draw(g);
            _descriptionBox.Draw(g);
            _playBtn.Draw(g);
            _editBtn.Draw(g);
            _copyBtn.Draw(g);

            g.DrawString($"ID: {_map.ID}", _font, _textBrush, _idRect, _textFormat);

            g.DrawString($"Размер: {_map.ColumnsCount}x{_map.RowsCount}",
                _font, _textBrush, _sizeRect, _textFormat);
        }

        protected override void OnResize(EventArgs e)
        {
            _nameBox.Width = Width * 5 / 10;
            _nameBox.Height = 80;
            _nameBox.Y = Height * 15 / 100;
            _nameBox.X = (Width - _nameBox.Width) / 2;

            _descriptionBox.Width = Width * 7 / 10;
            _descriptionBox.Height = 210;
            _descriptionBox.Y = _nameBox.Bottom + 5;
            _descriptionBox.X = (Width - _descriptionBox.Width) / 2;

            _bottomPanelRect.Width = Width * 5 / 10;
            _bottomPanelRect.Height = Height / 14;
            _bottomPanelRect.X = (Width - _bottomPanelRect.Width) / 2;
            _bottomPanelRect.Y = (Height - _bottomPanelRect.Height) * 98 / 100;

            _idRect.Width = _bottomPanelRect.Width * 4 / 10;
            _idRect.Height = _bottomPanelRect.Height;
            _idRect.X = _bottomPanelRect.X;
            _idRect.Y = _bottomPanelRect.Y;

            _sizeRect.Width = _bottomPanelRect.Width * 4 / 10;
            _sizeRect.Height = _bottomPanelRect.Height;
            _sizeRect.X = _bottomPanelRect.Right - _sizeRect.Width;
            _sizeRect.Y = _bottomPanelRect.Y;

            _playBtn.Width = _descriptionBox.Width / 7;
            _playBtn.Height = _playBtn.Width;
            _playBtn.X = _descriptionBox.X + _descriptionBox.Width / 4;
            _playBtn.Y = _descriptionBox.Bottom + 10;

            _editBtn.Width = _descriptionBox.Width / 7;
            _editBtn.Height = _playBtn.Width;
            _editBtn.X = _descriptionBox.Right - 
                _descriptionBox.Width / 4 - _editBtn.Width;
            _editBtn.Y = _descriptionBox.Bottom + 10;

            _copyBtn.Width = Width * 5 / 100;
            _copyBtn.Height = _copyBtn.Width;
            _copyBtn.X = Width - _copyBtn.Width - 10;
            _copyBtn.Y = Height * 3 / 10;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _nameBox.IsPressed = _nameBox.IsFocused(e.Location);
                _descriptionBox.IsPressed = _descriptionBox.IsFocused(e.Location);
                _editBtn.IsPressed = _editBtn.IsFocused(e.Location);
                _playBtn.IsPressed = _playBtn.IsFocused(e.Location);
                _copyBtn.IsPressed = _copyBtn.IsFocused(e.Location);
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _nameBox.IsSelected = _nameBox.IsFocused(e.Location);
            _descriptionBox.IsSelected = _descriptionBox.IsFocused(e.Location);
            _editBtn.IsSelected = _editBtn.IsFocused(e.Location);
            _playBtn.IsSelected = _playBtn.IsFocused(e.Location);
            _copyBtn.IsSelected = _copyBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_editBtn.IsPressed)
            {
                OnEditClick(new MapActionClickArgs(Map));
            }

            if (_playBtn.IsPressed)
            {
                OnPlayClick(new MapActionClickArgs(Map));
            }

            if (_copyBtn.IsPressed)
            {
                OnCopyClick(new MapActionClickArgs(Map));
            }

            _editBtn.IsPressed = false;
            _playBtn.IsPressed = false;
            _copyBtn.IsPressed = false;
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            foreach (var box in _boxes)
            {
                if (box.IsPressed)
                {
                    if (box == _nameBox)
                        if (@"/\|*<>:?""".Contains(e.KeyChar.ToString()))
                            return;

                    if (e.KeyChar != '\b' && e.KeyChar != '\r')
                    {
                        box.Text += e.KeyChar;
                        Invalidate();
                    }

                    else if (e.KeyChar == '\b')
                    {
                        if (box.Text.Length > 0)
                        {
                            box.Text = box.Text.Remove(box.Text.Length - 1);
                            Invalidate();
                        }
                    }

                    if (box == _nameBox)
                    {
                        _map.Name = _nameBox.Text;
                        OnNameChanged(new MapActionClickArgs(_map));
                    }

                    else
                    {
                        _map.Description = _descriptionBox.Text;
                        OnDescriptionChanged(new MapActionClickArgs(_map));
                    }
                }
            }
        }

        private void OnEditClick(MapActionClickArgs e)
        {
            EditingClicked?.Invoke(this, e);
        }

        private void OnPlayClick(MapActionClickArgs e)
        {
            PlayingClicked?.Invoke(this, e);
        }

        private void OnNameChanged(MapActionClickArgs e)
        {
            NameChanged?.Invoke(this, e);
        }

        private void OnDescriptionChanged(MapActionClickArgs e)
        {
            DescriptionChanged?.Invoke(this, e);
        }

        private void OnCopyClick(MapActionClickArgs e)
        {
            CopyingClicked?.Invoke(this, e);
        }
    }
}
