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
            this.sudokuMakerDisplay1 = new Sudoku.SpecialControls.SudokuMakerDisplay();
            this.SuspendLayout();
            // 
            // sudokuMakerDisplay1
            // 
            this.sudokuMakerDisplay1.DataSource = map1;
            this.sudokuMakerDisplay1.Dock = System.Windows.Forms.DockStyle.Right;
            this.sudokuMakerDisplay1.Font = new System.Drawing.Font("Times New Roman", 26F);
            this.sudokuMakerDisplay1.Location = new System.Drawing.Point(850, 0);
            this.sudokuMakerDisplay1.Name = "sudokuMakerDisplay1";
            this.sudokuMakerDisplay1.Size = new System.Drawing.Size(661, 682);
            this.sudokuMakerDisplay1.TabIndex = 0;
            this.sudokuMakerDisplay1.Text = "sudokuMakerDisplay1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1511, 682);
            this.Controls.Add(this.sudokuMakerDisplay1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private SpecialControls.SudokuMakerDisplay sudokuMakerDisplay1;
    }
}

