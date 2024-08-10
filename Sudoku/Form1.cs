using Sudoku.Controls;
using Sudoku.MapLogic;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class Form1 : Form
    {
        private SudokuMainMenu menu;
        private SudokuMapsPage mapListPage;
        private SudokuEditor mapEditor;
        private SudokuPlayerDisplay mapPlayer;
        private MapCreatingPage mapCreatingPage;

        public Form1()
        {
            InitializeComponent();
            SudokuMapsPage mapsControl = new SudokuMapsPage();
            scenesContainer.Control = mapsControl;
            /*string name = sudokuMaker1.Map.MapName;
            sudokuMaker1.SavePath = $"D:\\По шарпу\\Судоку\\Папка для карт\\{name}.sdkm";*/
        }

        private void BackScene(object sender, System.EventArgs e)
        {

        }

        private void CloseForm(object sender, System.EventArgs e)
        {
            Close();
        }

        private void scenesContainer1_ExitButtonClicked(object sender, System.EventArgs e)
        {

        }
    }
}
