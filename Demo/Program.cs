using CommandLineParserLib;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        [Command("test")]
        public void Test(string name)
        {
            Console.WriteLine("Helsfgalo, " + name);
        }
        
        [Command("awesome")]
        public void Awesome(string name, int age, bool married)
        {
            Console.WriteLine("Hello, " + name);
        }
    }
}
