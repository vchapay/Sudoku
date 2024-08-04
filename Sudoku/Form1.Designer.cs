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
            Sudoku.MapLogic.Map map1 = new Sudoku.MapLogic.Map();
            this.sudokuMaker1 = new Sudoku.SpecialControls.SudokuMaker();
            this.SuspendLayout();
            // 
            // sudokuMaker1
            // 
            this.sudokuMaker1.CellPanelHeight = 250F;
            this.sudokuMaker1.CellPanelsSplitterWidth = 3F;
            this.sudokuMaker1.Font = new System.Drawing.Font("Times New Roman", 26F);
            this.sudokuMaker1.Location = new System.Drawing.Point(277, 65);
            map1.Name = null;
            map1.SavesCapacity = 15;
            this.sudokuMaker1.Map = map1;
            this.sudokuMaker1.Name = "sudokuMaker1";
            this.sudokuMaker1.Size = new System.Drawing.Size(1028, 537);
            this.sudokuMaker1.TabIndex = 0;
            this.sudokuMaker1.Text = "sudokuMaker1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1511, 682);
            this.Controls.Add(this.sudokuMaker1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private SpecialControls.SudokuMaker sudokuMaker1;
    }
}

