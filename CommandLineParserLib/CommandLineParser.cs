using System.Reflection;

namespace CommandLineParserLib
{
    public class CommandLineParser
    {
        private string[] _args;
        public CommandLineParser(string[] args)
        {
            _args = args;
        }
    }
}
