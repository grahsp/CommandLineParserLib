using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CommandLineParserLib
{
    public class CommandLineParser
    {
        private string[] _args;
        private Dictionary<string, CommandInfo> _commands;

        private MethodInfo method;
        private ParameterInfo[] parameters;

        public CommandLineParser(string[] args)
        {
            _args = args;
            _commands = LoadCommands("C:\\Projects\\C#\\CommandLineParserLib\\CommandLineParserLib\\Commands.json");

            method = GetMethod(_commands[args[0]]);
            parameters = method.GetParameters();
        }

        private Dictionary<string, CommandInfo> LoadCommands(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, CommandInfo>>(jsonString, new JsonSerializerOptions { WriteIndented = true }) ?? [];
        }

        private MethodInfo GetMethod(CommandInfo command)
        {
            var fullClassName = $"{command.Namespace}.{command.Class}";

            var assembly = Assembly.Load(command.Assembly);
            if (assembly == null) { throw new ArgumentNullException($"Could not load assembly '{command.Assembly}'"); }

            var type = assembly.GetType(fullClassName);
            if (type == null) { throw new ArgumentNullException($"Could not load class '{fullClassName}'"); }

            var method = type.GetMethod(command.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method == null) { throw new ArgumentNullException($"Could not load method '{command.Method}' of '{fullClassName}'"); }

            var parameters = method.GetParameters();

            return method;
        }
    }
}
