using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

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
            writeLog.writeProgramLog("打开引导页");
            button1.Enabled = false;
            radioButton1.Checked = true;
            Settings1.Default.tci1 = 1;
            Settings1.Default.tci2 = 2;
            Settings1.Default.tci3 = 3;
            Settings1.Default.Save();
            checkBox5.Checked = true;
            if (Settings1.Default.cleanSSMS == true) checkBox2.Checked = true;
            else checkBox2.Checked = false;
            if (Settings1.Default.startBoot == true) checkBox3.Checked = true;
            else checkBox3.Checked = false;
            if (Settings1.Default.index999 == true) checkBox4.Checked = true;
            else checkBox4.Checked = false;
            if (Settings1.Default.quiteCheck == true) checkBox6.Checked = true;
            else checkBox6.Checked = false;
            if (Settings1.Default.skipGuide == true) checkBox7.Checked = true;
            else checkBox7.Checked = false;
            textBox1.Text = Settings1.Default.defaultPassword;
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if(e.TabPageIndex == Settings1.Default.tci1 || e.TabPageIndex == Settings1.Default.tci2 || e.TabPageIndex == Settings1.Default.tci3)
            {
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings1.Default.tci1 = 0;
            Settings1.Default.tci2 = 2;
            Settings1.Default.tci3 = 3;
            Settings1.Default.Save();
            tabControl1.SelectedIndex = 1;
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Settings1.Default.tci1 = 1;
            Settings1.Default.tci2 = 2;
            Settings1.Default.tci3 = 3;
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
            Regex defaultpersonPasswordCheck = new Regex("^[0-9]*$");
            string defaultpersonPassword = textBox1.Text.Trim();
            if (defaultpersonPasswordCheck.IsMatch(defaultpersonPassword) && defaultpersonPassword.Length > 0)
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
                    //退出前询问
                    label5.Text += checkBox6.Text + "\n";
                    Settings1.Default.quiteCheck = true;
                }
                
                if (checkBox7.Checked == true) {
                    //不再显示引导页
                    label5.Text += checkBox7.Text + "\n";
                    Settings1.Default.skipGuide = true;
                }
                label5.Text += "您设置的员工默认登录密码为：" + defaultpersonPassword;
                if (Settings1.Default.havaAdmin == true)
                {
                    string adminName = Settings1.Default.adminName;
                    MessageBox.Show("您已经拥有一个管理员账户：" + adminName);
                    Settings1.Default.tci1 = 0;
                    Settings1.Default.tci2 = 1;
                    Settings1.Default.tci3 = 2;
                    Settings1.Default.Save();
                    tabControl1.SelectedIndex = 3;
                }
                else
                {
                    Settings1.Default.tci1 = 0;
                    Settings1.Default.tci2 = 1;
                    Settings1.Default.tci3 = 3;
                    Settings1.Default.Save();
                    tabControl1.SelectedIndex = 2;
                }
            }
            else
            {
                MessageBox.Show("默认密码设置格式有误，请设置为1~8位的纯数字密码，请检查后重试！","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label5.Text = "";
            if (Settings1.Default.havaAdmin == true)
            {
                Settings1.Default.tci1 = 0;
                Settings1.Default.tci2 = 2;
                Settings1.Default.tci3 = 3;
                Settings1.Default.Save();
                tabControl1.SelectedIndex = 1;
            }
            else
            {
                Settings1.Default.tci1 = 0;
                Settings1.Default.tci2 = 1;
                Settings1.Default.tci3 = 3;
                Settings1.Default.Save();
                tabControl1.SelectedIndex = 2;
            }
            
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                Settings1.Default.cleanSSMS = true;
                Settings1.Default.Save();
                writeLog.writeProgramLog("提示：启用功能-关闭程序时清理SQL Server Management Studio客户端");
            }
            if (checkBox3.Checked == true)
            {
                Settings1.Default.startBoot = true;
                Settings1.Default.Save();
                writeLog.writeProgramLog("提示：启用功能-开机自动启动");
            }
            if (checkBox4.Checked == true)
            {
                Settings1.Default.quiteCheck = true;
                Settings1.Default.Save();
                writeLog.writeProgramLog("提示：启用功能-保持窗口总在最前");
            }
            if (checkBox6.Checked == true)
            {
                Settings1.Default.index999 = true;
                Settings1.Default.Save();
                writeLog.writeProgramLog("提示：启用功能-退出前询问");
            }
            if (checkBox7.Checked == true)
            {
                Settings1.Default.skipGuide = true;
                Settings1.Default.Save();
                writeLog.writeProgramLog("提示：启用功能-不再显示引导页");
            }
            Settings1.Default.havaAdmin = true;
            Settings1.Default.defaultPassword = textBox1.Text.Trim();
            Settings1.Default.Save();
            MessageBox.Show("设置已保存，请重新启动程序！","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
            writeLog.writeProgramLog("设置管理员账户成功");
            writeLog.writeProgramLog("设置员工默认登录密码");
            writeLog.writeProgramLog("退出程序");
            Application.Exit();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Settings1.Default.tci1 = 0;
            Settings1.Default.tci2 = 2;
            Settings1.Default.tci3 = 3;
            Settings1.Default.Save();
            tabControl1.SelectedIndex = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
             * 进行输入数据的验证
             * 0.先进行非空验证
             * 1.验证姓名为2-3为的纯汉字
             * 2.为字段personLimit添加默认值：admin
             * 3.为字段personNum添加默认值：admin
             * 4.验证密码为1-8位纯数字
             * 5.验证手机号是否正确
             * 6.连接数据库
             * 7.创建SQL语句插入数据
             * 8.下一步
             */

            //step 0
            string name = textBox2.Text.Trim();
            string password = textBox3.Text.Trim();
            string phone = textBox4.Text.Trim();
            string address = textBox5.Text.Trim();
            if(name == "" || password == "" || phone == "" || address == "")
            {
                MessageBox.Show("请将信息填写完整！","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            else
            {
                //step 1
                Regex nameCheck = new Regex("^[\u4e00-\u9fa5]{0,}$");
                //正则，验证汉字
                if (nameCheck.IsMatch(name) && name.Length > 1)
                {
                    //step 4
                    Regex passwordCheck = new Regex("^[0-9]*$");
                    //正则，验证数字
                    if (passwordCheck.IsMatch(password) && password.Length > 0)
                    {
                        //step 5
                        Regex phoneCheck1 = new Regex("(^1(3[4-9]|4[7]|5[0-27-9]|7[8]|8[2-478]|9[8])\\d{8}$)|(^1705\\d{7}$)");
                        //正则，验证移动手机号
                        Regex phoneCheck2 = new Regex("(^1(3[0-2]|4[5]|5[56]|6[6]|7[6]|8[56])\\d{8}$)|(^1709\\d{7}$)");
                        //正则，验证联通手机号
                        Regex phoneCheck3 = new Regex("(^1(33|53|77|99|8[019])\\d{8}$)|(^1700\\d{7}$)");
                        //正则，验证电信手机号
                        if (phoneCheck1.IsMatch(phone) || phoneCheck2.IsMatch(phone) || phoneCheck3.IsMatch(phone))
                        {
                            string sex = "";
                            if (radioButton1.Checked == true)
                                sex = "男";
                            else
                                sex = "女";
                            //step 6
                            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                            con.Open();
                            //step 7
                            SqlCommand com = new SqlCommand("insert into personInf (personName,personPassword,personLimit,personNum,personSex,personAddress,personPhoneNum) values ('" + name + "','" + password + "','admin','admin','" + sex + "','" + address + "','" + phone + "')", con);
                            com.ExecuteNonQuery();
                            con.Close();
                            MessageBox.Show("您已经成功添加了一个管理员账户：" + name, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //step 8
                            Settings1.Default.adminName = name;
                            Settings1.Default.tci1 = 0;
                            Settings1.Default.tci2 = 1;
                            Settings1.Default.tci3 = 2;
                            Settings1.Default.Save();
                            tabControl1.SelectedIndex = 3;
                        }
                        else
                            MessageBox.Show("手机号格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                        MessageBox.Show("密码格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("姓名格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
                radioButton2.Checked = false;
            else
                radioButton2.Checked = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
                radioButton1.Checked = false;
            else
                radioButton1.Checked = true;
        }
    }
}
