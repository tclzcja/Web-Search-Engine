using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WSE3.Class;

namespace WSE3
{
    public class Program_Index
    {
        private const int Current_Process_Range = 100;
        private static List<Doc> List_Index;

        public static void Go()
        {
            Directory.CreateDirectory(Program.Directory_Normalization);
            Directory.CreateDirectory(Program.Directory_Normalization + "Doc");
            Directory.CreateDirectory(Program.Directory_Normalization + "Index");
            Directory.CreateDirectory(Program.Directory_Output);
            Directory.CreateDirectory(Program.Directory_Output + "Inverted Index 1");
            Directory.CreateDirectory(Program.Directory_Output + "Inverted Index 10");
            Directory.CreateDirectory(Program.Directory_Output + "Inverted Index 100");
            Directory.CreateDirectory(Program.Directory_Output + "Inverted Index 1000");
            Directory.CreateDirectory(Program.Directory_Output + "Inverted Index 10000");
            Directory.CreateDirectory(Program.Directory_Output + "Inverted Index Ultimate");
            Directory.CreateDirectory(Program.Directory_Output + "Lexicon");
            Directory.CreateDirectory(Program.Directory_Output + "Word Amount");

            DateTime Start = System.DateTime.UtcNow;
            Make_Doc();
            Make_Inverted_Index_1();
            Make_Inverted_Index_10();
            Make_Inverted_Index_100();
            Make_Inverted_Index_1000();
            Make_Inverted_Index_10000();
            Make_Inverted_Index_Ultimate();
            Process_Cache_Doc_Term_Frequency();
            DateTime End = System.DateTime.UtcNow;

            TimeSpan TS = End - Start;

            Console.WriteLine("Time Costing " + TS.Minutes + " minutes " + TS.Seconds + " seconds");

            Console.ReadLine();
        }

        private static void Make_Doc()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Normalization files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Normalization + "Doc/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting all Normalization files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            Console.WriteLine("Processing : Loading all data from the Data folder and Normalizing them into Normalization folder");

            List_Index = new List<Doc>();

            TextWriter TW_Index = File.CreateText(Program.Directory_Normalization + "Index/Index.txt");
            int doc_id = 0;

            for (var data_source_id = 0; data_source_id < Current_Process_Range; data_source_id++)
            {
                Console.WriteLine("Normalizing Data Set " + data_source_id.ToString() + "/" + Current_Process_Range);

                TextReader TR_Index = new StreamReader(Program.Directory_Data + "/tmpindex_" + data_source_id.ToString(), Encoding.ASCII);
                TextReader TR_Doc = new StreamReader(Program.Directory_Data + data_source_id.ToString() + "_data~", Encoding.ASCII);

                while (true)
                {
                    var line = TR_Index.ReadLine();
                    if (line != null)
                    {
                        var sl = line.Split(' ');

                        var L = new Doc()
                        {
                            ID = doc_id,
                            Url = sl[0],
                            IP = sl[4],
                            Length = Int32.Parse(sl[3]),
                            Data_Source_ID = data_source_id
                        };
                        List_Index.Add(L);

                        var ns = doc_id + " " + sl[0] + " " + sl[4] + " " + sl[3] + " " + data_source_id.ToString();
                        TW_Index.WriteLine(ns);

                        // Deal with the Page Document
                        TextWriter TW_Doc = File.CreateText(Program.Directory_Normalization + "Doc/" + L.ID.ToString() + ".txt");
                        var old_char = 'a';
                        for (var j = 0; j < L.Length; j++)
                        {
                            var c = (char)TR_Doc.Read();
                            // If c is not control characters, not surrogate characters, not another space in sequence
                            // Then write it into the doc
                            if (!char.IsControl(c) && !char.IsSurrogate(c) && !(old_char == ' ' && c == ' '))
                            {
                                TW_Doc.Write(c);
                                old_char = c;
                            }
                        }
                        TW_Doc.Close();
                        doc_id++;
                    }
                    else
                    {
                        break;
                    }
                }

                TR_Index.Close();
                TR_Doc.Close();
            }
            /*
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<Index>));
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"../../Normalization/Index/Index.xml", false);
            writer.Serialize(file, List_Index);
            file.Close();
            */
            TW_Index.Close();
            Program.Global_Doc_Amount = doc_id;
        }

        private static string Strip(string incoming)
        {
            var result = "";
            for (var i = 0; i < incoming.Length; i++)
            {
                if (char.IsLetterOrDigit(incoming[i]))
                {
                    result += incoming[i];
                }
            }
            return result.ToLower();
        }

