using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class MethodOverloadGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Find all method declarations in the compilation
		var methodDeclarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: (node, _) => node is MethodDeclarationSyntax,
				transform: (ctx, _) => (MethodDeclarationSyntax)ctx.Node
			)
			.Where(method => method.ParameterList.Parameters.Any(p => p.Default != null)); // Only methods with optional params

		context.RegisterSourceOutput(
			methodDeclarations,
			(spc, method) =>
			{
				var classDecl = method.Parent as ClassDeclarationSyntax;
				if (classDecl == null) return;

				var ns = method.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
				var parameters = method.ParameterList.Parameters;
				int optionalCount = parameters.Reverse().TakeWhile(p => p.Default != null).Count();
				if (optionalCount == 0) return;

				for (int i = 1; i <= optionalCount; i++)
				{
					var paramList = parameters.Take(parameters.Count - i).ToList();
					var defaultParams = parameters.Skip(parameters.Count - i).ToList();

					var paramTypes = string.Join(", ", paramList.Select(p => $"{p.Type} {p.Identifier}"));
					var invokeParams = string.Join(", ",
						paramList.Select(p => p.Identifier.ToString())
						.Concat(defaultParams.Select(p => p.Default!.Value.ToFullString())));

					var sb = new StringBuilder();
					if (!string.IsNullOrEmpty(ns))
						sb.AppendLine($"namespace {ns} {{");
					sb.AppendLine($"partial class {classDecl.Identifier} {{");
					sb.AppendLine($"    public {method.ReturnType} {method.Identifier}({paramTypes}) => {method.Identifier}({invokeParams});");
					sb.AppendLine("}");
					if (!string.IsNullOrEmpty(ns))
						sb.AppendLine("}");

					spc.AddSource(
						$"{classDecl.Identifier}_{method.Identifier}_overload_{i}.g.cs",
						sb.ToString());
				}
			}
		);
	}
}