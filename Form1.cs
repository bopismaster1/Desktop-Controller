using Gma.System.MouseKeyHook;
using MovablePython;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Desktop_COntroller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;

        }
        dbQuery query = new dbQuery();
        private IKeyboardMouseEvents m_GlobalHook;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.L))
            {
                MessageBox.Show("Display Settings");
                return true;
            }
            else if(keyData == (Keys.Control | Keys.D))
            {
                System.Windows.Forms.Application.Exit();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);
            //richTextBox1.Text += "MouseDown: \t{0}; \t System Timestamp: \t{1}"+ e.Button+ e.Timestamp;
            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                var key = "b14ca5898a4e4133bbce2ea2315a1916";
                DataTable dt = new DataTable();
                dt = query.DbSearch("Select * from user where password = '" + query.EncryptString(key, textBox1.Text.ToString()) + "'");

                if (dt.Rows.Count != 0)
                {
                    MessageBox.Show("Login Successfull");
                    main main = new main();
                    main.Show();
                    Hide();

                }
                else {
                    MessageBox.Show("Failed to Login!");
                }

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Subscribe();
            ipChecker();
            Properties.Settings.Default.ipv4 = GetLocalIPAddress();
            Properties.Settings.Default.Save();
            ipv4Address.Text = "Address: "+Properties.Settings.Default.ipv4;
            bool ConnectionStatus = PingHost(Properties.Settings.Default.host);

            if (ConnectionStatus) {
                label4.Text = "Connected";
                label4.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                label4.Text = "Disconnected";
                label4.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void ipChecker() {

            DataTable res = query.DbSearch("Select * from IPAddress where address='" + GetLocalIPAddress() + "'");

            if (res.Rows.Count==0) {
                 var ress=query.insert_update_dete("Insert into IPAddress(`address`,`lastPing`,`machineName`) values('" + GetLocalIPAddress() + "',now(),'" + System.Environment.MachineName + "')");
            }
            else
            {
                query.insert_update_dete("update IPAddress set `lastPing`=NOW() where address='"+GetLocalIPAddress()+"'");

            }
        }
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
        public  string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
