using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;

namespace Schalken.CsLox;

internal class Environment(Environment? enclosingScope)
{
    private readonly Environment? _enclosingScope = enclosingScope;
    private readonly Dictionary<string, object?> _values = [];

    public Environment() : this(null) { }

    public void Define(string name, object? value) => _values[name] = value;

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme.Get().ToString()))
        {
            _values[name.Lexeme.Get().ToString()] = value;
            return;
        }

        if (_enclosingScope is not null)
        {
            _enclosingScope.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme.Get().ToString()}'.");
    }

    public void AssignAt(int depth, Token name, object? value) =>
        Ancestor(depth)._values[name.Lexeme.Get().ToString()] = value;

    public object? Get(Token token)
    {
        if (_values.TryGetValue(token.Lexeme.Get().ToString(), out var value))
        {
            return value;
        }

        if (_enclosingScope is not null)
        {
            return _enclosingScope.Get(token);
        }

        throw new RuntimeError(token, $"Undefined variable '{token.Lexeme.Get().ToString()}'.");
    }

    public object? GetAt(int depth, string name) => Ancestor(depth)._values[name];

    private Environment Ancestor(int depth)
    {
        var environment = this;

        for (int i = 0; i < depth; i++)
        {
            environment = environment!._enclosingScope;
        }

        return environment!;
    }
}
