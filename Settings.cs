using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop_COntroller
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            var Appsettings = Properties.Settings.Default;
            server.Text = Appsettings.host;
            username.Text = Appsettings.username;
            password.Text = Appsettings.password;

        }
    }
}
