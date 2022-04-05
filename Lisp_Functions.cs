using System;
using System.Collections.Generic;
using System.Linq;

namespace Lisp_Interpreter
{
    class Defined_Function
    {
        public List<string> statements = new List<string>();
        public List<string> vars = new List<string>();
    }
    class Lisp_Functions
    {
        public Dictionary<string, string> variables = new Dictionary<string, string>();
        public Dictionary<string, Defined_Function> functions = new Dictionary<string, Defined_Function>();
        public string define(string s)
        {
            string key = s.Split(" ")[1];
            s = s.Substring(s.IndexOf(' '));
            int[] inx = Program.util.Read_Next_Partial_Expression(s, 0);

            Defined_Function temp = new Defined_Function();
            string[] arr = { "", " ", "(", ")" };
            foreach (string str in s.Substring(inx[0] + 1, inx[1]-inx[0]).Split(" ").Where(x => !arr.Contains(x)).ToArray())
            {
                if(!variables.ContainsKey(str))
                    variables.Add(str, "");
                temp.vars.Add(str);
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

        public string call(string s)
        {
            s = s.Trim();
            string key = s.Split(" ").First();
            s = s[(s.IndexOf('(')+1)..(s.LastIndexOf(')')-1)].Trim();

            Defined_Function temp = new Defined_Function();

            functions.TryGetValue(key, out temp);

            string[] arr = { "", " ", "(", ")" };
            int i = 0;
            foreach (string str in s.Split(" ").Where(x => !arr.Contains(x)))
            {
                variables[temp.vars[i]] = str;
                i++;
            }

            foreach(string str in temp.statements)
            {
                Program.util.Evaluate_Atom(new int[] { 0, str.Length - 1 }, str);
            }

            foreach (string str in temp.vars)
            {
                variables[str] = "";
            }

            return "";
        }

        public string if_func(string s)
        {
            int[] inx = Program.util.Read_First_Partial_Expression(s);
            s = Program.util.Evaluate_Atom(inx, s);
            if (s.Split(" ").Where(x => x != "" && x != " ").ToArray()[1] == "T")
            {
                inx = Program.util.Read_First_Partial_Expression(s);
                s = s.Substring(inx[0], inx[1] - inx[0] + 1).Trim();
                inx[0] = 0;
                inx[1] = s.Length - 1;
                s = Program.util.Evaluate_Atom(inx, s);
                return "";
            }
            else
            {
                inx = Program.util.Read_First_Partial_Expression(s);
                s = s.Replace(s.Substring(0, inx[1] + 1), "").Trim();
                inx = Program.util.Read_First_Partial_Expression(s);
                s = s.Replace(s.Substring(0, inx[1] + 1), "").Trim();
                inx = Program.util.Read_First_Partial_Expression(s);
                s = Program.util.Evaluate_Atom(inx, s);
                return "";
            }
            return "";
        }

        public string begin(string s)
        {
            s = s.Replace("begin", "").Trim();
            int[] inx = Program.util.Read_First_Partial_Expression(s);
            while(s.Contains("("))
            {
                s = Program.util.Evaluate_Atom(inx, s);
                if (s != "")
                    inx = Program.util.Read_First_Partial_Expression(s);
            }
            return "";
        }

        public string while_func(string s)
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
            while (Program.util.Evaluate_Atom(new int[] { 0, (evalStr.Length - 1)}, evalStr).Contains("T"))
            {
                foreach(string str in subs)
                {
                    Program.util.Evaluate_Atom(new int[] { 0, str.Length - 1 }, str);
                }
            }
            return "";
        }

        public string print(string s)
        {
            while (s.Contains('('))
            {
                int[] inx = Program.util.Read_First_Partial_Expression(s);
                s = Program.util.Evaluate_Atom(inx, s);
            }
            s = Program.util.Sub_All_Variable_Values(s).Trim();
            if (s == "")
                Console.WriteLine("()");
            else
                Console.WriteLine(s.Trim());
            return "";
        }

        public string get_var(string input)
        {
            foreach(KeyValuePair<string,string> k in variables)
            {
                if (k.Key == input)
                    return $"{k.Value}";
            }
            return "";
        }

        public string set(string input)
        {
            string[] arr = new string[2];
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            arr = input.Split(" ")[1..].Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (variables.ContainsKey(arr[1])) arr[1] = get_var(arr[1]);
            if (variables.ContainsKey(arr[0])) variables.Remove(arr[0]);
            variables.Add(arr[0], arr[1]);
            return "";
        }

        public string add(string input)
        {
            double total = 0;
            if(input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            foreach (string x in args)
            {
                total += Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string sub(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            total -= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        public string div(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            total /= Convert.ToDouble(args[1]);
            return total.ToString();
        }

        public string mul(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            double total = Convert.ToDouble(args[0]);
            foreach (string x in args[1..])
            {
                total *= Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string lt(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) < Convert.ToDouble(args[1])) return "T";
            return "()";
        }
        public string eq(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) == Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        public string gt(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] args = Program.util.Sub_All_Variable_Values(input).Split(" ").Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
            if (Convert.ToDouble(args[0]) > Convert.ToDouble(args[1])) return "T";
            return "()";
        }

        public string number(string input)
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

        public string symbol(string input)
        {
            string[] arr = { "", " ", "(", ")", "symbol?" };
            string[] temp = input.Split(" ").Where(x => !arr.Contains(x)).ToArray();
            if (Program.lisp.variables.ContainsKey(temp[0]))
                return "T";
            return "()";
        }

        public string ls(string input)
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

        public string nil(string input)
        {
            return (input.Contains("()") ? "T" : "()");
        }
    }
}
