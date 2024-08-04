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
        private readonly List<GroupInterface> _areas;

        public MapInterface(Map map) 
        {
            _cells = map.GetCells();
            _areas = map.GetGroups();
            _width = map.Width;
            _height = map.Height;
            _type = map.Type;
        }

        public IReadOnlyCollection<CellInterface> Cells { get { return _cells; } }

        public IReadOnlyCollection<GroupInterface> Areas { get { return _areas; } }

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
        /// <param name="areaId"></param>
        /// <returns></returns>
        public List<CellInterface> GetCellsByArea(int areaId)
        {
            List<CellInterface> cells = new List<CellInterface>();

            foreach (var cell in Cells)
            {
                foreach (var area in cell.Groups)
                {
                    if (area.ID == areaId)
                        cells.Add(cell);
                }
            }

            return cells;
        }

        public void ChangeCellSelection(int row, int column)
        {
            CellInterface cell = this[row, column];
            cell.IsSelected = !cell.IsSelected;
            foreach (GroupInterface area in cell.Groups)
            {
                if (cell.IsSelected)
                {
                    _areas.Find(a => a.ID == area.ID).IsSelected = true;
                    area.IsSelected = true;
                }

                else
                {
                    if (GetCellsByArea(area.ID).Where(c => c.IsSelected).Count() == 0)
                    {
                        _areas.Find(a => a.ID == area.ID).IsSelected = false;
                        area.IsSelected = false;
                    }
                }
            }
        }

        public void ClearSelection()
        {
            foreach (var cell in Cells)
            {
                cell.IsSelected = false;

                foreach (var area in cell.Groups)
                {
                    _areas.Find(a => a.ID == area.ID).IsSelected = false;
                    area.IsSelected = false;
                }
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