        private static void Go_Through_Node(Dictionary<string, List<int>> HT, HtmlAgilityPack.HtmlNode Current, Word_Amount Word_Amount)
        {
            if (Current != null)
            {
                if (Current.ChildNodes.Count == 0)
                {
                    var cit = Current.InnerText.Split(' ');
                    foreach (string s in cit)
                    {
                        // Remove all the punctuation, all the spaces and convert into lower case
                        var ss = Strip(s);
                        if (ss != "" && ss != "&nbsp" && ss.Length < 20) // If the processed string is not a space or nothing or too long, then go on
                        {
                            if (!HT.ContainsKey(ss))
                            {
                                HT[ss] = new List<int>();
                            }

                            HT[ss].Add(Current.StreamPosition);

                            Word_Amount.Amount++;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < Current.ChildNodes.Count; i++)
                    {
                        Go_Through_Node(HT, Current.ChildNodes[i], Word_Amount);
                    }
                }
            }
        }

        private static void Make_Inverted_Index_1()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Inverted Index 1 files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Output + "Inverted Index 1/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting all Inverted Index 1 files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            fileset = Directory.GetFiles(Program.Directory_Output + "Word Amount/");
            foreach (var file in fileset)
            {
                File.Delete(file);
            }

            TextWriter TW_Word_Amount = File.CreateText(Program.Directory_Output + "Word Amount/amount.txt");

            for (var doc_id = 0; doc_id < Program.Global_Doc_Amount; doc_id++)
            {
                var WA = new Word_Amount();

                Console.WriteLine("Generating Inverted Index 1 from Doc " + doc_id.ToString() + "/" + Program.Global_Doc_Amount);

                HtmlDocument doc = new HtmlDocument();
                TextReader TR = new StreamReader(Program.Directory_Normalization + "Doc/" + doc_id.ToString() + ".txt");
                TextWriter TW = File.CreateText(Program.Directory_Output + "Inverted Index 1/" + doc_id.ToString() + ".txt");
                doc.Load(TR);

                Dictionary<string, List<int>> Dic = new Dictionary<string, List<int>>();
                Go_Through_Node(Dic, doc.DocumentNode.SelectSingleNode("/html"), WA);

                TW_Word_Amount.WriteLine(doc_id + " " + WA.Amount);

                List<Index> LL = new List<Index>();

                foreach (var i in Dic)
                {
                    Index L = new Index()
                    {
                        ID = i.Key,
                        Doc_ID = doc_id,
                        Position = i.Value
                    };

                    LL.Add(L);
                }

                LL = LL.OrderBy(l => l.ID).ToList();

                for (var i = 0; i < LL.Count; i++)
                {
                    var outputline = LL[i].ID + " " + LL[i].Doc_ID.ToString() + " " + LL[i].Position.Count + " ";
                    foreach (var p in LL[i].Position)
                    {
                        outputline += p.ToString() + " ";
                    }
                    TW.WriteLine(outputline);
                }

                TR.Close();
                TW.Close();
                /*
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<Lexicon>));
                System.IO.StreamWriter file = new System.IO.StreamWriter(@"../../Output/Lexicon/" + doc_id.ToString() + ".xml", false);
                writer.Serialize(file, LL);
                file.Close();*/
            }

            TW_Word_Amount.Close();

        }

        private static void Make_Inverted_Index_10()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Inverted Index 10 files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Output + "Inverted Index 10/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting all Inverted Index 10 files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            int current_lexicon = 0;
            int current_inverted_index_10 = 0;

