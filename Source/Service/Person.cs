using System;

namespace Service
{
    public class Person
    {
        public string Name { set; get; }

        public int Calculator(string op, int a, int b)
        {
            if (op == "+")
            {
                return a + b;
            }
            else if (op == "-")
            {
                return a - b;
            }

            return -1;
        }
        public string GetName(int a)
        {
            if (a > 0)
            {
                return "shahzad mughal";
            }
            else
            {
                return Name;
            }
        }
    }
}