using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier;

#pragma warning disable CS0618 // XUnitVerifier is obsolete in Roslyn SDK, but required for source generator tests
public class MethodOverloadGeneratorEdgeCaseTests
{
    const string AttributeSource = @"using System;
namespace SourceGenerators
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GenerateOverloadsAttribute : Attribute
    {
    }
}
";

    [Fact]
    public async Task No_Optional_Parameters_Generates_No_Overloads()
    {
        var input = @"
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Bar(int x, int y) { }
}
";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                // No generated sources expected
            }
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task Handles_Methods_With_Reference_And_Value_Types()
    {
        var input = @"
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Baz(string? s = null, int n = 42) { }
}
";

        var expected0 = @"partial class MyClass {
    public void Baz() => Baz(null, 42);
}
";
        var expected1 = @"partial class MyClass {
    public void Baz(string? s) => Baz(s, 42);
}
";
        var expected2 = @"partial class MyClass {
    public void Baz(int n) => Baz(null, n);
}
";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Baz_overload_1.g.cs", expected0),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Baz_overload_2.g.cs", expected1),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Baz_overload_3.g.cs", expected2),
                }
            }
        };

        await test.RunAsync();
    }
}
#pragma warning restore CS0618
