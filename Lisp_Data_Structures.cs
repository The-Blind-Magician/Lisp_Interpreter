using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisp_Interpreter
{
    class Lisp_Data_Structures
    {
        public class Atom
        {
            string op = "";
            List<string> exprs;

            public Atom()
            {
            }

            public Atom(string atm)
            {
                op = atm.Split("(")[0];
                int[] ndx = new int[2];
                int i = 0;
                foreach(char c in atm)
                {
                    if (c == '(') ndx[0] = i;
                    else if(c==')')ndx[]
                }
            }

            public void Add_Expr(string ex)
            {
                exprs.Add(ex);
            }
            public void Set_Op(string op)
            {
                this.op = op;
            }
        }
    }
}
