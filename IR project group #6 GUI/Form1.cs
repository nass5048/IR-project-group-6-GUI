using IR_Project_group6_C_;
using System;
using System.CodeDom;
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
            setData();
            

            
        }
        private void setData()
        {
            tabPage1.Text = "Search";
            tabPage2.Text = "Index";
            tabPage3.Text = "Stats";
            tabPage4.Text = "Config";
            tabPage5.Text = "File Stats";
            fileSystemWatcher1.Path = engine.path;
            label2.Text = "File Path: " + engine.path;
            Label label = new Label();
            label.Text = "Items in the Inverterted index: " + engine.data.Count;
            label.Text += "\nFiles parsed: " + engine.filesParsed;
            label.Text += "\nTotal words parsed: " + engine.totalWordsParsed;
            var topwords = engine.data.OrderByDescending(data => data.locations.Count).ToList();
            label.Text += "\nTop word: " + topwords[0].token;
            label.Text += "\nTop 100th word: " + topwords[99].token;
            label.Text += "\nTop 500th word: " + topwords[499].token;
            label.Text += "\nTop 1000th word: " + topwords[999].token;
            label.Text += "\nLast word: " + topwords.Last().token;
            label.Dock = DockStyle.Fill;
            tabPage3.Controls.Add(label);
            DataTable dtf = new DataTable();
            dtf.Columns.Add("File", typeof(string));
            dtf.Columns.Add("Total Words", typeof(int));
            dtf.Columns.Add("Distinct Words", typeof(int));
            foreach (var data in engine.FilePaths)
            {
                dtf.Rows.Add(data.path.Split('\\').Last(), data.totalWords, data.distinctWords);
            }
            dataGridView3.DataSource = dtf;

            DataTable dt = new DataTable();
            dt.Columns.Add("Token", typeof(string));
            dt.Columns.Add("Soundex", typeof(string));
            dt.Columns.Add("Frequency", typeof(int));
            dt.Columns.Add("Total Words", typeof(int));
            //dt.Columns.Add("Publish", typeof(DateTime));
            //dt.Columns.Add("Author", typeof(string));
            foreach (var data in engine.data)
            {
                dt.Rows.Add(data.token, data.soundex, data.locations.Count, data.totalWords);
            }
            dataGridView1.DataSource = dt;
        }
        private void button1_Click_1(object sender, EventArgs e)
        {

            string userinput = textBox1.Text;
            var test = engine.Search(userinput, checkBox1.Checked);
            Label label = new Label();
            label1.Text = "Files found with query: " + test.Count;
            //label1.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(label);
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

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            engine.DeleteLocation(e.FullPath);
            engine.Process(e.FullPath);
            setData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if(!string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
                engine = new searchEngine(folderBrowserDialog1.SelectedPath);
            MessageBox.Show("File Path Changed");
            label2.Text = "File Path: " + engine.path;
            fileSystemWatcher1.Path = engine.path;
        }
    }
}
