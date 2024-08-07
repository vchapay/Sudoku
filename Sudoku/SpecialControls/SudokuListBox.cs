using System.Windows.Forms;
using System.Collections.Generic;
using Sudoku.MapLogic;
using System;
using System.Drawing;
using System.Linq;
namespace Sudoku.SpecialControls
{
    internal class SudokuListBox : Control
    {
        private const int _defaultMapInfoPanelHeight = 150;

        private readonly HashSet<Map> _maps;
        private readonly List<MapInfoPanel> _mapPanels;
        private int _scroll;
        private int _splitterWidth;
        private int _pageInd;

        private RectangleF _mapListRect;

        private BorderStyle _borderStyle;

        /// <summary>
        /// Инициализирует новый экземпляр SudokuListBox
        /// </summary>
        public SudokuListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
            Map map = new Map()
            {
                Name = "TestMap"
            };

            _maps = new HashSet<Map>()
            {
                map
            };

            _mapPanels = new List<MapInfoPanel>()
            {
                new MapInfoPanel(_mapListRect)
                {
                    Height = _defaultMapInfoPanelHeight,
                    MapName = map.Name,
                    MapSize = new Size(map.ColumnsCount, map.RowsCount)
                }
            };

            _splitterWidth = 3;
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

        public IReadOnlyCollection<Map> Maps { get { return _maps; } }

        public bool AddMap(Map map)
        {
            if (map == null)
                throw new ArgumentNullException();

            if (_maps.Where(m => m.Name == map.Name).Count() > 0)
                throw new ArgumentException("Карта с таким названием уже существует");

            if (_maps.Add(map))
            {
                MapInfoPanel panel = new MapInfoPanel(_mapListRect)
                {
                    Height = _defaultMapInfoPanelHeight,
                    MapSize = new Size(map.ColumnsCount, map.RowsCount),
                    MapName = map.Name,
                };

                _mapPanels.Add(panel);
                return true;
            }

            return false;
        }

        public bool RemoveMap(Map map)
        {
            if (map == null)
                throw new ArgumentNullException();

            if (_maps.Remove(map))
            {
                _mapPanels.RemoveAll(p => p.MapName == map.Name);
                return true;
            }

            return false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.White);
            g.DrawRectangle(Pens.Black, _mapListRect.X, _mapListRect.Y,
                _mapListRect.Width, _mapListRect.Height);

            int nextY = _scroll;
            for (int i = 0; i < _mapPanels.Count; i++)
            {
                var panel = _mapPanels[i];
                panel.YIndent = nextY;
                nextY += _splitterWidth + panel.Height;
                panel.Draw(g);
            }

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

        protected override void OnResize(EventArgs e)
        {
            _mapListRect = new RectangleF()
            {
                Width = Width * 0.55f,
                Height = Height - 3,
                X = (Width - _mapListRect.Width) / 2,
                Y = 1
            };

            foreach (var panel in _mapPanels)
            {
                panel.ChangeContainer(_mapListRect);
            }
        }

        private class MapInfoPanel
        {
            private RectangleF _container;
            private RectangleF _bounds;
            private RectangleF _viewBtnBounds;
            private RectangleF _delBtnBounds;
            private RectangleF _nameLlbBounds;
            private RectangleF _sizeLlbBounds;
            private Pen _pen;
            private Pen _selectionPen;
            private Brush _selectionBrush;
            private Brush _textBrush;
            private Font _font;
            private StringFormat _format;
            private int _yIndent;
            private int _height;

            public MapInfoPanel(RectangleF container)
            {
                _container = container;
                _height = 150;

                _pen = new Pen(Color.LightGray);
                _selectionPen = new Pen(Color.DarkGray);

                _selectionBrush = new SolidBrush(Color.LightGray);
                _textBrush = new SolidBrush(Color.Black);

                _font = new Font("Times New Roman", 20);

                _format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                }; 

                ConstructBounds();
            }

            public int YIndent
            {
                get { return _yIndent; }
                set 
                {
                    if (_yIndent != value)
                    {
                        _yIndent = value;
                        ConstructBounds();
                    }
                }
            }

            public int Height
            {
                get { return _height; }
                set
                {
                    if (_height != value && value > 0) 
                    {
                        _height = value;
                        ConstructBounds();
                    }
                }
            }

            public bool IsDeleteButtonSelected { get; set; }

            public bool IsViewButtonSelected { get; set; }

            public string MapName { get; set; }

            public Size MapSize { get; set; }

            public void Draw(Graphics g) 
            {
                DrawButton(g, _delBtnBounds, IsDeleteButtonSelected);
                DrawButton(g, _viewBtnBounds, IsViewButtonSelected);
                g.DrawString(MapName, _font, _textBrush, _nameLlbBounds, _format);
                g.DrawString($"{MapSize.Width}x{MapSize.Height}",
                    _font, _textBrush, _sizeLlbBounds, _format);
            }

            private void DrawButton(Graphics g, RectangleF bounds, bool isSelected)
            {
                Pen pen = _pen;
                if (isSelected)
                {
                    g.FillRectangle(_selectionBrush, bounds);
                    pen = _selectionPen;
                }

                g.DrawRectangle(pen, bounds.X, bounds.Y,
                    bounds.Width, bounds.Height);
            }

            public void ChangeContainer(RectangleF container)
            {
                _container = container;
                ConstructBounds();
            }

            public bool IsDeleteButtonFocused(PointF point)
            { 
                return _delBtnBounds.Contains(point);
            }

            public bool IsViewButtonFocused(PointF point)
            {
                return _viewBtnBounds.Contains(point);
            }

            private void ConstructBounds()
            {
                _bounds = new RectangleF()
                {
                    X = _container.X,
                    Y = _container.Y,
                    Width = _container.Width,
                    Height = _height
                };

                _viewBtnBounds = new RectangleF()
                {
                    Height = _bounds.Height - 10,
                    Width = _viewBtnBounds.Height,
                    X = _bounds.Right - 5 - _viewBtnBounds.Width,
                    Y = _bounds.Y + 5,
                };

                _delBtnBounds = new RectangleF()
                {
                    Height = _bounds.Height - 10,
                    Width = _delBtnBounds.Height,
                    X = _viewBtnBounds.Left - 5 - _delBtnBounds.Width,
                    Y = _bounds.Y + 5,
                };

                _nameLlbBounds = new RectangleF()
                {
                    X = _bounds.X + 3,
                    Y = _bounds.Y + 3,
                    Width = _bounds.Width * 0.3f,
                    Height = _bounds.Height
                };

                _sizeLlbBounds = new RectangleF()
                {
                    X = _bounds.X + 3,
                    Y = _bounds.Y + 3,
                    Width = _bounds.Width * 0.2f,
                    Height = _bounds.Height
                };
            }
        }
    }
}
