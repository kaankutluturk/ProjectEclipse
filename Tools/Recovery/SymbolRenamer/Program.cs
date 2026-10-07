// Symbol-resolved renaming for the remaining obfuscated identifiers.
//
// The Beebyte-style obfuscator used by the shipped game produced eleven-letter
// identifiers drawn from A-P (for example PFENLAPGKFM). The same spelling is
// frequently reused for unrelated symbols, so text replacement is unsafe. This
// tool builds Roslyn compilations of the four project assemblies, identifies
// every source-declared symbol with such a name, and renames by resolved symbol.
//
// Commands:
//   diag                         compile and print diagnostics summary
//   extract <out-dir>            write symbols.jsonl, unresolved.tsv, json-types.tsv
//   apply <map.tsv>... [--dry-run] [--drop-conflicts] [--no-file-renames]
//
// Map rows: <symbol id>\t<new name>[\t...ignored columns]. Lines starting with
// '#' and the header row "id\t..." are ignored.
//
// apply verifies that, after renaming, every identifier token in the project
// binds to the same symbol it bound to before and that no new compiler errors
// appear. Conflicting rows are reported, and with --drop-conflicts removed
// until the verification passes.

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

static class Program
{
    public static readonly Regex Obfuscated = new("^_?[A-Pa-p][A-P]{10}$", RegexOptions.Compiled);

    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("usage: diag | extract <out-dir> | apply <map.tsv>... [--dry-run] [--drop-conflicts] [--no-file-renames]");
            return 2;
        }

        var root = FindRepoRoot();
        var project = ProjectModel.Load(root, null);
        switch (args[0])
        {
            case "diag":
                project.PrintDiagnostics(verbose: args.Contains("-v"));
                return 0;
            case "extract":
                Extractor.Run(project, args[1]);
                return 0;
            case "apply":
                return Applier.Run(project, args.Skip(1).ToArray());
            case "external":
                return External.Run(project, args[1], args[2], args.Skip(3).ToArray());
            default:
                Console.Error.WriteLine("unknown command " + args[0]);
                return 2;
        }
    }

    static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null && !File.Exists(Path.Combine(dir, "AGENTS.md")))
        {
            dir = Path.GetDirectoryName(dir);
        }
        return dir ?? throw new InvalidOperationException("run from inside the repository");
    }
}

sealed class Unit
{
    public string Name;
    public bool Ours;
    public string[] Defines;
    public List<string> References = new();
    public List<string> UnitRefs = new();
    public List<string> Files = new();
    public CSharpCompilation Compilation;
}

sealed class ProjectModel
{
    public string Root;
    public List<Unit> Units = new();
    public Dictionary<string, SyntaxTree> TreesByPath = new();
    public Dictionary<SyntaxTree, Unit> UnitOfTree = new();
    readonly ConcurrentDictionary<SyntaxTree, Dictionary<(int, int, int), int>> ordinals = new();

    public IEnumerable<SyntaxTree> OurTrees => Units.Where(u => u.Ours).SelectMany(u => u.Compilation.SyntaxTrees);

