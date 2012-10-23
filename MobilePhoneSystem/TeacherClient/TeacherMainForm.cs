// 文件名：TeacherMainForm.cs
// 
// 文件功能阐述：
// 1. 远程对幻灯片的操作
// 2. 对电脑端的幻灯片实时地标注
// 3. 收集课堂学生的数据
// 4. 对幻灯片插入学生答案Slide并播放
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
using InTheHand.Net;
using InTheHand.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Microsoft.WindowsCE.Forms;


namespace TeacherClient
{
    public partial class Teacher : Form
    {

        public static Teacher pTeacher = null;
        public Teacher()
        {
            InitializeComponent();
            StateOfBegin();
            pTeacher = this;

        }
        
        private List<Point> m_newLine;  // 用来存储标注时产生的点

        /// <summary>
        /// 往m_newLine里面插入点
        /// </summary>
        /// <param name="x"></点的X坐标>
        /// <param name="y"></点的Y坐标>
        private void m_addPoint(int x, int y)
        {
            //将经过的点添加到线条

            m_newLine.Add(new Point(x, y));

            //绘制线段，连接当前线条的最后一点和新经过的这一点

            int points = m_newLine.Count;
            if (points > 1)
            {

                Graphics g = this.pictureBox1.CreateGraphics();
                Pen p = new Pen(Color.Red);
                g.DrawLine(
                    p,
                    m_newLine[points - 2].X, m_newLine[points - 2].Y,
                    m_newLine[points - 1].X, m_newLine[points - 1].Y
                    );
                g.Dispose();
                p.Dispose();
            }
        }

        /// <summary>
        /// 更新picBoxUpDraw上面的图像，与电脑端幻灯片同步
        /// </summary>
        private void picBoxUpDraw()
        {
            try
            {
                string Apppath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;

                string pathstr = new FileInfo(Apppath).DirectoryName;

                Bitmap bm = new Bitmap(pathstr + @"\1.jpg");
                Image myImage = Image.FromHbitmap(bm.GetHbitmap());

                this.pictureBox1.Image = myImage;


            }

            catch (Exception)
            {

            }

        }

        /// <summary>
        /// 获取当前目录
        /// </summary>
        /// <returns></返回当前程序运行的目录>
        static private string getFilePath()
        {
            string Apppath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            string pathstr = new FileInfo(Apppath).DirectoryName;
            return pathstr;
        }

        /// <summary>
        /// 将m_newLine里面的点转成字符串的形式存储，以便通过来蓝牙发送
        /// </summary>
        /// <returns></returns>
        private string TransferList2Str()
        {
            string strOut = "";
            foreach (Point p in m_newLine)
            {
                strOut += p.X;
                strOut += ',';
                strOut += p.Y;
                strOut += ';';
            }

            return strOut;
        }

        #region Bluetooth
        // 蓝牙参数的设定
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
        private FileStream receivefilestream;
        private System.IO.FileStream sendfilestream;

        // 发送和接收信息的参数
        static string pathstr = getFilePath();
        private string DATATOSEND = "";
        private string RECEIVEDATA = "";
        private string RECEIVEFILENAME;
        private string RECEIVEFILEADDRESS;

        private string SENDFILENAME;
        private string SENDFILEADDRESS;

        // 定义线程
        private System.Threading.Thread ReceiveThread;  // 接收线程
        private System.Threading.Thread sendDataThread;  // 发送信息线程
        private System.Threading.Thread sendfileThread;  // 发送文件线程


        public List<string> studentFileNameList = new List<string>();

        /// <summary>
        /// 检查手机端的蓝牙是否已经打开
        /// </summary>
        /// <returns></蓝牙没有打开则返回false>
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
            PrePage.Enabled = false;
            NextPage.Enabled = false;
            menuItem2.Enabled = false;
            menuItem3.Enabled = false;
        }

        private void StateOfRunning()
        {
            PrePage.Enabled = true;
            NextPage.Enabled = true;
            menuItem2.Enabled = true;
            menuItem3.Enabled = true;
            menuItemLogin.Enabled = false;
        }

        /// <summary>
        /// 连接函数
        /// </summary>
        /// <returns></连接失败返回false>
        private bool connect()
        {
            // 创建选择框选择要连接的设备
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
                        // 对指定的目标进行蓝牙连接请求
                        client = new BluetoothClient();
                        client.Connect(selecteddevice.DeviceAddress, BluetoothService.SerialPort);
                        stream = client.GetStream();
                        stream.Write(System.Text.Encoding.ASCII.GetBytes("teacher"), 0, 7);
                    }

                    catch
                    {
                        return false;
                    }

                    StateOfRunning();

                    stream = client.GetStream();
                    // 标记stream已经接受对象实例化
                    SIGN_STREAMSETUP = true;
                    // 启动接收图片的线程
                    receiving = true;
                    ReceiveThread = new System.Threading.Thread(ReceiveLoop);
                    ReceiveThread.Start();
                    // 启动信息发送线程
                    sendDataThread = new System.Threading.Thread(SendDataLoop);
                    closeSendData();
                    sendDataThread.Start();

