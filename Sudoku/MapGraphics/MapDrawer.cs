using Sudoku.MapLogic;
using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Sudoku.MapGraphics
{
    /// <summary>
    /// Предоставляет средство для отображения карт судоку
    /// </summary>
    internal class MapDrawer
    {
        private const int _defaultMainCellFontSize = 26;
        private const float _sumAreaOutlineIndent = 0.07f;
        private const int _defaultSumAreaTextSize = 11;
        private const float _defaultGridPenWidth = 0.3f;
        private const float _defaultBasicAreaPenWidth = 2.5f;
        private const float _defaultSumAreaPenWidth = 1f;
        private float _cellSize;
        private float _imageWidth;
        private float _imageHeight;
        private PointF _imagePosition;
        private Size _mapSize;
        private Font _font;
        private Font _sumAreaFont;
        private readonly Brush _openedValuesBrush;
        private readonly Brush _enteredValuesBrush;
        private readonly Brush _incorrectValuesTextBrush;
        private readonly Brush _incorrectValuesFillBrush;
        private readonly Brush _cellSelectionBrush;
        private readonly Brush _sameContentCellsSelectionBrush;
        private readonly Brush _basicAreaSelectionBrush;
        private readonly Brush _rowsColsSelectionBrush;
        private readonly Brush _sumAreaSelectionBrush;
        private readonly Pen _cellSelectionPen;
        private readonly Pen _defaultAreaPen;
        private readonly Pen _sumAreaPen;
        private readonly Pen _gridPen;
        private readonly Pen _outlinePen;
        private readonly StringFormat _mainCellsTextFormat;

        /// <summary>
        /// Создает и инициализирует средства отображения по умолчанию
        /// </summary>
        public MapDrawer()
        {
            _gridPen = new Pen(Color.Gray)
            {
                Width = _defaultGridPenWidth
            };
            _outlinePen = new Pen(Color.DarkGray);
            _font = new Font("Times New Roman", _defaultMainCellFontSize);
            _mainCellsTextFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            _sumAreaFont = new Font("Times New Roman", _defaultSumAreaTextSize);
            _openedValuesBrush = new SolidBrush(Color.Black);
            _enteredValuesBrush = new SolidBrush(Color.DarkBlue);
            _incorrectValuesTextBrush = new SolidBrush(Color.DarkRed);
            _incorrectValuesFillBrush = new SolidBrush(Color.FromArgb(45, 240, 60, 0));
            _defaultAreaPen = new Pen(Color.Black)
            {
                Width = _defaultBasicAreaPenWidth,
            };
            _sumAreaPen = new Pen(Color.Black)
            {
                Width = _defaultSumAreaPenWidth,
                DashStyle = DashStyle.Dash,
                DashPattern = new float[]
                {
                    0.8f,
                    0.4f,
                }
            };
            _cellSelectionBrush = new SolidBrush(Color.FromArgb(50, 40, 240, 10));
            _cellSelectionPen = new Pen(Color.FromArgb(150, 50, 120, 10))
            {
                Width = 4f
            };
            _rowsColsSelectionBrush = new SolidBrush(Color.FromArgb(20, 0, 190, 220));
            _sumAreaSelectionBrush = new SolidBrush(Color.FromArgb(30, 0, 250, 50));
            _sameContentCellsSelectionBrush = new SolidBrush(Color.FromArgb(50, 0, 240, 200));
            _basicAreaSelectionBrush = new SolidBrush(Color.FromArgb(20, 0, 190, 220));
        }

        public Font Font
        {
            get { return _font; }
            set { _font = value ?? _font; }
        }

        /// <summary>
        /// Отображает карту, используя заданный объект Graphics
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Draw(Graphics g, Map map)
        {
            if (map == null || g == null)
                throw new ArgumentException();

            var cells = map.GetCells();

            _mapSize = new Size(map.Width, map.Height);
            g.Clear(Color.White);
            ConstructBase(g.ClipBounds.Size, _mapSize);
            DrawGrid(g);
            var selectedCells = cells.Where(c => c.IsSelected);
            DrawSelections(g, map.GetCellsInArea, cells, false, false, false);
            DrawCellsContent(g, cells);
            DrawAreas(g, map.GetAreas(), map.GetCellsInArea);
        }

        /// <summary>
        /// Отображает карту, используя заданный объект Graphics
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Draw(Graphics g, MapInterface map)
        {
            if (map == null || g == null)
                throw new ArgumentException();

            _mapSize = new Size(map.Width, map.Height);
            g.Clear(Color.White);
            ConstructBase(g.ClipBounds.Size, _mapSize);
            DrawGrid(g);
            DrawAreas(g, map.Areas, map.GetAreaCells);
            DrawSelections(g, map.GetAreaCells, map.Cells, true, true, true);
            DrawCellsContent(g, map.Cells);
        }

        /// <summary>
        /// Возвращает координаты ячейки, содержащей пиксель с заданными координатами
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Структура Point, где X - столбец, а Y - строка заданной карты.</returns>
        public Point GetCell(float x, float y, Size map, Size display)
        {
            ConstructBase(display, map);

            int col = (int)((x - _imagePosition.X) / _cellSize);
            int row = (int)((y - _imagePosition.Y) / _cellSize);

            return new Point(col, row);
        }

        private void ConstructBase(SizeF display, Size map)
        {
            _cellSize = FindCellSize(display, map);

            _imageWidth = _cellSize * map.Width;
            _imageHeight = _cellSize * map.Height;

            float xIndent = (display.Width - _imageWidth) / 2;
            float yIndent = (display.Height - _imageHeight) / 2;
            _imagePosition = new PointF(xIndent, yIndent);
        }

        private float FindCellSize(SizeF display, Size map)
        {
            float smallerControlSide = display.Width > display.Height
                ? display.Height : display.Width;
            int biggerMapSide = map.Width < map.Height
                ? map.Height : map.Width;

            return smallerControlSide / biggerMapSide;
        }

        private void DrawGrid(Graphics g)
        {
            SizeF displaySize = g.ClipBounds.Size;

            g.DrawRectangle(_outlinePen, 0, 0, displaySize.Width - 1, displaySize.Height - 1);

            g.DrawRectangle(_outlinePen, _imagePosition.X, _imagePosition.Y,
                _imageWidth, _imageHeight);

            for (int x = 0; x < _mapSize.Width; x++)
            {
                float beginningX = _imagePosition.X + x * _cellSize;
                float beginningY = _imagePosition.Y;
                float endingX = _imagePosition.X + x * _cellSize;
                float endingY = _imagePosition.Y + _mapSize.Height * _cellSize;

                g.DrawLine(_gridPen, beginningX, beginningY, endingX, endingY);
            }

            for (int y = 0; y < _mapSize.Height; y++)
            {
                float beginningX = _imagePosition.X;
                float beginningY = _imagePosition.Y + y * _cellSize;
                float endingX = _imagePosition.X + _mapSize.Width * _cellSize;
                float endingY = _imagePosition.Y + y * _cellSize;

                g.DrawLine(_gridPen, beginningX, beginningY, endingX, endingY);
            }
        }

        private void DrawCellsContent(Graphics g, IReadOnlyCollection<CellInterface> cells)
        {
            foreach (var cell in cells)
            {
                RectangleF rect = new RectangleF()
                {
                    X = _imagePosition.X + cell.Column * _cellSize,
                    Y = _imagePosition.Y + cell.Row * _cellSize,
                    Width = _cellSize,
                    Height = _cellSize
                };

                if (!cell.IsAvailable && cell.Correct != 0)
                {
                    g.DrawString(cell.Correct.ToString(), _font, _openedValuesBrush,
                        rect, _mainCellsTextFormat);
                }

                if (cell.IsAvailable && cell.Entered != 0)
                {
                    var brush = _enteredValuesBrush;

                    if (cell.IsThereMistake)
                        brush = _incorrectValuesTextBrush;

                    g.DrawString(cell.Entered.ToString(), _font, brush,
                        rect, _mainCellsTextFormat);
                }
            }
        }

        private void DrawAreas(Graphics g, 
            IReadOnlyCollection<AreaInterface> areas, Func<int, List<CellInterface>> areaCells)
        {
            foreach (var area in areas)
            {
                bool isSumArea = area.Type == Map.AreaType.Sum;

                Pen pen = _defaultAreaPen;

                var cells = areaCells(area.ID);
                var outline = ConstructOutline(isSumArea, cells);

                if (isSumArea)
                    pen = _sumAreaPen;

                foreach (var pts in outline)
                {
                    g.DrawLines(pen, pts.ToArray());
                }

                if (isSumArea)
                {
                    PointF first = outline.First().First();

                    RectangleF textRect = new RectangleF
                    {
                        X = first.X - 5,
                        Y = first.Y - 3,
                        Width = 22,
                        Height = 15
                    };

                    RectangleF backRect = new RectangleF
                    {
                        X = first.X - 3,
                        Y = first.Y - 3,
                        Width = 20,
                        Height = 15
                    };

                    g.FillEllipse(Brushes.White, backRect);
                    g.DrawString(area.Sum.ToString(), _sumAreaFont, _openedValuesBrush,
                        textRect, _mainCellsTextFormat);
                }
            }
        }

        private List<List<PointF>> ConstructOutline(bool isSumArea, List<CellInterface> cells)
        {
            var points = ConstructPoints(cells, isSumArea);
            List<List<PointF>> outline = Arrange(points);
            return outline;
        }

        private Dictionary<PointF, NextDirection> ConstructPoints(List<CellInterface> cells, bool isSumArea)
        {
            NextDirection direction;
            var points = new Dictionary<PointF, NextDirection>();

            foreach (var cell in cells)
            {
                bool isCellInTop = cells.Find(
                    c => cell.Row - 1 == c.Row && c.Column == cell.Column) != null;
                bool isCellInLeft = cells.Find(
                    c => cell.Column - 1 == c.Column && c.Row == cell.Row) != null;
                bool isCellInRight = cells.Find(
                    c => cell.Column + 1 == c.Column && c.Row == cell.Row) != null;
                bool isCellInBottom = cells.Find(
                    c => cell.Row + 1 == c.Row && c.Column == cell.Column) != null;

                bool isCellInTopRight = cells.Find(
                    c => cell.Row - 1 == c.Row && c.Column == cell.Column + 1) != null;
                bool isCellInTopLeft = cells.Find(
                    c => cell.Column - 1 == c.Column && c.Row == cell.Row - 1) != null;
                bool isCellInBottomRight = cells.Find(
                    c => cell.Column + 1 == c.Column && c.Row == cell.Row + 1) != null;
                bool isCellInBottomLeft = cells.Find(
                    c => cell.Row + 1 == c.Row && c.Column == cell.Column - 1) != null;

                if ((isCellInTop == isCellInLeft &&
                    !isCellInTop) || (isCellInTop == isCellInLeft &&
                    isCellInTop && !isCellInTopLeft))
                {
                    PointF newP = new PointF(cell.Column * _cellSize + _imagePosition.X,
                        cell.Row * _cellSize + _imagePosition.Y);

                    if (isSumArea)
                    {
                        newP.X += _sumAreaOutlineIndent * _cellSize;
                        newP.Y += _sumAreaOutlineIndent * _cellSize;
                    }

                    if (!isCellInTop)
                        direction = NextDirection.Right;
                    else direction = NextDirection.Up;

                    points.Add(newP, direction);
                }

                if ((isCellInTop == isCellInRight
                    && !isCellInTop) || (isCellInTop == isCellInRight
                    && isCellInTop && !isCellInTopRight))
                {
                    PointF newP = new PointF((cell.Column + 1) * _cellSize + _imagePosition.X,
                        cell.Row * _cellSize + _imagePosition.Y);

                    if (isSumArea)
                    {
                        newP.X -= _sumAreaOutlineIndent * _cellSize;
                        newP.Y += _sumAreaOutlineIndent * _cellSize;
                    }

                    if (!isCellInTop)
                        direction = NextDirection.Down;
                    else direction = NextDirection.Right;

                    points.Add(newP, direction);
                }

                if ((isCellInBottom == isCellInLeft &&
                    !isCellInBottom) || (isCellInBottom == isCellInLeft
                    && isCellInBottom && !isCellInBottomLeft))
                {
                    PointF newP = new PointF(cell.Column * _cellSize + _imagePosition.X,
                        (cell.Row + 1) * _cellSize + _imagePosition.Y);

                    if (isSumArea)
                    {
                        newP.X += _sumAreaOutlineIndent * _cellSize;
                        newP.Y -= _sumAreaOutlineIndent * _cellSize;
                    }

                    if (!isCellInBottom)
                        direction = NextDirection.Up;
                    else direction = NextDirection.Left;

                    points.Add(newP, direction);
                }

                if ((isCellInBottom == isCellInRight &&
                    !isCellInBottom) || (isCellInBottom == isCellInRight
                    && isCellInBottom && !isCellInBottomRight))
                {
                    PointF newP = new PointF((cell.Column + 1) * _cellSize + _imagePosition.X,
                        (cell.Row + 1) * _cellSize + _imagePosition.Y);

                    if (isSumArea)
                    {
                        newP.X -= _sumAreaOutlineIndent * _cellSize;
                        newP.Y -= _sumAreaOutlineIndent * _cellSize;
                    }

                    if (!isCellInBottom)
                        direction = NextDirection.Left;
                    else direction = NextDirection.Down;

                    points.Add(newP, direction);
                }
            }

            return points;
        }

        private List<List<PointF>> Arrange(Dictionary<PointF, NextDirection> points)
        {
            List<List<PointF>> outline = new List<List<PointF>>();
            List<PointF> path = new List<PointF>();
            outline.Add(path);
            path.Add(points.First().Key);
            var first = points.First();
            var cur = first;
            PointF curP = cur.Key;
            PointF nextP;

            while (true)
            {
                nextP = FindNextPoint(curP, points.Keys.ToList(), cur.Value);

                if (nextP == PointF.Empty)
                {
                    path.Add(first.Key);
                    break;
                }

                if (nextP == first.Key)
                {
                    points.Remove(nextP);
                    points.Remove(curP);
                    path.Add(nextP);

                    if (points.Count > 0)
                    {
                        first = points.First();
                        cur = first;
                        curP = cur.Key;
                        path = new List<PointF>();
                        outline.Add(path);
                        path.Add(first.Key);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                path.Add(nextP);

                if (curP != first.Key)
                    points.Remove(curP);

                curP = nextP;
                cur = new KeyValuePair<PointF, NextDirection>(curP, points[curP]);
            }

            return outline;
        }

        private PointF FindNextPoint(PointF curP, 
            List<PointF> pts, NextDirection direction)
        {
            if (pts.Count < 2)
                return PointF.Empty;

            switch (direction)
            {
                case NextDirection.Up:
                    float nextY = pts.Where(p => p.X == curP.X 
                        && p.Y < curP.Y).Max(p => p.Y);
                    return new PointF(curP.X, nextY);
                case NextDirection.Down:
                    nextY = pts.Where(p => p.X == curP.X
                        && p.Y > curP.Y).Min(p => p.Y);
                    return new PointF(curP.X, nextY);
                case NextDirection.Left:
                    float nextX = pts.Where(p => p.Y == curP.Y
                        && p.X < curP.X).Max(p => p.X);
                    return new PointF(nextX, curP.Y);
                case NextDirection.Right:
                    nextX = pts.Where(p => p.Y == curP.Y
                        && p.X > curP.X).Min(p => p.X);
                    return new PointF(nextX, curP.Y);
            }

            return PointF.Empty;
        }

        private void DrawSelections(Graphics g, Func<int, List<CellInterface>> areaCells, IEnumerable<CellInterface> cells, 
            bool selectRowsAndCols, bool selectSameContent, bool selectAreas)
        {
            var selected = cells.Where(c => c.IsSelected);

            if (selected.Count() > 0)
            {
                foreach (var cell in selected)
                {
                    RectangleF cellRect = ConstructCellRect(cell);
                    g.FillRectangle(_cellSelectionBrush, cellRect);
                    g.DrawRectangle(_cellSelectionPen, cellRect.X, cellRect.Y,
                        cellRect.Width, cellRect.Height);

                    if (selectRowsAndCols)
                    {
                        DrawRowSelection(g, cells, cell.Row, cell.Column);
                        DrawColumnSelection(g, cells, cell.Column, cell.Row);
                    }

                    if (selectSameContent)
                    {
                        DrawSelectionSameContent(g, cell, cells);
                    }

                    if (selectAreas)
                    {
                        foreach (var area in cell.Areas)
                        {
                            DrawAreaSelection(g, areaCells.Invoke(area.ID),
                                area.Type == Map.AreaType.Sum);
                        }
                    }
                }
            }

            DrawMistakes(g, cells);
        }

        private void DrawMistakes(Graphics g, IEnumerable<CellInterface> cells)
        {
            foreach (var cell in cells)
            {
                if (cell.IsThereMistake && cell.IsAvailable)
                {
                    FillCell(g, cell, _incorrectValuesFillBrush);
                }
            }
        }

        private void DrawSelectionSameContent(Graphics g, CellInterface cell,
            IEnumerable<CellInterface> cells)
        {
            if (cell.IsAvailable && cell.Entered == 0)
                return;

            foreach (var comp in cells)
            {
                if (comp == cell)
                    continue;

                RectangleF compRect = ConstructCellRect(comp);

                if (cell.IsAvailable)
                {
                    if (comp.IsAvailable)
                    {
                        if (cell.Entered == comp.Entered)
                        {
                            g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
                        }
                    }

                    else
                    {
                        if (cell.Entered == comp.Correct)
                        {
                            g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
                        }
                    }
                }

                else
                {
                    if (comp.IsAvailable)
                    {
                        if (cell.Correct == comp.Entered)
                        {
                            g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
                        }
                    }

                    else
                    {
                        if (cell.Correct == comp.Correct)
                        {
                            g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
                        }
                    }
                }
            }
        }

        private void DrawColumnSelection(Graphics g, IEnumerable<CellInterface> cells, int column, int skipRow)
        {
            foreach (var cell in cells)
            {
                if (cell.IsSelected)
                    continue;

                if (cell.IsThereMistake)
                    continue;

                if (cell.Column == column && cell.Row != skipRow)
                {
                    FillCell(g, cell, _rowsColsSelectionBrush);
                }
            }
        }

        private void DrawRowSelection(Graphics g, IEnumerable<CellInterface> cells, 
            int row, int skipCol)
        {
            foreach (var cell in cells)
            {
                if (cell.IsSelected)
                    continue;

                if (cell.IsThereMistake)
                    continue;

                if (cell.Row == row && cell.Column != skipCol)
                {
                    FillCell(g, cell, _rowsColsSelectionBrush);
                }
            }
        }

        private void DrawAreaSelection(Graphics g, IEnumerable<CellInterface> cells, bool isSumArea)
        {
            foreach (CellInterface cell in cells)
            {
                if (cell.IsThereMistake)
                    continue;

                Brush brush = _basicAreaSelectionBrush;

                if (isSumArea)
                    brush = _sumAreaSelectionBrush;

                FillCell(g, cell, brush);
            }
        }

        private void FillCell(Graphics g, CellInterface cell, Brush brush)
        {
            RectangleF cellRect = ConstructCellRect(cell);
            g.FillRectangle(brush, cellRect);
        }

        private RectangleF ConstructCellRect(CellInterface cell)
        {
            float x = cell.Column * _cellSize + _imagePosition.X;
            float y = cell.Row * _cellSize + _imagePosition.Y;
            RectangleF cellRect = new RectangleF(x, y, _cellSize, _cellSize);
            return cellRect;
        }

        private enum NextDirection
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}
