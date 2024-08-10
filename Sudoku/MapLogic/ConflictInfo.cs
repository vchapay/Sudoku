using System.Collections.Generic;

namespace Sudoku.MapLogic
{
    /// <summary>
    /// Предоставляет информацию о конфликте карты судоку.
    /// </summary>
    internal sealed class ConflictInfo
    {
        private readonly IReadOnlyCollection<CellInfo> _cells;
        private readonly int _conflictValue;

        /// <summary>
        /// Инициализирует новый экземпляр.
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="conflictValue"></param>
        public ConflictInfo(IReadOnlyCollection<CellInfo> cells, int conflictValue)
        {
            _cells = cells;
            _conflictValue = conflictValue;
        }

        /// <summary>
        /// Возвращает конфликтное значение, из-за которого
        /// существует конфликт между ячейками.
        /// </summary>
        public int ConflictValue => _conflictValue;

        /// <summary>
        /// Возвращает конфликтующие ячейки.
        /// </summary>
        public IReadOnlyCollection<CellInfo> Cells { get { return _cells; } }
    }
}