    public string Rel(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');

    public static ProjectModel Load(string root, IReadOnlyDictionary<string, SourceText> overrideTexts)
    {
        var model = new ProjectModel { Root = root };
        var inputSystemDir = Environment.GetEnvironmentVariable("INPUTSYSTEM_SRC");
        if (string.IsNullOrEmpty(inputSystemDir) || !Directory.Exists(inputSystemDir))
        {
            throw new InvalidOperationException("set INPUTSYSTEM_SRC to the com.unity.inputsystem package InputSystem/ directory");
        }

        string[] DefinesOf(string csproj)
        {
            var text = File.ReadAllText(Path.Combine(root, csproj));
            var m = Regex.Match(text, "<DefineConstants>([^<]*)</DefineConstants>");
            return m.Groups[1].Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        List<string> HintsOf(string csproj)
        {
            var text = File.ReadAllText(Path.Combine(root, csproj));
            return Regex.Matches(text, "<HintPath>([^<]*)</HintPath>")
                .Select(m => m.Groups[1].Value)
                .Select(p => Path.IsPathRooted(p) ? p : Path.Combine(root, p))
                .Where(File.Exists)
                .Where(p => !p.Contains("Library/ScriptAssemblies/Assembly-CSharp", StringComparison.Ordinal)
                    && !p.EndsWith("Eclipse.Runtime.dll", StringComparison.Ordinal))
                .ToList();
        }

        var input = new Unit { Name = "Unity.InputSystem", Ours = false, Defines = DefinesOf("Assembly-CSharp.csproj").Append("UNITY_INPUT_SYSTEM_ENABLE_UI").ToArray(), References = HintsOf("Assembly-CSharp-Editor.csproj") };
        input.Files.AddRange(Directory.EnumerateFiles(inputSystemDir, "*.cs", SearchOption.AllDirectories));
        var runtime = new Unit { Name = "Eclipse.Runtime", Ours = true, Defines = DefinesOf("Eclipse.Runtime.csproj"), References = HintsOf("Eclipse.Runtime.csproj"), UnitRefs = { "Unity.InputSystem" } };
        var firstpass = new Unit { Name = "Assembly-CSharp-firstpass", Ours = true, Defines = DefinesOf("Assembly-CSharp-firstpass.csproj"), References = HintsOf("Assembly-CSharp-firstpass.csproj"), UnitRefs = { "Unity.InputSystem", "Eclipse.Runtime" } };
        var main = new Unit { Name = "Assembly-CSharp", Ours = true, Defines = DefinesOf("Assembly-CSharp.csproj"), References = HintsOf("Assembly-CSharp.csproj"), UnitRefs = { "Unity.InputSystem", "Eclipse.Runtime", "Assembly-CSharp-firstpass" } };
        var editor = new Unit { Name = "Assembly-CSharp-Editor", Ours = true, Defines = DefinesOf("Assembly-CSharp-Editor.csproj"), References = HintsOf("Assembly-CSharp-Editor.csproj"), UnitRefs = { "Unity.InputSystem", "Eclipse.Runtime", "Assembly-CSharp-firstpass", "Assembly-CSharp" } };

        var assets = Path.Combine(root, "Assets");
        foreach (var file in Directory.EnumerateFiles(assets, "*.cs", SearchOption.AllDirectories))
        {
            var rel = model.Rel(file);
            if (rel.Split('/').Any(part => part.EndsWith('~') || part.StartsWith('.') || part == "node_modules"))
            {
                continue;
            }
            if (rel.Contains("/Editor/", StringComparison.Ordinal))
            {
                editor.Files.Add(file);
            }
            else if (rel.StartsWith("Assets/Scripts/Eclipse/Runtime/", StringComparison.Ordinal))
            {
                runtime.Files.Add(file);
            }
            else if (rel.StartsWith("Assets/Plugins/", StringComparison.Ordinal) || rel.StartsWith("Assets/Standard Assets/", StringComparison.Ordinal))
            {
                firstpass.Files.Add(file);
            }
            else
            {
                main.Files.Add(file);
            }
        }

        model.Units.AddRange(new[] { input, runtime, firstpass, main, editor });
        var byName = model.Units.ToDictionary(u => u.Name);
        var metadataCache = new Dictionary<string, MetadataReference>();
        foreach (var unit in model.Units)
        {
            var parse = new CSharpParseOptions(LanguageVersion.CSharp9, DocumentationMode.Parse, SourceCodeKind.Regular, unit.Defines);
            var trees = unit.Files.AsParallel().AsOrdered().Select(file =>
            {
                SourceText text = null;
                overrideTexts?.TryGetValue(file, out text);
                text ??= SourceText.From(File.ReadAllText(file), Encoding.UTF8);
                return CSharpSyntaxTree.ParseText(text, parse, file);
            }).ToList();
            var refs = new List<MetadataReference>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in unit.References)
            {
                if (!seen.Add(Path.GetFileName(path)))
                {
                    continue;
                }
                if (!metadataCache.TryGetValue(path, out var r))
                {
                    r = MetadataReference.CreateFromFile(path);
                    metadataCache[path] = r;
                }
                refs.Add(r);
            }
            foreach (var dep in unit.UnitRefs)
            {
                refs.Add(byName[dep].Compilation.ToMetadataReference());
            }
            unit.Compilation = CSharpCompilation.Create(unit.Name, trees, refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, concurrentBuild: true));
            foreach (var tree in trees)
            {
                model.UnitOfTree[tree] = unit;
                if (unit.Ours)
                {
                    model.TreesByPath[tree.FilePath] = tree;
                }
            }
        }
        return model;
    }

    public void PrintDiagnostics(bool verbose)
    {
        foreach (var unit in Units)
        {
            var errors = unit.Compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            Console.WriteLine($"{unit.Name}: {unit.Files.Count} files, {errors.Count} errors");
            foreach (var d in errors.Take(verbose ? 200 : 8))
            {
                Console.WriteLine("  " + d);
            }
        }
    }

    public SemanticModel Model(SyntaxTree tree) => UnitOfTree[tree].Compilation.GetSemanticModel(tree);

    int Ordinal(SyntaxNode node)
    {
        var map = ordinals.GetOrAdd(node.SyntaxTree, tree =>
        {
            var dict = new Dictionary<(int, int, int), int>();
            var i = 0;
            foreach (var n in tree.GetRoot().DescendantNodesAndSelf(descendIntoTrivia: true))
            {
                dict.TryAdd((n.SpanStart, n.Span.Length, n.RawKind), i++);
            }
            return dict;
        });
        return map.TryGetValue((node.SpanStart, node.Span.Length, node.RawKind), out var ord) ? ord : -1;
    }

    // Maps a bound symbol to the declaration that owns its name.
    public static ISymbol Normalize(ISymbol symbol)
    {
        switch (symbol)
        {
            case null:
                return null;
            case IAliasSymbol alias:
                return Normalize(alias.Target);
            case IMethodSymbol method:
                if (method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor or MethodKind.Destructor)
                {
                    return Normalize(method.ContainingType);
                }
                if (method.ReducedFrom != null)
                {
                    method = method.ReducedFrom;
                }
                method = method.OriginalDefinition;
                if (method.PartialDefinitionPart != null)
                {
                    method = method.PartialDefinitionPart;
                }
                while (method.OverriddenMethod != null)
                {
                    method = method.OverriddenMethod.OriginalDefinition;
                }
                if (method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove)
                {
                    return method;
                }
                return method;
            case IPropertySymbol property:
                property = property.OriginalDefinition;
                while (property.OverriddenProperty != null)
                {
                    property = property.OverriddenProperty.OriginalDefinition;
                }
                return property;
            case IEventSymbol ev:
                ev = ev.OriginalDefinition;
                while (ev.OverriddenEvent != null)
                {
                    ev = ev.OverriddenEvent.OriginalDefinition;
                }
                return ev;
            case IParameterSymbol parameter:
                if (parameter.ContainingSymbol is IMethodSymbol owner && owner.ReducedFrom != null)
                {
                    return owner.ReducedFrom.OriginalDefinition.Parameters[parameter.Ordinal + 1];
                }
                if (parameter.ContainingSymbol is IMethodSymbol m2 && !SymbolEqualityComparer.Default.Equals(m2, m2.OriginalDefinition))
                {
                    return m2.OriginalDefinition.Parameters[parameter.Ordinal];
                }
                if (parameter.ContainingSymbol is IMethodSymbol m3 && m3.PartialDefinitionPart != null)
                {
                    return m3.PartialDefinitionPart.Parameters[parameter.Ordinal];
                }
                return parameter.OriginalDefinition;
            case INamedTypeSymbol type:
                if (type.IsTupleType && type.TupleUnderlyingType != null)
                {
                    return type.OriginalDefinition;
                }
                return type.OriginalDefinition;
            case IFieldSymbol field:
                if (field.CorrespondingTupleField != null && !SymbolEqualityComparer.Default.Equals(field.CorrespondingTupleField, field))
                {
                    return field.CorrespondingTupleField;
                }
                return field.OriginalDefinition;
            default:
                return symbol.OriginalDefinition ?? symbol;
        }
    }

