using System;
using System.Collections.Generic;
using System.IO;

namespace Lisp_Interpreter
{
    class Program
    {
        public static Lisp_Functions lisp = new Lisp_Functions();
        public static Lisp_Dictionary dictionary = new Lisp_Dictionary();
        public static Utilities util;
        public static string homeDir = Directory.GetCurrentDirectory() + "\\";

        static void Main(string[] args)
        {
            int barLength = 40;
            foreach (string f in Directory.GetFiles(homeDir + "testing", "*.lisp"))
            {
                Console.WriteLine(f[(f.LastIndexOf('\\')+1)..]);
                Console.WriteLine(new string('=', barLength));
                util = new Utilities(f, dictionary, lisp);
                string line = "";
                do
                {
                    line = util.Read_Next_Whole_Expression().Trim();
                    if (line == "") break;
                    util.Prep_Input(ref line);
                    util.Evaluate_Atom(new int[] { 0, line.Length - 1 }, ref line, null);
                } while (line != "");
                Console.WriteLine(new string('=', barLength));
                Console.WriteLine();
            }
            Console.WriteLine("Press any key to close the window...");
            Console.ReadKey();
        }

        
    }
}