            while (current_lexicon < Program.Global_Doc_Amount)
            {
                TextWriter TW = File.CreateText(Program.Directory_Output + "Inverted Index 10/" + current_inverted_index_10.ToString() + ".txt");

                List<TextReader> Pipeline = new List<TextReader>();
                List<string> Pipeline_String = new List<string>();

                // Load 10 TextReader into Pipeline
                for (var i = 0; i < 10; i++)
                {
                    if (current_lexicon < Program.Global_Doc_Amount)
                    {
                        Console.WriteLine("Load Lexicon " + current_lexicon);
                        TextReader TR = new StreamReader(Program.Directory_Output + "Inverted Index 1/" + current_lexicon.ToString() + ".txt");
                        var s = TR.ReadLine();
                        if (s != null)
                        {
                            Pipeline.Add(TR);
                            Pipeline_String.Add(s);
                        }
                        current_lexicon++;
                    }
                }

                while (Pipeline.Count > 0)
                {
                    int p = 0;
                    string s = Pipeline_String[0] == null ? "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" : Pipeline_String[0];

                    // Look for the "Biggest" string in the pipe
                    for (var i = 0; i < Pipeline_String.Count; i++)
                    {
                        if (Pipeline_String[i] != null)
                        {
                            var ss1 = s.Split(' ');
                            var ss2 = Pipeline_String[i].Split(' ');
                            var flag = string.Compare(ss1[0], ss2[0]);

                            if (flag > 0)
                            {
                                p = i;
                                s = Pipeline_String[i];
                            }
                            else if (flag == 0)
                            {
                                if (Int32.Parse(ss1[1]) > Int32.Parse(ss2[1]))
                                {
                                    p = i;
                                    s = Pipeline_String[i];
                                }
                            }
                        }
                    }

                    // Write the string and Read that position for next string
                    TW.WriteLine(Pipeline_String[p]);
                    Pipeline_String[p] = Pipeline[p].ReadLine();
                    if (Pipeline_String[p] == null)
                    {
                        Pipeline[p].Close();
                        Pipeline.RemoveAt(p);
                        Pipeline_String.RemoveAt(p);
                    }
                }

                Console.WriteLine("Finish Constructing Inverted Index 10 - " + current_inverted_index_10);

                TW.Close();
                current_inverted_index_10++;
            }

