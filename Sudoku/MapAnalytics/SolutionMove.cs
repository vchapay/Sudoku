using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.MapAnalytics
{
    /// <summary>
    /// Перечисляет типы решений ячеек класса SudokuSolver.
    /// </summary>
    internal enum SolutionType
    {
        /// <summary>
        /// Для ячейки найдено единственное решение.
        /// </summary>
        Succeful,

        /// <summary>
        /// Для ячейки найдено несколько решений.
        /// </summary>
        Fork,

        /// <summary>
        /// Для ячейки не существует решений.
        /// </summary>
        DeadEnd
    }

    /// <summary>
    /// Описывает информацию о решении ячейки.
    /// </summary>
    internal class SolutionMove
    {
        private int _index;
        private int _solution;
        private int _row;
        private int _column;

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (value > 0)
                {
                    _index = value;
                }
            }
        }

        public int Solution
        {
            get
            {
                return _solution;
            }

            set
            {
                if (value > -1 && value < 21) 
                {
                    _solution = value;
                }
            }
        }

        public int Row
        {
            get
            {
                return _row;
            }
            set
            {
                if (value > 0)
                {
                    _row = value;
                }
            }
        }

        public int Column
        {
            get
            {
                return _column;
            }
            set
            {
                if (value > 0)
                {
                    _column = value;
                }
            }
        }

        public SolutionType Type { get; set; }


    }
}
