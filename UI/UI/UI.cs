// 文件名：UI.cs
// 
// 文件功能阐述：
// 1. 实现文件的P2P网络共享
// 2. 查找下载文件或者教学数据
// 
// 最后编写日期：2009年9月29日

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NET;
using System.Net;

namespace UI
{
    public partial class UI : Form
    {
        IP2PNode IP2P;
        public UI()
        {
            InitializeComponent();
            this.comboBox1.Items.Add(new ComboBoxItem("精确匹配（单关键字）", "Precise"));
            this.comboBox1.Items.Add(new ComboBoxItem("模糊匹配（单关键字）", "fuzzy"));
            //this.comboBox1.Items.Add(new ComboBoxItem("多关键字匹配", "multiple-key"));
            this.comboBox1.SelectedIndex = 0;
            IP2P = new EasyNode(show);
            CreateMyListView();

        }
        private void show(NET.Message mes)
        {
            SetText(mes);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.InitialDirectory = @"C:\";
            dlgOpenFile.ShowReadOnly = false;
            dlgOpenFile.ReadOnlyChecked = true;
            dlgOpenFile.Filter = "所有文件 (*.*)|*.*";
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                //if (dlgOpenFile.ReadOnlyChecked == true)
                {
                    this.textBox1.Text = dlgOpenFile.FileName.ToString();
                    this.textBox2.Text = System.IO.Path.GetFileName(dlgOpenFile.FileName);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text == null||this.textBox1.Text.Equals(""))
            {
                MessageBox.Show("请先选择文件");
            }
            else
            {
                IPAddress[] ipa = Dns.GetHostAddresses(Dns.GetHostName());
                string ip = ipa[ipa.Length - 1].ToString();
                System.IO.FileInfo file = new System.IO.FileInfo(this.textBox1.Text);
                long len = file.Length;
                FileInfo fi = new FileInfo(this.textBox2.Text, ip, this.textBox1.Text);
                fi.Length = len;
                fi.introduction = this.textBox6.Text;
                fi.keyList.Add(this.textBox2.Text);
                if (this.textBox3.Text != null && !this.textBox3.Text.Equals(""))
                    fi.keyList.Add(this.textBox3.Text);
                if (this.textBox4.Text != null && !this.textBox4.Text.Equals(""))
                    fi.keyList.Add(this.textBox4.Text);
                if (this.textBox5.Text != null && !this.textBox5.Text.Equals(""))
                    fi.keyList.Add(this.textBox5.Text);
                IP2P.Upload(fi);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.comboBox1.SelectedText.Equals("多关键字匹配"))
            {
                string[] str = this.comboBox1.Text.Split(' ');
                string type = ((ComboBoxItem)this.comboBox1.SelectedItem).mvalue;
                IP2P.Search(str, type);
            }
            else
            {
                string type = ((ComboBoxItem)this.comboBox1.SelectedItem).mvalue;
                IP2P.Search(this.textBox7.Text, type);
            }
        }

        private void UI_FormClosing(object sender, FormClosingEventArgs e)
        {
            IP2P.exit();
        }

        public delegate void MyInvoke(NET.Message mes);
        private void SetText(NET.Message mes)
        {
            if (this.listView1.InvokeRequired)
            {
                MyInvoke _myInvoke = new MyInvoke(SetText);
                this.Invoke(_myInvoke, new object[] { mes });
            }
            else
            {
                NET.Message m = mes as NET.Message;
                ListViewItem[] lvi = new ListViewItem[m.Result.Count];
                FileInfo fi;
                for(int i=0;i<m.Result.Count;i++)
                {
                    fi = m.Result[i];
                    lvi[i] = new ListViewItem(new string[] { fi.fileName, fi.IP, fi.Path, fi.introduction });
                }
                this.listView1.Items.AddRange(lvi);
                //foreach (FileInfo fi in m.Result)
                //{
                //    this.listBox1.Items.Add(new ListBoxItem(fi.ID, fi.fileName, fi.IP,fi.Path, fi.introduction));
                //}
            }

        }

        private void UI_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems != null)
            {
                string path;
                string filename = this.listView1.SelectedItems[0].SubItems[0].Text;
                string ip = this.listView1.SelectedItems[0].SubItems[1].Text;
                string mp = this.listView1.SelectedItems[0].SubItems[2].Text;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.CheckFileExists = false;
                sfd.FileName = filename;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    path = System.IO.Path.GetFileName(sfd.FileName);
                    FileInfo fi = new FileInfo(filename, ip, mp);
                    IP2P.Download(fi, path);
                }
            }
        }
        private void CreateMyListView()
        {
            listView1.GridLines = true;//显示行与行之间的分隔线     
            listView1.FullRowSelect = true;//要选择就是一行     
            listView1.View = View.Details;//定义列表显示的方式    
            listView1.Scrollable = true;//需要时候显示滚动条    
            listView1.MultiSelect = false; // 不可以多行选择     
            listView1.HeaderStyle = ColumnHeaderStyle.Clickable;
            listView1.Visible = true;//lstView可见    
            this.listView1.Columns.Add("文件名", 80);
            this.listView1.Columns.Add("目标地址IP", 90);
            this.listView1.Columns.Add("目标地址文件路径", 260);
            this.listView1.Columns.Add("文件说明", 360);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            CreateMyListView();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            button4_Click(sender, e);
        }

    }
}
