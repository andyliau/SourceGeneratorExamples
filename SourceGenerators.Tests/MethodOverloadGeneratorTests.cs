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
    public void Foo(int x, string y = ""x"", bool z = false) { }
}
";

        var expected1 = @"partial class MyClass {
    public void Foo(int x) => Foo(x, ""x"", false);
}
";
        var expected2 = @"partial class MyClass {
    public void Foo(int x, string y) => Foo(x, y, false);
}
";
        var expected3 = @"partial class MyClass {
    public void Foo(int x, bool z) => Foo(x, ""x"", z);
}
";

        var expectedSet = new HashSet<string> { expected1, expected2, expected3 };

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, XUnitVerifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Foo_overload_1.g.cs", expected1),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Foo_overload_2.g.cs", expected2),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Foo_overload_3.g.cs", expected3),
                }
            }
        };

        await test.RunAsync();
    }
}
