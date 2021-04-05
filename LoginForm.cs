using System;
using System.Data.SqlClient;
using System.Windows.Forms;

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
            //创建变量
            string personName = textBox1.Text.Trim();
            string personPassword = textBox2.Text.Trim();
            string personNum = "";
            //构造查询语句
            string sqlLang1 = String.Format("select count(*) from personInf where personName = '{0}' and personPassword = '{1}'", personName, personPassword);
            SqlCommand com = new SqlCommand(sqlLang1, con);

            //验证是否有与输入内容一致的列
            int numCheck = (int)com.ExecuteScalar();

            //有查询结果
            if (numCheck > 0)
            {
                string personLimit = "";
                string sqllang2 = "select personLimit,personNum from personInf where personName = '" + personName + "'";
                SqlCommand limitCheck = new SqlCommand(sqllang2, con);
                //读取查询的结果
                SqlDataReader reader = limitCheck.ExecuteReader();
                while (reader.Read())
                {
                    personLimit = reader["personLimit"].ToString().Trim();
                    personNum = reader["personNum"].ToString().Trim();
                }
                //判断登录人身份
                if (personLimit == "admin")
                {
                    Settings1.Default.Save();
                    MessageBox.Show("欢迎，" + personName + "！您拥有管理员权限，即将为您打开后台控制程序。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    adminMainForm amf = new adminMainForm();
                    Dispose();
                    this.Close();
                    amf.ShowDialog();
                }
                else if (personLimit == "worker")
                {
                    Settings1.Default.workerLastUseName = Settings1.Default.nowUser = personName;
                    Settings1.Default.workerLastUseTime = DateTime.Now.ToString();
                    Settings1.Default.workerLastUseNum = personNum;
                    Settings1.Default.Save();
                    MessageBox.Show("欢迎，" + personName + "!", "提示");
                    workerMainForm wmf = new workerMainForm();
                    Dispose();
                    this.Close();
                    wmf.ShowDialog();
                }
            }
            //无查询结果
            else
            {
                MessageBox.Show("请确认您输入的姓名和密码是否正确！请在确认后重新尝试登录！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Text = textBox2.Text = "";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("忘记密码请联系管理员！", "提示");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tiaoshi ts = new tiaoshi();
            ts.Show();
        }
    }
}
