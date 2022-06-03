using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

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

        /// <summary>
        /// Reads file and preprocesses input into an array of whole expressions
        /// </summary>
        /// <param name="filePath"></param>
        void Init_FileStream(string filePath)
        {
            string input = File.ReadAllText(filePath);
            Prep_Input(ref input);
            List<string> ls = input.Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
            List<string> result = new List<string>();
            int track = 0;
            string expr = "";
            foreach (string str in ls)
            {
                if (str == "(") track++;
                else if (str == ")") track--;
                expr += str + " ";
                if (track == 0 && expr != "")
                {
                    result.Add(expr);
                    expr = "";
                }
            }
            fileStream = result.ToArray();
        }

        /// <summary>
        /// Extract an atom, processes it, and returns the result
        /// </summary>
        /// <param name="x"></param>
        /// <param name="line"></param>
        /// <param name="func"></param>
        public void Evaluate_Atom(int[] x, ref string line, Defined_Function func)
        {
            if (x[0] == -1 && x[1] == -1) return; 
            string atom = line[x[0]..(x[1]+1)].Trim();
            string atomArgs = atom[(atom.IndexOf('(')+1)..atom.LastIndexOf(')')].Trim();
            string answer = "";
            
            //Try to find op, if it doesn't exist return un-nested list of values
            try
            {
                var op = atomArgs.Trim().Split(" ")[0].Trim();
                answer = dictionary.dict[op](atomArgs, func != null ? func : null);
            }
            catch(Exception)
            {
                if (atomArgs == " ") atomArgs = "()";
                line = line[0..(x[0])] + " " + atomArgs + " " + line[(x[1] + 1)..];
                line = line.Trim();
                return;
            }
            line = line[0..(x[0])] + " " + answer + " " + line[(x[1] + 1)..];
        }

        /// <summary>
        /// Iteratively evaluate all sub-expressions in a given input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        public void Evaluate_Nested_Functions(ref string input, Defined_Function func = null)
        {
            while (true)
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
        }

        /// <summary>
        /// Get next full expressino from input array
        /// </summary>
        /// <returns></returns>
        public string Read_Next_Whole_Expression()
        {
            if (lineNumber == fileStream.Length)
                return "";
            return fileStream[lineNumber++];
        }

        /// <summary>
        /// Finds and returns the index of the first same-level pair of
        /// parentheses
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int[] Read_First_Partial_Expression(string input)
        {
            int[] inx = { -1, -1 };
            int i = 0;
            int track = 0;
            string[] subArr = Program.util.Get_Substring_Array(input);
            foreach (string c in subArr) //Find individual parentheses and track their level until finding the close parentesis associated with level zero
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
                        return new int[] { temp, str.Length + temp - 1 };
                    }
                }
                i++;
            }
            inx = new int[] {-1,-1};
            return inx;
        }
        /// <summary>
        /// Calls Read_First_Partial_Expression() on a substring given a startIndex
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public int[] Read_Next_Partial_Expression(string str, int startIndex) 
        {
            string temp = str[startIndex..];
            int[] inx = Read_First_Partial_Expression(temp);
            inx[0] += startIndex;
            inx[1] += startIndex;
            return inx;
        }
        /// <summary>
        /// Finds and substitutes all defined variables in an input string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        public void Sub_All_Variable_Values(ref string input, Defined_Function func = null)
        {
            string[] args = Get_Substring_Array(input);
            for (int i = 0; i < args.Length; i++) //Foreach string in input
            {
                if (func != null) //If func != null, substitute local variables first
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
                else if (lisp.variables.ContainsKey(args[i])) //Else, sub all global variables
                {
                    args[i] = lisp.get_var(args[i]);
                }
            }
            input = string.Join(" ", args);
        }
        /// <summary>
        /// Spaces out all parentheses for processing later, skipping any one with a single quote prefix or suffix 
        /// </summary>
        /// <param name="input"></param>
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

        /// <summary>
        /// Extracts the key from a function, modifies the original string, and returns the key
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Extract_Key(ref string input)
        {
            string key = input.Trim().Split(" ")[0]; //Extract key from atom
            input = input[(input.IndexOf(key) + key.Length)..].Trim(); //Remove key from atom for arg(s) processing
            return key;
        }

        /// <summary>
        /// Separates select parts of a string into an array by whitespace,
        /// filtering the array given an (optional) array of substrings
        /// to be omitted
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inx"></param>
        /// <param name="filtChars"></param>
        /// <returns></returns>
        public string[] Get_Substring_Array(string input, int[] inx = null, string[] filtChars = null)
        {
            filtChars ??= new string[] { };
            inx ??= new int[] { 0, input.Length -1};
            string str = input[inx[0]..(inx[1] + 1)];
            return str.Split(" ").Where(x => !filtChars.Contains(x) && !String.IsNullOrWhiteSpace(x)).ToArray();
        }

        /// <summary>
        /// Join elements of a string array with spaces between
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Recompile_String(string[] input)
        {
            return String.Join(" ", input);
        }
    }
}
