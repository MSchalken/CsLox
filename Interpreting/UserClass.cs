using System.Diagnostics.CodeAnalysis;
using Schalken.CsLox.Interpreting;

namespace Schalken.CsLox;

internal class UserClass(string name, IReadOnlyDictionary<string, UserFunction> methods) : ICallable
{
    private string _name = name;
    private IReadOnlyDictionary<string, UserFunction> _methods = methods;

    public int Arity() => TryFindMethod("init", out var init) ? init.Arity() : 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new UserClassInstance(this);

        if (TryFindMethod("init", out var init))
        {
            init.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public bool TryFindMethod(string name, [MaybeNullWhen(false)] out UserFunction method) => _methods.TryGetValue(name, out method);

    public override string ToString() => _name;
}
