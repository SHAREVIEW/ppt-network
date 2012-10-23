namespace StudentClient
{
    partial class StudentClient
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBoxPaper = new System.Windows.Forms.PictureBox();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.ButtonClear = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem1);
            this.mainMenu1.MenuItems.Add(this.menuItem2);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.menuItem4);
            this.menuItem1.MenuItems.Add(this.menuItem7);
            this.menuItem1.MenuItems.Add(this.menuItem5);
            this.menuItem1.MenuItems.Add(this.menuItem6);
            this.menuItem1.Text = "菜单";
            // 
            // menuItem4
            // 
            this.menuItem4.Text = "登录";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Text = "我的卡片";
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Text = "关于";
            // 
            // menuItem6
            // 
            this.menuItem6.Text = "退出";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Text = "提交";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.pictureBoxPaper);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 219);
            // 
            // pictureBoxPaper
            // 
            this.pictureBoxPaper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPaper.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxPaper.Name = "pictureBoxPaper";
            this.pictureBoxPaper.Size = new System.Drawing.Size(240, 219);
            this.pictureBoxPaper.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPaper_MouseMove);
            this.pictureBoxPaper.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPaper_MouseDown);
            this.pictureBoxPaper.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBoxPaper_MouseUp);
            // 
            // ButtonSave
            // 
            this.ButtonSave.Location = new System.Drawing.Point(36, 233);
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.Size = new System.Drawing.Size(71, 22);
            this.ButtonSave.TabIndex = 1;
            this.ButtonSave.Text = "保存图片";
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // ButtonClear
            // 
            this.ButtonClear.Location = new System.Drawing.Point(136, 232);
            this.ButtonClear.Name = "ButtonClear";
            this.ButtonClear.Size = new System.Drawing.Size(72, 22);
            this.ButtonClear.TabIndex = 2;
            this.ButtonClear.Text = "清空笔迹";
            this.ButtonClear.Click += new System.EventHandler(this.ButtonClear_Click);
            // 
            // StudentClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.ButtonClear);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.panel1);
            this.Menu = this.mainMenu1;
            this.Name = "StudentClient";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.StudentClient_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBoxPaper;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.Button ButtonClear;
        private System.Windows.Forms.MenuItem menuItem7;
    }
}

