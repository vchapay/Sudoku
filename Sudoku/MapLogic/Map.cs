using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace Sudoku.MapLogic
{
    /// <summary>
    /// Перечисляет типы карт судоку
    /// </summary>
    enum MapTypes
    {
        Custom = 0,
        Classical,
        Monolith,
        Sums,
        FreeAreas,
        NumBall,
        GreaterThan,
        Dots,
        Colors,
    }

    /// <summary>
    /// Описывает карту и предоставляет средства для ее заполнения,
    /// а также получения интерфейса для проигрывания
    /// </summary>
    internal class Map
    {
        /// <summary>
        /// Минимальное значение для ширины и длины карты в ячейках
        /// </summary>
        public const int MinLength = 3;

        /// <summary>
        /// Стандартное значение для ширины и длины карты в ячейках
        /// </summary>
        public const int BaseLength = 9;

        private const string _cellNotFoundMessage = "Попытка получить доступ к несуществующей ячейке";
        private const string _areaIsAlreadyExistMessage = "Область с указанным идентификатором уже существует";
        private const string _areaNotFoundMessage = "Область с полученным идентификатором не найдена";
        private const string _creatingInterfaceWithConflictsMessage = "Нельзя получить интерфейс карты с конфликтными ячейками";
        private MapTypes _type;
        private readonly List<Cell> _cells = new List<Cell>();
        private readonly List<Conflict> _conflicts = new List<Conflict>();
        private readonly List<Area> _areas = new List<Area>();

        /// <summary>
        /// Создает карту с указанными размерами
        /// </summary>
        /// <param name="width"></param>
        /// <param name="heght"></param>
        public Map(int width, int heght) 
        {
            if (width < MinLength)
                width = MinLength;

            if (heght < MinLength)
                heght = MinLength;

            InitializeCells(width, heght);
            Width = width;
            Height = heght;
        }

        /// <summary>
        /// Создает карту со стандартными размерами 9х9
        /// </summary>
        public Map() : this(BaseLength, BaseLength) 
        {
        }

        /// <summary>
        /// Ширина карты
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Высота карты
        /// </summary>
        public int Height { get; }

        public MapTypes Type { get { return _type; } }

        /// <summary>
        /// Возвращает число конфликтов между ячейками
        /// </summary>
        public int ConflictsCount => _conflicts.Count;

        private Cell this[int row, int column]
        {
            get { return _cells.Find(
                c => c.Row == row && c.Column == column); }
        }

        /// <summary>
        /// Записывает новое значение в заданную ячейку или вызывает
        /// ArgumentException, если такая ячейка не существует.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        public void Write(int row, int column, int value)
        {
            Cell cell = FindCell(row, column);
            UpdateConflicts(cell, value);
            cell.Correct = value;
        }

        /// <summary>
        /// Изменяет выделение заданной ячейки или вызывает
        /// ArgumentException, если такая ячейка не существует.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="selection"></param>
        public void ChangeSelection(int row, int column, bool selection)
        {
            Cell cell = FindCell(row, column);
            cell.IsSelected = selection;
        }

        /// <summary>
        /// Изменяет доступность для ввода игроком в заданную ячейку или вызывает
        /// ArgumentException, если такая ячейка не существует.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="availability"></param>
        public void ChangeAvailability(int row, int column, bool availability)
        {
            Cell cell = FindCell(row, column);
            cell.IsAvailable = availability;
        }

        /// <summary>
        /// Создает область, добавляя в нее указанную ячейку
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CreateGroup(int row, int column, int id, GroupType type)
        {
            if (_areas.Find(a => a.ID == id) != null)
                throw new ArgumentException(_areaIsAlreadyExistMessage);

            Cell cell = FindCell(row, column);
            Area area = new Area(id, this);
            area.AddCell(cell);
            area.Type = type;
            _areas.Add(area);
        }

        /// <summary>
        /// Удаляет область
        /// </summary>
        /// <param name="id"></param>
        public void RemoveArea(int id)
        {
            Area area = FindArea(id);
            area.Clear();
            _areas.Remove(area);
        }

        /// <summary>
        /// Добавляет ячейку в область, если эта ячейка может быть добавлена
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id"></param>
        public void AddCellToArea(int row, int column, int id)
        {
            Cell cell = FindCell(row, column);
            Area area = FindArea(id);

            area.AddCell(cell);
        }

        /// <summary>
        /// Возвращает все ячейки карты
        /// </summary>
        /// <returns></returns>
        public List<CellInterface> GetCells()
        {
            List<CellInterface> cells = new List<CellInterface>();
            foreach (Cell cell in _cells)
            {
                cells.Add(cell.GetInterface());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает все области карты
        /// </summary>
        /// <returns></returns>
        public List<GroupInterface> GetAreas()
        {
            List<GroupInterface> areas = new List<GroupInterface>();
            foreach (Area area in _areas)
            {
                areas.Add(area.GetInterface());
            }

            return areas;
        }

        /// <summary>
        /// Возвращает ячейки, принадлежащие области с заданным идентификатором
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<CellInterface> GetCellsInArea(int id)
        {
            Area area = FindArea(id);
            List<CellInterface> cells = new List<CellInterface>();

            foreach (Cell cell in area.Cells)
            {
                cells.Add(cell.GetInterface());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает интерфейс для игрового взаимодействия на основе текущего экземпляра.
        /// Вызывает исключение, если на карте обнаружены явные конфликты: повторение значения
        /// по вертикали, горизонтали или внутри области.
        /// </summary>
        /// <returns> Оболочка над типом Map, являющаяся интерфейсом проигрывания карты </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public MapInterface GetInterface()
        {
            if (ConflictsCount > 0)
                throw new InvalidOperationException(_creatingInterfaceWithConflictsMessage);

            return new MapInterface(this);
        }

        /// <summary>
        /// Очищает текущий экземпляр
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _areas.Count; i++)
            {
                RemoveArea(_areas[i].ID);
            }

            for (int i = 0; i < _cells.Count; i++)
            {
                Cell cell = _cells[i];
                cell.Correct = 0;
                cell.IsAvailable = true;
                cell.IsSelected = false;
            }
        }

        /// <summary>
        /// Заполняет текущий экзмепляр задачей судоку по-умолчанию
        /// </summary>
        public void FillWithDefaultValues()
        {
            Clear();

            int[,] test = InitializeTest();
            int[,] openedCells = GetTestOpenedCellsMap();
            FillTestCellsWithValues(test);
            SetTestOpenedCells(openedCells);
            CreateTestAreas();
        }

        private void InitializeCells(int width, int heght)
        {
            for (int row = 0; row < heght; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    Cell cell = new Cell(row, col);
                    _cells.Add(cell);
                }
            }
        }

        private void CreateArea(Cell cell, int id, GroupType type)
        {
            CreateGroup(cell.Row, cell.Column, id, type);
        }

        private void AddCellToGroup(Cell cell, int id)
        {
            AddCellToArea(cell.Row, cell.Column, id);
        }

        private Area FindArea(int id)
        {
            return _areas.Find(c => c.ID == id)
                ?? throw new ArgumentException(_areaNotFoundMessage);
        }

        private Cell FindCell(int row, int column)
        {
            return _cells.Find(c => (c.Row == row && c.Column == column))
                ?? throw new ArgumentException(_cellNotFoundMessage);
        }

        private int[,] InitializeTest()
        {
            return new int[,]
            {
                { 4, 6, 3, 1, 8, 2, 9, 7, 5 },
                { 5, 8, 7, 4, 6, 9, 1, 2, 3 },
                { 9, 2, 1, 3, 5, 7, 8, 6, 4 },
                { 2, 4, 8, 6, 7, 1, 3, 5, 9 },
                { 7, 5, 9, 2, 4, 3, 6, 1, 8 },
                { 1, 3, 6, 5, 9, 8, 7, 4, 2 },
                { 3, 7, 5, 9, 2, 6, 4, 8, 1 },
                { 8, 1, 4, 7, 3, 5, 2, 9, 6 },
                { 6, 9, 2, 8, 1, 4, 5, 3, 7 },
            };
        }

        private int[,] GetTestOpenedCellsMap()
        {
            return new int[,]
            {
                { 1, 0, 1, 0, 0, 1, 0, 0, 0 },
                { 1, 0, 0, 0, 1, 0, 1, 1, 0 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 0, 0, 1, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 1, 0, 0, 1 },
                { 0, 1, 1, 0, 0, 0, 1, 0, 0 },
                { 0, 1, 0, 1, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 1, 0, 1, 1 },
                { 0, 0, 0, 1, 0, 1, 1, 0, 0 },
            };
        }

        private void FillTestCellsWithValues(int[,] values)
        {
            for (int column = 0; column < values.GetLength(0); column++)
            {
                for (int row = 0; row < values.GetLength(1); row++)
                {
                    Write(row, column, values[row, column]);
                }
            }
        }

        private void SetTestOpenedCells(int[,] openedsMap)
        {
            for (int column = 0; column < openedsMap.GetLength(0); column++)
            {
                for (int row = 0; row < openedsMap.GetLength(1); row++)
                {
                    this[row, column].IsAvailable = openedsMap[row, column] != 1;
                }
            }
        }

        private void CreateTestAreas()
        {
            foreach (Cell cell in _cells)
            {
                // из-за целочисленного деления cell.Row / 3 * 3 не лишено смысла
                int id = cell.Column / 3 + cell.Row / 3 * 3;

                if (_areas.Find(a => a.ID == id) == null)
                    CreateArea(cell, id, GroupType.Basic);

                AddCellToGroup(cell, id);
            }

            CreateGroup(row: 2, column: 2, id: 9, GroupType.Sum);
            AddCellToGroup(this[3, 2], 9);
            AddCellToGroup(this[2, 3], 9);

            CreateGroup(row: 5, column: 1, id: 10, GroupType.Sum);
            AddCellToGroup(this[7, 1], 10);

            CreateGroup(row: 6, column: 6, id: 11, GroupType.Sum);
            AddCellToGroup(this[6, 5], 11);
            AddCellToGroup(this[5, 6], 11);

            CreateGroup(row: 1, column: 7, id: 12, GroupType.Sum);
            AddCellToGroup(this[1, 6], 12);
            AddCellToGroup(this[1, 5], 12);
            AddCellToGroup(this[2, 6], 12);
            AddCellToGroup(this[2, 5], 12);
            AddCellToGroup(this[2, 7], 12);
            AddCellToGroup(this[3, 7], 12);
            AddCellToGroup(this[3, 6], 12);

            CreateGroup(row: 3, column: 3, id: 13, GroupType.Sum);
            AddCellToGroup(this[4, 3], 13);
            AddCellToGroup(this[5, 3], 13);
            AddCellToGroup(this[5, 4], 13);
            AddCellToGroup(this[5, 5], 13);
            AddCellToGroup(this[4, 5], 13);
            AddCellToGroup(this[3, 5], 13);
            AddCellToGroup(this[3, 4], 13);
        }

        private void UpdateConflicts(Cell target, int value)
        {
            target.Correct = value;
            int rowT = target.Row;
            int colT = target.Column;

            // удаляю текущую ячейку из конфликтов, так как было задано новое значение
            for (int i = 0; i < _conflicts.Count; i++)
            {
                Conflict conflict = _conflicts[i];
                if (conflict.Cells.Remove(target)
                    && conflict.Cells.Count < 2)
                {
                    _conflicts.RemoveAt(i);
                }
            }

            if (value == 0)
                return;

            List<Cell> conflictList = new List<Cell>
            {
                target
            };

            // проверяю конфликты по вертикали
            for (int y = 0; y < Height; y++)
            {
                Cell cur = this[rowT, y];
                if (target != cur && 
                    cur.Correct == target.Correct)
                {
                    if (!conflictList.Contains(cur))
                        conflictList.Add(cur);
                }
            }

            // проверяю конфликты по горизонтали
            for (int x = 0; x < Width; x++)
            {
                Cell cur = this[x, colT];
                if (target != cur && 
                    cur.Correct == target.Correct)
                {
                    if (!conflictList.Contains(cur))
                        conflictList.Add(cur);
                }
            }

            foreach (Area area in target.Areas)
            {
                foreach (Cell cur in area.Cells)
                {
                    if (cur.Correct == target.Correct)
                    {
                        if (!conflictList.Contains(cur))
                            conflictList.Add(cur);
                    }
                }
            }

            if (conflictList.Count > 1)
            {
                Conflict conflict = _conflicts.Find(c => c.ConflictValue == target.Correct);
                if (conflict != null)
                {
                    conflict.Cells.Add(target);
                }

                else
                {
                    Conflict newConflict = new Conflict(conflictList);
                    _conflicts.Add(newConflict);
                }
            }
        }

        private class Cell
        {
            private int _correct;

            private readonly List<Area> _areas = new List<Area>();

            public Cell(int row, int column) 
            {
                Row = row;
                Column = column;
                IsAvailable = false;
                IsSelected = false;
            }

            public int Correct 
            { 
                get { return _correct; }
                set {
                    if (_correct > -1 && _correct < 10)
                        _correct = value;
                }
            }


            public int Row { get; set; }

            public int Column { get; set; }

            public bool IsAvailable { get; set; }

            public bool IsSelected { get; set; }

            public IReadOnlyCollection<Area> Areas { get { return _areas; } }

            public void AddArea(Area area)
            {
                if (!_areas.Contains(area))
                {
                    _areas.Add(area);
                    area.AddCell(this);
                }
            }

            /*public void AddArea(Area area)
            {
                if (!_areas.Contains(area))
                {
                    if (area.IsPossibleToAdd(this))
                    {
                        _areas.Add(area);
                        area.AddCell(this);
                    }
                }
            }*/

            public void RemoveArea(Area area)
            {
                if (_areas.Contains(area))
                {
                    _areas.Remove(area);
                    area.RemoveCell(this);
                }
            }

            public CellInterface GetInterface()
            {
                List<GroupInterface> areas = new List<GroupInterface>();

                foreach (Area area in _areas)
                {
                    areas.Add(area.GetInterface());
                }

                return new CellInterface(Correct, Row, Column,
                    IsAvailable, areas);
            }
        }

        private class Conflict
        {
            private readonly List<Cell> _cells;

            public Conflict(ICollection<Cell> cells)
            {
                _cells = cells.ToList();
            }

            public List<Cell> Cells { get { return _cells; } }

            public int ConflictValue => _cells.First().Correct;
        }

        public enum GroupType
        {
            Basic,
            Sum
        }

        private class Area
        {
            private readonly int _id;
            private readonly List<Cell> _cells = new List<Cell>();
            private Map _map;

            public Area(int id, Map map)
            {
                _map = map;
                _id = id;
                Type = GroupType.Basic;
            }

            public int ID { get { return _id; } }

            public IReadOnlyCollection<Cell> Cells { get { return _cells; } }

            public int Sum => _cells.Sum(c => c.Correct);

            public GroupType Type { get; set; }

            public bool AddCell(Cell cell)
            {
                if (!_cells.Contains(cell))
                {
                    _cells.Add(cell);
                    cell.AddArea(this);
                    _map.UpdateConflicts(cell, cell.Correct);
                    return true;
                }

                return false;
            }

            /*public bool AddCell(Cell cell)
            {
                if (!_cells.Contains(cell))
                {
                    if (_cells.Count == 0 || IsPossibleToAdd(cell))
                    {
                        _cells.Add(cell);
                        cell.AddArea(this);
                        _map.UpdateConflicts(cell, cell.Correct);
                        return true;
                    }
                }

                return false;
            }*/

            public void RemoveCell(Cell cell)
            {
                if (_cells.Contains(cell))
                {
                    _cells.Remove(cell);
                    cell.RemoveArea(this);
                }
            }

            /*public bool IsPossibleToAdd(Cell target)
            {
                foreach (Cell cell in _cells)
                {
                    if (Math.Abs(cell.Column - target.Column) < 2
                        && cell.Row == target.Row)
                        return true;

                    if (Math.Abs(cell.Row - target.Row) < 2
                        && cell.Column == target.Column)
                        return true;
                }

                return false;
            }*/

            public void Clear()
            {
                for (int i = 0; i < _cells.Count; i++)
                {
                    Cell cell = _cells[i];
                    RemoveCell(cell);
                }
            }

            public GroupInterface GetInterface()
            {
                return new GroupInterface(ID, Type, Sum);
            }
        }

        private enum CellRulesType
        {
            GratherThan,
            Dots,
        }

        private class CellRule
        {

        }
    }
}
