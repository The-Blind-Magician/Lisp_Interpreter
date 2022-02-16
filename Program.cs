using System;
using System.Collections.Generic;

namespace Lisp_Interpreter
{
    class Program
    {

        static Utilities util = new Utilities(@"C:\Users\chris\Documents\GitHub\Lisp_Interpreter\code.txt");
        public static Lisp_Functions lisp = new Lisp_Functions();
        static Lisp_Dictionary dictionary = new Lisp_Dictionary();

        //static Lisp_Data_Structures base_atom = new Lisp_Data_Structures();
        static void Main(string[] args)
        {
            string line = "";
            do
            {
                line = util.Read_Next_Expression();

                Console.WriteLine(line);

            } while (line != "");
            
        }

        static string Evaluate_Atom(int[] x, string line)
        {
            string atom = line.Substring(x[0], x[1] - x[0] + 1).Trim();
            string atomArgs = line.Substring(x[0] + 1, x[1] - x[0] - 1).Trim();

            string answer = dictionary.dict[atomArgs.Split(" ")[0].ToLower()](atomArgs);
         
            return line.Replace(atom, " " + answer + " ");
        }

        static string Sub_All_Variable_Values(string line)
        {
            string[] proc_line = line.Split(" ");
            for (int i = 0; i < proc_line.Length; i++)
            {
                string var;
                if((var = lisp.get_var(proc_line[i])) != "")
                {
                    proc_line[i] = var;
                }
            }
           
            var str = string.Join(" ", proc_line);
            return str;
        }
        static int[] Get_Lowest_Bracket_Pair(string line)
        {
            int open = 0;
            int[] pairs = new int[2];
            int i = 0;
            foreach (char c in line)
            {
                switch (c)
                {
                    case '(':
                        open = i;
                        break;
                    case ')':
                        pairs = new int[] { open, i};
                        return pairs;
                }
                i++;
            }
            return pairs;
        }
    }
}
