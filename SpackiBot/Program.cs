namespace SpackiBot
{
    internal class Program
    {
        public static SpackiBot SpackiBot { get; private set; }

        private static void Main(string[] args)
        {
            SpackiBot = new SpackiBot();
        }
    }
}