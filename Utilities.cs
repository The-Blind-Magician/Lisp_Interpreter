using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lisp_Interpreter
{
    class Utilities
    {
        Lisp_Dictionary dictionary;
        Lisp_Functions lisp;
        private string[] fileStream;
        private int numberOfLines = 0;
        private int lineNumber = 0;
        private int wordNumber = 0;

        public Utilities(string filePath, Lisp_Dictionary dict, Lisp_Functions lisp)
        {
            Init_FileStream(filePath);
            dictionary = dict;
            this.lisp = lisp;
        }

        void Init_FileStream(string filePath)
        {
            fileStream = File.ReadAllLines(filePath).Where(x=> !String.IsNullOrWhiteSpace(x)).ToArray();
        }
        public string Evaluate_Atom(int[] x, string line, Defined_Function func)
        {
            string atom = line.Substring(x[0], x[1] - x[0]+1).Trim();
            string atomArgs = line.Substring(x[0] + 1, x[1] - x[0] - 1).Trim();
            string answer = "";
            try
            {
                answer = dictionary.dict[atomArgs.Trim().Split(" ")[0].ToLower()](atomArgs, func != null ? func : null);
            }
            catch(Exception e)
            {
                return line[0..(x[0])] + " " + atomArgs + " " + line[(x[1] + 1)..];
            }
            string temp = line[0..(x[0])] + " " + answer + " " + line[(x[1]+1)..];
            return temp;
        }
        public string Read_Next_Whole_Expression()
        {
            int track = 0;
            string expr = "";
            for (; lineNumber < fileStream.Length; lineNumber++)
            {
                foreach (char c in fileStream[lineNumber])
                {
                    if (c == '(') track++;
                    else if (c == ')') track--;
                }
                expr += fileStream[lineNumber];
                if (track == 0) { lineNumber++; return expr; }
            }
            return expr;
        }
        public int[] Read_First_Partial_Expression(string str)
        {
            int[] inx = { -1, -1 };
            int i = 0;
            int track = 0;
            foreach (char c in str)
            {
                if (c == '(' && track++ == 0 && inx[0] == -1)
                {
                    inx[0] = i;
                }
                else if (c == ')' && --track == 0 && inx[1] == -1)
                {
                    inx[1] = i;
                }
                if (inx[1] != -1 && inx[0] != -1) { return inx; }
                i++;
            }
            return inx;
        }
        public int[] Read_Next_Partial_Expression(string str, int startIndex)
        {
            int[] inx = { -1, -1 };
            int i = startIndex;
            int track = 0;
            foreach (char c in str[startIndex..])
            {
                if (c == '(' && track++ == 0 && inx[0] == -1)
                {
                    inx[0] = i;
                }
                else if (c == ')' && --track == 0 && inx[1] == -1)
                {
                    inx[1] = i;
                }
                if (inx[1] != -1 && inx[0] != -1 && inx[1] > inx[0]) { return inx; }
                i++;
            }
            return inx;
        }
        public string Sub_All_Variable_Values(string input, Defined_Function func = null)
        {
            string[] args = input.Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray()[1..];
            for (int i = 0; i < args.Length; i++)
            {
                if (func != null)
                {
                    if (func.vars.ContainsKey(args[i]))
                    {
                        args[i] = lisp.get_var(args[i], func);
                    }
                    else
                    {
                        if (lisp.variables.ContainsKey(args[i])) 
                            args[i] = lisp.get_var(args[i]);
                    }
                }
                else if (lisp.variables.ContainsKey(args[i]))
                {
                    args[i] = lisp.get_var(args[i]);
                }
            }
            var str = string.Join(" ", args);
            return str;
        }
        public string Prep_Input(string str)
        {
            str = str.Replace("(", " ( ");
            str = str.Replace(")", " ) ");
            str = String.Join(' ', str.Split(" ", StringSplitOptions.RemoveEmptyEntries));

            return str;
        }
        public string Evaluate_Nested_Functions(string input, Defined_Function func = null)
        {
            while (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input, func);
            }
            return input;
        }
    }
}
