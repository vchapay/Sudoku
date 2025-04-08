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
    /// Предоставляет средство для отрисовки объектов MapInterface.
    /// </summary>
    internal sealed class MapPlayerDisplayDrawer : Drawer
    {
        private const int _defaultNoteFontSize = 9;
        private const int _incorrectOutlineWidth = 3;
        private const int _notesXIndent = 0;
        private const int _notesYIndent = 4;
        private const int _notesRectTopBottomWidth = 8;
        private const int _notesRectLeftRightWidth = 0;
        private SolidBrush _enteredValuesBrush;

        private SolidBrush _incorrectValuesTextBrush;

        private SolidBrush _incorrectValuesFillBrush;

        private SolidBrush _notesBrush;

        private SolidBrush _notesSelectionBrush;

        private MapInterface _target;

        private Font _notesFont;

        private Pen _incorrectCellPen;

        public MapPlayerDisplayDrawer(MapInterface map, RectangleF display)
        {
            _target = map ?? throw new ArgumentNullException();
            Size mapSize = new Size(map.RowsCount, map.ColumnsCount);
            ConstructImage(display, mapSize);
            InitializeGraphics();
        }

        /// <summary>
        /// Возвращает или задает объект карты, который нужно нарисовать.
        /// </summary>
        public MapInterface Target
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

        /// <summary>
        /// Возвращает или задает объект Font
        /// для отображения заметок карандашом.
        /// </summary>
        public Font NotesFont
        {
            get
            {
                return _notesFont;
            }
            set
            {
                if (value != null)
                {
                    _notesFont = value;
                }
            }
        }

        public override void Draw(Graphics g)
        {
            var cells = _target.Cells.Select(c => c.GetInfo());
            var groups = _target.Groups.Select(gr => gr.GetInfo());
            g.Clear(Color.White);
            DrawBackGround(g);
            DrawUnblockedCells(g, cells.ToList());
            DrawTips(g, cells.ToList());
            DrawEnteredSolutions(g, _target.Cells);
            DrawGroupsOutlines(g, cells.ToList(), groups.ToList());
            DrawSelections(g);
            DrawMistakes(g, _target.Cells);
            DrawNotes(g, _target.Cells);
        }

        private void InitializeGraphics()
        {
            _notesFont = new Font("Times New Roman", _defaultNoteFontSize);
            _enteredValuesBrush = new SolidBrush(Color.DarkBlue);
            _incorrectValuesTextBrush = new SolidBrush(Color.DarkRed);
            _incorrectValuesFillBrush = new SolidBrush(Color.FromArgb(70, 240, 60, 0));
            _incorrectCellPen = new Pen(Color.FromArgb(180, 100, 40, 0))
            {
                Width = _incorrectOutlineWidth,
                DashStyle = DashStyle.Dash
            };
            _notesBrush = new SolidBrush(Color.DarkBlue);
            _notesSelectionBrush = new SolidBrush(
                Color.FromArgb(90, 170, 180, 230));
        }

        private void DrawMistakes(Graphics g, IEnumerable<CellInterface> cells)
        {
            foreach (var cell in cells)
            {
                if (cell.IsThereMistake)
                {
                    RectangleF rect = ConstructCellRect(cell.Row, cell.Column);
                    g.FillRectangle(_incorrectValuesFillBrush, rect);
                    g.DrawRectangle(_incorrectCellPen, rect.X, rect.Y,
                        rect.Width, rect.Height);
                }
            }
        }

        private void DrawNotes(Graphics g, IReadOnlyCollection<CellInterface> cells)
        {
            var selectedNums = _target.GetSelectedNums();

            foreach (var cell in cells)
            {
                if (cell.IsEntered || !cell.IsAvailable)
                    continue;

                foreach (int note in cell.Notes)
                {
                    // для 20 возможных значений заметки
                    // взят квадрат 4x5
                    RectangleF noteRect = ConstructNoteRect(cell.Row, cell.Column, note);
                    if (selectedNums.Contains(note))
                        g.FillRectangle(_notesSelectionBrush, noteRect);
                    g.DrawString($"{note}", _notesFont, _notesBrush,
                        noteRect, _textFormat);
                }
            }
        }

        private RectangleF ConstructNoteRect(int row, int column, int note)
        {
            int noteRow = (note - 1) / 4;
            int noteColumn = (note - 1) - 4 * noteRow;
            float noteRectWidth = _cellSize - _notesRectLeftRightWidth;
            float noteRectHeight = _cellSize - _notesRectTopBottomWidth;
            noteRectWidth = noteRectWidth > 0 ? noteRectWidth : _cellSize;
            noteRectHeight = noteRectHeight > 0 ? noteRectHeight : _cellSize;
            float noteCellWidth = noteRectWidth / 4;
            float noteCellHeight = noteRectHeight / 5;
            float cellX = column * _cellSize +
                _imagePosition.X + _notesXIndent;
            float cellY = row * _cellSize +
                _imagePosition.Y + _notesYIndent;
            float x = cellX + noteCellWidth * noteColumn;
            float y = cellY + noteCellHeight * noteRow;
            RectangleF noteRect =
                new RectangleF(x, y, noteCellWidth, noteCellHeight);
            return noteRect;
        }

        private void DrawEnteredSolutions(Graphics g,
            IReadOnlyCollection<CellInterface> cells)
        {
            foreach (var cell in cells)
            {
                RectangleF rect = ConstructCellRect(cell.Row, cell.Column);

                if (cell.IsAvailable && cell.Entered != 0)
                {
                    var brush = _enteredValuesBrush;

                    if (cell.IsThereMistake)
                        brush = _incorrectValuesTextBrush;

                    g.DrawString(cell.Entered.ToString(), SolutionsFont, brush,
                        rect, _textFormat);
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
                var selectedInfos = selected.Select(c => c.GetInfo());
                outline = ConstructOutline(_selectedOutlineIndent,
                    selectedInfos.ToList());
                foreach (var pts in outline)
                {
                    g.DrawPolygon(_cellSelectionPen, pts.ToArray());
                }

                foreach (var cell in selected)
                {
                    if (cell.IsThereMistake)
                        continue;

                    RectangleF cellRect = ConstructCellRect(cell.Row, cell.Column);
                    g.FillRectangle(_cellSelectionBrush, cellRect);
                    if (cell.IsSolved || (!cell.IsAvailable && !cell.IsBlocked))
                    {
                        if (_selectedContent.Add(cell.Solution))
                        {
                            SelectContent(g, cell.Solution);
                        }
                    }
                }
            }

            if (selected.All(c => c.IsBlocked))
                return;

            IEnumerable<CellInterface> associated =
                _target.GetAssociatedCells(selected.ToArray());
            associated = associated.Where(c => !c.IsBlocked);
            var infos = associated.Select(c => c.GetInfo());

            if (!associated.Any())
                return;

            outline = ConstructOutline(_associatedOutlineIndent, infos.ToList());
            GraphicsPath fillPath = new GraphicsPath();
            foreach (var pts in outline)
            {
                fillPath.AddPolygon(pts.ToArray());
                fillPath.CloseFigure();
            }

            fillPath.FillMode = FillMode.Alternate;
            g.FillPath(_cellSelectionBrush, fillPath);

            /*IEnumerable<CellInterface> associated =
                _target.GetAssociatedCells(selected.ToArray());
            associated = associated.Where(c => !c.IsBlocked);
            var infos = associated.Select(c => c.GetInfo());
            outline = ConstructOutline(_associatedOutlineIndent, infos.ToList());
            foreach (var pts in outline)
            {
                g.FillPolygon(_cellSelectionBrush, pts.ToArray());
            }*/
        }

        private void SelectContent(Graphics g, int solution)
        {
            if (solution == 0)
                return;

            var targets = _target.GetCellsByOpenedSolution(solution);
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
