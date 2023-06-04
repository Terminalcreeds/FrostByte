using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using LibUsbDotNet.DeviceNotify;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using FrostByte.Properties;
using MetroFramework.Components;
using MetroFramework.Controls;
using static System.Windows.Forms.AxHost;

namespace FrostByte
{


    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
            getFullIdeviceInfo_normal();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            var text = "Double click to copy";
            new ToolTip().SetToolTip(metroLabel5, text);
            new ToolTip().SetToolTip(metroLabel2, text);
            new ToolTip().SetToolTip(metroLabel6, text);
            new ToolTip().SetToolTip(metroLabel4, text);

        }

        private static int deviceInfoLock = 0;
        private Process ideviceinfo = null;
        public string type = "";
        public static IDeviceNotifier UsbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
        private bool getIdeviceInfo(string argument = @"")
        {
            CheckForIllegalCrossThreadCalls = false;

            ideviceinfo = new Process();
            ideviceinfo.StartInfo.FileName = Environment.CurrentDirectory + "/files/x64/ideviceinfo.exe";
            ideviceinfo.StartInfo.Arguments = argument;
            ideviceinfo.StartInfo.UseShellExecute = false;
            ideviceinfo.StartInfo.RedirectStandardOutput = true;
            ideviceinfo.StartInfo.CreateNoWindow = true;

            ideviceinfo.Start();
            iOSDevice.isMEID = false;
            iOSDevice.MEID = "";
            iOSDevice.IMEI = "";

            var lines = 0;

            while (!ideviceinfo.StandardOutput.EndOfStream)
            {
                lines++;

                string line = ideviceinfo.StandardOutput.ReadLine();

                var text2 = line.Replace("\r", "");

                if (text2.StartsWith("UniqueDeviceID: "))
                {
                    var text3 = text2.Replace("UniqueDeviceID: ", "");
                    iOSDevice.UDID = text3.Trim();
                    metroLabel6.Text = text3.Trim().ToUpper();
                }
                else if (text2.StartsWith("ProductVersion: "))
                {
                    var text3 = text2.Replace("ProductVersion: ", "");
                    //iOS = text3;
                  //  labelVersion.Text = text3 ;
                    iOSDevice.IOSVersion = text3;
                }
                else if (text2.StartsWith("BuildVersion: "))
                {
                    var text3 = text2.Replace("BuildVersion: ", "");
                    //build = text3;
                }
                else if (text2.StartsWith("ProductType: "))
                {
                    var text3 = text2.Replace("ProductType: ", "");
                    type = text3;
                    metroLabel5.Text = type;
                    iOSDevice.setProductType(type);
                    metroLabel5.Text = iOSDevice.Model;
                    
                }
                else if (text2.StartsWith("ProductVersion: "))
                {
                    var text3 = text2.Replace("ProductVersion: ", "");
                   // iOS = text3;
                  //  labelVersion.Text = text3;
                    iOSDevice.IOSVersion = text3;
                }
                else if (text2.StartsWith("BuildVersion: "))
                {
                    var text3 = text2.Replace("BuildVersion: ", "");
                    iOSDevice.BuildVersion = text3;
                }
                else if (text2.StartsWith("DeviceName: "))
                {
                    var text3 = text2.Replace("DeviceName: ", "");
                    iOSDevice.DeviceName = text3;
                }
                else if (text2.StartsWith("InternationalMobileEquipmentIdentity: "))
                {
                    var text3 = text2.Replace("InternationalMobileEquipmentIdentity: ", "");
                    iOSDevice.IMEI = text3;
                   // labelIMEI.Text = text3;
                }
                else if (text2.StartsWith("PhoneNumber: "))
                {
                    var text3 = text2.Replace("PhoneNumber: ", "");
                    //abelnumber.Text = text3;
                }
                else if (text2.StartsWith("ModelNumber: "))
                {
                    var text3 = text2.Replace("ModelNumber: ", "");
                }
                else if (text2.StartsWith("TimeZone: "))
                {
                    var text3 = text2.Replace("TimeZone: ", "");
                }

                var split = line.Split(new char[] { ':' });

                if (split[0] == "SerialNumber")
                {
                    iOSDevice.SerialNumber = split[1].Trim();
                    metroLabel2.Text = split[1].Trim();


                    iOSDevice.SerialNumber = split[1].Trim();
                    metroLabel2.Text = split[1].Trim();
                    string response = modelserver();
                    iOSDevice.ModelServer = response;
                    metroLabel5.Text = iOSDevice.ModelServer;
                }

                if (split[0] == "ActivationState")
                {
                    iOSDevice.ActivationState = split[1].Trim();
                    metroLabel4.Text = split[1].Trim();
                  //  metroLabel4.Text = metroLabel4;
                }

                if (split[0] == "SIMStatus")
                {
                    iOSDevice.SIMStatus = split[1].Trim();
                    iOSDevice.isSIMInserted = iOSDevice.SIMStatus == "kCTSIMSupportSIMStatusReady" ^ iOSDevice.SIMStatus == "kCTSIMSupportSIMStatusPINLocked";

                }
                if (line.Contains("MobileEquipmentIdentifier"))
                {
                    iOSDevice.MEID = split[1].Trim();
                }
                GetDevice();
            }
            Interlocked.Exchange(ref deviceInfoLock, 0);

            if (lines <= 2)
            {
                return false;
            }

            return true;
        }
        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            if (e.EventType.ToString() == "DeviceRemoveComplete")
            {
                getFullIdeviceInfo_normal();

            }
            else
            {
                getFullIdeviceInfo_normal();

            }
        }

        public string modelserver()
        {
            string text = metroLabel2.Text;
            int cantidad = text.Length;
            string requestUriString = "https://iservices-dev.us/check/model.php?imei=" + text;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(requestUriString);
            httpWebRequest.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
            httpWebRequest.Timeout = 60000;
            string result;
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            if (result == "\"ERROR\"/\"Invalid IMEI/Serial Number\"")
            {

                return "\"ERROR\"/\"Invalid IMEI/Serial Number\"";
            }
            if (cantidad == 15)
            {
                dynamic stuff = JObject.Parse(result);

                string model = stuff.Modelo;

                return model;
            }
            if (cantidad == 12)
            {
                dynamic stuff = JObject.Parse(result);

                string Modelo = stuff.Modelo;

                return Modelo;
            }
            return result;

        }
        public void getFullIdeviceInfo_normal(bool connect = true)
        {
            CheckForIllegalCrossThreadCalls = false;

            if (0 == Interlocked.Exchange(ref deviceInfoLock, 1))
            {
                var res = getIdeviceInfo();


                if (res)
                {
                    // var res = getIdeviceInfo();
                    DeviceConnect_Normal();
                }

                if (res == false)
                {
                    DeviceDisconnect_Normal();
                }
            }
        }


        public void OFFGetDevice()
        {
            if (iOSDevice.ProductType.Contains("iPhone"))
            {
                switch (type)
                {
                    case "iPhone6,1":
                        picbox.Visible = false;
                        break;
                    case "iPhone6,2":
                        picbox.Visible = false;
                        break;
                    case "iPhone7,1":
                        picbox.Visible = false;
                        break;
                    case "iPhone7,2":
                        picbox.Visible = false;
                        break;
                    case "iPhone8,2":
                        picbox.Visible = false;
                        break;
                    case "iPhone8,1":
                        picbox.Visible = false;
                        break;
                    case "iPhone8,4":
                        picbox.Visible = false;
                        break;
                    case "iPhone9,1":
                        picbox.Visible = false;
                        break;
                    case "iPhone9,2":
                        picbox.Visible = false;
                        break;
                    case "iPhone9,3":
                        picbox.Visible = false;
                        break;
                    case "iPhone9,4":
                        picbox.Visible = false;
                        break;
                    case "iPhone10,4":
                        picbox.Visible = false;
                        break;
                    case "iPhone10,1":
                        picbox.Visible = false;
                        break;
                    case "iPhone10,2":
                        picbox.Visible = false;
                        break;
                    case "iPhone10,5":
                        picbox.Visible = false;
                        break;
                    case "iPhone10,6":
                        picbox.Visible = false;
                        break;
                    default:
                        picbox.Visible = false;
                        break;
                }
            }
            if (iOSDevice.ProductType.Contains("iPad"))
            {
                picbox.Visible = false;
            }
            if (iOSDevice.ProductType.Contains("iPod"))
            {
                picbox.Visible = false;
            }
        }


        public void ONGetDevice()
        {
            if (iOSDevice.ProductType.Contains("iPhone"))
            {
                switch (type)
                {
                    case "iPhone6,1":
                        picbox.Visible = true;
                        break;
                    case "iPhone6,2":
                        picbox.Visible = true;
                        break;
                    case "iPhone7,1":
                        picbox.Visible = true;
                        break;
                    case "iPhone7,2":
                        picbox.Visible = true;
                        break;
                    case "iPhone8,2":
                        picbox.Visible = true;
                        break;
                    case "iPhone8,1":
                        picbox.Visible = true;
                        break;
                    case "iPhone8,4":
                        picbox.Visible = true;
                        break;
                    case "iPhone9,1":
                        picbox.Visible = true;
                        break;
                    case "iPhone9,2":
                        picbox.Visible = true;
                        break;
                    case "iPhone9,3":
                        picbox.Visible = true;
                        break;
                    case "iPhone9,4":
                        picbox.Visible = true;
                        break;
                    case "iPhone10,4":
                        picbox.Visible = true;
                        break;
                    case "iPhone10,1":
                        picbox.Visible = true;
                        break;
                    case "iPhone10,2":
                        picbox.Visible = true;
                        break;
                    case "iPhone10,5":
                        picbox.Visible = true;
                        break;
                    case "iPhone10,6":
                        picbox.Visible = true;
                        break;
                    default:
                        picbox.Visible = true;
                        break;
                }
            }
            if (iOSDevice.ProductType.Contains("iPad"))
            {
                picbox.Visible = true;
            }
            if (iOSDevice.ProductType.Contains("iPod"))
            {
                picbox.Visible = true;
            }
        }
        public void GetDevice()
        {
            if (iOSDevice.ProductType.Contains("iPhone"))
            {
                //this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                switch (type)
                {
                    case "iPhone6,1":
                        break;

                    case "iPhone5,1":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone5,2":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone4,1":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone4,2":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone7,1":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone7,2":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone8,2":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone8,1":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone8,4":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";
                        break;
                    case "iPhone9,1":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone9,2":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone9,3":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone9,4":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone10,4":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone10,1":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone10,2":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone10,5":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    case "iPhone10,6":
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                    default:
                        this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPhone" + "/" + iOSDevice.ProductType + "/online-infobox.png";

                        break;
                }
            }
            if (iOSDevice.ProductType.Contains("iPad"))
            {
                this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPad" + "/" + iOSDevice.ProductType + "/online-infobox.png";

            }
            if (iOSDevice.ProductType.Contains("iPod"))
            {
                this.picbox.ImageLocation = "https://statici.icloud.com/fmipmobile/deviceImages-9.0/" + "iPod" + "/" + iOSDevice.ProductType + "/online-infobox.png";

            }
        }

        public void DeviceDisconnect_Normal()
        {
            OFFGetDevice();
            pictureBox1.Visible = true;
           // pictureBox3.Visible= true;
            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
            metroLabel5.Text = "-";
            metroLabel2.Text = "-";
            metroLabel6.Text = "-";
            metroLabel4.Text = "-";
            metroProgressSpinner1.Visible = false;
            pictureBox2.Visible = false;
            metroLabel1.Text = "No Device Connected";
        }
        public void DeviceConnect_Normal()
        {
            ONGetDevice();
            pictureBox1.Visible=false;
            //pictureBox3.Visible=false;
           // label13.Text = "Model";
            metroLabel5.Text = "Model: "+iOSDevice.ModelServer;
            //label00.Text = "UDID";
            metroLabel6.Text = "UDID: "+iOSDevice.UDID;
            //label7.Text = "SerialNumber";
            metroLabel2.Text = "SerialNumber: "+iOSDevice.SerialNumber;
            metroLabel4.Text = "Activation: " + iOSDevice.ActivationState;
            metroProgressSpinner1.Visible = false;
            pictureBox2.Visible = false;
            //label9.Text = "CPID";
            //labelhwid.Text = "HWID";
            //label13.Text = "Serial/SN";
            //label15.Text = "Registration";
            //label17.Text = "PWND";
           // Button(true);
          //  metroLabel1.ForeColor = Color.Red;
            metroLabel1.Text = "Device Connected";
            metroLabel1.BackColor = Color.Green;

        }
       
        //-----------< Connect Device Action >---------------------------

       


       

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            Magiccfg n = new Magiccfg();


            if (metroToggle1.Checked)
            {
                this.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel1.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel2.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel6.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel5.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel4.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel3.Theme = MetroFramework.MetroThemeStyle.Light;
                metroPanel1.Theme = MetroFramework.MetroThemeStyle.Light;
                n.Theme = MetroFramework.MetroThemeStyle.Light;
                metroProgressSpinner1.Theme = MetroFramework.MetroThemeStyle.Light;
                picbox.BackColor= Color.White;
                metroLabel7.Theme = MetroFramework.MetroThemeStyle.Light;
                metroProgressBar1.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel3.Text = "LightMode";
            }
            else
            {
                this.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel1.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel2.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel6.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel5.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel4.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel3.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroPanel1.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroProgressSpinner1.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel7.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroProgressBar1.Theme = MetroFramework.MetroThemeStyle.Dark;
                n.Theme = MetroFramework.MetroThemeStyle.Dark;
                picbox.BackColor = Color.FromArgb(17,17,17);
                metroLabel3.Text = "DarkMode";
            }
        }

        private void metroLabel3_Click(object sender, EventArgs e)
        {

        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Magiccfg form= new Magiccfg();
            
            if(metroToggle1.Checked)
            {
                form.Theme = MetroFramework.MetroThemeStyle.Light;
               // form.BackColor = Color.White;
                form.ShowDialog();
            }
            else
            {
                form.Theme = MetroFramework.MetroThemeStyle.Dark;
               // form.BackColor = Color.FromArgb(17,17, 17);
                form.ShowDialog();
            }
            
        }

        private void metroProgressSpinner1_Click(object sender, EventArgs e)
        {

        }
    }
}
