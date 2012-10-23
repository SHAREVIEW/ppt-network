// 文件名：StudentCarrdForm.cs
// 
// 文件功能阐述：
// 1. 记录学生的信息
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

namespace StudentClient
{
    public partial class StudentCardForm : Form
    {
        public StudentCardForm()
        {
            InitializeComponent();
            NameListBox.Text = StudentClient.pstudentclient.LOCALBLUETOOTHNAME;
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            if (NameListBox.Text == null)
                MessageBox.Show("名字不能为空");
            else 
            {
                StudentClient.pstudentclient.LOCALBLUETOOTHNAME = NameListBox.Text;
                menuItem1_Click(sender, e);
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}