using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSE3
{
    public class Program_Query
    {
        private static bool Iterated_Quary = false;

        private static Dictionary<string, int> Cache_Doc_Amount_Containing_Word;
        private static Dictionary<string, Dictionary<int, int>> Cache_Term_Frequency_In_Doc;
        private static Dictionary<string, int> Cache_Word_Index_Line;
        private static Dictionary<int, int> Cache_Doc_Word_Amount;
        private static Dictionary<int, string> Cache_Url;

        private static int Average_Word_Amount;

        private static string Query_String;
        private static List<string> Keywords;
        private static List<string> Keywords_History;

        private static double[] doc_score = new double[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static int[] doc_id = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private const double Power_Removed = 0.5;
        private const double Power_Added = 0.5;

        public static void Go()
        {
            Directory.CreateDirectory(Program.Directory_Output + "Ultimate Lexicon");

            Cache_Doc_Amount_Containing_Word = new Dictionary<string, int>();
            Cache_Term_Frequency_In_Doc = new Dictionary<string, Dictionary<int, int>>();
            Cache_Word_Index_Line = new Dictionary<string, int>();
            Cache_Doc_Word_Amount = new Dictionary<int, int>();
            Cache_Url = new Dictionary<int, string>();

            Process_Cache_Doc_Word_Amount();
            Process_Cache_Url();

            Read_Query();

            Console.ReadLine();
        }

        private static void Read_Query()
        {
            Console.WriteLine("Input Query");

            Query_String = Console.ReadLine();
            Keywords = Query_String.Split(' ').ToList();
            for (var i = 0; i < Keywords.Count; i++)
            {
                Keywords[i] = Keywords[i].Trim();
            }

            if (!Iterated_Quary)
            {
                Keywords_History = Keywords;

                for (var i = Program.Global_Doc_Amount - 1; i > 0; i--)
                {
                    var totalscore = 0.0;
                    foreach (string s in Keywords)
                    {
                        totalscore += Score(s, i);
                    }

                    var j = 0;

                    while (j < doc_score.Length && totalscore > doc_score[j])
                    {
                        j++;
                    }

                    j--;

                    if (j >= 0)
                    {
                        doc_score[j] = totalscore;
                        doc_id[j] = i;
                    }
                }

                Output_Query(doc_id);

                Iterated_Quary = true;
            }
            else
            {
                //Here's the fun part.

                List<string> Added_Keywords = new List<string>();
                List<string> Removed_Keywords = new List<string>();

                foreach (string s in Keywords_History)
                {
                    if (!Keywords.Contains(s))
                    {
                        Removed_Keywords.Add(s);
                    }
                }

                foreach (string s in Keywords)
                {
                    if (!Keywords_History.Contains(s))
                    {
                        Added_Keywords.Add(s);
                    }
                }

                for (var i = 0; i < doc_id.Length; i++)
                {
                    foreach (string s in Removed_Keywords)
                    {
                        doc_score[i] -= Score(s, doc_id[i]) * Power_Removed;
                    }
                    foreach (string s in Added_Keywords)
                    {
                        doc_score[i] += Score(s, doc_id[i]) * Power_Added;
                    }
                }

                double[] new_doc_score = new double[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] new_doc_id = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                for (var i = Program.Global_Doc_Amount - 1; i > 0; i--)
                {
                    var totalscore = 0.0;
                    foreach (string s in Keywords)
                    {
                        totalscore += Score(s, i);
                    }

                    var j = 0;

                    while (j < new_doc_score.Length && totalscore > new_doc_score[j])
                    {
                        j++;
                    }

                    j--;

                    if (j >= 0)
                    {
                        new_doc_score[j] = totalscore;
                        new_doc_id[j] = i;
                    }
                }

                int[] final_doc_id = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                int po = 0;
                int pn = 0;
                int p = 0;

                while (p < final_doc_id.Length)
                {
                    if (doc_score[po] < new_doc_score[pn])
                    {
                        final_doc_id[p] = new_doc_id[pn];
                        pn++;
                        p++;
                    }
                    else
                    {
                        final_doc_id[p] = doc_id[po];
                        po++;
                        p++;
                    }
                }

                Output_Query(final_doc_id);
            }
        }

        private static void Output_Query(int[] doc_id)
        {
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine("Number " + i + " Result: " + Cache_Url[doc_id[i]]);
            }

            Read_Query();
        }

        private static void Output_Query(int[] doc_id, int[] new_doc_id)
        {

        }

        private static double Score(string word, int doc_id)
        {
            double k1 = 1.6;
            double b = 0.75;
            int N = Program.Global_Doc_Amount;
            double finalscore = 0.0;

            int nq = Get_Doc_Amount_Containing_Word(word);
            int fqd = Get_Term_Frequency_In_Doc(word, doc_id);
            double IDF = (N - nq + 0.5) / (nq + 0.5);
            double secondpart = Get_Term_Frequency_In_Doc(word, doc_id) * (k1 + 1) / (Get_Term_Frequency_In_Doc(word, doc_id) + k1 * (1 - b + b * Cache_Doc_Word_Amount[doc_id] / Average_Word_Amount));
            finalscore = IDF * secondpart;

            if (secondpart != 0.0)
            {
                var a = 1;
            }

            //Frequency in Document D
            //Document Length
            //Average Document Length in collection

            return finalscore;
        }

        private static void Process_Cache_Doc_Word_Amount()
        {
            Average_Word_Amount = 0;

            TextReader TR = new StreamReader(Program.Directory_Output + "/Word Amount/amount.txt", Encoding.ASCII);

            var s = TR.ReadLine();

            while (s != null)
            {
                var ss = s.Split(' ');
                Cache_Doc_Word_Amount[Int32.Parse(ss[0])] = Int32.Parse(ss[1]);
                Average_Word_Amount += Int32.Parse(ss[1]);
                s = TR.ReadLine();
            }

            Average_Word_Amount /= Program.Global_Doc_Amount;
        }

        private static void Process_Cache_Url()
        {
            TextReader TR = new StreamReader(Program.Directory_Normalization + "/Index/index.txt", Encoding.ASCII);
            string s = TR.ReadLine();
            while (s != null)
            {
                var ss = s.Split(' ');
                Cache_Url[Int32.Parse(ss[0])] = ss[1];
                s = TR.ReadLine();
            }
        }

        private static int Get_Doc_Amount_Containing_Word(string word)
        {
            if (Cache_Doc_Amount_Containing_Word.ContainsKey(word))
            {
                return Cache_Doc_Amount_Containing_Word[word];
            }
            else
            {
                TextReader TR = new StreamReader(Program.Directory_Output + "/lexicon/lexicon.txt", Encoding.ASCII);

                var s = TR.ReadLine();
                var ss = s.Split(' ');

                while (s != null && string.Compare(ss[0], word) <= 0)
                {
                    Cache_Word_Index_Line[word] = Int32.Parse(ss[1]);

                    if (ss[0] == word)
                    {
                        var s2 = TR.ReadLine();
                        if (s2 != null)
                        {
                            var s2s = s2.Split(' ');
                            TR.Close();
                            Cache_Doc_Amount_Containing_Word[word] = Int32.Parse(s2s[1]) - Int32.Parse(ss[1]);
                            return Int32.Parse(s2s[1]) - Int32.Parse(ss[1]);
                        }
                        else
                        {
                            s = TR.ReadLine();
                            TR.Close();
                        }
                    }
                    else
                    {
                        s = TR.ReadLine();
                        ss = s == null ? new string[1] { "" } : s.Split(' ');
                    }
                }

                TR.Close();
                return -1;
            }
        }

        private static int Get_Term_Frequency_In_Doc(string word, int doc_id)
        {
            if (Cache_Term_Frequency_In_Doc.ContainsKey(word))
            {
                if (Cache_Term_Frequency_In_Doc[word].ContainsKey(doc_id))
                {
                    return Cache_Term_Frequency_In_Doc[word][doc_id];
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (File.Exists(Program.Directory_Output + "Ultimate Lexicon/lexicon " + word + ".txt"))
                {
                    Cache_Term_Frequency_In_Doc[word] = new Dictionary<int, int>();
                    TextReader TR = new StreamReader(Program.Directory_Output + "/Ultimate Lexicon/lexicon " + word + ".txt", Encoding.ASCII);
                    string s = TR.ReadLine();
                    while (s != null)
                    {
                        var ss = s.Split(' ');
                        Cache_Term_Frequency_In_Doc[word][Int32.Parse(ss[0])] = Int32.Parse(ss[1]);
                        s = TR.ReadLine();
                    }
                    TR.Close();

                    if (Cache_Term_Frequency_In_Doc[word].ContainsKey(doc_id))
                    {
                        return Cache_Term_Frequency_In_Doc[word][doc_id];
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
