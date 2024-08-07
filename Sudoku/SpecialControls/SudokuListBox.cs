using System.Windows.Forms;
using System.Collections.Generic;
using Sudoku.MapLogic;
using System;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
namespace Sudoku.SpecialControls
{
    internal class SudokuListBox : Control
    {
        private const int _defaultMapInfoPanelHeight = 50;

        private readonly HashSet<Map> _maps;
        private readonly List<MapInfoPanel> _mapPanels;
        private int _scroll;
        private int _splitterWidth;
        private int _pageInd;

        private Rectangle _mapListRect;

        private BorderStyle _borderStyle;
        private int _showingMapsSumHeight;

        private HatchBrush _bgBrush;

        /// <summary>
        /// Инициализирует новый экземпляр SudokuListBox
        /// </summary>
        public SudokuListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            Width = 300;
            Height = 300;
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
                },
            };

            _splitterWidth = 3;

            _bgBrush = new HatchBrush(HatchStyle.LargeGrid,
                Color.White, Color.FromArgb(50, 180, 160, 200));
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

        public event EditButtonClickHandler EditButtonClicked;

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

            g.FillRectangle(_bgBrush, ClientRectangle);
            g.FillRectangle(Brushes.White, _mapListRect);
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
            _mapListRect = new Rectangle()
            {
                Width = (int)(Width * 0.55f),
                Height = Height - 3,
                X = (int)(Width - Width * 0.55f) / 2,
                Y = 1
            };

            if (_mapPanels != null)
            {
                foreach (var panel in _mapPanels)
                {
                    panel.ChangeContainer(_mapListRect);
                }
            }

            ValidateScroll();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            foreach(var panel in _mapPanels)
            {
                panel.IsDeleteButtonSelected =
                    panel.IsDeleteButtonFocused(e.Location);
                panel.IsViewButtonSelected =
                    panel.IsViewButtonFocused(e.Location);
            }

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_mapPanels.Count == 0)
                return;

            int changing = e.Delta / 4;
            _scroll += changing;

            ValidateScroll();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                foreach (var panel in _mapPanels)
                {
                    if (panel.IsViewButtonSelected)
                    {
                        OnEditButtonClick(new EditButtonClickArgs(
                            _maps.Where(m => m.Name == panel.MapName).First()));
                    }
                }
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
            private HatchBrush _bgHatchBrush;
            private LinearGradientBrush _bgGradientBrush;
            private Font _font;
            private StringFormat _format;
            private int _yIndent;
            private int _height;

            public MapInfoPanel(RectangleF container)
            {
                _container = container;
                _height = 100;
                ConstructBounds();

                _pen = new Pen(Color.LightGray) 
                { 
                    Width = 2
                };
                _selectionPen = new Pen(Color.DarkGray);

                _selectionBrush = new SolidBrush(Color.FromArgb(100, 150, 150, 150));
                _textBrush = new SolidBrush(Color.Gray);
                _bgHatchBrush = new HatchBrush(HatchStyle.Cross, 
                    Color.White, Color.FromArgb(200, 230, 230, 250));

                _font = new Font("Times New Roman", 26);

                _format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
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
                g.FillRectangle(_bgHatchBrush, _bounds);
                g.FillRectangle(_bgGradientBrush, _bounds);
                Image image = Properties.Resources.ClearingIcon;
                g.DrawImage(image, _delBtnBounds);
                image = Properties.Resources.ChangingIcon;
                g.DrawImage(image, _viewBtnBounds);
                g.DrawRectangle(_pen, _bounds.X, _bounds.Y,
                    _bounds.Width, _bounds.Height);
                DrawButton(g, _delBtnBounds, IsDeleteButtonSelected);
                DrawButton(g, _viewBtnBounds, IsViewButtonSelected);

                string trimmingName = MapName;
                if (trimmingName.Length > 8)
                {
                    trimmingName = trimmingName.Substring(0, 8) + "...";
                }

                g.DrawString(trimmingName, _font, _textBrush, _nameLlbBounds, _format);
                g.DrawString($"{MapSize.Width}x{MapSize.Height}",
                    _font, _textBrush, _sizeLlbBounds, _format);
            }

            public bool IsFocused(PointF point)
            {
                return _bounds.Contains(point);
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

            private void ConstructBounds()
            {
                _bounds = new RectangleF()
                {
                    X = _container.X + 4,
                    Y = _yIndent + _container.Y + 4,
                    Width = _container.Width - 8,
                    Height = _height
                };

                _viewBtnBounds = new RectangleF()
                {
                    Height = _bounds.Height - 10,
                    Width = _viewBtnBounds.Height,
                    X = _bounds.Right - 5 - _viewBtnBounds.Height,
                    Y = _bounds.Y + 5,
                };

                _delBtnBounds = new RectangleF()
                {
                    Height = _bounds.Height - 10,
                    Width = _delBtnBounds.Height,
                    X = _viewBtnBounds.Left - 5 - _delBtnBounds.Height,
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
                    X = _nameLlbBounds.Right + 3,
                    Y = _bounds.Y + 3,
                    Width = _bounds.Width * 0.2f,
                    Height = _bounds.Height
                };

                _bgGradientBrush = new LinearGradientBrush(_bounds,
                    Color.Transparent, Color.White, LinearGradientMode.Horizontal);
            }
        }

        private void ValidateScroll()
        {
            if (_mapPanels == null)
                return;

            _showingMapsSumHeight = 0;
            foreach (var panel in _mapPanels)
            {
                _showingMapsSumHeight += panel.Height + _splitterWidth;
            }

            if (_scroll > 0)
                _scroll = 0;

            if (_scroll < _mapListRect.Height - _showingMapsSumHeight - 5)
                _scroll = _mapListRect.Height - _showingMapsSumHeight - 5;

            if (_mapListRect.Height > _showingMapsSumHeight)
                _scroll = 0;
        }

        private void OnEditButtonClick(EditButtonClickArgs e)
        {
            EditButtonClicked?.Invoke(this, e);
        }
    }

    internal class EditButtonClickArgs
    {
        private readonly Map _map;

        public EditButtonClickArgs(Map map)
        {
            _map = map;
        }

        public Map OpenedMap => _map;
    }

    delegate void EditButtonClickHandler(object sender, EditButtonClickArgs e);
}
