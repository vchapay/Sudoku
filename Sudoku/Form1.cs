using Sudoku.Controls;
using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

            _menu.EditorButtonClicked += GoToMapsList;
            _mapListPage.CreateMapButtonClicked += OpenCreatingPage;
            _mapListPage.ViewMapButtonClicked += OpenPreviewPage;
            _mapListPage.ExportMapButtonClicked += ExportMap;
            _mapListPage.DeleteMapButtonClicked += DeleteMap;
            _mapListPage.ImportMapButtonClicked += ImportMap;
            _mapCreatingPage.CreateMapClicked += CreateMap;
            _mapPreviewPage.EditingClicked += OpenEditorPage;
            _mapEditor.SaveButtonClicked += SaveMap;

            /*string name = sudokuMaker1.Map.MapName;
            sudokuMaker1.SavePath = $"D:\\По шарпу\\Судоку\\Папка для карт\\{name}.sdkm";*/
        }

        private void SaveMap(object sender, MapActionClickArgs e)
        {
            string filePath = $"{_mapsFolder}\\{e.Map.Name}{_extension}";
            using (Stream stream = File.OpenWrite(filePath))
            {
                DataContractSerializer serializer =
                    new DataContractSerializer(new Map().GetType());

                serializer.WriteObject(stream, e.Map);
            }
        }

        private void OpenEditorPage(object sender, MapActionClickArgs e)
        {
            OpenEditor(e.Map);
        }

        private void CreateMap(object sender, MapCreatingArgs e)
        {
            Map map = new Map(e.Size.Width, e.Size.Height)
            {
                Name = e.Name,
                Description = e.Description,
            };

            string filePath = $"{_mapsFolder}\\{map.Name}{_extension}";
            using (Stream stream = File.OpenWrite(filePath))
            {
                DataContractSerializer serializer =
                    new DataContractSerializer(new Map().GetType());

                serializer.WriteObject(stream, map);
                _mapListPage.AddMap(map);

                OpenEditor(map);
            }
        }

        private void OpenEditor(Map map)
        {
            _scenesContainer.Control = _mapEditor;
            _mapEditor.Map = map;
        }

        private void ImportMap(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filePath = $"{_mapsFolder}\\{dialog.FileName}";
                    using (Stream stream = File.OpenRead(filePath))
                    {
                        DataContractSerializer serializer =
                            new DataContractSerializer(new Map().GetType());

                        Map map = (Map)serializer.ReadObject(stream);
                        _mapListPage.AddMap(map);

                        OpenPreviewPage(_mapListPage,
                            new MapActionClickArgs(map));
                    }
                }

                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void DeleteMap(object sender, MapActionClickArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(_mapsFolder);
            var files = directory.EnumerateFiles();
            foreach (var file in files)
            {
                if (file.Name == e.Map.Name)
                {
                    File.Delete(_mapsFolder + "\\" + file.Name);
                }
            }
        }

        private void ExportMap(object sender, MapActionClickArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream stream = File.OpenWrite(_mapsFolder + 
                    $"\\{dialog.FileName}"))
                {
                    DataContractSerializer serializer =
                        new DataContractSerializer(e.Map.GetType());

                    serializer.WriteObject(stream, e.Map);
                }
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
            _mapCreatingPage.UsedNames = _mapListPage.Maps.Select(m => m.Name).ToArray();
        }

        private void GoToMapsList(object sender, EventArgs e)
        {
            _prevControls.Push(_menu);
            _scenesContainer.Control = _mapListPage;
            _scenesContainer.IsBackButtonVisible = true;

            DirectoryInfo directory = new DirectoryInfo(_mapsFolder);
            _mapListPage.Clear();
            var files = directory.EnumerateFiles();
            foreach (var file in files)
            {
                if (file.Extension == _extension)
                {
                    using(Stream stream = File.OpenRead(file.FullName))
                    {
                        DataContractSerializer serializer = 
                            new DataContractSerializer(new Map().GetType());

                        Map map = (Map)serializer.ReadObject(stream);
                        _mapListPage.AddMap(map);
                    }
                }
            }
        }

        private void BackScene(object sender, EventArgs e)
        {
            Control prev = _prevControls.Pop();
            if (_prevControls.Count == 0)
            {
                _scenesContainer.IsBackButtonVisible = false;
            }

            if (_scenesContainer.Control == _mapEditor)
            {
                if (!_mapEditor.IsMapSaved)
                {

                }
            }

            _scenesContainer.Control = prev;
        }

        private void CloseForm(object sender, EventArgs e)
        {
            Close();
        }
    }
}
