namespace TeacherClient
{
    partial class StudentList
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
            this.menuItemReturn = new System.Windows.Forms.MenuItem();
            this.menuItemShow = new System.Windows.Forms.MenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.listBoxStudent = new System.Windows.Forms.ListBox();
            this.PreView = new System.Windows.Forms.Button();
            this.Refresh = new System.Windows.Forms.Button();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelofStudentNum = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItemReturn);
            this.mainMenu1.MenuItems.Add(this.menuItemShow);
            // 
            // menuItemReturn
            // 
            this.menuItemReturn.Text = "返回";
            this.menuItemReturn.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuItemShow
            // 
            this.menuItemShow.Text = "显示到幻灯片";
            this.menuItemShow.Click += new System.EventHandler(this.menuItemShow_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.labelofStudentNum);
            this.panel1.Controls.Add(this.listBoxStudent);
            this.panel1.Location = new System.Drawing.Point(0, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(107, 182);
            // 
            // listBoxStudent
            // 
            this.listBoxStudent.Location = new System.Drawing.Point(0, 6);
            this.listBoxStudent.Name = "listBoxStudent";
            this.listBoxStudent.Size = new System.Drawing.Size(104, 128);
            this.listBoxStudent.TabIndex = 0;
            // 
            // PreView
            // 
            this.PreView.Location = new System.Drawing.Point(235, 143);
            this.PreView.Name = "PreView";
            this.PreView.Size = new System.Drawing.Size(57, 29);
            this.PreView.TabIndex = 1;
            this.PreView.Text = "预览";
            this.PreView.Click += new System.EventHandler(this.PreView_Click);
            // 
            // Refresh
            // 
            this.Refresh.Location = new System.Drawing.Point(129, 143);
            this.Refresh.Name = "Refresh";
            this.Refresh.Size = new System.Drawing.Size(56, 26);
            this.Refresh.TabIndex = 2;
            this.Refresh.Text = "刷新";
            this.Refresh.Click += new System.EventHandler(this.Refresh_Click_1);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Location = new System.Drawing.Point(13, 10);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(400, 300);
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.pictureBoxPreview);
            this.panel2.Location = new System.Drawing.Point(113, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(204, 134);
            // 
            // labelofStudentNum
            // 
            this.labelofStudentNum.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.labelofStudentNum.Location = new System.Drawing.Point(0, 137);
            this.labelofStudentNum.Name = "labelofStudentNum";
            this.labelofStudentNum.Size = new System.Drawing.Size(100, 20);
            this.labelofStudentNum.Text = "上交作业人数：";
            // 
            // StudentList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(320, 188);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.Refresh);
            this.Controls.Add(this.PreView);
            this.Controls.Add(this.panel1);
            this.Menu = this.mainMenu1;
            this.Name = "StudentList";
            this.Text = "StudentList";
            this.Load += new System.EventHandler(this.StudentList_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuItemReturn;
        private System.Windows.Forms.MenuItem menuItemShow;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button PreView;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.ListBox listBoxStudent;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelofStudentNum;
    }
}