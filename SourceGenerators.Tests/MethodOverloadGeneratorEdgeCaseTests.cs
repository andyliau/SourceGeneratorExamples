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

		var expected0 = """
partial class MyClass {
    public void Baz() => Baz(null, 42);
}
""";
        var expected1 = """
partial class MyClass {
    public void Baz(string? s) => Baz(s, 42);
}
""";
        var expected2 = """
partial class MyClass {
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

		var expected1 = """
partial class MyClass {
    public void Quux(int a) => Quux(a, 1, 2, 3);
}
""";
        var expected2 = """
partial class MyClass {
    public void Quux(int a, int b) => Quux(a, b, 2, 3);
}
""";
        var expected3 = """
partial class MyClass {
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

		var expected1 = """
partial class MyClass {
    public void Quack(string a) => Quack(a, "1", 2, 3);
}
""";
        var expected2 = """
partial class MyClass {
    public void Quack(string a, string b) => Quack(a, b, 2, 3);
}
""";
        var expected3 = """
partial class MyClass {
    public void Quack(string a, int c) => Quack(a, "1", c, 3);
}
""";
        var expected4 = """
partial class MyClass {
    public void Quack(string a, string b, int c) => Quack(a, b, c, 3);
}
""";
        var expected5 = """
partial class MyClass {
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

	[Test]
	public async Task Handles_Method_With_Complex_Optional_Parameters()
	{
		var input = @"
using SourceGenerators;

public partial class MyClass
{
    [GenerateOverloads]
    public void Complex(string req, int a = 1, string? b = null, double c = 2.5, object? d = null, bool e = true) { }
}
";

		var expected0 = """
partial class MyClass {
    public void Complex(string req) => Complex(req, 1, null, 2.5, null, true);
}
""";
        var expected1 = """
partial class MyClass {
    public void Complex(string req, int a) => Complex(req, a, null, 2.5, null, true);
}
""";
        var expected2 = """
partial class MyClass {
    public void Complex(string req, string? b) => Complex(req, 1, b, 2.5, null, true);
}
""";
        var expected3 = """
partial class MyClass {
    public void Complex(string req, double c) => Complex(req, 1, null, c, null, true);
}
""";
        var expected4 = """
partial class MyClass {
    public void Complex(string req, object? d) => Complex(req, 1, null, 2.5, d, true);
}
""";
        var expected5 = """
partial class MyClass {
    public void Complex(string req, bool e) => Complex(req, 1, null, 2.5, null, e);
}
""";
        var expected6 = """
partial class MyClass {
    public void Complex(string req, int a, string? b) => Complex(req, a, b, 2.5, null, true);
}
""";
        var expected7 = """
partial class MyClass {
    public void Complex(string req, int a, double c) => Complex(req, a, null, c, null, true);
}
""";
        var expected8 = """
partial class MyClass {
    public void Complex(string req, string? b, double c) => Complex(req, 1, b, c, null, true);
}
""";
        var expected9 = """
partial class MyClass {
    public void Complex(string req, int a, object? d) => Complex(req, a, null, 2.5, d, true);
}
""";
        var expected10 = """
partial class MyClass {
    public void Complex(string req, string? b, object? d) => Complex(req, 1, b, 2.5, d, true);
}
""";
        var expected11 = """
partial class MyClass {
    public void Complex(string req, double c, object? d) => Complex(req, 1, null, c, d, true);
}
""";
        var expected12 = """
partial class MyClass {
    public void Complex(string req, int a, bool e) => Complex(req, a, null, 2.5, null, e);
}
""";
        var expected13 = """
partial class MyClass {
    public void Complex(string req, int a, object? d) => Complex(req, a, null, 2.5, d, true);
}
""";
        var expected14 = """
partial class MyClass {
    public void Complex(string req, string? b, int c) => Complex(req, 1, b, c, null, true);
}
""";
        var expected15 = """
partial class MyClass {
    public void Complex(string req, string? b, bool e) => Complex(req, 1, b, 2.5, null, e);
}
""";
        var expected16 = """
partial class MyClass {
    public void Complex(string req, double c, object? d) => Complex(req, 1, null, c, d, true);
}
""";
        var expected17 = """
partial class MyClass {
    public void Complex(string req, double c, bool e) => Complex(req, 1, null, c, null, e);
}
""";
        var expected18 = """
partial class MyClass {
    public void Complex(string req, object? d, bool e) => Complex(req, 1, null, 2.5, d, e);
}
""";
        var expected19 = """
partial class MyClass {
    public void Complex(string req, int a, string? b, double c) => Complex(req, a, b, c, null, true);
}
""";
        var expected20 = """
partial class MyClass {
    public void Complex(string req, int a, string? b, object? d) => Complex(req, a, b, 2.5, d, true);
}
""";
        var expected21 = """
partial class MyClass {
    public void Complex(string req, int a, string? b, bool e) => Complex(req, a, b, 2.5, null, e);
}
""";
        var expected22 = """
partial class MyClass {
    public void Complex(string req, int a, double c, object? d) => Complex(req, a, null, c, d, true);
}
""";
        var expected23 = """
partial class MyClass {
    public void Complex(string req, int a, double c, bool e) => Complex(req, a, null, c, null, e);
}
""";
        var expected24 = """
partial class MyClass {
    public void Complex(string req, int a, object? d, bool e) => Complex(req, a, null, 2.5, d, e);
}
""";
        var expected25 = """
partial class MyClass {
    public void Complex(string req, string? b, double c, object? d) => Complex(req, 1, b, c, d, true);
}
""";
        var expected26 = """
partial class MyClass {
    public void Complex(string req, string? b, double c, bool e) => Complex(req, 1, b, c, null, e);
}
""";
        var expected27 = """
partial class MyClass {
    public void Complex(string req, string? b, object? d, bool e) => Complex(req, 1, b, 2.5, d, e);
}
""";
        var expected28 = """
partial class MyClass {
    public void Complex(string req, double c, object? d, bool e) => Complex(req, 1, null, c, d, e);
}
""";
        var expected29 = """
partial class MyClass {
    public void Complex(string req, int a, string? b, double c, object? d) => Complex(req, a, b, c, d, true);
}
""";
        var expected30 = """
partial class MyClass {
    public void Complex(string req, int a, string? b, double c, bool e) => Complex(req, a, b, c, null, e);
}
""";
        var expected31 = """
partial class MyClass {
    public void Complex(string req, int a, string? b, object? d, bool e) => Complex(req, a, b, 2.5, d, e);
}
""";
        var expected32 = """
partial class MyClass {
    public void Complex(string req, int a, double c, object? d, bool e) => Complex(req, a, null, c, d, e);
}
""";
        var expected33 = """
partial class MyClass {
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
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_1.g.cs", expected0),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_2.g.cs", expected1),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_3.g.cs", expected2),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_4.g.cs", expected3),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_5.g.cs", expected4),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_6.g.cs", expected5),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_7.g.cs", expected6),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_8.g.cs", expected7),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_9.g.cs", expected8),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_10.g.cs", expected9),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_11.g.cs", expected10),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_12.g.cs", expected11),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_13.g.cs", expected12),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_14.g.cs", expected13),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_15.g.cs", expected14),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_16.g.cs", expected15),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_17.g.cs", expected16),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_18.g.cs", expected17),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_19.g.cs", expected18),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_20.g.cs", expected19),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_21.g.cs", expected20),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_22.g.cs", expected21),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_23.g.cs", expected22),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_24.g.cs", expected23),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_25.g.cs", expected24),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_26.g.cs", expected25),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_27.g.cs", expected26),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_28.g.cs", expected27),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_29.g.cs", expected28),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_30.g.cs", expected29),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_31.g.cs", expected30),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_32.g.cs", expected31),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_33.g.cs", expected32),
					(typeof(SourceGenerators.MethodOverloadGenerator), "MyClass_Complex_overload_34.g.cs", expected33),
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
