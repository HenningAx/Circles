namespace CirclesServer
{
    partial class ServerForm
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
            this.bt_StartServer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bt_StartServer
            // 
            this.bt_StartServer.Location = new System.Drawing.Point(133, 170);
            this.bt_StartServer.Name = "bt_StartServer";
            this.bt_StartServer.Size = new System.Drawing.Size(227, 50);
            this.bt_StartServer.TabIndex = 0;
            this.bt_StartServer.Text = "Start Server";
            this.bt_StartServer.UseVisualStyleBackColor = true;
            this.bt_StartServer.Click += new System.EventHandler(this.StartServer_Click);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 254);
            this.Controls.Add(this.bt_StartServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ServerForm";
            this.Text = "Server";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bt_StartServer;
    }
}

