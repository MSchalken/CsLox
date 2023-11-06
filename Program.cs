using Schalken.CsLox;
using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;
using Schalken.CsLox.Parsing;

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
        if (Logger.HasError) return 66;
        if (Logger.HasRuntimeError) return 70;
        return 0;
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
    var parser = new Parser(tokens);
    var statements = parser.Parse().ToList();

    if (Logger.HasError) return;

    var interpreter = Interpreter.Instance;
    var resolver = new Resolver(interpreter);
    resolver.Resolve(statements);

    interpreter.Interpret(statements);
}