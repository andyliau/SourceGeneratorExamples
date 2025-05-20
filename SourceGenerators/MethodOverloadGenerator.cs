using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerators;

[Generator]
public class MethodOverloadGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Use ForAttributeWithMetadataName to find methods with [GenerateOverloads]
        var methodDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "SourceGenerators.GenerateOverloadsAttribute",
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => (MethodDeclarationSyntax)ctx.TargetNode
            );

        context.RegisterSourceOutput(
            methodDeclarations,
            (spc, method) =>
            {
                var classDecl = method.Parent as ClassDeclarationSyntax;
                if (classDecl == null) return;

                var ns = method.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
                var parameters = method.ParameterList.Parameters;
                var requiredCount = parameters.Count(p => p.Default == null);
                var optionalParams = parameters.Skip(requiredCount).ToList();
                if (optionalParams.Count == 0) return;

                // Generate all possible combinations of optional parameters (regardless of type)
                int combinations = (1 << optionalParams.Count) - 1;
                var allCombos = new List<(List<ParameterSyntax> paramList, List<string> invokeParams)>();
                for (int mask = 0; mask <= combinations; mask++)
                {
                    var paramList = parameters.Take(requiredCount).ToList();
                    var invokeParams = paramList.Select(p => p.Identifier.ToString()).ToList();
                    for (int i = 0; i < optionalParams.Count; i++)
                    {
                        var p = optionalParams[i];
                        if ((mask & (1 << i)) != 0)
                        {
                            paramList.Add(p);
                            invokeParams.Add(p.Identifier.ToString());
                        }
                        else
                        {
                            invokeParams.Add(p.Default!.Value.ToFullString());
                        }
                    }
                    // Only generate overloads with fewer parameters than the original
                    if (paramList.Count == parameters.Count)
                        continue;
                    allCombos.Add((paramList, invokeParams));
                }

                var originalTypes = parameters.Select(p => p.Type?.ToString()).ToList();
                var validCombos = new List<(List<ParameterSyntax> paramList, List<string> invokeParams)>();
                foreach (var combo in allCombos.OrderBy(c => c.paramList.Count))
                {
                    var candidateTypes = combo.paramList.Select(p => p.Type?.ToString()).ToList();
                    // Skip overloads whose type sequence matches the full original signature
                    if (candidateTypes.Count == originalTypes.Count && candidateTypes.SequenceEqual(originalTypes))
                        continue;
                    // Only allow if not ambiguous with any already-added overload (by type sequence)
                    bool ambiguous = validCombos.Any(existing =>
                        existing.paramList.Count == combo.paramList.Count &&
                        existing.paramList.Select(p => p.Type?.ToString()).SequenceEqual(candidateTypes)
                    );
                    if (!ambiguous)
                        validCombos.Add(combo);
                }

                int overloadIndex = 1;
                foreach (var (paramList, invokeParams) in validCombos)
                {
                    var paramTypes = string.Join(", ", paramList.Select(p => $"{p.Type} {p.Identifier}"));
                    var invokeArgs = string.Join(", ", invokeParams);
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(ns))
                        sb.AppendLine($"namespace {ns} {{");
                    sb.AppendLine($"partial class {classDecl.Identifier} {{");
                    sb.AppendLine($"    public {method.ReturnType} {method.Identifier}({paramTypes}) => {method.Identifier}({invokeArgs});");
                    sb.AppendLine("}");
                    if (!string.IsNullOrEmpty(ns))
                        sb.AppendLine("}");
                    spc.AddSource(
                        $"{classDecl.Identifier}_{method.Identifier}_overload_{overloadIndex++}.g.cs",
                        sb.ToString());
                }
            }
        );
    }
}