                    sendfileThread = new System.Threading.Thread(SendFileLoop);
                    return true;
                }
                return false;
            }

        }

        /// <summary>
        /// 发送文件线程函数
        /// </summary>
        private void SendFileLoop()
        {
            while (SIGN_STREAMSETUP)
            {
                if (!SUSPEND_OF_SENDFILETHREAD)
                {
                    sendfilestream = new FileStream(SENDFILEADDRESS, FileMode.Open, FileAccess.Read, FileShare.None);
                    byte[] buffer = new byte[sendfilestream.Length];

                    // metaOfFile 以“//”标记符来标记数据。
                    // metaOfFile
                    string metaOfFile = "picture" + "//" + SENDFILENAME + "//" +
                    sendfilestream.Length.ToString();
                    byte[] byte_metaOfFile = System.Text.Encoding.ASCII.GetBytes(metaOfFile);
                    // 往流里面写入数据
                    stream.Write(byte_metaOfFile, 0, byte_metaOfFile.Length);

                    while (true)
                    {
                        // 利用SUSPEND_OF_SENDFILETHREAD使手机端线程能够挂起
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

        /// <summary>
        /// 发送文件函数
        /// </summary>
        /// <param name="filename"></传入要发送文件的名称>
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
                SUSPEND_OF_SENDFILETHREAD = true;
            }
        }


        /// <summary>
        /// 改变标记参数，使发送文件进程挂起
        /// </summary>
        private void closeSendFile()
        {
            SUSPEND_OF_SENDFILETHREAD = true;
        }

        /// <summary>
        /// 发送信息线程函数
        /// </summary>
        private void SendDataLoop()
        {
            while (SIGN_STREAMSETUP)
            {
                if (!SUSPEND_OF_SENDDATATHREAD)
                {
                    byte[] commandbuffer = System.Text.Encoding.ASCII.GetBytes("command");
                    stream.Write(commandbuffer, 0, commandbuffer.Length);

                    byte[] buffer = System.Text.Encoding.Default.GetBytes(DATATOSEND);
                    if (buffer.Length == 0)
                        closeSendData();
                    try
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.ToString());
                        break;
                    }

                    closeSendData();
                }
                else
                {
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// 启动发送信息的函数
        /// </summary>
        /// <param name="msg"></传进要发送的信息>
        public void SendData(string msg)
        {
            DATATOSEND = msg;
            SUSPEND_OF_SENDDATATHREAD = false;
        }

        /// <summary>
        /// 改变标记参数，发送信息线程挂起
        /// </summary>
        private void closeSendData()
        {
            SUSPEND_OF_SENDDATATHREAD = true;
        }

        /// <summary>
        /// 接收信息、文件线程函数体
        /// </summary>
        private void ReceiveLoop()
        {
            byte[] buffer;
            byte[] mark;
            while (receiving)
            {
                if (stream.CanRead)
                {
                    try
                    {
                        int streamLength;
                        int fullLength = 0;
                        mark = new byte[100];

                        // 接收Command来区别指令
                        int iii = 0;
                        iii = stream.Read(mark, 0, mark.Length);
                        if (iii == 0) continue;

                        string receiveCommand = System.Text.Encoding.ASCII.GetString(mark, 0, iii);
                        if (receiveCommand == "command")
                        {
                            RECEIVEDATA = "";
                            do
                            {
                                buffer = new byte[1024 * 2];
                                streamLength = stream.Read(buffer, 0, buffer.Length);
                                RECEIVEDATA += System.Text.Encoding.Default.GetString(buffer, 0, streamLength);
                            } while (streamLength == 2048);

                            // 如果设备离开范围或者断开，则退出
                            if (streamLength == 0)
                            {
                                ReceiveThread.Abort();
                                break;
                            }
                            if (RECEIVEDATA.StartsWith("file"))
                            {
                                FiletoList(RECEIVEDATA);
                            }
                            stream.Flush();
                        }

                        else if (receiveCommand.StartsWith("picture"))
                        {
                            fullLength = getNumFromCommand(receiveCommand);
                            if (fullLength == -1) continue;

                            RECEIVEFILENAME = getFileNameFromCommand(receiveCommand);
                            SetReceiveFileAddress(RECEIVEFILENAME);
                            if (System.IO.File.Exists(RECEIVEFILEADDRESS))
                                System.IO.File.Delete(RECEIVEFILEADDRESS);

                            FileInfo fileinfo = new FileInfo(RECEIVEFILEADDRESS);
                            receivefilestream = fileinfo.OpenWrite();
                            BinaryWriter bw = new BinaryWriter(receivefilestream);
                            int recieveLength = 0;

                            do
                            {
                                buffer = new byte[1024 * 100];
                                streamLength = stream.Read(buffer, 0, buffer.Length);
                                recieveLength += streamLength;
                                stream.Flush();
                                bw.Write(buffer, 0, streamLength);
                                bw.Flush();
                                if (recieveLength > (fullLength - 50))
                                    break;
                            } while (streamLength > 0);
#if DEBUG
                                MessageBox.Show("源文件的大小" + fullLength.ToString() + '\n' +
                                                "实际文件的大小" + recieveLength.ToString());
#endif
                            receivefilestream.Close();
                            SIGN_FIlE_RECEIVED = true;
                            bw.Close();
                        }
                        else continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 设置接收文件的地址
        /// </summary>
        /// <param name="filename"></传进文件的名称>
        private void SetReceiveFileAddress(string filename)
        {
            RECEIVEFILEADDRESS = @pathstr + @"\" + @filename;
        }

        /// <summary>
        /// 从接受的命令中提取要接受文件的大小
        /// </summary>
        /// <param name="aaa"></文件名称>
        /// <returns></文件的大小>
        private int getNumFromCommand(string aaa)
        {
            string s = "//";
            int value = -1;
            int count = 0;
            for (int i = 0; i < aaa.Length - 1; i++)
            {
                if (aaa.Substring(i, 2) == s)
                    ++count;
                if (count == 2)
                {
                    value = int.Parse(aaa.Substring(i + 2));
                    break;
                }
            }
            return value;
        }

        /// <summary>
        /// 从接受的命令中提取要接受文件的名称
        /// </summary>
        /// <param name="aaa"></param>
        /// <returns></returns>
        private string getFileNameFromCommand(string aaa)
        {
            string s = "//";
            int count = 0;
            int start = 0;
            int end = 0;
            for (int i = 0; i < aaa.Length - 1; i++)
            {
                if (aaa.Substring(i, 2) == s)
                {
                    ++count;
                    if (count == 1)
                    {
                        start = i + 2;
                    }

                    if (count == 2)
                    {
                        end = i;
                        break;
                    }
                }

            }

            return aaa.Substring(start, end - start);
        }

        /// <summary>
        /// 讲所有文件名保存起来
        /// </summary>
        /// <param name="sss"></param>
        private void FiletoList(string sss)
        {
            string s = "//";
            int start = 0;
            int end = 0;
            int mark = 1;
            int count = 0;
            for (int i = 0; i < sss.Length - 1; i++)
            {
                if (sss.Substring(i, 2) == "//")
                {
                    ++count;

                    if (count == mark)
                    {
                        start = i + 2;
                    }
                    if (count == mark + 1)
                    {
                        end = i;
                        bool mark_of_repeat = false;
                        foreach (string ss in studentFileNameList)
                        {
                            if (ss == sss.Substring(start, end - start - 4))
                                mark_of_repeat = true;
                        }
                        if (!mark_of_repeat)
                            studentFileNameList.Add(sss.Substring(start, end - start - 4));
                        ++mark;
                        start = end + 2;
                    }
                }
            }
        }
        /// <summary>
        /// 另一个版本
        /// </summary>
        /// <param name="str"></param>

        #endregion


        #region 菜单操作
        // 登陆选项操作
        private void menuItemLogin_Click(object sender, EventArgs e)
        {
            if (!checkBluetooth())
                MessageBox.Show("请打开您的手机蓝牙！");
            if (!connect())
                MessageBox.Show("请检查PC端服务是否开启");

        }

        // 放映PPT选项操作
        private void menuItem2_Click(object sender, EventArgs e)
        {
            //  m_newLine = new List<Point>();
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            SendData("Start");
            do
            {
                if (SIGN_FIlE_RECEIVED)
                {

                    picBoxUpDraw();
                    SIGN_FIlE_RECEIVED = false;
                    break;
                }

            }
            while (true);
        }

        // 上一张按钮操作
        private void PrePage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            SendData("PrePage");
            do
            {
                if (SIGN_FIlE_RECEIVED)
                {

                    picBoxUpDraw();
                    SIGN_FIlE_RECEIVED = false;
                    break;
                }

            }
            while (true);
        }

        // 下一张按钮操作
        private void NextPage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            SendData("NextPage");

            do
            {
                if (SIGN_FIlE_RECEIVED)
                {
                    picBoxUpDraw();
                    SIGN_FIlE_RECEIVED = false;
                    break;
                }
            }
            while (true);

        }

        // 却换到预览、选择学生提交答案的窗体
        private void menuItem3_Click(object sender, EventArgs e)
        {
            StudentList studentlist = new StudentList();
            studentlist.Show();
        }

        // 退出事件
        private void menuItemEx_Click(object sender, EventArgs e)
        {
            ReceiveThread.Abort();
            sendDataThread.Abort();
        }

        private void Teacher_Closing(object sender, CancelEventArgs e)
        {
            SystemSettings.ScreenOrientation = ScreenOrientation.Angle0;
        }

        private void menuItemOp_Click(object sender, EventArgs e)
        {
            menuItemEx_Click(sender, e);
            Application.Exit();
        }

        #endregion

        #region 鼠标操作

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            m_newLine = new List<Point>();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            m_addPoint(e.X, e.Y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            m_addPoint(e.X, e.Y);

            // 发送画线的点集字符串
            SendData(TransferList2Str());
        }


        #endregion
        
    }
}


