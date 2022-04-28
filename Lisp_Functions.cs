﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lisp_Interpreter
{
    class Defined_Function
    {
        /* Object to store statments, variables, and variable labels for each custom defined function */
        public List<string> statements = new List<string>();
        public Dictionary<string, string> vars = new Dictionary<string, string>();
        public List<string> varLabels = new List<string>();
    }
    class Lisp_Functions
    {
        /* Dictionary entries to store global variables and list of defined custom functions */
        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public Dictionary<string, Defined_Function> functions = new Dictionary<string, Defined_Function>();

        /// <summary>
        /// Extracts and defines new custom functions
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string define(string input, Defined_Function func = null)
        {
            string key = Program.util.Extract_Key(ref input); //Extract key from atom
            int[] inx = Program.util.Read_Next_Partial_Expression(input, 0); //Find next partial expression

            Defined_Function newFunc = new Defined_Function(); //New object for creation of new function
            string[] filtChars = { "", " ", "(", ")" }; //Characters to filter out of string

            //Extract variables and create dictionary entries
            foreach (string str in Program.util.Get_Substring_Array(input, inx, filtChars))
            {
                newFunc.vars.Add(str, "");
                newFunc.varLabels.Add(str);
            }

            //Add all remaining statements to statments list
            while (true)
            {
                inx = Program.util.Read_Next_Partial_Expression(input, inx[1]+1);
                if (inx[0] * inx[1] <= 1) break;
                newFunc.statements.Add(Program.util.Recompile_String(Program.util.Get_Substring_Array(input, inx)));
            }

            functions.Add(key, newFunc); //Add newFunc to list of custom functions
            Program.dictionary.dict.Add(key, Program.lisp.call); //Add key as item in dictionary of function keywords
            return "";
        }

        /// <summary>
        /// Handles all program-created custom functions
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string call(string input, Defined_Function func = null)
        {
            input = input.Trim(); //Clean up surrounding whitespace
            string key = Program.util.Extract_Key(ref input); //Extract key

            Program.util.Sub_All_Variable_Values(ref input);
            Program.util.Evaluate_Nested_Functions(ref input, func); //Evaluate all nested functions in arg(s)

            //If () exists, extract arg(s) from it
            input = input.Contains('(') ? input[(input.IndexOf('(') + 1)..(input.LastIndexOf(')'))].Trim() : input;

            Defined_Function temp = new Defined_Function();

            functions.TryGetValue(key, out temp); //Get the function stored in the functions dict

            string[] filtChars = { "", " ", "(", ")" };
            int i = 0;

            //Assign all arguments to corresponding local variables
            foreach (string str in Program.util.Get_Substring_Array(input, filtChars:filtChars))
            {
                temp.vars[temp.varLabels[i]] = str;
                i++;
            }

            string returnVal = ""; //String to return last evaluated value

            foreach(string str in temp.statements) //Iterate through all statments in function
            {
                returnVal = str;
                Program.util.Evaluate_Atom(new int[] { 0, str.Length - 1 }, ref returnVal, temp);
            }
            return returnVal;
        }

        /// <summary>
        /// Function for processing if statements
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string if_func(string input, Defined_Function func = null) //Function for processing if statments
        {
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input)); //Clean input
            int[] inx = Program.util.Read_First_Partial_Expression(input);

            Program.util.Evaluate_Atom(inx, ref input, func); //Evaluate conditional input of if statement

            if (Program.util.Get_Substring_Array(input)[1] == "T")
            {
                inx = Program.util.Read_First_Partial_Expression(input);
                input = input.Substring(inx[0], inx[1] - inx[0] + 1).Trim(); //Set whole input to T portion of if statment

                inx = new int[] { 0, input.Length - 1};

                Program.util.Evaluate_Atom(inx, ref input, func);
                return "";
            }
            else
            {
                inx = Program.util.Read_First_Partial_Expression(input);
                input = input.Replace(input.Substring(0, inx[1] + 1), "").Trim(); //Get rid of input up to () portion of if statment
                Program.util.Sub_All_Variable_Values(ref input, func);

                inx = Program.util.Read_First_Partial_Expression(input);
                input = input.Substring(inx[0], inx[1] - inx[0] + 1).Trim(); //Remove trailing ')' characters

                input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input)); //Filter unwanted characters
                Program.util.Evaluate_Atom(new int[] { 0, input.Length - 1 }, ref input, func);
                return "";
            }
        }

        /// <summary>
        /// Function for processing begin statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string begin(string input, Defined_Function func = null)
        {
            string[] filt = { "begin" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars:filt)); //Filter out whitespace and keywords
            int[] inx = Program.util.Read_First_Partial_Expression(input);
            while(input.Contains("("))
            {
                Program.util.Evaluate_Atom(inx, ref input , func);
                if (input != "")
                    inx = Program.util.Read_First_Partial_Expression(input);
            }
            return "";
        }

        public string while_func(string input, Defined_Function func = null)
        {
            string[] filt = { "while" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            int[] inx = Program.util.Read_First_Partial_Expression(input);
            string evalStr = input.Substring(inx[0], inx[1] - inx[0] + 1);
            string procStr = input.Substring(inx[1] + 1, input.Length - inx[1] - 1).Trim();
            List<string> subs = new List<string>();
            while(procStr != "")
            {
                inx = Program.util.Read_First_Partial_Expression(procStr);
                subs.Add(procStr.Substring(inx[0], inx[1] - inx[0] + 1));
                procStr = procStr.Remove(inx[0], inx[1] + 1).Trim();
            }
            
            do
            {
                Program.util.Evaluate_Atom(new int[] { 0, (evalStr.Length - 1) }, ref evalStr, func);
                string temp = "";
                foreach (string str in subs)
                {
                    Program.util.Evaluate_Atom(new int[] { 0, str.Length - 1 }, ref str, func);
                }
            } while (evalStr.Contains("T"));
            return "";
        }

        public string print(string input, Defined_Function func = null)
        {
            string[] filt = { "print" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = String.Join(" ", input.Split(" ").Where(x=> !String.IsNullOrWhiteSpace(x)).ToArray()[1..]);
            Program.util.Evaluate_Nested_Functions (ref input, func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            if (input == "" || input == "()")
                Console.WriteLine("()");
            else if (input == "\\n")
                Console.WriteLine("");
            else
                Console.WriteLine(input.Trim());
            return "";
        }

        /// <summary>
        /// Gets variable stored in a given scope
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns>value of variable</returns>
        public string get_var(string input, Defined_Function func = null)
        {
            string[] filt = { "get" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            if (func == null)
                return variables[input];
            else
                return func.vars[input];
        }

        public string set(string input, Defined_Function func = null)
        {
            string[] filt = { "set" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            string[] arr = new string[2];
            input = Program.util.Evaluate_Nested_Functions(input, func);
            arr = input.Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (variables.ContainsKey(arr[1])) arr[1] = get_var(arr[1]);
            if (variables.ContainsKey(arr[0])) variables.Remove(arr[0]);
            variables.Add(arr[0], arr[1]);
            return "";
        }

        public string add(string input, Defined_Function func = null)
        {
            string[] filt = { "+" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            double total = 0;
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            foreach (string x in args)
            {
                total += Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string sub(string input, Defined_Function func)
        {
            string[] filt = { "-" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            total -= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        public string div(string input, Defined_Function func)
        {
            string[] filt = { "/" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            total /= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        public string mul(string input, Defined_Function func)
        {
            string[] filt = { "*" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            foreach (string x in args[1..])
            {
                total *= Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string lt(string input, Defined_Function func)
        {
            string[] filt = { "<" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) < Convert.ToDouble(args[1])) return "T";
            return "()";
        }
        public string eq(string input, Defined_Function func)
        {
            string[] filt = { "=" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) == Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        public string gt(string input, Defined_Function func)
        {
            string[] filt = { ">" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) > Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        public string number(string input, Defined_Function func)
        {
            string[] arr = { "", " ", "(", ")", "number?" };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();

            try
            {
                Convert.ToDouble(temp[0]);
                return "T";
            }
            catch
            {
                return "()";
            }
        }

        public string symbol(string input, Defined_Function func)
        {
            string[] arr = { "", " ", "(", ")", "symbol?" };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();
            if (Program.lisp.variables.ContainsKey(temp[0]))
                return "T";
            return "()";
        }

        public string ls(string input, Defined_Function func)
        {
            string[] arr = { "", " ", "(", ")", "list?" };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();
            if (temp.Length < 2) return "()";
            foreach(string str in temp)
            {
                if (Program.dictionary.dict.ContainsKey(str))
                    return "()";
            }
            return "T";
        }

        public string nil(string input, Defined_Function func)
        {
            return (input.Contains("()") || input.Contains("( )") ? "T" : "()");
        }

        public string car(string input, Defined_Function func)
        {
            string[] arr = { "", " ", "car" };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();
            if (temp[1] != "(") return temp[1];
            int[] inx = { -1, -1 };
            int track = 0;
            for (int i = 1; i < temp.Length; i++)
            {
                if (temp[i] == "(")
                {
                    if (track == 0)
                        inx[0] = i;
                    track++;
                }
                else if (temp[i] == ")")
                {
                    track--;
                    if (track == 0)
                    {
                        inx[1] = i + 1;
                        break;
                    }
                }
            }
            return String.Join(" ", temp[inx[0]..inx[1]]);
        }

        public string cdr(string input, Defined_Function func)
        {
            string[] arr = { "", " " };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();
            string res = String.Join(" ", temp);
            res = res.Replace("cdr", "car");
            string getCar = car(res, null);
            res = res.Replace(getCar, "");
            return res[res.IndexOf('(')..];
        }

        public string cons(string input, Defined_Function func)
        {
            int x = input.Where(x => Program.dictionary.dict.Keys.ToArray().ToString().Contains(x)).Count();
            for (; x > 0; x--)
            {
                input = Program.util.Evaluate_Nested_Functions(input, func);
            }
            string[] arr = { "", " ", "cons" };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();
            string res = "( " + String.Join(" ", temp) + " )";
            return res;
        }
    }
}
