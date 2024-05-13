using IR_Project_group6_C_;
using Pluralize.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static System.Net.Mime.MediaTypeNames;
//
// Mike Nassoiy, Andrew CS465/665, S24, and Project # 1

namespace IR_project_group__6_GUI
{
    public class searchEngine
    {
        //sets the public variables
        public List<InvertedIndexData> data = new List<InvertedIndexData>();
        public int filesParsed = 0;
        public string path = string.Empty;
        public int totalWordsParsed;
        public List<FileData> FilePaths = new List<FileData>();
        public searchEngine(string path)
        {
            totalWordsParsed = 0;
            var files = from file in Directory.EnumerateFiles(path, "*.txt") select file;
            int docID = 0;
            this.path = path;
            //goes through each file in the foldee
            foreach (var file in files)
            {

                filesParsed++;
                data = Process(file);
            }
            //prints out the index
            data = Sort();
        }
        //sorts the inverted index by alphabetical order
        private List<InvertedIndexData> Sort()
        {
            data = data.OrderBy(i => i.token).ToList();
            return data;
        }
        /// <summary>
        /// searches for the individual terms
        /// </summary>
        /// <param name="query"></param>
        /// <param name="soundex"></param>
        /// <returns></returns>
        private List<InvertedIndexData> SearchData(queryTerm query, bool soundex)
        {
            IEnumerable<InvertedIndexData> location;
            if (soundex)
            {
                location = data.Where(temp => (temp.soundex == query.soundex.ToUpper()));
            }
            else
            {
                location = data.Where(temp => (temp.token == query.term));
            }
            return location.ToList();
        }
        /// <summary>
        /// checks the inverted index for the term and if not found adds it else will add its location to the proper location
        /// </summary>
        /// <param name="invertedindex"></param>
        /// <param name="term"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InvertedIndexData> CheckIndex(List<InvertedIndexData> invertedindex, string term, string location)
        {
            bool isInIndex = false;
            var endPath = location.Split('\\').Last();

            int i = 0;
            //gets the index of the the term
            var index = invertedindex.IndexOf(invertedindex.FirstOrDefault(temp => temp.token == term));
            if (index == -1)
            {
                invertedindex.Add(new InvertedIndexData(term, endPath));
            } else
            {
                invertedindex[index].AddLocation(endPath);
                invertedindex[index].locations = invertedindex[index].locations.Distinct().ToList();
            }
            return invertedindex;
        }
        /// <summary>
        /// deletes a file from the inverted index
        /// </summary>
        /// <param name="path"></param>
        public void DeleteLocation(string path)
        {
            var endPath = path.Split('\\').Last();
            for(int i = 0; i < data.Count; i++)
            {
                data[i].locations.RemoveAll(p => p == endPath);
            }
        }
        public List<InvertedIndexData> Process(string path)
        {
            //opens the file 
            StreamReader reader = File.OpenText(path);
            IPluralize pluralizer = new Pluralizer();
            string line;
            FilePaths.Add(new FileData(path));
            //grabs the text from the document and proccesses it
            while ((line = reader.ReadLine()) != null)
            {
                
                //splits the line by there spaces
                string[] items = line.Split(' ');

                // Now let's find the path.
                List<string> list = new List<string>();
                foreach (string item in items)
                {
                    
                    string temp;
                    //Console.WriteLine(item);
                    //removes the special characters and replaces them with spaces
                    temp = Regex.Replace(item, @"[^\w\d\s]", "");
                    temp = temp.ToLower();
                    //makds all the words there singular form
                    temp = pluralizer.Singularize(temp);

                    //removes the terms that have numbers as their first or last character
                    if (string.IsNullOrWhiteSpace(temp) || "1234567890".Contains(temp.First()) || "1234567890".Contains(temp.Last()))
                        continue;
                    totalWordsParsed++;
                    FilePaths[FilePaths.Count - 1].totalWords++;
                    FilePaths[FilePaths.Count - 1].words.Add(item);
                    data = CheckIndex(data, temp, path);
                    //Console.WriteLine(temp);

                    list.Add(temp);
                }

                // At this point, `myInteger` and `path` contain the values we want
                // for the current line. We can then store those values or print them,
                // or anything else we like.
            }
            FilePaths[FilePaths.Count - 1].done();
            reader.Close();
            return data;
        }
        /// <summary>
        /// searches the data from simple query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="soundex"></param>
        /// <returns></returns>
        public List<string> Search(string query, bool soundex)
        {
            //uses the same processing as the inverted index
            #region 
            IPluralize pluralizer = new Pluralizer();
            string line;
            //grabs the text from the document and proccesses it
            string[] items = query.Split(' ');
            int reRun = 0;
            //int myInteger = int.Parse(items[1]);   // Here's your integer.

            // proccess the query and puts it in its own list of strings
            List<queryTerm> list = new List<queryTerm>();
            foreach (string item in items)
            {
                string temp;
                //Console.WriteLine(item);
                temp = item.ToLower();
                temp = Regex.Replace(temp, @"[^\w\d\s]", "");
                temp = pluralizer.Singularize(temp);

                if (string.IsNullOrWhiteSpace(temp) || "1234567890".Contains(temp.First()) /*|| "1234567890".Contains(temp.Last())*/)
                    continue;
                //Console.WriteLine(temp);


                list.Add(new queryTerm(temp));
            }
            #endregion
            //finds the and and or operators
            var locations = new List<string>();
            //searchs for the query when there is only 1 term
            if (list.Count == 1)
            {
                var searchData = SearchData(list[0], soundex);
                if (searchData != null)
                {
                    List<string> tempLocations = new List<string>();
                    foreach (var item in SearchData(list[0], soundex)) {
                        tempLocations = tempLocations.Concat(item.locations).ToList();
                    }
                    return tempLocations.Distinct().ToList();
                } else { return new List<string>(); }
            }
            //processes the query when there is more variables
            for (int i = 0; i < list.Count; i++)
            {
                //if the and keyword is used in the middle of the query
                if (list[i].term == "and" && i > 0 && i < list.Count-1)
                {
                    reRun++;
                    var temp1 = SearchData(list[i - 1], soundex);
                    var temp2 = SearchData( list[i + 1], soundex);
                    List<string> tempLocations1 = new List<string>();
                    foreach (var item in temp1)
                    {
                        tempLocations1 = tempLocations1.Concat(item.locations).ToList();
                    }
                    List<string> tempLocations2 = new List<string>();
                    foreach (var item in temp2)
                    {
                        tempLocations2 = tempLocations2.Concat(item.locations).ToList();
                    }

                    if (temp1 != null && temp2 != null)
                    {
                        foreach (var test in tempLocations1)
                        {
                            foreach (var test2 in tempLocations2)
                            {

                                if (test == test2)
                                {
                                    locations.Add(test);
                                }
                            }
                        }
                    }
                    locations = locations.Distinct().ToList();
                }
                //when the or is used in the middle of the query
                else if (list[i].term == "or" && i > 0 && i < list.Count-1)
                {
                    reRun++;
                    var temp1 = SearchData(list[i - 1], soundex);
                    var temp2 = SearchData(list[i + 1], soundex);
                    List<string> tempLocations1 = new List<string>();
                    foreach (var item in temp1)
                    {
                        tempLocations1 = tempLocations1.Concat(item.locations).ToList();
                    }
                    List<string> tempLocations2 = new List<string>();
                    foreach (var item in temp2)
                    {
                        tempLocations2 = tempLocations2.Concat(item.locations).ToList();
                    }


                    if (temp1 != null && temp2 == null)
                    {
                        foreach (var test in tempLocations1)
                        {
                            locations.Add(test);
                        }
                    }
                    else if (temp1 == null && temp2 != null)
                    {
                        foreach (var test in tempLocations2)
                        {

                            locations.Add(test);
                        }
                    }
                    else if (temp1 != null && temp2 != null)
                    {

                        foreach (var test in tempLocations1)
                        {
                            locations.Add(test);
                        }
                        foreach (var test2 in tempLocations2)
                        {
                            locations.Add(test2);
                        }
                        
                    } 
                    locations = locations.Distinct().ToList();
                }
            }
            //if there where no and or or's in the query reparse with and between every term
            if (reRun==0)
            {
                query = "";
                for(int i = 0; i < items.Length; i++)
                {
                    query += items[i] + " and ";
                    
                }
                //reruns the search with the new query
                locations = Search(query, soundex);
            }
            // returns the paths that have the terms in them
            return locations;
        }
    
    }
}
