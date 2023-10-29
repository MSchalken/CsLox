namespace Schalken.CsLox;

internal static class Logger
{
    public static bool HasError;

    public static void Warn(int line, string message)
    {
        Console.WriteLine($"[line {line}] Warn: {message}");
    }
    public static void Error(int line, string message)
    {
        Console.WriteLine($"[line {line}] Error: {message}");
        HasError = true;
    }
}