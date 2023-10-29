using Schalken.CsLox;
using Schalken.CsLox.Lexing;

if (args.Length > 1)
{
    Console.WriteLine("Usage: cslox [script]");
    return 64;
}

if (args.Length == 1)
{
    return RunFile(args[0]);
}

return RunRepl();

int RunFile(string file)
{
    try
    {
        var content = File.ReadAllText(file);
        Run(content);
        return Logger.HasError ? 66 : 0;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to execute file: {file}");
        Console.WriteLine(e.Message);
        return 65;
    }
}

int RunRepl()
{
    const string prompt = "> ";
    while (true)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();
        if (string.IsNullOrEmpty(line)) break;
        Run(line);
        Logger.HasError = false;
    }

    return 0;
}

void Run(string content)
{
    var lexer = new Lexer(content);
    var tokens = lexer.ScanTokens();

    foreach (var token in tokens)
    {
        Console.WriteLine(token);
    }
}