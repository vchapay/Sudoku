using System.Windows.Forms;
using System.Collections.Generic;
using Sudoku.MapLogic;
using System;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
namespace Sudoku.Controls
{
    internal class SudokuMapsPage : Control
    {
        private const int _defaultMapInfoPanelHeight = 50;

        private readonly HashSet<Map> _maps;
        private readonly List<MapInfoPanel> _mapPanels;
        private int _scroll;
        private int _splitterWidth;

        private Rectangle _mapListRect;
        private Rectangle _bottomPanelRect;

        private SudokuControlModel _createMapBtn;
        private SudokuControlModel _importMapBtn;
        private SudokuControlModel _searchBtn;
        private SudokuControlModel _resetSearchBtn;
        private SudokuControlModel _searchBox;

        private BorderStyle _borderStyle;
        private int _showingMapsSumHeight;

        private HatchBrush _bgHatchBrush;
        private LinearGradientBrush _bgGradientBrush;

        private string _mapsDirPath;

        /// <summary>
        /// Инициализирует новый экземпляр SudokuMapsPage
        /// </summary>
        public SudokuMapsPage()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            _createMapBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.AddingIcon,
                BackColor = Color.FromArgb(100, 100, 140, 230)
            };

            _importMapBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.ImportIcon,
                BackColor = Color.FromArgb(70, 90, 180, 230)
            };

            _searchBox = new SudokuControlModel()
            {
                BackColor = Color.FromArgb(100, 180, 180, 210),
                TextTrimming = 32
            };

            _searchBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.SearchingIcon,
                BackColor = Color.FromArgb(100, 160, 200, 240)
            };

            _resetSearchBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.CrossIcon,
                BackColor = Color.FromArgb(140, 230, 180, 170)
            };

            Width = 300;
            Height = 300;
            DoubleBuffered = true;
            Map map1 = new Map()
            {
                Name = "TestMap"
            };

            Map map2 = new Map()
            {
                Name = "TemtMap"
            };

            Map map3 = new Map()
            {
                Name = "TespPamEfeearfd"
            };

            _maps = new HashSet<Map>()
            {
                map1,
                map2,
                map3
            };

            _mapPanels = new List<MapInfoPanel>()
            {
                new MapInfoPanel(_mapListRect)
                {
                    Height = _defaultMapInfoPanelHeight,
                    MapName = map1.Name,
                    MapSize = new Size(map1.ColumnsCount, map1.RowsCount),
                    IsVisible = true,
                },

                new MapInfoPanel(_mapListRect)
                {
                    Height = _defaultMapInfoPanelHeight,
                    MapName = map2.Name,
                    MapSize = new Size(map2.ColumnsCount, map2.RowsCount),
                    IsVisible = true,
                },

                new MapInfoPanel(_mapListRect)
                {
                    Height = _defaultMapInfoPanelHeight,
                    MapName = map3.Name,
                    MapSize = new Size(map3.ColumnsCount, map3.RowsCount),
                    IsVisible = true,
                },
            };

            _splitterWidth = 3;

            _bgHatchBrush = new HatchBrush(HatchStyle.Sphere,
                Color.White, Color.FromArgb(50, 180, 160, 200));

            _bgGradientBrush = new LinearGradientBrush(ClientRectangle, 
                Color.FromArgb(120, 250, 250, 250), 
                Color.FromArgb(70, 190, 190, 210), 72);
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
        /// Возвращает коллекцию карт для контрола
        /// </summary>
        public IReadOnlyCollection<Map> Maps { get { return _maps; } }

        /// <summary>
        /// Происходит при нажатии кнопки "удалить карту".
        /// </summary>
        public event MapPanelButtonClickHandler DeleteMapButtonClicked;

        /// <summary>
        /// Происходит при нажатии кнопки "открыть карту".
        /// </summary>
        public event MapPanelButtonClickHandler ViewMapButtonClicked;

        /// <summary>
        /// Происходит при нажатии кнопки "выгрузить карту".
        /// </summary>
        public event MapPanelButtonClickHandler ExportMapButtonClicked;

        /// <summary>
        /// Происходит при нажатии кнопки "открыть карту".
        /// </summary>
        public event EventHandler CreateMapButtonClicked;

        /// <summary>
        /// Происходит при нажатии кнопки "выгрузить карту".
        /// </summary>
        public event EventHandler ImportMapButtonClicked;

        /// <summary>
        /// Добавляет карту в коллекцию контрола.
        /// Названия карт не должны повторяться.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
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
                    IsVisible = true
                };

                _mapPanels.Add(panel);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Удаляет карту из коллекции контрола.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
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

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRectangle(_bgHatchBrush, ClientRectangle);
            g.FillRectangle(_bgGradientBrush, ClientRectangle);
            g.FillRectangle(Brushes.White, _mapListRect);
            g.DrawRectangle(Pens.Black, _mapListRect.X, _mapListRect.Y,
                _mapListRect.Width, _mapListRect.Height);

            _createMapBtn.Draw(g);
            _importMapBtn.Draw(g);
            _searchBox.Draw(g);
            _searchBtn.Draw(g);
            _resetSearchBtn.Draw(g);

            int nextY = _scroll;
            for (int i = 0; i < _mapPanels.Count; i++)
            {
                var panel = _mapPanels[i];
                if (panel.IsVisible)
                {
                    panel.YIndent = nextY;
                    nextY += _splitterWidth + panel.Height;
                    panel.Draw(g);
                }
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
                Width = Width * 6 / 10,
                Height = Height * 7 / 10,
                X = (Width - Width * 6 / 10) / 2,
                Y = Height * 1 / 10
            };

            _bottomPanelRect = new Rectangle()
            {
                Width = _mapListRect.Width * 9 / 10,
                Height = Height - _mapListRect.Bottom,
                X = _mapListRect.X + (_mapListRect.Width -
                    _mapListRect.Width * 9 / 10) / 2,
                Y = _mapListRect.Bottom
            };

            _createMapBtn.Height = _bottomPanelRect.Height - 10;
            _createMapBtn.Width = _createMapBtn.Height;
            _createMapBtn.X = _bottomPanelRect.Right - _createMapBtn.Width;
            _createMapBtn.Y = _bottomPanelRect.Top + 5;

            _importMapBtn.Height = _bottomPanelRect.Height - 10;
            _importMapBtn.Width = _importMapBtn.Height;
            _importMapBtn.X = _bottomPanelRect.Left;
            _importMapBtn.Y = _bottomPanelRect.Top + 5;

            _searchBox.Height = _mapListRect.Top - 10;
            _searchBox.Width = _mapListRect.Width * 7 / 10;
            _searchBox.X = _mapListRect.X + (_mapListRect.Width -
                _mapListRect.Width * 7 / 10) / 2;
            _searchBox.Y = 5;

            _searchBtn.Height = _searchBox.Height;
            _searchBtn.Width = _searchBtn.Height;
            _searchBtn.X = _searchBox.Right + 10;
            _searchBtn.Y = 5;

            _resetSearchBtn.Height = _searchBox.Height;
            _resetSearchBtn.Width = _searchBtn.Height;
            _resetSearchBtn.X = _searchBox.Left -
                _resetSearchBtn.Width - 10;
            _resetSearchBtn.Y = 5;

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
                panel.DeleteButton.IsSelected =
                    panel.DeleteButton.IsFocused(e.Location);
                panel.ViewButton.IsSelected =
                    panel.ViewButton.IsFocused(e.Location);
                panel.ExportButton.IsSelected =
                    panel.ExportButton.IsFocused(e.Location);
            }

            _searchBtn.IsSelected = _searchBtn.IsFocused(e.Location);
            _searchBox.IsSelected = _searchBox.IsFocused(e.Location);
            _resetSearchBtn.IsSelected = _resetSearchBtn.IsFocused(e.Location);
            _createMapBtn.IsSelected = _createMapBtn.IsFocused(e.Location);
            _importMapBtn.IsSelected = _importMapBtn.IsFocused(e.Location);

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            foreach (var panel in _mapPanels)
            {
                panel.DeleteButton.IsPressed =
                    panel.DeleteButton.IsFocused(e.Location);
                panel.ViewButton.IsPressed =
                    panel.ViewButton.IsFocused(e.Location);
                panel.ExportButton.IsPressed =
                    panel.ExportButton.IsFocused(e.Location);
            }

            _searchBtn.IsPressed = _searchBtn.IsFocused(e.Location);
            _searchBox.IsPressed = _searchBox.IsFocused(e.Location);
            _resetSearchBtn.IsPressed = _resetSearchBtn.IsFocused(e.Location);
            _createMapBtn.IsPressed = _createMapBtn.IsFocused(e.Location);
            _importMapBtn.IsPressed = _importMapBtn.IsFocused(e.Location);

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < _mapPanels.Count; i++)
                {
                    MapInfoPanel panel = _mapPanels[i];

                    if (panel.ViewButton.IsPressed)
                    {
                        OnViewButtonClick(new MapPanelButtonClickArgs(
                            _maps.Where(m => m.Name == panel.MapName).First()));
                    }

                    if (panel.DeleteButton.IsPressed)
                    {
                        OnDeleteButtonClick(new MapPanelButtonClickArgs(
                            _maps.Where(m => m.Name == panel.MapName).First()));

                        _mapPanels.RemoveAll(p => p == panel);
                        _maps.RemoveWhere(m => m.Name == panel.MapName);
                        i--;
                    }

                    if (panel.ExportButton.IsPressed)
                    {
                        OnExportButtonClick(new MapPanelButtonClickArgs(
                            _maps.Where(m => m.Name == panel.MapName).First()));
                    }

                    panel.ViewButton.IsPressed = false;
                    panel.DeleteButton.IsPressed = false;
                    panel.ExportButton.IsPressed = false;
                }
            }

            if (_searchBtn.IsPressed)
                ShowSearchResult(_searchBox.Text);

            if (_resetSearchBtn.IsPressed)
            {
                _searchBox.Text = string.Empty;
                ResetSearchResult();
            }

            if (_importMapBtn.IsPressed)
            {
                OnImportButtonClick(EventArgs.Empty);
            }

            if (_createMapBtn.IsPressed)
            {
                OnCreateButtonClick(EventArgs.Empty);
            }

            _searchBtn.IsPressed = false;
            _resetSearchBtn.IsPressed = false;
            _createMapBtn.IsPressed = false;
            _importMapBtn.IsPressed = false;

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

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (_searchBox.IsPressed)
            {
                if (e.KeyChar != '\b' && e.KeyChar != '\r')
                {
                    _searchBox.Text += e.KeyChar;
                    Invalidate();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                if (!_searchBox.IsPressed)
                    return;

                if (_searchBox.Text.Length > 0)
                {
                    _searchBox.Text = _searchBox.Text.Remove(
                        _searchBox.Text.Length - 1);
                }
            }

            if (e.KeyCode == Keys.Enter) 
            {
                if (!_searchBox.IsPressed)
                    return;

                if (_searchBox.Text.Length == 0)
                    ResetSearchResult();

                else
                    ShowSearchResult(_searchBox.Text);
            }

            Invalidate();
        }

        private void ShowSearchResult(string name)
        {
            foreach (var panel in _mapPanels) 
            {
                if (panel.MapName.Contains(name))
                {
                    panel.IsVisible = true;
                }

                else
                {
                    panel.IsVisible = false;
                }
            }
        }

        private void ResetSearchResult()
        {
            foreach (var panel in _mapPanels)
            {
                panel.IsVisible = true;
            }
        }

        private class MapInfoPanel
        {
            private Rectangle _container;
            private Rectangle _bounds;
            private SudokuControlModel _viewBtn;
            private SudokuControlModel _delBtn;
            private SudokuControlModel _exportBtn;
            private Rectangle _nameLlbBounds;
            private Rectangle _sizeLlbBounds;
            private Rectangle _typeLlbBounds;
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

            public MapInfoPanel(Rectangle container)
            {
                _container = container;
                _height = 100;

                _viewBtn = new SudokuControlModel();
                _delBtn = new SudokuControlModel();
                _exportBtn = new SudokuControlModel();

                _viewBtn.Image = Properties.Resources.ChangingIcon;
                _delBtn.Image = Properties.Resources.ClearingIcon;
                _exportBtn.Image = Properties.Resources.ExportIcon;

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

                IsVisible = true;
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

            public SudokuControlModel ViewButton => _viewBtn;

            public SudokuControlModel ExportButton => _exportBtn;

            public SudokuControlModel DeleteButton => _delBtn;

            public bool IsVisible { get; set; }

            public string MapName { get; set; }

            public MapTypes MapType { get; set; }

            public Size MapSize { get; set; }

            public void Draw(Graphics g) 
            {
                g.FillRectangle(_bgHatchBrush, _bounds);
                g.FillRectangle(_bgGradientBrush, _bounds);
                g.DrawRectangle(_pen, _bounds.X, _bounds.Y,
                    _bounds.Width, _bounds.Height);

                _viewBtn.Draw(g);
                _delBtn.Draw(g);
                _exportBtn.Draw(g);

                if (MapName == null)
                    return;

                string trimmingName = MapName;
                if (trimmingName.Length > 8)
                {
                    trimmingName = trimmingName.Substring(0, 8) + "...";
                }

                g.DrawString(trimmingName, _font, 
                    _textBrush, _nameLlbBounds, _format);
                g.DrawString($"{MapSize.Width}x{MapSize.Height}",
                    _font, _textBrush, _sizeLlbBounds, _format);
                g.DrawString($"{MapType}", _font, 
                    _textBrush, _typeLlbBounds, _format);
            }

            public bool IsFocused(Point point)
            {
                return _bounds.Contains(point);
            }

            public void ChangeContainer(Rectangle container)
            {
                _container = container;
                ConstructBounds();
            }

            private void ConstructBounds()
            {
                _bounds = new Rectangle()
                {
                    X = _container.X + 4,
                    Y = _yIndent + _container.Y + 4,
                    Width = _container.Width - 8,
                    Height = _height
                };

                _viewBtn.Height = _bounds.Height - 10;
                _viewBtn.Width = _viewBtn.Height;
                _viewBtn.X = _bounds.Right - 5 - _viewBtn.Height;
                _viewBtn.Y = _bounds.Y + 5;

                _delBtn.Height = _bounds.Height - 10;
                _delBtn.Width = _delBtn.Height;
                _delBtn.X = _viewBtn.Left - 5 - _delBtn.Height;
                _delBtn.Y = _bounds.Y + 5;

                _exportBtn.Height = _bounds.Height - 10;
                _exportBtn.Width = _delBtn.Height;
                _exportBtn.X = _delBtn.Left - 5 - _exportBtn.Height;
                _exportBtn.Y = _bounds.Y + 5;

                _nameLlbBounds = new Rectangle()
                {
                    X = _bounds.X + 3,
                    Y = _bounds.Y,
                    Width = _bounds.Width * 3 / 10,
                    Height = _bounds.Height
                };

                _sizeLlbBounds = new Rectangle()
                {
                    X = _nameLlbBounds.Right + 3,
                    Y = _bounds.Y,
                    Width = _bounds.Width * 2 / 10,
                    Height = _bounds.Height
                };

                _typeLlbBounds = new Rectangle()
                {
                    X = _sizeLlbBounds.Right + 3,
                    Y = _bounds.Y,
                    Width = _bounds.Width * 3 / 10,
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

        private void OnViewButtonClick(MapPanelButtonClickArgs e)
        {
            ViewMapButtonClicked?.Invoke(this, e);
        }

        private void OnDeleteButtonClick(MapPanelButtonClickArgs e)
        {
            DeleteMapButtonClicked?.Invoke(this, e);
        }

        private void OnExportButtonClick(MapPanelButtonClickArgs e)
        {
            ExportMapButtonClicked?.Invoke(this, e);
        }

        private void OnCreateButtonClick(EventArgs e)
        {
            CreateMapButtonClicked?.Invoke(this, e);
        }

        private void OnImportButtonClick(EventArgs e)
        {
            ImportMapButtonClicked?.Invoke(this, e);
        }
    }
}