    // Stable identity that survives renames: file + syntax-node ordinal for
    // source declarations, documentation id for metadata symbols.
    public string Id(ISymbol symbol)
    {
        symbol = Normalize(symbol);
        if (symbol == null)
        {
            return null;
        }
        var decl = symbol.DeclaringSyntaxReferences
            .Where(r => TreesByPath.ContainsKey(r.SyntaxTree.FilePath) || UnitOfTree.ContainsKey(r.SyntaxTree))
            .OrderBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal)
            .ThenBy(r => r.Span.Start)
            .FirstOrDefault();
        if (decl != null)
        {
            var node = decl.GetSyntax();
            return $"{Rel(decl.SyntaxTree.FilePath)}#{Ordinal(node)}";
        }
        if (symbol is IParameterSymbol implicitParameter && implicitParameter.ContainingSymbol != null && IsOursSource(implicitParameter.ContainingSymbol))
        {
            // Implicit 'value' of setters and event accessors has no syntax of its own.
            return Id(implicitParameter.ContainingSymbol) + "#p" + implicitParameter.Ordinal;
        }
        return "M:" + (symbol.GetDocumentationCommentId() ?? symbol.ToDisplayString()) + (symbol is IParameterSymbol p ? "#" + p.Ordinal : "");
    }

    public bool IsOursSource(ISymbol symbol)
    {
        symbol = Normalize(symbol);
        return symbol != null && symbol.DeclaringSyntaxReferences.Any(r => TreesByPath.ContainsKey(r.SyntaxTree.FilePath));
    }

    // Resolves the symbol named by an identifier token, declaration or reference.
    public static ISymbol Resolve(SemanticModel model, SyntaxToken token)
    {
        var parent = token.Parent;
        if (parent == null)
        {
            return null;
        }
        switch (parent)
        {
            case BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or MethodDeclarationSyntax or PropertyDeclarationSyntax
                or EventDeclarationSyntax or EnumMemberDeclarationSyntax or VariableDeclaratorSyntax or ParameterSyntax
                or TypeParameterSyntax or SingleVariableDesignationSyntax or ForEachStatementSyntax or CatchDeclarationSyntax
                or LocalFunctionStatementSyntax or LabeledStatementSyntax or QueryClauseSyntax or QueryContinuationSyntax
                or JoinIntoClauseSyntax or ConstructorDeclarationSyntax or DestructorDeclarationSyntax:
                if (parent is ForEachStatementSyntax fe && fe.Identifier != token) break;
                if (parent is LabeledStatementSyntax ls && ls.Identifier != token) break;
                return model.GetDeclaredSymbol(parent);
            case ExternAliasDirectiveSyntax:
                return null;
        }
        if (parent is IdentifierNameSyntax or GenericNameSyntax)
        {
            var target = parent;
            var info = model.GetSymbolInfo(target);
            if (info.Symbol != null)
            {
                return info.Symbol;
            }
            if (info.CandidateSymbols.Length > 0)
            {
                var normalized = info.CandidateSymbols.Select(Normalize).Distinct(SymbolEqualityComparer.Default).ToList();
                if (normalized.Count == 1)
                {
                    return normalized[0];
                }
                var named = info.CandidateSymbols.Where(c => c.Name == token.ValueText).Select(Normalize).Distinct(SymbolEqualityComparer.Default).ToList();
                return named.Count == 1 ? named[0] : null;
            }
            return parent is IdentifierNameSyntax idName ? model.GetAliasInfo(idName) : null;
        }
        if (parent is NameEqualsSyntax ne && ne.Parent is AttributeArgumentSyntax aa)
        {
            return model.GetSymbolInfo(ne.Name).Symbol;
        }
        if (parent is GotoStatementSyntax)
        {
            return null;
        }
        return null;
    }

    public static IEnumerable<SyntaxToken> IdentifierTokens(SyntaxNode root)
        => root.DescendantTokens(descendIntoTrivia: true).Where(t => t.IsKind(SyntaxKind.IdentifierToken));
}

// --- Union of names that must change together (interface implementations) ---
sealed class Groups
{
    readonly Dictionary<string, string> parent = new();

    public string Find(string x)
    {
        if (!parent.TryGetValue(x, out var p))
        {
            return x;
        }
        if (p == x)
        {
            return x;
        }
        var r = Find(p);
        parent[x] = r;
        return r;
    }

    public void Union(string a, string b)
    {
        var ra = Find(a);
        var rb = Find(b);
        if (ra == rb)
        {
            return;
        }
        if (string.CompareOrdinal(ra, rb) < 0)
        {
            parent[rb] = ra;
            parent.TryAdd(ra, ra);
        }
        else
        {
            parent[ra] = rb;
            parent.TryAdd(rb, rb);
        }
    }

    public IEnumerable<string> Keys => parent.Keys;

