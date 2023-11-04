using Schalken.CsLox.Parsing;

namespace Schalken.CsLox.Interpreting;

internal class UserFunction(FuncDecl declaration, Environment closure) : ICallable
{
    private readonly FuncDecl _declaration = declaration;
    private readonly Environment _closure = closure;

    public int Arity() => _declaration.Parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(_closure);

        foreach (var (parameter, argument) in _declaration.Parameters.Zip(arguments))
        {
            environment.Define(parameter.Lexeme.Get().ToString(), argument);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Interpreter.ReturnValue returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString() => $"<fn {_declaration.Name.Lexeme.Get().ToString()}>";
}
