using Sudoku.MapLogic;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.MapPlayingLogic
{
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

        public IReadOnlyCollection<CellInterface> Cells { get { return _cells; } }

        public IReadOnlyCollection<GroupInterface> Groups { get { return _groups; } }

        public CellInterface this[int row, int column]
        {
            get { return _cells.Find(
                cell => (cell.Row == row && cell.Column == column)); }
        }

        public int Width { get { return _width; } }

        public int Height { get { return _height; } }

        public MapTypes Type { get { return _type; } }

        /// <summary>
        /// Возвращает список ячеек, принадлежащих области с заданным индентификатором
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
    }
}
