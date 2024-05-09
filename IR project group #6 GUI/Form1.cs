using IR_Project_group6_C_;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IR_project_group__6_GUI
{
    public partial class Form1 : Form
    {
        searchEngine engine;
        public Form1(searchEngine engine)
        {
            this.engine = engine;
            InitializeComponent();
            Label label = new Label();
            label.Text = "Items in the Inverterted index: " + engine.data.Count;
            label.Text += "\n Files parsed: " + engine.filesParsed;
            label.Dock = DockStyle.Fill;
            tabPage3.Controls.Add(label);

            DataTable dt = new DataTable();
            dt.Columns.Add("Token", typeof(string));
            dt.Columns.Add("Soundex", typeof(string));
            dt.Columns.Add("Frequency", typeof(int));
            //dt.Columns.Add("Publish", typeof(DateTime));
            //dt.Columns.Add("Author", typeof(string));
            foreach (var data in engine.data)
            {
                dt.Rows.Add(data.token, data.soundex, data.locations.Count);
            }
            dataGridView1.DataSource = dt;

            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            string userinput = textBox1.Text;
            var test = engine.Search(userinput, checkBox1.Checked);
            DataTable dt = new DataTable();
            dt.Columns.Add("locations",typeof(string));
            foreach (var t in test)
            {
                dt.Rows.Add(t);
            }
            dataGridView2.DataSource = dt;
            dataGridView2.Columns[0].Width = dataGridView2.Size.Width;
            tabPage2.Update();
        }
    }
}
