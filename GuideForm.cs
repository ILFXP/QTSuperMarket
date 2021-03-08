using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QTSuperMarket
{
    public partial class GuideForm : Form
    {
        public GuideForm()
        {
            InitializeComponent();
            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_Selecting);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void GuideForm_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Settings1.Default.tci1 = 1;
            Settings1.Default.tci2 = 2;
            Settings1.Default.Save();
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if(e.TabPageIndex == Settings1.Default.tci1 || e.TabPageIndex == Settings1.Default.tci2)
            {
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings1.Default.tci1 = 0;
            Settings1.Default.tci2 = 2;
            Settings1.Default.Save();
            tabControl1.SelectedIndex = 1;
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Settings1.Default.tci1 = 1;
            Settings1.Default.tci2 = 2;
            Settings1.Default.Save();
            tabControl1.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "")
                MessageBox.Show("请设置正确的员工默认登录密码！","提示");
            else
            {
                if (checkBox2.Checked == true) label5.Text = label5.Text + checkBox2.Text + "\n";
                if (checkBox3.Checked == true) label5.Text = label5.Text + checkBox3.Text + "\n";
                if (checkBox4.Checked == true) label5.Text = label5.Text + checkBox4.Text + "\n";
                if (checkBox6.Checked == true) label5.Text = label5.Text + checkBox6.Text + "\n";
                if (checkBox7.Checked == true) label5.Text = label5.Text + checkBox7.Text + "\n";
                label5.Text = label5.Text + "员工默认登录密码：" + textBox1.Text.Trim();
                Settings1.Default.tci1 = 0;
                Settings1.Default.tci2 = 1;
                Settings1.Default.Save();
                tabControl1.SelectedIndex = 2;
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Settings1.Default.tci1 = 0;
            Settings1.Default.tci2 = 2;
            Settings1.Default.Save();
            tabControl1.SelectedIndex = 1;
            label5.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                Settings1.Default.cleanSSMS = true;
                Settings1.Default.Save();
            }
            if (checkBox3.Checked == true)
            {
                Settings1.Default.startBoot = true;
                Settings1.Default.Save();
            }
            if (checkBox4.Checked == true)
            {
                Settings1.Default.useRecord = true;
                Settings1.Default.Save();
            }
            if (checkBox6.Checked == true)
            {
                Settings1.Default.quiteCheck = true;
                Settings1.Default.Save();
            }
            if (checkBox7.Checked == true)
            {
                Settings1.Default.skipGuide = true;
                Settings1.Default.Save();
            }
            Settings1.Default.defaultPassword = textBox1.Text.Trim();
            Settings1.Default.Save();
            LoginForm LF = new LoginForm();
            LF.Show();
            this.Hide();
        }
    }
}
