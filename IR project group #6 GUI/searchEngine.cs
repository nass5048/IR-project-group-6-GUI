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

namespace IR_project_group__6_GUI
{
    public class searchEngine
    {
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
            //create some sort of loading so the user knows the program is working
            foreach (var file in files)
            {

                filesParsed++;
                data = Process(file);
            }
            //prints out the index
            data = Sort();
        }
        private List<InvertedIndexData> Sort()
        {
            data = data.OrderBy(i => i.token).ToList();
            return data;
        }
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
        
        private List<InvertedIndexData> checkIndex(List<InvertedIndexData> invertedindex, string test, string location)
        {
            bool isInIndex = false;
            var endPath = location.Split('\\').Last();

            int i = 0;
            if (/*test == "and" || test == "or" ||*/ test == "not")
            {
                return invertedindex;
            }
            var index = invertedindex.IndexOf(invertedindex.FirstOrDefault(temp => temp.token == test));
            if (index == -1)
            {
                invertedindex.Add(new InvertedIndexData(test, endPath));
            } else
            {
                invertedindex[index].AddLocation(endPath);
                invertedindex[index].locations = invertedindex[index].locations.Distinct().ToList();
            }
            return invertedindex;
        }
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

            StreamReader reader = File.OpenText(path);
            IPluralize pluralizer = new Pluralizer();
            string line;
            FilePaths.Add(new FileData(path));
            //grabs the text from the document and proccesses it
            while ((line = reader.ReadLine()) != null)
            {
                line = Regex.Replace(line, @"[^\w\d\s]", " ");
                string[] items = line.Split(' ');
                //int myInteger = int.Parse(items[1]);   // Here's your integer.

                // Now let's find the path.
                List<string> list = new List<string>();
                foreach (string item in items)
                {
                    
                    string temp;
                    //Console.WriteLine(item);
                    temp = item.ToLower();
                    
                    temp = pluralizer.Singularize(temp);

                    if (string.IsNullOrWhiteSpace(temp) || "1234567890".Contains(temp.First()) || "1234567890".Contains(temp.Last()))
                        continue;
                    totalWordsParsed++;
                    FilePaths[FilePaths.Count - 1].totalWords++;
                    FilePaths[FilePaths.Count - 1].words.Add(item);
                    data = checkIndex(data, temp, path);
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
        //need to figure out how to create the inverted index probably do this in the process function
        public List<string> Search(string query, bool soundex)
        {
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
                if (list.Count >= 2 && list[list.Count - 2].term == "not")
                {
                    list[list.Count - 1].op = false;
                    list.RemoveAll(t => t.term == "not");
                }
            }
            //finds the and not and or operators
            var locations = new List<string>();
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
            for (int i = 0; i < list.Count; i++)
            {
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
            if (reRun==0)
            {
                query = "";
                for(int i = 0; i < items.Length; i++)
                {
                    query += items[i] + " and ";
                    
                }
                locations = Search(query, soundex);
            }
            // At this point, `myInteger` and `path` contain the values we want
            // for the current line. We can then store those values or print them,
            // or anything else we like.
            return locations;
        }
    
    }
}
