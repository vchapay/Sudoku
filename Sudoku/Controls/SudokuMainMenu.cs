using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku.Controls
{
    internal class SudokuMainMenu : Control
    {
        public SudokuMainMenu()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            DoubleBuffered = true;
        }
    }
}
