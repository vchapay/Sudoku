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

        public CellInterface GetCell(int row, int column)
        {
            return _cells.Find(c => c.Row == row && c.Column == column);
        }

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

        /// <summary>
        /// Возвращает все выделенные ячейки.
        /// </summary>
        /// <returns></returns>
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
            SetCellSelection(row, column, !cell.IsSelected);
        }

        /// <summary>
        /// Изменяет выделение ячейки на заданное состояние.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="isSelected"></param>
        public void SetCellSelection(int row, int column, bool isSelected)
        {
            CellInterface cell = this[row, column];
            if (cell == null)
                return;

            cell.IsSelected = isSelected;
            foreach (int gID in cell.Groups)
            {
                if (isSelected)
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
        /// Возвращает все числа, встречающиеся в выделенных ячейках
        /// (заметки не учитываются).
        /// </summary>
        /// <returns></returns>
        public List<int> GetSelectedNums()
        {
            var selected = new HashSet<int>();

            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                {
                    if (!cell.IsAvailable)
                    {
                        selected.Add(cell.Solution);
                    }

                    else
                    {
                        selected.Add(cell.Entered);
                    }
                }
            }

            selected.Remove(0);
            return selected.ToList();
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
        /// Возвращает все ячейки с указанным решением.
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public List<CellInterface> GetCellsBySolution(int solution)
        {
            var res = new List<CellInterface>();
            foreach (var cell in _cells)
            {
                if (cell.Solution == solution)
                {
                    res.Add(cell);
                }
            }

            return res;
        }

        /// <summary>
        /// Возвращает все открытые и решенные ячейки с указанным решением.
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public List<CellInterface> GetCellsByOpenedSolution(int solution)
        {
            var res = new List<CellInterface>();
            foreach (var cell in _cells)
            {
                if (cell.IsSolved && cell.Solution == solution)
                {
                    res.Add(cell);
                }

                else
                {
                    if (!cell.IsAvailable && cell.Solution == solution)
                    {
                        res.Add(cell);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Находит все ассоциированные (связанные каким-либо правилом карты)
        /// ячейки для заданной группы ячеек.
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<CellInterface> GetAssociatedCells(params CellInterface[] cells)
        {
            if (cells == null)
                throw new ArgumentNullException();

            if (cells.Length == 0)
                return new List<CellInterface>();

            HashSet<CellInterface> res = new HashSet<CellInterface>();
            res.UnionWith(_cells);
            foreach (var cell in cells)
            {
                var sameCol = _cells.Where(c => c.Column == cell.Column);
                var sameRow = _cells.Where(c => c.Row == cell.Row);
                var sameGroup = _cells.Where(c =>
                    c.Groups.Intersect(cell.Groups).Any());

                var allSame = sameGroup.Union(sameCol.Union(sameRow));
                res.IntersectWith(allSame);
            }

            return res.ToList();
        }

        /// <summary>
        /// Записывает заметку карандашом в выделенные ячейки.
        /// </summary>
        /// <param name="num"></param>
        public bool WriteNote(int num)
        {
            bool res = false;
            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                {
                    if (num == 0)
                    {
                        cell.ClearNotes();
                        res = true;
                    }

                    else
                    {
                        bool forCell = cell.WriteNote(num);
                        if (!res)
                            res = forCell;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Убирает заметку карандашом из выделенной ячейки.
        /// </summary>
        /// <param name="num"></param>
        public bool RemoveNote(int num)
        {
            bool res = false;
            foreach (var cell in Cells)
            {
                if (cell.IsSelected)
                {
                    bool forCell = cell.RemoveNote(num);
                    if (!res)
                        res = forCell;
                }
            }

            return res;
        }

        /// <summary>
        /// Считает число заданных не найденных решений ячеек в текущем экземпляре.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public int CountUnsolveContent(int content)
        {
            int count = 0;

            foreach (var cell in _cells)
            {
                if (cell.Solution == content && cell.IsAvailable
                    && cell.Entered != cell.Solution)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