    public static Groups Build(ProjectModel project)
    {
        var groups = new Groups();
        foreach (var tree in project.OurTrees)
        {
            var model = project.Model(tree);
            foreach (var typeDecl in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol type)
                {
                    continue;
                }
                foreach (var iface in type.AllInterfaces)
                {
                    foreach (var member in iface.GetMembers())
                    {
                        if (member is IMethodSymbol { MethodKind: not MethodKind.Ordinary } )
                        {
                            continue;
                        }
                        var impl = type.FindImplementationForInterfaceMember(member);
                        if (impl == null || impl.Name != member.Name)
                        {
                            continue;
                        }
                        if (!Program.Obfuscated.IsMatch(member.Name))
                        {
                            continue;
                        }
                        groups.Union(project.Id(impl), project.Id(member));
                    }
                }
            }
        }
        return groups;
    }
}

static class Extractor
{
    sealed class Record
    {
        public string id { get; set; }
        public string name { get; set; }
        public string kind { get; set; }
        public string unit { get; set; }
        public string file { get; set; }
        public int line { get; set; }
        public string container { get; set; }
        public string signature { get; set; }
        public string type { get; set; }
        public string access { get; set; }
        public bool isStatic { get; set; }
        public List<string> flags { get; set; } = new();
        public List<string> group { get; set; }
        public int refs { get; set; }
        public List<string> usages { get; set; } = new();
    }

    public static void Run(ProjectModel project, string outDir)
    {
        Directory.CreateDirectory(outDir);
        var groups = Groups.Build(project);
        var records = new ConcurrentDictionary<string, Record>();
        var unresolved = new ConcurrentBag<string>();
        var usageLines = new ConcurrentDictionary<string, ConcurrentBag<(string file, int line, string text)>>();
        var jsonTypes = new ConcurrentBag<string>();

        Parallel.ForEach(project.OurTrees, tree =>
        {
            var model = project.Model(tree);
            var text = tree.GetText();
            var rel = project.Rel(tree.FilePath);
            foreach (var token in ProjectModel.IdentifierTokens(tree.GetRoot()))
            {
                if (!Program.Obfuscated.IsMatch(token.ValueText))
                {
                    continue;
                }
                var line = text.Lines.GetLineFromPosition(token.SpanStart);
                var lineNo = line.LineNumber + 1;
                var symbol = ProjectModel.Normalize(ProjectModel.Resolve(model, token));
                if (symbol == null || !project.IsOursSource(symbol))
                {
                    unresolved.Add($"{rel}\t{lineNo}\t{token.ValueText}\t{(symbol == null ? "unbound" : "external:" + symbol.ToDisplayString())}\t{line.ToString().Trim()}");
                    continue;
                }
                if (symbol.Name != token.ValueText)
                {
                    continue;
                }
                var id = groups.Find(project.Id(symbol));
                var record = records.GetOrAdd(id, _ => Describe(project, symbol, id));
                lock (record)
                {
                    record.refs++;
                }
                usageLines.GetOrAdd(id, _ => new()).Add((rel, lineNo, line.ToString().Trim()));
            }

            foreach (var inv in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var name = inv.Expression switch
                {
                    MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
                    IdentifierNameSyntax id => id.Identifier.ValueText,
                    GenericNameSyntax g => g.Identifier.ValueText,
                    _ => null,
                };
                if (name is not ("SerializeObject" or "DeserializeObject" or "ToObject" or "FromObject" or "PopulateObject" or "ToJson"))
                {
                    continue;
                }
                if (model.GetSymbolInfo(inv).Symbol is not IMethodSymbol method)
                {
                    continue;
                }
                foreach (var t in method.TypeArguments)
                {
                    jsonTypes.Add($"{rel}\t{t.ToDisplayString()}");
                }
                foreach (var arg in inv.ArgumentList.Arguments.Take(1))
                {
                    var t = model.GetTypeInfo(arg.Expression).Type;
                    if (t != null && name is "SerializeObject" or "ToJson" or "FromObject")
                    {
                        jsonTypes.Add($"{rel}\t{t.ToDisplayString()}");
                    }
                }
            }
        });

        foreach (var (id, record) in records)
        {
            var lines = usageLines[id].OrderBy(u => u.file == record.file ? 1 : 0).ThenBy(u => u.file).ThenBy(u => u.line)
                .Where(u => !(u.file == record.file && u.line == record.line))
                .Select(u => $"{u.file}:{u.line}: {Trim(u.text, 160)}")
                .Distinct().Take(8).ToList();
            record.usages = lines;
            record.group = groups.Keys.Where(k => groups.Find(k) == id && k != id).ToList();
        }

        var options = new JsonSerializerOptions { WriteIndented = false };
        using (var w = new StreamWriter(Path.Combine(outDir, "symbols.jsonl")))
        {
            foreach (var record in records.Values.OrderBy(r => r.file, StringComparer.Ordinal).ThenBy(r => r.line))
            {
                w.WriteLine(JsonSerializer.Serialize(record, options));
            }
        }
        File.WriteAllLines(Path.Combine(outDir, "unresolved.tsv"), unresolved.OrderBy(x => x, StringComparer.Ordinal));
        File.WriteAllLines(Path.Combine(outDir, "json-types.tsv"), jsonTypes.Distinct().OrderBy(x => x, StringComparer.Ordinal));
        Console.WriteLine($"symbols: {records.Count}; unresolved tokens: {unresolved.Count}");
        foreach (var g in records.Values.GroupBy(r => r.kind).OrderByDescending(g => g.Count()))
        {
            Console.WriteLine($"  {g.Key}: {g.Count()}");
        }
    }

