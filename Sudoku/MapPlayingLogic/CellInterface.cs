using System;
using System.Collections.Generic;

namespace Sudoku.MapLogic
{
    [Serializable]
    /// <summary>
    /// Предоставляет оболочку над закрытым типом Cell для использования в игровой сессии
    /// </summary>
    internal sealed class CellInterface
    {
        private readonly int _solution;
        private readonly CellType _state;
        private int _entered;
        private readonly int _row;
        private readonly int _column;
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
            int row, int column, List<int> areas, CellType state)
        {
            _solution = correct;
            _row = row;
            _column = column;
            _groups = areas;
            _state = state;
        }

        /// <summary>
        /// Значение, являющееся решением ячейки
        /// </summary>
        public int Solution => _solution;

        /// <summary>
        /// Значение, введенное игроком
        /// </summary>
        public int Entered
        {
            get { return _entered; }
            set {
                if (value > -1 && value < 21)
                    _entered = value; 
            }
        }

        /// <summary>
        /// Строка, в которой расположена ячейка
        /// </summary>
        public int Row => _row;

        /// <summary>
        /// Столбец, в котором расположена ячейка
        /// </summary>
        public int Column => _column;

        /// <summary>
        /// Группы, в которые входит ячейка
        /// </summary>
        public IReadOnlyCollection<int> Groups => _groups;

        /// <summary>
        /// Заметки карандашом в ячейке
        /// </summary>
        public IReadOnlyCollection<int> Notes => _notes;

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
        public bool IsEntered => _entered != 0;

        /// <summary>
        /// Возвращает значение, найдено ли решение для ячейки
        /// (всегда false для ячеек-подсказок).
        /// </summary>
        public bool IsSolved => IsAvailable && Entered != 0 && !IsThereMistake;

        /// <summary>
        /// Возвращает состояние ячейки (доступна ли для ввода,
        /// гарантирует ли не пустое решение и т.д.)
        /// </summary>
        public CellType State => _state;

        /// <summary>
        /// Возвращает значение, является ли ячейка 
        /// доступной для ввода значений игроком.
        /// </summary>
        public bool IsAvailable => _state != CellType.Tip;

        /// <summary>
        /// Есть ли в решении ячейки ошибка 
        /// (всегда false для ячеек-подсказок).
        /// </summary>
        public bool IsThereMistake => State != CellType.Tip
            && IsEntered && (_solution != _entered);

        /// <summary>
        /// Возвращает значение, заблокирована ли ячейка
        /// (недоступна для ввода и имеет пустое решение).
        /// </summary>
        public bool IsBlocked => State == CellType.Tip && Solution == 0;

        /// <summary>
        /// Записывает число в заметки, если такого еще нет
        /// </summary>
        /// <param name="note"></param>
        public bool WriteNote(int note)
        {
            return _notes.Add(note);
        }

        /// <summary>
        /// Удаляет число из заметок, если такое число есть
        /// </summary>
        /// <param name="note"></param>
        public bool RemoveNote(int note)
        {
            return _notes.Remove(note);
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
                Solution, _groups, IsSelected, State);
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
