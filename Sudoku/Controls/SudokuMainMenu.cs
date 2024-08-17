using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal class SudokuMainMenu : Control
    {
        private Rectangle _nameLblRect;
        private Font _nameFont;
        private SolidBrush _nameBrush;
        private StringFormat _format;
        private SudokuControlModel _continueBtn;
        private bool _isContinueBtnEnable;
        private SudokuControlModel _generateBtn;
        private SudokuControlModel _editorBtn;
        private HatchBrush _bgHatchBrush;
        private LinearGradientBrush _bgGradientBrush;

        public SudokuMainMenu()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;

            _continueBtn = new SudokuControlModel()
            {
                Text = "Продолжить",
                TextTrimming = 20,
                Font = new Font("Times New Roman", 32)
            };

            _generateBtn = new SudokuControlModel()
            {
                Text = "Сгенерировать",
                TextTrimming = 20,
                Font = new Font("Times New Roman", 32)
            };

            _editorBtn = new SudokuControlModel()
            {
                Text = "Редактор",
                TextTrimming = 20,
                Font = new Font("Times New Roman", 32),
                BackColor = Color.FromArgb(150, 225, 230, 240)
            };

            Width = 500;
            Height = 300;

            _bgHatchBrush = new HatchBrush(HatchStyle.LargeGrid,
                Color.FromArgb(150, 200, 200, 230), Color.FromArgb(100, 200, 200, 250));

            _bgGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, Width, Height),
                Color.FromArgb(240, 240, 240), Color.FromArgb(50, 200, 200, 240), 50);

            _nameFont = new Font("Times New Roman", 72);
            _nameBrush = new SolidBrush(Color.Black);
            _format = new StringFormat() 
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
            };

            BackColor = Color.White;
        }

        public bool IsContinueButtonEnable
        {
            get 
            {
                return _isContinueBtnEnable;
            }
            set 
            {
                _isContinueBtnEnable = value;
            }
        }

        public event EventHandler ContinueButtonClicked;

        public event EventHandler GenerateButtonClicked;

        public event EventHandler EditorButtonClicked;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.FillRectangle(_bgHatchBrush, new Rectangle(0, 0, Width, Height));
            g.FillRectangle(_bgGradientBrush, new Rectangle(0, 0, Width, Height));
            g.DrawString("Судоку", _nameFont, _nameBrush, _nameLblRect, _format);
            if (_isContinueBtnEnable)
                _continueBtn.Draw(g);
            _generateBtn.Draw(g);
            _editorBtn.Draw(g);
        }

        protected override void OnResize(EventArgs e)
        {
            _nameLblRect.Width = Width * 8 / 10;
            _nameLblRect.Height = 300;
            _nameLblRect.X = (Width - _nameLblRect.Width) / 2;
            _nameLblRect.Y = 0;

            _continueBtn.Height = 100;
            _continueBtn.Width = Width * 5 / 10;
            _continueBtn.X = (Width - _generateBtn.Width) / 2;
            _continueBtn.Y = Height * 4 / 10;

            _generateBtn.Width = Width * 5 / 10;
            _generateBtn.Height = 100;
            _generateBtn.X = (Width - _generateBtn.Width) / 2;
            _generateBtn.Y = _continueBtn.Bottom + 10;

            _editorBtn.Width = Width * 5 / 10;
            _editorBtn.Height = 100;
            _editorBtn.X = (Width - _generateBtn.Width) / 2;
            _editorBtn.Y = _generateBtn.Bottom + 10;

            if (Width > 0 && Height > 0)
            {
                _bgGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, Width, Height),
                    Color.FromArgb(250, 250, 250), Color.FromArgb(20, 200, 200, 240), 75);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_isContinueBtnEnable)
                _continueBtn.IsPressed = _generateBtn.IsFocused(e.Location);
            _generateBtn.IsPressed = _generateBtn.IsFocused(e.Location);
            _editorBtn.IsPressed = _editorBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _continueBtn.IsSelected = _generateBtn.IsFocused(e.Location);
            _generateBtn.IsSelected = _generateBtn.IsFocused(e.Location);
            _editorBtn.IsSelected = _editorBtn.IsFocused(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_continueBtn.IsPressed)
                OnContinueButtonClicked(EventArgs.Empty);

            if (_generateBtn.IsPressed)
                OnGenerateButtonClick(EventArgs.Empty);

            if (_editorBtn.IsPressed)
                OnEditorButtonClick(EventArgs.Empty);

            _generateBtn.IsPressed = false;
            _editorBtn.IsPressed = false;
            Invalidate();
        }

        private void OnContinueButtonClicked(EventArgs e)
        {
            ContinueButtonClicked?.Invoke(this, e);
        }

        private void OnGenerateButtonClick(EventArgs e)
        {
            GenerateButtonClicked?.Invoke(this, e);
        }

        private void OnEditorButtonClick(EventArgs e)
        {
            EditorButtonClicked?.Invoke(this, e);
        }
    }
}
