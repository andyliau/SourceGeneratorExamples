using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.NUnitVerifier;

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

    [Test]
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

        try
        {
            await test.RunAsync();
        }
        catch (AssertionException ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.Data != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    TestContext.WriteLine($"{key}: {ex.Data[key]}");
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.InnerException != null)
                TestContext.WriteLine("Inner: " + ex.InnerException.Message);
            throw;
        }
    }

    [Test]
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

        try
        {
            await test.RunAsync();
        }
        catch (AssertionException ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.Data != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    TestContext.WriteLine($"{key}: {ex.Data[key]}");
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.InnerException != null)
                TestContext.WriteLine("Inner: " + ex.InnerException.Message);
            throw;
        }
    }

    [Test]
    public async Task Handles_Method_With_Four_Parameters_And_Same_Type()
    {
        var input = @"
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Quux(int a, int b = 1, int c = 2, int d = 3) { }
}
";

        var expected1 = @"partial class MyClass {
    public void Quux(int a) => Quux(a, 1, 2, 3);
}
";
        var expected2 = @"partial class MyClass {
    public void Quux(int a, int b) => Quux(a, b, 2, 3);
}
";
        var expected3 = @"partial class MyClass {
    public void Quux(int a, int b, int c) => Quux(a, b, c, 3);
}
";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quux_overload_1.g.cs", expected1),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quux_overload_2.g.cs", expected2),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quux_overload_3.g.cs", expected3),
                }
            }
        };

        try
        {
            await test.RunAsync();
        }
        catch (AssertionException ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.Data != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    TestContext.WriteLine($"{key}: {ex.Data[key]}");
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.InnerException != null)
                TestContext.WriteLine("Inner: " + ex.InnerException.Message);
            throw;
        }
    }

    [Test]
    public async Task Handles_Method_With_Mixed_Optional_Types_Combinations()
    {
        var input = @"
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Quack(string a, string b = ""1"", int c = 2, int d = 3) { }
}
";

        var expected1 = @"partial class MyClass {
    public void Quack(string a) => Quack(a, ""1"", 2, 3);
}
";
        var expected2 = @"partial class MyClass {
    public void Quack(string a, string b) => Quack(a, b, 2, 3);
}
";
        var expected3 = @"partial class MyClass {
    public void Quack(string a, string b, int c) => Quack(a, b, c, 3);
}
";
        var expected4 = @"partial class MyClass {
    public void Quack(string a, int c, int d) => Quack(a, ""1"", c, d);
}
";
        var expected5 = @"partial class MyClass {
    public void Quack(string a, int c) => Quack(a, ""1"", c, 3);
}
";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quack_overload_1.g.cs", expected1),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quack_overload_2.g.cs", expected2),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quack_overload_3.g.cs", expected3),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quack_overload_4.g.cs", expected4),
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quack_overload_5.g.cs", expected5),
                }
            }
        };

        try
        {
            await test.RunAsync();
        }
        catch (AssertionException ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.Data != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    TestContext.WriteLine($"{key}: {ex.Data[key]}");
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            TestContext.WriteLine("Test failed: " + ex.Message);
            if (ex.InnerException != null)
                TestContext.WriteLine("Inner: " + ex.InnerException.Message);
            throw;
        }
    }
}
