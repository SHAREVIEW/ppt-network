// 文件名：StudentClient.cs
// 
// 文件功能阐述：
// 1. 学生端手机操作界面
// 2. 提供一块白板可以让学生进行作答
// 3. 可以让白板上面的内容发送到电脑端并允许老师访问
// 4. 设置了我的卡片进行个人身份的标记
// 5. 重新获取白板功能
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

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Windows.Forms;

using System.IO;

namespace StudentClient
{
    public partial class StudentClient : Form
    {
        public static StudentClient pstudentclient = null;
        public StudentClient()
        {
            InitializeComponent();
            pstudentclient = this;
            StateOfBegin();
        }

        #region 菜单操作
        // 保存按钮， 可以将图片保存在手机
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            gbmp.DrawImage(bm, 0, 0);
            
            bm.Save(pathstr + @"\" + LOCALBLUETOOTHNAME + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
           // bm.Dispose();
            Graphics.FromImage(bm).Clear(Color.White);
            gbmp.Dispose();
        }


        // 提交图片
        private void menuItem2_Click(object sender, EventArgs e)
        {
            ButtonSave_Click(sender, e);
            SendFile(LOCALBLUETOOTHNAME + ".jpg");
        }

        // 获取新的白板
        private void ButtonClear_Click(object sender, EventArgs e)
        {

            this.pictureBoxPaper.Image = null;
            //  gbmp.Dispose();
            // pictureBoxPaper.Refresh();
            // pictureBoxPaper.Dispose();         

        }