    static string Trim(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "…";

    static Record Describe(ProjectModel project, ISymbol symbol, string id)
    {
        var decl = symbol.DeclaringSyntaxReferences.OrderBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal).ThenBy(r => r.Span.Start).First();
        var lineSpan = decl.SyntaxTree.GetLineSpan(decl.Span);
        var record = new Record
        {
            id = id,
            name = symbol.Name,
            kind = KindOf(symbol),
            unit = project.UnitOfTree[decl.SyntaxTree].Name,
            file = project.Rel(decl.SyntaxTree.FilePath),
            line = lineSpan.StartLinePosition.Line + 1,
            container = symbol.ContainingSymbol?.ToDisplayString(),
            signature = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
            access = symbol.DeclaredAccessibility.ToString(),
            isStatic = symbol.IsStatic,
            type = symbol switch
            {
                IFieldSymbol f => f.Type.ToDisplayString(),
                IPropertySymbol p => p.Type.ToDisplayString(),
                ILocalSymbol l => l.Type.ToDisplayString(),
                IParameterSymbol p => p.Type.ToDisplayString(),
                IMethodSymbol m => m.ReturnType.ToDisplayString(),
                IEventSymbol e => e.Type.ToDisplayString(),
                _ => null,
            },
        };
        if (symbol is IFieldSymbol or IPropertySymbol && !symbol.IsStatic && symbol.ContainingType is { } owner)
        {
            var isConst = symbol is IFieldSymbol { IsConst: true };
            var attrs = symbol.GetAttributes().Select(a => a.AttributeClass?.Name ?? "").ToList();
            if (!isConst && symbol is IFieldSymbol fld && !fld.IsReadOnly && InheritsUnityObject(owner)
                && (fld.DeclaredAccessibility == Accessibility.Public || attrs.Contains("SerializeField")) && !attrs.Contains("NonSerializedAttribute"))
            {
                record.flags.Add("unity-serialized");
            }
            if (owner.GetAttributes().Any(a => a.AttributeClass?.Name == "SerializableAttribute") && symbol is IFieldSymbol && !attrs.Contains("NonSerializedAttribute"))
            {
                record.flags.Add("serializable-type");
            }
            if (attrs.Any(a => a.StartsWith("Json", StringComparison.Ordinal)) || owner.GetAttributes().Any(a => a.AttributeClass?.Name.StartsWith("Json", StringComparison.Ordinal) == true))
            {
                record.flags.Add("json-attributed");
            }
            if (attrs.Any(a => a.StartsWith("Proto", StringComparison.Ordinal)))
            {
                record.flags.Add("protobuf");
            }
        }
        if (symbol is IMethodSymbol { DeclaredAccessibility: not Accessibility.Public } method && method.ContainingType is { } mt && InheritsUnityObject(mt) && method.Parameters.Length <= 1)
        {
            record.flags.Add("possible-unity-message");
        }
        return record;
    }

    static bool InheritsUnityObject(INamedTypeSymbol type)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.ToDisplayString() == "UnityEngine.Object")
            {
                return true;
            }
        }
        return false;
    }

    public static string KindOf(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol t => "type:" + t.TypeKind.ToString().ToLowerInvariant(),
        IMethodSymbol m => m.MethodKind == MethodKind.LocalFunction ? "localfunction" : "method",
        IPropertySymbol => "property",
        IEventSymbol => "event",
        IFieldSymbol f => f.ContainingType?.TypeKind == TypeKind.Enum ? "enummember" : f.IsConst ? "const" : "field",
        IParameterSymbol p => p.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } ? "lambdaparam" : "param",
        ILocalSymbol => "local",
        ITypeParameterSymbol => "typeparam",
        IRangeVariableSymbol => "rangevar",
        ILabelSymbol => "label",
        _ => symbol.Kind.ToString().ToLowerInvariant(),
    };
}

// Rewrites C# outside the project assemblies (test validators, C# embedded in
// PowerShell here-strings) using a checkout of the source before renaming.
// Run from the pre-rename checkout: external <ledger.tsv> <target-root> <files...>
// Ledger: Deobfuscation/inferred_symbols.tsv (id_at_extraction, new_name columns).
static class External
{
    static readonly Regex HereString = new("@(['\"])\\r?\\n(.*?)\\r?\\n\\1@", RegexOptions.Singleline | RegexOptions.Compiled);

    public static int Run(ProjectModel project, string ledgerPath, string targetRoot, string[] files)
    {
        var ledger = new Dictionary<string, string>();
        foreach (var line in ledgerPath == "-" ? Array.Empty<string>() : File.ReadLines(ledgerPath).Skip(1))
        {
            var c = line.Split('\t');
            if (c.Length > 5)
            {
                ledger[c[1]] = c[5];
            }
        }
        var groups = Groups.Build(project);
        var mapping = new Dictionary<string, string>();
        foreach (var k in groups.Keys)
        {
            if (ledger.TryGetValue(groups.Find(k), out var n))
            {
                mapping[k] = n;
            }
        }
        foreach (var (k, n) in ledger)
        {
            mapping[k] = n;
        }

        // Test fixtures often carry stub copies of recovered types; their members
        // take the name of the project member with the same documentation id.
        var byDocId = new Dictionary<string, string>();
        var conflictingDocIds = new HashSet<string>();
        foreach (var tree in project.OurTrees)
        {
            var model = project.Model(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is not (MemberDeclarationSyntax or VariableDeclaratorSyntax or EnumMemberDeclarationSyntax))
                {
                    continue;
                }
                var declared = model.GetDeclaredSymbol(node);
                if (declared == null || !Program.Obfuscated.IsMatch(declared.Name))
                {
                    continue;
                }
                var docId = ProjectModel.Normalize(declared).GetDocumentationCommentId();
                if (docId == null || !mapping.TryGetValue(project.Id(declared), out var name))
                {
                    continue;
                }
                if (byDocId.TryGetValue(docId, out var existing) && existing != name)
                {
                    conflictingDocIds.Add(docId);
                }
                byDocId[docId] = name;
            }
        }
        foreach (var docId in conflictingDocIds)
        {
            byDocId.Remove(docId);
        }

