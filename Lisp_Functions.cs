using System;
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
            key = Program.util.Extract_Key(ref input);
            int[] inx = Program.util.Read_Next_Partial_Expression(input, 0); //Find next partial expression
            Defined_Function newFunc = new Defined_Function(); //New object for creation of new function
            string[] filtChars = { "", " ", "(", ")"}; //Characters to filter out of string

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
                if (inx[1] - inx[0] < 0 || inx[0] == inx[1]) break;
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
            
            //Process all expressions sequentially, destructively modifying the input string
            while(input.Contains("(")) 
            {
                Program.util.Evaluate_Atom(inx, ref input , func);
                if (input != "")
                    inx = Program.util.Read_First_Partial_Expression(input);
            }
            return "";
        }

        /// <summary>
        /// Function for processing while statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string while_func(string input, Defined_Function func = null)
        {
            string[] filt = { "while" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt)); //Filter out whitespace and keywords
            int[] inx = Program.util.Read_First_Partial_Expression(input);

            string evalStr = input.Substring(inx[0], inx[1] - inx[0] + 1); //Extract the conditional

            string procStr = input.Substring(inx[1] + 1, input.Length - inx[1] - 1).Trim(); //Extract the statments
            List<string> subs = new List<string>();

            //Build list of statments to iterate through
            while(procStr != "")
            {
                inx = Program.util.Read_First_Partial_Expression(procStr);
                subs.Add(procStr.Substring(inx[0], inx[1] - inx[0] + 1));
                procStr = procStr.Remove(inx[0], inx[1] + 1).Trim();
            }

            //Evalutate expressions until condition becomes false
            string cond;
            do
            {
                cond = evalStr;
                Program.util.Evaluate_Atom(new int[] { 0, (cond.Length - 1) }, ref cond, func); //Evaluate condition
                string temp = "";
                foreach (string str in subs)
                {
                    temp = str;
                    Program.util.Evaluate_Atom(new int[] { 0, temp.Length - 1 }, ref temp, func); //Evaluate statments
                }
            } while (cond.Contains("T"));
            return "";
        }

        /// <summary>
        /// Function for processing print statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string print(string input, Defined_Function func = null)
        {
            string[] filt = { "print" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));

            Program.util.Evaluate_Nested_Functions (ref input, func); //Evaluate all nested functions before printing
            Program.util.Sub_All_Variable_Values(ref input, func);

            if (input == "" || input == "()") //Print () when necessary
                Console.WriteLine("()");
            else if (input == "\\n") //Print a newline
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

            //Extracts variable from either local or global, depending on whether the var is called within a function
            if (func == null) 
                return variables[input];
            else
                return func.vars[input];
        }

        /// <summary>
        /// Function for processing set statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string set(string input, Defined_Function func = null)
        {
            string[] filt = { "set" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            string[] arr = new string[2];

            Program.util.Evaluate_Nested_Functions(ref input, func);

            arr = Program.util.Get_Substring_Array(input);

            if (variables.ContainsKey(arr[1])) arr[1] = get_var(arr[1]); //If 2nd key is a variable
            if (variables.ContainsKey(arr[0])) variables.Remove(arr[0]); //If first key is already defined, remove it
            variables.Add(arr[0], arr[1]); //Define value for key
            return "";
        }

        /// <summary>
        /// Function for processing add statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string add(string input, Defined_Function func = null)
        {
            string[] filt = { "+" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            double total = 0;
            Program.util.Evaluate_Nested_Functions(ref input, func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);
            foreach (string x in args)
            {
                total += Convert.ToDouble(x);
            }
            return total.ToString();
        }

        /// <summary>
        /// Function for processing subtract statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string sub(string input, Defined_Function func)
        {
            string[] filt = { "-" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            Program.util.Evaluate_Nested_Functions(ref input , func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);

            double total = Convert.ToDouble(args[0]);
            total -= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        /// <summary>
        /// Function for processing division statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string div(string input, Defined_Function func)
        {
            string[] filt = { "/" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            Program.util.Evaluate_Nested_Functions(ref input, func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);

            double total = Convert.ToDouble(args[0]);
            total /= Convert.ToDouble(args[1]);

            return total.ToString();
        }

        /// <summary>
        /// Function for processing multiplication statments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string mul(string input, Defined_Function func)
        {
            string[] filt = { "*" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            Program.util.Evaluate_Nested_Functions(ref input, func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);
            double total = Convert.ToDouble(args[0]);

            foreach (string x in args[1..])
            {
                total *= Convert.ToDouble(x);
            }
            return total.ToString();
        }

        /// <summary>
        /// Test for less than
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string lt(string input, Defined_Function func)
        {
            string[] filt = { "<" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            Program.util.Evaluate_Nested_Functions(ref input , func); 
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);
            if (Convert.ToDouble(args[0]) < Convert.ToDouble(args[1])) return "T";
            return "()";
        }
        /// <summary>
        /// Tests for equality
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string eq(string input, Defined_Function func)
        {
            string[] filt = { "=" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            Program.util.Evaluate_Nested_Functions(ref input, func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);
            if (Convert.ToDouble(args[0]) == Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        /// <summary>
        /// Tests for greater than
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string gt(string input, Defined_Function func)
        {
            string[] filt = { ">" };
            input = Program.util.Recompile_String(Program.util.Get_Substring_Array(input, filtChars: filt));
            Program.util.Evaluate_Nested_Functions(ref input , func);
            Program.util.Sub_All_Variable_Values(ref input, func);
            string[] args = Program.util.Get_Substring_Array(input);
            if (Convert.ToDouble(args[0]) > Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        /// <summary>
        /// Tests if a given argument is a number
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string number(string input, Defined_Function func)
        {
            string[] filt = { "number?" };
            string[] temp = Program.util.Get_Substring_Array(input, filtChars: filt);

            try
            {
                Convert.ToDouble(temp[0]); //If the first arg can't be converted to a double, it is not a number
                return "T";
            }
            catch
            {
                return "()";
            }
        }

        /// <summary>
        /// Tests if a given argument is a number
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string symbol(string input, Defined_Function func)
        {
            string[] filt = { "symbol?" };
            string[] temp = Program.util.Get_Substring_Array(input, filtChars: filt);

            if (Program.lisp.variables.ContainsKey(temp[0])) //If the argument is an existing variable
                return "T";
            return "()";
        }

        /// <summary>
        /// Tests if a given argument is a list
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string ls(string input, Defined_Function func)
        {
            string[] filt = { "list?" };
            string[] temp = Program.util.Get_Substring_Array(input, filtChars: filt);
            if (temp.Length < 2) return "()";
            foreach(string str in temp)
            {
                if (Program.dictionary.dict.ContainsKey(str)) //As long as the atom doesn't contain an operator, return true
                    return "()";
            }
            return "T";
        }

        /// <summary>
        /// Tests if a given argument is null
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string nil(string input, Defined_Function func)
        {
            return (input.Contains("()") || input.Contains("( )") ? "T" : "()"); //Test for nil ()
        }

        /// <summary>
        /// Gets the car of an atom
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string car(string input, Defined_Function func)
        {
            string[] filt = { "car" };
            input = input.Replace("car", "").Trim();

            if (input[2] != '(') //If the 2nd char is not a parentheses, return the first argument 
            {
                return input.Split(" ")[1];
            }
            else //If it is a parentheses, find string with the matching brackets
            {
                string proc = input[1..(input.Length - 1)].Trim();
                int[] inx = Program.util.Read_First_Partial_Expression(proc);
                return proc[inx[0]..(inx[1] + 1)];
            }
        }

        /// <summary>
        /// Gets the cdr of an atom
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string cdr(string input, Defined_Function func)
        {
            string[] temp = Program.util.Get_Substring_Array(input);
            string res = String.Join(" ", temp);
            res = res.Replace("cdr", "car");
            string getCar = car(res, null); //Find car to remove from list
            res = res.Replace(getCar, "");
            return res[res.IndexOf('(')..];
        }

        /// <summary>
        /// Creates a cons from two parameters
        /// </summary>
        /// <param name="input"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public string cons(string input, Defined_Function func)
        {
            int x = input.Where(x => Program.dictionary.dict.Keys.ToArray().ToString().Contains(x)).Count();

            for (; x > 0; x--)//Evaluate nested functions to ensure no weird values
            {
                Program.util.Evaluate_Nested_Functions(ref input, func);
            }
            string[] filt = { "cons" };
            string[] temp = Program.util.Get_Substring_Array(input, filtChars: filt);

            string res = "( " + String.Join(" ", temp) + " )"; //Join args together with surrounding parentheses
            return res;
        }
    }
}
