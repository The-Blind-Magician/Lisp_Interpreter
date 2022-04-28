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
        private int lineNumber = 0;

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
        public void Evaluate_Atom(int[] x, ref string line, Defined_Function func)
        {
            if (x[0] == -1 && x[1] == -1) return; 
            string atom = line.Substring(x[0], x[1] - x[0]).Trim();
            string atomArgs = atom[(atom.IndexOf('(') + 1)..atom.LastIndexOf(')')].Trim();
            string answer = "";
            try
            {
                var op = atomArgs.Trim().Split(" ")[0];
                answer = dictionary.dict[op](atomArgs, func != null ? func : null);
            }
            catch(Exception e)
            {
                if (atomArgs == " ") atomArgs = "()";
                line = line[0..(x[0])] + " " + atomArgs + " " + line[(x[1] + 1)..];
                return;
            }
            line = line[0..(x[0])] + " " + answer + " " + line[(x[1] + 1)..];
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
        public int[] Read_First_Partial_Expression(string input)
        {
            Prep_Input(ref input);
            int[] inx = { -1, -1 };
            int i = 0;
            int track = 0;
            string[] subArr = Program.util.Get_Substring_Array(input);
            foreach (string c in subArr)
            {
                if (c == "(" && track++ == 0 && inx[0] == -1)
                {
                    inx[0] = i;
                }
                else if (c == ")" && --track == 0 && inx[1] == -1)
                {
                    inx[1] = i;
                }
                if (inx[1] != -1 && inx[0] != -1) 
                {
                    if (inx[1] + inx[0] < 2)
                    {
                        inx[0] = -1;
                        inx[1] = -1;
                        track = 0;
                    }
                    else
                    {
                        string str = String.Join(" ", subArr[inx[0]..(inx[1]+1)]);
                        int temp = input.IndexOf(str);
                        return new int[] { temp, str.Length + temp };
                    }
                }
                i++;
            }
            inx = new int[] {-1,-1};
            return inx;
        }
        public int[] Read_Next_Partial_Expression(string str, int startIndex) ///////SOMEHOW LOSING LAST BRACKET
        {
            string temp = str[startIndex..];
            int[] inx = Read_First_Partial_Expression(temp);
            inx[0] += startIndex != 0 ? startIndex + 1 : startIndex;
            inx[1] += startIndex != 0 ? startIndex + 1 : startIndex;
            return inx;
        }
        public void Sub_All_Variable_Values(ref string input, Defined_Function func = null)
        {
            string[] args = Get_Substring_Array(input);
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
            input = string.Join(" ", args);
        }
        public void Prep_Input(ref string input)
        {
            for(int i = 0; i < input.Length; i++)
            {
                if(input[i] == '(' || input[i] == ')')
                {
                    if((i > 0 && i < input.Length - 1) && (input[i-1] == '\'' || input[i+1] == '\''))
                    {
                        continue;
                    }
                    else if(i == 0)
                    {
                        input = input[i] + " " + input[(i + 1)..];
                    }
                    else
                    {
                        input = input[..(i)] + " " + input[i] + " " + input[(i + 1)..];
                        i++;
                    }
                }
            }
            input = Recompile_String(Get_Substring_Array(input));
        }
        public void Evaluate_Nested_Functions(ref string input, Defined_Function func = null)
        {
            while (input.Contains("("))
            {
                int[] x = Program.util.Read_First_Partial_Expression(input);
                if (x[0] == -1)
                {
                    input = input.Replace("( )", "()").Trim();
                    return;
                }
                Program.util.Evaluate_Atom(x, ref input, func);
                input = input.Trim();
            }
            input = input.Trim();
        }

        public string Extract_Key(ref string input)
        {
            string key = input.Trim().Split(" ")[0]; //Extract key from atom
            input = input[(input.IndexOf(key) + key.Length)..].Trim(); //Remove key from atom for arg(s) processing
            return key;
        }

        public string[] Get_Substring_Array(string input, int[] inx = null, string[] filtChars = null)
        {
            filtChars ??= new string[] {  };
            inx ??= new int[] { 0, input.Length };
            return input.Substring(inx[0], inx[1] - inx[0]).Split(" ").Where(x => !filtChars.Contains(x) && !String.IsNullOrWhiteSpace(x)).ToArray();
        }

        public string Recompile_String(string[] input)
        {
            return String.Join(" ", input);
        }
    }
}
