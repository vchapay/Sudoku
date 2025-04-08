using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Sudoku.MapGraphics
{
    /// <summary>
    /// Предоставляет средство для отрисовки объектов Map.
    /// </summary>
    internal class MapEditorDisplayDrawer : Drawer
    {
        private Map _target;
        private SolidBrush _hiddenSolutionsBrush;

        public MapEditorDisplayDrawer(Map map, RectangleF display)
        {
            _target = map ?? throw new ArgumentNullException();
            Size mapSize = new Size(map.RowsCount, map.ColumnsCount);
            ConstructImage(display, mapSize);
            InitializeGraphics();
        }

        /// <summary>
        /// Возвращает или задает объект карты, который нужно нарисовать.
        /// </summary>
        public Map Target
        {
            get
            {
                return _target;
            }
            set
            {
                if (value != null)
                {
                    _target = value;
                    Size mapSize = new Size(_target.RowsCount, _target.ColumnsCount);
                    ConstructImage(_display, mapSize);
                }
            }
        }

        /// <summary>
        /// Возвращает или задает прямоугольник холста.
        /// </summary>
        public RectangleF Display
        {
            get
            {
                return _display;
            }

            set
            {
                if (_display != value)
                {
                    _display = value;
                    Size mapSize = new Size(_target.RowsCount, _target.ColumnsCount);
                    ConstructImage(_display, mapSize);
                }
            }
        }

        public override void Draw(Graphics g)
        {
            var cells = _target.GetCells();
            var groups = _target.GetGroups();
            g.Clear(Color.White);
            DrawBackGround(g);
            DrawUnblockedCells(g, cells);
            DrawTips(g, cells);
            DrawHiddenSolutions(g, cells);
            DrawGroupsOutlines(g, cells, groups);
            DrawConflict(g, _target.GetConflicts());
            DrawSelections(g);
        }

        private void InitializeGraphics()
        {
            _hiddenSolutionsBrush =
                new SolidBrush(Color.LightGray);
        }

        private void DrawConflict(Graphics g, List<ConflictInfo> conflictInterfaces)
        {
            foreach (var conflict in conflictInterfaces)
            {
                foreach (var cell in conflict.Cells)
                {
                    RectangleF rect = ConstructCellRect(cell.Row, cell.Column);
                    g.FillRectangle(_conflictCellBrush, rect);
                    g.DrawRectangle(_conflictCellPen, rect.X,
                        rect.Y, rect.Width, rect.Height);
                }
            }
        }

        private void DrawHiddenSolutions(Graphics g, List<CellInfo> cells)
        {
            foreach (var cell in cells)
            {
                if (cell.IsAvailable)
                {
                    RectangleF rect = ConstructCellRect(cell.Row, cell.Column);

                    if (cell.Solution != 0)
                    {
                        g.DrawString(cell.Solution.ToString(), SolutionsFont,
                            _hiddenSolutionsBrush, rect, _textFormat);
                    }
                }
            }
        }

        private void DrawSelections(Graphics g)
        {
            var selected = _target.GetSelectedCells();
            _selectedContent.Clear();
            List<List<PointF>> outline;
            if (selected.Count() > 0)
            {
                outline = ConstructOutline(_selectedOutlineIndent, selected.ToList());
                foreach (var pts in outline)
                {
                    g.DrawPolygon(_cellSelectionPen, pts.ToArray());
                }

                foreach (var cell in selected)
                {
                    if (_target.IsCellConflict(cell.Row, cell.Column))
                        continue;

                    RectangleF cellRect = ConstructCellRect(cell.Row, cell.Column);
                    g.FillRectangle(_cellSelectionBrush, cellRect);
                    if (_selectedContent.Add(cell.Solution))
                    {
                        SelectContent(g, cell.Solution);
                    }
                }
            }

            if (selected.All(c => c.IsBlocked))
                return;

            var associated = _target.GetAssociatedCells(selected.ToArray());
            associated = associated.Where(c => !c.IsBlocked);

            if (!associated.Any())
                return;

            outline = ConstructOutline(_associatedOutlineIndent, associated.ToList());
            GraphicsPath fillPath = new GraphicsPath();
            foreach (var pts in outline)
            {
                fillPath.AddPolygon(pts.ToArray());
                fillPath.CloseFigure();
            }

            fillPath.FillMode = FillMode.Alternate;
            g.FillPath(_cellSelectionBrush, fillPath);
        }

        private void SelectContent(Graphics g, int solution)
        {
            if (solution == 0)
                return;

            var targets = _target.GetCellsBySolution(solution);
            foreach (var cell in targets)
            {
                if (cell.IsSelected)
                    continue;

                RectangleF compRect = ConstructCellRect(cell.Row, cell.Column);
                g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
            }
        }
    }
}
