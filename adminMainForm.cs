using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
        public string updateName = "";
        public string updateNum = "";
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
            countlabel.Text = "";
            //调节dateGridView1的视觉效果
            dataGridView1.Columns[0].Width = 148;
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 160;
            dataGridView1.Columns[4].Width = 160;
            dataGridView1.Columns[5].Width = 160;
            dataGridView1.RowTemplate.Height = 207;

            //写入日志
            writeLog.writeProgramLog(string.Concat("使用管理员账号使用系统，", "使用人为：", Settings1.Default.adminName));
            //writeLog.writeProgramLog("登录系统后台，使用人：" + Settings1.Default.adminName.ToString());
            //statusTrip
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
            toolStripStatusLabel1.Text = "当前使用人：" + Settings1.Default.adminName.Trim();
            //读取Settings设置
            textBox1.Text = Settings1.Default.workerLastUseName.Trim();
            textBox2.Text = Settings1.Default.workerLastUseTime.Trim();
            textBox3.Text = Settings1.Default.workerLastUseNum.Trim();
            insertMalerad.Checked = true;
            /*
             * 判断之前是否有员工使用过
             */
            if (textBox1.Text != "" || textBox2.Text != "" || textBox3.Text != "")
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

        //方法：选择一张图片并插入
        private void insertPhotobtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择员工的照片";
            ofd.Filter = "图片文件(JPG，JPEG，BMP，PNG)|*.jpg;*.jpeg;*.bmp;*.png";
            if (ofd.ShowDialog() == DialogResult.OK && (openFileDialog1.FileName != ""))
            {
                insertpicb.ImageLocation = ofd.FileName;
            }
            ofd.Dispose();
        }

        
        //方法：选中一个另一个取消选中
        private void insertMalerad_CheckedChanged(object sender, EventArgs e)
        {
            if (insertMalerad.Checked == true) insertFemalerad.Checked = false;
            if (insertFemalerad.Checked == true) insertMalerad.Checked = false;
        }

        private void insertFemalerad_CheckedChanged(object sender, EventArgs e)
        {
            if (insertMalerad.Checked == true) insertFemalerad.Checked = false;
            if (insertFemalerad.Checked == true) insertMalerad.Checked = false;
        }

        //方法：插入员工信息
        private void insertbtn_Click(object sender, EventArgs e)
        {
            //
            //初始化变量
            string personName = insertNametxt.Text.Trim();
            string personNum = insertNumtxt.Text.Trim();
            string personAddress = insertAddresstxt.Text.Trim();
            string personPhoneNum = insertPhoneNumtxt.Text.Trim();
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
                if (nameCheck.IsMatch(personName) && personName.Length > 1)
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
                            if (insertMalerad.Checked == true)
                                personSex = "男";
                            else
                                personSex = "女";
                            if (insertFemalerad.Checked == true)
                                personSex = "女";
                            else
                                personSex = "男";
                            //step 5 手机号验证
                            if (personPhoneNum.Length < 11)
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
                                    if (insertpicb.ImageLocation == null)
                                    {
                                        MessageBox.Show("未找到员工照片，请添加后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        if(otxt.Text == "")
                                        {
                                            //默认直接添加
                                            string fullPath = insertpicb.ImageLocation;
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
                                            MessageBox.Show("您已经成功添加一名员工：" + personName + "。", "提示");
                                            deleteInf();
                                            DialogResult result = MessageBox.Show("是否在查询界面查看添加的员工信息？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                            if (result == DialogResult.Yes)
                                            {
                                                tabControl2.SelectedIndex = 0;
                                                searchtxt.Text = personName;
                                                searchbtn.PerformClick();
                                                searchtxt.Text = "";
                                                cleanupdate();
                                            }

                                        }
                                        else if(otxt.Text != "")
                                        {
                                            string fullPath = insertpicb.ImageLocation;
                                            string newPassword = updatePasswordtxt.Text.Trim();
                                            FileStream fs = new FileStream(fullPath, FileMode.Open);
                                            byte[] bytes = new byte[fs.Length];
                                            BinaryReader br = new BinaryReader(fs);
                                            bytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                                            SqlCommand comm = new SqlCommand("insert into personInf(personName,personPassword,personLimit,personNum,personSex,personAddress,personPhoneNum,personPhoto) values ('" + personName + "','" + newPassword + "','worker','" + personNum + "','" + personSex + "','" + personAddress + "','" + personPhoneNum + "',@ImageList)", con);
                                            comm.Parameters.Add("ImageList", SqlDbType.Image);
                                            comm.Parameters["ImageList"].Value = bytes;
                                            comm.ExecuteNonQuery();
                                            con.Close();
                                            MessageBox.Show("您已经成功添加一名员工：" + personName + "。", "提示");
                                            deleteInf();
                                            DialogResult result = MessageBox.Show("是否在查询界面查看添加的员工信息？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                            if (result == DialogResult.Yes)
                                            {
                                                tabControl2.SelectedIndex = 0;
                                                searchtxt.Text = personName;
                                                searchbtn.PerformClick();
                                                searchtxt.Text = "";
                                                cleanupdate();
                                            }
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
            insertNametxt.Text = "";
            insertNumtxt.Text = "";
            insertAddresstxt.Text = "";
            insertPhoneNumtxt.Text = "";
            insertMalerad.Checked = true;
            insertpicb.ImageLocation = null;
            searchtxt.Text = "";
        }
        private void visiblestatus()
        {
            gobtn.Visible = firstbtn.Visible = previousbtn.Visible = nextbtn.Visible = lastbtn.Visible = rownumbtn.Visible = true;
        }
        private void searchbtn_Click(object sender, EventArgs e)
        {
            rownumbtn.Text = "1";
            string search = searchtxt.Text.Trim();
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            if (search == "")
            {
                //简化查询
                visiblestatus();
                SqlCommand com1 = new SqlCommand("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where personLimit = 'worker'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from personInf where personLimit = 'worker'", con);
                //com2.ExecuteScalar();
                countlabel.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
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
                if (searchCheck.IsMatch(search))
                {
                    //查询的是工号
                    //构造Sql语句
                    visiblestatus();
                    SqlDataAdapter searchda = new SqlDataAdapter("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where (personNum like '%" + search + "%' and personLimit != 'admin')", con);
                    SqlCommand searchcom = new SqlCommand("select count(*) from personInf where (personNum like '%" + search + "%' and personLimit != 'admin')", con);
                    countlabel.Text = "共查询到" + searchcom.ExecuteScalar() + "条数据";
                    count = Convert.ToInt32(searchcom.ExecuteScalar());
                    if (count == 0)
                    {
                        rownumbtn.Text = "0";
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
                        countlabel.Text = "共查询到" + searchcom.ExecuteScalar() + "条数据";
                        count = Convert.ToInt32(searchcom.ExecuteScalar());
                        if (count == 0)
                        {
                            rownumbtn.Text = "0";
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
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture), dataGridView1.DefaultCellStyle.Font, sb, e.RowBounds.Location.X, e.RowBounds.Location.Y);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
        }

        private void gobtn_Click(object sender, EventArgs e)
        {
            //判断是否通过数据库查询到了值
            //count的值必定是 >= 0的
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (count > 0)
            {
                if (rownumbtn.Text.Trim() == "")
                {
                    //判断textBox10值为空值，如果是将其赋值为1
                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                    rownumbtn.Text = "1";
                }
                else
                {
                    //验证输入内容是否为数字
                    Regex numCheck = new Regex("^[0-9]*$");
                    if (numCheck.IsMatch(rownumbtn.Text.Trim()))
                    {
                        //通过验证
                        //判断是否超出了count的大小
                        if (Convert.ToInt32(rownumbtn.Text.Trim()) >= count)
                        {
                            //超出
                            rownumbtn.Text = count.ToString();
                            dataGridView1.CurrentCell = dataGridView1.Rows[count - 1].Cells[0];
                        }
                        else
                        {
                            //未超出
                            //判断输入值是否为0和1
                            if (rownumbtn.Text.Trim() == "0" || rownumbtn.Text.Trim() == "1")
                            {
                                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                                rownumbtn.Text = "1";
                            }
                            else
                            {
                                dataGridView1.CurrentCell = dataGridView1.Rows[Convert.ToInt32(rownumbtn.Text.Trim()) - 1].Cells[0];
                                rownumbtn.Text = (dataGridView1.CurrentRow.Index + 1).ToString();
                            }
                        }
                    }
                    else
                        MessageBox.Show("输入内容格式错误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void nextbtn_Click(object sender, EventArgs e)
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
                    rownumbtn.Text = count.ToString();
                }
                else
                {
                    //如果现行选中项的行索引不为count（还有可以增加的空间）
                    //变更现行选中项
                    dataGridView1.CurrentCell = dataGridView1.Rows[currentSelect + 1].Cells[0];
                    currentSelect = dataGridView1.CurrentRow.Index;
                    rownumbtn.Text = (currentSelect + 1).ToString();
                }
            }
        }

        private void lastbtn_Click(object sender, EventArgs e)
        {
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[count - 1].Cells[0];
                rownumbtn.Text = count.ToString();
            }
        }

        private void previousbtn_Click(object sender, EventArgs e)
        {
            //上一条
            //判断count是否为0
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //判断是否已经选到了第一条数据
                //给全局变量currentSelect赋值，令其与现行选中项的行索引相等
                currentSelect = dataGridView1.CurrentRow.Index;
                //此时currentSelect应该和textBox10的显示值相差1

                if (currentSelect == 0)
                {
                    rownumbtn.Text = "1";
                }
                else
                {
                    //如果现行选中项的行索引不为0（还有可以减少的空间）
                    //变更现行选中项
                    dataGridView1.CurrentCell = dataGridView1.Rows[currentSelect - 1].Cells[0];
                    rownumbtn.Text = currentSelect.ToString();
                }
            }
        }

        private void firstbtn_Click(object sender, EventArgs e)
        {
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                rownumbtn.Text = "1";
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                rownumbtn.Text = "0";
            }
            else
            {
                if (rownumbtn.Text == "0")
                {
                }
                else
                {
                    int currentIndex = dataGridView1.CurrentRow.Index;
                    rownumbtn.Text = (currentIndex + 1).ToString();
                }

            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //判断count的值，为0时不允许修改和删除
            if (count == 0)
            {
                if (contextMenuStrip1.Items[1].Selected == true || contextMenuStrip1.Items[2].Selected == true)
                {
                    contextMenuStrip1.Close();
                    MessageBox.Show("暂无选中项需要更改", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (contextMenuStrip1.Items[0].Selected == true)
                {
                    contextMenuStrip1.Close();
                    tabControl2.SelectedIndex = 1;
                }
            }
            else
            {
                if (contextMenuStrip1.Items[0].Selected == true)
                {
                    contextMenuStrip1.Close();
                    tabControl2.SelectedIndex = 1;
                }
                else if (contextMenuStrip1.Items[1].Selected == true)
                {
                    contextMenuStrip1.Close();
                    //获取当前选中行
                    currentSelect = dataGridView1.CurrentRow.Index;
                    //获取选中行中的员工数据
                    string updatepersonName = dataGridView1.Rows[currentSelect].Cells[1].Value.ToString();
                    string updatepersonNum = dataGridView1.Rows[currentSelect].Cells[3].Value.ToString();
                    string updatepersonSex = dataGridView1.Rows[currentSelect].Cells[2].Value.ToString();
                    string updatepersonPassword = dataGridView1.Rows[currentSelect].Cells[4].Value.ToString();
                    string updatepersonPhoneNum = dataGridView1.Rows[currentSelect].Cells[5].Value.ToString();
                    string updatepersonAddress = dataGridView1.Rows[currentSelect].Cells[6].Value.ToString();
                    updateName = updatepersonName;
                    updateNum = updatepersonNum;
                    otxt.Text = "您想要修改的员工信息如下：\r\n" + "姓名：" + updatepersonName + "\r\n" + "工号：" + updatepersonNum + "\r\n" + "性别：" + updatepersonSex + "\r\n" + "手机号：" + updatepersonPhoneNum + "\r\n" + "地址："  + updatepersonAddress + "\r\n" + "密码：" + updatepersonPassword;
                    //赋值
                    updateNametxt.Text = updatepersonName;
                    updateNumtxt.Text = updatepersonNum;
                    if (updatepersonSex == "男")
                    {
                        radioButton3.Checked = false;
                        radioButton4.Checked = true;
                    }
                    else
                    {
                        radioButton4.Checked = false;
                        radioButton3.Checked = true;
                    }
                    updatePhoneNumtxt.Text = updatepersonPhoneNum;
                    updateAddresstxt.Text = updatepersonAddress;
                    updatePasswordtxt.Text = updatepersonPassword;
                    //在opicb和updatepicb中显示照片
                    byte[] imagebytes = null;
                    string personNum = textBox3.Text;
                    SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                    con.Open();
                    SqlCommand com = new SqlCommand("select personPhoto from personInf where personNum = '" + updatepersonNum + "'", con);
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
                    opicb.Image = bmpt;
                    tabControl2.SelectedIndex = 1;
                    //修改
                }
                else if (contextMenuStrip1.Items[2].Selected == true)
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
                        SqlCommand deletecom = new SqlCommand("delete from personInf where personNum = '" + deletepersonNum + "'", deletecon);
                        deletecom.ExecuteScalar();
                        MessageBox.Show("您已成功删除" + deletepersonName + "的员工信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        deletecon.Close();
                        searchtxt.Text = "";
                        searchbtn.PerformClick();
                    }
                }
            }
        }

        private void updatebtn_Click(object sender, EventArgs e)
        {
            if(otxt.Text == "")
            {
                MessageBox.Show("请先选择一位要修改的员工！","提示",MessageBoxButtons.OK);
            }
            else
            {
                //初始化变量
                string updatepersonName = updateNametxt.Text.Trim();
                string updatepersonNum = updateNumtxt.Text.Trim();
                string updatepersonAddress = updateAddresstxt.Text.Trim();
                string updatepersonPhoneNum = updatePhoneNumtxt.Text.Trim();
                string updatepersonPassword = updatePasswordtxt.Text.Trim();
                string updatepersonSex = "男";
                if (radioButton3.Checked == true)
                {
                    updatepersonSex = "女";
                }
                else if (radioButton4.Checked == true)
                {
                    updatepersonSex = "男";
                }
                /*
                 * 数据验证
                 */
                if (updateNametxt.Text.Trim() == "" || updateNumtxt.Text.Trim() == "" || updatePhoneNumtxt.Text.Trim() == "" || updateAddresstxt.Text.Trim() == "" || updatePasswordtxt.Text.Trim() == "")
                {
                    MessageBox.Show("请将要修改的信息填充完整！", "警告", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
                else
                {
                    //验证姓名为汉字
                    Regex nameCheck = new Regex("^[\u4e00-\u9fa5]{0,}$");
                    if (nameCheck.IsMatch(updatepersonName) && updatepersonName.Length > 1)
                    {
                        //验证工号为8位数字
                        Regex numCheck = new Regex("^[0-9]*$");
                        if (numCheck.IsMatch(updatepersonNum) && updatepersonNum.Length == 8)
                        {
                            //验证手机号
                            Regex phoneCheck1 = new Regex("(^1(3[4-9]|4[7]|5[0-27-9]|7[8]|8[2-478]|9[8])\\d{8}$)|(^1705\\d{7}$)");
                            //正则，验证移动手机号
                            Regex phoneCheck2 = new Regex("(^1(3[0-2]|4[5]|5[56]|6[6]|7[6]|8[56])\\d{8}$)|(^1709\\d{7}$)");
                            //正则，验证联通手机号
                            Regex phoneCheck3 = new Regex("(^1(33|53|77|99|8[019])\\d{8}$)|(^1700\\d{7}$)");
                            //正则，验证电信手机号
                            if (phoneCheck1.IsMatch(updatepersonPhoneNum) || phoneCheck2.IsMatch(updatepersonPhoneNum) || phoneCheck3.IsMatch(updatepersonPhoneNum))
                            {
                                //验证密码为8位以下数字，不允许有其他字符
                                Regex passwordCheck = new Regex("^[0-9]*$");
                                if (passwordCheck.IsMatch(updatepersonPassword) && updatepersonPassword.Length < 9)
                                {

                                    /*
                                     * 分情况执行代码
                                     * 
                                     * 1.1工号和姓名均未发生变化，以工号为条件
                                     * 1.2工号未发生变化，姓名发生了变化，以工号为条件
                                     * 
                                     * 2.工号发生变化，姓名没有发生变化，以姓名为条件
                                     * 
                                     * 3.姓名和工号均发生了变化
                                     * 暂定的方法为先删除再创建
                                     */
                                    //打开数据库连接
                                    SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                                    con.Open();
                                    //1.以工号为条件
                                    if (updateNum == updatepersonNum)
                                    {
                                        //构造Sql语句
                                        SqlCommand com = new SqlCommand("update personInf set personName = '" + updatepersonName + "',personSex = '" + updatepersonSex + "',personPassword = '" + updatepersonPassword + "',personPhoneNum = '" + updatepersonPhoneNum + "',personAddress = '" + updatepersonAddress + "' where personNum = '" + updatepersonNum + "'",con);
                                        com.ExecuteNonQuery();
                                        con.Close();
                                        MessageBox.Show("您已经成功修改了一名员工的信息","提示",MessageBoxButtons.OK);
                                        cleanupdate();
                                        //跳转修改后的结果
                                        tabControl2.SelectedIndex = 0;
                                        searchtxt.Text = updatepersonNum;
                                        searchbtn.PerformClick();
                                        searchtxt.Text = "";
                                    }
                                    //2.以姓名为条件
                                    else if (updateName == updatepersonName)
                                    {
                                        string personNumCheck = updateNumtxt.Text.Trim();
                                        //判断是否已经存在了有工号相同的员工
                                        SqlCommand judgecom = new SqlCommand("select count(*) from personInf where personNum = '" + personNumCheck + "'", con);
                                        int numRet = (int)judgecom.ExecuteScalar();
                                        if (numRet > 0)
                                            MessageBox.Show("已经存在工号重复的员工，请更改工号后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        else
                                        {
                                            SqlCommand com = new SqlCommand("update personInf set personNum = '" + updatepersonNum + "',personSex = '" + updatepersonSex + "',personPassword = '" + updatepersonPassword + "',personPhoneNum = '" + updatepersonPhoneNum + "',personAddress = '" + updatepersonAddress + "' where personName = '" + updatepersonName + "'", con);
                                            com.ExecuteNonQuery();
                                            con.Close();
                                            MessageBox.Show("您已经成功修改了一名员工的信息", "提示", MessageBoxButtons.OK);
                                            cleanupdate();
                                            tabControl2.SelectedIndex = 0;
                                            searchtxt.Text = updatepersonName;
                                            searchbtn.PerformClick();
                                            searchtxt.Text = "";
                                        }
                                    }
                                    //3.删除后添加
                                    else if (updateName != updatepersonName && updateNum != updatepersonNum)
                                    {
                                        DialogResult result = MessageBox.Show("您同时修改了员工的姓名和工号，如果您确定修改请先删除此条信息另行添加", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                                        if(result == DialogResult.OK)
                                        {
                                            insertNametxt.Text = updatepersonName;
                                            insertNumtxt.Text = updatepersonNum;
                                            if(radioButton3.Checked == true)
                                            {
                                                insertFemalerad.Checked = true;
                                                insertMalerad.Checked = false;
                                            }
                                            else if(radioButton4.Checked == true)
                                            {
                                                insertMalerad.Checked = true;
                                                insertFemalerad.Checked = false;
                                            }
                                            insertPhoneNumtxt.Text = updatepersonPhoneNum;
                                            insertAddresstxt.Text = updatepersonAddress;

                                            SqlConnection deletecon = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                                            deletecon.Open();
                                            SqlCommand deletecom = new SqlCommand("delete from personInf where personNum = '" + updateNum + "'", deletecon);
                                            deletecom.ExecuteScalar();
                                            deletecon.Close();
                                            searchtxt.Text = "";
                                            searchbtn.PerformClick();
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("密码格式不正确，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("手机号格式不正确，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                        }
                        else
                        {
                            MessageBox.Show("工号格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("姓名格式有误，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void cleanupdate()
        {
            opicb.Image = null;
            otxt.Text = "";
            //opicb.ImageLocation = updatepicb.ImageLocation = null;
            updateNametxt.Text = updateNumtxt.Text = updatePasswordtxt.Text = updatePhoneNumtxt.Text = updateAddresstxt.Text = "";
        }
    }
}
