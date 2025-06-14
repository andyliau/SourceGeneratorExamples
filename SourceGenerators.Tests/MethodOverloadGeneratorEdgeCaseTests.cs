using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Verifier = Microsoft.CodeAnalysis.Testing.Verifiers.NUnitVerifier;

public class MethodOverloadGeneratorEdgeCaseTests
{
	const string AttributeSource = """
using System;	
namespace SourceGenerators
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GenerateOverloadsAttribute : Attribute
    {
    }
}
""";

	[Test]
	public async Task No_Optional_Parameters_Generates_No_Overloads()
	{
		var input = """
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Bar(int x, int y) { }
}
""";

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
		var input = """
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Baz(string? s = null, int n = 42) { }
}
""";

		var expected = """
partial class MyClass {
    public void Baz() => Baz(null, 42);
    public void Baz(string? s) => Baz(s, 42);
    public void Baz(int n) => Baz(null, n);
}

""";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Baz_overloads.g.cs", expected),
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
		var input = """
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Quux(int a, int b = 1, int c = 2, int d = 3) { }
}
""";

		var expected = """
partial class MyClass {
    public void Quux(int a) => Quux(a, 1, 2, 3);
    public void Quux(int a, int b) => Quux(a, b, 2, 3);
    public void Quux(int a, int b, int c) => Quux(a, b, c, 3);
}

""";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quux_overloads.g.cs", expected),
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
		var input = """
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Quack(string a, string b = "1", int c = 2, int d = 3) { }
}
""";

		var expected = """
partial class MyClass {
    public void Quack(string a) => Quack(a, "1", 2, 3);
    public void Quack(string a, string b) => Quack(a, b, 2, 3);
    public void Quack(string a, int c) => Quack(a, "1", c, 3);
    public void Quack(string a, string b, int c) => Quack(a, b, c, 3);
    public void Quack(string a, int c, int d) => Quack(a, "1", c, d);
}

""";

        var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
        {
            TestState =
            {
                Sources = { AttributeSource, input },
                GeneratedSources =
                {
                    (typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Quack_overloads.g.cs", expected),
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
	public async Task Handles_Method_With_Complex_Optional_Parameters()
	{
		var input = """
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Complex(string req, int a = 1, string? b = null, double c = 2.5, object? d = null, bool e = true) { }
}
""";

		var expected = """
partial class MyClass {
    public void Complex(string req) => Complex(req, 1, null, 2.5, null, true);
    public void Complex(string req, int a) => Complex(req, a, null, 2.5, null, true);
    public void Complex(string req, string? b) => Complex(req, 1, b, 2.5, null, true);
    public void Complex(string req, double c) => Complex(req, 1, null, c, null, true);
    public void Complex(string req, object? d) => Complex(req, 1, null, 2.5, d, true);
    public void Complex(string req, bool e) => Complex(req, 1, null, 2.5, null, e);
    public void Complex(string req, int a, string? b) => Complex(req, a, b, 2.5, null, true);
    public void Complex(string req, int a, double c) => Complex(req, a, null, c, null, true);
    public void Complex(string req, string? b, double c) => Complex(req, 1, b, c, null, true);
    public void Complex(string req, int a, object? d) => Complex(req, a, null, 2.5, d, true);
    public void Complex(string req, string? b, object? d) => Complex(req, 1, b, 2.5, d, true);
    public void Complex(string req, double c, object? d) => Complex(req, 1, null, c, d, true);
    public void Complex(string req, int a, bool e) => Complex(req, a, null, 2.5, null, e);
    public void Complex(string req, string? b, bool e) => Complex(req, 1, b, 2.5, null, e);
    public void Complex(string req, double c, bool e) => Complex(req, 1, null, c, null, e);
    public void Complex(string req, object? d, bool e) => Complex(req, 1, null, 2.5, d, e);
    public void Complex(string req, int a, string? b, double c) => Complex(req, a, b, c, null, true);
    public void Complex(string req, int a, string? b, object? d) => Complex(req, a, b, 2.5, d, true);
    public void Complex(string req, int a, double c, object? d) => Complex(req, a, null, c, d, true);
    public void Complex(string req, string? b, double c, object? d) => Complex(req, 1, b, c, d, true);
    public void Complex(string req, int a, string? b, bool e) => Complex(req, a, b, 2.5, null, e);
    public void Complex(string req, int a, double c, bool e) => Complex(req, a, null, c, null, e);
    public void Complex(string req, string? b, double c, bool e) => Complex(req, 1, b, c, null, e);
    public void Complex(string req, int a, object? d, bool e) => Complex(req, a, null, 2.5, d, e);
    public void Complex(string req, string? b, object? d, bool e) => Complex(req, 1, b, 2.5, d, e);
    public void Complex(string req, double c, object? d, bool e) => Complex(req, 1, null, c, d, e);
    public void Complex(string req, int a, string? b, double c, object? d) => Complex(req, a, b, c, d, true);
    public void Complex(string req, int a, string? b, double c, bool e) => Complex(req, a, b, c, null, e);
    public void Complex(string req, int a, string? b, object? d, bool e) => Complex(req, a, b, 2.5, d, e);
    public void Complex(string req, int a, double c, object? d, bool e) => Complex(req, a, null, c, d, e);
    public void Complex(string req, string? b, double c, object? d, bool e) => Complex(req, 1, b, c, d, e);
}

""";


		var test = new CSharpSourceGeneratorTest<SourceGenerators.MethodOverloadGenerator, Verifier>
		{
			TestState =
			{
				Sources = { AttributeSource, input },
				GeneratedSources =
				{
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overloads.g.cs", expected),
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
