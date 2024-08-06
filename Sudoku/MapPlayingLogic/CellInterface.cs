using System.Collections.Generic;

namespace Sudoku.MapLogic
{
    /// <summary>
    /// Предоставляет оболочку над закрытым типом Cell для использования в игровой сессии
    /// </summary>
    internal sealed class CellInterface
    {
        private readonly int _correct;
        private int _entered;
        private readonly int _row;
        private readonly int _column;
        private readonly bool _available;
        private readonly List<int> _groups;
        private readonly HashSet<int> _notes = new HashSet<int>();
        private bool _isSelected;

        /// <summary>
        /// Создает интерфейс на основе данных целевой ячейки
        /// </summary>
        /// <param name="correct"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="available"></param>
        /// <param name="areas"></param>
        public CellInterface(int correct, 
            int row, int column, bool available,
            List<int> areas)
        {
            _correct = correct;
            _row = row;
            _column = column;
            _available = available;
            _groups = areas;
        }

        /// <summary>
        /// Значение, являющееся решением ячейки
        /// </summary>
        public int Correct { get { return _correct; } }

        /// <summary>
        /// Значение, введенное игроком
        /// </summary>
        public int Entered
        {
            get { return _entered; }
            set {
                if (value > -1 && value < 10)
                    _entered = value; 
            }
        }

        /// <summary>
        /// Строка, в которой расположена ячейка
        /// </summary>
        public int Row { get { return _row; } }

        /// <summary>
        /// Столбец, в котором расположена ячейка
        /// </summary>
        public int Column { get { return _column; } }

        /// <summary>
        /// Открыта ли ячейка для ввода значения
        /// </summary>
        public bool IsAvailable { get { return _available; } }

        /// <summary>
        /// Группы, в которые входит ячейка
        /// </summary>
        public IReadOnlyCollection<int> Groups { get { return _groups; } }

        /// <summary>
        /// Заметки карандашом в ячейке
        /// </summary>
        public IReadOnlyCollection<int> Notes { get { return _notes; } }

        /// <summary>
        /// Выделена ли ячейка
        /// </summary>
        public bool IsSelected 
        { 
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                }
            }
        }

        /// <summary>
        /// Было ли введено игроком значение в ячейку
        /// </summary>
        public bool IsEntered { get { return _entered != 0; } }

        /// <summary>
        /// Решена ли ячейка верно
        /// </summary>
        public bool IsThereMistake => _available && IsEntered && (_correct != _entered);

        /// <summary>
        /// Записывает число в заметки, если такого еще нет
        /// </summary>
        /// <param name="note"></param>
        public void WriteNote(int note)
        {
            if (note > 0 && note < 10)
                _notes.Add(note);
        }

        /// <summary>
        /// Удаляет число из заметок, если такое число есть
        /// </summary>
        /// <param name="note"></param>
        public void RemoveNote(int note)
        {
            if (note > 0 && note < 10)
                _notes.Remove(note);
        }

        /// <summary>
        /// Очищает заметки карандашом
        /// </summary>
        public void ClearNotes()
        {
            _notes.Clear();
        }

        public CellInfo GetInfo()
        {
            return new CellInfo(Row, Column, 
                Correct, _groups, IsAvailable, IsSelected);
        }

        public static bool operator ==(CellInterface left, CellInterface right)
        {
            if (ReferenceEquals(null, left) && !ReferenceEquals(null, right))
                return false;

            if (!ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return false;

            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            return left.Equals(right);
        }

        public static bool operator !=(CellInterface left, CellInterface right)
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
        /// Если переданный объект также является экземпляром CellInterface, использует для сравнения 
        /// информацию о ячейках
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is CellInterface comp)
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
