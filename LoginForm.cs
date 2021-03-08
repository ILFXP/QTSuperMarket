using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;

namespace QTSuperMarket
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //建立数据库的连接
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();

            string personName = textBox1.Text.Trim();
            string personPassword = textBox2.Text.Trim();
            string personNum = "";
            //构造查询语句
            string sqlLang1 = String.Format("select count(*) from personInf where personName = '{0}' and personPassword = '{1}'", personName, personPassword);
            SqlCommand com = new SqlCommand(sqlLang1, con);

            //验证是否有与输入内容一致的列
            int numCheck = (int)com.ExecuteScalar();

            //有查询结果
            if(numCheck > 0)
            {
                string personLimit = "";
                string sqllang2 = "select personLimit,personNum from personInf where personName = '" + personName + "'";
                SqlCommand limitCheck = new SqlCommand(sqllang2,con);
                //读取查询的结果
                SqlDataReader reader = limitCheck.ExecuteReader();
                while (reader.Read())
                {
                    personLimit = reader["personLimit"].ToString().Trim();
                    personNum = reader["personNum"].ToString().Trim();
                }
                //判断登录人身份
                if(personLimit == "admin")
                {
                    Settings1.Default.nowAdmin = personName;
                    Settings1.Default.Save();
                    MessageBox.Show("欢迎，" + personName + "！您拥有管理员权限，即将为您打开后台控制程序。","提示");
                    adminMainForm amf = new adminMainForm();
                    amf.Show();
                    this.Hide();

                }
                else if(personLimit == "worker"){
                    Settings1.Default.workerLastUseName = Settings1.Default.nowWorker = personName;
                    Settings1.Default.workerLastUseTime = DateTime.Now.ToString();
                    Settings1.Default.workerLastUseNum = personNum;
                    Settings1.Default.Save();
                    MessageBox.Show("欢迎，" + personName + "!","提示");
                    workerMainForm wmf = new workerMainForm();
                    wmf.Show();
                    this.Hide();
                }
            }
            //无查询结果
            else
            {
                MessageBox.Show("请确认您输入的姓名和密码是否正确！请在确认后重新尝试登录！", "错误");
                textBox1.Text = textBox2.Text = "";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("忘记密码请联系管理员！","提示");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
             * 读取本地设置中的配置
             * 1.保持窗口总在最前
             * 2.不再显示引导页
             */
            if(Settings1.Default.skipGuide == false)
            {
                GuideForm gf = new GuideForm();
                gf.Show();
                this.Hide();
            }
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
             * 读取本地设置中的配置
             * 判断是否开启以下功能：
             * 1.关闭程序是清理SQL Server Management Studio客户端
             * 2.退出确认
             */
            if (Settings1.Default.cleanSSMS == true)
            {
                if(Settings1.Default.quiteCheck == true)
                {
                    DialogResult result = MessageBox.Show("是否退出程序？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (result == DialogResult.OK)
                    {
                        Process[] ifSSMSisRun = Process.GetProcessesByName("SSMS");
                        if (ifSSMSisRun.Length > 0)
                        {
                            MessageBox.Show("即将为您关闭SQL Server Management Studio并退出程序！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            foreach (Process killSSMS in ifSSMSisRun)
                                killSSMS.Kill();
                            Dispose();
                            Application.Exit();
                        }
                        else
                        {
                            MessageBox.Show("本地的SQL Server Managem Studio客户端未运行,程序即将退出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Dispose();
                            Application.Exit();
                        }
                    }
                    else if(result == DialogResult.Cancel){
                        e.Cancel = true;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GuideForm gf = new GuideForm();
            gf.Show();
            tiaoshi ts = new tiaoshi();
            ts.Show();

        }
    }
}
