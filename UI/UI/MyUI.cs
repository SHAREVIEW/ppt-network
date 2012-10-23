// 文件名：MyUI.cs
// 
// 文件功能阐述：
// 1. 监听外围蓝牙设备
// 2. 同步电脑端幻灯片与手机端操作
// 3. 界面的美化
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
using System.Net;
using NET;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using InTheHand.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Drawing.Imaging;

using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Graph = Microsoft.Office.Interop.Graph;
using System.Runtime.InteropServices;

namespace UI
{
    public partial class MyUI : Form
    {
        IP2PNode IP2P;
        public MyUI()
        {
            InitializeComponent();
            // 初始化窗体
            this.comboBox1.Items.Add(new ComboBoxItem("精确匹配（单关键字）", "Precise"));
            this.comboBox1.Items.Add(new ComboBoxItem("模糊匹配（单关键字）", "fuzzy"));
            //this.comboBox1.Items.Add(new ComboBoxItem("多关键字匹配", "multiple-key"));
            this.comboBox1.SelectedIndex = 0;
            this.listView1.GridLines = true;  //显示行与行之间的分隔线     
            this.listView1.FullRowSelect = true;  //要选择就是一行     
            this.listView1.View = View.Details;  //定义列表显示的方式    
            this.listView1.Scrollable = true;  //需要时候显示滚动条    
            this.listView1.MultiSelect = false;  // 不可以多行选择     
            this.listView1.HeaderStyle = ColumnHeaderStyle.Clickable;
            this.listView1.Visible = true;  //lstView可见    
            this.listView1.Columns.Add("文件名", 80);
            this.listView1.Columns.Add("目标地址IP", 90);
            this.listView1.Columns.Add("目标地址文件路径", 230);
            this.listView1.Columns.Add("文件说明", 360);
            this.skinEngine1.SkinFile = Application.StartupPath + @"\ssk\MP10.ssk";
            IP2P = new EasyNode(show);

            // 初始化定义蓝牙相关参数
            string ativeDir = @"D:\";
            string newPath = System.IO.Path.Combine(ativeDir, "HomeWork");
            System.IO.Directory.CreateDirectory(newPath);
            // 检查蓝牙设备是否已经打开
            if (!checkBluetooth())
            {
                MessageBox.Show("请打开电脑端蓝牙设备");
            }
            // 启动监听蓝牙设备连接的服务
            Listen();

            // 初始化图片压缩数据
            imageCompression();
        }

        private void show(NET.Message mes)
        {
            SetText(mes);
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
                NET.FileInfo fi;
                for (int i = 0; i < m.Result.Count; i++)
                {
                    fi = m.Result[i];
                    lvi[i] = new ListViewItem(new string[] { fi.fileName, fi.IP, fi.Path, fi.introduction });
                }
                this.listView1.Items.AddRange(lvi);
            }

        }

