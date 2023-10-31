if (args.Length != 2)
{
    Console.WriteLine("Usage: expression_generator <output directory> <definition file>");
    return 64;
}

var outputDirectory = args[0];
var astInput = args[1];

var astDefinitionContent = File.ReadAllLines(astInput);
var name = astDefinitionContent[0];
var astDefinition = astDefinitionContent[1..];

GenerateAst(outputDirectory, name, astDefinition);

static void GenerateAst(string outputDirectory, string name, string[] astDefinition)
{
    var astDef = ReadAstDefinition(astDefinition).ToArray();

    var outputPath = Path.Combine(outputDirectory, $"{name}.cs");

    using var writer = new StreamWriter(outputPath, false);

    WriteHeader(writer);
    WriteInterface(writer, name);
    writer.WriteLine();
    WriteRecords(writer, astDef, name);
    WriteVisitorWithRetValue(writer, astDef, name);
    WriteVisitorWithVoid(writer, astDef, name);

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

static void WriteInterface(StreamWriter writer, string name)
{
    writer.WriteLine($"internal interface I{name}");
    writer.WriteLine("{");
    writer.WriteLine($"\tT Accept<T>(I{name}Visitor<T> visitor);");
    writer.WriteLine($"\tvoid Accept(I{name}Visitor visitor);");
    writer.WriteLine("}");
}

static void WriteRecords(StreamWriter writer, IEnumerable<TypeDef> types, string name)
{
    foreach (var type in types)
    {
        WriteRecord(writer, type, name);
        writer.WriteLine();
    }
}

static void WriteRecord(StreamWriter writer, TypeDef type, string name)
{
    var fields = string.Join(", ", type.Fields.Select(f => $"{f.TypeName} {f.Name}"));
    writer.WriteLine($"internal sealed record {type.Name}({fields}) : I{name}");
    writer.WriteLine("{");
    writer.WriteLine($"\tpublic T Accept<T>(I{name}Visitor<T> visitor) => visitor.Visit(this);");
    writer.WriteLine($"\tpublic void Accept(I{name}Visitor visitor) => visitor.Visit(this);");
    writer.WriteLine("}");
}

static void WriteVisitorWithRetValue(StreamWriter writer, IEnumerable<TypeDef> types, string name)
{
    writer.WriteLine($"internal interface I{name}Visitor<T>");
    writer.WriteLine("{");

    foreach (var type in types)
    {
        writer.WriteLine($"\tT Visit({type.Name} {name.ToLowerInvariant()});");
    }

    writer.WriteLine("}");
}

static void WriteVisitorWithVoid(StreamWriter writer, IEnumerable<TypeDef> types, string name)
{
    writer.WriteLine($"internal interface I{name}Visitor");
    writer.WriteLine("{");

    foreach (var type in types)
    {
        writer.WriteLine($"\tvoid Visit({type.Name} {name.ToLowerInvariant()});");
    }

    writer.WriteLine("}");
}

return 0;

record TypeDef(string Name, FieldDef[] Fields);
record FieldDef(string TypeName, string Name);