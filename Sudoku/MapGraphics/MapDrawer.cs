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
    internal sealed class MapDrawer
    {
        private const int _defaultMainCellFontSize = 26;
        private const float _sumAreaOutlineIndent = 0.06f;
        private const float _selectedOutlineIndent = 0.02f;
        private const float _unavailbaleBoxOutlineIndent = 0.07f;
        private const int _defaultSumAreaTextSize = 11;
        private const float _defaultGridPenWidth = 0.3f;
        private const float _defaultBasicAreaPenWidth = 2.5f;
        private const float _defaultSumAreaPenWidth = 1f;
        private const int _defaultNoteFontSize = 8;
        private float _cellSize;
        private float _imageWidth;
        private float _imageHeight;
        private PointF _imagePosition;
        private Size _mapSize;
        private Font _font;
        private Font _sumAreaFont;
        private Font _notesFont;
        private readonly Brush _openedValuesBrush;
        private readonly Brush _enteredValuesBrush;
        private readonly Brush _unavailableValuesBrush;
        private readonly Brush _incorrectValuesTextBrush;
        private readonly Brush _incorrectValuesFillBrush;
        private readonly Brush _cellSelectionBrush;
        private readonly Brush _sameContentCellsSelectionBrush;
        private readonly Brush _basicAreaSelectionBrush;
        private readonly Brush _rowsColsSelectionBrush;
        private readonly Brush _sumAreaSelectionBrush;
        private readonly Brush _conflictCellBrush;
        private readonly Brush _notesBrush;
        private readonly HatchBrush _bgHatchBrush;
        private LinearGradientBrush _bgGradientBrush;
        private RectangleF _canvasRect;
        private readonly Pen _cellSelectionPen;
        private readonly Pen _defaultAreaPen;
        private readonly Pen _sumAreaPen;
        private readonly Pen _selectedSumAreaPen;
        private readonly Pen _gridPen;
        private readonly Pen _outlinePen;
        private readonly Pen _unavailableIconPen;
        private readonly Pen _conflictCellPen;
        private readonly Pen _unblockedCellOutlinePen;
        private readonly Pen _mustToWriteOutlinePen;
        private readonly StringFormat _textFormat;
        private readonly List<int> _selectedContent = new List<int>();
        private readonly List<int> _selectedGroups = new List<int>();

        /// <summary>
        /// Создает и инициализирует средства отображения по умолчанию
        /// </summary>
        public MapDrawer()
        {
            _gridPen = new Pen(Color.LightGray)
            {
                Width = _defaultGridPenWidth
            };
            _outlinePen = new Pen(Color.DarkGray);
            _unblockedCellOutlinePen = new Pen(Color.FromArgb(120, 120, 120))
            {
                Width = 0.7f
            };
            _unavailableIconPen = new Pen(Color.LightGray);
            _font = new Font("Times New Roman", _defaultMainCellFontSize);
            _sumAreaFont = new Font("Times New Roman", _defaultSumAreaTextSize);
            _notesFont = new Font("Times New Roman", _defaultNoteFontSize);
            _textFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };
            _openedValuesBrush = new SolidBrush(Color.Black);
            _enteredValuesBrush = new SolidBrush(Color.DarkBlue);
            _unavailableValuesBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 100));
            _incorrectValuesTextBrush = new SolidBrush(Color.DarkRed);
            _incorrectValuesFillBrush = new SolidBrush(Color.FromArgb(45, 240, 60, 0));
            _conflictCellBrush = new SolidBrush(Color.FromArgb(60, 220, 90, 0));
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
                    5f,
                    4f
                }
            };
            _selectedSumAreaPen = new Pen(Color.Black)
            {
                Width = _defaultSumAreaPenWidth + 1,
            };
            _cellSelectionBrush = new SolidBrush(Color.FromArgb(20, 150, 170, 180));
            _cellSelectionPen = new Pen(Color.FromArgb(100, 100, 170, 200))
            {
                Width = 4f
            };
            _conflictCellPen = new Pen(Color.FromArgb(180, 100, 40, 0))
            {
                Width = 3,
                DashStyle = DashStyle.Dash
            };
            _mustToWriteOutlinePen = new Pen(Color.FromArgb(140, 150, 150, 180))
            {
                Width = 2.5f,
                DashStyle = DashStyle.Dot
            };
            _notesBrush = new SolidBrush(Color.DarkGray);
            _rowsColsSelectionBrush = new SolidBrush(Color.FromArgb(20, 0, 190, 220));
            _sumAreaSelectionBrush = new SolidBrush(Color.FromArgb(30, 0, 250, 50));
            _sameContentCellsSelectionBrush = new SolidBrush(Color.FromArgb(50, 0, 240, 200));
            _basicAreaSelectionBrush = new SolidBrush(Color.FromArgb(20, 150, 170, 180));
            _bgHatchBrush = new HatchBrush(HatchStyle.DiagonalCross, 
                Color.FromArgb(230, 230, 230), Color.White);
        }

        public Font Font
        {
            get { return _font; }
            set { _font = value ?? _font; }
        }

        /// <summary>
        /// Отображает карту, представленную объектом Map,
        /// используя заданный объект Graphics
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Draw(Graphics g, RectangleF surface, Map map)
        {
            if (map == null || g == null)
                throw new ArgumentNullException();

            var cells = map.GetCells();
            _mapSize = new Size(map.ColumnsCount, map.RowsCount);
            g.Clear(Color.White);
            ConstructBase(surface.Size, _mapSize);
            DrawBackGround(g);
            DrawCellsContent(g, cells);
            DrawGroups(g, map.GetCells(), map.GetGroups());
            DrawSelections(g, map, true, true);
            DrawConflict(g, map.GetConflicts());
        }

        /// <summary>
        /// Отображает карту, представленную объектом MapInterface,
        /// используя заданный объект Graphics
        /// </summary>
        /// <param name="g"></param>
        /// <param name="map"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Draw(Graphics g, RectangleF surface, MapInterface map)
        {
            if (map == null || g == null)
                throw new ArgumentNullException();

            _mapSize = new Size(map.ColumnsCount, map.RowsCount);
            g.Clear(Color.White);
            ConstructBase(surface.Size, _mapSize);
            DrawBackGround(g);
            var cells = map.Cells.Select(c => c.GetInfo());
            var groups = map.Groups.Select(gr => gr.GetInfo());
            DrawGroups(g, cells.ToList(), groups.ToList());
            DrawSelections(g, map, true, true, true);
            DrawNotes(g, map.Cells);
            DrawCellsContent(g, map.Cells);
        }

        /// <summary>
        /// Конструирует для карты заданного размера 
        /// прямоугольник изображения на полотне
        /// и находит ячейку, прямоугольнику которой принадлежит точка. 
        /// Координаты находится так, будто бы в том месте, 
        /// где находится целевая точка, ячейка на карте существует, 
        /// однако это может быть и не так.
        /// </summary>
        /// <param name="x"> Координата X целевой точки </param>
        /// <param name="y"> Координата Y целевой точки </param>
        /// <param name="map"> Размер карты (в ячейках) </param>
        /// <param name="display"> Размер полотна для отображения карты </param>
        /// <returns> Структура Point, где X 
        /// соответствует столбцу ячейки, а Y - строке. </returns>
        public Point GetCell(float x, float y, Size map, Size display)
        {
            ConstructBase(display, map);

            int col = (int)((x - _imagePosition.X) / _cellSize);
            int row = (int)((y - _imagePosition.Y) / _cellSize);

            return new Point(col, row);
        }

        /// <summary>
        /// Конструирует для карты заданного размера изображение на прямоугольнике
        /// и находит ячейки, прямоугольники которых содержат точки прямоугольника выделения. 
        /// Координаты находится так, будто бы в области выделения
        /// ячейки на карте существуют, однако это может быть и не так.
        /// </summary>
        /// <param name="rect"> Прямоугольник выделения </param>
        /// <param name="map"> Размер карты (в ячейках) </param>
        /// <param name="display"> Размер полотна для отображения карты </param>
        /// <returns> Список структур Point, в которых X 
        /// соответствует столбцу ячейки, а Y - строке. </returns>
        public List<Point> GetCells(RectangleF rect, Size map, SizeF display)
        {
            List<Point> cells = new List<Point>();
            ConstructBase(display, map);
            int colBeg = (int)((rect.X - _imagePosition.X) / _cellSize);
            int rowBeg = (int)((rect.Y - _imagePosition.Y) / _cellSize);
            int colEnd = (int)((rect.X + rect.Width - _imagePosition.X) / _cellSize);
            int rowEnd = (int)((rect.Y + rect.Height - _imagePosition.Y) / _cellSize);
            for (int r = rowBeg; r <= rowEnd; r++)
            {
                for (int c = colBeg; c <= colEnd; c++)
                {
                    cells.Add(new Point(c, r));
                }
            }
            return cells;
        }

        private void ConstructBase(SizeF display, Size map)
        {
            display.Width--;
            display.Height--;
            _cellSize = FindCellSize(display, map);

            _imageWidth = _cellSize * map.Width;
            _imageHeight = _cellSize * map.Height;

            float xIndent = (display.Width - _imageWidth) / 2;
            float yIndent = (display.Height - _imageHeight) / 2;
            _imagePosition = new PointF(xIndent, yIndent);

            _canvasRect = new RectangleF()
            {
                X = 0,
                Y = 0,
                Width = _imageWidth + _imagePosition.X * 2,
                Height = _imageHeight + _imagePosition.Y * 2
            };
        }

        private void DrawBackGround(Graphics g)
        {
            RectangleF gradRect = new RectangleF()
            {
                X = 0,
                Y = 0,
                Width = _imageWidth,
                Height = _imageHeight
            };

            _bgGradientBrush = new LinearGradientBrush(_canvasRect,
            Color.FromArgb(15, 250, 250, 250),
            Color.FromArgb(45, 200, 200, 240), 90);

            g.FillRectangle(_bgHatchBrush, _canvasRect);
            g.FillRectangle(_bgGradientBrush, _canvasRect);

            g.FillRectangle(Brushes.White, _imagePosition.X,
                _imagePosition.Y, _imageWidth, _imageHeight);
        }

        private float FindCellSize(SizeF display, Size map)
        {
            float smallerControlSide = display.Width > display.Height
                ? display.Height : display.Width;
            int biggerMapSide = map.Width < map.Height
                ? map.Height : map.Width;

            return smallerControlSide / biggerMapSide;
        }

        private void DrawNotes(Graphics g, IReadOnlyCollection<CellInterface> cells)
        {
            foreach (var cell in cells)
            {
                if (cell.IsEntered || !cell.IsAvailable)
                    continue;

                foreach (int note in cell.Notes)
                {
                    // для 20 возможных значений заметки
                    // нужен квадрат 4x5
                    int row = (note - 1) / 4;
                    int column = (note - 1) - 4 * row;
                    float noteRectWidth = _cellSize - 8;
                    float noteRectHeight = _cellSize - 16;
                    noteRectWidth = noteRectWidth > 0 ? noteRectWidth : _cellSize;
                    noteRectHeight = noteRectHeight > 0 ? noteRectHeight : _cellSize;
                    float noteCellWidth = noteRectWidth / 4;
                    float noteCellHeight = noteRectHeight / 5;
                    float cellX = cell.Column * _cellSize + _imagePosition.X + 4;
                    float cellY = cell.Row * _cellSize + _imagePosition.Y + 12;
                    float x = cellX + noteCellWidth * column;
                    float y = cellY + noteCellHeight * row;
                    RectangleF noteRect =
                        new RectangleF(x, y, noteCellWidth, noteCellHeight);
                    g.DrawString($"{note}", _notesFont, _notesBrush,
                        noteRect, _textFormat);
                }
            }
        }

        private void DrawConflict(Graphics g, List<ConflictInfo> conflictInterfaces)
        {
            foreach (var conflict in conflictInterfaces)
            {
                foreach (var cell in conflict.Cells)
                {
                    FillCell(g, cell, _conflictCellBrush);
                    DrawCellOutline(g, cell, _conflictCellPen);
                }
            }
        }

        private void DrawCellsContent(Graphics g,
            IReadOnlyCollection<CellInfo> cells)
        {
            foreach (var cell in cells)
            {
                RectangleF rect = ConstructCellRect(cell);

                if (!cell.IsAvailable && cell.Solution != 0)
                {
                    g.DrawString(cell.Solution.ToString(), _font, _openedValuesBrush,
                        rect, _textFormat);
                }

                if (cell.IsAvailable && cell.Solution != 0)
                {
                    g.DrawString(cell.Solution.ToString(), _font, _unavailableValuesBrush,
                        rect, _textFormat);
                }

                if (!cell.IsAvailable && cell.Solution == 0)
                {
                    DrawUnavailableIcon(g, rect);
                }

                else
                {
                    DrawCellOutline(g, cell, _unblockedCellOutlinePen);
                }

                if (cell.State == CellType.MustWrite)
                {
                    float width = rect.Width - 6;
                    float height = rect.Height - 6;
                    width = width > 0 ? width : 1;
                    height = height > 0 ? height : 1;
                    float x = rect.X + (rect.Width - width) / 2;
                    float y = rect.Y + (rect.Height - height) / 2;
                    g.DrawRectangle(_mustToWriteOutlinePen, x, y, width, height);
                }
            }
        }

        private void DrawCellsContent(Graphics g, 
            IReadOnlyCollection<CellInterface> cells)
        {
            foreach (var cell in cells)
            {
                RectangleF rect = ConstructCellRect(cell.GetInfo());

                if (!cell.IsAvailable && cell.Solution != 0)
                {
                    g.DrawString(cell.Solution.ToString(), _font, _openedValuesBrush,
                        rect, _textFormat);
                }

                if (!cell.IsAvailable && cell.Solution == 0)
                {
                    DrawUnavailableIcon(g, rect);
                }

                else
                {
                    DrawCellOutline(g, cell.GetInfo(), _unblockedCellOutlinePen);
                }

                if (cell.IsAvailable && cell.Entered != 0)
                {
                    var brush = _enteredValuesBrush;

                    if (cell.IsThereMistake)
                        brush = _incorrectValuesTextBrush;

                    g.DrawString(cell.Entered.ToString(), _font, brush,
                        rect, _textFormat);
                }

                if (cell.State == CellType.MustWrite)
                {
                    float width = rect.Width - 6;
                    float height = rect.Height - 6;
                    width = width > 0 ? width : 1;
                    height = height > 0 ? height : 1;
                    float x = rect.X + (rect.Width - width) / 2;
                    float y = rect.Y + (rect.Height - height) / 2;
                    g.DrawRectangle(_mustToWriteOutlinePen, x, y, width, height);
                }
            }
        }
        private void DrawUnavailableIcon(Graphics g, RectangleF rect)
        {
            float indent = _cellSize * _unavailbaleBoxOutlineIndent;
            rect.X += indent;
            rect.Y += indent;
            rect.Width -= indent * 2;
            rect.Height -= indent * 2;

            g.DrawRectangle(_unavailableIconPen, rect.X,
                rect.Y, rect.Width, rect.Height);

            g.DrawLine(_unavailableIconPen, rect.X,
                rect.Y, rect.X + rect.Width, rect.Y + rect.Height);

            g.DrawLine(_unavailableIconPen, rect.X,
                rect.Y + rect.Height, rect.X + rect.Width, rect.Y);
        }

        private void DrawGroups(Graphics g,
            List<CellInfo> cells, List<GroupInfo> groups)
        {
            List<int> drawnGroups = new List<int>();
            foreach (var gr in groups)
            {
                if (drawnGroups.Contains(gr.ID))
                    continue;

                var grCells = cells.Where(c => c.Groups.Contains(gr.ID));

                bool isSumGroup = gr.Type == GroupType.Sum;
                float indentCoef = 0;
                if (isSumGroup)
                    indentCoef = _sumAreaOutlineIndent;
                var outline = ConstructOutline(indentCoef, grCells.ToList());
                if (outline.Count == 0)
                    continue;

                Pen pen = _defaultAreaPen;
                if (isSumGroup)
                    if (gr.IsSelected)
                        pen = _selectedSumAreaPen;
                    else
                        pen = _sumAreaPen;

                foreach (var pts in outline)
                {
                    g.DrawLines(pen, pts.ToArray());
                }

                if (isSumGroup)
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
                    g.DrawString(gr.Sum.ToString(), _sumAreaFont, _openedValuesBrush,
                        textRect, _textFormat);
                }

                drawnGroups.Add(gr.ID);
            }
        }

        private List<List<PointF>> ConstructOutline(float indentCoef, List<CellInfo> cells)
        {
            var points = ConstructPoints(cells, indentCoef);
            List<List<PointF>> outline = Arrange(points);
            return outline;
        }

        private List<(PointF, NextDirection)> ConstructPoints(
            List<CellInfo> cells, float indentCoef)
        {
            NextDirection direction;
            var points = new List<(PointF, NextDirection)>();

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

                    newP.X += indentCoef * _cellSize;
                    newP.Y += indentCoef * _cellSize;

                    if (!isCellInTop)
                        direction = NextDirection.Right;
                    else direction = NextDirection.Up;

                    points.Add((newP, direction));
                }

                if ((isCellInTop == isCellInRight
                    && !isCellInTop) || (isCellInTop == isCellInRight
                    && isCellInTop && !isCellInTopRight))
                {
                    PointF newP = new PointF((cell.Column + 1) * _cellSize + _imagePosition.X,
                        cell.Row * _cellSize + _imagePosition.Y);

                    newP.X -= indentCoef * _cellSize;
                    newP.Y += indentCoef * _cellSize;

                    if (!isCellInTop)
                        direction = NextDirection.Down;
                    else direction = NextDirection.Right;

                    points.Add((newP, direction));
                }

                if ((isCellInBottom == isCellInLeft &&
                    !isCellInBottom) || (isCellInBottom == isCellInLeft
                    && isCellInBottom && !isCellInBottomLeft))
                {
                    PointF newP = new PointF(cell.Column * _cellSize + _imagePosition.X,
                        (cell.Row + 1) * _cellSize + _imagePosition.Y);

                    newP.X += indentCoef * _cellSize;
                    newP.Y -= indentCoef * _cellSize;

                    if (!isCellInBottom)
                        direction = NextDirection.Up;
                    else direction = NextDirection.Left;

                    points.Add((newP, direction));
                }

                if ((isCellInBottom == isCellInRight &&
                    !isCellInBottom) || (isCellInBottom == isCellInRight
                    && isCellInBottom && !isCellInBottomRight))
                {
                    PointF newP = new PointF((cell.Column + 1) * _cellSize + _imagePosition.X,
                        (cell.Row + 1) * _cellSize + _imagePosition.Y);

                    newP.X -= indentCoef * _cellSize;
                    newP.Y -= indentCoef * _cellSize;

                    if (!isCellInBottom)
                        direction = NextDirection.Left;
                    else direction = NextDirection.Down;

                    points.Add((newP, direction));
                }
            }

            return points;
        }

        private List<List<PointF>> Arrange(List<(PointF, NextDirection)> points)
        {
            List<List<PointF>> outline = new List<List<PointF>>();
            if (points == null || points.Count == 0)
                return outline;
            List<PointF> path = new List<PointF>();
            outline.Add(path);
            path.Add(points.First().Item1);
            var first = points.First();
            var cur = first;
            PointF curP = cur.Item1;
            PointF nextP;

            while (true)
            {
                nextP = FindNextPoint(curP, points.Select(p => p.Item1).ToList(), cur.Item2);

                if (nextP == PointF.Empty)
                {
                    path.Add(first.Item1);
                    break;
                }

                if (nextP == first.Item1)
                {
                    points.Remove(first);
                    points.Remove(points.Find(p => p.Item1 == curP));
                    path.Add(nextP);

                    if (points.Count > 0)
                    {
                        first = points.First();
                        cur = first;
                        curP = cur.Item1;
                        path = new List<PointF>();
                        outline.Add(path);
                        path.Add(first.Item1);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                path.Add(nextP);

                if (curP != first.Item1)
                    points.Remove(points.Find(p => p.Item1 == curP));

                curP = nextP;
                cur = (curP, points.Find(p => p.Item1 == curP).Item2);
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

        private void DrawSelections(Graphics g, Map map,
            bool selectSameContent, bool selectGroups)
        {
            var cells = map.GetCells();
            var selected = cells.Where(c => c.IsSelected);
            _selectedContent.Clear();
            _selectedGroups.Clear();
            if (selected.Count() > 0)
            {
                var outline = ConstructOutline(_selectedOutlineIndent, selected.ToList());
                foreach (var pts in outline)
                {
                    g.DrawLines(_cellSelectionPen, pts.ToArray());
                }

                foreach (var cell in selected)
                {
                    RectangleF cellRect = ConstructCellRect(cell);
                    g.FillRectangle(_cellSelectionBrush, cellRect);

                    if (selectSameContent)
                    {
                        int content = cell.Solution;
                        if (!_selectedContent.Contains(content))
                        {
                            DrawSelectionSameContent(g, content, cells);
                            _selectedContent.Add(content);
                        }
                    }

                    if (selectGroups)
                    {
                        foreach (var gID in cell.Groups)
                        {
                            if (_selectedGroups.Contains(gID))
                                continue;

                            GroupInfo group = map.GetGroups().Find(gr => gr.ID == gID);
                            var groupCells = map.GetCells().Where(c => c.Groups.Contains(gID));

                            DrawGroupSelection(g, groupCells, group.Type == GroupType.Sum);

                            _selectedGroups.Add(gID);
                        }
                    }
                }
            }
        }

        private void DrawSelections(Graphics g, MapInterface map, 
            bool selectRowsAndCols, bool selectSameContent, bool selectAreas)
        {
            var cells = map.Cells;
            var selected = cells.Where(c => c.IsSelected);
            _selectedContent.Clear();
            _selectedGroups.Clear();
            if (selected.Count() > 0)
            {
                List<CellInfo> selectedInfo = new List<CellInfo>();
                foreach (var s in selected)
                {
                    selectedInfo.Add(s.GetInfo());
                }

                var outline = ConstructOutline(_selectedOutlineIndent, selectedInfo.ToList());
                foreach (var pts in outline)
                {
                    g.DrawLines(_cellSelectionPen, pts.ToArray());
                }

                foreach (var cell in selected)
                {
                    RectangleF cellRect = ConstructCellRect(cell.GetInfo());
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
                        int content = cell.Solution;
                        if (cell.IsAvailable)
                            content = cell.Entered;
                        if (!_selectedContent.Contains(content))
                        {
                            DrawSelectionSameContent(g, content, cells);
                            _selectedContent.Add(content);
                        }
                    }

                    if (selectAreas)
                    {
                        foreach (var gID in cell.Groups)
                        {
                            if (_selectedGroups.Contains(gID))
                                continue;

                            GroupInterface group = map.Groups.Where(gr => gr.ID == gID).First();
                            var groupCells = map.Cells.Where(c => c.Groups.Contains(gID));

                            DrawGroupSelection(g, groupCells, group.Type == GroupType.Sum);

                            _selectedGroups.Add(gID);
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
                    FillCell(g, cell.GetInfo(), _incorrectValuesFillBrush);
                }
            }
        }

        private void DrawSelectionSameContent(Graphics g, int content,
            IEnumerable<CellInfo> cells)
        {
            if (content == 0)
                return;

            foreach (var cell in cells)
            {
                if (cell.IsSelected)
                    continue;

                RectangleF compRect = ConstructCellRect(cell);

                if (cell.Solution == content)
                {
                    g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
                }
            }
        }

        private void DrawSelectionSameContent(Graphics g, int content,
            IEnumerable<CellInterface> cells)
        {
            if (content == 0)
                return;

            foreach (var cell in cells)
            {
                if (cell.IsSelected)
                    continue;

                RectangleF compRect = ConstructCellRect(cell.GetInfo());

                if (cell.IsAvailable)
                {
                    if (cell.Entered == content)
                    {
                        g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
                    }
                }

                if (!cell.IsAvailable)
                {
                    if (cell.Solution == content)
                    {
                        g.FillRectangle(_sameContentCellsSelectionBrush, compRect);
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
                    FillCell(g, cell.GetInfo(), _rowsColsSelectionBrush);
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
                    FillCell(g, cell.GetInfo(), _rowsColsSelectionBrush);
                }
            }
        }

        private void DrawGroupSelection(Graphics g, IEnumerable<CellInfo> cells, bool isSumArea)
        {
            foreach (CellInfo cell in cells)
            {
                Brush brush = _basicAreaSelectionBrush;

                if (isSumArea)
                    brush = _sumAreaSelectionBrush;

                FillCell(g, cell, brush);
            }
        }

        private void DrawGroupSelection(Graphics g, IEnumerable<CellInterface> cells, bool isSumArea)
        {
            foreach (CellInterface cell in cells)
            {
                if (cell.IsThereMistake)
                    continue;

                Brush brush = _basicAreaSelectionBrush;

                if (isSumArea)
                    brush = _sumAreaSelectionBrush;

                FillCell(g, cell.GetInfo(), brush);
            }
        }

        private void FillCell(Graphics g, CellInfo cell, Brush brush)
        {
            RectangleF cellRect = ConstructCellRect(cell);
            g.FillRectangle(brush, cellRect);
        }

        private void DrawCellOutline(Graphics g, CellInfo cell, Pen pen)
        {
            RectangleF cellRect = ConstructCellRect(cell);
            g.DrawRectangle(pen, cellRect.X, cellRect.Y,
                cellRect.Width, cellRect.Height);
        }

        private RectangleF ConstructCellRect(CellInfo cell)
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