        // 选取用来共享的文件
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
                    this.button2.Enabled = true;
                }
            }
        }

        // 对选中文件进行共享
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text == null || this.textBox1.Text.Equals(""))
            {
                MessageBox.Show("请先选择文件");
            }
            else
            {
                IPAddress[] ipa = Dns.GetHostAddresses(Dns.GetHostName());
                string ip = ipa[ipa.Length - 1].ToString();
                System.IO.FileInfo file = new System.IO.FileInfo(this.textBox1.Text);
                long len = file.Length;
                NET.FileInfo fi = new NET.FileInfo(this.textBox2.Text, ip, this.textBox1.Text);
                fi.Length = len;
                fi.introduction = this.textBox6.Text;
                fi.keyList.Add(this.textBox2.Text);
                if (this.textBox3.Text != null && !this.textBox3.Text.Equals(""))
                    fi.keyList.Add(this.textBox3.Text);
                if (this.textBox4.Text != null && !this.textBox4.Text.Equals(""))
                    fi.keyList.Add(this.textBox4.Text);
                IP2P.Upload(fi);
                MessageBox.Show("共享成功！");
                this.button2.Enabled = false;
            }
        }

        // 选择进行搜索的方式
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
                IP2P.Search(this.textBox6.Text, type);
            }
        }

        // 对选中的文件进行下载操作
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
                    NET.FileInfo fi = new NET.FileInfo(filename, ip, mp);
                    IP2P.Download(fi, path);
                }
            }
        }

        // 清空listView1的内容
        private void button5_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            button4_Click(sender, e);
        }

        private void MyUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            IP2P.exit();
        }

        #region ///获得学生端图片

        private void buildImage(string str)
        {
            Graphics gbmp = this.CreateGraphics();
        }

        #endregion


        public bool ThumbnailCallback()
        {
            return false;
        }

        /// <summary>
        /// 获取当前屏幕的图像，并且写入到文件里面
        /// </summary>
        private void printSrc()
        {
            int iWidth = Screen.PrimaryScreen.Bounds.Width;
            int iHeight = Screen.PrimaryScreen.Bounds.Height;

            Image img = new Bitmap(iWidth, iHeight);

            Graphics gc = Graphics.FromImage(img);

            gc.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));

            Image.GetThumbnailImageAbort myCallback =
                new Image.GetThumbnailImageAbort(ThumbnailCallback);

            Image myThumbnail400x300 = img.GetThumbnailImage(512, 384, myCallback, IntPtr.Zero);

            myThumbnail400x300.Save(@"D:\" + 1 + ".jpg", ici, epa);

        }

        #region 图片压缩

        private static ImageCodecInfo ici;
        private static System.Drawing.Imaging.Encoder enc;
        private static EncoderParameter ep;
        private static EncoderParameters epa;

        private static void imageCompression()
        {
            ici = GetEncoderInfo("image/jpeg");

            enc = System.Drawing.Imaging.Encoder.Quality;

            epa = new EncoderParameters(1);

            ep = new EncoderParameter(enc, 25L);
            epa.Param[0] = ep;

        }
        /// <summary>
        /// 获取图像压缩器信息
        /// </summary>
        /// <param name="mimeType"></传进压缩图片的类型>
        /// <returns></压缩成功返回图像的ImageCodecInfo 信息>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                {
                    return encoders[j];
                }
            }
            return null;

        }

        #endregion

        /// <summary>
        /// 清空上一截图的文件
        /// </summary>
        private void printedPicsClear()
        {
            System.IO.File.Delete(@"D:\" + 1 + ".jpg");
        }

        #region 幻灯片演示中标记功能
        /*   [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]*/

        public struct Point2
        {
            ///LONG->int
            public int x;

            ///LONG->int
            public int y;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct HWND__
        {
            ///int
            public int unused;
        }

        public partial class NativeConstants
        {
            /// KEYEVENTF_KEYUP -> 0x0002
            public const int KEYEVENTF_KEYUP = 2;

            /// VK_DOWN -> 0x28
            public const int VK_DOWN = 40;

            /// VK_LEFT -> 0x25
            public const int VK_LEFT = 37;

            /// VK_RIGHT -> 0x27
            public const int VK_RIGHT = 39;

            /// VK_UP -> 0x26
            public const int VK_UP = 38;

            /// MOUSEEVENTF_LEFTUP -> 0x0004
            public const int MOUSEEVENTF_LEFTUP = 4;

            /// MOUSEEVENTF_LEFTDOWN -> 0x0002
            public const int MOUSEEVENTF_LEFTDOWN = 2;

            public const int VK_F5 = 116;

        }

        public partial class NativeMethods
        {
            /// VK_CONTROL -> 0x11
            public const int VK_CONTROL = 17;

            /// MOUSEEVENTF_LEFTDOWN -> 0x0002
            public const int MOUSEEVENTF_LEFTDOWN = 2;

            /// MOUSEEVENTF_LEFTUP -> 0x0004
            public const int MOUSEEVENTF_LEFTUP = 4;
            public const int VK_P = 80;
            public const int VK_A = 65;

            public const int VK_UP = 38;
            public const int VK_DOWN = 40;

            public const int VK_F5 = 116;

            /// Return Type: void
            ///bVk: BYTE->unsigned char
            ///bScan: BYTE->unsigned char
            ///dwFlags: DWORD->unsigned int
            ///dwExtraInfo: ULONG_PTR->unsigned int
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "keybd_event")]
            public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

            /// Return Type: void
            ///dwFlags: DWORD->unsigned int
            ///dx: DWORD->unsigned int
            ///dy: DWORD->unsigned int
            ///dwData: DWORD->unsigned int
            ///dwExtraInfo: ULONG_PTR->unsigned int
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "mouse_event")]
            public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

            [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "Sleep")]
            public static extern void Sleep(uint dwMilliseconds);

            /// Return Type: UINT->unsigned int
            ///uCode: UINT->unsigned int
            ///uMapType: UINT->unsigned int
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "MapVirtualKeyW")]
            public static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

            /// Return Type: HWND->HWND__*
            ///hWnd: HWND->HWND__*
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetFocus")]
            public static extern System.IntPtr SetFocus([System.Runtime.InteropServices.InAttribute()] System.IntPtr hWnd);

            /// Return Type: HWND->HWND__*
            ///Point: POINT->tagPOINT
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "WindowFromPoint")]
            public static extern System.IntPtr WindowFromPoint(Point2 Point);

            /// Return Type: BOOL->int
            ///X: int
            ///Y: int
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
            [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int X, int Y);

        }

        private void Draw(string str)
        {
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL,
                                       (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_CONTROL, 0),
                                       0,
                                       0);
            NativeMethods.keybd_event(NativeMethods.VK_P,
                                      (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_P, 0),
                                      0,
                                      0);
            NativeMethods.keybd_event(NativeMethods.VK_P,
                                      (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_P, 0),
                                      NativeConstants.KEYEVENTF_KEYUP,
                                      0);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL,
                                      (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_CONTROL, 0),
                                      NativeConstants.KEYEVENTF_KEYUP,
                                      0);
            str2arr(str);
            for (int i = 0; i < listX.Count; i++)
            {
                DDALine_all(listX[i] * 2, listY[i] * 2, listX[i + 1] * 2, listY[i + 1] * 2);
            }
        }

        /// <summary>
        /// DDA转换算法
        /// </summary>
        /// <param name="x0"></传进第一个点的x坐标>
        /// <param name="y0"></传进第一个点的y坐标>
        /// <param name="x1"></传进第二个点的x坐标>
        /// <param name="y1"></传进第二个点的y坐标>
        void DDALine(int x0, int y0, int x1, int y1)
        {
            int i;
            float dx, dy, length, x, y;
            if (Math.Abs(x1 - x0) >= Math.Abs(y1 - y0))
            {
                length = Math.Abs(x1 - x0);
            }
            else
            {
                length = Math.Abs(y1 - x0);
            }

            dx = (x1 - x0) / length;
            dy = (y1 - y0) / length;

            i = 1;
            x = x0;
            y = y0;

            while (i <= length)
            {
                DrawPoint((int)x, (int)y);
                x = x + dx;
                y = y + dy;
                i++;
            }
        }

        /// <summary>
        /// 在幻灯片上面画点
        /// </summary>
        /// <param name="x"></点的X坐标>
        /// <param name="y"></点的y坐标>
        void DrawPoint(int x, int y)
        {
            NativeMethods.SetCursorPos(x, y);
            NativeMethods.mouse_event(NativeConstants.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            NativeMethods.mouse_event(NativeConstants.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// DDA转换算法
        /// </summary>
        /// <param name="x0"></传进第一个点的x坐标>
        /// <param name="y0"></传进第一个点的y坐标>
        /// <param name="x1"></传进第二个点的x坐标>
        /// <param name="y1"></传进第二个点的y坐标>
        void DDALine_all(int x0, int y0, int x1, int y1)
        {
            int i;
            if (x0 == x1)
            {
                //为竖线
                if (y0 <= y1)
                {
                    for (i = y0; i <= y1; i++)
                        DrawPoint(x0, i);
                }
                else
                {
                    for (i = y1; i <= y0; i++)
                        DrawPoint(x0, i);
                }

                return;
            }

            //为横线
            if (y0 == y1)
            {
                if (x0 <= x1)
                {
                    for (i = x0; i <= x1; i++)
                        DrawPoint(i, y0);
                }
                else
                {
                    for (i = x1; i <= x0; i++)
                        DrawPoint(i, y0);
                }

                return;
            }

            //为斜线
            double m = (y1 - y0) * 1.0 / (x1 - x0);
            float fTemp;

            if (Math.Abs(m) <= 1)
            {
                if (x0 < x1)
                {
                    fTemp = (float)(y0 - m);
                    for (i = x0; i <= x1; i++)
                        DrawPoint(i, (int)(fTemp += (float)m));
                }
                else
                {
                    fTemp = (float)(y1 - m);
                    for (i = x1; i <= x0; i++)
                        DrawPoint(i, (int)(fTemp += (float)m));
                }
                return;
            }

            if (y0 < y1)
            {
                fTemp = (float)(x0 - 1 / m);
                for (i = y0; i <= y1; i++)
                    DrawPoint((int)(fTemp += (float)(1 / m)), i);
            }
            else
            {
                fTemp = (float)(x1 - 1 / m);
                for (i = y1; i <= y0; i++)
                    DrawPoint((int)(fTemp += (float)(1 / m)), i);
            }

        }
        public List<int> listX = new List<int>();
        public List<int> listY = new List<int>();

        public void str2arr(string str)
        {
            listX.Clear();
            listY.Clear();
            int a = 0, b = 0, c = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ';')
                {
                    c = i;
                    for (int j = c; j > 0; j--)
                    {
                        if (str[j] == ',')
                        {
                            b = j;
                            break;
                        }
                    }
                    cutArrfromStr(a, b, c, str);
                    a = c + 1;
                    b = c;
                }
            }
        }

        /// <summary>
        /// 从string s中获取坐标
        /// </summary>
        /// <param name="a"></辅助参数a>
        /// <param name="b"></辅助参数b>
        /// <param name="c"></辅助参数c>
        /// <param name="s"></要转换的字符串>
        public void cutArrfromStr(int a, int b, int c, string s)
        {
            string s1 = "", s2 = "";

            for (int i = a; i < b; i++)
            {
                s1 += s[i];
            }
            for (int i = b + 1; i < c; i++)
            {
                s2 += s[i];
            }

            int x, y;

            x = int.Parse(s1);
            y = int.Parse(s2);

            listX.Add(x);
            listY.Add(y);

        }

        /// <summary>
        /// 切换幻灯片到下一页
        /// </summary>
        private void NextPage()
        {
            NativeMethods.keybd_event(NativeMethods.VK_DOWN,
                (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_DOWN, 0),
                0,
                0);

            NativeMethods.keybd_event(NativeMethods.VK_DOWN,
                          (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_DOWN, 0),
                          NativeConstants.KEYEVENTF_KEYUP,
                          0);
        }

        /// <summary>
        /// 切换幻灯片到上一页
        /// </summary>
        private void PrePage()
        {
            NativeMethods.keybd_event(NativeMethods.VK_UP,
                (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_UP, 0),
                0,
                0);

            NativeMethods.keybd_event(NativeMethods.VK_UP,
                (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_UP, 0),
                NativeConstants.KEYEVENTF_KEYUP,
                0);
        }

        /// <summary>
        /// 点击播放幻灯片
        /// </summary>
        private void Start()
        {
            NativeMethods.keybd_event(NativeMethods.VK_F5,
                (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_F5, 0),
                0,
                0);

            NativeMethods.keybd_event(NativeMethods.VK_F5,
                (byte)NativeMethods.MapVirtualKeyW(NativeMethods.VK_F5, 0),
                NativeConstants.KEYEVENTF_KEYUP,
                0);
        }

        #endregion

        #region 蓝牙模块
        // 初始化蓝牙参数
        private BluetoothRadio radio;
        private BluetoothClient client;
        private BluetoothListener listener;

        private bool receiving = false;  // 控制接收线程
        private bool listening = false;  // 控制监听蓝牙设备连接线程
        private bool SIGN_STREAMSETUP = false;  // 标记文件的接收

        // 用来标记是否已经接收完一个文件
        private bool SIGN_FIlE_RECEIVED = false;

        // 文件参数的定义
        private System.IO.Stream stream;  
        private System.IO.FileStream sendfilestream;  
        private System.IO.FileStream receivefilestream;

        // 发送和接收信息的参数
        private string SENDFILENAME;
        private string SENDFILEADDRESS;
        private string RECEIVEDATA = "";
        private string DATATOSEND = "123";
        private string RECEIVEFILENAME;
        private string RECEIVEFILEADDRESS;
        private List<BluetoothClient> clientList = new List<BluetoothClient>();

        // 定义线程
        private System.Threading.Thread ReceiveThread;
        private System.Threading.Thread listenThread;
        private System.Threading.Thread sendfileThread;
        private System.Threading.Thread sendDataThread;

        private List<string> sendfileList = new List<string>();

        /// <summary>
        /// 检查电脑端的蓝牙设备是否存在
        /// </summary>
        /// <returns></检查正确择返回真>
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

        /// <summary>
        /// 创建监听线程，监听外围蓝牙设备的连接
        /// </summary>
        private void Listen()
        {
            if (radio == null)
                return;

            listener = new BluetoothListener(BluetoothService.SerialPort);
            listener.Start();
            listening = true;
            listenThread = new System.Threading.Thread(ListenLoop);
            listenThread.Start();
            sendfileThread = new System.Threading.Thread(SendFileLoop);
            sendDataThread = new System.Threading.Thread(SendDataLoop);
        }

        // 监听线程函数，用来监听来自手机端的请求并且分配各个用户的权限。
        private void ListenLoop()
        {
            while (listening)
            {
                try
                {
                    System.IO.Stream tempStream;
                    client = listener.AcceptBluetoothClient();
                    clientList.Add(client);
                    // 获取客户端信息（学生或者老师）
                    tempStream = client.GetStream();
                    byte[] mark;
                    int length;
                    mark = new byte[20];
                    length = tempStream.Read(mark, 0, mark.Length);
                    string right = System.Text.Encoding.ASCII.GetString(mark, 0, length);
                    if (right == "teacher")
                    {
                        stream = tempStream;
                        sendfileList.Add("tom.jpg");
                        WriteMessage("tom");
                        sendfileList.Add("jake.jpg");
                        WriteMessage("jake");
                        sendfileList.Add("sherry.jpg");
                        WriteMessage("sherry");
                    }
                    if (right.StartsWith("student"))
                    {
                        // 提取right中的学生名称并加进去listBox里面
                        string studentName = right.Substring(7, right.Length - 7);
                        WriteMessage(studentName);
                    }
                    SIGN_STREAMSETUP = true;

                    // 远程设备连接后，启动recieve程序
                    receiving = true;
                    new System.Threading.Thread(ReceiveLoop).Start((Object)client);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    continue;
                }
            }
            listener.Stop();
        }

        /// <summary>
        /// 将listbox1托管，是的数据的刷新能够及时显示出来。
        /// </summary>
        /// <param name="msg"></传进要显示的字符串>
        public delegate void SafeWinFormThreadDelegate(string msg);
        private void WriteMessage(string msg)
        { 
            SafeWinFormThreadDelegate d = new SafeWinFormThreadDelegate(Update);
            Invoke(d, new object[] {msg});
        }

        private void Update(string msg)
        {
            if (listBox1.Items.Count > 100)
            {
                listBox1.Items.RemoveAt(0);
            }
            listBox1.SelectedIndex = listBox1.Items.Add(msg);
            label3.Text = "登陆人数：" + listBox1.Items.Count.ToString();
        }

        /// <summary>
        /// 发送文件的线程函数。
        /// </summary>
        private void SendFileLoop()
        {
            while (SIGN_STREAMSETUP)
            {

                sendfilestream = new FileStream(SENDFILEADDRESS, FileMode.Open, FileAccess.Read, FileShare.None);
                byte[] buffer = new byte[sendfilestream.Length];

                string metaOfFile = "picture" + "//" + SENDFILENAME + "//" +
                    sendfilestream.Length.ToString();
                byte[] byte_metaOfFile = System.Text.Encoding.ASCII.GetBytes(metaOfFile);
                stream.Write(byte_metaOfFile, 0, byte_metaOfFile.Length);

                while (true)
                {
                    try
                    {
                        int length = sendfilestream.Read(buffer, 0, buffer.Length);
                        if (length == 0)
                        {
                            sendfilestream.Close();
                            sendfilestream.Dispose();

                            // 发送文件完毕，进程挂起。
                            sendfileThread.Suspend();
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
        }

        /// <summary>
        /// 发送文件函数
        /// </summary>
        /// <param name="filename"></传进要发送文件的名字>
        private void SendFile(string filename)
        {
            SENDFILEADDRESS = @"D:\" + @filename;
            SENDFILENAME = filename;
            if (sendfileThread.IsAlive)
                sendfileThread.Resume();
            else
                sendfileThread.Start();
        }

        /// <summary>
        /// 发送数据的线程函数。
        /// </summary>
        private void SendDataLoop()
        {
            while (SIGN_STREAMSETUP)
            {

                byte[] commandbuffer = System.Text.Encoding.ASCII.GetBytes("command");
                stream.Write(commandbuffer, 0, commandbuffer.Length);
                byte[] buffer = System.Text.Encoding.Default.GetBytes(DATATOSEND);
                if (buffer.Length == 0)
                    sendDataThread.Suspend();
                try
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.ToString());
                    break;
                }
                sendDataThread.Suspend();
            }
        }

        /// <summary>
        /// 发送信息函数
        /// </summary>
        /// <param name="filename"></传进要发送的信息>
        private void SendData(string msg)
        {
            DATATOSEND = msg;
            if (sendDataThread.IsAlive)
            {
                sendDataThread.Resume();
            }
            else
                sendDataThread.Start();
        }

        /// <summary>
        /// 监听接收端线程函数。
        /// </summary>
        /// <param name="oo"></传进每条连接Bluetoothclient的信息>
        private void ReceiveLoop(Object oo)
        {
            BluetoothClient btClient = (BluetoothClient)oo;
            Stream stream2 = btClient.GetStream();
            byte[] buffer;
            byte[] mark;
            while (receiving)
            {
                if (stream2.CanRead)
                {
                    try
                    {
                        int streamLength;
                        int fullLength = 0;
                        mark = new byte[100];

                        // 接收command来区别指令
                        int iii = 0;
                        iii = stream2.Read(mark, 0, mark.Length);
                        if (iii == 0)
                        {
                            clientList.Remove(btClient);
                            break;
                        }
                        string receiveCommand = System.Text.Encoding.ASCII.GetString(mark, 0, iii);
                        // 对接收到的命令分别处理
                        if (receiveCommand == "command")
                        {
                            RECEIVEDATA = "";
                            do
                            {
                                buffer = new byte[1024 * 2];
                                streamLength = stream2.Read(buffer, 0, buffer.Length);
                                RECEIVEDATA += System.Text.Encoding.Default.GetString(buffer, 0, streamLength);
                            } while (streamLength == 2048);

                            // 如果设备离开范围或者断开，则退出
                            if (streamLength == 0)
                            {
                                ReceiveThread.Abort();
                                break;
                            }

                            stream.Flush();

                            if (RECEIVEDATA == "NextPage")
                            {
                                NextPage();
                                NativeMethods.Sleep(300);
                                printSrc();
                                SendFile("1.jpg");
                                CURRENTPAGE++;
                            }
                            else
                                if (RECEIVEDATA == "PrePage")
                                {
                                    PrePage();
                                    NativeMethods.Sleep(300);
                                    printSrc();
                                    SendFile("1.jpg");
                                    --CURRENTPAGE;
                                }
                                else
                                    if (RECEIVEDATA.StartsWith("InsertAPage"))
                                    {
                                        string fileName = getFileNameFromCommand(RECEIVEDATA);
                                        string fileAddress = getCurrentFileAddress(fileName);
                                        InsertAPage(fileAddress, fileName);
                                    }
                                    else
                                        if (RECEIVEDATA == "Start")
                                        {
                                            Start();
                                            NativeMethods.Sleep(300);
                                            printSrc();
                                            SendFile("1.jpg");
                                            CURRENTPAGE = 1;
                                        }
                                        else
                                            if (RECEIVEDATA == "refresh")
                                            {
                                                string dataString = "file//";
                                                foreach (string s in sendfileList)
                                                {
                                                    dataString += s;
                                                    dataString += "//";
                                                }
                                                SendData(dataString);
                                            }
                                            else
                                                if (RECEIVEDATA.StartsWith("get"))
                                                {
                                                    // 发送教师端需要的文件
                                                    string filename = getFileNameFromCommand(RECEIVEDATA);
                                                    SendFile(filename);
                                                }

                                                else
                                                {
                                                    Draw(RECEIVEDATA);
                                                }
                        }
                            // 接收图片
                        else if (receiveCommand.StartsWith("picture"))
                        {
                            fullLength = getNumFromCommand(receiveCommand);
                            if (fullLength == -1) continue;

                            RECEIVEFILENAME = getFileNameFromCommand(receiveCommand);
                            SetReceiveFileAddress(RECEIVEFILENAME);
                            if (System.IO.File.Exists(RECEIVEFILEADDRESS))
                                System.IO.File.Delete(RECEIVEFILEADDRESS);

                            System.IO.FileInfo fileinfo = new System.IO.FileInfo(RECEIVEFILEADDRESS);
                            receivefilestream = fileinfo.OpenWrite();
                            BinaryWriter bw = new BinaryWriter(receivefilestream);
                            int recieveLength = 0;

                            do
                            {
                                buffer = new byte[1024 * 100];
                                streamLength = stream2.Read(buffer, 0, buffer.Length);
                                recieveLength += streamLength;
                                stream2.Flush();
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
                            bool mark_of_repeat = false;
                            foreach (string s in sendfileList)
                            {
                                if (s == RECEIVEFILENAME)
                                    mark_of_repeat = true;
                            }
                            if (!mark_of_repeat)
                                sendfileList.Add(RECEIVEFILENAME);
                            SIGN_FIlE_RECEIVED = true;
                            bw.Close();
                        }
                        else
                            continue;
                    }
                    catch
                    {
                        continue;
                    }

                }
            }
        }

        /// <summary>
        /// 获取当前文件的地址
        /// </summary>
        /// <param name="filename"></传入文件的名字>
        /// <returns></返回文件的地址>
        private string getCurrentFileAddress(string filename)
        {
            string fileAddress = @"D:" + @"\" + @filename;
            return fileAddress;
        }

        /// <summary>
        /// 设置接收文件的地址
        /// </summary>
        /// <param name="filename"></传进接收文件的名称>
        private void SetReceiveFileAddress(string filename)
        {
            RECEIVEFILEADDRESS = @"D:" + @"\" + @filename;
        }

        /// <summary>
        /// 从来自未来设备端的发送文件要求获取要接受文件的大小
        /// </summary>
        /// <param name="aaa"></传进接收到的命令>
        /// <returns></返回文件的大小>
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
        /// 从来自未来设备端的发送文件要求获取要接受文件的名字
        /// </summary>
        /// <param name="aaa"></传进接收到的命令>
        /// <returns></返回要接受文件的名称>
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

    #endregion

        // 用来记录幻灯片播放的页面
        // 定义操作幻灯片的参数。
        private int CURRENTPAGE = -1;
        bool bAssistantOn;

        PowerPoint.Application objApp;
        PowerPoint.Presentations objPresSet;
        PowerPoint._Presentation objPres;
        PowerPoint.Slides objSlides;
        PowerPoint._Slide objSlide;
        PowerPoint.TextRange objTextRng;
        PowerPoint.Shapes objShapes;
        PowerPoint.Shape objShape;
        PowerPoint.SlideShowWindows objSSWs;
        PowerPoint.SlideShowTransition objSST;
        PowerPoint.SlideShowSettings objSSS;
        PowerPoint.SlideRange objSldRng;
        Graph.Chart objChart;

        // 选择幻灯片进行播映
        private void button6_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openfileDialog = new
                System.Windows.Forms.OpenFileDialog();
            string pptAddress = "";
            openfileDialog = new System.Windows.Forms.OpenFileDialog();
            if (openfileDialog.ShowDialog() == DialogResult.OK)
            {
                if (openfileDialog.FileName.EndsWith("ppt") ||
                    openfileDialog.FileName.EndsWith("pptx"))
                {
                    pptAddress = openfileDialog.FileName;
                    this.ShowPresentation(pptAddress);
                }
                else
                    return;
            }
        }

        #region 控制幻灯片模块

        /// <summary>
        /// 打开被选中的幻灯片
        /// </summary>
        /// <param name="strTemplate"></param>
        private void ShowPresentation(string strTemplate)
        {
            objApp = new PowerPoint.Application();
            objApp.Visible = MsoTriState.msoTrue;
            objPresSet = objApp.Presentations;
            objPres = objPresSet.Open(strTemplate,
                MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
            objSlides = objPres.Slides;
        }

        /// <summary>
        /// 往正在播放的幻灯片后面插入一个Slide
        /// </summary>
        /// <param name="fileAddress"></正在播放幻灯片的地址>
        /// <param name="fileName"></正在播放幻灯片的名字>
        private void InsertAPage(string fileAddress, string fileName)
        {
            objSlide = objSlides.Add(CURRENTPAGE + 1, PowerPoint.PpSlideLayout.ppLayoutTitleOnly);
            objTextRng = objSlide.Shapes[1].TextFrame.TextRange;
            objTextRng.Text = "From " + fileName.Substring(0, fileName.Length - 4);
            objTextRng.Font.Name = "Comic Sans MS";
            objTextRng.Font.Size = 48;
            objSlide.Shapes.AddPicture(@fileAddress, MsoTriState.msoFalse, MsoTriState.msoTrue,
                150, 150, 500, 350);
        }

        #endregion

    }
}
