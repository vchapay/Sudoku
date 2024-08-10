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
            this.scenesContainer = new Sudoku.Controls.ScenesContainer();
            this.SuspendLayout();
            // 
            // scenesContainer
            // 
            this.scenesContainer.BackColor = System.Drawing.Color.White;
            this.scenesContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scenesContainer.Control = null;
            this.scenesContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scenesContainer.Location = new System.Drawing.Point(0, 0);
            this.scenesContainer.Name = "scenesContainer";
            this.scenesContainer.Size = new System.Drawing.Size(1539, 693);
            this.scenesContainer.TabIndex = 0;
            this.scenesContainer.Text = "scenesContainer1";
            this.scenesContainer.BackButtonClicked += new System.EventHandler(this.BackScene);
            this.scenesContainer.ExitButtonClicked += new System.EventHandler(this.CloseForm);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1539, 693);
            this.Controls.Add(this.scenesContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ScenesContainer scenesContainer;
    }
}

