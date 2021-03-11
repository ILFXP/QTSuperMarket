using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace QTSuperMarket
{
    public partial class adminMainForm : Form
    {
        public adminMainForm()
        {
            InitializeComponent();
        }
        //定义全局变量
        public int count = 0;
        public int currentSelect = 0;
        public string editpersonNum = "";
        private void adminMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
             * 读取本地设置中的配置
             * 判断是否开启以下功能：
             * 1.关闭程序是清理SQL Server Management Studio客户端
             * 2.退出确认
             */
            if (Settings1.Default.cleanSSMS == true)
            {
                if (Settings1.Default.quiteCheck == true)
                {
                    //打开-打开
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
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    //打开-关闭
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
            }
            else
            {
                if (Settings1.Default.quiteCheck == true)
                {
                    //关闭-打开
                    DialogResult result = MessageBox.Show("是否退出程序？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (result == DialogResult.OK)
                    {
                        Dispose();
                        Application.Exit();
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    //关闭-关闭
                    Dispose();
                    Application.Exit();
                }
            }
        }

        private void adminMainForm_Load(object sender, EventArgs e)
        {
            label11.Text = "";
            //调节dateGridView1的视觉效果
            dataGridView1.Columns[0].Width = 148;
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 160;
            dataGridView1.Columns[4].Width = 160;
            dataGridView1.Columns[5].Width = 160;
            dataGridView1.RowTemplate.Height = 207;

            //写入日志
            writeLog.writeProgramLog(string.Concat("使用管理员账号使用系统，","使用人为：",Settings1.Default.adminName));
            //writeLog.writeProgramLog("登录系统后台，使用人：" + Settings1.Default.adminName.ToString());
            //statusTrip
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
            toolStripStatusLabel1.Text = "当前使用人：" + Settings1.Default.adminName.Trim();
            //读取Settings设置
            textBox1.Text = Settings1.Default.workerLastUseName.Trim();
            textBox2.Text = Settings1.Default.workerLastUseTime.Trim();
            textBox3.Text = Settings1.Default.workerLastUseNum.Trim();
            radioButton1.Checked = true;
            /*
             * 判断之前是否有员工使用过
             */
            if (textBox1.Text != "" || textBox2.Text !=  "" || textBox3.Text != "")
            {
                /*
                 * 当配置中的上次使用员工、上次试用员工工号、上次使用时间不为空值时
                 * 执行SQL语句，查询上次使用人的照片并在pictureBox1显示
                 */
                byte[] imagebytes = null;
                string personNum = textBox3.Text;
                SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                con.Open();
                SqlCommand com = new SqlCommand("select personPhoto from personInf where personNum = '" + personNum + "'", con);
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    imagebytes = (byte[])dr.GetValue(0);
                }
                dr.Close();
                com.Clone();
                con.Close();
                MemoryStream ms = new MemoryStream(imagebytes);
                Bitmap bmpt = new Bitmap(ms);
                pictureBox1.Image = bmpt;
            }
        }

        private void checkBox1_MouseHover(object sender, EventArgs e)
        {
            label8.Text = "-自动生成员工工号-";
            textBox7.Text = "开启-自动生成员工工号-功能后，在进行员工信息的添加时程序会以一定的算法自动生成员工工号以保证工号作为主键时的唯一性。";
        }

        private void checkBox2_MouseHover(object sender, EventArgs e)
        {
            label8.Text = "-退出软件时关闭数据库-";
            textBox7.Text = "开启-退出软件时关闭数据库-功能后，程序会检测SQL Server Management Studio的运行状态并在退出软件时将之关闭以保证数据的安全。";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择员工的照片";
            ofd.Filter = "图片文件(JPG，JPEG，BMP，PNG)|*.jpg;*.jpeg;*.bmp;*.png";
            if (ofd.ShowDialog() == DialogResult.OK && (openFileDialog1.FileName != ""))
            {
                pictureBox2.ImageLocation = ofd.FileName;
            }
            ofd.Dispose();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true) radioButton2.Checked = false;
            if (radioButton2.Checked == true) radioButton1.Checked = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true) radioButton2.Checked = false;
            if (radioButton2.Checked == true) radioButton1.Checked = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //
            //初始化变量
            string personName = textBox4.Text.Trim();
            string personNum = textBox5.Text.Trim();
            string personAddress = textBox8.Text.Trim();
            string personPhoneNum = textBox9.Text.Trim();
            string personSex = "男";

            //step 1 进行非空验证
            if (personName == "" || personNum == "" || personAddress == "" || personPhoneNum == "")
            {
                MessageBox.Show("请先将员工信息补充完整！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //step 2 姓名验证
                Regex nameCheck = new Regex("^[\u4e00-\u9fa5]{0,}$");
                if (nameCheck.IsMatch(personName))
                {
                    //step 3 工号验证
                    //3.1 数字验证
                    Regex numCheck = new Regex("^[0-9]*$");
                    if (numCheck.IsMatch(personNum) && personNum.Length == 8)
                    {
                        //3.2 重复验证
                        SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                        con.Open();
                        SqlCommand com = new SqlCommand("select count(*) from personInf where personNum = '" + personNum + "'", con);
                        int numRet = (int)com.ExecuteScalar();
                        if (numRet > 0)
                            MessageBox.Show("已经存在工号重复的员工，请更改工号后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                        {
                            //step 4 添加性别
                            if (radioButton1.Checked == true)
                                personSex = "男";
                            else
                                personSex = "女";
                            if (radioButton2.Checked == true)
                                personSex = "女";
                            else
                                personSex = "男";
                            //step 5 手机号验证
                            if (personPhoneNum.Length < 11 || personPhoneNum.Length > 11)
                            {
                                MessageBox.Show("手机号不完整，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                Regex phoneCheck1 = new Regex("(^1(3[4-9]|4[7]|5[0-27-9]|7[8]|8[2-478]|9[8])\\d{8}$)|(^1705\\d{7}$)");
                                //正则，验证移动手机号
                                Regex phoneCheck2 = new Regex("(^1(3[0-2]|4[5]|5[56]|6[6]|7[6]|8[56])\\d{8}$)|(^1709\\d{7}$)");
                                //正则，验证联通手机号
                                Regex phoneCheck3 = new Regex("(^1(33|53|77|99|8[019])\\d{8}$)|(^1700\\d{7}$)");
                                //正则，验证电信手机号
                                if (phoneCheck1.IsMatch(personPhoneNum) || phoneCheck2.IsMatch(personPhoneNum) || phoneCheck3.IsMatch(personPhoneNum))
                                {
                                    if(pictureBox2.ImageLocation == null)
                                    {
                                        MessageBox.Show("未找到员工照片，请添加后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        string fullPath = pictureBox2.ImageLocation;
                                        string defaultPassword = Settings1.Default.defaultPassword;
                                        FileStream fs = new FileStream(fullPath, FileMode.Open);
                                        byte[] bytes = new byte[fs.Length];
                                        BinaryReader br = new BinaryReader(fs);
                                        bytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                                        SqlCommand comm = new SqlCommand("insert into personInf(personName,personPassword,personLimit,personNum,personSex,personAddress,personPhoneNum,personPhoto) values ('" + personName + "','" + defaultPassword + "','worker','" + personNum + "','" + personSex + "','" + personAddress + "','" + personPhoneNum + "',@ImageList)", con);
                                        comm.Parameters.Add("ImageList", SqlDbType.Image);
                                        comm.Parameters["ImageList"].Value = bytes;
                                        comm.ExecuteNonQuery();
                                        con.Close();
                                        MessageBox.Show("您已经成功添加一名员工：" + personName + "。","提示");
                                        deleteInf();
                                        DialogResult result = MessageBox.Show("是否在查询界面查看添加的员工信息？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                        if (result == DialogResult.Yes)
                                        {
                                            tabControl2.SelectedIndex = 0;
                                            textBox6.Text = personName;
                                            button3.PerformClick();
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("手机号格式不正确，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("工号格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("姓名格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void deleteInf()
        {
            textBox4.Text = "";
            textBox5.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            radioButton1.Checked = true;
            pictureBox2.ImageLocation = null;
            textBox6.Text = "";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
        }
        private void visiblestatus()
        {
            button4.Visible = button5.Visible = button6.Visible = button7.Visible = button8.Visible = textBox10.Visible = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            textBox10.Text = "1";
            string search = textBox6.Text.Trim();
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            if (search == "")
            {
                //简化查询
                visiblestatus();
                SqlCommand com1 = new SqlCommand("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where personLimit = 'worker'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from personInf where personLimit = 'worker'", con);
                //com2.ExecuteScalar();
                label11.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                count = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                SqlCommandBuilder bu = new SqlCommandBuilder(da);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "personInf");
                dataGridView1.DataSource = ds.Tables["personInf"];
                con.Close();
            }
            else
            {
                //通过验证
                //判断输入内容是否为数字--工号
                Regex searchCheck = new Regex("^[0-9]*[1-9][0-9]*$");
                if(searchCheck.IsMatch(search)){
                    //查询的是工号
                    //构造Sql语句
                    visiblestatus();
                    SqlDataAdapter searchda = new SqlDataAdapter("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where (personNum like '%" + search + "%' and personLimit != 'admin')",con);
                    SqlCommand searchcom = new SqlCommand("select count(*) from personInf where (personNum like '%" + search + "%' and personLimit != 'admin')",con);
                    label11.Text = "共查询到" + searchcom.ExecuteScalar() + "条数据";
                    count = Convert.ToInt32(searchcom.ExecuteScalar());
                    if (count == 0)
                    {
                        textBox10.Text = "0";
                    }
                    DataSet searchds = new DataSet();
                    searchds.Clear();
                    searchda.Fill(searchds);
                    DataTable searchdt = searchds.Tables[0];
                    dataGridView1.DataSource = searchdt.DefaultView;
                    con.Close();
                    
                }
                else
                {
                    //查询的是姓名
                    //验证是否为汉字
                    Regex nameCheck = new Regex("^[\u4e00-\u9fa5]{0,}$");
                    if (nameCheck.IsMatch(search))
                    {
                        //通过验证
                        //构造SQL语句
                        visiblestatus();
                        SqlDataAdapter searchda = new SqlDataAdapter("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where (personName like '%" + search + "%' and personLimit != 'admin')", con);
                        SqlCommand searchcom = new SqlCommand("select count(*) from personInf where (personName like '%" + search + "%' and personLimit != 'admin')", con);
                        label11.Text = "共查询到" + searchcom.ExecuteScalar() + "条数据";
                        count = Convert.ToInt32(searchcom.ExecuteScalar());
                        if(count == 0)
                        {
                            textBox10.Text = "0";
                        }
                        DataSet searchds = new DataSet();
                        searchds.Clear();
                        searchda.Fill(searchds);
                        DataTable searchdt = searchds.Tables[0];
                        dataGridView1.DataSource = searchdt.DefaultView;
                        con.Close();
                    }
                    else
                        MessageBox.Show("请检查输入是否正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            //为dataGridView1自动添加序号
            SolidBrush sb = new SolidBrush(dataGridView1.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture),dataGridView1.DefaultCellStyle.Font,sb,e.RowBounds.Location.X,e.RowBounds.Location.Y);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //判断是否通过数据库查询到了值
            //count的值必定是 >= 0的
            if(count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(count > 0){
                if (textBox10.Text.Trim() == "")
                {
                    //判断textBox10值为空值，如果是将其赋值为1
                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                    textBox10.Text = "1";
                }
                else
                {
                    //验证输入内容是否为数字
                    Regex numCheck = new Regex("^[0-9]*$");
                    if (numCheck.IsMatch(textBox10.Text.Trim()))
                    {
                        //通过验证
                        //判断是否超出了count的大小
                        if(Convert.ToInt32(textBox10.Text.Trim()) >= count)
                        {
                            //超出
                            textBox10.Text = count.ToString();
                            dataGridView1.CurrentCell = dataGridView1.Rows[count -1].Cells[0];
                        }
                        else
                        {
                            //未超出
                            //判断输入值是否为0和1
                            if(textBox10.Text.Trim() == "0" || textBox10.Text.Trim() == "1")
                            {
                                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                                textBox10.Text = "1";
                            }
                            else
                            {
                                dataGridView1.CurrentCell = dataGridView1.Rows[Convert.ToInt32(textBox10.Text.Trim()) - 1].Cells[0];
                                textBox10.Text = (dataGridView1.CurrentRow.Index + 1).ToString();
                            }
                        }
                    }
                    else
                        MessageBox.Show("输入内容格式错误，请检查后重试！","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //下一条
            //判断count是否为0
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //判断是否已经选到了最后一条数据
                //给全局变量currentSelect赋值，令其与现行选中项的行索引相等
                currentSelect = dataGridView1.CurrentRow.Index;
                int countNum = count - 1;
                if (currentSelect == countNum)
                {
                    //现行选中项的行索引已将增加到了和count一样了
                    //实际的currentSelect的值应该要比count小1
                    //给textBox10赋值为和count一样
                    textBox10.Text = count.ToString();
                }
                else
                {
                    //如果现行选中项的行索引不为count（还有可以增加的空间）
                    //变更现行选中项
                    dataGridView1.CurrentCell = dataGridView1.Rows[currentSelect + 1].Cells[0];
                    currentSelect = dataGridView1.CurrentRow.Index;
                    textBox10.Text = (currentSelect + 1).ToString();
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if(count == 0)
            {
                MessageBox.Show("无查询结果！","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            else
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[count - 1].Cells[0];
                textBox10.Text = count.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //上一条
            //判断count是否为0
            if(count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //判断是否已经选到了第一条数据
                //给全局变量currentSelect赋值，令其与现行选中项的行索引相等
                currentSelect = dataGridView1.CurrentRow.Index;
                //此时currentSelect应该和textBox10的显示值相差1

                if(currentSelect == 0)
                {
                    textBox10.Text = "1";
                }
                else
                {
                    //如果现行选中项的行索引不为0（还有可以减少的空间）
                    //变更现行选中项
                    dataGridView1.CurrentCell = dataGridView1.Rows[currentSelect - 1].Cells[0];
                    textBox10.Text = currentSelect.ToString();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                textBox10.Text = "1";
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridView1.Rows.Count == 0)
            {
                textBox10.Text = "0";
            }
            else
            {
                if(textBox10.Text == "0")
                {
                }
                else
                {
                    int currentIndex = dataGridView1.CurrentRow.Index;
                    textBox10.Text = (currentIndex + 1).ToString();
                }
                
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //判断count的值，为0时不允许修改和删除
            if(count == 0)
            {
                if(contextMenuStrip1.Items[1].Selected == true || contextMenuStrip1.Items[2].Selected == true)
                {
                    contextMenuStrip1.Close();
                    MessageBox.Show("暂无选中项需要更改","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                else if(contextMenuStrip1.Items[0].Selected == true)
                {
                    contextMenuStrip1.Close();
                    tabControl2.SelectedIndex = 1;
                }
            }
            else
            {
                if(contextMenuStrip1.Items[0].Selected == true)
                {
                    contextMenuStrip1.Close();
                    tabControl2.SelectedIndex = 1;
                }
                else if(contextMenuStrip1.Items[1].Selected == true)
                {
                    contextMenuStrip1.Close();
                    //获取当前选中行
                    currentSelect = dataGridView1.CurrentRow.Index;
                    //获取选中行中的工号数据
                    editpersonNum = dataGridView1.Rows[currentSelect].Cells[3].Value.ToString();
                    tabControl2.SelectedIndex = 2;
                    //修改
                }
                else if(contextMenuStrip1.Items[2].Selected == true)
                {
                    contextMenuStrip1.Close();
                    //获取当前选中行
                    currentSelect = dataGridView1.CurrentRow.Index;
                    //获取选中行中的工号数据
                    string deletepersonName = dataGridView1.Rows[currentSelect].Cells[1].Value.ToString();
                    string deletepersonNum = dataGridView1.Rows[currentSelect].Cells[3].Value.ToString();
                    string deletepersonSex = dataGridView1.Rows[currentSelect].Cells[2].Value.ToString();
                    string deletepersonPhoneNum = dataGridView1.Rows[currentSelect].Cells[5].Value.ToString();
                    string deleteperAddress = dataGridView1.Rows[currentSelect].Cells[6].Value.ToString();
                    DialogResult result = MessageBox.Show("您想要删除的员工信息如下：\n" + "姓名：" + deletepersonName + "\n" + "性别：" + deletepersonSex + "\n" + "工号：" + deletepersonNum + "\n" + "手机：" + deletepersonPhoneNum + "\n" + "地址：" + deleteperAddress + "\n" + "确定删除吗？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (result == DialogResult.OK)
                    {
                        SqlConnection deletecon = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                        deletecon.Open();
                        SqlCommand deletecom = new SqlCommand("delete from personInf where personNum = '" + deletepersonNum + "'",deletecon);
                        deletecom.ExecuteScalar();
                        MessageBox.Show("您已成功删除" + deletepersonName + "的员工信息！","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                        deletecon.Close();
                        textBox6.Text = "";
                        button3.PerformClick();
                    }
                }
            }
        }
    }
}
