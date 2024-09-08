using System.Reflection;
using System.Text.Json;

namespace CommandLineParserLib
{
    public class CommandLineParser
    {
        private static readonly JsonSerializerOptions _JsonSerializerOptions = new() { WriteIndented = true };

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


        public static object[] ParseArgumentsOfMethod(MethodInfo method, string[] args)
        {
            var parameters = method.GetParameters();

            var parameterIndicies = MapParameterIndicies(parameters);
            var (namedArgs, positionalArgs) = ParseArguments(args, parameters, parameterIndicies);

            return ConstructParameterArray(parameters, namedArgs, positionalArgs);
        }


        private static object[] ConstructParameterArray(ParameterInfo[] parameters, Dictionary<string, string> namedArgs, List<string> positionalArgs)
        {
            var parsedParams = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramName = parameters[i].Name!.ToLower() ?? string.Empty;
                if (namedArgs.TryGetValue(paramName, out var _))
                {
                    parsedParams[i] = Convert.ChangeType(namedArgs[paramName], parameters[i].ParameterType);
                }
                else if (positionalArgs.Count > 0)
                {
                    parsedParams[i] = Convert.ChangeType(positionalArgs[0], parameters[i].ParameterType);
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


        private static (Dictionary<string, string>, List<string>) ParseArguments(string[] args, ParameterInfo[] parameters, Dictionary<string, int> parameterIndicies)
        {
            Dictionary<string, string> namedArgs = [];
            List<string> positionalArgs = [];

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (IsNamedArgument(arg))
                {
                    var paramName = GetArgumentName(arg);
                    if (!parameterIndicies.TryGetValue(paramName, out var _)) throw new ArgumentException($"Parameter '{paramName}' does not exist");

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

        private static string ParseArgumentWithValue(string[] args, ref int index, ParameterInfo parameter)
        {
            if (index + 1 < args.Length && !IsNamedArgument(args[index + 1]) && parameter.ParameterType != typeof(bool))
            {
                index++;
                //return Convert.ChangeType(args[index], parameter.ParameterType);
                return args[index];
            }
            // Handle bools without a value
            else if (parameter.ParameterType == typeof(bool))
            {
                return "true";
            }

            throw new ArgumentException($"Value for parameter '{parameter.Name}' is missing");

        }

        private static bool IsNamedArgument(string argument)
        {
            return argument.StartsWith('-');
        }

        private static string GetArgumentName(string argument)
        {
            return argument.Trim('-').ToLower();
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




        private static Dictionary<string, CommandInfo> LoadCommands(string path)
        {
            var jsonString = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, CommandInfo>>(jsonString, _JsonSerializerOptions) ?? [];
        }


        private static MethodInfo GetMethod(CommandInfo command)
        {
            var fullClassName = $"{command.Namespace}.{command.Class}";

            var assembly = Assembly.Load(command.Assembly) ?? throw new ArgumentNullException($"Could not load assembly '{command.Assembly}'");
            var type = assembly.GetType(fullClassName) ?? throw new ArgumentNullException($"Could not load class '{fullClassName}'");
            var method = type.GetMethod(command.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) ?? throw new ArgumentNullException($"Could not load method '{command.Method}' of '{fullClassName}'");

            return method;
        }
    }
}
