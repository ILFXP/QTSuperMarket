using System;
using System.Windows.Forms;

namespace QTSuperMarket
{
    public partial class tiaoshi : Form
    {
        public tiaoshi()
        {
            InitializeComponent();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (Settings1.Default.cleanSSMS == true)
                textBox1.Text = "true";
            else
                textBox1.Text = "false";
            if (Settings1.Default.skipGuide == true)
                textBox2.Text = "true";
            else
                textBox2.Text = "false";
            if (Settings1.Default.quiteCheck == true)
                textBox3.Text = "true";
            else
                textBox3.Text = "false";
            if (Settings1.Default.startBoot == true)
                textBox4.Text = "true";
            else
                textBox4.Text = "false";
            if (Settings1.Default.index999 == true)
                textBox5.Text = "true";
            else
                textBox5.Text = "false";
            textBox6.Text = Settings1.Default.defaultPassword;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "true")
            {
                Settings1.Default.cleanSSMS = true;
            }
            else
            {
                Settings1.Default.cleanSSMS = false;
            }
            if (textBox2.Text == "true")
            {
                Settings1.Default.skipGuide = true;
            }
            else
            {
                Settings1.Default.skipGuide = false;
            }
            if (textBox3.Text == "true")
            {
                Settings1.Default.quiteCheck = true;
            }
            else
            {
                Settings1.Default.quiteCheck = false;
            }
            if (textBox4.Text == "true")
            {
                Settings1.Default.startBoot = true;
            }
            else
            {
                Settings1.Default.startBoot = false;
            }
            if (textBox5.Text == "true")
            {
                Settings1.Default.index999 = true;
            }
            else
            {
                Settings1.Default.index999 = false;
            }
            Settings1.Default.defaultPassword = textBox6.Text.Trim();
            Settings1.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "true")
            {
                textBox1.Text = "false";
                Settings1.Default.cleanSSMS = false;
            }
            else
            {
                textBox1.Text = "true";
                Settings1.Default.cleanSSMS = true;
            }
            Settings1.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "true")
            {
                textBox2.Text = "false";
                Settings1.Default.skipGuide = false;
            }
            else
            {
                textBox2.Text = "true";
                Settings1.Default.skipGuide = true;
            }
            Settings1.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "true")
            {
                textBox3.Text = "false";
                Settings1.Default.quiteCheck = false;
            }
            else
            {
                textBox3.Text = "true";
                Settings1.Default.quiteCheck = true;
            }
            Settings1.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "true")
            {
                textBox4.Text = "false";
                Settings1.Default.startBoot = false;
            }
            else
            {
                textBox4.Text = "true";
                Settings1.Default.startBoot = true;
            }
            Settings1.Default.Save();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "true")
            {
                textBox5.Text = "false";
                Settings1.Default.index999 = false;
            }
            else
            {
                textBox5.Text = "true";
                Settings1.Default.index999 = true;
            }
            Settings1.Default.Save();
        }
    }
}
