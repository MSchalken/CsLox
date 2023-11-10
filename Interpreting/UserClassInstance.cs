using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;

namespace Schalken.CsLox;

internal class UserClassInstance(UserClass userClass)
{
    private readonly UserClass _userClass = userClass;
    private readonly Dictionary<string, object?> _fields = [];

    public object? Get(Token name)
    {
        var referredName = name.Lexeme.Get().ToString();

        if (_fields.TryGetValue(referredName, out var field))
        {
            return field;
        }

        if (_userClass.TryFindMethod(referredName, out var method))
        {
            return method.Bind(this);
        }

        throw new RuntimeError(name, $"Undefined property '{name.Lexeme.Get().ToString()}'.");
    }

    public object? Set(Token name, object? value)
    {
        _fields[name.Lexeme.Get().ToString()] = value;
        return value;
    }

    public override string ToString() => $"{_userClass} instance";
}