            Program.Global_Inverted_Index_10_Amount = current_inverted_index_10;
        }

        private static void Make_Inverted_Index_100()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Inverted Index 100 files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Output + "Inverted Index 100/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting all Inverted Index 100 files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            int current_inverted_index_10 = 0;
            int current_inverted_index_100 = 0;

            while (current_inverted_index_10 < Program.Global_Inverted_Index_10_Amount)
            {
                TextWriter TW = File.CreateText(Program.Directory_Output + "Inverted Index 100/" + current_inverted_index_100.ToString() + ".txt");

                List<TextReader> Pipeline = new List<TextReader>();
                List<string> Pipeline_String = new List<string>();

                // Load 10 TextReader into Pipeline
                for (var i = 0; i < 10; i++)
                {
                    if (current_inverted_index_10 < Program.Global_Inverted_Index_10_Amount)
                    {
                        Console.WriteLine("Load Inverted Index 10 " + current_inverted_index_10);
                        TextReader TR = new StreamReader(Program.Directory_Output + "Inverted Index 10/" + current_inverted_index_10.ToString() + ".txt");
                        var s = TR.ReadLine();
                        if (s != null)
                        {
                            Pipeline.Add(TR);
                            Pipeline_String.Add(s);
                        }
                        current_inverted_index_10++;
                    }
                }

                while (Pipeline.Count > 0)
                {
                    int p = 0;
                    string s = Pipeline_String[0] == null ? "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" : Pipeline_String[0];

                    // Look for the "Biggest" string in the pipe
                    for (var i = 0; i < Pipeline_String.Count; i++)
                    {
                        if (Pipeline_String[i] != null)
                        {
                            var ss1 = s.Split(' ');
                            var ss2 = Pipeline_String[i].Split(' ');
                            var flag = string.Compare(ss1[0], ss2[0]);

                            if (flag > 0)
                            {
                                p = i;
                                s = Pipeline_String[i];
                            }
                            else if (flag == 0)
                            {
                                if (Int32.Parse(ss1[1]) > Int32.Parse(ss2[1]))
                                {
                                    p = i;
                                    s = Pipeline_String[i];
                                }
                            }
                        }
                    }

                    // Write the string and Read that position for next string
                    TW.WriteLine(Pipeline_String[p]);
                    Pipeline_String[p] = Pipeline[p].ReadLine();
                    if (Pipeline_String[p] == null)
                    {
                        Pipeline[p].Close();
                        Pipeline.RemoveAt(p);
                        Pipeline_String.RemoveAt(p);
                    }
                }

                Console.WriteLine("Finish Constructing Inverted Index 100 - " + current_inverted_index_100);

                TW.Close();
                current_inverted_index_100++;
            }

            Program.Global_Inverted_Index_100_Amount = current_inverted_index_100;
        }

        private static void Make_Inverted_Index_1000()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Inverted Index 1000 files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Output + "Inverted Index 1000/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting all Inverted Index 1000 files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            int current_inverted_index_100 = 0;
            int current_inverted_index_1000 = 0;

            while (current_inverted_index_100 < Program.Global_Inverted_Index_100_Amount)
            {
                TextWriter TW = File.CreateText(Program.Directory_Output + "Inverted Index 1000/" + current_inverted_index_1000.ToString() + ".txt");

                List<TextReader> Pipeline = new List<TextReader>();
                List<string> Pipeline_String = new List<string>();

                // Load 10 TextReader into Pipeline
                for (var i = 0; i < 10; i++)
                {
                    if (current_inverted_index_100 < Program.Global_Inverted_Index_100_Amount)
                    {
                        Console.WriteLine("Load Inverted Index 100 " + current_inverted_index_100);
                        TextReader TR = new StreamReader(Program.Directory_Output + "Inverted Index 100/" + current_inverted_index_100.ToString() + ".txt");
                        var s = TR.ReadLine();
                        if (s != null)
                        {
                            Pipeline.Add(TR);
                            Pipeline_String.Add(s);
                        }
                        current_inverted_index_100++;
                    }
                }

                while (Pipeline.Count > 0)
                {
                    int p = 0;
                    string s = Pipeline_String[0] == null ? "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" : Pipeline_String[0];

                    // Look for the "Biggest" string in the pipe
                    for (var i = 0; i < Pipeline_String.Count; i++)
                    {
                        if (Pipeline_String[i] != null)
                        {
                            var ss1 = s.Split(' ');
                            var ss2 = Pipeline_String[i].Split(' ');
                            var flag = string.Compare(ss1[0], ss2[0]);

                            if (flag > 0)
                            {
                                p = i;
                                s = Pipeline_String[i];
                            }
                            else if (flag == 0)
                            {
                                if (Int32.Parse(ss1[1]) > Int32.Parse(ss2[1]))
                                {
                                    p = i;
                                    s = Pipeline_String[i];
                                }
                            }
                        }
                    }

                    // Write the string and Read that position for next string
                    TW.WriteLine(Pipeline_String[p]);
                    Pipeline_String[p] = Pipeline[p].ReadLine();
                    if (Pipeline_String[p] == null)
                    {
                        Pipeline[p].Close();
                        Pipeline.RemoveAt(p);
                        Pipeline_String.RemoveAt(p);
                    }
                }

                Console.WriteLine("Finish Constructing Inverted Index 1000 - " + current_inverted_index_1000);

                TW.Close();
                current_inverted_index_1000++;
            }

            Program.Global_Inverted_Index_1000_Amount = current_inverted_index_1000;
        }

        private static void Make_Inverted_Index_10000()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Inverted Index 10000 files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Output + "Inverted Index 10000/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting all Inverted Index 10000 files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            int current_inverted_index_1000 = 0;
            int current_inverted_index_10000 = 0;

            while (current_inverted_index_1000 < Program.Global_Inverted_Index_1000_Amount)
            {
                TextWriter TW = File.CreateText(Program.Directory_Output + "Inverted Index 10000/" + current_inverted_index_10000.ToString() + ".txt");

                List<TextReader> Pipeline = new List<TextReader>();
                List<string> Pipeline_String = new List<string>();

                // Load 10 TextReader into Pipeline
                for (var i = 0; i < 10; i++)
                {
                    if (current_inverted_index_1000 < Program.Global_Inverted_Index_1000_Amount)
                    {
                        Console.WriteLine("Load Inverted Index 1000 " + current_inverted_index_1000);
                        TextReader TR = new StreamReader(Program.Directory_Output + "Inverted Index 1000/" + current_inverted_index_1000.ToString() + ".txt");
                        var s = TR.ReadLine();
                        if (s != null)
                        {
                            Pipeline.Add(TR);
                            Pipeline_String.Add(s);
                        }
                        current_inverted_index_1000++;
                    }
                }

                while (Pipeline.Count > 0)
                {
                    int p = 0;
                    string s = Pipeline_String[0] == null ? "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" : Pipeline_String[0];

                    // Look for the "Biggest" string in the pipe
                    for (var i = 0; i < Pipeline_String.Count; i++)
                    {
                        if (Pipeline_String[i] != null)
                        {
                            var ss1 = s.Split(' ');
                            var ss2 = Pipeline_String[i].Split(' ');
                            var flag = string.Compare(ss1[0], ss2[0]);

                            if (flag > 0)
                            {
                                p = i;
                                s = Pipeline_String[i];
                            }
                            else if (flag == 0)
                            {
                                if (Int32.Parse(ss1[1]) > Int32.Parse(ss2[1]))
                                {
                                    p = i;
                                    s = Pipeline_String[i];
                                }
                            }
                        }
                    }

                    // Write the string and Read that position for next string
                    TW.WriteLine(Pipeline_String[p]);
                    Pipeline_String[p] = Pipeline[p].ReadLine();
                    if (Pipeline_String[p] == null)
                    {
                        Pipeline[p].Close();
                        Pipeline.RemoveAt(p);
                        Pipeline_String.RemoveAt(p);
                    }
                }

                Console.WriteLine("Finish Constructing Inverted Index 10000 - " + current_inverted_index_10000);

                TW.Close();
                current_inverted_index_10000++;
            }

            Program.Global_Inverted_Index_10000_Amount = current_inverted_index_10000;
        }

        private static void Make_Inverted_Index_Ultimate()
        {
            var line = 0;
            Dictionary<string, int> lexicon = new Dictionary<string, int>();

            Console.WriteLine("Deleting all Inverted Index Ultimate files...");

            File.Delete(Program.Directory_Output + "Inverted Index Ultimate/index.txt");
            File.Delete(Program.Directory_Output + "Lexicon/lexicon.txt");

            TextWriter TW = File.CreateText(Program.Directory_Output + "Inverted Index Ultimate/index.txt");
            TextWriter LW = File.CreateText(Program.Directory_Output + "Lexicon/lexicon.txt");

            List<TextReader> Pipeline = new List<TextReader>();
            List<string> Pipeline_String = new List<string>();

            // Load 10 TextReader into Pipeline
            for (var i = 0; i < Program.Global_Inverted_Index_10000_Amount; i++)
            {
                Console.WriteLine("Load All Inverted Index 10000 " + i.ToString());
                TextReader TR = new StreamReader(Program.Directory_Output + "Inverted Index 10000/" + i.ToString() + ".txt");
                var s = TR.ReadLine();
                if (s != null)
                {
                    Pipeline.Add(TR);
                    Pipeline_String.Add(s);
                }
            }

            while (Pipeline.Count > 0)
            {
                int p = 0;
                string s = Pipeline_String[0] == null ? "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" : Pipeline_String[0];

                // Look for the "Biggest" string in the pipe
                for (var i = 0; i < Pipeline_String.Count; i++)
                {
                    if (Pipeline_String[i] != null)
                    {
                        var ss1 = s.Split(' ');
                        var ss2 = Pipeline_String[i].Split(' ');
                        var flag = string.Compare(ss1[0], ss2[0]);

                        if (flag > 0)
                        {
                            p = i;
                            s = Pipeline_String[i];
                        }
                        else if (flag == 0)
                        {
                            if (Int32.Parse(ss1[1]) > Int32.Parse(ss2[1]))
                            {
                                p = i;
                                s = Pipeline_String[i];
                            }
                        }
                    }
                }

                // Write the string and Read that position for next string
                TW.WriteLine(s);
                Pipeline_String[p] = Pipeline[p].ReadLine();
                line++;

                var ss = s.Substring(0, s.IndexOf(' '));

                if (!lexicon.ContainsKey(ss))
                {
                    lexicon[ss] = line;
                    LW.WriteLine(ss + " " + line.ToString());
                }

                if (Pipeline_String[p] == null)
                {
                    Pipeline[p].Close();
                    Pipeline.RemoveAt(p);
                    Pipeline_String.RemoveAt(p);
                }
            }

            Console.WriteLine("Finish Constructing Inverted Index Ultimate");
            TW.Close();
            LW.Close();
        }

        private static void Process_Cache_Doc_Term_Frequency()
        {
            int deleteflag = 0;

            Console.WriteLine("Deleting all Ultimate Lexicon files...");

            string[] fileset = Directory.GetFiles(Program.Directory_Output + "Ultimate Lexicon/");
            foreach (var file in fileset)
            {
                File.Delete(file);
                Console.WriteLine("Deleting Ultimate Lexicon files " + deleteflag.ToString() + "/" + fileset.Length);
                deleteflag++;
            }

            TextReader TR = new StreamReader(Program.Directory_Output + "/Inverted Index Ultimate/index.txt", Encoding.ASCII);
            TextWriter TW;

            var s = TR.ReadLine();
            TW = File.CreateText(Program.Directory_Output + "Ultimate Lexicon/" + s.Split(' ')[0] + ".txt");

            while (s != null)
            {
                var ss = s.Split(' ');

                TW.WriteLine(ss[1] + " " + ss[2]);

                s = TR.ReadLine();

                var ns = s == null ? new string[1] { "" } : s.Split(' ');
                if (ns[0] != "" && ns[0] != ss[0])
                {
                    TW.Close();
                    TW = File.CreateText(Program.Directory_Output + "Ultimate Lexicon/lexicon " + ns[0] + ".txt");
                    Console.WriteLine("Constructing Ultimate Lexicon for word : " + ns[0]);
                }
            }

            TW.Close();
            TR.Close();
        }
    }
}
