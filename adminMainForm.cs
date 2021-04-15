using System;
using System.Collections.Generic;
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
        /*定义一部分全局变量*/

        //查询到的总数
        public int count = 0;
        public int stockCount = 0;
        //当前选中项
        public int currentSelect = 0;
        //当前选中页
        public int currentSelectPage = 0;
        //编辑的员工工号
        public string editpersonNum = "";
        //要修改的员工姓名
        public string updateName = "";
        //要修改的员工工号
        public string updateNum = "";

        //库存的剩余保质期
        public string stockExtime1 = "";
        public string stockExtime2 = "";
        //库存的过期日期
        public string stockExDate = "";
        //库存的当前状态
        public string stockExState = "";
        public string insertDate2 = "";

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
        private void groupBox4Hide()
        {
            groupBox4.Visible = false;
        }
        private void adminMainForm_Load(object sender, EventArgs e)
        {
            /*临时使用*/
            Settings1.Default.nowUser = "测试";
            Settings1.Default.Save();

            //调节控件属性
            stockDomtxt.Text = DateTime.Now.ToLongDateString();
            countlabel.Text = "";
            monthCalendar1.Hide();
            overduerd.Checked = true;
            haveQgprd.Checked = true;

            //调节dateGridView1的视觉效果
            dataGridView1.Columns[0].Width = 148;
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 160;
            dataGridView1.Columns[4].Width = 160;
            dataGridView1.Columns[5].Width = 160;
            dataGridView1.RowTemplate.Height = 207;
            //调节dateGridView2的视觉效果
            
            dataGridView2.Columns[0].Width = 60;
            dataGridView2.Columns[1].Width = 300;
            dataGridView2.Columns[2].Width = 160;
            dataGridView2.Columns[3].Width = 160;
            dataGridView2.Columns[4].Width = 160;
            dataGridView2.Columns[5].Width = 160;
            dataGridView2.Columns[6].Width = 170;
            dataGridView2.RowTemplate.Height = 86;

            //写入日志
            writeLog.writeProgramLog(string.Concat("使用管理员账号使用系统，", "使用人为：", Settings1.Default.adminName));
            //writeLog.writeProgramLog("登录系统后台，使用人：" + Settings1.Default.adminName.ToString());
            //statusTrip
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
            toolStripStatusLabel1.Text = "当前使用人：" + Settings1.Default.nowUser.Trim();
            //读取Settings设置
            textBox1.Text = Settings1.Default.workerLastUseName.Trim();
            textBox2.Text = Settings1.Default.workerLastUseTime.Trim();
            textBox3.Text = Settings1.Default.workerLastUseNum.Trim();
            insertMalerad.Checked = true;

            //执行方法
            mainCategortcomBox();
            numUnitcomBox();
            qgpUnitcomBox();
            stockNamecomBox();
            stockBrandcomBox();
            noSort();
            groupBox4Hide();
            insertPersoncomBox();

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
        private void visiblestate()
        {
            gobtn.Visible = firstbtn.Visible = previousbtn.Visible = nextbtn.Visible = lastbtn.Visible = rownumtxt.Visible = true;
        }
        private void searchbtn_Click(object sender, EventArgs e)
        {
            rownumtxt.Text = "1";
            string search = searchtxt.Text.Trim();
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            if (search == "")
            {
                //简化查询
                visiblestate();
                SqlCommand com1 = new SqlCommand("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where personLimit = 'worker'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from personInf where personLimit = 'worker'", con);
                //com2.ExecuteScalar();
                countlabel.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                count = Convert.ToInt32(com2.ExecuteScalar());
                if (count == 0)
                {
                    rownumtxt2.Text = "0";
                    con.Close();
                }
                else
                {
                    SqlDataAdapter da = new SqlDataAdapter(com1);
                    SqlCommandBuilder bu = new SqlCommandBuilder(da);
                    DataSet ds = new DataSet();
                    ds.Clear();
                    da.Fill(ds, "personInf");
                    dataGridView1.DataSource = ds.Tables["personInf"];
                    con.Close();
                }
            }
            else
            {
                //通过验证
                //判断输入内容是否为数字--工号
                Regex searchCheck = new Regex("^(0|[1-9][0-9]*)$");
                if (searchCheck.IsMatch(search))
                {
                    //查询的是工号
                    //构造Sql语句
                    visiblestate();
                    SqlDataAdapter searchda = new SqlDataAdapter("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where (personNum like '%" + search + "%' and personLimit != 'admin')", con);
                    SqlCommand searchcom = new SqlCommand("select count(*) from personInf where (personNum like '%" + search + "%' and personLimit != 'admin')", con);
                    countlabel.Text = "共查询到" + searchcom.ExecuteScalar() + "条数据";
                    count = Convert.ToInt32(searchcom.ExecuteScalar());
                    if (count == 0)
                    {
                        rownumtxt.Text = "0";
                        DataSet searchds = new DataSet();
                        searchds.Clear();
                        searchda.Fill(searchds);
                        DataTable searchdt = searchds.Tables[0];
                        dataGridView1.DataSource = searchdt.DefaultView;
                        con.Close();
                        con.Close();
                    }
                    else
                    {
                        DataSet searchds = new DataSet();
                        searchds.Clear();
                        searchda.Fill(searchds);
                        DataTable searchdt = searchds.Tables[0];
                        dataGridView1.DataSource = searchdt.DefaultView;
                        con.Close();
                    }
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
                        visiblestate();
                        SqlDataAdapter searchda = new SqlDataAdapter("select personPhoto,personName,personSex,personNum,personPassword,personPhoneNum,personAddress from personInf where (personName like '%" + search + "%' and personLimit != 'admin')", con);
                        SqlCommand searchcom = new SqlCommand("select count(*) from personInf where (personName like '%" + search + "%' and personLimit != 'admin')", con);
                        countlabel.Text = "共查询到" + searchcom.ExecuteScalar() + "条数据";
                        count = Convert.ToInt32(searchcom.ExecuteScalar());
                        if (count == 0)
                        {
                            rownumtxt.Text = "0";
                            DataSet searchds = new DataSet();
                            searchds.Clear();
                            searchda.Fill(searchds);
                            DataTable searchdt = searchds.Tables[0];
                            dataGridView1.DataSource = searchdt.DefaultView;
                            con.Close();
                            con.Close();
                        }
                        else
                        {
                            DataSet searchds = new DataSet();
                            searchds.Clear();
                            searchda.Fill(searchds);
                            DataTable searchdt = searchds.Tables[0];
                            dataGridView1.DataSource = searchdt.DefaultView;
                            con.Close();
                        }
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
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture),dataGridView1.DefaultCellStyle.Font, sb, e.RowBounds.Location.X, e.RowBounds.Location.Y);
        }
        private void noSort()
        {
            //1
            for(int i = 0;i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            //2
            for (int j = 0; j < dataGridView2.Columns.Count; j++)
            {
                dataGridView2.Columns[j].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = "当前时间：" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
            insertDate2 = DateTime.Now.ToLongDateString();
            insertStockPersonNametxt.Text = Settings1.Default.nowUser;
            insertStockDateTimetxt.Text = DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
        }

        private void gobtn_Click(object sender, EventArgs e)
        {
            //count的值必定是 >= 0的
            if (count == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (count > 0)
            {
                if (rownumtxt.Text.Trim() == "")
                {
                    //判断rownumtxt值是否为空值，如果是将其赋值为1
                    dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                    rownumtxt.Text = "1";
                }
                else
                {
                    //验证输入内容是否为数字
                    Regex numCheck = new Regex("^[0-9]*$");
                    if (numCheck.IsMatch(rownumtxt.Text.Trim()))
                    {
                        //通过验证
                        //判断是否超出了count的大小
                        if (Convert.ToInt32(rownumtxt.Text.Trim()) >= count)
                        {
                            //超出
                            dataGridView1.CurrentCell = dataGridView1.Rows[count - 1].Cells[0];
                            rownumtxt.Text = count.ToString();
                        }
                        else
                        {
                            //未超出
                            //判断输入值是否为0和1
                            if (rownumtxt.Text.Trim() == "0" || rownumtxt.Text.Trim() == "1")
                            {
                                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                                rownumtxt.Text = "1";
                            }
                            else
                            {
                                dataGridView1.CurrentCell = dataGridView1.Rows[Convert.ToInt32(rownumtxt.Text.Trim()) - 1].Cells[0];
                                rownumtxt.Text = (dataGridView1.CurrentRow.Index + 1).ToString();
                            }
                        }
                    }
                    else
                        MessageBox.Show("仅支持数字输入，请检查后重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    //现行选中项的行索引已经增加到了和count一样了
                    //实际的currentSelect的值应该要比count小1
                    //给textBox10赋值为和count一样
                    rownumtxt.Text = count.ToString();
                }
                else
                {
                    //如果现行选中项的行索引不为count（还有可以增加的空间）
                    //变更现行选中项
                    dataGridView1.CurrentCell = dataGridView1.Rows[currentSelect + 1].Cells[0];
                    currentSelect = dataGridView1.CurrentRow.Index;
                    rownumtxt.Text = (currentSelect + 1).ToString();
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
                rownumtxt.Text = count.ToString();
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
                    rownumtxt.Text = "1";
                }
                else
                {
                    //如果现行选中项的行索引不为0（还有可以减少的空间）
                    //变更现行选中项
                    dataGridView1.CurrentCell = dataGridView1.Rows[currentSelect - 1].Cells[0];
                    rownumtxt.Text = currentSelect.ToString();
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
                rownumtxt.Text = "1";
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                rownumtxt.Text = "0";
            }
            else
            {
                if (rownumtxt.Text == "0")
                {
                }
                else
                {
                    int currentIndex = dataGridView1.CurrentRow.Index;
                    rownumtxt.Text = (currentIndex + 1).ToString();
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

        private void selectDatebtn1_Click(object sender, EventArgs e)
        {
            monthCalendar1.BringToFront();
            monthCalendar1.Show();
        }

        private void mainCategortcomBox()
        {
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com = new SqlCommand("select mainCategoryName from mainCategory",con);
            com.ExecuteNonQuery();
            SqlDataReader rd = com.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    mainCategorycob.Items.Add(rd[0].ToString());
                }
            }
            mainCategorycob.SelectedIndex = 0;
            rd.Close();
            con.Close();
        }

        private void numUnitcomBox()
        {
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com = new SqlCommand("select numUnitName from numUnit", con);
            com.ExecuteNonQuery();
            SqlDataReader rd = com.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    numUnitcob.Items.Add(rd[0].ToString());
                }
            }
            numUnitcob.SelectedIndex = 0;
            rd.Close();
            con.Close();
        }
        private void qgpUnitcomBox()
        {
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com = new SqlCommand("select qgpUnitName from qgpUnit", con);
            com.ExecuteNonQuery();
            SqlDataReader rd = com.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    qgpUnitcob.Items.Add(rd[0].ToString());
                }
            }
            qgpUnitcob.SelectedIndex = 1;
            rd.Close();
            con.Close();
        }
        private void stockNamecomBox()
        {
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com = new SqlCommand("select stockNames from stockNamesInf", con);
            com.ExecuteNonQuery();
            SqlDataReader rd = com.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    stockNamecob.Items.Add(rd[0].ToString());
                }
                stockNamecob.SelectedIndex = 0;
            }
            rd.Close();
            con.Close();
        }
        private void stockBrandcomBox()
        {
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com = new SqlCommand("select stockBrands from stockBrandsInf", con);
            com.ExecuteNonQuery();
            SqlDataReader rd = com.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    stockBrandcob.Items.Add(rd[0].ToString());
                }
                stockBrandcob.SelectedIndex = 0;
            }
            rd.Close();
            con.Close();
        }

        private void mainCategorycob_SelectedIndexChanged(object sender, EventArgs e)
        {
            subCategorycob.Items.Clear();
            string selectName = mainCategorycob.Text.Trim();
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            //先查询数据库 获取 当前类的id，再构造语句查询此id
            SqlCommand com1 = new SqlCommand("select mainCategoryId from mainCategory where mainCategoryName = '" + selectName + "'",con);
            string selectId = com1.ExecuteScalar().ToString();
            SqlCommand com2 = new SqlCommand("select subCategoryName from subCategory where mainId = '" + selectId + "'", con);
            com2.ExecuteNonQuery();
            SqlDataReader rd = com2.ExecuteReader();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    subCategorycob.Items.Add(rd[0].ToString());
                }
                subCategorycob.SelectedIndex = 0;
            }
            rd.Close();
            con.Close();
        }

        private void deleteInf2()
        {
            stockIdtxt.Text = "";
            insertStockpicb.ImageLocation = null;
            stockIdtxt.Text = "";
            stockNamecob.Text = "";
            stockBarcodetxt.Text = "";
            stockNumnud.Value = 0;
            stockQgpnud.Value = 0;
            stockNotetxt.Text = "";
            insertStockPersonNametxt.Text = "";
            insertStockDateTimetxt.Text = "";
        }
        private void insertStockInfbtn_Click(object sender, EventArgs e)
        {
            /*
             * 初始化变量如下：
             * 存储号
             * 库存名
             * 条形码
             * 主副类别
             * 数量单位
             * 生产日期
             * 保质期单位
             * 备注
             * 添加人添加时间
             * 详情用来输出信息
             */
            showDetailtxt.Text = "";
            string stockId = stockIdtxt.Text;
            string stockName = stockNamecob.Text.Trim();
            if(stockName == "")
            {
                showDetailtxt.Text += "错误：您输入的库存名为空\r\n";
                MessageBox.Show("您输入的库存名为空，请检查后重试！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string stockBarCode = "";
            if(noBarCodecb.Checked == true)
            {
                showDetailtxt.Text += "该库存无条形码\r\n";
                stockBarCode = "无条形码";
            }
            else
            {
                stockBarCode = stockBarcodetxt.Text.Trim();
                //进行条码为13位数字的数据验证
                Regex numCheck = new Regex("^[0-9]*$");
                if(numCheck.IsMatch(stockBarCode) && stockBarCode.Length == 13)
                {
                    //通过条形码的验证
                    stockBarCode = stockBarcodetxt.Text.Trim();
                }
                else
                {
                    showDetailtxt.Text += "错误：您输入的条形码格式错误\r\n";
                    MessageBox.Show("请检查您输入的条形码格式是否正确","提示",MessageBoxButtons.OK);
                    return;
                }
            }
            string stockBrand = stockBrandcob.Text.Trim();
            if(stockBrand == "")
            {
                showDetailtxt.Text += "错误：您输入的品牌名为空\r\n";
                MessageBox.Show("您输入的品牌名为空，请检查后重试！","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            string mainCategory = mainCategorycob.Text;
            string subCategory = subCategorycob.Text;
            string stockNum = "";
            string stockNum2 = "";
            if(stockNumnud.Value == 0)
            {
                showDetailtxt.Text += "错误：您选择的库存数量为0\r\n";
                MessageBox.Show("请注意：您选择的库存数量为0\r\n这是不合常理的，请检查后重试","提示");
                return;
            }
            else
            {
                stockNum = stockNumnud.Value.ToString();
            }
            string stockNumUnit = numUnitcob.Text;
            stockNum2 = stockNum + stockNumUnit;
            string stockDom = stockDomtxt.Text;
            string stockQgp = "";
            string stockQgp2 = "";
            if (noQgpcb.Checked == true)
            {
                showDetailtxt.Text += "该库存无保质期\r\n";
                stockQgp = "无保质期";
                stockQgp2 = "无保质期";
                //剩余保质期：天数
                stockExtime1 = "无";
                stockExtime2 = "无";
                stockExDate = "无";
                stockExState = "未过期";
            }
            else
            {
                if(stockQgpnud.Value == 0)
                {
                    showDetailtxt.Text += "错误：您选择的保质期为0\r\n";
                    MessageBox.Show("请注意：您选择的保质期为0\r\n这是不合常理的，请您检查后重试","提示");
                    return;
                }
                else
                {
                    //有保质期的情况下调用time方法进行一系列的计算
                    stockQgp = stockQgpnud.Value.ToString();
                    stockQgp2 = stockQgpnud.Value.ToString() + qgpUnitcob.Text;
                    time();
                }
            }
            string stockQgpUnit = qgpUnitcob.Text;
            string stockNote = stockNotetxt.Text.Trim();
            string insertPerson = insertStockPersonNametxt.Text.Trim();
            string insertDate1 = insertStockDateTimetxt.Text.Trim();

            if (insertStockpicb.ImageLocation == null)
            {
                showDetailtxt.Text += "错误：您未选择图片\r\n";
                MessageBox.Show("请先选择库存照片","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            else
            {
                SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                con.Open();
                SqlCommand com = new SqlCommand("select count(*) from stockNamesInf where stockNames = '" + stockName + "'", con);
                /*
                 * 查询时numCheck如果有数据时返回值应该是大于0
                 * 此时不用向数据库插入数据
                 * 如果没的话向数据库中插入数据
                 */
                int numCheck1 = (int)com.ExecuteScalar();
                
                if(numCheck1 > 0)
                {
                    showDetailtxt.Text += "已经存在库存名为“" + stockName + "”的库存\r\n";
                }
                else
                {
                    //此时数据库中还没有数据，可以直接插入
                    SqlCommand com1 = new SqlCommand("insert into stockNamesInf values ('" + stockName + "')", con);
                    com1.ExecuteScalar();
                    showDetailtxt.Text += "已经添加库存名为“" + stockName + "”的库存\r\n";
                }

                SqlCommand com999 = new SqlCommand("select count(*) from stockBrandsInf where stockBrands = '" + stockBrand + "'", con);
                int numCheck2 = (int)com999.ExecuteScalar();
                if (numCheck2 > 0)
                {
                    showDetailtxt.Text += "已经存在品牌名为“" + stockBrand + "”的库存\r\n";
                }
                else
                {
                    SqlCommand com998 = new SqlCommand("insert into stockBrandsInf values ('" + stockBrand + "')", con);
                    com998.ExecuteScalar();
                    showDetailtxt.Text += "已经添加品牌名为“" + stockBrand + "”的库存\r\n";
                }
                string fullPath = insertStockpicb.ImageLocation;
                FileStream fs = new FileStream(fullPath, FileMode.Open);
                byte[] bytes = new byte[fs.Length];
                BinaryReader br = new BinaryReader(fs);
                bytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                SqlCommand com2 = new SqlCommand("insert into stockInf (stockId,stockName,stockBarCode,stockBrand,mainCateGory,subCategory,stockNum,stockNumUnit,stockNum2,stockDom,stockQgp,stockQgpUnit,stockQgp2,stockNote,insertPerson,insertDate1,insertDate2,stockExtime1,stockExtime2,stockExDate,stockExState,stockImage) values ('" + stockId + "','" + stockName + "','" + stockBarCode + "','" + stockBrand + "','" + mainCategory + "','" + subCategory + "','" + stockNum + "','" + stockNumUnit + "','" + stockNum2 + "','" + stockDom + "','" + stockQgp + "','" + stockQgpUnit + "','" + stockQgp2 + "','" + stockNote + "','" + insertPerson + "','" + insertDate1 + "','" + insertDate2 + "','" + stockExtime1 + "','" + stockExtime2 + "','" + stockExDate + "','" + stockExState + "',@ImageList)", con);
                com2.Parameters.Add("ImageList", SqlDbType.Image);
                com2.Parameters["ImageList"].Value = bytes;
                com2.ExecuteNonQuery();
                con.Close();
                stockNamecob.Items.Clear();
                stockNamecomBox();
                stockBrandcob.Items.Clear();
                stockBrandcomBox();
                deleteInf2();
                DialogResult result = MessageBox.Show("您添加的" + mainCategory + "-" + subCategory + "类库存的信息如下：\r\n库存名：" + stockName + "\r\n库存数量：" + stockNum2 + "\r\n生产日期：" + stockDom + "\r\n是否前往查询界面查看？", "提示", MessageBoxButtons.OKCancel,MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    searchtxt2.Text = stockName;
                    tabControl3.SelectedIndex = 0;
                    stocksearchbtn.PerformClick();
                    searchtxt2.Text = "";
                }
            }
        }

        private void time()
        {
            int a = qgpUnitcob.SelectedIndex;
            switch (a)
            {
                case 0:
                    {
                        //天
                        DateTime scrq = Convert.ToDateTime(stockDomtxt.Text);
                        int tcts = Convert.ToInt32(stockQgpnud.Value);
                        string today = DateTime.Now.ToShortDateString();
                        string oscrq = scrq.AddDays(tcts).ToLongDateString();
                        DateTime x1 = Convert.ToDateTime(today);
                        DateTime x2 = Convert.ToDateTime(oscrq);
                        TimeSpan ts = x2.Subtract(x1);
                        double x3 = Convert.ToDouble(ts.TotalDays);
                        stockExDate = oscrq;
                        if (x3 > 0)
                        {
                            //年
                            double x4 = Math.Floor(x3 / 365);
                            //月
                            double x5 = Math.Floor((x3 % 365) / 30);
                            //天
                            double x6 = ((x3 % 365) % 30);
                            showDetailtxt.Text += "您添加的库存将于：" + oscrq + "过期\r\n距今天还有：" + x3 + "天\r\n折合" + x4 + "年零" + x5 + "个月零" + x6 + "天\r\n";
                            stockExtime1 = x3 + "天";
                            stockExtime2 = x4 + "年零" + x5 + "个月零" + x6 + "天";
                            stockExState = "未过期";
                        }
                        else if(x3 == 0)
                        {
                            showDetailtxt.Text += "您添加的库存将于今天过期，请及时确认\r\n";
                            stockExtime1 = "0天";
                            stockExtime2 = "0年零0个月零0天";
                            stockExState = "未过期";
                        }
                        else if (x3 < 0)
                        {
                            double x4 = -x3;
                            double x5 = Math.Floor(x4 / 365);
                            double x6 = Math.Floor((x4 % 365) / 30);
                            double x7 = ((x4 % 365) % 30);
                            showDetailtxt.Text += "您添加的库存应于：" + oscrq + "过期\r\n已经过期：" + x4 + "天折合" + x5 + "年零" + x6 + "个月零" + x7 + "天\r\n请及时确认\r\n";
                            stockExtime1 = "已过期" + x4 + "天";
                            stockExtime2 = "已过期" + x5 + "年零" + x6 + "个月零" + x7 + "天";
                            stockExState = "已过期";
                        }
                        break;
                    }
                case 1:
                    {
                        //月
                        DateTime scrq = Convert.ToDateTime(stockDomtxt.Text);
                        int tcts = Convert.ToInt32(stockQgpnud.Value * 30);
                        string today = DateTime.Now.ToShortDateString();
                        string oscrq = scrq.AddDays(tcts).ToLongDateString();
                        DateTime x1 = Convert.ToDateTime(today);
                        DateTime x2 = Convert.ToDateTime(oscrq);
                        TimeSpan ts = x2.Subtract(x1);
                        double x3 = Convert.ToDouble(ts.TotalDays);
                        stockExDate = oscrq;
                        if (x3 > 0)
                        {
                            //年
                            double x4 = Math.Floor(x3 / 365);
                            //月
                            double x5 = Math.Floor((x3 % 365) / 30);
                            //天
                            double x6 = ((x3 % 365) % 30);
                            showDetailtxt.Text += "您添加的库存将于：" + oscrq + "过期\r\n距今天还有：" + x3 + "天\r\n折合" + x4 + "年零" + x5 + "个月零" + x6 + "天\r\n";
                            stockExtime1 = x3 + "天";
                            stockExtime2 = x4 + "年零" + x5 + "个月零" + x6 + "天";
                            stockExState = "未过期";
                        }
                        else if (x3 == 0)
                        {
                            showDetailtxt.Text += "您添加的库存将于今天过期，请及时确认\r\n";
                            stockExtime1 = "0天";
                            stockExtime2 = "0年零0个月零0天";
                            stockExState = "未过期";
                        }
                        else if (x3 < 0)
                        {
                            double x4 = -x3;
                            double x5 = Math.Floor(x4 / 365);
                            double x6 = Math.Floor((x4 % 365) / 30);
                            double x7 = ((x4 % 365) % 30);
                            showDetailtxt.Text += "您添加的库存应于：" + oscrq + "过期\r\n已经过期：" + x4 + "天折合" + x5 + "年零" + x6 + "个月零" + x7 + "天\r\n请及时确认\r\n";
                            stockExtime1 = "已过期" + x4 + "天";
                            stockExtime2 = "已过期" + x5 + "年零" + x6 + "个月零" + x7 + "天";
                            stockExState = "已过期";
                        }
                        break;
                    }
                case 2:
                    {
                        //年
                        DateTime scrq = Convert.ToDateTime(stockDomtxt.Text);
                        int tcts = Convert.ToInt32(stockQgpnud.Value * 365);
                        string today = DateTime.Now.ToShortDateString();
                        string oscrq = scrq.AddDays(tcts).ToLongDateString();
                        DateTime x1 = Convert.ToDateTime(today);
                        DateTime x2 = Convert.ToDateTime(oscrq);
                        TimeSpan ts = x2.Subtract(x1);
                        double x3 = Convert.ToDouble(ts.TotalDays);
                        stockExDate = oscrq;
                        if (x3 > 0)
                        {
                            //年
                            double x4 = Math.Floor(x3 / 365);
                            //月
                            double x5 = Math.Floor((x3 % 365) / 30);
                            //天
                            double x6 = ((x3 % 365) % 30);
                            showDetailtxt.Text += "您添加的库存将于：" + oscrq + "过期\r\n距今天还有：" + x3 + "天\r\n折合" + x4 + "年零" + x5 + "个月零" + x6 + "天\r\n";
                            stockExtime1 = x3 + "天";
                            stockExtime2 = x4 + "年零" + x5 + "个月零" + x6 + "天";
                            stockExState = "未过期";
                        }
                        else if (x3 == 0)
                        {
                            showDetailtxt.Text += "您添加的库存将于今天过期，请及时确认\r\n";
                            stockExtime1 = "0天";
                            stockExtime2 = "0年零0个月零0天";
                            stockExState = "未过期";
                        }
                        else if (x3 < 0)
                        {
                            double x4 = -x3;
                            double x5 = Math.Floor(x4 / 365);
                            double x6 = Math.Floor((x4 % 365) / 30);
                            double x7 = ((x4 % 365) % 30);
                            showDetailtxt.Text += "您添加的库存应于：" + oscrq + "过期\r\n已经过期：" + x4 + "天折合" + x5 + "年零" + x6 + "个月零" + x7 + "天\r\n请及时确认\r\n";
                            stockExtime1 = "已过期" + x4 + "天";
                            stockExtime2 = "已过期" + x5 + "年零" + x6 + "个月零" + x7 + "天";
                            stockExState = "已过期";
                        }
                        break;
                    }
            }
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            stockDomtxt.Text = monthCalendar1.SelectionStart.ToLongDateString();
            monthCalendar1.Hide();
        }

        private void noQgpcb_CheckedChanged(object sender, EventArgs e)
        {
            if(noQgpcb.Checked == true)
            {
                stockQgpnud.Enabled = false;
                qgpUnitcob.Enabled = false;
            }
            else
            {
                stockQgpnud.Enabled = true;
                qgpUnitcob.Enabled = true;
            }
        }

        private void noBarCodecb_CheckedChanged(object sender, EventArgs e)
        {
            if(noBarCodecb.Checked == true)
            {
                stockBarcodetxt.Text = "无条形码";
                stockBarcodetxt.Enabled = false;

            }
            else
            {
                stockBarcodetxt.Enabled = true;
                stockBarcodetxt.Text = "";
            }
        }

        private void stockNamecob_TextChanged(object sender, EventArgs e)
        {
            string str1 = stockNamecob.Text.Trim();
            string str2 = DateTime.Now.ToShortDateString().Replace("/", "");
            string str3 = DateTime.Now.ToLongTimeString().Replace(":", "");
            stockIdtxt.Text = str1 + str2 + "01" + str3;
        }

        private void insertStockImgbtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择库存品的照片";
            ofd.Filter = "图片文件(JPG，JPEG，BMP，PNG)|*.jpg;*.jpeg;*.bmp;*.png";
            if (ofd.ShowDialog() == DialogResult.OK && (openFileDialog1.FileName != ""))
            {
                insertStockpicb.ImageLocation = ofd.FileName;
            }
            ofd.Dispose();
        }

        private void cleanNames_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("清理库存名后，您将无法快捷选择您所添加过的库存名，您确定清理吗？","提示",MessageBoxButtons.OKCancel);
            if(result == DialogResult.OK)
            {
                SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                con.Open();
                SqlCommand com = new SqlCommand("truncate table stockNamesInf", con);
                com.ExecuteScalar();
                con.Close();
                stockNamecob.Text = "";
                stockNamecob.Items.Clear();
                MessageBox.Show("保存的库存名已经成功清理", "提示");
            }
        }
        //方法：设置可见状态
        private void visiblestate1()
        {
            firstbtn2.Visible = true;
            lastbtn2.Visible = true;
            previousbtn2.Visible = true;
            nextbtn2.Visible = true;
            rownumtxt2.Visible = true;
            gobtn2.Visible = true;
        }
        
        //方法：执行查询
        private void stocksearchbtn_Click(object sender, EventArgs e)
        {
            /*
             * 支持模糊查询
             */
            rownumtxt2.Text = "0";
            string search = searchtxt2.Text.Trim();
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            if (search == "")
            {
                //全部查询
                visiblestate1();
                SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf",con);
                SqlCommand com2 = new SqlCommand("select count(*) from stockInf", con);
                countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                stockCount = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "stockInf");
                dataGridView2.DataSource = ds.Tables["stockInf"];
                con.Close();
                if (stockCount == 0)
                    rownumtxt2.Text = "0";
                else
                {
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
            }
            else
            {
                //模糊查询库存名
                visiblestate1();
                SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where stockName like '%" + search + "%'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from stockInf where stockName like '%" + search + "%'", con);
                countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                stockCount = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "stockInf");
                dataGridView2.DataSource = ds.Tables["stockInf"];
                con.Close();
                if (stockCount == 0)
                    rownumtxt2.Text = "0";
                else
                {
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
            }
        }

        private void contextMenuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            /*
             * 详细信息 0
             * 分隔符   1
             * 添加     2
             * 修改     3
             * 删除     4
             * 
             */
            if (stockCount == 0)
            {
                //修改和删除
                if (contextMenuStrip2.Items[3].Selected == true || contextMenuStrip2.Items[4].Selected == true)
                {
                    contextMenuStrip2.Close();
                    MessageBox.Show("暂无选中项需要更改", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                //添加
                else if(contextMenuStrip2.Items[2].Selected == true)
                {
                    contextMenuStrip2.Close();
                    tabControl3.SelectedIndex = 1;
                }
                //详细信息
                else if (contextMenuStrip2.Items[0].Selected == true)
                {
                    contextMenuStrip2.Close();
                    MessageBox.Show("暂无选中项可以查看", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                //stockCount不等于0
                if (contextMenuStrip2.Items[0].Selected == true)
                {
                    //查看详细信息
                    contextMenuStrip2.Close();
                    currentSelect = dataGridView2.CurrentRow.Index;
                    string a1 = dataGridView2.Rows[currentSelect].Cells[0].Value.ToString();
                    DetailForm df = new DetailForm();
                    df.str = a1;
                    df.ShowDialog();
                }
                else if(contextMenuStrip2.Items[2].Selected == true)
                {
                    tabControl3.SelectedIndex = 1;
                }
                else if (contextMenuStrip2.Items[3].Selected == true)
                {
                    contextMenuStrip2.Close();
                    //获取当前选中行
                    currentSelect = dataGridView2.CurrentRow.Index;
                    //获取选中行中的员工数据
                    string updatepersonName = dataGridView1.Rows[currentSelect].Cells[1].Value.ToString();
                    string updatepersonNum = dataGridView1.Rows[currentSelect].Cells[3].Value.ToString();
                    string updatepersonSex = dataGridView1.Rows[currentSelect].Cells[2].Value.ToString();
                    string updatepersonPassword = dataGridView1.Rows[currentSelect].Cells[4].Value.ToString();
                    string updatepersonPhoneNum = dataGridView1.Rows[currentSelect].Cells[5].Value.ToString();
                    string updatepersonAddress = dataGridView1.Rows[currentSelect].Cells[6].Value.ToString();
                    updateName = updatepersonName;
                    updateNum = updatepersonNum;
                    otxt.Text = "您想要修改的员工信息如下：\r\n" + "姓名：" + updatepersonName + "\r\n" + "工号：" + updatepersonNum + "\r\n" + "性别：" + updatepersonSex + "\r\n" + "手机号：" + updatepersonPhoneNum + "\r\n" + "地址：" + updatepersonAddress + "\r\n" + "密码：" + updatepersonPassword;
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
                else if (contextMenuStrip2.Items[4].Selected == true)
                {
                    //删除
                    contextMenuStrip2.Close();
                    //获取当前选中行
                    currentSelect = dataGridView2.CurrentRow.Index;
                    //获取选中行中的工号数据
                    string d1 = dataGridView2.Rows[currentSelect].Cells[2].Value.ToString();
                    string d2 = dataGridView2.Rows[currentSelect].Cells[3].Value.ToString();
                    string d3 = dataGridView2.Rows[currentSelect].Cells[4].Value.ToString();
                    string d4 = dataGridView2.Rows[currentSelect].Cells[5].Value.ToString();
                    string d5 = dataGridView2.Rows[currentSelect].Cells[6].Value.ToString();
                    string d6 = dataGridView2.Rows[currentSelect].Cells[0].Value.ToString();
                    DialogResult result = MessageBox.Show("您想要删除的库存信息如下：\r\n库存名：" + d1 + "\r\n库存数量：" + d2 + "\r\n生产日期：" + d3 + "\r\n过期日期：" + d4 + "\r\n剩余保质期：" + d5 + "\r\n确定删除吗？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                        con.Open();
                        SqlCommand deletecom = new SqlCommand("delete from stockInf where stockId = '" + d6 + "'", con);
                        deletecom.ExecuteScalar();
                        MessageBox.Show("您已成功删除一条" + d1 + "的库存信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        con.Close();
                        searchtxt2.Text = "";
                        stocksearchbtn.PerformClick();
                    }
                }
            }
        }

        private void gobtn2_Click(object sender, EventArgs e)
        {
            //判断是否通过数据库查询到了值
            //stockCount的值必定是 >= 0的
            if (stockCount == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (stockCount > 0)
            {
                if (rownumtxt2.Text.Trim() == "")
                {
                    //判断rownumtxt值是否为空值，如果是将其赋值为1
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
                else
                {
                    //验证输入内容是否为数字
                    Regex numCheck = new Regex("^[0-9]*$");
                    if (numCheck.IsMatch(rownumtxt2.Text.Trim()))
                    {
                        //通过验证
                        //判断是否超出了stockCount的大小
                        if (Convert.ToInt32(rownumtxt2.Text.Trim()) >= stockCount)
                        {
                            //超出
                            dataGridView2.CurrentCell = dataGridView2.Rows[stockCount - 1].Cells[0];
                            rownumtxt2.Text = stockCount.ToString();
                        }
                        else
                        {
                            //未超出
                            //判断输入值是否为0和1
                            if (rownumtxt2.Text.Trim() == "0" || rownumtxt2.Text.Trim() == "1")
                            {
                                dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                                rownumtxt2.Text = "1";
                            }
                            else
                            {
                                dataGridView2.CurrentCell = dataGridView2.Rows[Convert.ToInt32(rownumtxt2.Text.Trim()) - 1].Cells[0];
                                rownumtxt2.Text = (dataGridView2.CurrentRow.Index + 1).ToString();
                            }
                        }
                    }
                    else
                        MessageBox.Show("仅支持数字输入，请检查后重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //方法：为dateGridView2绘制行索引
        private void dataGridView2_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            //为dataGridView2自动添加序号
            SolidBrush sb = new SolidBrush(dataGridView2.RowHeadersDefaultCellStyle.ForeColor);
            e.Graphics.DrawString((e.RowIndex + 1).ToString(System.Globalization.CultureInfo.CurrentUICulture),dataGridView2.DefaultCellStyle.Font, sb, e.RowBounds.Location.X, e.RowBounds.Location.Y);
        }

        private void firstbtn2_Click(object sender, EventArgs e)
        {
            if (stockCount == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                rownumtxt2.Text = "1";
            }
        }

        private void previousbtn2_Click(object sender, EventArgs e)
        {
            //上一条
            //判断stockCount是否为0
            if (stockCount == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //判断是否已经选到了第一条数据
                //给全局变量currentSelect赋值，令其与现行选中项的行索引相等
                currentSelect = dataGridView2.CurrentRow.Index;
                //此时currentSelect应该和textBox10的显示值相差1

                if (currentSelect == 0)
                {
                    rownumtxt2.Text = "1";
                }
                else
                {
                    //如果现行选中项的行索引不为0（还有可以减少的空间）
                    //变更现行选中项
                    dataGridView2.CurrentCell = dataGridView2.Rows[currentSelect - 1].Cells[0];
                    rownumtxt2.Text = currentSelect.ToString();
                }
            }
        }
        //方法：下一个
        private void nextbtn2_Click(object sender, EventArgs e)
        {
            //下一条
            //判断stockCount是否为0
            if (stockCount == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //判断是否已经选到了最后一条数据
                //给全局变量currentSelect赋值，令其与现行选中项的行索引相等
                currentSelect = dataGridView2.CurrentRow.Index;
                int countNum = stockCount - 1;
                if (currentSelect == countNum)
                {
                    /*
                     * 如果现行选中项的行索引已经增加到了和stockCount的值一样时
                     * 此时实际的currentSelect的值已经是比stockCount的值要小1
                     * 此时应该给rownumtxt2的Text属性赋值为和stockCount一致
                     */
                    rownumtxt2.Text = stockCount.ToString();
                }
                else
                {
                    /*
                     * 如果此时的选中项的行索引不等于stockCount
                     * 说明此时应该还可以继续增加
                     * 因此可以让currentSelect+1
                     * 最后变更选中项的行索引
                     */
                    dataGridView2.CurrentCell = dataGridView2.Rows[currentSelect + 1].Cells[0];
                    currentSelect = dataGridView2.CurrentRow.Index;
                    rownumtxt2.Text = (currentSelect + 1).ToString();
                }
            }
        }

        private void lastbtn2_Click(object sender, EventArgs e)
        {
            if (stockCount == 0)
            {
                MessageBox.Show("无查询结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                /*
                 * 令选中的值和stockCount一样
                 * 并且变更rownumtxt2的数值
                 */
                dataGridView2.CurrentCell = dataGridView2.Rows[stockCount - 1].Cells[0];
                rownumtxt2.Text = stockCount.ToString();
            }
        }
        //方法：清理保存的品牌名
        private void cleanBrand_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("清理品牌后，您将无法快捷选择您所添加过的品牌名，您确定清理吗？", "提示", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
                con.Open();
                SqlCommand com = new SqlCommand("truncate table stockBrandsInf", con);
                com.ExecuteScalar();
                con.Close();
                stockBrandcob.Text = "";
                stockBrandcob.Items.Clear();
                MessageBox.Show("保存的库存名已经成功清理", "提示");
            }
        }
        //方法：时钟事件，令添加库存信息界面的添加人和添加时间每隔一分钟刷新一次
        private void timer2_Tick(object sender, EventArgs e)
        {
            
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 0)
            {
                rownumtxt.Text = "0";
            }
            else
            {
                if (rownumtxt.Text == "0")
                {
                }
                else
                {
                    int currentIndex = dataGridView2.CurrentRow.Index;
                    rownumtxt2.Text = (currentIndex + 1).ToString();
                }

            }
        }
        private void insertPersoncomBox()
        {
            //将添加人填入按姓名查询
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com = new SqlCommand("select insertPerson from stockInf", con);
            com.ExecuteNonQuery();
            SqlDataReader rd = com.ExecuteReader();
            List<string> list1 = new List<string>();
            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    //string[] strs = new string[] { "A", "A", "A", "A", "B", "A", "C", "B" };
                    list1.Add(rd[0].ToString());
                    for (int i = 0; i < list1.Count; i++)
                    {
                        for (int j = list1.Count - 1; j > i; j--)
                        {
                            if (list1[i] == list1[j])
                            {
                                list1.RemoveAt(j);
                            }
                        }
                    }
                }
                for (int k = 0; k < list1.Count; k++)
                {
                    insertPersoncob.Items.Add(list1[k]);
                }
                insertPersoncob.SelectedIndex = 0;
            }
            rd.Close();
            con.Close();
        }
        //方法：按条件查询
        private void searchIfbtn_Click(object sender, EventArgs e)
        {
            groupBox4.Visible = true;
            searchIfbtn.Enabled = false;
        }
        //方法：变更日期时查询
        private void monthCalendar2_DateSelected(object sender, DateRangeEventArgs e)
        {
            datesearchtxt.Text = monthCalendar2.SelectionStart.ToLongDateString();
            insertDate2 = monthCalendar2.SelectionStart.ToLongDateString();
            rownumtxt2.Text = "0";
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            visiblestate1();
            SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where insertDate2 = '" + insertDate2 + "'", con);
            SqlCommand com2 = new SqlCommand("select count(*) from stockInf where insertDate2 = '" + insertDate2 + "'", con);
            countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
            stockCount = Convert.ToInt32(com2.ExecuteScalar());
            SqlDataAdapter da = new SqlDataAdapter(com1);
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds, "stockInf");
            dataGridView2.DataSource = ds.Tables["stockInf"];
            con.Close();
            if (stockCount == 0)
                rownumtxt2.Text = "0";
            else
            {
                dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                rownumtxt2.Text = "1";
            }
                
        }

        private void hidegroupbox4btn_Click(object sender, EventArgs e)
        {
            groupBox4Hide();
            searchIfbtn.Enabled = true;
        }

        //按照有无保质期查询
        private void qgpsearchbtn_Click(object sender, EventArgs e)
        {
            visiblestate1();
            rownumtxt2.Text = "0";
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            if (haveQgprd.Checked == true)
            {
                string search = "无保质期";
                SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where stockQgp2 != '" + search + "'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from stockInf where stockQgp2 != '" + search + "'", con);
                countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                stockCount = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "stockInf");
                dataGridView2.DataSource = ds.Tables["stockInf"];
                con.Close();
                if (stockCount == 0)
                    rownumtxt2.Text = "0";
                else
                {
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
            }
            else if (noQgprd.Checked == true)
            {
                string search = "无保质期";
                SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where stockQgp2 = '" + search + "'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from stockInf where stockQgp2 = '" + search + "'", con);
                countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                stockCount = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "stockInf");
                dataGridView2.DataSource = ds.Tables["stockInf"];
                con.Close();
                if (stockCount == 0)
                    rownumtxt2.Text = "0";
                else
                {
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
            }
        }

        //按照过期状态查询
        private void exstatesearchbtn_Click(object sender, EventArgs e)
        {
            visiblestate1();
            rownumtxt2.Text = "0";
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            if (overduerd.Checked == true)
            {
                string search = "已过期";
                SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where stockExState = '" + search + "'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from stockInf where stockExState = '" + search + "'", con);
                countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                stockCount = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "stockInf");
                dataGridView2.DataSource = ds.Tables["stockInf"];
                con.Close();
                if (stockCount == 0)
                    rownumtxt2.Text = "0";
                else
                {
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
            }
            else if (unoverduerd.Checked == true)
            {
                string search = "未过期";
                SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where stockExState = '" + search + "'", con);
                SqlCommand com2 = new SqlCommand("select count(*) from stockInf where stockExState = '" + search + "'", con);
                countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
                stockCount = Convert.ToInt32(com2.ExecuteScalar());
                SqlDataAdapter da = new SqlDataAdapter(com1);
                DataSet ds = new DataSet();
                ds.Clear();
                da.Fill(ds, "stockInf");
                dataGridView2.DataSource = ds.Tables["stockInf"];
                con.Close();
                if (stockCount == 0)
                    rownumtxt2.Text = "0";
                else
                {
                    dataGridView2.CurrentCell = dataGridView2.Rows[0].Cells[0];
                    rownumtxt2.Text = "1";
                }
            }
        }

        //双击打开详情窗口
        private void dataGridView2_DoubleClick(object sender, EventArgs e)
        {
            if(stockCount == 0)
            {
                MessageBox.Show("暂无选中项可以查看", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //stockCount不等于0
                currentSelect = dataGridView2.CurrentRow.Index;
                string a1 = dataGridView2.Rows[currentSelect].Cells[0].Value.ToString();
                DetailForm df = new DetailForm();
                df.str = a1;
                df.ShowDialog();
            }
        }
        
        //按添加人姓名查询
        private void namesearchbtn_Click(object sender, EventArgs e)
        {
            visiblestate1();
            string search = insertPersoncob.SelectedItem.ToString();
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlCommand com1 = new SqlCommand("select stockId,stockImage,stockName,stockNum2,stockDom,stockExDate,stockExtime1,stockExState from stockInf where insertPerson = '" + search + "'", con);
            SqlCommand com2 = new SqlCommand("select count(*) from stockInf where insertPerson = '" + search + "'", con);
            countlabel2.Text = "共查询到" + com2.ExecuteScalar() + "条数据";
            stockCount = Convert.ToInt32(com2.ExecuteScalar());
            SqlDataAdapter da = new SqlDataAdapter(com1);
            DataSet ds = new DataSet();
            ds.Clear();
            da.Fill(ds, "stockInf");
            dataGridView2.DataSource = ds.Tables["stockInf"];
            con.Close();
        }

    }
}
