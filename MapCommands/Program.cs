using System.Reflection;
using CommandLineParserLib;

namespace MapCommands
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: MapCommands <outputFile> <assembliesDirectory>");
                return;
            }

            var outputFile = args[0];
            var assembliesDirectory = args[1];

            var assemblyFiles = Directory.GetFiles(assembliesDirectory, "*.dll");
            var commands = new List<string>();

            foreach (var file in assemblyFiles)
            {
                var assembly = Assembly.LoadFrom(file);
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        var attribute = method.GetCustomAttribute<CommandAttribute>();
                        if (attribute != null)
                        {
                            var paramInfo = method.GetParameters();
                            var paramTypes = string.Join(",", paramInfo.Select(p => p.ParameterType.FullName));
                            var commandInfo = $"{attribute.Name.ToLower()}:{type.FullName}.{method.Name}({paramTypes})";
                            commands.Add(commandInfo);
                        }
                    }
                }
            }

            File.WriteAllLines(outputFile, commands);
        }
    }
}
