using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAMFS
{
    class Program
    {
        static private string[] _txtToIndex;
        static List<string> _stopWords;
        static void Main(string[] args)
        {
            const string txtToIndexPath = @"C:\Users\pen\Documents\Visual Studio 2013\Projects\FAMFS\FAMFS\textes\text1.txt";
            var fileName = @"C:\Users\pen\Documents\Visual Studio 2013\Projects\FAMFS\FAMFS\textes\fr.txt";
            //var txtToIndexPath = Environment.CurrentDirectory + @"\textes\text1.txt";

           
            LoadStopWords(fileName, out _stopWords);
            var txtToIndex2 = File.ReadAllLines(txtToIndexPath);
            _txtToIndex = new string[txtToIndex2.Length];
            for (int i = 0; i < txtToIndex2.Length; i++)
            {
                _txtToIndex[i] = StringWordsRemove(txtToIndex2[i]);
            }
            var pairs = new List<Pair>();
            foreach (var s in _txtToIndex)
            {
                pairs.AddRange(GetPairs(s.TrimEnd().TrimStart()));

            }

            var frequantPairs = FrequentPairs(_txtToIndex, pairs);
            var ngrams = FindAllMaxSeq(frequantPairs.ToList());
            foreach (var ngram in ngrams)
            {
                Console.WriteLine(ngram);
            }
            var final = new List<String>();
            Console.ReadKey();
        }

        private static void LoadStopWords(string fileName, out List<string> stopWords)
        {
            stopWords = new List<string>();
            using (StreamReader reader = new StreamReader(fileName, Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    stopWords.Add(" " + line + " "); // Add to list.
            }
        }


        static IEnumerable<Pair> GetPairs(string line)
        {
            var result = new List<Pair>();
            var lines = line.Split(' ');
            for (int i = 0; i < lines.Count(); i++)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], "^[a-zA-Z0-9\x20]+$"))
                {
                    
                }
                var key = lines[i].ToLower();
                var j = i + 1;
                if (j < lines.Length)
                {
                    while (j < lines.Length)
                    {
                        if (key.ToLower() != lines[j].ToLower().Trim() && !_stopWords.Any(x => x.Contains(key)) || !_stopWords.Any(x => x.Contains(lines[j].ToLower().Trim())))
                            result.Add(new Pair
                            {
                                Word1 = key.Trim(),
                                Word2 = lines[j].Trim().ToLower()
                            });

                        j++;
                    }
                }
                else
                {
                    break;
                }

            }
            return result;
        }

        static List<String> FrequentPairs(string[] S, IEnumerable<Pair> list)
        {
            var result = new List<String>();
            foreach (var pair in list.Distinct())
            {
                string[] sf = {pair.Word1, pair.Word2.Trim()};
                int b = 0;
                foreach (var s in _txtToIndex)
                {
                    if( GetOccurence(s.Split(' '), sf))  b++;
                }
                if (b > 1)
                {
                    if (pair.Word1.Trim()!=pair.Word2.Trim()&&!result.Any(x => x.Equals(pair.Word1.Trim() + " " + pair.Word2.Trim())))
                    result.Add(pair.Word1.Trim() + " " + pair.Word2.Trim());
                }

            }
            return result.Distinct().ToList();
        }

        static List<String> FindAllMaxSeq(List<string> pairs)
        {
            var result = new List<string>();

            //int k = 1;
            foreach (var pair in pairs)
            {
                var ngram = "";
                
                for (int j = 0; j < pair.TrimEnd().TrimStart().Split(' ').Length; j++)
                {
                    for (int k = 1; k < pairs.Count; k++)
                    {
                        var addedWord = pairs[k].Split(' ')[j];

                        if (string.IsNullOrEmpty(ngram))
                        {
                          
                                ngram = pair + " " + addedWord.Trim();
                        }
                        else
                        {
                            if (ngram.IndexOf(addedWord, StringComparison.Ordinal) == -1) ngram = ngram.TrimEnd().TrimStart() + " " + addedWord.Trim();
                            else
                            {
                                // k--;
                                continue;
                            }
                        }
                        if (!string.IsNullOrEmpty(ngram))
                        {
                            int q = 0;
                            ngram = ngram.TrimStart().TrimEnd();
                            var items = ngram.TrimStart().TrimEnd().Split(' ');
                            string ngram1 = ngram;
                            Parallel.ForEach(_txtToIndex, s =>
                            {
                                if (GetOccurence(s.Split(' '), ngram1.Split(' '))) q++;
                            });
                           
                            if (q < 2)
                            {
                                int index = items.Length - 1;
                                ngram = items.Take(index).Aggregate("", (current, item) => current + " " + item).TrimEnd();
                            }
                        }
                    }
                }
                if (ngram != "")
                {
                    ngram = ngram.Replace('.', ' ').TrimEnd().TrimStart();
                    int q = 0;
                    Parallel.ForEach(result, r =>
                    {
                        if (GetOccurence(r.Split(' '), ngram.Split(' '))) q++;
                    });
                    //foreach (var r in result)
                    //{
                    //    if (GetOccurence(r.Split(' '), ngram.Split(' '))) q++; 
                    //}
                    if (q ==0 )result.Add(ngram);
                }
            }
            return result.Distinct().ToList();
        }

     
        static bool GetOccurence(string[] ss, string[] sf)
        {
            var c = new int[sf.Length];
            int k = 0;

            for (int x = 0; x < ss.Length; x++)
            {
                for (int y = k; y < sf.Length; y++)
                {
                    c[y] +=sf[y].Trim().ToLower()==ss[x].Trim().ToLower()? 1 : 0;
                    if (c[y] == 1)
                    {
                        k++;
                        break;
                    }
                    
                }
                if (k==sf.Length)break;
            }
            foreach (var i in c)
            {
                if (i == 0) return false;
            }
            return true;
        }
        static string StringWordsRemove(string stringToClean)
        {
            // Define how to tokenize the input string, i.e. space only or punctuations also
            return string.Join(" ", stringToClean
                .Split(new[] { '/', '«', '»', '"', '"', ')', '(', ':', ';', ',', '.', '?', '!','-' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
    

    public class Pair
    {
        public string Word1 { get; set; }
        public string Word2 { get; set; }
    }
}
