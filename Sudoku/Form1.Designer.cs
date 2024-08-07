namespace Sudoku
{
    public partial class Form1
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

        private void InitializeComponent()
        {
            this.sudokuListBox2 = new Sudoku.SpecialControls.SudokuListBox();
            this.SuspendLayout();
            // 
            // sudokuListBox2
            // 
            this.sudokuListBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sudokuListBox2.Location = new System.Drawing.Point(196, 51);
            this.sudokuListBox2.Name = "sudokuListBox2";
            this.sudokuListBox2.Size = new System.Drawing.Size(979, 532);
            this.sudokuListBox2.TabIndex = 0;
            this.sudokuListBox2.Text = "sudokuListBox2";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1346, 622);
            this.Controls.Add(this.sudokuListBox2);
            this.Name = "Form1";
            this.Text = "b";
            this.ResumeLayout(false);

        }

        #endregion

        private SpecialControls.SudokuListBox sudokuListBox1;
        private SpecialControls.SudokuListBox sudokuListBox2;
    }
}

