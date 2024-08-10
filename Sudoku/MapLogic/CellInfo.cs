using System.Collections.Generic;

namespace Sudoku.MapLogic
{
    /// <summary>
    /// Предоставляет информацию о ячейке карты судоку.
    /// </summary>
    internal sealed class CellInfo
    {
        /// <summary>
        /// Инициализирует новый экземпляр.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="correct"></param>
        /// <param name="groups"></param>
        /// <param name="isAvailable"></param>
        /// <param name="isSelected"></param>
        public CellInfo(int row, int column,
            int correct, IEnumerable<int> groups, bool isAvailable,
            bool isSelected)
        {
            Row = row;
            Column = column;
            Correct = correct;
            Groups = groups;
            IsAvailable = isAvailable;
            IsSelected = isSelected;
        }

        /// <summary>
        /// Возвращает строку, в которой расположена ячейка.
        /// </summary>
        public int Row { get; }

        /// <summary>
        ///  Возвращает столбец, в которой расположена ячейка.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Возвращает значение, являющееся решением ячейки.
        /// </summary>
        public int Correct { get; }

        /// <summary>
        /// Возвращает коллекцию идентификаторов групп, 
        /// в которых состоит ячейка.
        /// </summary>
        public IEnumerable<int> Groups { get; }

        /// <summary>
        /// Возвращает значение, является ли ячейка 
        /// доступной для ввода значений игроком.
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// Возвращает значение, выделена ли ячейка.
        /// </summary>
        public bool IsSelected { get; }

        public static bool operator ==(CellInfo left, CellInfo right)
        {
            if (ReferenceEquals(null, left) && !ReferenceEquals(null, right))
                return false;

            if (!ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return false;

            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            return left.Equals(right);
        }

        public static bool operator !=(CellInfo left, CellInfo right)
        {
            if (ReferenceEquals(null, left) && !ReferenceEquals(null, right))
                return true;

            if (!ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Сравнивает текущий экземпляр ячейки с переданным объектом.
        /// Если переданный объект также является экземпляром CellInfo,
        /// использует для сравнения информацию о ячейках.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is CellInfo comp)
            {
                bool rows = this.Row == comp.Row;
                bool cols = this.Column == comp.Column;
                return rows && cols;
            }

            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
