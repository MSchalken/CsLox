using Schalken.CsLox.Parsing;

namespace Schalken.CsLox.Interpreting;

internal class UserFunction(FuncDecl declaration, Environment closure, bool isInitializer = false)
    : ICallable
{
    private readonly FuncDecl _declaration = declaration;
    private readonly Environment _closure = closure;
    private readonly bool _isInitializer = isInitializer;

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
            return _isInitializer ? _closure.GetAt(0, "this") : returnValue.Value;
        }

        return _isInitializer ? _closure.GetAt(0, "this") : null;
    }

    public UserFunction Bind(UserClassInstance instance)
    {
        var environment = new Environment(_closure);
        environment.Define("this", instance);
        return new UserFunction(_declaration, environment, _isInitializer);
    }

    public override string ToString() => $"<fn {_declaration.Name.Lexeme.Get().ToString()}>";
}
