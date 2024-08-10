using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal class MapCreatingPage : Control
    {
        private HatchBrush _bgHatchBrush;
        private LinearGradientBrush _bgGradientBrush;
        private SudokuControlModel _nameBox;
        private SudokuControlModel _descriptionBox;
        private SudokuControlModel _rowsBox;
        private SudokuControlModel _columnsBox;
        private List<SudokuControlModel> _letterBoxes;
        private List<SudokuControlModel> _digitBoxes;
        private SudokuControlModel _createBtn;
        private Rectangle _rowsLblRect;
        private Rectangle _columnsLblRect;
        private StringFormat _format;
        private SolidBrush _textBrush;
        private Font _lblsFont;

        public MapCreatingPage() 
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;

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
            _rowsBox = new SudokuControlModel()
            {
                Text = "9",
                TextMaxLenght = 2
            };
            _columnsBox = new SudokuControlModel()
            {
                Text = "9",
                TextMaxLenght = 2
            };

            _letterBoxes = new List<SudokuControlModel>()
            {
                _nameBox,
                _descriptionBox
            };

            _digitBoxes = new List<SudokuControlModel>()
            {
                _rowsBox,
                _columnsBox
            };

            _createBtn = new SudokuControlModel()
            {
                Text = "Создать",
                BackColor = Color.FromArgb(60, 100, 140, 230),
                Font = new Font("Times New Roman", 30)
            };

            Width = 300;
            Height = 300;

            _bgHatchBrush = new HatchBrush(HatchStyle.Sphere,
               Color.White, Color.FromArgb(50, 180, 160, 200));

            _bgGradientBrush = new LinearGradientBrush(ClientRectangle,
                Color.FromArgb(120, 250, 250, 250), 
                Color.FromArgb(70, 190, 190, 210), 72);

            _textBrush = new SolidBrush(ForeColor);
            _format = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            _lblsFont = new Font("Times New Roman", 16);
        }

        /// <summary>
        /// Список названий карт, которые уже использованы
        /// и не могут быть использованы повторно
        /// </summary>
        public string[] UsedNames { get; set; }

        /// <summary>
        /// Происходит при нажатии на кнопку "Создать"
        /// </summary>
        public event MapCreatingHandler CreateMapClicked;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(_bgHatchBrush, ClientRectangle);
            g.FillRectangle(_bgGradientBrush, ClientRectangle);
            _nameBox.Draw(g);
            _descriptionBox.Draw(g);
            g.DrawString("Число строк", _lblsFont, 
                _textBrush, _rowsLblRect, _format);
            _rowsBox.Draw(g);
            g.DrawString("Число столбцов", _lblsFont,
                _textBrush, _columnsLblRect, _format);
            _columnsBox.Draw(g);
            _createBtn.Draw(g);
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

            _rowsLblRect = new Rectangle()
            {
                Width = Width * 2 / 10,
                Height = 20,
                Y = _descriptionBox.Bottom + 15,
                X = (Width - _rowsLblRect.Width) / 2
            };

            _rowsBox.Width = Width * 3 / 10;
            _rowsBox.Height = 50;
            _rowsBox.Y = _rowsLblRect.Bottom + 5;
            _rowsBox.X = (Width - _rowsBox.Width) / 2;

            _columnsLblRect = new Rectangle()
            {
                Width = Width * 2 / 10,
                Height = 20,
                Y = _rowsBox.Bottom + 5,
                X = (Width - _columnsLblRect.Width) / 2
            };

            _columnsBox.Width = Width * 3 / 10;
            _columnsBox.Height = 50;
            _columnsBox.Y = _columnsLblRect.Bottom + 5;
            _columnsBox.X = (Width - _columnsBox.Width) / 2;

            _createBtn.Width = Width * 4 / 10;
            _createBtn.Height = 70;
            _createBtn.Y = ClientRectangle.Bottom - 5 - _createBtn.Height;
            _createBtn.X = (Width - _createBtn.Width) / 2;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _nameBox.IsPressed = _nameBox.IsFocused(e.Location);
                _descriptionBox.IsPressed = _descriptionBox.IsFocused(e.Location);
                _rowsBox.IsPressed = _rowsBox.IsFocused(e.Location);
                _columnsBox.IsPressed = _columnsBox.IsFocused(e.Location);
                _createBtn.IsPressed = _createBtn.IsFocused(e.Location);
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _nameBox.IsSelected = _nameBox.IsFocused(e.Location);
            _descriptionBox.IsSelected = _descriptionBox.IsFocused(e.Location);
            _rowsBox.IsSelected = _rowsBox.IsFocused(e.Location);
            _columnsBox.IsSelected = _columnsBox.IsFocused(e.Location);
            _createBtn.IsSelected = _createBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_createBtn.IsPressed)
            {
                int rows = int.TryParse(_rowsBox.Text, out int res) ? res : 0;
                int cols = int.TryParse(_columnsBox.Text, out res) ? res : 0;

                if (rows < Map.MinLength || rows > Map.MaxLength)
                {
                    _rowsBox.BackColor = Color.FromArgb(50, Color.DarkRed);
                }

                else if (cols < Map.MinLength || cols > Map.MaxLength)
                {
                    _columnsBox.BackColor = Color.FromArgb(50, Color.DarkRed);
                }

                else OnCreateClick(new MapCreatingArgs(
                    _nameBox.Text, new Size(rows, cols), _descriptionBox.Text));
            }

            _createBtn.IsPressed = false;
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            foreach (var box in _letterBoxes)
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
                }
            }

            foreach (var box in _digitBoxes)
            {
                if (box.IsPressed)
                {
                    box.BackColor = Color.FromArgb(180, Color.White);
                    if (char.IsDigit(e.KeyChar))
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
                }
            }
        }

        private void OnCreateClick(MapCreatingArgs e)
        {
            CreateMapClicked?.Invoke(this, e);
        }
    }
}
