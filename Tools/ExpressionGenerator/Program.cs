if (args.Length != 2)
{
    Console.WriteLine("Usage: expression_generator <output directory> <definition file>");
    return 64;
}

var outputDirectory = args[0];
var astInput = args[1];

var astDefinition = File.ReadAllLines(astInput);

GenerateAst(outputDirectory, "Expression.cs", astDefinition);

static void GenerateAst(string outputDirectory, string fileName, string[] astDefinition)
{
    var astDef = ReadAstDefinition(astDefinition).ToArray();

    var outputPath = Path.Combine(outputDirectory, fileName);

    using var writer = new StreamWriter(outputPath, false);

    WriteHeader(writer);
    WriteInterface(writer);
    writer.WriteLine();
    WriteRecords(writer, astDef);
    WriteVisitor(writer, astDef);

    writer.Flush();
}

static IEnumerable<TypeDef> ReadAstDefinition(string[] astDefinition)
{
    foreach (var definition in astDefinition)
    {
        var split = definition.Split(':', 2, StringSplitOptions.TrimEntries);
        var typeName = split[0];
        var fields = split[1];
        var fieldDefs = fields.Split(',', StringSplitOptions.TrimEntries)
            .Select(f =>
            {
                var fieldSplit = f.Split(' ', 2, StringSplitOptions.TrimEntries);
                var typeName = fieldSplit[0];
                var fieldName = fieldSplit[1];
                return new FieldDef(typeName, fieldName);
            }).ToArray();
        yield return new TypeDef(typeName, fieldDefs);
    }
}


static void WriteHeader(StreamWriter writer)
{
    writer.WriteLine("using Schalken.CsLox.Lexing;");
    writer.WriteLine();
    writer.WriteLine("namespace Schalken.CsLox.Parsing;");
    writer.WriteLine();
}

static void WriteInterface(StreamWriter writer)
{
    writer.WriteLine("internal interface IExpr");
    writer.WriteLine("{");
    writer.WriteLine("\tT Accept<T>(IExpressionVisitor<T> visitor);");
    writer.WriteLine("}");
}

static void WriteRecords(StreamWriter writer, IEnumerable<TypeDef> types)
{
    foreach (var type in types)
    {
        WriteRecord(writer, type);
        writer.WriteLine();
    }
}

static void WriteRecord(StreamWriter writer, TypeDef type)
{
    var fields = string.Join(", ", type.Fields.Select(f => $"{f.TypeName} {f.Name}"));
    writer.WriteLine($"internal sealed record {type.Name}({fields}) : IExpr");
    writer.WriteLine("{");
    writer.WriteLine($"\tpublic T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);");
    writer.WriteLine("}");
}

static void WriteVisitor(StreamWriter writer, IEnumerable<TypeDef> types)
{
    writer.WriteLine("internal interface IExpressionVisitor<T>");
    writer.WriteLine("{");

    foreach (var type in types)
    {
        writer.WriteLine($"\tT Visit({type.Name} expression);");
    }

    writer.WriteLine("}");
}

return 0;

record TypeDef(string Name, FieldDef[] Fields);
record FieldDef(string TypeName, string Name);