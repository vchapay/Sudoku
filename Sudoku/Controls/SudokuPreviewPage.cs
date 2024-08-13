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
                Font = new Font("Times New Roman", 36)
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

            _boxes = new List<SudokuControlModel>()
            {
                _nameBox,
                _descriptionBox,
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
        }

        protected override void OnResize(EventArgs e)
        {
            _nameBox.Width = Width * 5 / 10;
            _nameBox.Height = 80;
            _nameBox.Y = 5;
            _nameBox.X = (Width - _nameBox.Width) / 2;

            _descriptionBox.Width = Width * 7 / 10;
            _descriptionBox.Height = 210;
            _descriptionBox.Y = _nameBox.Bottom + 5;
            _descriptionBox.X = (Width - _descriptionBox.Width) / 2;

            _playBtn.Width = _descriptionBox.Width / 7;
            _playBtn.Height = _playBtn.Width;
            _playBtn.X = _descriptionBox.X + _descriptionBox.Width / 4;
            _playBtn.Y = _descriptionBox.Bottom + _playBtn.Width;

            _editBtn.Width = _descriptionBox.Width / 7;
            _editBtn.Height = _playBtn.Width;
            _editBtn.X = _descriptionBox.Right - 
                _descriptionBox.Width / 4 - _editBtn.Width;
            _editBtn.Y = _descriptionBox.Bottom + _editBtn.Width;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _nameBox.IsPressed = _nameBox.IsFocused(e.Location);
                _descriptionBox.IsPressed = _descriptionBox.IsFocused(e.Location);
                _editBtn.IsPressed = _editBtn.IsFocused(e.Location);
                _playBtn.IsPressed = _playBtn.IsFocused(e.Location);
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _nameBox.IsSelected = _nameBox.IsFocused(e.Location);
            _descriptionBox.IsSelected = _descriptionBox.IsFocused(e.Location);
            _editBtn.IsSelected = _editBtn.IsFocused(e.Location);
            _playBtn.IsSelected = _playBtn.IsFocused(e.Location);
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

            _editBtn.IsPressed = false;
            _playBtn.IsPressed = false;
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
                        _map.Name = _nameBox.Text;

                    else
                        _map.Description = _descriptionBox.Text;
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
    }
}
