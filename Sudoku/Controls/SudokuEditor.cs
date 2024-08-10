using Sudoku.MapGraphics;
using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    /// <summary>
    /// Элемент управления, предоставляющий средства редактирования карт судоку.
    /// </summary>
    internal sealed class SudokuEditor : Control
    {
        private Map _map;
        private readonly MapDrawer _drawer;
        private readonly List<CellInfoPanel> _showingCells;
        private readonly GroupEditingPanel _editingPanel;
        private readonly MenuPanel _menuPanel;
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
        private RectangleF _menuRect;
        private float _splitterRatio = 0.7f;
        private float _cellPanelHeight;
        private float _cellPanelsSplitterWidth;
        private float _scroll;
        private Font _noSelectedCellsMessageFont;
        private StringFormat _noSelectedCellsMessageFormat;
        private Brush _noSelectedCellsMessageBrush;
        private Brush _editRectBrush;
        private float _showingCellsSumHeight;
        private bool _isShiftPressed;
        private BorderStyle _borderStyle;

        /// <summary>
        /// Инициализирует новый экземпляр SudokuEditor.
        /// </summary>
        public SudokuEditor()
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
            _editingPanel = new GroupEditingPanel(_editRect);
            _menuPanel = new MenuPanel(_editRect);
            Width = 350;
            Height = 350;

            Font = _drawer.Font;
            _noSelectedCellsMessageFont = new Font("Times New Roman", 18);

            _leftTopDecoPen = new Pen(Color.DarkGray)
            {
                Width = 1,
                DashStyle = DashStyle.Custom,
                DashPattern = new float[]
                {
                    3f,
                    3f,
                }
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

        /// <summary>
        /// Карта, редактируемая текущим контролом.
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set { _map = value; }
        }

        /// <summary>
        /// Высота панели с информацией о выделенной ячейке.
        /// </summary>
        public float CellPanelHeight
        {
            get => _cellPanelHeight;
            set
            {
                if (value > 0)
                    _cellPanelHeight = value;
            }
        }

        /// <summary>
        /// Толщина разделителя панелей
        /// с информацией о выделенных ячейках.
        /// </summary>
        public float CellPanelsSplitterWidth
        {
            get => _cellPanelsSplitterWidth;
            set
            {
                if (value > 0)
                    _cellPanelsSplitterWidth = value;
            }
        }

        /// <summary>
        /// Стиль границ контрола
        /// </summary>
        public BorderStyle BorderStyle 
        { 
            get 
            {
                return _borderStyle;
            } 
            set
            {
                _borderStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Путь, по которому будет сохраняться объект карты
        /// при нажатии на кнопку сохранения
        /// </summary>
        public string SavePath { get; set; }

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

            float nextY = _scroll;
            for (int i = 0; i < _showingCells.Count; i++)
            {
                var cell = _showingCells[i];
                cell.YIndent = nextY;
                nextY += _cellPanelsSplitterWidth + cell.Height;
                if (cell.YIndent + cell.Height > 0 && cell.YIndent < Height)
                    cell.Draw(g);
            }

            g.FillRectangle(Brushes.White, _menuRect);
            g.FillRectangle(Brushes.White, _editRect);
            g.FillRectangle(_editRectBrush, _editRect);
            g.DrawRectangle(Pens.DarkGray, _editRect.X, 
                _editRect.Y, _editRect.Width, _editRect.Height);

            _editingPanel.Draw(g);
            _menuPanel.Draw(g);
            switch (_borderStyle)
            {
                case BorderStyle.None:
                    break;
                case BorderStyle.FixedSingle:
                    Rectangle outline = new Rectangle()
                    {
                        X = 0,
                        Y = 0,
                        Width = Width - 1,
                        Height = Height - 1
                    };

                    g.DrawRectangle(Pens.Black, outline);
                    break;
            }
        }

        private enum CellState
        {
            NonInteractive,
            Interactive
        }

        private class MenuPanel
        {
            private RectangleF _container;
            private RectangleF _bounds;
            private RectangleF _saveBtnBounds;
            private RectangleF _redoBtnBounds;
            private RectangleF _undoBtnBounds;
            private RectangleF _clearBtnBounds;
            private int _splitterWidth;
            private readonly Brush _btnSelectionBrush;
            private HatchBrush _bgHatchBrush;
            private LinearGradientBrush _bgGradientBrush;
            private readonly Pen _pen;
            private readonly Pen _arrowPen;
            private readonly Pen _selectionPen;

            public MenuPanel(RectangleF container)
            {
                _container = container;
                ConstructBounds();

                _btnSelectionBrush = new SolidBrush(Color.FromArgb(100, 150, 150, 150));
                _bgHatchBrush = new HatchBrush(HatchStyle.Cross,
                    Color.White, Color.FromArgb(200, 230, 230, 250));

                _pen = new Pen(Color.FromArgb(200, 100, 100, 120))
                {
                    Width = 1
                };
                _selectionPen = Pens.Black;

                _arrowPen = new Pen(Color.DarkGray)
                {
                    Width = 3,
                    StartCap = LineCap.ArrowAnchor,
                };
            }

            public bool IsSaveButtonSelected { get; set; }

            public bool IsRedoButtonSelected { get; set; }

            public bool IsUndoButtonSelected { get; set; }

            public bool IsClearButtonSelected { get; set; }

            public bool IsSaveButtonPressed { get; set; }

            public bool IsRedoButtonPressed { get; set; }

            public bool IsUndoButtonPressed { get; set; }

            public bool IsClearButtonPressed { get; set; }

            public void Draw(Graphics g)
            {
                g.FillRectangle(_bgHatchBrush, _bounds);
                g.FillRectangle(_bgGradientBrush, _bounds);
                g.DrawRectangle(_pen, _bounds.X, _bounds.Y,
                    _bounds.Width, _bounds.Height);
                DrawSaveIcon(g, _saveBtnBounds);
                DrawButton(g, IsSaveButtonSelected, _saveBtnBounds);
                DrawRedoIcon(g, _redoBtnBounds);
                DrawButton(g, IsRedoButtonSelected, _redoBtnBounds);
                DrawUndoIcon(g, _undoBtnBounds);
                DrawButton(g, IsUndoButtonSelected, _undoBtnBounds);
                DrawClearIcon(g, _clearBtnBounds);
                DrawButton(g, IsClearButtonSelected, _clearBtnBounds);
                FillButton(g, IsSaveButtonPressed, _saveBtnBounds);
                FillButton(g, IsRedoButtonPressed, _redoBtnBounds);
                FillButton(g, IsUndoButtonPressed, _undoBtnBounds);
                FillButton(g, IsClearButtonPressed, _clearBtnBounds);
            }

            private void DrawRedoIcon(Graphics g, RectangleF redoBtnBounds)
            {
                Image image = Properties.Resources.RedoIcon;
                g.DrawImage(image, redoBtnBounds);
            }

            private void DrawUndoIcon(Graphics g, RectangleF undoBtnBounds)
            {
                Image image = Properties.Resources.UndoIcon;
                g.DrawImage(image, undoBtnBounds);
            }

            private void DrawClearIcon(Graphics g, RectangleF clearBtnBounds)
            {
                Image image = Properties.Resources.ClearingIcon;
                g.DrawImage(image, clearBtnBounds);
            }

            private void DrawSaveIcon(Graphics g, RectangleF saveBtnBounds)
            {
                Image image = Properties.Resources.SavingIcon;
                g.DrawImage(image, saveBtnBounds);
            }

            private void FillButton(Graphics g, bool isPressed, RectangleF rect)
            {
                if (isPressed)
                {
                    g.FillRectangle(_btnSelectionBrush, rect);
                }
            }

            private void DrawButton(Graphics g, bool isSelected, RectangleF rect)
            {
                if (isSelected)
                {
                    g.DrawRectangle(_selectionPen, rect.X, rect.Y,
                        rect.Width, rect.Height);
                }

                else
                {
                    g.DrawRectangle(_pen, rect.X, rect.Y,
                        rect.Width, rect.Height);
                }
            }

            public void ChangeContainer(RectangleF container)
            {
                _container = container;
                ConstructBounds();
            }

            public bool IsSaveButtonFocused(PointF point)
            {
                return _saveBtnBounds.Contains(point);
            }

            public bool IsRedoButtonFocused(PointF point)
            {
                return _redoBtnBounds.Contains(point);
            }

            public bool IsUndoButtonFocused(PointF point)
            {
                return _undoBtnBounds.Contains(point);
            }

            public bool IsClearButtonFocused(PointF point)
            {
                return _clearBtnBounds.Contains(point);
            }

            private void ConstructBounds()
            {
                float widthB = _container.Width - 8;
                float heightB = _container.Height - 4;
                _bounds = new RectangleF()
                {
                    X = _container.X + 4,
                    Y = _container.Y + 2,
                    Width = widthB > 1 ? widthB : 1,
                    Height = heightB > 7 ? heightB : 7
                };

                _splitterWidth = (int)(_bounds.Width / 40);

                float height = _bounds.Height - 6;

                _clearBtnBounds = new RectangleF()
                {
                    X = _bounds.X + _splitterWidth,
                    Y = _bounds.Y + 3,
                    Height = height,
                    Width = height,
                };

                _saveBtnBounds = new RectangleF()
                {
                    X = _clearBtnBounds.Right + _splitterWidth,
                    Y = _bounds.Y + 3,
                    Height = height,
                    Width = height,
                };

                _redoBtnBounds = new RectangleF()
                {
                    X = _saveBtnBounds.Right + _splitterWidth,
                    Y = _bounds.Y + 3,
                    Height = height,
                    Width = height,
                };

                _undoBtnBounds = new RectangleF()
                {
                    X = _redoBtnBounds.Right + _splitterWidth,
                    Y = _bounds.Y + 3,
                    Height = height,
                    Width = height,
                };

                _bgGradientBrush = new LinearGradientBrush(_bounds,
                    Color.Transparent, Color.White, LinearGradientMode.Horizontal);
            }
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
            private Dictionary<int, GroupType> _groups;
            private float _height = 250;
            private float _titleHeight = 15;
            private float _correctHeight = 70;
            private float _yIndent;
            private float _groupsSplitter = 3;
            private float _groupPanelHeight = 20;
            private readonly List<GroupPanel> _groupPanels;

            private readonly CellStateButton _cellStateButton;

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

                _groups = new Dictionary<int, GroupType>();
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

            public Dictionary<int, GroupType> Groups 
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
                    _bounds.Width * 0.04f - _bounds.Width * 0.08f;
                _cellStateButton.Y = _bounds.Top + _bounds.Height * 0.02f;
                _cellStateButton.Height = _titleBounds.Height * 1.5f;
                _cellStateButton.Width = _cellStateButton.Height;
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

                public GroupType Type { get; set; }

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
                        Height = Height,
                        Width = Height
                    };
                }

                public bool IsFocused(PointF point)
                {
                    return _bounds.Contains(point);
                }
            }
        }

        private class GroupEditingPanel
        {
            private RectangleF _container;
            private RectangleF _bounds;
            private RectangleF _typeBoxBounds;
            private RectangleF _idBoxBounds;
            private RectangleF _editingBtnBounds;
            private RectangleF _addingBtnImageBounds;
            private string _enteredID;
            private string _enteredType;
            private readonly Pen _pen;
            private readonly Pen _selectionBoxPen;
            private readonly Pen _selectionBtnPen;
            private readonly Pen _enteringPen;
            private readonly Brush _eneteredBrush;
            private readonly Brush _selectionBrush;
            private readonly StringFormat _format;
            private Font _font;
            private readonly Image _image;

            public GroupEditingPanel(RectangleF container)
            {
                _enteredID = string.Empty;
                _enteredType = string.Empty;
                _container = container;
                _format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };

                _pen = new Pen(Color.Gray)
                {
                    Width = 1
                };
                _selectionBoxPen = new Pen(Color.LightGray)
                {
                    Width = 0.7f
                };
                _selectionBtnPen = new Pen(Color.Black)
                {
                    Width = 1.2f
                };
                _enteringPen = new Pen(Color.Black)
                {
                    Width = 1
                };

                _eneteredBrush = new SolidBrush(Color.Gray);
                _selectionBrush = new SolidBrush(Color.LightGray);

                _font = new Font("Times New Roman", 18);

                _image = Properties.Resources.ChangingIcon;
                ConstructBounds();
            }

            public RectangleF Bounds { get { return _bounds; } }

            public bool IsIDBoxSelected { get; set; }

            public bool IsTypeBoxSelected { get; set; }

            public bool IsIDEntering { get; set; }

            public bool IsTypeEntering { get; set; }

            public bool IsButtonSelected { get; set; }

            public bool IsButtonPressed { get; set; }

            public string EnteredID
            {
                get
                {
                    return _enteredID;
                }

                set
                {
                    if (_enteredID != null)
                    {
                        if (value.Length > 3)
                        {
                            value = value.Substring(0, 3);
                        }

                        _enteredID = value;
                    }
                }
            }

            public string EnteredType
            {
                get
                {
                    return _enteredType;
                }

                set
                {
                    if (value != null)
                        _enteredType = value;
                }
            }

            public void Draw(Graphics g)
            {
                g.DrawRectangle(_pen, _bounds.X, 
                    _bounds.Y, _bounds.Width, _bounds.Height);

                if (IsIDBoxSelected)
                {
                    g.DrawRectangle(_selectionBoxPen, _idBoxBounds.X,
                        _idBoxBounds.Y, _idBoxBounds.Width, _idBoxBounds.Height);
                }

                if (IsTypeBoxSelected)
                {
                    g.DrawRectangle(_selectionBoxPen, _typeBoxBounds.X,
                        _typeBoxBounds.Y, _typeBoxBounds.Width, _typeBoxBounds.Height);
                }

                if (IsIDEntering)
                {
                    g.DrawRectangle(_enteringPen, _idBoxBounds.X,
                        _idBoxBounds.Y, _idBoxBounds.Width, _idBoxBounds.Height);
                }

                if (IsTypeEntering)
                {
                    g.DrawRectangle(_enteringPen, _typeBoxBounds.X,
                        _typeBoxBounds.Y, _typeBoxBounds.Width, _typeBoxBounds.Height);
                }

                if (IsButtonPressed)
                {
                    g.FillRectangle(_selectionBrush, _editingBtnBounds);
                }

                if (IsButtonSelected)
                {
                    g.DrawRectangle(_selectionBtnPen, _editingBtnBounds.X,
                        _editingBtnBounds.Y, _editingBtnBounds.Width, _editingBtnBounds.Height);
                }

                else
                {
                    g.DrawRectangle(_pen, _editingBtnBounds.X,
                        _editingBtnBounds.Y, _editingBtnBounds.Width, _editingBtnBounds.Height);
                }

                g.DrawString(_enteredID, _font, 
                    _eneteredBrush, _idBoxBounds, _format);
                g.DrawString(_enteredType, _font,
                    _eneteredBrush, _typeBoxBounds, _format);

                g.DrawImage(_image, _addingBtnImageBounds);
            }

            public void ChangeContainer(RectangleF container)
            {
                _container = container;
                ConstructBounds();
            }

            public void ConstructBounds()
            {
                _bounds = new RectangleF()
                {
                    X = _container.X + 5,
                    Y = _container.Y + 5,
                    Width = _container.Width - 10,
                    Height = _container.Height - 10
                };

                _idBoxBounds = new RectangleF()
                {
                    X = _bounds.X + 3,
                    Y = _bounds.Y + 3,
                    Width = _bounds.Width * 0.4f - 6,
                    Height = _bounds.Height - 6
                };

                _typeBoxBounds = new RectangleF()
                {
                    X = _idBoxBounds.Right + 3,
                    Y = _bounds.Y + 3,
                    Width = _bounds.Width * 0.4f,
                    Height = _bounds.Height - 6
                };

                _editingBtnBounds = new RectangleF()
                {
                    X = _typeBoxBounds.Right + 3,
                    Y = _bounds.Y,
                    Width = _bounds.Width - _idBoxBounds.Width
                        - _typeBoxBounds.Width - 6,
                    Height = _bounds.Height
                };

                float width = _editingBtnBounds.Width;
                float height = _editingBtnBounds.Height;
                bool isWidhtBigger = width > height;
                float smallerSide = isWidhtBigger ? height : width;

                float indent = (isWidhtBigger ?
                    width - smallerSide : height - smallerSide) / 2;

                float x = _editingBtnBounds.X + (isWidhtBigger ? indent : 0);
                float y = _editingBtnBounds.Y + (isWidhtBigger ? 0 : indent);

                _addingBtnImageBounds = new RectangleF()
                {
                    X = x,
                    Y = y,
                    Width = smallerSide,
                    Height = smallerSide
                };
            }

            public bool IsIDBoxFocused(PointF point)
            {
                return _idBoxBounds.Contains(point);
            }

            public bool IsTypeBoxFocused(PointF point)
            {
                return _typeBoxBounds.Contains(point);
            }

            public bool IsButtonFocused(PointF point)
            {
                return _editingBtnBounds.Contains(point);
            }
        }

        private void DrawLeftTopDeco(Graphics g)
        {
            PointF begin = new PointF(_mapDisplayRect.Width + 7, _menuRect.Bottom + 6);
            float endX = Width - 10;
            float endY = Height - _menuRect.Bottom - 10;

            g.DrawLine(_leftTopDecoPen, begin.X, begin.Y, endX, begin.Y);
            g.DrawLine(_leftTopDecoPen, begin.X, begin.Y, begin.X, endY);

            g.FillEllipse(Brushes.Gray, _mapDisplayRect.Width + 4,
                _menuRect.Bottom + 3, 6, 6);
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
                X = _mapDisplayRect.Width,
                Y = 0
            };

            _menuRect = new RectangleF()
            {
                X = _cellEditRect.X,
                Y = _cellEditRect.Y,
                Width = _cellEditRect.Width,
                Height = _cellEditRect.Height * 0.07f
            };

            _selectedCellsListRect = new RectangleF()
            {
                X = _cellEditRect.X + 15,
                Y = _menuRect.Bottom + 15,
                Width = _cellEditRect.Width - 30,
                Height = _cellEditRect.Height * 0.78f
            };

            _editRect = new RectangleF()
            {
                X = _cellEditRect.X,
                Y = _selectedCellsListRect.Bottom + 20,
                Width = _cellEditRect.Width - 1,
                Height = _cellEditRect.Height -
                    _selectedCellsListRect.Height -
                    _menuRect.Height - 35
            };

            _editingPanel?.ChangeContainer(_editRect);
            _menuPanel?.ChangeContainer(_menuRect);
            _scroll = 0;
            UpdateShowingCells();
            ValidateScroll();
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
                {
                    _selectionRectBegin = e.Location;
                    _selectionRectEnd = e.Location;
                }

                _isSelecting = true;

                _menuPanel.IsClearButtonPressed = _menuPanel.IsClearButtonFocused(e.Location);
                _menuPanel.IsRedoButtonPressed = _menuPanel.IsRedoButtonFocused(e.Location);
                _menuPanel.IsUndoButtonPressed = _menuPanel.IsUndoButtonFocused(e.Location);
                _menuPanel.IsSaveButtonPressed = _menuPanel.IsSaveButtonFocused(e.Location);
                _editingPanel.IsButtonPressed = _editingPanel.IsButtonSelected;

                Invalidate();
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

            _editingPanel.IsButtonSelected = _editingPanel.IsButtonFocused(e.Location);
            _editingPanel.IsIDBoxSelected = _editingPanel.IsIDBoxFocused(e.Location);
            _editingPanel.IsTypeBoxSelected = _editingPanel.IsTypeBoxFocused(e.Location);
            _menuPanel.IsSaveButtonSelected = _menuPanel.IsSaveButtonFocused(e.Location);
            _menuPanel.IsClearButtonSelected = _menuPanel.IsClearButtonFocused(e.Location);
            _menuPanel.IsRedoButtonSelected = _menuPanel.IsRedoButtonFocused(e.Location);
            _menuPanel.IsUndoButtonSelected = _menuPanel.IsUndoButtonFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!_isShiftPressed && _mapDisplayRect.Contains(e.Location))
                    _map.ClearSelection();

                if (_isSelecting)
                {
                    _selectionRectEnd = e.Location;
                    ConstructSelectionRect();
                    Size mapSize = new Size(_map.ColumnsCount, _map.RowsCount);
                    List<Point> cells = _drawer.GetCells(_selectionRect,
                        mapSize, _mapDisplayRect.Size);
                    foreach (var cellPos in cells)
                    {
                        if (cellPos.X > -1 && cellPos.X < _map.ColumnsCount
                        && cellPos.Y > -1 && cellPos.Y < _map.RowsCount)
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

                if (!string.IsNullOrEmpty(_editingPanel.EnteredID)
                    && _editingPanel.IsButtonSelected)
                {
                    GroupType type;

                    switch (_editingPanel.EnteredType)
                    {
                        case "sum":
                            type = GroupType.Sum;
                            break;
                        default:
                            type = GroupType.Basic;
                            break;
                    }

                    int id = int.Parse(_editingPanel.EnteredID);
                    if (_map.AddSelectedToGroup(id))
                        _map.ChangeGroupType(id, type);
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                _map.ClearSelection();
            }

            _editingPanel.IsIDEntering = _editingPanel.IsIDBoxFocused(e.Location);
            _editingPanel.IsTypeEntering = _editingPanel.IsTypeBoxFocused(e.Location);

            if (_menuPanel.IsClearButtonPressed)
            {
                _map.Clear();
            }

            if (_menuPanel.IsRedoButtonPressed)
            {
                _map.Redo();
            }

            if (_menuPanel.IsUndoButtonPressed)
            {
                _map.Undo();
            }

            if (_menuPanel.IsSaveButtonPressed)
            {
                if (!string.IsNullOrEmpty(SavePath))
                {
                    using (Stream stream = File.Open(SavePath, 
                        FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(_map.GetType());
                        serializer.WriteObject(stream, _map);
                    }
                }
            }

            _menuPanel.IsClearButtonPressed = false;
            _menuPanel.IsRedoButtonPressed = false;
            _menuPanel.IsUndoButtonPressed = false;
            _menuPanel.IsSaveButtonPressed = false;
            _editingPanel.IsButtonPressed = false;

            _isSelecting = false;
            UpdateShowingCells();
            ValidateScroll();
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_showingCells.Count == 0)
                return;

            float changing = e.Delta / 4;
            _scroll += changing;

            ValidateScroll();
            Invalidate();
        }

        private void ValidateScroll()
        {
            _showingCellsSumHeight = 0;
            foreach (var cell in _showingCells)
            {
                _showingCellsSumHeight += cell.Height + _cellPanelsSplitterWidth;
            }

            if (_scroll > 0)
                _scroll = 0;

            if (_scroll < _selectedCellsListRect.Height - _showingCellsSumHeight)
                _scroll = _selectedCellsListRect.Height - _showingCellsSumHeight;

            if (_selectedCellsListRect.Height > _showingCellsSumHeight)
                _scroll = 0;
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
                    case Keys.G:
                        _map.GroupSelected(GroupType.Basic);
                        break;
                    case Keys.S:
                        _map.GroupSelected(GroupType.Sum);
                        break;
                    case Keys.O:
                        _map.ChangeSelectedAvailability();
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
                    case Keys.Back:
                        if (_editingPanel.IsIDEntering && 
                            !string.IsNullOrEmpty(_editingPanel.EnteredID))
                        {
                            string ent = _editingPanel.EnteredID;
                            _editingPanel.EnteredID = ent.Remove(ent.Length - 1, 1);
                        }
                        if (_editingPanel.IsTypeEntering 
                            && !string.IsNullOrEmpty(_editingPanel.EnteredType))
                        {
                            _editingPanel.EnteredType = string.Empty;
                        }
                        else _map.Write(0);
                        break;
                    case Keys.A:
                        _map.Write(10);
                        break;
                    case Keys.B:
                        if (_editingPanel.IsTypeEntering)
                        {
                            _editingPanel.EnteredType = "basic";
                        }
                        else _map.Write(11);
                        break;
                    case Keys.C:
                        _map.Write(12);
                        break;
                    case Keys.D:
                        _map.Write(13);
                        break;
                    case Keys.E:
                        _map.Write(14);
                        break;
                    case Keys.F:
                        _map.Write(15);
                        break;
                    case Keys.G:
                        _map.Write(16);
                        break;
                    case Keys.H:
                        _map.Write(17);
                        break;
                    case Keys.I:
                        _map.Write(18);
                        break;
                    case Keys.J:
                        _map.Write(19);
                        break;
                    case Keys.K:
                        _map.Write(20);
                        break;
                    case Keys.S:
                        if (_editingPanel.IsTypeEntering)
                        {
                            _editingPanel.EnteredType = "sum";
                        }
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
                if (_editingPanel.IsIDEntering)
                {
                    _editingPanel.EnteredID += e.KeyChar.ToString();
                }

                else 
                { 
                    _map.Write(int.Parse(e.KeyChar.ToString())); 
                }
            }

            UpdateShowingCells();
            Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _isShiftPressed = false;
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

                var groups = new Dictionary<int, GroupType>();
                foreach (var gID in cell.Groups)
                {
                    GroupInfo group = Map.GetGroups().Find(g => g.ID == gID);
                    groups.Add(group.ID, group.Type);
                }

                cellPanel.Groups = groups;
                _showingCells.Add(cellPanel);
            }
        }
    }
}
