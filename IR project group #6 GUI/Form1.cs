﻿using IR_Project_group6_C_;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
// Mike Nassoiy CS465/665, S24, and Project # 1



namespace IR_project_group__6_GUI
{
    public partial class GUI : Form
    {
        searchEngine engine;
        public GUI(searchEngine engine)
        {
            //gets the current search engine class
            this.engine = engine;
            //initalizes the GUI
            InitializeComponent();
            //Sets the data
            setData();
        }
        //naming follows the autogenerated toolbox names for gui
        //sets the data to be displayed
        private void setData()
        {
            tabPage1.Text = "Search";
            tabPage2.Text = "Index";
            tabPage3.Text = "Stats";
            tabPage4.Text = "Config";
            tabPage5.Text = "File Stats";
            fileSystemWatcher1.Path = engine.path;
            label2.Text = "File Path: " + engine.path;
            label3.Text = "Items in the Inverted index: " + engine.data.Count;
            label3.Text += "\nFiles parsed: " + engine.filesParsed;
            label3.Text += "\nTotal words parsed: " + engine.totalWordsParsed;
            var topwords = engine.data.OrderByDescending(data => data.locations.Count).ToList();
            if(topwords.Count > 0)
                label3.Text += "\nTop word: " + topwords[0].token;
            if (topwords.Count > 99)
                label3.Text += "\nTop 100th word: " + topwords[99].token;
            if (topwords.Count > 499)
                label3.Text += "\nTop 500th word: " + topwords[499].token;
            if (topwords.Count > 999)
                label3.Text += "\nTop 1000th word: " + topwords[999].token;
            if (topwords.Count > 0)
                label3.Text += "\nLast word: " + topwords.Last().token;
            label3.Dock = DockStyle.Fill;
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
        //this is the search function
        private void button1_Click_1(object sender, EventArgs e)
        {
            //gets the user input from the texxtbox
            string userinput = textBox1.Text;
            //gets a list of the files that have been found
            var test = engine.Search(userinput, checkBox1.Checked);
            //creates a new label to use
            Label label = new Label();
            label1.Text = "Files found with query: " + test.Count;
            tabPage1.Controls.Add(label);
            DataTable dt = new DataTable();
            dt.Columns.Add("locations",typeof(string));
            //adds the locations to the datatable 
            foreach (var t in test)
            {
                dt.Rows.Add(t);
            }
            dataGridView2.DataSource = dt;
            dataGridView2.Columns[0].Width = dataGridView2.Size.Width;
            //updates the page
            tabPage2.Update();
        }
        //dynamiclally parses through the files in the selected folder
        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            //deletes the data from that document
            engine.DeleteLocation(e.FullPath);
            //parses the data from that document if it wasnt deleted
            if(e.ChangeType != System.IO.WatcherChangeTypes.Deleted)
                engine.Process(e.FullPath);
            setData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //shows the dialog to garantee a proper file 
            folderBrowserDialog1.ShowDialog();
            //creates a new engine for that file
            if(!string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
                engine = new searchEngine(folderBrowserDialog1.SelectedPath);
            //MessageBox.Show("File Path Changed");
            label2.Text = "File Path: " + engine.path;
            setData();
            //sets the folder to watch for edits
            fileSystemWatcher1.Path = engine.path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var text = "";
            int i = 0;
            backgroundWorker1.ReportProgress(0);
            foreach (var s in engine.data)
            {
                text += s.token + "\n";
                text += s.soundex + "\n";
                text += "Total Files Word is in: " + s.totalWords + "\n";
                foreach (var locations in s.locations)
                {
                    text += locations + "\n";
                }
                text += "====================================================================\n";
                i++;
                backgroundWorker1.ReportProgress((int)((int)(double)i / (double)engine.data.Count *100));
            }
            File.WriteAllText("Stats.txt", text);
            backgroundWorker1.ReportProgress(100);
            MessageBox.Show("Statistics Created");
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
    }
}
