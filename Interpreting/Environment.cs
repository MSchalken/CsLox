using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;

namespace Schalken.CsLox;

internal class Environment(Environment? enclosingScope)
{
    private readonly Environment? _enclosingScope = enclosingScope;
    private readonly Dictionary<string, object?> _globalValues = [];

    public Environment() : this(null) { }

    public void Define(string name, object? value) => _globalValues[name] = value;

    public void Assign(Token name, object? value)
    {
        if (_globalValues.ContainsKey(name.Lexeme.Get().ToString()))
        {
            _globalValues[name.Lexeme.Get().ToString()] = value;
            return;
        }

        if (_enclosingScope is not null)
        {
            _enclosingScope.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme.Get().ToString()}'.");
    }

    public object? Get(Token token)
    {
        if (_globalValues.TryGetValue(token.Lexeme.Get().ToString(), out var value))
        {
            return value;
        }

        if (_enclosingScope is not null)
        {
            return _enclosingScope.Get(token);
        }

        throw new RuntimeError(token, $"Undefined variable '{token.Lexeme.Get().ToString()}'.");
    }
}