        // 设置学生信息
        private void menuItem7_Click(object sender, EventArgs e)
        {
            StudentCardForm frmStudentCardForm = new StudentCardForm();
            frmStudentCardForm.Show();
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // 登陆电脑端
        private void menuItem4_Click(object sender, EventArgs e)
        {
            if (!checkBluetooth())
                MessageBox.Show("请打开您的手机蓝牙！");
            if (!connect())
                MessageBox.Show("请检查PC端服务是否开启");
        }

        #endregion
        
        #region 画图

        Graphics gbmp;
        Bitmap bm = null;
        private List<Point> m_newLine;
        private void Paper_Load(object sender, EventArgs e)
        {
            int w = this.pictureBoxPaper.Width;
            int h = this.pictureBoxPaper.Height;

            bm = new Bitmap(w, h);
            Graphics.FromImage(bm).Clear(Color.White);//消除底图的黑色   

        }

       

        /// <summary>
        /// 向当前线条中添加一个点，并将其与线条中的最后一个点连接起来
        /// </summary>
        /// <param name="x">新添加的点的横坐标</param>
        /// <param name="e">新添加的点的纵坐标</param>
        private void m_addPoint(int x, int y)
        {
            //将经过的点添加到线条

            m_newLine.Add(new Point(x, y));

            //绘制线段，连接当前线条的最后一点和新经过的这一点

            int points = m_newLine.Count;
            if (points > 1)
            {

                gbmp = Graphics.FromImage(bm);
                Graphics g = this.pictureBoxPaper.CreateGraphics();
                Pen p = new Pen(Color.Red, 2);
                g.DrawLine(
                    p,
                    m_newLine[points - 2].X, m_newLine[points - 2].Y,
                    m_newLine[points - 1].X, m_newLine[points - 1].Y
                    );
                gbmp.DrawLine(
                     p,
                    m_newLine[points - 2].X, m_newLine[points - 2].Y,
                    m_newLine[points - 1].X, m_newLine[points - 1].Y

                    );
                p.Dispose();
                g.Dispose();

            }
        }
        // 窗体登陆的时动作
        private void StudentClient_Load(object sender, EventArgs e)
        {
            int w = this.pictureBoxPaper.Width;
            int h = this.pictureBoxPaper.Height;

            bm = new Bitmap(w, h);
            Graphics.FromImage(bm).Clear(Color.White);//消除底图的黑色   

        }

        // 获取点击事件
        private void pictureBoxPaper_MouseDown(object sender, MouseEventArgs e)
        {
            m_newLine = new List<Point>();
        }

        private void pictureBoxPaper_MouseMove(object sender, MouseEventArgs e)
        {
            m_addPoint(e.X, e.Y);
        }

        private void pictureBoxPaper_MouseUp(object sender, MouseEventArgs e)
        {
            m_addPoint(e.X, e.Y);
        }

        #endregion

        #region 蓝牙

        private BluetoothRadio radio;
        private BluetoothClient client;

        // 用来控制接收函数
        private bool receiving = false;
        // 用来标记Stream是否建立
        private bool SIGN_STREAMSETUP = false;
        // 用来标记是否已经接收完一个文件
        public bool SIGN_FIlE_RECEIVED = false;
        // 用来控制文件的接收 
        private bool SUSPEND_OF_SENDFILETHREAD = false;
        // 用来控制数据的接收 
        private bool SUSPEND_OF_SENDDATATHREAD = false;

        // 流数据的定义
        private System.IO.Stream stream;

        // 文件参数的定义
        private System.IO.FileStream receivefilestream;
        private System.IO.FileStream sendfilestream;

        // 发送和接收信息的参数
        static string Apppath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
        string pathstr = new FileInfo(Apppath).DirectoryName;
        private string DATATOSEND = "";
        private string RECEIVEDATA = "";
        private string RECEIVEFILENAME;
        private string RECEIVEFILEADDRESS;
        public string LOCALBLUETOOTHNAME = "student01";

        private string SENDFILENAME;
        private string SENDFILEADDRESS;

        // 定义线程
        private System.Threading.Thread ReceiveThread;
        private System.Threading.Thread sendDataThread;
        private System.Threading.Thread sendfileThread;

        private bool checkBluetooth()
        {
            radio = BluetoothRadio.PrimaryRadio;
            if (radio == null)
                return false;
            if (radio.Mode == RadioMode.PowerOff)
                return false;
            radio.Mode = RadioMode.Discoverable;
            return true;
        }

        private void StateOfBegin()
        {
            menuItem2.Enabled = false;
        }

        private void StateOfRunning()
        {
            menuItem2.Enabled = true;
        }

        // 连接电脑端蓝牙
        private bool connect()
        {
            using (SelectBluetoothDeviceDialog bldialog = new SelectBluetoothDeviceDialog())
            {
                bldialog.ShowRemembered = false;
                if (bldialog.ShowDialog() == DialogResult.OK)
                {
                    if (bldialog.SelectedDevice == null)
                    {
                        MessageBox.Show("No device selected");
                        return false;
                    }

                    BluetoothDeviceInfo selecteddevice = bldialog.SelectedDevice;

                    if (!selecteddevice.Authenticated)
                    {
                        if (!BluetoothSecurity.PairRequest(selecteddevice.DeviceAddress, "0000"))
                        {
                            MessageBox.Show("PairRequest Error");
                            return false;
                        }
                    }

                    try
                    {
                        client = new BluetoothClient();
                        client.Connect(selecteddevice.DeviceAddress, BluetoothService.SerialPort);
                        stream = client.GetStream();
                        stream.Write(System.Text.Encoding.ASCII.GetBytes("student" + LOCALBLUETOOTHNAME), 0, 7 + LOCALBLUETOOTHNAME.Length);
                    }

                    catch
                    {
                        return false;
                    }

                    StateOfRunning();
                    stream = client.GetStream();
                    // 标记stream已经接受对象实例化
                    SIGN_STREAMSETUP = true;
                    // 启动接收图片的进程
                    receiving = true;
                    // 
                    sendfileThread = new System.Threading.Thread(SendFileLoop);
                    return true;
                }
                return false;
            }

        }

        // 发送文件的线程函数
        private void SendFileLoop()
        {
            while (SIGN_STREAMSETUP)
            {
                if (!SUSPEND_OF_SENDFILETHREAD)
                {
                    sendfilestream = new FileStream(SENDFILEADDRESS, FileMode.Open, FileAccess.Read, FileShare.None);
                    byte[] buffer = new byte[sendfilestream.Length];

                    string metaOfFile = "picture" + "//" + SENDFILENAME + "//" +
                    sendfilestream.Length.ToString();
                    byte[] byte_metaOfFile = System.Text.Encoding.ASCII.GetBytes(metaOfFile);
                    stream.Write(byte_metaOfFile, 0, byte_metaOfFile.Length);

                    while (true)
                    {
                        if (!SUSPEND_OF_SENDFILETHREAD)
                            try
                            {
                                int length = sendfilestream.Read(buffer, 0, buffer.Length);
                                if (length == 0)
                                {
                                    sendfilestream.Close();
                                    sendfilestream.Dispose();

                                    // 发送文件完毕，进程挂起。
                                    closeSendFile();
                                    break;
                                }
                                stream.Write(buffer, 0, length);
                            }
                            catch (IOException ex)
                            {
                                MessageBox.Show(ex.ToString());
                                break;
                            }
                    }

                }
                else
                {
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        // 发送文件函数
        private void SendFile(string filename)
        {
            SENDFILENAME = filename;
            SENDFILEADDRESS = pathstr + @"\" + SENDFILENAME;
            try
            {
                sendfileThread.Start();
            }
            catch
            {
                SUSPEND_OF_SENDFILETHREAD = false;
            }
        }

        // 关系发送文件
        private void closeSendFile()
        {
            SUSPEND_OF_SENDFILETHREAD = true;
        }

        #endregion
      
    }
}