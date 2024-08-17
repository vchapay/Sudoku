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
            this._scenesContainer = new Sudoku.Controls.ScenesContainer();
            this.SuspendLayout();
            // 
            // _scenesContainer
            // 
            this._scenesContainer.BackColor = System.Drawing.Color.White;
            this._scenesContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._scenesContainer.Control = null;
            this._scenesContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._scenesContainer.IsBackButtonVisible = false;
            this._scenesContainer.Location = new System.Drawing.Point(0, 0);
            this._scenesContainer.Name = "_scenesContainer";
            this._scenesContainer.Size = new System.Drawing.Size(1644, 808);
            this._scenesContainer.TabIndex = 0;
            this._scenesContainer.Text = "scenesContainer1";
            this._scenesContainer.BackButtonClicked += new System.EventHandler(this.BackScene);
            this._scenesContainer.ExitButtonClicked += new System.EventHandler(this.CloseForm);
            this._scenesContainer.CollapseButtonClicked += new System.EventHandler(this.CollapseForm);
            this._scenesContainer.ExpandButtonClicked += new System.EventHandler(this.ChangeExpandingOfForm);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1644, 808);
            this.Controls.Add(this._scenesContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }
        #endregion

        private Controls.SudokuMainMenu sudokuMainMenu1;
        private Controls.ScenesContainer _scenesContainer;
    }
}

