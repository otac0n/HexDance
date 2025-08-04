namespace HexDance
{
    partial class HexDisplay
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new NotifyIcon(this.components);
            this.contextMenu = new ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Tick += this.UpdateTimer_Tick;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Text = "Hex Dance";
            this.notifyIcon.Visible = true;
            // 
            // contextMenu
            // 
            this.contextMenu.ImageScalingSize = new Size(24, 24);
            this.contextMenu.Items.AddRange(new ToolStripItem[] { this.closeToolStripMenuItem });
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new Size(128, 36);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new Size(127, 32);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += this.CloseMenuItem_Click;
            // 
            // HexDisplay
            // 
            this.AutoScaleDimensions = new SizeF(10F, 25F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "HexDisplay";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "Hex Dance";
            this.TopMost = true;
            this.Paint += this.Form_Paint;
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer updateTimer;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem closeToolStripMenuItem;
    }
}
