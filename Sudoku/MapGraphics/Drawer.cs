using Sudoku.MapLogic;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Sudoku.MapGraphics
{
    internal abstract class Drawer
    {
        protected const int _defaultOpenedValuesFontSize = 26;
        protected const float _sumGroupOutlineIndent = 0.06f;
        protected const float _selectedOutlineIndent = 0.06f;
        protected const float _associatedOutlineIndent = 0.15f;
        protected const float _unavailbaleBoxOutlineIndent = 0.07f;
        protected const int _defaultSumGroupTextSize = 11;
        protected const float _defaultBasicGroupPenWidth = 2.5f;
        protected const float _defaultSelectionPenWidth = 8f;
        protected const float _defaultSumGroupPenWidth = 1f;
        private const float _mustWritePenWidth = 2.8f;
        private const int conflictPenWidth = 3;
        private const int _mustWriteIconIndent = 8;
        protected RectangleF _display;
        protected RectangleF _imageRect;
        protected float _cellSize;
        protected float _imageWidth;
        protected float _imageHeight;
        protected PointF _imagePosition;
        protected Size _mapSize;

        protected readonly SolidBrush _unblockedCellBrush;
        protected readonly SolidBrush _cellSelectionBrush;
        protected readonly SolidBrush _sumGroupSelectionBrush;
        protected readonly SolidBrush _tipsBrush;
        protected readonly SolidBrush _sumGroupValuesBrush;
        protected readonly SolidBrush _sameContentCellsSelectionBrush;
        private Font _solutionsFont;

        protected readonly SolidBrush _conflictCellBrush;
        protected readonly Pen _conflictCellPen;

        protected readonly Pen _mapOutlinePen;
        protected readonly Pen _unblockedCellPen;
        protected readonly Pen _unavailableIconPen;
        protected readonly Pen _mustWriteOutlinePen;

        protected readonly Pen _cellSelectionPen;
        protected readonly Pen _basicGroupPen;
        protected readonly Pen _sumGroupPen;
        protected readonly Pen _selectedSumGroupPen;

        private Font _sumGroupFont;

        protected readonly StringFormat _textFormat;

        private readonly HatchBrush _bgHatchBrush;
        private LinearGradientBrush _bgGradientBrush;

        private readonly float[] _mustWriteDash = new float[] { 6f, 4f, 1, 1 };
        private readonly float[] _sumGroupDash = new float[] { 5f, 4f };
        protected readonly HashSet<int> _selectedContent =
            new HashSet<int>();

        public Drawer() 
        {
            _unblockedCellPen = new Pen(Color.Gray);

            _unblockedCellBrush = new SolidBrush(Color.White);

            _unavailableIconPen = new Pen(Color.LightGray);

            _solutionsFont = new Font("Times New Roman", _defaultOpenedValuesFontSize);

            _sumGroupFont = new Font("Times New Roman", _defaultSumGroupTextSize);

            _textFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None,
            };

            _tipsBrush = new SolidBrush(Color.Black);

            _sumGroupValuesBrush = new SolidBrush(Color.Black);

            _conflictCellBrush = new SolidBrush(Color.FromArgb(60, 220, 90, 0));

            _sameContentCellsSelectionBrush =
                new SolidBrush(Color.FromArgb(50, 0, 240, 200));

            _basicGroupPen = new Pen(Color.Black)
            {
                Width = _defaultBasicGroupPenWidth,
            };

            _sumGroupPen = new Pen(Color.Black)
            {
                Width = _defaultSumGroupPenWidth,
                DashStyle = DashStyle.Dash,
                DashPattern = _sumGroupDash
            };

            _selectedSumGroupPen = new Pen(Color.Black)
            {
                Width = _defaultSumGroupPenWidth + 1,
            };

            _cellSelectionBrush = new SolidBrush(Color.FromArgb(60, 180, 190, 230));

            _sumGroupSelectionBrush = new SolidBrush(Color.FromArgb(60, 110, 200, 200));

            _cellSelectionPen = new Pen(Color.FromArgb(150, 50, 160, 200))
            {
                Width = _defaultSelectionPenWidth
            };

            _conflictCellPen = new Pen(Color.FromArgb(180, 100, 40, 0))
            {
                Width = conflictPenWidth,
                DashStyle = DashStyle.Dash
            };

            _mustWriteOutlinePen = new Pen(Color.FromArgb(140, 150, 150, 180))
            {
                Width = _mustWritePenWidth,
                DashPattern = _mustWriteDash
            };

            _bgHatchBrush = new HatchBrush(HatchStyle.DiagonalCross,
                Color.FromArgb(150, 230, 230, 230), Color.White);
        }

        /// <summary>
        /// Возвращает или задает объект Font
        /// для отображения решений в ячейках.
        /// </summary>
        public Font SolutionsFont
        {
            get 
            {
                return _solutionsFont;
            }
            set
            {
                if (value != null)
                {
                    _solutionsFont = value;
                }
            }
        }

        /// <summary>
        /// Возвращает или задает объект Font
        /// для отображения сумм суммирующих групп.
        /// </summary>
        public Font SumGroupsFont
        {
            get
            {
                return _sumGroupFont;
            }
            set
            {
                if (value != null)
                {
                    _sumGroupFont = value;
                }
            }
        }

        /// <summary>
        /// Возвращает или задает значение,
        /// нужно ли помимо выделенных ячеек помечать также
        /// связанные с ними ячейки (в одном столбце, строке, группе и т.д.)
        /// </summary>
        public bool SelectRelated { get; set; }

        /// <summary>
        /// Возвращает или задает цвет значений ячеек-подсказок.
        /// </summary>
        public Color OpenedValuesColor
        {
            get
            {
                return _tipsBrush.Color;
            }

            set
            {
                _tipsBrush.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет
        /// отображения сумм суммирующих групп.
        /// </summary>
        public Color SumGroupValuesColor
        {
            get
            {
                return _sumGroupValuesBrush.Color;
            }

            set
            {
                _sumGroupValuesBrush.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет для заливки
        /// выделенных ячеек.
        /// </summary>
        public Color SelectionFillColor
        {
            get
            {
                return _cellSelectionBrush.Color;
            }

            set
            {
                _cellSelectionBrush.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет границ
        /// выделенных ячеек.
        /// </summary>
        public Color SelectionOutlineColor
        {
            get
            {
                return _cellSelectionPen.Color;
            }

            set
            {
                _cellSelectionPen.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет для заливки
        /// конфликтных ячеек.
        /// </summary>
        public Color ConflictsFillColor
        {
            get
            {
                return _conflictCellBrush.Color;
            }
            set
            {
                _conflictCellBrush.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет границ
        /// конфликтных ячеек.
        /// </summary>
        public Color ConflictOutlineColor
        {
            get
            {
                return _conflictCellPen.Color;
            }
            set
            {
                _conflictCellPen.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет иконки
        /// недоступной ячейки.
        /// </summary>
        public Color UnavailableIconColor
        {
            get
            {
                return _unavailableIconPen.Color;
            }
            set
            {
                _unavailableIconPen.Color = value;
            }
        }

        /// <summary>
        /// Возвращает или задает цвет иконки
        /// ячейки гарантирующей не пустое решение.
        /// </summary>
        public Color MustToWriteIconColor
        {
            get
            {
                return _mustWriteOutlinePen.Color;
            }
            set
            {
                _mustWriteOutlinePen.Color = value;
            }
        }

        /// <summary>
        /// Отображает карту, используя заданный объект Graphics.
        /// </summary>
        /// <param name="g"></param>
        public abstract void Draw(Graphics g);

        /// <summary>
        /// Конструирует основу для изображения карты заданного размера
        /// (в ячейках) на заданном дисплее (в пикселях).
        /// </summary>
        /// <param name="display"></param>
        /// <param name="mapSize"></param>
        public void ConstructImage(RectangleF display, Size mapSize)
        {
            _display = display;
            _mapSize = mapSize;

            _display.Width--;
            _display.Height--;
            _cellSize = FindCellSize();

            _imageWidth = _cellSize * _mapSize.Width;
            _imageHeight = _cellSize * _mapSize.Height;

            float x = (_display.Width - _imageWidth) / 2;
            float y = (_display.Height - _imageHeight) / 2;
            _imagePosition = new PointF(x, y);
            _imageRect = new RectangleF(x, y, _imageWidth, _imageHeight);
        }

        /// <summary>
        /// Находит ячейку, которой бы принадлежала заданная точка,
        /// если такая ячейка существует на карте.
        /// </summary>
        /// <param name="x"> Координата X целевой точки </param>
        /// <param name="y"> Координата Y целевой точки </param>
        /// <returns> Структура Point, где X 
        /// соответствует столбцу ячейки, а Y - строке. </returns>
        public Point GetCell(float x, float y)
        {
            int col = (int)((x - _imagePosition.X) / _cellSize);
            int row = (int)((y - _imagePosition.Y) / _cellSize);
            return new Point(col, row);
        }

        /// <summary>
        /// Находит группу ячеек, которые бы пересекались с заданным прямоугольником,
        /// если такие ячейки существуют на карте.
        /// </summary>
        /// <param name="rect"> Прямоугольник выделения </param>
        /// <returns> Список структур Point, в которых X 
        /// соответствует столбцу ячейки, а Y - строке. </returns>
        public List<Point> GetCells(RectangleF rect)
        {
            List<Point> cells = new List<Point>();
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

        /// <summary>
        /// Находит все точки ломаной линии, проходящей через
        /// внешнюю границу группы ячеек.
        /// </summary>
        /// <param name="indent">Коэффициент отступа границы от сетки.</param>
        /// <param name="cells">Целевая группа ячеек.</param>
        /// <returns></returns>
        protected List<List<PointF>> ConstructOutline(float indent,
            List<CellInfo> cells)
        {
            var points = ConstructPoints(cells, indent);
            List<List<PointF>> outline = Arrange(points);
            return outline;
        }

        /// <summary>
        /// Отображает задний фон.
        /// </summary>
        /// <param name="g"></param>
        protected void DrawBackGround(Graphics g)
        {
            RectangleF gradRect = new RectangleF()
            {
                X = 0,
                Y = 0,
                Width = _imageWidth,
                Height = _imageHeight
            };

            _bgGradientBrush = new LinearGradientBrush(_display,
            Color.FromArgb(15, 250, 250, 250),
            Color.FromArgb(45, 200, 200, 240), 90);

            g.FillRectangle(_bgHatchBrush, _display);
            g.FillRectangle(_bgGradientBrush, _display);
        }

        /// <summary>
        /// Строит прямоугольник, соответствующий изображению ячейки
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected RectangleF ConstructCellRect(int row, int column)
        {
            float x = column * _cellSize + _imagePosition.X;
            float y = row * _cellSize + _imagePosition.Y;
            RectangleF cellRect = new RectangleF(x, y, _cellSize, _cellSize);
            return cellRect;
        }

        /// <summary>
        /// Отображает формы незаблокированныч ячеек.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cells"></param>
        protected void DrawUnblockedCells(Graphics g, List<CellInfo> cells)
        {
            foreach (var cell in cells)
            {
                RectangleF rect = ConstructCellRect(cell.Row, cell.Column);

                if (!cell.IsBlocked)
                {
                    g.FillRectangle(_unblockedCellBrush, rect);
                    g.DrawRectangle(_unblockedCellPen, rect.X,
                        rect.Y, rect.Width, rect.Height);
                }
            }
        }

        /// <summary>
        /// Отображает все подсказки карты.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cells"></param>
        protected void DrawTips(Graphics g, List<CellInfo> cells)
        {
            foreach (var cell in cells)
            {
                RectangleF rect = ConstructCellRect(cell.Row, cell.Column);

                if (!cell.IsAvailable)
                {
                    if (cell.Solution == 0)
                    {
                        DrawUnavailableIcon(g, rect);
                    }

                    else
                    {
                        g.DrawString(cell.Solution.ToString(), _solutionsFont,
                            _tipsBrush, rect, _textFormat);
                    }
                }

                else
                {
                    if (cell.State == CellType.MustWrite)
                    {
                        DrawMustWriteCell(g, rect);
                    }
                }
            }
        }

        private void DrawMustWriteCell(Graphics g, RectangleF rect)
        {
            float width = rect.Width - _mustWriteIconIndent;
            float height = rect.Height - _mustWriteIconIndent;
            width = width > 0 ? width : 1;
            height = height > 0 ? height : 1;
            float x = rect.X + (rect.Width - width) / 2;
            float y = rect.Y + (rect.Height - height) / 2;
            g.DrawRectangle(_mustWriteOutlinePen, x, y, width, height);
        }

        /// <summary>
        /// Отображает границы групп карты.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cells"></param>
        /// <param name="groups"></param>
        protected void DrawGroupsOutlines(Graphics g,
            List<CellInfo> cells, List<GroupInfo> groups)
        {
            List<int> drawnGroups = new List<int>();
            foreach (var gr in groups)
            {
                if (drawnGroups.Contains(gr.ID))
                    continue;

                var grCells = cells.Where(c => c.Groups.Contains(gr.ID));

                bool isSumGroup = gr.Type == GroupType.Sum;
                float indent = 0;
                if (gr.Type == GroupType.Sum)
                    indent = _sumGroupOutlineIndent;

                var outline = ConstructOutline(indent, grCells.ToList());
                if (outline.Count == 0)
                    continue;

                Pen pen;
                if (isSumGroup)
                {
                    if (gr.IsSelected)
                    {
                        pen = _selectedSumGroupPen;
                    }
                    else
                    {
                        pen = _sumGroupPen;
                    }
                }
                else
                {
                    pen = _basicGroupPen;
                }

                foreach (var pts in outline)
                {
                    g.DrawLines(pen, pts.ToArray());
                }

                if (isSumGroup)
                {
                    DrawSum(g, gr, outline);
                }

                drawnGroups.Add(gr.ID);
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

        private void DrawSum(Graphics g, GroupInfo gr, List<List<PointF>> outline)
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
            g.DrawString(gr.Sum.ToString(), _sumGroupFont, _tipsBrush,
                textRect, _textFormat);
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

        private float FindCellSize()
        {
            float smallerControlSide = _display.Width > _display.Height
                ? _display.Height : _display.Width;
            int biggerMapSide = _mapSize.Width < _mapSize.Height
                ? _mapSize.Height : _mapSize.Width;

            return smallerControlSide / biggerMapSide;
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
