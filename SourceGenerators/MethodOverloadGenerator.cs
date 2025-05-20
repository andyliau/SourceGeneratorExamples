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

                // Generate all valid combinations of optional parameters (except empty set)
                int combinations = (1 << optionalParams.Count) - 1;
                int overloadIndex = 1;
                for (int mask = 1; mask <= combinations; mask++)
                {
                    var paramList = parameters.Take(requiredCount).ToList();
                    var invokeParams = new List<string>();
                    // Always add required params to invocation
                    invokeParams.AddRange(paramList.Select(p => p.Identifier.ToString()));

                    // For each optional param, decide if it's included in this overload
                    for (int i = 0; i < optionalParams.Count; i++)
                    {
                        var p = optionalParams[i];
                        if ((mask & (1 << i)) != 0)
                        {
                            // Include in signature and invocation
                            paramList.Add(p);
                            invokeParams.Add(p.Identifier.ToString());
                        }
                        else
                        {
                            // Not included in signature, use default in invocation
                            invokeParams.Add(p.Default!.Value.ToFullString());
                        }
                    }

                    // Only generate overloads with fewer parameters than the original
                    if (paramList.Count == parameters.Count)
                        continue;

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