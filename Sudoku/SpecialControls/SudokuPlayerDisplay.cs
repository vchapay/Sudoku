using Sudoku.MapGraphics;
using Sudoku.MapLogic;
using Sudoku.MapPlayingLogic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku.SpecialControls
{
    internal sealed class SudokuPlayerDisplay : Control
    {
        private MapInterface _dataSource;
        private readonly MapDrawer _drawer;

        public SudokuPlayerDisplay() 
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;

            Map map = new Map();
            map.FillWithDefaultValues();
            _drawer = new MapDrawer();
            _dataSource = map.GetInterface();
            Width = 350;
            Height = 350;
            Font = _drawer.Font;
        }

        public MapInterface DataSource
        {
            get { return _dataSource; } 
            set { _dataSource = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            _drawer.Draw(e.Graphics, ClientRectangle, _dataSource);
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            _drawer.Font = Font;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _dataSource.ClearSelection();
            Size mapSize = new Size(_dataSource.Width, _dataSource.Height);
            Point pos = _drawer.GetCell(e.X, e.Y, mapSize, Size);
            if (pos.X > -1 && pos.X < _dataSource.Width
                && pos.Y > -1 && pos.Y < _dataSource.Height)
            {
                _dataSource.ChangeCellSelection(pos.Y, pos.X);
            }

            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                _dataSource.Write(0);
            }

            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                _dataSource.Write(int.Parse(e.KeyChar.ToString()));
            }

            Invalidate();
        }
    }
}
