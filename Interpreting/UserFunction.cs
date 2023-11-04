using Schalken.CsLox.Parsing;

namespace Schalken.CsLox.Interpreting;

internal class UserFunction(FuncDecl declaration) : ICallable
{
    private readonly FuncDecl _declaration = declaration;

    public int Arity() => _declaration.Parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(interpreter.Globals);

        foreach (var (parameter, argument) in _declaration.Parameters.Zip(arguments))
        {
            environment.Define(parameter.Lexeme.Get().ToString(), argument);
        }

        interpreter.ExecuteBlock(_declaration.Body, environment);
        return null;
    }

    public override string ToString() => $"<fn {_declaration.Name.Lexeme.Get().ToString()}>";
}
