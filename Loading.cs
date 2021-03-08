using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

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
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            label1.Text = "打开连接";
            SqlCommand selectcom = new SqlCommand("select COUNT(*) from connecttest where col1 = 'a'",con);
            int numCheck = (int)selectcom.ExecuteScalar();
            if (numCheck > 0)
            {
                label1.Text = "数据库连接正常";
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
                label1.Text = "数据库连接失败，请检查数据库状态";
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
