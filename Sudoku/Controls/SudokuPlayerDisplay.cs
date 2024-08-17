using Sudoku.MapGraphics;
using Sudoku.MapLogic;
using Sudoku.MapPlayingLogic;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal sealed class SudokuPlayerDisplay : Control
    {
        private MapInterface _map;
        private readonly MapDrawer _drawer;
        private readonly ContentCounterPanel[] _panels;
        private int _panelsSplitter = 3;
        private Rectangle _mapRect;
        private Rectangle _bottomRect;
        private Rectangle _countersRect;
        private Rectangle _leftBottomPanelRect;
        private Rectangle _rightBottomPanelRect;
        private SudokuControlModel _switchModeBtn;
        private SudokuControlModel _clearBtn;
        private float _ratio = 0.85f;
        private Pen _splitterPen;
        private WritingMode _writingMode;

        public SudokuPlayerDisplay() 
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;

            Map map = new Map();
            map.FillWithDefaultValues();
            _drawer = new MapDrawer();
            _map = map.GetInterface();

            _panels = new ContentCounterPanel[20];
            for (int i = 0; i < _panels.Length; i++)
            {
                _panels[i] = new ContentCounterPanel();
                _panels[i].ContentFont = new Font("Times New Roman", 30);
                _panels[i].CountFont = new Font("Times New Roman", 14);
                _panels[i].Content = i + 1;
                _panels[i].Font = new Font("Times New Roman", 30);
            }

            _switchModeBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.PencilIcon,
                BackColor = Color.FromArgb(100, Color.WhiteSmoke)
            };

            _clearBtn = new SudokuControlModel()
            {
                Image = Properties.Resources.ClearingIcon,
                BackColor = Color.FromArgb(100, Color.WhiteSmoke)
            };

            Width = 350;
            Height = 350;
            Font = _drawer.Font;
            _splitterPen = new Pen(Color.Gray)
            {
                Width = _panelsSplitter,
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
            };

            _writingMode = WritingMode.Solution;
        }

        public MapInterface Map
        {
            get
            {
                return _map;
            } 
            set
            {
                _map = value;
                UpdateCounters();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            _drawer.Draw(e.Graphics, _mapRect, _map);
            g.DrawLine(_splitterPen, 0, _bottomRect.Y,
                Width, _bottomRect.Y);

            foreach (var p in _panels)
            {
                p.Draw(g);
            }

            _switchModeBtn.Draw(g);
            _clearBtn.Draw(g);
        }

        protected override void OnResize(EventArgs e)
        {
            _mapRect.Width = Width;
            _mapRect.Height = (int)(Height * _ratio);
            _mapRect.X = 0;
            _mapRect.Y = 0;

            _bottomRect.Width = Width;
            _bottomRect.Height = Height - _mapRect.Height;
            _bottomRect.X = 0;
            _bottomRect.Y = _mapRect.Bottom;

            _countersRect.Width = _bottomRect.Width * 75 / 100;
            _countersRect.Height = _bottomRect.Height;
            _countersRect.X = (Width - _countersRect.Width) / 2;
            _countersRect.Y = _bottomRect.Y;

            _leftBottomPanelRect.Width = _countersRect.X;
            _leftBottomPanelRect.Height = _bottomRect.Height;
            _leftBottomPanelRect.X = _bottomRect.X;
            _leftBottomPanelRect.Y = _bottomRect.Y;

            _rightBottomPanelRect.Width = _countersRect.X;
            _rightBottomPanelRect.Height = _bottomRect.Height;
            _rightBottomPanelRect.X = _countersRect.Right;
            _rightBottomPanelRect.Y = _bottomRect.Y;

            _switchModeBtn.Height = _leftBottomPanelRect.Height * 9 / 10;
            _switchModeBtn.Width = _switchModeBtn.Height;
            _switchModeBtn.X = _leftBottomPanelRect.X
                + (_leftBottomPanelRect.Width - _switchModeBtn.Width) / 2;
            _switchModeBtn.Y = _leftBottomPanelRect.Y
                + (_leftBottomPanelRect.Height - _switchModeBtn.Height) / 2;

            _clearBtn.Height = _rightBottomPanelRect.Height * 9 / 10;
            _clearBtn.Width = _clearBtn.Height;
            _clearBtn.X = _rightBottomPanelRect.X
                + (_rightBottomPanelRect.Width - _clearBtn.Width) / 2;
            _clearBtn.Y = _rightBottomPanelRect.Y
                + (_rightBottomPanelRect.Height - _clearBtn.Height) / 2;

            int panelWidth = (_countersRect.Width - 22 * _panelsSplitter) / 20;
            int panelHeight = _countersRect.Height - 10;
            int nextX = _panelsSplitter + _countersRect.X;
            for (int i = 0; i < _panels.Length; i++)
            {
                var panel = _panels[i];
                panel.Width = panelWidth;
                panel.Height = panelHeight;
                panel.X = nextX;
                panel.Y = _countersRect.Y + 5;
                nextX += panelWidth + _panelsSplitter;
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            _drawer.Font = Font;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            foreach (var panel in _panels)
            {
                panel.IsSelected = panel.IsFocused(e.Location);
            }

            _clearBtn.IsSelected = _clearBtn.IsFocused(e.Location);
            _switchModeBtn.IsSelected = _switchModeBtn.IsFocused(e.Location);

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            foreach (var panel in _panels)
            {
                panel.IsPressed = panel.IsFocused(e.Location);
            }

            _clearBtn.IsPressed = _clearBtn.IsFocused(e.Location);
            if (_switchModeBtn.IsFocused(e.Location))
                _switchModeBtn.IsPressed = !_switchModeBtn.IsPressed;

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_mapRect.Contains(e.X, e.Y))
            {
                _map.ClearSelection();
                Size mapSize = new Size(_map.ColumnsCount, _map.RowsCount);
                Point pos = _drawer.GetCell(e.X, e.Y, mapSize, _mapRect.Size);
                if (pos.X > -1 && pos.X < _map.ColumnsCount
                    && pos.Y > -1 && pos.Y < _map.RowsCount)
                {
                    _map.ChangeCellSelection(pos.Y, pos.X);
                }
            }

            foreach (var panel in _panels)
            {
                if (panel.IsPressed)
                {
                    panel.IsPressed = false;
                    if (panel.Count > 0) 
                        Write(panel.Content);
                    break;
                }
            }

            if (_clearBtn.IsPressed)
            {
                Write(0);
            }

            _clearBtn.IsPressed = false;
            UpdateCounters();
            Invalidate();
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Control && !e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                    case Keys.Back:
                        Write(0);
                        break;
                    case Keys.A:
                        Write(10);
                        break;
                    case Keys.B:
                        Write(11);
                        break;
                    case Keys.C:
                        Write(12);
                        break;
                    case Keys.D:
                        Write(13);
                        break;
                    case Keys.E:
                        Write(14);
                        break;
                    case Keys.F:
                        Write(15);
                        break;
                    case Keys.G:
                        Write(16);
                        break;
                    case Keys.H:
                        Write(17);
                        break;
                    case Keys.I:
                        Write(18);
                        break;
                    case Keys.J:
                        Write(19);
                        break;
                    case Keys.K:
                        Write(20);
                        break;
                }

                var selectedCells = _map.GetSelectedCells();
                if (selectedCells.Count == 1)
                {
                    CellInterface cell = selectedCells.First();
                    switch (e.KeyCode)
                    {
                        case Keys.Up:
                            int newPos = cell.Row - 1;
                            if (newPos > -1)
                            {
                                _map.ClearSelection();
                                _map.ChangeCellSelection(newPos, cell.Column);
                            }
                            break;
                        case Keys.Down:
                            newPos = cell.Row + 1;
                            if (newPos < _map.RowsCount)
                            {
                                _map.ClearSelection();
                                _map.ChangeCellSelection(newPos, cell.Column);
                            }
                            break;
                        case Keys.Left:
                            newPos = cell.Column - 1;
                            if (newPos > -1)
                            {
                                _map.ClearSelection();
                                _map.ChangeCellSelection(cell.Row, newPos);
                            }
                            break;
                        case Keys.Right:
                            newPos = cell.Column + 1;
                            if (newPos < _map.ColumnsCount)
                            {
                                _map.ClearSelection();
                                _map.ChangeCellSelection(cell.Row, newPos);
                            }
                            break;
                    }
                }
            }

            UpdateCounters();
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                Write(int.Parse(e.KeyChar.ToString()));
            }

            UpdateCounters();
            Invalidate();
        }

        private void Write(int value) 
        {
            if (!_switchModeBtn.IsPressed)
                _map.Write(value);
            else
            {
                if (!_map.WriteNote(value))
                {
                    _map.RemoveNote(value);
                }
            }
        }

        private void UpdateCounters()
        {
            foreach (var p in _panels)
            {
                p.Count = _map.CountUnsolveContent(p.Content);
            }
        }

        private class ContentCounterPanel : SudokuControlModel
        {
            private SudokuControlModel _countPanel;
            private SudokuControlModel _contentPanel;
            private SolidBrush _unavailableBrush;
            private SolidBrush _textBrush;
            private StringFormat _format;

            public ContentCounterPanel()
            {
                _countPanel = new SudokuControlModel();
                _contentPanel = new SudokuControlModel();
                _countPanel.Font = new Font("Times New Roman", 12);
                _contentPanel.Font = new Font("Times New Roman", 30);
                Font = new Font("Times New Roman", 36);
                _textBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 100));
                _unavailableBrush = new SolidBrush(Color.FromArgb(70, 200, 200, 200));
                _format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                _countPanel.Text = "0";
            }

            public int Content
            {
                get
                {
                    return int.Parse(_contentPanel.Text);
                }
                set
                {
                    _contentPanel.Text = $"{value}";
                }
            }

            public int Count
            {
                get
                {
                    return int.Parse(_countPanel.Text);
                }
                set
                {
                    _countPanel.Text = $"{value}";
                }
            }

            public Font ContentFont
            {
                get
                {
                    return _contentPanel.Font;
                }
                set
                {
                    _contentPanel.Font = value;
                }
            }

            public Font CountFont
            {
                get
                {
                    return _countPanel.Font;
                }
                set
                {
                    _countPanel.Font = value;
                }
            }

            public override void Draw(Graphics g)
            {
                if (Count > 0)
                {
                    ConstructBounds();
                    int diam = _contentPanel.Width * 8 / 10;
                    int ellX = _contentPanel.X + (_contentPanel.Width - diam) / 2;
                    int ellY = _contentPanel.Y + (_contentPanel.Height - diam) / 2;
                    g.FillEllipse(Brushes.LightSkyBlue, ellX, ellY, diam, diam);
                    _contentPanel.Draw(g);
                    _countPanel.Draw(g);
                    base.Draw(g);
                }

                else
                {
                    Rectangle bounds = new Rectangle(X, Y, Width, Height);
                    g.FillRectangle(_unavailableBrush, bounds);
                    g.DrawString($"{Content}", Font, _textBrush, bounds, _format);
                }
            }

            private void ConstructBounds()
            {
                _contentPanel.X = X;
                _contentPanel.Y = Y;
                _contentPanel.Width = Width;
                _contentPanel.Height = Height * 7 / 10;

                _countPanel.Y = _contentPanel.Bottom;
                _countPanel.X = X;
                _countPanel.Width = Width;
                _countPanel.Height = Height - _contentPanel.Height;
            }
        }

        private enum WritingMode
        {
            Solution,
            Note
        }
    }
}
