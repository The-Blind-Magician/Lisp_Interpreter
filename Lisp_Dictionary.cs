using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Lisp_Interpreter
{
    class Lisp_Dictionary
    {
        public Dictionary<string, Func<string, string>> dict = new Dictionary<string, Func<string, string>>();

        public Lisp_Dictionary()
        {
            Initialize_Dictionary();
        }
        public void Initialize_Dictionary()
        {
            dict.Add("+", Program.lisp.add);
            dict.Add("-", Program.lisp.sub);
            dict.Add("/", Program.lisp.div);
            dict.Add("*", Program.lisp.mul);
            dict.Add("print", Program.lisp.print);
            dict.Add("set", Program.lisp.set);
            dict.Add("<", Program.lisp.lt);
            dict.Add(">", Program.lisp.gt);
            dict.Add("number?", Program.lisp.number);
            dict.Add("null?", Program.lisp.nil);
            dict.Add("symbol?", Program.lisp.symbol);
            dict.Add("list?", Program.lisp.ls);
            dict.Add("if", Program.lisp.if_func);
            dict.Add("begin", Program.lisp.begin);
            dict.Add("=", Program.lisp.eq);
            dict.Add("while", Program.lisp.while_func);
            dict.Add("define", Program.lisp.define);
        }
    }
}
