using CommandLineParserLib;

namespace Demo
{
    public class Program
    {
        static void Main(string[] args)
        {
            var parser = new CommandLineParser(args);

        }

        [Command("test")]
        public static void Test()
        {
            Console.WriteLine("Helsfgalo, " + "Joe");
        }
        
        [Command("awesome")]
        public static void Awesome(string name, int age, bool married)
        {
            Console.WriteLine("Hello, " + name);
        }
    }
}
