using Sudoku.MapGraphics;
using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace Sudoku.SpecialControls
{
    internal class SudokuMaker : Control
    {
        private Map _map;
        private readonly MapDrawer _drawer;
        private readonly List<CellInfoPanel> _showingCells;
        private PointF _selectionRectBegin;
        private PointF _selectionRectEnd;
        private bool _isSelecting;
        private Pen _selectionRectPen;
        private Pen _splitterPen;
        private Pen _leftTopDecoPen;
        private RectangleF _mapDisplayRect;
        private RectangleF _cellEditRect;
        private RectangleF _selectionRect;
        private RectangleF _selectedCellsListRect;
        private RectangleF _editRect;
        private float _splitterRatio = 0.7f;
        private float _cellPanelHeight;
        private float _cellPanelsSplitterWidth;
        private float _cellPanelScroll;
        private Font _noSelectedCellsMessageFont;
        private StringFormat _noSelectedCellsMessageFormat;
        private Brush _noSelectedCellsMessageBrush;
        private Brush _editRectBrush;
        private float _showingCellsSumHeight;
        private bool _isShiftPressed;

        public SudokuMaker()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
            Map map = new Map();
            _drawer = new MapDrawer();
            _map = map;

            _showingCells = new List<CellInfoPanel>();
            Width = 350;
            Height = 350;

            Font = _drawer.Font;
            _noSelectedCellsMessageFont = new Font("Times New Roman", 18);

            _leftTopDecoPen = new Pen(Color.DarkGray)
            {
                Width = 1,
                DashStyle = DashStyle.Dot,
            };
            _selectionRectPen = new Pen(Color.FromArgb(150, 50, 150, 0))
            {
                Width = 1.5f
            };
            _splitterPen = new Pen(Color.Gray)
            {
                Width = 2f,
                DashStyle = DashStyle.Dash,
            };

            _mapDisplayRect = new RectangleF()
            {
                Height = Height,
                Width = Width * _splitterRatio,
            };

            _noSelectedCellsMessageFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            _noSelectedCellsMessageBrush = new SolidBrush(Color.Gray);
            _editRectBrush = new SolidBrush(Color.White);
            _cellPanelHeight = 250;
            _cellPanelsSplitterWidth = 3;
        }

        public Map Map
        {
            get { return _map; }
            set { _map = value; }
        }

        public float CellPanelHeight
        {
            get => _cellPanelHeight;
            set
            {
                if (value > 0)
                    _cellPanelHeight = value;
            }
        }

        public float CellPanelsSplitterWidth
        {
            get => _cellPanelsSplitterWidth;
            set
            {
                if (value > 0)
                    _cellPanelsSplitterWidth = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            _drawer.Draw(g, _mapDisplayRect, _map);
            if (_isSelecting)
            {
                ConstructSelectionRect();

                g.DrawRectangle(_selectionRectPen, 
                    _selectionRect.X, _selectionRect.Y,
                    _selectionRect.Width, _selectionRect.Height);
            }

            g.DrawLine(_splitterPen, 
                Width * _splitterRatio, 0, Width * _splitterRatio, Height);

            DrawLeftTopDeco(g);
            if (_showingCells.Count == 0)
            {
                g.DrawString("Нет выделенных ячеек", _noSelectedCellsMessageFont,
                    _noSelectedCellsMessageBrush, _cellEditRect, _noSelectedCellsMessageFormat);
            }

            float nextY = _cellPanelScroll;
            for (int i = 0; i < _showingCells.Count; i++)
            {
                var cell = _showingCells[i];
                cell.YIndent = nextY;
                nextY += _cellPanelsSplitterWidth + cell.Height;
                cell.Draw(g);
            }

            g.FillRectangle(_editRectBrush, _editRect);
            g.DrawRectangle(Pens.DarkGray, _editRect.X, 
                _editRect.Y, _editRect.Width, _editRect.Height);
        }

        private enum CellState
        {
            NonInteractive,
            Interactive
        }

        private class CellInfoPanel
        {
            private const int _conflictFlagHeight = 15;
            private RectangleF _container;
            private RectangleF _bounds;
            private RectangleF _titleBounds;
            private RectangleF _posBounds;
            private RectangleF _correctBounds;
            private RectangleF _groupsBounds;
            private RectangleF _conflictFlagRect;
            private Font _titlesFont;
            private Font _correctFont;
            private Font _posFont;
            private readonly Brush _titlesBrush;
            private readonly Brush _correctBrush;
            private readonly Brush _posBrush;
            private readonly Brush _nonConflictBrush;
            private readonly Brush _conflictBrush;
            private readonly Pen _outlinePen;
            private readonly StringFormat _textFormat;
            private Dictionary<int, Map.GroupType> _groups;
            private float _height = 250;
            private float _titleHeight = 15;
            private float _correctHeight = 70;
            private float _yIndent;
            private float _groupsSplitter = 3;
            private float _groupPanelHeight = 20;
            private readonly List<GroupPanel> _groupPanels;

            private CellStateButton _cellStateButton;

            public CellInfoPanel(RectangleF container)
            {
                _container = container;

                _titlesFont = new Font("Times New Roman", 20, GraphicsUnit.Pixel);
                _correctFont = new Font("Times New Roman", 54);
                _posFont = new Font("Times New Roman", 15, GraphicsUnit.Pixel);

                _outlinePen = new Pen(Color.Gray)
                {
                    Width = 0.5f
                };

                _correctBrush = new SolidBrush(Color.Black);
                _titlesBrush = new SolidBrush(Color.Black);
                _posBrush = new SolidBrush(Color.Gray);
                _conflictBrush = new SolidBrush(Color.FromArgb(50, 240, 50, 0));
                _nonConflictBrush = new SolidBrush(Color.FromArgb(30, 40, 240, 0));

                _textFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };

                _groups = new Dictionary<int, Map.GroupType>();
                _cellStateButton = new CellStateButton();
                _groupPanels = new List<GroupPanel>();
                ConstructBounds();
            }

            public float YIndent 
            { 
                get 
                { 
                    return _yIndent; 
                }
                set
                {
                    _yIndent = value;
                    ConstructBounds();
                }
            }

            public float TitleHeight 
            { 
                get 
                {
                    return _titleHeight;
                }
                set
                {
                    if (value > 0)
                        _titleHeight = value;
                }
            }

            public float CorrectValueHeight
            {
                get
                {
                    return _correctHeight;
                }
                set
                {
                    if (value > 0)
                        _correctHeight = value;
                }
            }

            public float GroupPanelHeight
            {
                get 
                { 
                    return _groupPanelHeight; 
                }
                set
                {
                    if (value > 0)
                        _groupPanelHeight = value;
                }
            }

            public float Height { get { return _height; } }

            public int Correct { get; set; }

            public bool IsConflict { get; set; }

            public int Row { get; set; }

            public int Column { get; set; }

            public CellState CellState { get; set; }

            public RectangleF Bounds { get { return _bounds; } }

            public Dictionary<int, Map.GroupType> Groups 
            { 
                get 
                { 
                    return _groups; 
                }
                set 
                {
                    if (value != _groups && value != null)
                    {
                        _groups = value;
                        _groupPanels.Clear();

                        ConstructBounds();
                        foreach (var group in _groups)
                        {
                            var panel = new GroupPanel(_groupsBounds)
                            {
                                ID = group.Key,
                                Height = _groupPanelHeight,
                                Type = group.Value,
                            };

                            _groupPanels.Add(panel);
                        }
                    }
                } 
            }

            public void Draw(Graphics g)
            {
                ConstructBounds();
                g.DrawRectangle(_outlinePen, _bounds.X, _bounds.Y,
                    _bounds.Width, _bounds.Height);
                DrawTitle(g);
                DrawPosition(g);
                DrawContent(g);
                DrawConflictFlag(g);
                DrawGroups(g);
                _cellStateButton.CellState = CellState;
                _cellStateButton.ConstructBounds();
                _cellStateButton.Draw(g);
            }

            public bool IsButtonFocused(PointF point)
            {
                return _cellStateButton.IsFocused(point);
            }

            public int GetFocusedGroup(PointF point)
            {
                int id = -1;
                foreach (var g in _groupPanels)
                {
                    if (g.IsFocused(point))
                    {
                        id = g.ID;
                        break;
                    }
                }

                return id;
            }

            public void ChangeGroupSelection(int id, bool selected)
            {
                foreach (var g in _groupPanels)
                {
                    if (g.ID == id)
                    {
                        g.IsSelected = selected;
                        break;
                    } 
                }
            }

            public void DeselectGroups()
            {
                foreach (var g in _groupPanels)
                {
                    g.IsSelected = false;
                }
            }

            public void ChangeButtonSelection(bool selected)
            {
                _cellStateButton.IsSelected = selected;
            }

            private void DrawGroups(Graphics g)
            {
                g.DrawRectangle(Pens.LightGray, _groupsBounds.X, _groupsBounds.Y,
                    _groupsBounds.Width, _groupsBounds.Height);

                for (int i = 0; i < _groupPanels.Count; i++)
                {
                    var panel = _groupPanels[i];
                    panel.YIndent = YIndent + _groupsSplitter +
                        (_groupsSplitter + panel.Height) * i;
                    panel.Draw(g);
                }
            }

            private void DrawPosition(Graphics g)
            {
                g.DrawString($"[ {Row}, {Column} ]", _posFont,
                    _posBrush, _posBounds, _textFormat);
            }

            private void DrawConflictFlag(Graphics g)
            {
                g.DrawRectangle(Pens.Gray, _conflictFlagRect.X, _conflictFlagRect.Y,
                    _conflictFlagRect.Width, _conflictFlagRect.Height);

                Brush brush = _nonConflictBrush;
                if (IsConflict)
                    brush = _conflictBrush;

                g.FillRectangle(brush, _conflictFlagRect);
            }

            private void DrawContent(Graphics g)
            {
                g.DrawString(Correct.ToString(), _correctFont, 
                    _correctBrush, _correctBounds, _textFormat);
            }

            private void ConstructBounds()
            {
                _bounds = new RectangleF()
                {
                    X = _container.X,
                    Y = YIndent + _container.Y,
                    Width = _container.Width,
                };

                _titleBounds = new RectangleF()
                {
                    X = _bounds.X,
                    Y = _bounds.Y + 10,
                    Width = _bounds.Width,
                    Height = _titleHeight
                };

                _posBounds = new RectangleF()
                {
                    X = _bounds.X,
                    Y = _titleBounds.Bottom + 10,
                    Width = _bounds.Width,
                    Height = _titleHeight
                };

                _correctBounds = new RectangleF()
                {
                    X = _bounds.X,
                    Y = _posBounds.Bottom + 10,
                    Width = _bounds.Width,
                    Height = _correctHeight
                };

                _groupsBounds = new RectangleF()
                {
                    X = _bounds.X + 5,
                    Y = _correctBounds.Bottom,
                    Width = _bounds.Width - 10,
                    Height = _groupsSplitter + 
                        (_groupPanelHeight + _groupsSplitter) * _groups.Count,
                };

                _height = _groupsBounds.Bottom - _bounds.Top + _conflictFlagHeight + 5;
                _bounds.Height = _height;

                _conflictFlagRect = new RectangleF()
                {
                    X = _bounds.X,
                    Y = _bounds.Bottom - _conflictFlagHeight,
                    Width = _bounds.Width,
                    Height = _conflictFlagHeight
                };

                _cellStateButton.X = _bounds.Right - 
                    _bounds.Width * 0.02f - _bounds.Width * 0.08f;
                _cellStateButton.Y = _bounds.Top + _bounds.Height * 0.02f;
                _cellStateButton.Width = _bounds.Width * 0.08f;
                _cellStateButton.Height = _cellStateButton.Width;
            }

            private void DrawTitle(Graphics g)
            {
                g.DrawString("Ячейка", _titlesFont, _titlesBrush, _titleBounds, _textFormat);
                g.DrawLine(Pens.LightGray, _bounds.X + 15, _titleBounds.Bottom + 5,
                    _bounds.Right - 15, _titleBounds.Bottom + 5);
            }

            private class GroupPanel
            {
                private RectangleF _container;
                private RectangleF _bounds;
                private RectangleF _idBounds;
                private RectangleF _typeBounds;
                private float _height;
                private float _yIndent;
                private readonly Pen _pen;
                private readonly SolidBrush _textBrush;
                private readonly SolidBrush _selectedBrush;
                private readonly StringFormat _textFormat;
                private Color _nonSelectedColor;
                private Color _selectedColor;
                private readonly Font _font;

                public GroupPanel(RectangleF container)
                {
                    _container = container;

                    _textFormat = new StringFormat()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                    };

                    _nonSelectedColor = Color.Gray;

                    _selectedColor = Color.Black;

                    _pen = new Pen(Color.LightGray)
                    {
                        Width = 0.7f
                    };

                    _textBrush = new SolidBrush(Color.Gray);

                    _selectedBrush = new SolidBrush(Color.FromArgb(60, 150, 150, 150));

                    _font = new Font("Times New Roman", 14);

                    ConstructBounds();
                }

                public float YIndent 
                {
                    get
                    {
                        return _yIndent;
                    }
                    set
                    {
                        _yIndent = value;
                        ConstructBounds();
                    }
                }

                public float Height 
                {
                    get { return _height; }
                    set
                    {
                        if (value > 0)
                        {
                            _height = value;
                            ConstructBounds();
                        }
                    }
                }

                public int ID { get; set; }

                public Map.GroupType Type { get; set; }

                public RectangleF Bounds { get { return _bounds; } }

                public bool IsSelected { get; set; }

                public void Draw(Graphics g)
                {
                    _pen.Color = _nonSelectedColor;
                    _textBrush.Color = _nonSelectedColor;

                    if (IsSelected)
                    {
                        _pen.Color = _selectedColor;
                        _textBrush.Color = _selectedColor;
                        g.FillRectangle(_selectedBrush, _bounds);
                    }

                    g.DrawRectangle(_pen, _bounds.X, _bounds.Y,
                        _bounds.Width, _bounds.Height);

                    g.DrawLine(_pen, _idBounds.Right, _bounds.Top, 
                        _idBounds.Right, _bounds.Bottom);

                    g.DrawString(ID.ToString(), _font, _textBrush, _idBounds, _textFormat);
                    g.DrawString(Type.ToString(), _font, _textBrush, _typeBounds, _textFormat);
                }

                public void ConstructBounds()
                {
                    _bounds = new RectangleF()
                    {
                        X = _container.X + 5,
                        Y = YIndent + _container.Y,
                        Width = _container.Width - 10,
                        Height = _height
                    };

                    _idBounds = new RectangleF()
                    {
                        X = _bounds.X,
                        Y = _bounds.Y,
                        Width = _bounds.Width / 3,
                        Height = _height
                    };

                    _typeBounds = new RectangleF()
                    {
                        X = _idBounds.Right,
                        Y = _bounds.Y,
                        Width = _bounds.Width / 3 * 2,
                        Height = _height
                    };
                }

                public bool IsFocused(PointF point)
                {
                    return _bounds.Contains(point);
                }
            }

            private class CellStateButton
            {
                private float _width;
                private float _height;
                private RectangleF _bounds;
                private readonly Pen _selectionPen;
                private readonly Brush _selectionBrush;

                public CellStateButton()
                {
                    _selectionPen = new Pen(Color.DarkGray);
                    _selectionBrush = new SolidBrush(Color.FromArgb(50, 150, 150, 150));
                }

                public float X { get; set; }

                public float Y { get; set; }

                public CellState CellState { get; set; }

                public float Width
                {
                    get 
                    { 
                        return _width; 
                    }
                    set 
                    { 
                        if (value > 0)
                            _width = value; 
                    }
                }

                public float Height
                {
                    get
                    {
                        return _height;
                    }
                    set
                    {
                        if (value > 0)
                            _height = value;
                    }
                }

                public bool IsSelected { get; set; }

                public void Draw(Graphics g)
                {
                    Image image;

                    switch (CellState)
                    {
                        case CellState.Interactive:
                            image = Properties.Resources.InteractiveCellIcon;
                            break;
                        case CellState.NonInteractive:
                            image = Properties.Resources.NonInteractiveCellIcon;
                            break;
                        default:
                            image = Properties.Resources.InteractiveCellIcon;
                            break;
                    }

                    g.DrawImage(image, _bounds);

                    if (IsSelected)
                    {
                        g.DrawRectangle(_selectionPen, _bounds.X, 
                            _bounds.Y, _bounds.Width, _bounds.Height);
                        g.FillRectangle(_selectionBrush, _bounds);
                    }
                }

                public void ConstructBounds()
                {
                    _bounds = new RectangleF()
                    {
                        X = X,
                        Y = Y,
                        Width = Width,
                        Height = Height
                    };
                }

                public bool IsFocused(PointF point)
                {
                    return _bounds.Contains(point);
                }
            }
        }

        private void DrawLeftTopDeco(Graphics g)
        {
            PointF begin = new PointF(_mapDisplayRect.Width + 7, 6);
            float endX = Width - 10;
            float endY = Height - 10;

            g.DrawLine(_leftTopDecoPen, begin.X, begin.Y, endX, begin.Y);
            g.DrawLine(_leftTopDecoPen, begin.X, begin.Y, begin.X, endY);

            g.FillEllipse(Brushes.Gray, _mapDisplayRect.Width + 4,
                3, 6, 6);
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
            _mapDisplayRect = new RectangleF()
            {
                Width = Width * _splitterRatio,
                Height = Height,
            };

            _cellEditRect = new RectangleF()
            {
                Width = Width - _mapDisplayRect.Width,
                Height = Height,
                X = _mapDisplayRect.Width
            };

            _selectedCellsListRect = new RectangleF()
            {
                X = _cellEditRect.X + 15,
                Y = _cellEditRect.Y + 15,
                Width = _cellEditRect.Width - 30,
                Height = _cellEditRect.Height * 0.7f
            };

            _editRect = new RectangleF()
            {
                X = _cellEditRect.X,
                Y = _selectedCellsListRect.Bottom + 15,
                Width = _cellEditRect.Width - 1,
                Height = _cellEditRect.Height -
                    _selectedCellsListRect.Height - 40
            };

            _cellPanelScroll = 0;
            UpdateShowingCells();
            ValidateScroll();
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            _drawer.Font = Font;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_selectedCellsListRect.Contains(e.Location))
            {
                return;
            }

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
            }

            foreach (var cell in _showingCells)
            {
                cell.DeselectGroups();
                int id = cell.GetFocusedGroup(e.Location);

                if (id != -1)
                {
                    cell.ChangeGroupSelection(id, selected: true);
                }

                bool btnFocused = cell.IsButtonFocused(e.Location);
                cell.ChangeButtonSelection(btnFocused);
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!_isShiftPressed)
                    _map.ClearSelection();

                if (_isSelecting)
                {
                    _selectionRectEnd = e.Location;
                    ConstructSelectionRect();
                    Size mapSize = new Size(_map.Width, _map.Height);
                    List<Point> cells = _drawer.GetCells(_selectionRect,
                        mapSize, _mapDisplayRect.Size);
                    foreach (var cellPos in cells)
                    {
                        if (cellPos.X > -1 && cellPos.X < _map.Width
                        && cellPos.Y > -1 && cellPos.Y < _map.Height)
                        {
                            _map.ChangeCellSelection(cellPos.Y, cellPos.X);
                        }
                    }
                }

                else
                {
                    foreach (var cell in _showingCells)
                    {
                        int id = cell.GetFocusedGroup(e.Location);

                        if (id != -1)
                        {
                            _map.RemoveCellFromGroup(cell.Row, cell.Column, id);
                        }

                        if (cell.IsButtonFocused(e.Location))
                        {
                            _map.ChangeAvailability(cell.Row, cell.Column);
                        }
                    }
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                _map.ClearSelection();
            }

            _isSelecting = false;
            UpdateShowingCells();
            ValidateScroll();
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_showingCells.Count == 0)
                return;

            float changing = e.Delta / 4;
            _cellPanelScroll += changing;

            ValidateScroll();
            Invalidate();
            base.OnMouseWheel(e);
        }

        private void ValidateScroll()
        {
            _showingCellsSumHeight = 0;
            foreach (var cell in _showingCells)
            {
                _showingCellsSumHeight += cell.Height + _cellPanelsSplitterWidth;
            }

            if (_cellPanelScroll > 0)
                _cellPanelScroll = 0;

            if (_cellPanelScroll < _selectedCellsListRect.Height - _showingCellsSumHeight)
                _cellPanelScroll = _selectedCellsListRect.Height - _showingCellsSumHeight;

            if (_selectedCellsListRect.Height > _showingCellsSumHeight)
                _cellPanelScroll = 0;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Shift)
            {
                _isShiftPressed = true;
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        _map.AddSelectedToGroup();
                        break;
                    case Keys.R:
                        _map.RemoveSelectedFromGroup();
                        break;
                    case Keys.Z:
                        _map.Undo();
                        break;
                    case Keys.Y:
                        _map.Redo();
                        break;
                }
            }

            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        _map.Write(0);
                        break;
                    case Keys.G:
                        _map.GroupSelected(Map.GroupType.Basic);
                        break;
                    case Keys.S:
                        _map.GroupSelected(Map.GroupType.Sum);
                        break;
                    case Keys.A:
                        _map.ChangeSelectedAvailability();
                        break;
                }
            }

            UpdateShowingCells();
            ValidateScroll();
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                _map.Write(int.Parse(e.KeyChar.ToString()));
            }

            UpdateShowingCells();
            Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _isShiftPressed = false;
            base.OnKeyUp(e);
        }

        private void UpdateShowingCells()
        {
            _showingCells.Clear();
            var cells = _map.GetSelectedCells();
            foreach (var cell in cells)
            {
                var cellPanel = new CellInfoPanel(_selectedCellsListRect)
                {
                    Row = cell.Row,
                    Column = cell.Column,
                    Correct = cell.Correct,
                    CellState = cell.IsAvailable ? 
                        CellState.Interactive : CellState.NonInteractive,
                    IsConflict = _map.IsCellConflict(cell.Row, cell.Column)
                };

                var groups = new Dictionary<int, Map.GroupType>();
                foreach (var g in cell.Groups)
                {
                    groups.Add(g.ID, g.Type);
                }

                cellPanel.Groups = groups;
                _showingCells.Add(cellPanel);
            }
        }
    }
}
