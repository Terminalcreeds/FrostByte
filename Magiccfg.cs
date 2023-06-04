using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrostByte
{
    public partial class Magiccfg : MetroFramework.Forms.MetroForm
    {
        public Magiccfg()
        {
            InitializeComponent();
        }

        private void Magiccfg_Load(object sender, EventArgs e)
        {
            if (Theme == MetroFramework.MetroThemeStyle.Light)
            {
                metroLabel1.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel5.Theme = MetroFramework.MetroThemeStyle.Light;
                metroLabel2.Theme = MetroFramework.MetroThemeStyle.Light;
                metroTextBox1.Theme = MetroFramework.MetroThemeStyle.Light;
                metroButton1.Theme = MetroFramework.MetroThemeStyle.Light;
                metroButton2.Theme = MetroFramework.MetroThemeStyle.Light;
                metroButton3.Theme = MetroFramework.MetroThemeStyle.Light;
                metroProgressSpinner1.Theme = MetroFramework.MetroThemeStyle.Light;
            }
            else
            {
                metroLabel1.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel5.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroLabel2.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroButton1.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroButton2.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroButton3.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroTextBox1.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroProgressSpinner1.Theme = MetroFramework.MetroThemeStyle.Dark;

            }

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
