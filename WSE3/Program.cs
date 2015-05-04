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
    class Program
    {
        public const string Directory_Data = "../../Data/";
        public const string Directory_Normalization = "../../Normalization/";
        public const string Directory_Output = "../../Output/";

        public static int Global_Doc_Amount = 75364;
        public static int Global_Inverted_Index_10_Amount = 7537;
        public static int Global_Inverted_Index_100_Amount = 754;
        public static int Global_Inverted_Index_1000_Amount = 76;
        public static int Global_Inverted_Index_10000_Amount = 8;

        static void Main(string[] args)
        {
            Console.WriteLine("Do you need to normalize all the data? Y/N");
            var s = Console.ReadLine();
            if (s == "Y")
            {
                Program_Index.Go();
            }
            Program_Query.Go();
        }
    }
}
