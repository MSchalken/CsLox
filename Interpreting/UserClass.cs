using Schalken.CsLox.Interpreting;

namespace Schalken.CsLox;

internal class UserClass(string name) : ICallable
{
    private string _name = name;

    public int Arity() => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments) => new UserClassInstance(this);

    public override string ToString() => _name;
}
