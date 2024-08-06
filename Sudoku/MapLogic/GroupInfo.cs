using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sudoku.MapLogic.Map;

namespace Sudoku.MapLogic
{
    /// <summary>
    /// Перечисляет типы групп в картах судоку.
    /// </summary>
    internal enum GroupType
    {
        Basic,
        Sum
    }

    /// <summary>
    /// Предоставляет информацию о группе карты судоку.
    /// </summary>
    internal sealed class GroupInfo
    {
        /// <summary>
        /// Инициализирует новый экземпляр.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="sum"></param>
        /// <param name="isSelected"></param>
        public GroupInfo(int id, GroupType type, int sum, bool isSelected)
        {
            ID = id;
            Type = type;
            Sum = sum;
            IsSelected = isSelected;
        }

        /// <summary>
        /// Возвращает идентификатор группы.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Возвращает тип группы.
        /// </summary>
        public GroupType Type { get; }

        /// <summary>
        /// Возвращает сумму значений решений входящих в группу ячеек.
        /// </summary>
        public int Sum { get; }

        /// <summary>
        /// Возвращает значение, выделена ли группа.
        /// </summary>
        public bool IsSelected { get; }
    }
}
