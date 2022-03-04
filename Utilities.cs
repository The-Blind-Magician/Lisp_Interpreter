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
            fileStream = File.ReadAllLines(filePath);
        }
        public string Read_Next_Line_Of_File()
        {
            try
            {
                wordNumber = 0;
                return fileStream[lineNumber++];
            }
            catch(IndexOutOfRangeException e)
            {
                lineNumber = 0;
                return "";
            }
        }
        public string Evaluate_Atom(int[] x, string line)
        {
            string atom = line.Substring(x[0], x[1] - x[0]+1).Trim();
            string atomArgs = line.Substring(x[0] + 1, x[1] - x[0] - 1).Trim();
            string answer = "";
            try
            {
                answer = dictionary.dict[atomArgs.Trim().Split(" ")[0].ToLower()](atomArgs);
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

        public int[] Read_Next_Partial_Expression(string str, int[] prev)
        {
            str = str.Substring(prev[1] + 1);
            int[] inx = { -1, -1 };
            int i = 0;
            int track = 0;
            foreach (char c in str)
            {
                if (c == '(' && track++ == 0 && inx[0] == -1)
                {
                    inx[0] = i + prev[1];
                }
                else if (c == ')' && --track == 0 && inx[1] == -1)
                {
                    inx[1] = i + prev[1];
                }
                if (inx[1] != -1 && inx[0] != -1) { return inx; }
                i++;
            }
            return inx;
        }
        public string Sub_All_Variable_Values(string input)
        {
            string[] args = input.Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray()[1..];
            for (int i = 0; i < args.Length; i++)
            {
                if (lisp.variables.ContainsKey(args[i])) args[i] = lisp.get_var(args[i]);
            }
            var str = string.Join(" ", args);
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
                        pairs = new int[] { open, i };
                        return pairs;
                }
                i++;
            }
            return pairs;
        }
        public string Read_Next_Word_From_Line(string line)
        {
            try
            { 
                return line.Split(" ")[wordNumber++];
            }
            catch (IndexOutOfRangeException e)
            {
                wordNumber = 0;
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine("Index out of range. Returning first element.\nwordNumber set to: " + wordNumber);
                return line.Split(" ")[wordNumber++];
            }
        }
        public string Get_Line_At_Index(int i)
        {
            try
            {
                return fileStream[i];
            }
            catch (IndexOutOfRangeException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine("Index out of range. Returning first element.");
                return fileStream[0];
            }
        }
        public int Get_Number_Of_Lines_In_File()
        {
            return numberOfLines;
        }
        public int Get_Current_Line_Number()
        {
            return lineNumber;
        }
        public string Prep_Input(string str)
        {
            str = str.Replace("(", " ( ");
            str = str.Replace(")", " ) ");
            str = String.Join(' ', str.Split(" ", StringSplitOptions.RemoveEmptyEntries));

            return str;
        }
    }
}
