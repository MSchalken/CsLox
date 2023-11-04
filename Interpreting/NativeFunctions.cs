namespace Schalken.CsLox.Interpreting;

internal class ClockFunction : ICallable
{
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments) =>
        (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public override string ToString() => "<native fn>";
}
