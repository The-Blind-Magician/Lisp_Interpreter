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

            return "";
        }

        public string print(string s)
        {
            foreach (string str in s.Split(" "))
            {
                if (str != "print") Console.Write(str + " ");
            }
            Console.Write("\n");
            return "";
        }

        public string get_var(string input)
        {
            foreach(KeyValuePair<string,string> k in variables)
            {
                if (k.Key == input)
                    return k.Value;
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
            foreach(string x in input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..])
            {
                total += Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string sub(string input)
        {
            var nums = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            double total = Convert.ToDouble(nums[0]);
            total -= Convert.ToDouble(nums[1]);
            return total.ToString();
        }

        public string div(string input)
        {
            string[] nums = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];

            double total = Convert.ToDouble(nums[0]);
            total /= Convert.ToDouble(nums[1]);
            return total.ToString();
        }

        public string mul(string input)
        {
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];

            double total = Convert.ToDouble(sep[0]);
            foreach (string x in sep[1..])
            {
                total *= Convert.ToDouble(x);
            }
            return total.ToString();
        }

        public string lt(string input)
        {
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            if (Convert.ToDouble(sep[0]) < Convert.ToDouble(sep[1])) return "T";
            return "()";
        }

        public string gt(string input)
        {
            string[] sep = input.Split(" ").Where(x => x != "" && x != " ").ToArray()[1..];
            if (Convert.ToDouble(sep[0]) > Convert.ToDouble(sep[1])) return "T";
            return "()";
        }

        public string number(string input)
        {
            try
            {
                Convert.ToDouble(input);
                return "T";
            }
            catch
            {
                return "()";
            }
        }

        public string nil(string input)
        {
            return (input.Contains("()") ? "T" : "()");
        }
    }
}
