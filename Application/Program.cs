using Application.Infrastructure;
using System.Reflection;

var arguments = Environment.GetCommandLineArgs();

string? sourcePath = null;
string? configurationPath = null;

if (arguments.Count() == 1)
{
    Console.WriteLine("Runnning test program...");
    configurationPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\example_configuration.txt");
    sourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestFiles\\example_source.txt");
}
else if (arguments.Count() == 2)
{
    sourcePath = arguments[1];

    if (!File.Exists(sourcePath))
    {
        Console.WriteLine($"Source file {sourcePath} does not exists");
        return;
    }
}
else if (arguments.Count() == 3)
{
    sourcePath = arguments[1];

    if (!File.Exists(sourcePath))
    {
        Console.WriteLine($"Source file {sourcePath} does not exists");
        return;
    }

    configurationPath = arguments[2];

    if (!File.Exists(configurationPath))
    {
        Console.WriteLine($"Configuration file {configurationPath} does not exists");
        return;
    }
}
else
{
    Console.WriteLine($"Invalid input arguments count: {arguments.Count()}");
    return;
}

var engine = new LanguageEngine(sourcePath, configurationPath);
engine.Execute();