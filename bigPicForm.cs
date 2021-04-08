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
    public partial class bigPicForm : Form
    {
        public bigPicForm()
        {
            InitializeComponent();
        }
        public string str;
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bigPicForm_Load(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection("Data Source=(local);Initial Catalog=QTSuperMarket;Integrated Security=True");
            con.Open();
            byte[] imagebytes = null;
            SqlCommand com1 = new SqlCommand("select stockImage from stockInf where stockId = '" + str + "'", con);
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
        }
    }
}
