namespace Sudoku
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.sudokuMaker1 = new Sudoku.SpecialControls.SudokuMaker();
            this.SuspendLayout();
            // 
            // sudokuMaker1
            // 
            this.sudokuMaker1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.sudokuMaker1.CellPanelHeight = 250F;
            this.sudokuMaker1.CellPanelsSplitterWidth = 3F;
            this.sudokuMaker1.Font = new System.Drawing.Font("Times New Roman", 26F);
            this.sudokuMaker1.Location = new System.Drawing.Point(334, 53);
            this.sudokuMaker1.Map = ((Sudoku.MapLogic.Map)(resources.GetObject("sudokuMaker1.Map")));
            this.sudokuMaker1.Name = "sudokuMaker1";
            this.sudokuMaker1.Size = new System.Drawing.Size(860, 438);
            this.sudokuMaker1.TabIndex = 0;
            this.sudokuMaker1.Text = "sudokuMaker1";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1496, 549);
            this.Controls.Add(this.sudokuMaker1);
            this.Name = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private SpecialControls.SudokuMaker sudokuMaker1;
    }
}

