using Sudoku.Controls;
using Sudoku.MapLogic;
using Sudoku.MapPlayingLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Sudoku
{
    public partial class Form1 : Form
    {
        private const string _mapsFolder = "D:\\По шарпу\\Судоку\\Папка для карт";
        private const string _extension = ".sdkm";
        private readonly SudokuMainMenu _menu;
        private readonly SudokuMapsPage _mapListPage;
        private readonly SudokuEditor _mapEditor;
        private readonly SudokuPlayerDisplay _mapPlayer;
        private readonly SudokuCreatingPage _mapCreatingPage;
        private readonly SudokuPreviewPage _mapPreviewPage;
        private Stack<Control> _prevControls;
        private MapInterface _lastGame;

        public Form1()
        {
            InitializeComponent();
            _prevControls = new Stack<Control>();

            _menu = new SudokuMainMenu();
            _mapListPage = new SudokuMapsPage();
            _mapEditor = new SudokuEditor();
            _mapPlayer = new SudokuPlayerDisplay();
            _mapCreatingPage = new SudokuCreatingPage();
            _mapPreviewPage = new SudokuPreviewPage();
            _scenesContainer.Control = _menu;

            InitializeHandlers();

            /*string name = sudokuMaker1.Map.MapName;
            sudokuMaker1.SavePath = $"D:\\По шарпу\\Судоку\\Папка для карт\\{name}.sdkm";*/
        }

        private void InitializeHandlers()
        {
            _menu.EditorButtonClicked += GoToMapsList;
            _menu.ContinueButtonClicked += ContinueGame;

            _mapListPage.CreateMapButtonClicked += OpenCreatingPage;
            _mapListPage.ViewMapButtonClicked += OpenPreviewPage;
            _mapListPage.ExportMapButtonClicked += ExportMap;
            _mapListPage.DeleteMapButtonClicked += WhenDeleteButtonClicked;
            _mapListPage.ImportMapButtonClicked += ImportMap;

            _mapCreatingPage.CreateMapClicked += CreateMap;

            _mapPreviewPage.EditingClicked += OpenEditorPage;
            _mapPreviewPage.CopyingClicked += WhenCopyButtonClicked;
            _mapPreviewPage.PlayingClicked += OpenPlayingPage;

            _mapEditor.SaveButtonClicked += SaveMap;
            _mapEditor.PlayButtonClicked += OpenPlayingPage;
        }

        private void ContinueGame(object sender, EventArgs e)
        {
            _prevControls.Push(_menu);
            _scenesContainer.Control = _mapPlayer;
            _mapPlayer.Map = _lastGame;
        }

        private void OpenPlayingPage(object sender, MapActionClickArgs e)
        {
            if (e.Map.ConflictsCount > 0)
            {
                MessageBox.Show("Чтобы играть в карту, " +
                    "необходимо, чтобы на ней не было конфликтных ячеек");
                return;
            }

            _prevControls.Push((Control)sender);
            _scenesContainer.Control = _mapPlayer;
            _mapPlayer.Map = e.Map.GetInterface();
        }

        private void WhenCopyButtonClicked(object sender, MapActionClickArgs e)
        {
            if (MessageBox.Show("Скопировать карту?", "Копирование карты",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            Map copy = e.Map.Clone();

            string adding;
            string name;
            var usedNames = _mapListPage.Maps.Select(m => m.Name);
            for (int i = 1; i < int.MaxValue; i++)
            {
                adding = $"_copy{i}";
                name = e.Map.Name + adding;
                if (!usedNames.Contains(name))
                {
                    copy.Name = name;
                    break;
                }
            }

            _mapPreviewPage.Map = copy;

            string filePath = $"{_mapsFolder}\\{copy.ID}_{copy.Name}{_extension}";
            SerializeMap(copy, filePath);
        }

        private void SaveMap(object sender, MapActionClickArgs e)
        {
            Map map = e.Map;
            string filePath = $"{_mapsFolder}\\{map.ID}_{map.Name}{_extension}";

            DeleteMap(map);

            try
            {
                SerializeMap(map, filePath);
            }

            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void OpenEditorPage(object sender, MapActionClickArgs e)
        {
            _prevControls.Push(_mapPreviewPage);
            OpenEditor(e.Map);
        }

        private void CreateMap(object sender, MapCreatingArgs e)
        {
            Map map = new Map(e.Size.Width, e.Size.Height)
            {
                Name = e.Name,
                Description = e.Description,
            };

            string filePath = $"{_mapsFolder}\\{map.ID}_{map.Name}{_extension}";
            SerializeMap(map, filePath);
            _mapListPage.AddMap(map);
            OpenEditor(map);
        }

        private void OpenEditor(Map map)
        {
            if (_scenesContainer.Control == _mapPreviewPage)
            {
                SaveMap(_mapPreviewPage,
                    new MapActionClickArgs(_mapPreviewPage.Map));
            }

            _scenesContainer.Control = _mapEditor;
            _mapEditor.Map = map;
        }

        private void ImportMap(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = _extension;
            dialog.AddExtension = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Map map = DeserializeMap(dialog.FileName);
                    _mapListPage.AddMap(map);
                    OpenPreviewPage(_mapListPage,
                        new MapActionClickArgs(map));
                }

                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void WhenDeleteButtonClicked(object sender, MapActionClickArgs e)
        {
            DeleteMap(e.Map);
        }

        private void DeleteMap(Map map)
        {
            DirectoryInfo directory = new DirectoryInfo(_mapsFolder);
            var files = directory.EnumerateFiles($"*{_extension}");
            foreach (var file in files)
            {
                try
                {
                    Map m = DeserializeMap(file.FullName);
                    if (m.ID == map.ID)
                    {
                        File.Delete(file.FullName);
                    }
                }

                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void ExportMap(object sender, MapActionClickArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = _extension;
            dialog.AddExtension = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SerializeMap(e.Map, dialog.FileName);
            }
        }

        private void OpenPreviewPage(object sender, MapActionClickArgs e)
        {
            _prevControls.Push(_mapListPage);
            _scenesContainer.Control = _mapPreviewPage;
            _mapPreviewPage.Map = e.Map;
        }

        private void OpenCreatingPage(object sender, EventArgs e)
        {
            _prevControls.Push(_mapListPage);
            _scenesContainer.Control = _mapCreatingPage;

            var usedNames = _mapListPage.Maps.Select(m => m.Name).ToArray();
            string name;
            for (int i = 1; i < int.MaxValue; i++)
            {
                name = $"map{i}";

                if (!usedNames.Contains(name))
                {
                    _mapCreatingPage.MapName = name;
                    break;
                }
            }
        }

        private void GoToMapsList(object sender, EventArgs e)
        {
            UpdateMapsList();

            _prevControls.Push(_menu);
            _scenesContainer.Control = _mapListPage;
            _scenesContainer.IsBackButtonVisible = true;
            _scenesContainer.Invalidate();
        }

        private void UpdateMapsList()
        {
            DirectoryInfo directory = new DirectoryInfo(_mapsFolder);
            _mapListPage.Clear();
            var files = directory.EnumerateFiles($"*{_extension}");
            foreach (var file in files)
            {
                try
                {
                    Map map = DeserializeMap(file.FullName);
                    _mapListPage.AddMap(map);
                }

                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void BackScene(object sender, EventArgs e)
        {
            if (_scenesContainer.Control == _mapPreviewPage)
            {
                _mapPreviewPage.Map.ClearSelection();
                SaveMap(_mapPreviewPage,
                    new MapActionClickArgs(_mapPreviewPage.Map));
            }

            if (_scenesContainer.Control == _mapEditor)
            {
                if (!_mapEditor.IsMapSaved)
                {
                    if (MessageBox.Show("Некоторые изменения не были сохранены. " +
                        "Все равно выйти?", "Изменения не были сохранены",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }
            }

            Control prev = _prevControls.Pop();

            if (prev == _mapListPage)
            {
                UpdateMapsList();
            }

            if (_prevControls.Count == 0)
            {
                _scenesContainer.IsBackButtonVisible = false;
            }

            _scenesContainer.Control = prev;
        }

        private void CloseForm(object sender, EventArgs e)
        {
            if (_scenesContainer.Control == _mapEditor)
            {
                if (!_mapEditor.IsMapSaved)
                {
                    if (MessageBox.Show("Некоторые изменения не были сохранены. " +
                        "Все равно выйти?", "Изменения не были сохранены",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }
            }

            Close();
        }

        private static void SerializeMap(Map map, string filePath)
        {
            using (Stream stream = File.OpenWrite(filePath))
            {
                BinaryFormatter serializer =
                    new BinaryFormatter();

                map.ClearSaves();
                serializer.Serialize(stream, map);
            }
        }

        private Map DeserializeMap(string filePath)
        {
            using (Stream stream = File.OpenRead(filePath))
            {
                BinaryFormatter serializer =
                    new BinaryFormatter();

                Map map = (Map)serializer.Deserialize(stream);
                return map;
            }
        }

        private void CollapseForm(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void ChangeExpandingOfForm(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else WindowState = FormWindowState.Maximized;

            _scenesContainer.Invalidate();
            _scenesContainer.Control.Invalidate();
        }
    }
}
