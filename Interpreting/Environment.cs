using Schalken.CsLox.Interpreting;
using Schalken.CsLox.Lexing;

namespace Schalken.CsLox;

internal class Environment
{
    private readonly Dictionary<string, object?> _globalValues = [];

    public void Define(string name, object? value) => _globalValues[name] = value;

    public void Assign(Token name, object? value)
    {
        if (_globalValues.ContainsKey(name.Lexeme.Get().ToString()))
        {
            _globalValues[name.Lexeme.Get().ToString()] = value;
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme.Get().ToString()}'.");
    }

    public object? Get(Token token) =>
        _globalValues.TryGetValue(token.Lexeme.Get().ToString(), out var value)
            ? value
            : throw new RuntimeError(token, $"Undefined variable '{token.Lexeme.Get().ToString()}'.");
}
