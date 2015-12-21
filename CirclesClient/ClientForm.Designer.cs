namespace CirclesClient
{
    partial class ClientForm
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
            this.pn_DrawingPanel = new System.Windows.Forms.Panel();
            this.bt_Draw = new System.Windows.Forms.Button();
            this.bt_Move = new System.Windows.Forms.Button();
            this.bt_Delete = new System.Windows.Forms.Button();
            this.lb_ModeDisplay = new System.Windows.Forms.Label();
            this.colorDialog_drawColor = new System.Windows.Forms.ColorDialog();
            this.pn_ColorSelectPanel = new System.Windows.Forms.Panel();
            this.bt_Connect = new System.Windows.Forms.Button();
            this.bt_Disconnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pn_DrawingPanel
            // 
            this.pn_DrawingPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.pn_DrawingPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pn_DrawingPanel.Location = new System.Drawing.Point(150, 100);
            this.pn_DrawingPanel.Name = "pn_DrawingPanel";
            this.pn_DrawingPanel.Size = new System.Drawing.Size(697, 518);
            this.pn_DrawingPanel.TabIndex = 0;
            this.pn_DrawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.pn_DrawingPanel_Paint);
            this.pn_DrawingPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pn_DrawingPanel_MouseDown);
            this.pn_DrawingPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pn_DrawingPanel_MouseMove);
            this.pn_DrawingPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pn_DrawingPanel_MouseUp);
            // 
            // bt_Draw
            // 
            this.bt_Draw.Enabled = false;
            this.bt_Draw.Location = new System.Drawing.Point(27, 170);
            this.bt_Draw.Name = "bt_Draw";
            this.bt_Draw.Size = new System.Drawing.Size(100, 30);
            this.bt_Draw.TabIndex = 1;
            this.bt_Draw.Text = "Draw";
            this.bt_Draw.UseVisualStyleBackColor = true;
            this.bt_Draw.Click += new System.EventHandler(this.bt_Draw_Click);
            // 
            // bt_Move
            // 
            this.bt_Move.Enabled = false;
            this.bt_Move.Location = new System.Drawing.Point(27, 270);
            this.bt_Move.Name = "bt_Move";
            this.bt_Move.Size = new System.Drawing.Size(100, 30);
            this.bt_Move.TabIndex = 2;
            this.bt_Move.Text = "Move";
            this.bt_Move.UseVisualStyleBackColor = true;
            this.bt_Move.Click += new System.EventHandler(this.bt_Move_Click);
            // 
            // bt_Delete
            // 
            this.bt_Delete.Enabled = false;
            this.bt_Delete.Location = new System.Drawing.Point(27, 370);
            this.bt_Delete.Name = "bt_Delete";
            this.bt_Delete.Size = new System.Drawing.Size(100, 30);
            this.bt_Delete.TabIndex = 3;
            this.bt_Delete.Text = "Delete";
            this.bt_Delete.UseVisualStyleBackColor = true;
            this.bt_Delete.Click += new System.EventHandler(this.bt_Delete_Click);
            // 
            // lb_ModeDisplay
            // 
            this.lb_ModeDisplay.AutoSize = true;
            this.lb_ModeDisplay.Font = new System.Drawing.Font("Calibri", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_ModeDisplay.Location = new System.Drawing.Point(365, 21);
            this.lb_ModeDisplay.Name = "lb_ModeDisplay";
            this.lb_ModeDisplay.Size = new System.Drawing.Size(254, 59);
            this.lb_ModeDisplay.TabIndex = 4;
            this.lb_ModeDisplay.Text = "Draw Mode";
            this.lb_ModeDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // colorDialog_drawColor
            // 
            this.colorDialog_drawColor.AnyColor = true;
            this.colorDialog_drawColor.FullOpen = true;
            // 
            // pn_ColorSelectPanel
            // 
            this.pn_ColorSelectPanel.BackColor = System.Drawing.Color.Red;
            this.pn_ColorSelectPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pn_ColorSelectPanel.Location = new System.Drawing.Point(27, 489);
            this.pn_ColorSelectPanel.Name = "pn_ColorSelectPanel";
            this.pn_ColorSelectPanel.Size = new System.Drawing.Size(100, 97);
            this.pn_ColorSelectPanel.TabIndex = 5;
            this.pn_ColorSelectPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pn_ColorSelectPanel_MouseDown);
            // 
            // bt_Connect
            // 
            this.bt_Connect.Location = new System.Drawing.Point(27, 52);
            this.bt_Connect.Name = "bt_Connect";
            this.bt_Connect.Size = new System.Drawing.Size(100, 28);
            this.bt_Connect.TabIndex = 6;
            this.bt_Connect.Text = "Connect";
            this.bt_Connect.UseVisualStyleBackColor = true;
            this.bt_Connect.Click += new System.EventHandler(this.bt_Connect_Click);
            // 
            // bt_Disconnect
            // 
            this.bt_Disconnect.Enabled = false;
            this.bt_Disconnect.Location = new System.Drawing.Point(27, 100);
            this.bt_Disconnect.Name = "bt_Disconnect";
            this.bt_Disconnect.Size = new System.Drawing.Size(100, 28);
            this.bt_Disconnect.TabIndex = 7;
            this.bt_Disconnect.Text = "Disconnect";
            this.bt_Disconnect.UseVisualStyleBackColor = true;
            this.bt_Disconnect.Click += new System.EventHandler(this.bt_Disconnect_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 661);
            this.Controls.Add(this.bt_Disconnect);
            this.Controls.Add(this.bt_Connect);
            this.Controls.Add(this.pn_ColorSelectPanel);
            this.Controls.Add(this.lb_ModeDisplay);
            this.Controls.Add(this.bt_Delete);
            this.Controls.Add(this.bt_Move);
            this.Controls.Add(this.bt_Draw);
            this.Controls.Add(this.pn_DrawingPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ClientForm";
            this.Text = "Client";
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pn_DrawingPanel;
        private System.Windows.Forms.Button bt_Draw;
        private System.Windows.Forms.Button bt_Move;
        private System.Windows.Forms.Button bt_Delete;
        private System.Windows.Forms.Label lb_ModeDisplay;
        private System.Windows.Forms.ColorDialog colorDialog_drawColor;
        private System.Windows.Forms.Panel pn_ColorSelectPanel;
        private System.Windows.Forms.Button bt_Connect;
        private System.Windows.Forms.Button bt_Disconnect;
    }
}

