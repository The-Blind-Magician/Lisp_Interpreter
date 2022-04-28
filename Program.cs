using System;
using System.Collections.Generic;

namespace Lisp_Interpreter
{
    class Program
    {
        public static Lisp_Functions lisp = new Lisp_Functions();
        public static Lisp_Dictionary dictionary = new Lisp_Dictionary();
        public static Utilities util;
       
        static void Main(string[] args)
        {

            util = new Utilities(@"C:\Users\chris\Documents\GitHub\Lisp_Interpreter\code.txt", dictionary, lisp);
            string line = "";
            do
            {
                line = util.Read_Next_Whole_Expression().Trim();
                if (line == "") break;
                line = util.Prep_Input(line);
                util.Evaluate_Atom(new int[] { 0, line.Length - 1 }, line, null);
            } while (line != "");
        }

        
    }
}
