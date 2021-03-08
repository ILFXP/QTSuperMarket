using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

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
            /*
             * 数据验证
             * 设置纯数字默认密码
             * 1-8位
             * 不可为空
             */
            Regex defaultpersonPasswordCheck = new Regex("^\d{1,8}$");
            string defaultpersonPassword = textBox1.Text.Trim();
            if (defaultpersonPasswordCheck.IsMatch(defaultpersonPassword))
            {
                /*
                 * 通过默认密码的数据验证
                 * 将选中项进行显示
                 * 将选中项写入配置
                 */
                if (checkBox2.Checked == true)
                {
                    //关闭程序时清理SQL Server Management Studio客户端
                    label5.Text += checkBox2.Text + "\n";
                    Settings1.Default.cleanSSMS = true;
                }

                if (checkBox3.Checked == true) {
                    //开机自动启动
                    label5.Text += checkBox3.Text + "\n";
                    Settings1.Default.startBoot = true;
                }

                if (checkBox4.Checked == true) {
                    //保持窗口总在最前
                    label5.Text += checkBox4.Text + "\n";
                    Settings1.Default.index999 = true;
                }
                
                if (checkBox6.Checked == true) {
                    //退出确认
                    label5.Text += checkBox6.Text + "\n";
                    Settings1.Default.quiteCheck = true;
                }
                
                if (checkBox7.Checked == true) {
                    //不再显示引导页
                    label5.Text += checkBox7.Text + "\n";
                    Settings1.Default.skipGuide = true;
                }
                
                label5.Text += "您设置的员工默认登录密码为：" + defaultpersonPassword;
                Settings1.Default.tci1 = 0;
                Settings1.Default.tci2 = 1;
                Settings1.Default.Save();
                tabControl1.SelectedIndex = 2;
            }
            else
            {
                MessageBox.Show("默认密码设置格式有误，请设置为1~8位的纯数字密码，请检查后重试！","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
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
                Settings1.Default.quiteCheck = true;
                Settings1.Default.Save();
            }
            if (checkBox6.Checked == true)
            {
                Settings1.Default.index999 = true;
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
