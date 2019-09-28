using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpPcap;
using PacketDotNet;
using SharpPcap.LibPcap;
using System.Net;
using System.Text.RegularExpressions;
namespace tcp实验
{
    // 协议类型
    public enum SnapProtocol : int
    {
        udp = 0,
        tcp = 1,
        arp = 2,
        rarp = 3,
        ip = 4
    }
    public partial class Form1 : Form
    {
        CaptureDeviceList device_list;         // 设备列表
        ICaptureDevice device;              // 当前选择设备
        DelegateMethod disp_info;           // 委托
        public Form1()
        {
            InitializeComponent();

            // 获得设备列表
            device_list = GetDeviceList();

            // 获取支持的协议列表
            LoadProto();

            // 载入所有网卡
            LoadDevice();

            // 显示数据包委托函数
            disp_info = new DelegateMethod(Disp_PacketInfo);
        }
        /// <summary>
        /// 获取支持的协议列表
        /// </summary>
        private void LoadProto()
        {
            comboBox2.Items.Clear();
            foreach (string e in Enum.GetNames(typeof(SnapProtocol)))
            {
                comboBox2.Items.Add(e);
            }
            comboBox2.SelectedIndex = 0;
        }

        /// <summary>
        /// 载入所有网卡信息
        /// </summary>
        private void LoadDevice()
        {
            comboBox1.Items.Clear();
            if (device_list == null)
            {
                MessageBox.Show("没有找到任何网卡设备！");
                return;
            }

            foreach (LibPcapLiveDevice dev in device_list)
            {
                try
                {
                    comboBox1.Items.Add(dev.Addresses[3].Addr);
                }
                catch
                {
                    continue;
                }
            }
            comboBox1.SelectedIndex = 0;
        }


        /// <summary>
        /// 获得当前的设备列表（网卡）
        /// </summary>
        /// <returns></returns>
        public CaptureDeviceList GetDeviceList()
        {
            // Print SharpPcap version
            string ver = SharpPcap.Version.VersionString;
            this.richTextBox1.Text = string.Format("SharpPcap {0}, DeviceList\n", ver);
            try
            {
                // Retrieve the device list
                CaptureDeviceList devices = CaptureDeviceList.Instance;
                // If no devices were found print an error
                if (devices.Count < 1)
                {
                    this.richTextBox1.Text += "No devices were found on thismachine\n";
                    return null;
                }
                this.richTextBox1.Text += "\nThe following devices areavailable on this machine:\n";
                this.richTextBox1.Text += "----------------------------------------------------\n";
                // Print out the available network devices
                //foreach (ICaptureDevice dev in device_list)
                //{
                //    string pattern1 = @"FriendlyName.*";
                //    string pattern2 = @"GatewayAddress.+";
                //    string pattern3 = @"Description.*";
                //    string pattern4 = @"Addr:.+";
                //    string pattern5 = @"Netmask:.+";
                //    string pattern6 = @"Broadaddr.+";
                //    string pattern7 = @"HW addr.+";
                //    string devInfo = string.Format("{0}\n", dev.ToString());
                //    foreach (Match match in Regex.Matches(devInfo, pattern1))
                //        this.richTextBox1.Text += match.Value + "\n";  
                //    foreach (Match match in Regex.Matches(devInfo, pattern2))
                //        this.richTextBox1.Text += match.Value + "\n";
                //    foreach (Match match in Regex.Matches(devInfo, pattern3))
                //        this.richTextBox1.Text += match.Value + "\n";
                //    foreach (Match match in Regex.Matches(devInfo, pattern4))
                //        this.richTextBox1.Text += match.Value + "\n";
                //    foreach (Match match in Regex.Matches(devInfo, pattern5))
                //        this.richTextBox1.Text += match.Value + "\n";
                //    foreach (Match match in Regex.Matches(devInfo, pattern6))
                //        this.richTextBox1.Text += match.Value + "\n";
                //    foreach (Match match in Regex.Matches(devInfo, pattern7))
                //        this.richTextBox1.Text += match.Value + "\n";
                //}
                Console.WriteLine(devices[1]);
                this.richTextBox1.Text += string.Format("{0}\n", devices[1].ToString());
                return devices;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            if (button1.Text == "打开设备")
            {
                button1.Text = "关闭设备";
                groupBox1.Enabled = false;


                device = device_list[comboBox1.SelectedIndex];
                device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);
                device.Open(DeviceMode.Promiscuous, 1000);
                device.Filter = RcvPacketFilter();
                device.StartCapture();
            }
            else
            {
                button1.Text = "打开设备";
                groupBox1.Enabled = true;

                device.StopCapture();
                device.Close();
            }
        }



        

        /// <summary>
        /// 根据要求构造数据包过滤字符串
        /// </summary>
        private string RcvPacketFilter()
        {
            SnapProtocol proto = (SnapProtocol)comboBox2.SelectedIndex;
            IPAddress src_host, dst_host;

            string filter = proto.ToString();
            if ((textBox1.Text.Trim().Length > 0) && (IPAddress.TryParse(textBox1.Text, out src_host)))
            {
                filter += string.Format(" and src host {0}", src_host.ToString());
            }

            if ((textBox2.Text.Trim().Length > 0) && (IPAddress.TryParse(textBox2.Text, out dst_host)))
            {
                filter += string.Format(" and dst host {0}", dst_host.ToString());
            }

            return filter;
        }


        /// <summary>
        /// 抓包事件函数，在抓到符合条件的数据包的时候该函数将被调用
        /// 功能：
        ///     1. 获得当前数据包的时间间隔、长度、协议类型、地址等参数
        ///     2. 将信息输出到RichTextBox控件显示出来
        /// </summary>
        private void device_OnPacketArrival(object sender, CaptureEventArgs packet)
        {
            // 时间和长度的获取
            DateTime time = packet.Packet.Timeval.Date;
            int len = packet.Packet.Data.Length;
            // 解析数据包成：IP包
            Packet p = Packet.ParsePacket(packet.Packet.LinkLayerType, packet.Packet.Data);
            IpPacket ip = (IpPacket)p.Extract(typeof(IpPacket));

            // 数据包信息
            string info = string.Format("\nsrc_addr={0}, des_addr={1}, type={2}\n",
                ip.SourceAddress, ip.DestinationAddress, ip.Protocol);
            info += string.Format("{0}:{1}:{2},{3} Len={4}\n",
                time.Hour, time.Minute, time.Second, time.Millisecond, len);
            info += string.Format(byteToHexStr(packet.Packet.Data));

            // 使用委托显示结果
            richTextBox1.Invoke(disp_info, info);
        }

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }

        /// <summary> 
        /// 字符串转16进制字节数组 
        /// </summary> 
        /// <param name="hexString"></param> 
        /// <returns></returns> 
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 字符串IP地址转换成byte数组
        /// </summary>
        /// <param name="decString"></param>
        /// <returns></returns>
        public static byte[] strIPToByte(string decString)
        {
            string[] decStringArray = decString.Split('.');
            if (decStringArray.Length == 4)
            {
                byte[] returnBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    returnBytes[i] = Convert.ToByte(decStringArray[i]);
                }
                return returnBytes;
            }
            else
            {
                MessageBox.Show("IP地址格式错误！（参考：192.168.0.1）");
                return null;
            }
        }


        delegate void DelegateMethod(string info);
        /// <summary>
        /// 显示收到数据包的信息（由于捕获过程开辟了新线程，因此捕获结果需要委托来传递到RichTextBox）
        /// </summary>
        /// <param name="info"></param>
        private void Disp_PacketInfo(string info)
        {
            richTextBox1.Text += info;
        }
        
    }

}