using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.MapPlayingLogic
{
    [Serializable]
    /// <summary>
    /// Предоставляет оболочку над типом Map для реализации игрового процесса
    /// </summary>
    internal class MapInterface
    {
        private readonly List<CellInterface> _cells = new List<CellInterface>();
        private readonly int _width;
        private readonly int _height;
        private readonly MapTypes _type;
        private readonly List<GroupInterface> _groups;

        public MapInterface(Map map) 
        {
            _cells = map.GetCellsInterfaces();
            _groups = map.GetGroupsInterfaces();
            _width = map.ColumnsCount;
            _height = map.RowsCount;
            _type = map.Type;
        }

        /// <summary>
        /// Возвращает ячейки текущего экземпляра.
        /// </summary>
        public IReadOnlyCollection<CellInterface> Cells { get { return _cells; } }

        /// <summary>
        /// Возвращает группы текущего экземпляра.
        /// </summary>
        public IReadOnlyCollection<GroupInterface> Groups { get { return _groups; } }

        /// <summary>
        /// Возвращает ячейку в заданных строке и столбце.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public CellInterface this[int row, int column]
        {
            get { return _cells.Find(
                cell => (cell.Row == row && cell.Column == column)); }
        }

        /// <summary>
        /// Число столбцов.
        /// </summary>
        public int ColumnsCount { get { return _width; } }

        /// <summary>
        /// Число строк.
        /// </summary>
        public int RowsCount { get { return _height; } }

        /// <summary>
        /// Тип карты.
        /// </summary>
        public MapTypes Type { get { return _type; } }

        /// <summary>
        /// Возвращает список ячеек, принадлежащих группе с заданным индентификатором
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public List<CellInterface> GetCellsByGroup(int targetId)
        {
            List<CellInterface> cells = new List<CellInterface>();

            foreach (var cell in Cells)
            {
                foreach (var gID in cell.Groups)
                {
                    if (gID == targetId)
                        cells.Add(cell);
                }
            }

            return cells;
        }

        public List<CellInterface> GetSelectedCells()
        {
            var cells = new List<CellInterface>();

            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                    cells.Add(cell);
            }

            return cells;
        }

        /// <summary>
        /// Изменяет выделение ячейки.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void ChangeCellSelection(int row, int column)
        {
            CellInterface cell = this[row, column];
            cell.IsSelected = !cell.IsSelected;
            foreach (int gID in cell.Groups)
            {
                if (cell.IsSelected)
                {
                    _groups.Find(g => g.ID == gID).IsSelected = true;
                }

                else
                {
                    if (GetCellsByGroup(gID).Where(c => c.IsSelected).Count() == 0)
                    {
                        _groups.Find(a => a.ID == gID).IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// Снимает выделение с ячеек.
        /// </summary>
        public void ClearSelection()
        {
            foreach (var cell in Cells)
            {
                cell.IsSelected = false;
            }

            foreach (var group in Groups)
            {
                group.IsSelected = false;
            }
        }

        /// <summary>
        /// Записывает решение в выделенную ячейку.
        /// </summary>
        /// <param name="num"></param>
        public void Write(int num)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                {
                    cell.Entered = num;
                }
            }
        }

        /// <summary>
        /// Записывает заметку карандашом в выделенную ячейку.
        /// </summary>
        /// <param name="num"></param>
        public bool WriteNote(int num)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                {
                    if (num == 0)
                    {
                        cell.ClearNotes();
                        return true;
                    }

                    else
                    {
                        return cell.WriteNote(num);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Убирает заметку карандашом из выделенной ячейки.
        /// </summary>
        /// <param name="num"></param>
        public bool RemoveNote(int num)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                {
                    return cell.RemoveNote(num);
                }
            }

            return false;
        }

        /// <summary>
        /// Считает число заданных решений ячеек в текущем экземпляре
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int CountUnsolveContent(int content)
        {
            int count = 0;

            foreach (var cell in _cells)
            {
                if (cell.Correct == content && cell.IsAvailable
                    && cell.Entered != cell.Correct)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
