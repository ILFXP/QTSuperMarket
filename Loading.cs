using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QTSuperMarket
{
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void Loading_Load(object sender, EventArgs e)
        {
            //日志
            writeLog.writeProgramLog("程序启动");
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            writeLog.writeProgramLog("测试数据库连接");
            SqlCommand selectcom = new SqlCommand("select COUNT(*) from connecttest where col1 = 'a'", con);
            int numCheck = (int)selectcom.ExecuteScalar();
            if (numCheck > 0)
            {
                writeLog.writeProgramLog("数据库连接正常");
                con.Close();
                if (Settings1.Default.skipGuide == true)
                {
                    LoginForm lf = new LoginForm();
                    lf.ShowDialog();
                    this.Close();
                }
                else if (Settings1.Default.skipGuide == false)
                {
                    GuideForm gf = new GuideForm();
                    gf.ShowDialog();
                    this.Close();
                }
            }
            else
            {
                con.Close();
                writeLog.writeProgramLog("错误：数据库连接失败");
                label1.Text = "数据库连接失败，请检查数据库状态";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            writeLog.writeProgramLog("退出程序");
            Application.Exit();
        }
    }
}
