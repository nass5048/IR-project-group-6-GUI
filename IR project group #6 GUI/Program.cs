﻿using IR_Project_group6_C_;
using System;
using System.Collections.Generic;
using Pluralize.NET;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics.Tracing;
//
// Mike Nassoiy CS465/665, S24, and Project # 1

namespace IR_project_group__6_GUI
{
    public class queryTerm
    {
        //the type that query terms are
        public queryTerm(string term1)
        {
            term = term1;
            op = true;
            
            var temp = new InvertedIndexData(term1, "");

            soundex = temp.soundex;
            if ("1234567890".Contains(term1.Last()))
                soundex = term1;
        }
        public string term;
        public bool op;
        public string soundex;
    }

    public class FileData
    {
        public FileData(string path)
        {
            this.path = path;
        }
        public string path;
        public int totalWords;
        public int distinctWords;
        public List<string> words= new List<string>();
        /// <summary>
        /// Must call this function when you are done with entering filedata
        /// </summary>
        public void done()
        {
            distinctWords = words.Distinct().ToList().Count;
            words.Clear();
        }
    }


    internal static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //"..\\..\\..\\Data"
            
            searchEngine engine = new searchEngine("..\\..\\");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUI(engine));
        }
    }
}


// See https://aka.ms/new-console-template for more information

//create a GUI
//parse files in blocks
//create the inverted index
//lookup data from the inverted index
//display if the data was found and where the data was found
//only create the inverted index when the user selects to on the gui -> this will prevent creatoin of the invertedd index every time
//see if there is a quick way to view changes in files to possibly just update the index every time a specipic file has been changed -> Metadata possibly

