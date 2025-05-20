using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

public class MethodOverloadGeneratorTests
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
    public async Task Generates_Overloads_For_Optional_Parameters()
    {
        var input = @"
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Foo(int x, int y = 1, int z = 2) { }
}
";

        // The expected output may need to be split into separate files for each overload
        var expected1 = @"partial class MyClass {
    public void Foo(int x, int y) => Foo(x, y, 2);
}
";
        var expected2 = @"partial class MyClass {
    public void Foo(int x) => Foo(x, 1, 2);
}
";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, XUnitVerifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Foo_overload_1.g.cs", expected1),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Foo_overload_2.g.cs", expected2),
                }
            }
        };

        await test.RunAsync();
    }
}
