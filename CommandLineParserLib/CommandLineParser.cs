using System.Reflection;
using System.Text.Json;

namespace CommandLineParserLib
{
    public class CommandLineParser
    {
        public CommandLineParser(string[] args)
        {
            var commands = LoadCommands("C:\\Projects\\C#\\CommandLineParserLib\\CommandLineParserLib\\Commands.json");

            var method = GetMethod(commands[args[0]]);

            var arguments = ParseArgumentsOfMethod(method, args[1..]);
            foreach (var arg in arguments)
            {
                Console.WriteLine(arg);
            }
        }


        public object[] ParseArgumentsOfMethod(MethodInfo method, string[] args)
        {
            var parameters = method.GetParameters();

            var parameterIndicies = MapParameterIndicies(parameters);
            var (namedArgs, positionalArgs) = ParseArguments(args, parameters, parameterIndicies);

            return ConstructParameterArray(parameters, namedArgs, positionalArgs);
        }


        private object[] ConstructParameterArray(ParameterInfo[] parameters, Dictionary<string, object> namedArgs, List<object> positionalArgs)
        {
            var parsedParams = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramName = parameters[i].Name.ToLower() ?? string.Empty;
                if (namedArgs.ContainsKey(paramName))
                {
                    parsedParams[i] = namedArgs[paramName];
                }
                else if (positionalArgs.Count > 0)
                {
                    parsedParams[i] = positionalArgs[0];
                    positionalArgs.RemoveAt(0);
                }
                else if (parameters[i].HasDefaultValue)
                {
                    parsedParams[i] = parameters[i].DefaultValue!;
                }
                else
                {
                    throw new Exception($"Parameter '{paramName}' is missing");
                }
            }

            return parsedParams;
        }


        private (Dictionary<string, object>, List<object>) ParseArguments(string[] args, ParameterInfo[] parameters, Dictionary<string, int> parameterIndicies)
        {
            Dictionary<string, object> namedArgs = [];
            List<object> positionalArgs = [];

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (IsNamedArgument(arg))
                {
                    var paramName = GetArgumentName(arg);
                    if (!parameterIndicies.ContainsKey(paramName)) throw new ArgumentException($"Parameter '{paramName}' does not exist");

                    var param = parameters[parameterIndicies[paramName]];
                    namedArgs[paramName] = ParseArgumentWithValue(args, ref i, param);
                }
                else
                {
                    positionalArgs.Add(arg);
                }
            }


            return (namedArgs, positionalArgs);
        }

        private static object ParseArgumentWithValue(string[] args, ref int index, ParameterInfo parameter)
        {
            if (index + 1 < args.Length && !IsNamedArgument(args[index + 1]))
            {
                index++;
                return Convert.ChangeType(args[index], parameter.ParameterType);
            }

            throw new ArgumentException($"Value for parameter '{parameter.Name}' is missing");
        }

        private static bool IsNamedArgument(string argument)
        {
            return argument.StartsWith('-');
        }

        private static string GetArgumentName(string argument)
        {
            return argument.Trim('-');
        }

        private static Dictionary<string, int> MapParameterIndicies(ParameterInfo[] parameters)
        {
            Dictionary<string, int> parameterIndicies = [];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var name = parameter.Name ?? string.Empty;
                var index = i;

                parameterIndicies.Add(name.Trim().ToLower(), index);
            }

            return parameterIndicies;
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
