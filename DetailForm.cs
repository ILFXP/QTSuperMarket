using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace QTSuperMarket
{
    public partial class DetailForm : Form
    {
        public DetailForm()
        {
            InitializeComponent();
        }
        public string str;
        private void DetailForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = str;

            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand com = new SqlCommand("select stockName,stockBarCode,stockBrand,mainCategory,subCategory,stockNum2,stockDom,stockQgp2,stockExtime1,stockExDate,stockNote,insertPerson,insertDate1 from stockInf where stockId = '" + str + "'",con);
            da.SelectCommand = com;
            DataSet ds = new DataSet();
            da.Fill(ds);
            int numCheck = ds.Tables[0].Rows.Count;
            if(numCheck > 0)
            {
                textBox2.Text = ds.Tables[0].Rows[0]["stockName"].ToString();
                textBox3.Text = ds.Tables[0].Rows[0]["stockBarCode"].ToString();
                textBox4.Text = ds.Tables[0].Rows[0]["stockBrand"].ToString();
                textBox5.Text = ds.Tables[0].Rows[0]["mainCategory"].ToString();
                textBox6.Text = ds.Tables[0].Rows[0]["subCategory"].ToString();
                textBox7.Text = ds.Tables[0].Rows[0]["stockNum2"].ToString();
                textBox8.Text = ds.Tables[0].Rows[0]["stockDom"].ToString();
                textBox9.Text = ds.Tables[0].Rows[0]["stockQgp2"].ToString();
                textBox10.Text = ds.Tables[0].Rows[0]["stockExtime1"].ToString();
                textBox11.Text = ds.Tables[0].Rows[0]["stockExDate"].ToString();
                textBox12.Text = ds.Tables[0].Rows[0]["stockNote"].ToString();
                textBox13.Text = ds.Tables[0].Rows[0]["insertPerson"].ToString();
                textBox14.Text = ds.Tables[0].Rows[0]["insertDate1"].ToString();

            }
            else
            {
                MessageBox.Show("未查询到数据","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            byte[] imagebytes = null;
            SqlCommand com1 = new SqlCommand("select stockImage from stockInf where stockId = '" + str + "'",con);
            SqlDataReader dr = com1.ExecuteReader();
            while (dr.Read())
            {
                imagebytes = (byte[])dr.GetValue(0);
            }
            dr.Close();
            com1.Clone();
            con.Close();
            MemoryStream ms = new MemoryStream(imagebytes);
            Bitmap bmpt = new Bitmap(ms);
            pictureBox1.Image = bmpt;
            this.Text = textBox2.Text + "的库存信息";
        }

        private void bigPicbtn_Click(object sender, EventArgs e)
        {
            bigPicForm bf = new bigPicForm();
            bf.str = textBox1.Text;
            bf.ShowDialog();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            this.bigPicbtn.PerformClick();
        }
    }
}
