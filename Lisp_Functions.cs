using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisp_Interpreter
{
    class Lisp_Functions
    {
        private Dictionary<string, string> variables = new Dictionary<string, string>();
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

        public string print(string s)
        {
            while (s.Contains('('))
            {
                int[] inx = Program.util.Read_First_Partial_Expression(s);
                s = Program.util.Evaluate_Atom(inx, s);
            }
            var temp = s;
            Console.WriteLine(s);
            return "";
        }

        public string get_var(string input)
        {
            foreach(KeyValuePair<string,string> k in variables)
            {
                if (k.Key == input)
                    return $"({k.Value})";
            }
            return "";
        }

        public string set(string input)
        {
            string[] arr = input.Split(" ")[1..];
            variables.Add(arr[1], arr[2]);
            return "";
        }

        public string add(string input)
        {
            double total = 0;
            if(input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            foreach(string x in input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..])
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
            var nums = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            double total = Convert.ToDouble(nums[0]);
            total -= Convert.ToDouble(nums[1]);
            return $"({total.ToString()})";
        }

        public string div(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] nums = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];

            double total = Convert.ToDouble(nums[0]);
            total /= Convert.ToDouble(nums[1]);
            return $"({total.ToString()})";
        }

        public string mul(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];

            double total = Convert.ToDouble(sep[0]);
            foreach (string x in sep[1..])
            {
                total *= Convert.ToDouble(x);
            }
            return $"({total.ToString()})";
        }

        public string lt(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            if (Convert.ToDouble(sep[0]) < Convert.ToDouble(sep[1])) return "T";
            return "()";
        }
        public string eq(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            if (Convert.ToDouble(sep[0]) == Convert.ToDouble(sep[1])) return "T";
            return "()";
        }

        public string gt(string input)
        {
            if (input.Contains("(") || input.Contains(")"))
            {
                input = Program.util.Evaluate_Atom(Program.util.Read_First_Partial_Expression(input), input);
            }
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            if (Convert.ToDouble(sep[0]) > Convert.ToDouble(sep[1])) return "T";
            return "()";
        }

        public string number(string input)
        {
            try
            {
                Convert.ToDouble(input);
                return "(T)";
            }
            catch
            {
                return "()";
            }
        }

        public string nil(string input)
        {
            return (input.Contains("()") ? "(T)" : "()");
        }
    }
}
