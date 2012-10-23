// 文件名：StudentList.cs
// 
// 文件功能阐述：
// 1. 课堂上实时监控学生提交的答案
// 2. 支持手机上预览学生答案
// 3. 可以将学生的答案插入到幻灯片上面，而不用退出幻灯片的播放
// 
// 最后编写日期：2009年9月29日

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Microsoft.WindowsCE.Forms;

namespace TeacherClient
{
    public partial class StudentList : Form
    {
        public StudentList()
        {
            InitializeComponent();
            labelofStudentNum.Text = "上交作业人数：0";
        }

        #region 菜单操作
        // 切换回主菜单的按钮
        private void menuItem1_Click(object sender, EventArgs e)
        {   
            this.Close();
            SystemSettings.ScreenOrientation = ScreenOrientation.Angle0;
        }

        // 刷新按钮处理函数
        private void Refresh_Click_1(object sender, EventArgs e)
        {
            Teacher.pTeacher.SendData("refresh");
            Cursor.Current = Cursors.WaitCursor;
            listBoxStudent.Items.Clear();
            if (this.pictureBoxPreview.Image != null)
            {
                this.pictureBoxPreview.Image = null;
            }
            
            foreach (string s in Teacher.pTeacher.studentFileNameList)
            {
                listBoxStudent.Items.Add(s);
            }
            Cursor.Current = Cursors.Default;
            int numOfHomework = Teacher.pTeacher.studentFileNameList.Count;
            this.labelofStudentNum.Text = "上交作业人数：" + numOfHomework.ToString();
            this.listBoxStudent.Refresh();
        }


        #endregion

        #region 预览图片

        // 预览按钮函数
        private void PreView_Click(object sender, EventArgs e)
        {
            pictureBoxPreview.Visible = true;

            if (pictureBoxPreview.Image != null)
            {
                pictureBoxPreview.Image.Dispose();
                pictureBoxPreview.Image = null;
            }
            try
            {
                string selectedItem = listBoxStudent.SelectedItem.ToString();
                Teacher.pTeacher.SendData("get" + "//" + selectedItem + ".jpg" + "//"); 
                do
                {
                    if (Teacher.pTeacher.SIGN_FIlE_RECEIVED)
                    {
                        picBoxUpDraw(selectedItem);
                        Teacher.pTeacher.SIGN_FIlE_RECEIVED = false;
                        break;
                    }
                }
                while (true);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("请在在左边列表选择学生进行预览");
            }
        }

        /// <summary>
        /// 重绘picBoxUpDraw函数
        /// </summary>
        /// <param name="name"></传进选择预览的名字>
        private void picBoxUpDraw(string name)
        {
            try
            {
                string Apppath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;

                string pathstr = new FileInfo(Apppath).DirectoryName;

                Bitmap bm = new Bitmap(pathstr +@"\" + name +@".jpg");
                Image myImage = Image.FromHbitmap(bm.GetHbitmap());
                this.pictureBoxPreview.Image = myImage;
            }

            catch (Exception)
            {

            }

        }
        #endregion

        // 插入一页
        private void menuItemShow_Click(object sender, EventArgs e)
        {
            /// 选择要显示的图片 控制PC
            Teacher.pTeacher.SendData("InsertAPage//" + listBoxStudent.SelectedItem.ToString() + ".jpg" + "//");
            menuItem1_Click(sender, e);

        }

        private void StudentList_Load(object sender, EventArgs e)
        {
            SystemSettings.ScreenOrientation = ScreenOrientation.Angle90;
        }
    }
}