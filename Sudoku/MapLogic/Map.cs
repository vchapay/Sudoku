using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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

    [DataContract]
    /// <summary>
    /// Описывает карту и предоставляет средства для ее редактирования,
    /// а также получения интерфейса для проигрывания
    /// </summary>
    internal sealed class Map
    {
        /// <summary>
        /// Минимальное значение для ширины и длины карты в ячейках.
        /// </summary>
        public const int MinLength = 3;

        /// <summary>
        /// Максимальное значение для ширины и длины карты в ячейках.
        /// </summary>
        public const int MaxLength = 20;

        /// <summary>
        /// Стандартное значение для ширины и длины карты в ячейках.
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
        /// Создает карту с указанными размерами.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="heght"></param>
        public Map(int width, int heght) 
        {
            if (width < MinLength || width > MaxLength)
                width = BaseLength;

            if (heght < MinLength || heght > MaxLength)
                heght = BaseLength;

            ColumnsCount = width;
            RowsCount = heght;
            InitializeCells();

            _savesCapacity = 15;
            Save();
        }

        /// <summary>
        /// Создает карту со стандартными размерами 9х9.
        /// </summary>
        public Map() : this(BaseLength, BaseLength) 
        {
        }

        /// <summary>
        /// Число столбцов на карте.
        /// </summary>
        public int ColumnsCount { get; }

        /// <summary>
        /// Число строк на карте.
        /// </summary>
        public int RowsCount { get; }

        /// <summary>
        /// Возвращает тип карты, определяемый встроенным алгоритмом.
        /// </summary>
        public MapTypes Type { get { return _type; } }

        /// <summary>
        /// Название карты.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание карты.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Возвращает число конфликтов между ячейками.
        /// </summary>
        public int ConflictsCount { get { return _conflicts.Count; } }

        /// <summary>
        /// Объем буфера для быстрых сохранений карты,
        /// необходимых для поддержки методов Redo и Undo.
        /// </summary>
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
        /// Записывает новое значение во все выделенные ячейки.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        public void Write(int value)
        {
            if (_saves.Count == 0)
                Save();

            var selectedCells = GetSelectedCells();
            foreach (CellInfo cell in selectedCells)
            {
                if (cell.Correct == value)
                    continue;

                Cell c = this[cell.Row, cell.Column];
                int oldValue = c.Correct;
                c.Correct = value;

                if (!c.IsAvailable && c.Correct == 0)
                {
                    for (int i = 0; i < c.Groups.Count; i++)
                    {
                        Group g = c.Groups.ElementAt(i);
                        g.RemoveCell(c);
                    }
                }

                UpdateConflicts(value);
                UpdateConflicts(oldValue);
            }

            Save();
        }

        /// <summary>
        /// Записывает в заданную ячейку значение или вызывает исключение,
        /// если ячейка не существует.
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

            if (!cell.IsAvailable && cell.Correct == 0)
            {
                for (int i = 0; i < cell.Groups.Count; i++)
                {
                    Group g = cell.Groups.ElementAt(i);
                    g.RemoveCell(cell);
                }
            }

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

            if (!cell.IsAvailable && cell.Correct == 0)
            {
                for (int i = 0; i < cell.Groups.Count; i++)
                {
                    Group g = cell.Groups.ElementAt(i);
                    g.RemoveCell(cell);
                }
            }

            Save();
        }

        /// <summary>
        /// Изменяет доступность для ввода игроком в выделенные ячейки.
        /// </summary>
        public void ChangeSelectedAvailability()
        {
            if (_saves.Count == 0)
                Save();

            foreach (Cell cell in _cells)
            {
                if (cell.IsSelected)
                    cell.IsAvailable = !cell.IsAvailable;

                if (!cell.IsAvailable && cell.Correct == 0)
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
        /// Создает группу и добавляет в нее указанную ячейку.
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
        /// Удаляет группу.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveGroup(int id)
        {
            if (_saves.Count == 0)
                Save();

            Group group = FindGroup(id, createNew: false);
            if (group == null) throw new InvalidOperationException(_areaNotFoundMessage);
            group.Clear();
            _groups.Remove(group);
            Save();
        }

        /// <summary>
        /// Добавляет все выделенные ячейки в группу, которая была выделена последней.
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
        /// Добавляет все выделенные ячейки в группу с заданным идентификатором.
        /// Создает новую группу, если группы с таким идентификатором не существует.
        /// Не создает группу, если не выделена ни одна ячейка.
        /// </summary>
        /// <param name="id"></param>
        /// <returns> True, если группа была создана. Иначе false. </returns>
        public bool AddSelectedToGroup(int id)
        {
            if (_saves.Count == 0)
                Save();

            var selected = GetSelectedCells();
            if (selected.Count == 0)
                return false;

            Group group = FindGroup(id, createNew: true);
            foreach (CellInfo cell in selected)
            {
                Cell c = this[cell.Row, cell.Column];
                if (cell.IsSelected)
                    group.AddCell(c);
            }

            Save();

            return true;
        }

        /// <summary>
        /// Удаляет все выделенные ячейки из группы, которая была выделена последней.
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
        /// Создает новую группу, если полученный идентификатор не занят.
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
        /// Удаляет ячейку из группы, если группа и ячейка существуют.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="id"></param>
        public void RemoveCellFromGroup(int row, int column, int id)
        {
            Cell cell = FindCell(row, column);
            Group group = FindGroup(id, createNew: false);
            if (group != null)
            {
                if (_saves.Count == 0)
                    Save();

                group.RemoveCell(cell);
                if (group.Cells.Count == 0)
                {
                    _groups.Remove(group);
                    _selectedGroups.Remove(group);
                }

                Save();
            }
        }

        /// <summary>
        /// Изменяет тип группы на заданный, если группа существует,
        /// иначе вызывает исключение
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public void ChangeGroupType(int id, GroupType type)
        {
            if (_saves.Count == 0)
                Save();

            Group group = FindGroup(id, createNew: false);
            if (group == null) throw new InvalidOperationException(_areaNotFoundMessage);
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
        /// Создает новую группу и добавляет все выделенные ячейки в нее.
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
        /// Снимает выделение со всех ячеек.
        /// </summary>
        public void ClearSelection()
        {
            foreach (var cell in _cells)
            {
                cell.IsSelected = false;
            }
        }

        /// <summary>
        /// Очищает текущий экземпляр.
        /// </summary>
        public void Clear()
        {
            if (_saves.Count == 0)
                Save();

            _conflicts.Clear();
            for (; 0 < _groups.Count;)
            {
                RemoveGroup(_groups[0].ID);
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
        /// Отменяет последнее изменение.
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
        /// Восстанавливает последнее изменение.
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
        /// Возвращает объекты с информацией о каждой ячейке.
        /// </summary>
        /// <returns></returns>
        public List<CellInfo> GetCells()
        {
            List<CellInfo> cells = new List<CellInfo>();
            foreach (Cell cell in _cells)
            {
                cells.Add(cell.GetInfo());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает оболочки игрового взаимодействия всех ячеек.
        /// </summary>
        /// <returns></returns>
        public List<CellInterface> GetCellsInterfaces()
        {
            List<CellInterface> cells = new List<CellInterface>();
            foreach (Cell cell in _cells)
            {
                cells.Add(cell.GetInterface());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает объект с информацией о ячейке карты или вызывает исключение,
        /// если такой ячейки не существует.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public CellInfo GetCell(int row, int column)
        {
            return FindCell(row, column).GetInfo();
        }

        /// <summary>
        /// Возвращает объекты с информацией о всех выделенных ячейках.
        /// </summary>
        /// <returns></returns>
        public List<CellInfo> GetSelectedCells()
        {
            List<CellInfo> cells = new List<CellInfo>();
            foreach (Cell cell in _cells)
            {
                if (cell.IsSelected) 
                    cells.Add(cell.GetInfo());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает объекты с информацией о всех группы карты.
        /// </summary>
        /// <returns></returns>
        public List<GroupInfo> GetGroups()
        {
            List<GroupInfo> groups = new List<GroupInfo>();
            foreach (Group group in _groups)
            {
                groups.Add(group.GetInfo());
            }

            return groups;
        }

        /// <summary>
        /// Возвращает оболочки игрового взаимодействия всех групп.
        /// </summary>
        /// <returns></returns>
        public List<GroupInterface> GetGroupsInterfaces()
        {
            List<GroupInterface> groups = new List<GroupInterface>();
            foreach (Group group in _groups)
            {
                groups.Add(group.GetInterface());
            }

            return groups;
        }

        /// <summary>
        /// Возвращает объекты с информацией о конфликтах карты.
        /// </summary>
        /// <returns></returns>
        public List<ConflictInfo> GetConflicts()
        {
            List<ConflictInfo> conflicts = new List<ConflictInfo>();
            foreach (Conflict conflict in _conflicts)
            {
                conflicts.Add(conflict.GetInterface());
            }

            return conflicts;
        }

        /// <summary>
        /// Возвращает объекты с информацией о ячейках, 
        /// принадлежащих группе с заданным идентификатором.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<CellInfo> GetCellsByGroup(int id)
        {
            Group group = FindGroup(id, createNew: false);
            if (group == null) throw new InvalidOperationException(_areaNotFoundMessage);
            List<CellInfo> cells = new List<CellInfo>();

            foreach (Cell cell in group.Cells)
            {
                cells.Add(cell.GetInfo());
            }

            return cells;
        }

        /// <summary>
        /// Возвращает интерфейс для игрового взаимодействия на основе текущего экземпляра.
        /// Вызывает исключение, если на карте обнаружены явные конфликты: повторение значения
        /// по вертикали, горизонтали или внутри групп.
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
            Map clone = new Map(ColumnsCount, RowsCount);
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
            if (map.ColumnsCount != ColumnsCount || map.RowsCount != RowsCount)
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
        /// Заполняет текущий экзмепляр задачей судоку по-умолчанию.
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
            for (int row = 0; row < RowsCount; row++)
            {
                for (int col = 0; col < ColumnsCount; col++)
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
                    group = new Group(id, this);
                    _groups.Add(group);
                    return group;
                }
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

            Conflict updatedConflict = new Conflict(conflictList, value);
            _conflicts.Add(updatedConflict);
        }

        private class Cell
        {
            private int _correct;

            private readonly List<Group> _groups = new List<Group>();

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
                    if (_correct > -1 && _correct < 21)
                        _correct = value;
                }
            }

            public int Row { get; set; }

            public int Column { get; set; }

            public bool IsAvailable { get; set; }

            public bool IsSelected { get; set; }

            public IReadOnlyCollection<Group> Groups { get { return _groups; } }

            public void AddGroup(Group area)
            {
                if (!IsAvailable && Correct == 0)
                    return;

                if (!_groups.Contains(area))
                {
                    _groups.Add(area);
                    area.AddCell(this);
                }
            }

            public void RemoveGroup(Group area)
            {
                if (_groups.Contains(area))
                {
                    _groups.Remove(area);
                    area.RemoveCell(this);
                }
            }

            public CellInterface GetInterface()
            {
                List<int> groupsID = new List<int>();

                foreach (Group group in _groups)
                {
                    groupsID.Add(group.ID);
                }

                CellInterface res = new CellInterface(Correct, Row, Column,
                    IsAvailable, groupsID);
                return res;
            }

            public CellInfo GetInfo()
            {
                List<GroupInfo> groups = new List<GroupInfo>();

                foreach (Group g in _groups)
                {
                    groups.Add(g.GetInfo());
                }

                CellInfo res = new CellInfo(Row, Column, Correct,
                    groups.Select(g => g.ID), IsAvailable, IsSelected);
                return res;
            }
        }

        private class Conflict
        {
            private readonly List<Cell> _cells;

            public Conflict(ICollection<Cell> cells, int conflictValue)
            {
                _cells = cells.ToList();
                ConflictValue = conflictValue;
            }

            public List<Cell> Cells { get { return _cells; } }

            public int ConflictValue { get; }

            public ConflictInfo GetInterface()
            {
                List<CellInfo> cells = new List<CellInfo>();
                foreach (Cell cell in _cells)
                {
                    cells.Add(cell.GetInfo());
                }

                return new ConflictInfo(cells, ConflictValue);
            }
        }

        private class Group
        {
            private readonly int _id;

            private readonly List<Cell> _cells = new List<Cell>();

            public Group(int id, Map map)
            {
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
                if (!cell.IsAvailable && cell.Correct == 0)
                    return false;

                if (!_cells.Contains(cell))
                {
                    _cells.Add(cell);
                    cell.AddGroup(this);
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
                return res;
            }

            public GroupInfo GetInfo()
            {
                GroupInfo res = new GroupInfo(ID, Type, Sum, IsSelected);
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
            private readonly List<CellInterface> _cells = new List<CellInterface>();
            private readonly List<GroupInfo> _groups = new List<GroupInfo>();
            private readonly List<GroupInfo> _selectedGroups = new List<GroupInfo>();
            private readonly List<ConflictInfo> _conflicts = new List<ConflictInfo>();

            public MapSave(Map map)
            {
                _cells = map.GetCellsInterfaces();
                _groups = map.GetGroups();
                foreach (Group g in map._selectedGroups)
                {
                    _selectedGroups.Add(g.GetInfo());
                }
                _conflicts = map.GetConflicts();
            }

            public int Height => _cells.Select(c => c.Row).Max();

            public int Width => _cells.Select(c => c.Column).Max();

            public List<CellInterface> Cells => _cells;

            public List<GroupInfo> Groups => _groups;

            public List<ConflictInfo> Conflicts => _conflicts;

            public List<GroupInfo> SelectedGroups => _selectedGroups;

            public void LoadTo(Map map)
            {
                map._cells.Clear();
                map._groups.Clear();
                map._selectedGroups.Clear();
                map._conflicts.Clear();

                foreach (var cell in _cells)
                {
                    Cell target = new Cell(cell.Row, cell.Column)
                    {
                        Correct = cell.Correct,
                        IsAvailable = cell.IsAvailable,
                        IsSelected = cell.IsSelected
                    };
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
                        if (cell.Groups.Contains(group.ID))
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

                    Conflict target = new Conflict(cells, conf.ConflictValue);
                    map._conflicts.Add(target);
                }
            }
        }
    }
}
