using Schalken.CsLox.Lexing;

namespace Schalken.CsLox;

internal static class Logger
{
    public static bool HasError;

    public static void Warn(int line, string message) => Output(line, message, error: false);
    public static void Error(int line, string message) => Output(line, message);

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Output(token.Lexeme.Line, message, " at end");
        }
        else
        {
            Output(token.Lexeme.Line, message, $" at '{token.Lexeme.Get().ToString()}'");
        }
    }

    private static void Output(int line, string message, string where = "", bool error = true)
    {
        var logLevel = error ? "Error" : "Warn";
        Console.WriteLine($"[line {line}] {logLevel}{where}: {message}");
        HasError |= error;
    }

}