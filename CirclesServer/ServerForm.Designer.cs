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
            this.lb_Amount = new System.Windows.Forms.Label();
            this.tB_CircleAmount = new System.Windows.Forms.TextBox();
            this.lb_averageRad = new System.Windows.Forms.Label();
            this.tB_averageRad = new System.Windows.Forms.TextBox();
            this.bt_Load = new System.Windows.Forms.Button();
            this.bt_Save = new System.Windows.Forms.Button();
            this.fD_loadConfig = new System.Windows.Forms.OpenFileDialog();
            this.fD_saveConfig = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // bt_StartServer
            // 
            this.bt_StartServer.Location = new System.Drawing.Point(136, 144);
            this.bt_StartServer.Name = "bt_StartServer";
            this.bt_StartServer.Size = new System.Drawing.Size(227, 50);
            this.bt_StartServer.TabIndex = 0;
            this.bt_StartServer.Text = "Start Server";
            this.bt_StartServer.UseVisualStyleBackColor = true;
            this.bt_StartServer.Click += new System.EventHandler(this.StartServer_Click);
            // 
            // lb_Amount
            // 
            this.lb_Amount.AutoSize = true;
            this.lb_Amount.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.lb_Amount.Location = new System.Drawing.Point(24, 49);
            this.lb_Amount.Name = "lb_Amount";
            this.lb_Amount.Size = new System.Drawing.Size(91, 25);
            this.lb_Amount.TabIndex = 1;
            this.lb_Amount.Text = "Amount:";
            // 
            // tB_CircleAmount
            // 
            this.tB_CircleAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.tB_CircleAmount.Location = new System.Drawing.Point(239, 48);
            this.tB_CircleAmount.Name = "tB_CircleAmount";
            this.tB_CircleAmount.ReadOnly = true;
            this.tB_CircleAmount.Size = new System.Drawing.Size(182, 27);
            this.tB_CircleAmount.TabIndex = 2;
            this.tB_CircleAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lb_averageRad
            // 
            this.lb_averageRad.AutoSize = true;
            this.lb_averageRad.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.lb_averageRad.Location = new System.Drawing.Point(24, 82);
            this.lb_averageRad.Name = "lb_averageRad";
            this.lb_averageRad.Size = new System.Drawing.Size(163, 25);
            this.lb_averageRad.TabIndex = 3;
            this.lb_averageRad.Text = "Average radius:";
            // 
            // tB_averageRad
            // 
            this.tB_averageRad.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F);
            this.tB_averageRad.Location = new System.Drawing.Point(239, 81);
            this.tB_averageRad.Name = "tB_averageRad";
            this.tB_averageRad.ReadOnly = true;
            this.tB_averageRad.Size = new System.Drawing.Size(182, 27);
            this.tB_averageRad.TabIndex = 4;
            this.tB_averageRad.Text = " ";
            this.tB_averageRad.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // bt_Load
            // 
            this.bt_Load.Location = new System.Drawing.Point(40, 210);
            this.bt_Load.Name = "bt_Load";
            this.bt_Load.Size = new System.Drawing.Size(150, 25);
            this.bt_Load.TabIndex = 5;
            this.bt_Load.Text = "Load...";
            this.bt_Load.UseVisualStyleBackColor = true;
            this.bt_Load.Click += new System.EventHandler(this.bt_Load_Click);
            // 
            // bt_Save
            // 
            this.bt_Save.Enabled = false;
            this.bt_Save.Location = new System.Drawing.Point(289, 210);
            this.bt_Save.Name = "bt_Save";
            this.bt_Save.Size = new System.Drawing.Size(150, 25);
            this.bt_Save.TabIndex = 6;
            this.bt_Save.Text = "Save...";
            this.bt_Save.UseVisualStyleBackColor = true;
            this.bt_Save.Click += new System.EventHandler(this.bt_Save_Click);
            // 
            // fD_loadConfig
            // 
            this.fD_loadConfig.DefaultExt = "bin";
            this.fD_loadConfig.FileName = "CircleConfig";
            this.fD_loadConfig.Filter = "Binary Files|*.bin|All files|*.*";
            this.fD_loadConfig.FileOk += new System.ComponentModel.CancelEventHandler(this.fD_loadConfig_FileOk);
            // 
            // fD_saveConfig
            // 
            this.fD_saveConfig.DefaultExt = "bin";
            this.fD_saveConfig.Filter = "Binary Files|*.bin|All files|*.*";
            this.fD_saveConfig.FileOk += new System.ComponentModel.CancelEventHandler(this.fD_saveConfig_FileOk);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 257);
            this.Controls.Add(this.bt_Save);
            this.Controls.Add(this.bt_Load);
            this.Controls.Add(this.tB_averageRad);
            this.Controls.Add(this.lb_averageRad);
            this.Controls.Add(this.tB_CircleAmount);
            this.Controls.Add(this.lb_Amount);
            this.Controls.Add(this.bt_StartServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ServerForm";
            this.Text = "Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_StartServer;
        private System.Windows.Forms.TextBox tB_CircleAmount;
        private System.Windows.Forms.Label lb_Amount;
        private System.Windows.Forms.Label lb_averageRad;
        private System.Windows.Forms.TextBox tB_averageRad;
        private System.Windows.Forms.Button bt_Load;
        private System.Windows.Forms.Button bt_Save;
        private System.Windows.Forms.OpenFileDialog fD_loadConfig;
        private System.Windows.Forms.SaveFileDialog fD_saveConfig;
    }
}

