using System;
using System.Collections.Generic;
using System.Linq;

namespace Lisp_Interpreter
{
    class Defined_Function
    {
        public List<string> statements = new List<string>();
        public Dictionary<string, string> vars = new Dictionary<string, string>();
        public List<string> varLabels = new List<string>();
    }
    class Lisp_Functions
    {
        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public Dictionary<string, Defined_Function> functions = new Dictionary<string, Defined_Function>();
        public string define(string s, Defined_Function func = null)
        {
            string key = s.Split(" ")[1];
            s = s.Substring(s.IndexOf(' '));
            int[] inx = Program.util.Read_Next_Partial_Expression(s, 0);

            Defined_Function temp = new Defined_Function();
            string[] arr = { "", " ", "(", ")" };
            foreach (string str in s.Substring(inx[0] + 1, inx[1]-inx[0]).Split(" ").Where(x => !arr.Contains(x)).ToArray())
            {
                temp.vars.Add(str, "");
                temp.varLabels.Add(str);
            }

            while (true)
            {
                inx = Program.util.Read_Next_Partial_Expression(s, inx[1]+1);
                if (inx[0] * inx[1] <= 1) break;
                temp.statements.Add(s.Substring(inx[0], inx[1] - inx[0] + 1));
            }
            functions.Add(key, temp);
            Program.dictionary.dict.Add(key, Program.lisp.call);
            return "";
        }

        public string call(string input, Defined_Function func = null)
        {
            input = input.Trim();
            string key = input.Split(" ").First();
            input = Program.util.Sub_All_Variable_Values(input);
            input = input[(input.IndexOf('(')+1)..(input.LastIndexOf(')')-1)].Trim();
            Defined_Function temp = new Defined_Function();

            functions.TryGetValue(key, out temp);

            string[] arr = { "", " ", "(", ")" };
            int i = 0;
            foreach (string str in input.Split(" ").Where(x => !arr.Contains(x)))
            {
                temp.vars[temp.varLabels[i]] = str;
                i++;
            }

            foreach(string str in temp.statements)
            {
                Program.util.Evaluate_Atom(new int[] { 0, str.Length - 1 }, str, temp);
            }

            foreach (string str in temp.varLabels)
            {
                temp.vars[str] = "";
            }

            return "";
        }

        public string if_func(string s, Defined_Function func = null)
        {
            int[] inx = Program.util.Read_First_Partial_Expression(s);
            s = Program.util.Evaluate_Atom(inx, s, func);
            if (s.Split(" ").Where(x => x != "" && x != " ").ToArray()[1] == "T")
            {
                inx = Program.util.Read_First_Partial_Expression(s);
                s = s.Substring(inx[0], inx[1] - inx[0] + 1).Trim();
                inx[0] = 0;
                inx[1] = s.Length - 1;
                s = Program.util.Evaluate_Atom(inx, s, func);
                return "";
            }
            else
            {
                inx = Program.util.Read_First_Partial_Expression(s);
                s = s.Replace(s.Substring(0, inx[1] + 1), "").Trim();
                inx = Program.util.Read_First_Partial_Expression(s);
                s = s.Replace(s.Substring(0, inx[1] + 1), "").Trim();
                inx = Program.util.Read_First_Partial_Expression(s);
                s = Program.util.Evaluate_Atom(inx, s, func);
                return "";
            }
            return "";
        }

        public string begin(string s, Defined_Function func = null)
        {
            s = s.Replace("begin", "").Trim();
            int[] inx = Program.util.Read_First_Partial_Expression(s);
            while(s.Contains("("))
            {
                s = Program.util.Evaluate_Atom(inx, s, func);
                if (s != "")
                    inx = Program.util.Read_First_Partial_Expression(s);
            }
            return "";
        }

        public string while_func(string s, Defined_Function func = null)
        {
            int[] inx = Program.util.Read_First_Partial_Expression(s);
            string evalStr = s.Substring(inx[0], inx[1] - inx[0] + 1);
            string procStr = s.Substring(inx[1] + 1, s.Length - inx[1] - 1).Trim();
            List<string> subs = new List<string>();
            while(procStr != "")
            {
                inx = Program.util.Read_First_Partial_Expression(procStr);
                string temp = procStr.Substring(inx[0], inx[1] - inx[0] + 1);
                subs.Add(procStr.Substring(inx[0], inx[1] - inx[0] + 1));
                procStr = procStr.Remove(inx[0], inx[1] + 1).Trim();
            }
            while (Program.util.Evaluate_Atom(new int[] { 0, (evalStr.Length - 1)}, evalStr, func).Contains("T"))
            {
                foreach(string str in subs)
                {
                    Program.util.Evaluate_Atom(new int[] { 0, str.Length - 1 }, str, func);
                }
            }
            return "";
        }

        public string print(string input, Defined_Function func = null)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            input = Program.util.Sub_All_Variable_Values(input, func).Trim();
            if (input == "" || input == "()")
                Console.WriteLine("()");
            else if (input == "\\n")
                Console.WriteLine("");
            else
                Console.WriteLine(input.Trim());
            return "";
        }

        public string get_var(string input, Defined_Function func = null)
        {
            if (func == null)
                return variables[input];
            else
                return func.vars[input];
        }

        public string set(string input, Defined_Function func = null)
        {
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
            double total = 0;
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            foreach (string x in args)
            {
                total += Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string sub(string input, Defined_Function func)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            total -= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        public string div(string input, Defined_Function func)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            total /= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        public string mul(string input, Defined_Function func)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            foreach (string x in args[1..])
            {
                total *= Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string lt(string input, Defined_Function func)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) < Convert.ToDouble(args[1])) return "T";
            return "()";
        }
        public string eq(string input, Defined_Function func)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) == Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        public string gt(string input, Defined_Function func)
        {
            input = Program.util.Evaluate_Nested_Functions(input, func);
            string[] args = Program.util.Sub_All_Variable_Values(input, func).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
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
            foreach(string str in temp)
            {
                if (Program.dictionary.dict.ContainsKey(str))
                    return "()";
            }
            return "T";
        }

        public string nil(string input, Defined_Function func)
        {
            return (input.Contains("()") ? "T" : "()");
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
