using Schalken.CsLox.Interpreting;

namespace Schalken.CsLox;

internal interface ICallable
{
    int Arity();
    object? Call(Interpreter interpreter, List<object?> arguments);
}
