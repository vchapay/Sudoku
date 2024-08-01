using Sudoku.MapGraphics;
using Sudoku.MapLogic;
using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku.SpecialControls
{
    internal class MapDisplay : Control
    {
        private MapInterface _dataSource;
        private MapDrawer _drawer;

        public MapDisplay() 
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
        }

        public MapInterface DataSource
        {
            get { return _dataSource; } 
            set { _dataSource = value; }
        }

        public Font OpenedCellsFont
        {
            get => _drawer.Font;
            set {
                _drawer.Font = value ?? _drawer.Font;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _drawer.Draw(e.Graphics, _dataSource);
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            Invalidate();
            base.OnFontChanged(e);
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
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Delete)
            {
                _dataSource.Write(0);
            }

            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (char.IsDigit(e.KeyChar))
            {
                _dataSource.Write(int.Parse(e.KeyChar.ToString()));
            }

            Invalidate();
        }
    }
}