        // Each external C# fragment becomes its own tree in one compilation that
        // references every project assembly.
        var fragments = new List<(string file, int offset, SyntaxTree tree)>();
        var parse = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse, SourceCodeKind.Regular,
            project.Units.First(u => u.Name == "Assembly-CSharp-Editor").Defines);
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            if (file.EndsWith(".cs", StringComparison.Ordinal))
            {
                fragments.Add((file, 0, CSharpSyntaxTree.ParseText(text, parse, file)));
                continue;
            }
            foreach (Match m in HereString.Matches(text))
            {
                var body = m.Groups[2];
                if (!Regex.IsMatch(body.Value, "\\b(class|static|void|var|return|using)\\b"))
                {
                    continue;
                }
                fragments.Add((file, body.Index, CSharpSyntaxTree.ParseText(body.Value, parse, file + "@" + body.Index)));
            }
        }
        var editor = project.Units.First(u => u.Name == "Assembly-CSharp-Editor");
        var refs = editor.Compilation.References.ToList();
        refs.Add(editor.Compilation.ToMetadataReference());
        var external = CSharpCompilation.Create("External", fragments.Select(f => f.tree), refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        if (ledgerPath == "-")
        {
            // Check mode: per-file error counts of the external fragments.
            foreach (var group in external.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error && d.Location.SourceTree != null)
                .GroupBy(d => project.Rel(d.Location.SourceTree.FilePath.Split('@')[0])).OrderBy(g => g.Key))
            {
                Console.WriteLine($"{group.Key}\t{group.Count()}");
                foreach (var d in group.Where(d => d.Id is "CS0117" or "CS1061" or "CS0103" or "CS0246").Take(50))
                {
                    Console.WriteLine($"  {d.Id} {d.GetMessage()}");
                }
            }
            return 0;
        }

        var edits = new Dictionary<string, List<TextChange>>();
        var unresolved = new List<string>();
        var applied = 0;
        foreach (var (file, offset, tree) in fragments)
        {
            var model = external.GetSemanticModel(tree);
            foreach (var token in ProjectModel.IdentifierTokens(tree.GetRoot()))
            {
                if (!Program.Obfuscated.IsMatch(token.ValueText))
                {
                    continue;
                }
                var symbol = ProjectModel.Resolve(model, token);
                var id = symbol == null ? null : project.Id(symbol);
                var line = tree.GetLineSpan(token.Span).StartLinePosition.Line + 1;
                string newName = null;
                if (id != null && !mapping.TryGetValue(id, out newName))
                {
                    var docId = ProjectModel.Normalize(symbol).GetDocumentationCommentId();
                    if (docId != null)
                    {
                        byDocId.TryGetValue(docId, out newName);
                    }
                }
                if (newName == null)
                {
                    unresolved.Add($"{project.Rel(file)}\t{token.ValueText}\t{(symbol == null ? "unbound" : "unmapped " + id)}\t{offset}:{line}");
                    continue;
                }
                if (!edits.TryGetValue(file, out var list))
                {
                    edits[file] = list = new List<TextChange>();
                }
                list.Add(new TextChange(new TextSpan(offset + token.SpanStart, token.Span.Length), newName));
                applied++;
            }
            // Reflection literals next to typeof(T).
            foreach (var lit in tree.GetRoot().DescendantTokens().Where(t => t.IsKind(SyntaxKind.StringLiteralToken) && Program.Obfuscated.IsMatch(t.ValueText) && t.Text == "\"" + t.ValueText + "\""))
            {
                INamedTypeSymbol owner = null;
                for (var node = lit.Parent; node != null && owner == null; node = node.Parent)
                {
                    var typeOf = node.DescendantNodes().OfType<TypeOfExpressionSyntax>().FirstOrDefault();
                    if (typeOf != null)
                    {
                        owner = model.GetTypeInfo(typeOf.Type).Type as INamedTypeSymbol;
                    }
                    if (node is StatementSyntax or MemberDeclarationSyntax)
                    {
                        break;
                    }
                }
                var candidates = new List<ISymbol>();
                for (var t = owner; t != null; t = t.BaseType)
                {
                    candidates.AddRange(t.GetMembers(lit.ValueText));
                }
                var ids = candidates.Select(project.Id).Distinct().ToList();
                if (ids.Count == 1 && mapping.TryGetValue(ids[0], out var n2))
                {
                    if (!edits.TryGetValue(file, out var list))
                    {
                        edits[file] = list = new List<TextChange>();
                    }
                    list.Add(new TextChange(new TextSpan(offset + lit.SpanStart + 1, lit.ValueText.Length), n2));
                    applied++;
                }
                else
                {
                    unresolved.Add($"{project.Rel(file)}\t{lit.ValueText}\tstring-literal\t{offset}:{tree.GetLineSpan(lit.Span).StartLinePosition.Line + 1}");
                }
            }
        }

        foreach (var (file, list) in edits)
        {
            var target = Path.Combine(targetRoot, project.Rel(file));
            var current = File.ReadAllText(target);
            var original = File.ReadAllText(file);
            if (current != original)
            {
                Console.WriteLine($"skipped (target differs from base): {project.Rel(file)}");
                continue;
            }
            var bytes = File.ReadAllBytes(target);
            var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            var newText = SourceText.From(original).WithChanges(list.GroupBy(c => c.Span).Select(g => g.First()));
            File.WriteAllText(target, newText.ToString(), new UTF8Encoding(hasBom));
        }
        Console.WriteLine($"fragments: {fragments.Count}; token edits: {applied} in {edits.Count} files; unresolved: {unresolved.Count}");
        foreach (var u in unresolved)
        {
            Console.WriteLine("unresolved\t" + u);
        }
        return 0;
    }
}

