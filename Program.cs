using System;
using System.Collections.Generic;

namespace Lisp_Interpreter
{
    class Program
    {
        public static Lisp_Functions lisp = new Lisp_Functions();
        public static Lisp_Dictionary dictionary = new Lisp_Dictionary();
        public static Utilities util = new Utilities(@"C:\Users\chris\Documents\GitHub\Lisp_Interpreter\code.txt", dictionary, lisp);
       
        static void Main(string[] args)
        {
            string line = "";
            do
            {
                line = util.Read_Next_Whole_Expression().Trim();
                if (line == "") break;
                line = util.Prep_Input(line);
                util.Evaluate_Atom(new int[] { 1, line.Length - 1 }, line);

            } while (line != "");
        }

        
    }
}
