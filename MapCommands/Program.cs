using System.Reflection;
using System.Text.Json;
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
            var commands = new Dictionary<string, CommandInfo>();

            try
            {
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
                                commands.Add(attribute.Name, new CommandInfo() {
                                    Assembly = type.Assembly.GetName().Name ?? string.Empty,
                                    Namespace = type.Namespace ?? string.Empty,
                                    Class = type.Name,
                                    Method = method.Name
                                });
                            }
                        }
                    }
                }


                var json = JsonSerializer.Serialize(commands, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(outputFile, json);

                Console.WriteLine("Success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
                throw;
            }
        }
    }
}
