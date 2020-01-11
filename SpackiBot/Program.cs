using System;

namespace SpackiBot
{
    class Program
    {
        public static SpackiBot SpackiBot { get; private set; }

        static void Main(string[] args)
        {
            SpackiBot = new SpackiBot();
        }
    }
}
