#pragma warning disable CS0618 // XUnitVerifier is obsolete in Roslyn SDK, but required for source generator tests
using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.NUnitVerifier;

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

    [Test]
    public async Task Generates_Overloads_For_Optional_Parameters()
    {
        var input = """
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Foo(int x, string y = "x", bool z = false) { }
}
""";

        var expected = """
partial class MyClass {
    public void Foo(int x) => Foo(x, "x", false);
    public void Foo(int x, string y) => Foo(x, y, false);
    public void Foo(int x, bool z) => Foo(x, "x", z);
}

""";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Foo_overloads.g.cs", expected),
                }
            }
        };

        await test.RunAsync();
    }
}
#pragma warning restore CS0618