static class Applier
{
    public static int Run(ProjectModel project, string[] args)
    {
        var dryRun = args.Contains("--dry-run");
        var dropConflicts = args.Contains("--drop-conflicts");
        var fileRenames = !args.Contains("--no-file-renames");
        var mapFiles = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToList();

        var requested = new Dictionary<string, string>();
        foreach (var mapFile in mapFiles)
        {
            foreach (var raw in File.ReadLines(mapFile))
            {
                if (raw.Length == 0 || raw.StartsWith('#') || raw.StartsWith("id\t", StringComparison.Ordinal))
                {
                    continue;
                }
                var cols = raw.Split('\t');
                if (cols.Length < 2 || cols[1].Length == 0)
                {
                    continue;
                }
                var name = cols[1].Trim();
                if (!SyntaxFacts.IsValidIdentifier(name) || SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
                {
                    Console.Error.WriteLine($"invalid identifier '{name}' for {cols[0]}; skipped");
                    continue;
                }
                requested[cols[0]] = name;
            }
        }

        var groups = Groups.Build(project);
        var rejected = new Dictionary<string, string>();
        var mapping = Expand(requested, groups);

        var before = Bind(project);
        var beforeErrors = ErrorKeys(project);

        for (var attempt = 0; ; attempt++)
        {
            var (texts, edits, renamedTypes) = Rewrite(project, before, mapping);
            Console.WriteLine($"attempt {attempt}: {mapping.Count} symbols, {edits} token edits in {texts.Count} files");
            var after = ProjectModel.Load(project.Root, texts);
            var afterBind = Bind(after);
            var conflicts = new HashSet<string>();
            var report = new List<string>();
            foreach (var (path, seq) in before)
            {
                var seq2 = afterBind[path];
                if (seq.Count != seq2.Count)
                {
                    report.Add($"{project.Rel(path)}: token count changed {seq.Count} -> {seq2.Count}");
                    continue;
                }
                for (var i = 0; i < seq.Count; i++)
                {
                    if (seq[i].id == seq2[i].id)
                    {
                        continue;
                    }
                    report.Add($"{project.Rel(path)}:{seq[i].line}: '{seq[i].text}' bound {seq[i].id} now {seq2[i].id}");
                    foreach (var id in new[] { seq[i].id, seq2[i].id })
                    {
                        if (id != null && mapping.ContainsKey(id))
                        {
                            conflicts.Add(id);
                        }
                    }
                }
            }
            var afterErrors = ErrorKeys(after);
            var newErrors = afterErrors.Except(beforeErrors).ToList();
            foreach (var e in newErrors)
            {
                report.Add("new error: " + e);
            }
            if (newErrors.Count > 0)
            {
                // Attribute new errors to mapped symbols whose new name appears in the message.
                foreach (var e in newErrors)
                {
                    foreach (var (id, name) in mapping)
                    {
                        if (e.Contains("'" + name + "'", StringComparison.Ordinal) || e.Contains(name + "'", StringComparison.Ordinal) || e.Contains("." + name, StringComparison.Ordinal))
                        {
                            conflicts.Add(id);
                        }
                    }
                }
            }

            if (report.Count == 0)
            {
                Console.WriteLine("verification passed: all identifier bindings preserved, no new errors");
                if (!dryRun)
                {
                    Write(project, texts, renamedTypes, fileRenames);
                }
                foreach (var (id, reason) in rejected)
                {
                    Console.WriteLine($"rejected\t{id}\t{reason}");
                }
                WarnStrings(project, mapping);
                return 0;
            }

            foreach (var line in report.Take(60))
            {
                Console.WriteLine("  " + line);
            }
            if (report.Count > 60)
            {
                Console.WriteLine($"  ... {report.Count - 60} more");
            }
            if (!dropConflicts || conflicts.Count == 0 || attempt >= 12)
            {
                Console.WriteLine("verification failed; nothing written");
                return 1;
            }
            foreach (var id in conflicts)
            {
                var root = groups.Find(id);
                foreach (var k in mapping.Keys.Where(k => groups.Find(k) == root).ToList())
                {
                    rejected[k] = "binding/compile conflict as " + mapping[k];
                    mapping.Remove(k);
                }
            }
        }
    }

    static Dictionary<string, string> Expand(Dictionary<string, string> requested, Groups groups)
    {
        var mapping = new Dictionary<string, string>();
        foreach (var (id, name) in requested)
        {
            var root = groups.Find(id);
            mapping[id] = name;
            mapping[root] = name;
            foreach (var k in groups.Keys.Where(k => groups.Find(k) == root))
            {
                mapping[k] = name;
            }
        }
        return mapping;
    }

    sealed record Bound(string id, string text, int line, int start);

    static Dictionary<string, List<Bound>> Bind(ProjectModel project)
    {
        var result = new ConcurrentDictionary<string, List<Bound>>();
        Parallel.ForEach(project.OurTrees, tree =>
        {
            var model = project.Model(tree);
            var text = tree.GetText();
            var list = new List<Bound>();
            foreach (var token in ProjectModel.IdentifierTokens(tree.GetRoot()))
            {
                ISymbol symbol = null;
                try
                {
                    symbol = ProjectModel.Resolve(model, token);
                }
                catch (Exception)
                {
                }
                var id = symbol == null ? "?" + token.ValueText : project.Id(symbol);
                list.Add(new Bound(id, token.ValueText, text.Lines.GetLineFromPosition(token.SpanStart).LineNumber + 1, token.SpanStart));
            }
            result[tree.FilePath] = list;
        });
        return new Dictionary<string, List<Bound>>(result);
    }

    static HashSet<string> ErrorKeys(ProjectModel project)
    {
        var set = new HashSet<string>();
        foreach (var unit in project.Units.Where(u => u.Ours))
        {
            foreach (var d in unit.Compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                var path = d.Location.SourceTree?.FilePath;
                set.Add($"{unit.Name} {d.Id} {(path == null ? "" : project.Rel(path))} {d.GetMessage()}");
            }
        }
        return set;
    }

    static (Dictionary<string, SourceText> texts, int edits, Dictionary<string, string> renamedTypes) Rewrite(
        ProjectModel project, Dictionary<string, List<Bound>> bound, Dictionary<string, string> mapping)
    {
        var texts = new Dictionary<string, SourceText>();
        var edits = 0;
        foreach (var (path, seq) in bound)
        {
            var changes = new List<TextChange>();
            foreach (var b in seq)
            {
                if (b.id != null && mapping.TryGetValue(b.id, out var name) && Program.Obfuscated.IsMatch(b.text))
                {
                    changes.Add(new TextChange(new TextSpan(b.start, b.text.Length), name));
                }
            }
            var tree = project.TreesByPath[path];
            changes.AddRange(ReflectionLiterals(project, tree, mapping));
            if (changes.Count == 0)
            {
                continue;
            }
            // Verbatim '@' prefixes are not used by the decompiler output for these names.
            texts[path] = tree.GetText().WithChanges(changes);
            edits += changes.Count;
        }

        // Top-level types whose script file carries the obfuscated name.
        var renamedTypes = new Dictionary<string, string>();
        foreach (var tree in project.OurTrees)
        {
            var stem = Path.GetFileNameWithoutExtension(tree.FilePath);
            if (!Program.Obfuscated.IsMatch(stem))
            {
                continue;
            }
            var model = project.Model(tree);
            var top = tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>()
                .Where(t => t.Parent is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax)
                .Where(t => (t as BaseTypeDeclarationSyntax)?.Identifier.ValueText == stem || (t as DelegateDeclarationSyntax)?.Identifier.ValueText == stem)
                .Select(t => model.GetDeclaredSymbol(t)).FirstOrDefault();
            if (top != null && mapping.TryGetValue(project.Id(top), out var newName))
            {
                renamedTypes[tree.FilePath] = newName;
            }
        }
        return (texts, edits, renamedTypes);
    }

    // String literals naming a member of a nearby typeof(T), as used by reflection
    // lookups such as typeof(T).GetField("NAME") or (typeof(T), "NAME") tuples.
    static IEnumerable<TextChange> ReflectionLiterals(ProjectModel project, SyntaxTree tree, Dictionary<string, string> mapping)
    {
        SemanticModel model = null;
        foreach (var lit in tree.GetRoot().DescendantTokens().Where(t => t.IsKind(SyntaxKind.StringLiteralToken)))
        {
            if (!Program.Obfuscated.IsMatch(lit.ValueText) || lit.Text != "\"" + lit.ValueText + "\"")
            {
                continue;
            }
            model ??= project.Model(tree);
            INamedTypeSymbol owner = null;
            for (var node = lit.Parent; node != null && owner == null; node = node.Parent)
            {
                var typeOf = node.DescendantNodes().OfType<TypeOfExpressionSyntax>().FirstOrDefault();
                if (typeOf != null)
                {
                    owner = model.GetTypeInfo(typeOf.Type).Type as INamedTypeSymbol;
                }
                if (node is StatementSyntax or MemberDeclarationSyntax)
                {
                    break;
                }
            }
            if (owner == null)
            {
                continue;
            }
            var candidates = new List<ISymbol>();
            for (var t = owner; t != null; t = t.BaseType)
            {
                candidates.AddRange(t.GetMembers(lit.ValueText));
            }
            var ids = candidates.Select(project.Id).Distinct().ToList();
            if (ids.Count == 1 && mapping.TryGetValue(ids[0], out var newName))
            {
                yield return new TextChange(lit.Span, "\"" + newName + "\"");
            }
        }
    }

    static void Write(ProjectModel project, Dictionary<string, SourceText> texts, Dictionary<string, string> renamedTypes, bool fileRenames)
    {
        foreach (var (path, text) in texts)
        {
            var original = File.ReadAllBytes(path);
            var hasBom = original.Length >= 3 && original[0] == 0xEF && original[1] == 0xBB && original[2] == 0xBF;
            File.WriteAllText(path, text.ToString(), new UTF8Encoding(hasBom));
        }
        Console.WriteLine($"wrote {texts.Count} files");
        if (!fileRenames)
        {
            return;
        }
        var logPath = Path.Combine(project.Root, "Deobfuscation", "inferred_file_renames.tsv");
        var log = new List<string>();
        foreach (var (path, newName) in renamedTypes)
        {
            var dest = Path.Combine(Path.GetDirectoryName(path)!, newName + ".cs");
            if (File.Exists(dest))
            {
                Console.WriteLine($"file rename skipped (exists): {project.Rel(dest)}");
                continue;
            }
            File.Move(path, dest);
            if (File.Exists(path + ".meta"))
            {
                File.Move(path + ".meta", dest + ".meta");
            }
            log.Add($"{project.Rel(path)}\t{project.Rel(dest)}");
        }
        if (log.Count > 0)
        {
            if (!File.Exists(logPath))
            {
                File.WriteAllText(logPath, "old_path\tnew_path\n");
            }
            File.AppendAllLines(logPath, log);
            Console.WriteLine($"renamed {log.Count} script files");
        }
    }

    static void WarnStrings(ProjectModel project, Dictionary<string, string> mapping)
    {
        var oldNames = new HashSet<string>();
        foreach (var tree in project.OurTrees)
        {
            // Names are collected from the bound map indirectly: any obfuscated literal is suspicious.
            foreach (var lit in tree.GetRoot().DescendantTokens().Where(t => t.IsKind(SyntaxKind.StringLiteralToken)))
            {
                if (Program.Obfuscated.IsMatch(lit.ValueText))
                {
                    Console.WriteLine($"warning: obfuscated-looking string literal \"{lit.ValueText}\" at {project.Rel(tree.FilePath)}:{tree.GetLineSpan(lit.Span).StartLinePosition.Line + 1}");
                }
            }
        }
    }
}
