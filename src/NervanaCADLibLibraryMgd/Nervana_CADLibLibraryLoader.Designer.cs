namespace NervanaCADLibLibraryMgd
{
    partial class Nervana_CADLibLibraryLoader
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.nervanaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nervanaGroupParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nervanaToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // nervanaToolStripMenuItem
            // 
            this.nervanaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nervanaGroupParametersToolStripMenuItem});
            this.nervanaToolStripMenuItem.Name = "nervanaToolStripMenuItem";
            this.nervanaToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.nervanaToolStripMenuItem.Text = "Nervana";
            // 
            // nervanaGroupParametersToolStripMenuItem
            // 
            this.nervanaGroupParametersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem,
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2,
            this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem});
            this.nervanaGroupParametersToolStripMenuItem.Name = "nervanaGroupParametersToolStripMenuItem";
            this.nervanaGroupParametersToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.nervanaGroupParametersToolStripMenuItem.Text = "Работа с параметрами";
            // 
            // nervanaCommandImportRevitSharedParametersFileToolStripMenuItem
            // 
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem.Name = "nervanaCommandImportRevitSharedParametersFileToolStripMenuItem";
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem.Size = new System.Drawing.Size(348, 22);
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem.Text = "Импорт Revit ФОП";
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem.Click += new System.EventHandler(this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem_Click);
            // 
            // nervanaCommandExportRevitSharedParametersFileToolStripMenuItem
            // 
            this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem.Name = "nervanaCommandExportRevitSharedParametersFileToolStripMenuItem";
            this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem.Size = new System.Drawing.Size(348, 22);
            this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem.Text = "Экспорт Revit ФОП";
            this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem.Click += new System.EventHandler(this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem_Click);
            // 
            // nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2
            // 
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2.Name = "nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2";
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2.Size = new System.Drawing.Size(194, 22);
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2.Text = "Импорт Revit ФОП (2)";
            this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2.Click += new System.EventHandler(this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2_Click);
            // 
            // Nervana_CADLibLibraryLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Nervana_CADLibLibraryLoader";
            this.Text = "Nervana_Ribbon";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem nervanaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nervanaGroupParametersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nervanaCommandImportRevitSharedParametersFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nervanaCommandExportRevitSharedParametersFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nervanaCommandImportRevitSharedParametersFileToolStripMenuItem2;
    }
}
