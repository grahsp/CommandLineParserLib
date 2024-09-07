namespace CommandLineParserLib
{
    [AttributeUsage(System.AttributeTargets.Method, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; private set; }

        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
}
