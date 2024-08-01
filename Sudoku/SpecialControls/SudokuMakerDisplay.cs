using Sudoku.MapGraphics;
using Sudoku.MapLogic;
using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku.SpecialControls
{
    internal class SudokuMakerDisplay : Control
    {
        private Map _dataSource;
        private readonly MapDrawer _drawer;
        private PointF _selectionRectBegin;
        private PointF _selectionRectEnd;
        private RectangleF _selectionRect;
        private bool _isSelecting;
        private Pen _selectionRectPen;

        public SudokuMakerDisplay()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
            Map map = new Map();
            //map.FillWithDefaultValues();
            _drawer = new MapDrawer();
            _dataSource = map;
            Width = 350;
            Height = 350;
            Font = _drawer.Font;
            _selectionRectPen = new Pen(Color.FromArgb(150, 50, 150, 0))
            {
                Width = 1.5f
            };
        }

        public Map DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            _drawer.Draw(e.Graphics, _dataSource);
            if (_isSelecting)
            {
                ConstructSelectionRect();

                e.Graphics.DrawRectangle(_selectionRectPen, 
                    _selectionRect.X, _selectionRect.Y,
                    _selectionRect.Width, _selectionRect.Height);
            }
        }

        private void ConstructSelectionRect()
        {
            float width = Math.Abs(_selectionRectBegin.X - _selectionRectEnd.X);
            float height = Math.Abs(_selectionRectBegin.Y - _selectionRectEnd.Y);
            float beginX = _selectionRectBegin.X < _selectionRectEnd.X ?
                                _selectionRectBegin.X : _selectionRectEnd.X;
            float beginY = _selectionRectBegin.Y < _selectionRectEnd.Y ?
                                _selectionRectBegin.Y : _selectionRectEnd.Y;

            _selectionRect = new RectangleF(beginX, beginY, width, height);
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!_isSelecting)
                    _selectionRectBegin = e.Location;

                _isSelecting = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _selectionRectEnd = e.Location;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _selectionRectEnd = e.Location;
                ConstructSelectionRect();
                Size mapSize = new Size(_dataSource.Width, _dataSource.Height);
                List<Point> cells = _drawer.GetCells(_selectionRect, mapSize, Size);
                foreach (var cell in cells)
                {
                    if (cell.X > -1 && cell.X < _dataSource.Width
                    && cell.Y > -1 && cell.Y < _dataSource.Height)
                    {
                        _dataSource.ChangeCellSelection(cell.X, cell.Y);
                    }
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                _dataSource.ClearSelection();
            }

            _isSelecting = false;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        _dataSource.AddSelectedToGroup();
                        break;
                    case Keys.R:
                        _dataSource.RemoveSelectedFromGroup();
                        break;
                    case Keys.Z:
                        _dataSource.Undo();
                        break;
                    case Keys.Y:
                        _dataSource.Redo();
                        break;
                }
            }

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    _dataSource.Write(0);
                    break;
                case Keys.G:
                    _dataSource.GroupSelected(Map.GroupType.Basic);
                    break;
                case Keys.S:
                    _dataSource.GroupSelected(Map.GroupType.Sum);
                    break;
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
