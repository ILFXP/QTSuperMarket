using System;
using System.Windows.Forms;

namespace QTSuperMarket
{
    public partial class workerMainForm : Form
    {
        public workerMainForm()
        {
            InitializeComponent();
        }
        //定义全局变量
        public string workername = "测试员工";

        private void workerMainForm_Load(object sender, EventArgs e)
        {
            //执行方法
            timer1.Start();
            //设置属性
            this.Text += workername;
            monthCalendar1.Visible = false;
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            toolStripStatusLabel1.Text = "当前使用人：" + workername;
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
        }

        private void stockNamecob_TextChanged(object sender, EventArgs e)
        {
            string str1 = stockNamecob.Text.Trim();
            string str2 = DateTime.Now.ToShortDateString().Replace("/", "");
            string str3 = DateTime.Now.ToLongTimeString().Replace(":", "");
            stockIdtxt.Text = str1 + str2 + "01" + str3;
        }

        private void noBarCodecb_CheckedChanged(object sender, EventArgs e)
        {
            if (noBarCodecb.Checked == true)
            {
                stockBarCodetxt.Enabled = false;
            }
            else
            {
                stockBarCodetxt.Enabled = true;
            }  
        }

        private void dateChoose_Click(object sender, EventArgs e)
        {
            monthCalendar1.Visible = true;
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            string chooseDate = monthCalendar1.SelectionStart.ToShortDateString();
            string todayDate = DateTime.Now.ToShortDateString();
            //这是选中的日期
            DateTime d1 = Convert.ToDateTime(chooseDate);
            //这是今天的日期
            DateTime d2 = Convert.ToDateTime(todayDate);
            int compare = DateTime.Compare(d1,d2);
            if (compare > 0)
            {
                MessageBox.Show("请注意，您选择的-生产日期-大于今天的日期\r\n这是不合常理的，请检查后重试","错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            monthCalendar1.Hide();
            domtxt.Text = monthCalendar1.SelectionStart.ToLongDateString();
        }
    }
}
