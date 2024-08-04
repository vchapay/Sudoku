using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
        private const string _incorrectSizesForCopyOperationMessage = "Копирование невозможно, если карты имеют разные размеры";
        private MapTypes _type;
        private readonly List<Cell> _cells = new List<Cell>();
        private readonly List<Conflict> _conflicts = new List<Conflict>();
        private readonly List<Group> _groups = new List<Group>();

        private readonly List<Group> _selectedGroups = new List<Group>();

        private readonly List<MapSave> _saves = new List<MapSave>();
        private int _savesCapacity;
        private int _saveInd;

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

            Width = width;
            Height = heght;
            InitializeCells();

            _savesCapacity = 15;
            Save();
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

        public string Name { get; set; }

        /// <summary>
        /// Возвращает число конфликтов между ячейками
        /// </summary>
        public int ConflictsCount => _conflicts.Count;

        public int SavesCapacity
        {
            get { return _savesCapacity; }
            set 
            {
                if (value > 2 && value < 30)
                {
                    if (_savesCapacity > value)
                    {
                        _saves.RemoveRange(0, _savesCapacity - value);
                    }

                    _savesCapacity = value;
                }
                    
            }
        }

        /// <summary>
        /// Записывает новое значение во все выделенные ячейки
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        public void Write(int value)
        {
            if (_saves.Count == 0)
                Save();

            var selectedCells = GetSelectedCells();
            foreach (CellInterface cell in selectedCells)
            {
                if (cell.Correct == value)
                    continue;

                Cell c = this[cell.Row, cell.Column];
                int oldValue = c.Correct;
                c.Correct = value;
                UpdateConflicts(value);
                UpdateConflicts(oldValue);
            }

            Save();
        }

        /// <summary>
        /// Записывает в заданную ячейку значение или вызывает исключение,
        /// если ячейки не существует
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public void Write(int row, int column, int value)
        {
            Cell cell = FindCell(row, column);
            if (cell.Correct == value)
                return;

            if (_saves.Count == 0)
                Save();

            int oldValue = cell.Correct;
            cell.Correct = value;
            UpdateConflicts(value);
            UpdateConflicts(oldValue);

            Save();
        }

        /// <summary>
        /// Изменяет доступность для ввода игроком в заданную ячейку или вызывает
        /// ArgumentException, если такая ячейка не существует.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="availability"></param>
        public void ChangeAvailability(int row, int column)
        {
            if (_saves.Count == 0)
                Save();

            Cell cell = FindCell(row, column);
            cell.IsAvailable = !cell.IsAvailable;

            if (!cell.IsAvailable)
            {
                for (int i = 0; i < cell.Groups.Count; i++)
                {
                    Group g = cell.Groups.ElementAt(i);
                    g.RemoveCell(cell);
                }
            }

            Save();
        }

        public void ChangeSelectedAvailability()
        {
            if (_saves.Count == 0)
                Save();

            foreach (Cell cell in _cells)
            {
                if (cell.IsSelected)
                    cell.IsAvailable = !cell.IsAvailable;

                if (!cell.IsAvailable)
                {
                    for (int i = 0; i < cell.Groups.Count; i++)
                    {
                        Group g = cell.Groups.ElementAt(i);
                        g.RemoveCell(cell);
                    }
                }
            }

            Save();
        }

        /// <summary>
        /// Создает область, добавляя в нее указанную ячейку
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id"></param>
        /// <exception cref="ArgumentException"></exception>
        public void CreateGroup(int row, int column, int id, GroupType type)
        {
            if (_saves.Count == 0)
                Save();

            if (_groups.Find(a => a.ID == id) != null)
                throw new ArgumentException(_areaIsAlreadyExistMessage);

            Cell cell = FindCell(row, column);
            Group group = new Group(id, this);
            group.AddCell(cell);
            group.Type = type;
            _groups.Add(group);

            Save();
        }

        /// <summary>
        /// Удаляет группу
        /// </summary>
        /// <param name="id"></param>
        public void RemoveGroup(int id)
        {
            if (_saves.Count == 0)
                Save();

            Group group = FindGroup(id, createNew: false);
            group.Clear();
            _groups.Remove(group);
            Save();
        }

        /// <summary>
        /// Добавляет все выделенные ячейки в группу, которая была выделена последней
        /// </summary>
        public void AddSelectedToGroup()
        {
            if (_saves.Count == 0)
                Save();

            Group group = _selectedGroups.LastOrDefault();
            if (group != null)
            {
                foreach (Cell cell in _cells)
                {
                    if (cell.IsSelected)
                        group.AddCell(cell);
                }
            }

            Save();
        }

        /// <summary>
        /// Удаляет все выделенные ячейки из группы, которая была выделена последней
        /// </summary>
        public void RemoveSelectedFromGroup()
        {
            if (_saves.Count == 0)
                Save();

            Group group = _selectedGroups.LastOrDefault();
            if (group != null)
            {
                var cells = GetSelectedCells();
                foreach (var cell in cells)
                {
                    RemoveCellFromGroup(cell.Row, cell.Column, group.ID);
                }
            }

            Save();
        }

        /// <summary>
        /// Добавляет ячейку в группу, если эта ячейка может быть добавлена.
        /// Создает новую группу, если полученный идентификатор не занят
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id"></param>
        public void AddCellToGroup(int row, int column, int id)
        {
            if (_saves.Count == 0)
                Save();

            Cell cell = FindCell(row, column);
            Group group = FindGroup(id, createNew: true);
            group.AddCell(cell);
            Save();
        }

        /// <summary>
        /// Удаляет ячейку из группы, если группа и ячейка существуют
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="id"></param>
        public void RemoveCellFromGroup(int row, int column, int id)
        {
            if (_saves.Count == 0)
                Save();

            Cell cell = FindCell(row, column);
            Group group = FindGroup(id, createNew: false);
            group.RemoveCell(cell);

            if (group.Cells.Count == 0)
            {
                _groups.Remove(group);
                _selectedGroups.Remove(group);
            }

            Save();
        }

        /// <summary>
        /// Изменяет тип группы на полученный, если группа существует
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public void ChangeGroupType(int id, GroupType type)
        {
            if (_saves.Count == 0)
                Save();

            Group group = FindGroup(id, createNew: false);
            group.Type = type;
            Save();
        }

        /// <summary>
        /// Возвращает логическое значение, является ли ячейка конфликтной
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsCellConflict(int row, int column)
        {
            foreach (Conflict conflict in _conflicts)
            {
                Cell cell = FindCell(row, column);
                if (conflict.Cells.Contains(cell))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Изменяет выделение ячейки или вызывает
        /// ArgumentException, если такая ячейка не существует.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void ChangeCellSelection(int row, int column)
        {
            Cell cell = FindCell(row, column);
            cell.IsSelected = !cell.IsSelected;
            var cellGroups = cell.Groups;
            if (cell.IsSelected)
            {
                foreach (Group group in cellGroups)
                {
                    _selectedGroups.Remove(group);
                    _selectedGroups.Add(group);
                }
            }
        }

        /// <summary>
        /// Изменяет выделение ячейки на заданное значение или вызывает
        /// ArgumentException, если такая ячейка не существует.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void ChangeCellSelection(int row, int column, bool selected)
        {
            Cell cell = FindCell(row, column);
            cell.IsSelected = selected;
            var cellGroups = cell.Groups;
            if (cell.IsSelected)
            {
                foreach (Group group in cellGroups)
                {
                    // выношу группу в конец листа выделенных групп
                    _selectedGroups.Remove(group);
                    _selectedGroups.Add(group);
                }
            }
        }

        /// <summary>
        /// Группирует выделенные ячейки в новую группу
        /// </summary>
        /// <param name="type"></param>
        public void GroupSelected(GroupType type)
        {
            if (_saves.Count == 0)
                Save();

            Group group = new Group(NextFreeNum(_groups.Select(g => g.ID)), this);
            group.Type = type;
            _groups.Add(group);
            foreach (Cell cell in _cells)
            {
                if (cell.IsSelected)
                {
                    group.AddCell(cell);
                }
            }

            Save();
        }

        /// <summary>
        /// Снимает выделение со всех ячеек
        /// </summary>
        public void ClearSelection()
        {
            foreach (var cell in _cells)
            {
                cell.IsSelected = false;
            }
        }

        /// <summary>
        /// Очищает текущий экземпляр
        /// </summary>
        public void Clear()
        {
            if (_saves.Count == 0)
                Save();

            for (int i = 0; i < _groups.Count; i++)
            {
                RemoveGroup(_groups[i].ID);
            }

            for (int i = 0; i < _cells.Count; i++)
            {
                Cell cell = _cells[i];
                cell.Correct = 0;
                cell.IsAvailable = true;
                cell.IsSelected = false;
            }

            _selectedGroups.Clear();
            Save();
        }

        /// <summary>
        /// Отменяет последнее изменение
        /// </summary>
        public void Undo()
        {
            if (_saveInd == 0)
                return;

            _saveInd--;
            MapSave save = _saves.ElementAt(_saveInd);
            save.LoadTo(this);
        }

        /// <summary>
        /// Восстанавливает последнее изменение
        /// </summary>
        public void Redo()
        {
            if (_saveInd == _saves.Count - 1)
                return;

            _saveInd++;
            MapSave save = _saves.ElementAt(_saveInd);
            save.LoadTo(this);
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
        /// Возвращает ячейку карты или вызывает исключение,
        /// если такой ячейки не существует
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public CellInterface GetCell(int row, int column)
        {
            return FindCell(row, column).GetInterface();
        }

        public List<CellInterface> GetSelectedCells()
        {
            var cells = new List<CellInterface>();
            foreach (Cell cell in _cells)
            {
                if (cell.IsSelected) 
                    cells.Add(cell.GetInterface());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает все области карты
        /// </summary>
        /// <returns></returns>
        public List<GroupInterface> GetGroups()
        {
            List<GroupInterface> groups = new List<GroupInterface>();
            foreach (Group group in _groups)
            {
                groups.Add(group.GetInterface());
            }

            return groups;
        }

        public List<ConflictInterface> GetConflicts()
        {
            List<ConflictInterface> conflicts = new List<ConflictInterface>();
            foreach (Conflict conflict in _conflicts)
            {
                conflicts.Add(conflict.GetInterface());
            }

            return conflicts;
        }

        /// <summary>
        /// Возвращает ячейки, принадлежащие области с заданным идентификатором
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<CellInterface> GetCellsInArea(int id)
        {
            Group area = FindGroup(id, createNew: false);
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

            ClearSelection();
            return new MapInterface(this);
        }

        /// <summary>
        /// Создает копию текущего объекта и возвращает ее.
        /// </summary>
        /// <returns></returns>
        public Map Clone()
        {
            Map clone = new Map(Width, Height);
            CopyTo(clone);
            return clone;
        }

        /// <summary>
        /// Копирует содержимое текущего экземпляра в переданный.
        /// Вызывает исключение, если карты имеют различные размеры.
        /// </summary>
        /// <param name="map"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void CopyTo(Map map)
        {
            if (map.Width != Width || map.Height != Height)
                throw new InvalidOperationException(_incorrectSizesForCopyOperationMessage);
            
            map.Clear();
            foreach (Cell cell in _cells)
            {
                map.Write(cell.Row, cell.Column, cell.Correct);
                if (cell.IsSelected)
                    map.ChangeCellSelection(cell.Row, cell.Column);
                if (!cell.IsAvailable)
                    map.ChangeAvailability(cell.Row, cell.Column);
                foreach (Group group in cell.Groups)
                {
                    map.AddCellToGroup(cell, group.ID);
                }
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
            CreateTestGroups();
        }

        private void InitializeCells()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    Cell cell = new Cell(row, col);
                    cell.IsAvailable = true;
                    _cells.Add(cell);
                }
            }
        }

        private Cell this[int row, int column]
        {
            get
            {
                return _cells.Find(c => c.Row == row && c.Column == column);
            }
        }

        private void CreateGroup(Cell cell, int id, GroupType type)
        {
            CreateGroup(cell.Row, cell.Column, id, type);
        }

        private void AddCellToGroup(Cell cell, int id)
        {
            AddCellToGroup(cell.Row, cell.Column, id);
        }

        private Group FindGroup(int id, bool createNew)
        {
            Group group = _groups.Find(c => c.ID == id);
            if (group == null)
            {
                if (createNew)
                {
                    group = new Group(NextFreeNum(_groups.Select(g => g.ID)), this);
                    _groups.Add(group);
                    return group;
                }

                else throw new ArgumentException(_areaNotFoundMessage);
            }

            return group;
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

        private void CreateTestGroups()
        {
            foreach (Cell cell in _cells)
            {
                // из-за целочисленного деления cell.Row / 3 * 3 не лишено смысла
                int id = cell.Column / 3 + cell.Row / 3 * 3;

                if (_groups.Find(a => a.ID == id) == null)
                    CreateGroup(cell, id, GroupType.Basic);

                AddCellToGroup(cell, id);
            }
        }

        private void Save()
        {
            if (_saves.Count == _savesCapacity)
            {
                _saves.Remove(_saves.First());
            }

            MapSave save = new MapSave(this);
            while (_saveInd < _saves.Count - 1)
            {
                _saves.Remove(_saves.Last());
            }

            _saves.Add(save);
            _saveInd = _saves.Count - 1;
        }

        private int NextFreeNum(IEnumerable<int> ints)
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                if (!ints.Contains(i))
                    return i;
            }

            throw new InvalidOperationException();
        }

        private void UpdateConflicts(int value)
        {
            if (value == 0)
                return;

            _conflicts.RemoveAll(c => c.ConflictValue == value);
            var cellByValue = _cells.Where(c => c.Correct == value);
            HashSet<Cell> conflictList = new HashSet<Cell>();
            foreach (var cell in cellByValue)
            {
                if (conflictList.Contains(cell))
                    continue;

                var conflictByRowsCols = cellByValue.Where(c => (c.Row == cell.Row ||
                    c.Column == cell.Column) && c != cell);

                var cellGroups = cell.Groups.Select(g => g.ID);

                var conflictByGroup = cellByValue.Where(c => 
                    c.Groups.Intersect(cell.Groups).Count() != 0 && c != cell);

                if (conflictByGroup.Count() == 0 && conflictByRowsCols.Count() == 0)
                    continue;

                conflictList.Add(cell);
                conflictList = conflictList.Concat(conflictByRowsCols).ToHashSet();
                conflictList = conflictList.Concat(conflictByGroup).ToHashSet();
            }

            if (conflictList.Count == 0)
                return;

            Conflict updatedConflict = new Conflict(conflictList);
            _conflicts.Add(updatedConflict);
        }

        private class Cell
        {
            private int _correct;

            private readonly List<Group> _areas = new List<Group>();

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

            public IReadOnlyCollection<Group> Groups { get { return _areas; } }

            public void AddGroup(Group area)
            {
                if (!IsAvailable)
                    return;

                if (!_areas.Contains(area))
                {
                    _areas.Add(area);
                    area.AddCell(this);
                }
            }

            public void RemoveGroup(Group area)
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

                foreach (Group area in _areas)
                {
                    areas.Add(area.GetInterface());
                }

                CellInterface res = new CellInterface(Correct, Row, Column,
                    IsAvailable, areas);
                res.IsSelected = IsSelected;
                return res;
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

            public ConflictInterface GetInterface()
            {
                List<CellInterface> cells = new List<CellInterface>();
                foreach (Cell cell in _cells)
                {
                    cells.Add(cell.GetInterface());
                }

                return new ConflictInterface(cells);
            }
        }

        public class ConflictInterface
        {
            private IReadOnlyCollection<CellInterface> _cells;

            public ConflictInterface(IReadOnlyCollection<CellInterface> cells)
            {
                _cells = cells;
            }

            public int ConflictValue => _cells.First().Correct;

            public IReadOnlyCollection<CellInterface> Cells { get { return _cells; } }
        }

        public enum GroupType
        {
            Basic,
            Sum
        }

        private class Group
        {
            private readonly int _id;
            private readonly List<Cell> _cells = new List<Cell>();
            private Map _map;

            public Group(int id, Map map)
            {
                _map = map;
                _id = id;
                Type = GroupType.Basic;
            }

            public int ID { get { return _id; } }

            public IReadOnlyCollection<Cell> Cells { get { return _cells; } }

            public int Sum => _cells.Sum(c => c.Correct);

            public GroupType Type { get; set; }

            public bool IsSelected => _cells.Where(c => c.IsSelected).Count() > 0;

            public bool AddCell(Cell cell)
            {
                if (!cell.IsAvailable)
                    return false;

                if (!_cells.Contains(cell))
                {
                    _cells.Add(cell);
                    cell.AddGroup(this);
                    _map.UpdateConflicts(cell.Correct);
                    return true;
                }

                return false;
            }

            public void RemoveCell(Cell cell)
            {
                if (_cells.Contains(cell))
                {
                    _cells.Remove(cell);
                    cell.RemoveGroup(this);
                }
            }

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
                GroupInterface res = new GroupInterface(ID, Type, Sum);
                res.IsSelected = IsSelected;
                return res;
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

        private class MapSave
        {
            private List<CellInterface> _cells = new List<CellInterface>();
            private List<GroupInterface> _groups = new List<GroupInterface>();
            private List<GroupInterface> _selectedGroups = new List<GroupInterface>();
            private List<ConflictInterface> _conflicts = new List<ConflictInterface>();

            public MapSave(Map map)
            {
                _cells = map.GetCells();
                _groups = map.GetGroups();
                foreach (Group g in map._selectedGroups)
                {
                    _selectedGroups.Add(g.GetInterface());
                }
                _conflicts = map.GetConflicts();
            }

            public int Height => _cells.Select(c => c.Row).Max();

            public int Width => _cells.Select(c => c.Column).Max();

            public List<CellInterface> Cells => _cells;

            public List<GroupInterface> Groups => _groups;

            public List<ConflictInterface> Conflicts => _conflicts;

            public List<GroupInterface> SelectedGroups => _selectedGroups;

            public void LoadTo(Map map)
            {
                map._cells.Clear();
                map._groups.Clear();
                map._selectedGroups.Clear();
                map._conflicts.Clear();

                foreach (var cell in _cells)
                {
                    Cell target = new Cell(cell.Row, cell.Column);
                    target.Correct = cell.Correct;
                    target.IsAvailable = cell.IsAvailable;
                    target.IsSelected = cell.IsSelected;
                    map._cells.Add(target);
                }

                foreach (var group in _groups)
                {
                    Group target = new Group(group.ID, map)
                    {
                        Type = group.Type
                    };
                    foreach (var cell in _cells)
                    {
                        if (cell.Groups.Select(g => g.ID).Contains(group.ID))
                        {
                            target.AddCell(map[cell.Row, cell.Column]);
                        }
                    }
                    map._groups.Add(target);
                }

                foreach (var selG in _selectedGroups)
                {
                    Group target = map._groups.Find(g => g.ID == selG.ID);
                    map._selectedGroups.Add(target);
                }

                foreach (var conf in _conflicts)
                {
                    List<Cell> cells = new List<Cell>();
                    foreach (var cell in conf.Cells)
                    {
                        cells.Add(map[cell.Row, cell.Column]);
                    }
                    Conflict target = new Conflict(cells);
                    map._conflicts.Add(target);
                }
            }
        }
    }
}
