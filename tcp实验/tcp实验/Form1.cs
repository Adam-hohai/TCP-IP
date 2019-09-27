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
using System.Text.RegularExpressions;
namespace tcp实验
{
 public partial class Form1 : Form
 {
 CaptureDeviceList device_list;
 public Form1()
 {
 InitializeComponent();
 device_list = GetDeviceList();
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
 CaptureDeviceList device_list = CaptureDeviceList.Instance;
 // If no devices were found print an error
 if (device_list.Count < 1)
 {
 this.richTextBox1.Text += "No devices were found on thismachine\n";
 return null;
 }
 this.richTextBox1.Text += "\nThe following devices areavailable on this machine:\n";
 this.richTextBox1.Text +="----------------------------------------------------\n";
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
 Console.WriteLine(device_list[1]);
 this.richTextBox1.Text += string.Format("{0}\n", device_list[1].ToString());

 
 return device_list;
 }
 catch (System.Exception ex)
 {
     MessageBox.Show(ex.ToString());
     return null;
 }
 }
 }
}