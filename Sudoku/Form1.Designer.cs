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
            this.mapDisplay1 = new Sudoku.SpecialControls.MapDisplay();
            this.SuspendLayout();
            // 
            // mapDisplay1
            // 
            this.mapDisplay1.Dock = System.Windows.Forms.DockStyle.Right;
            this.mapDisplay1.Location = new System.Drawing.Point(829, 0);
            this.mapDisplay1.Name = "mapDisplay1";
            this.mapDisplay1.OpenedCellsFont = new System.Drawing.Font("Times New Roman", 26F);
            this.mapDisplay1.Size = new System.Drawing.Size(682, 682);
            this.mapDisplay1.TabIndex = 0;
            this.mapDisplay1.Text = "mapDisplay1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1511, 682);
            this.Controls.Add(this.mapDisplay1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private SpecialControls.MapDisplay mapDisplay1;
    }
}

