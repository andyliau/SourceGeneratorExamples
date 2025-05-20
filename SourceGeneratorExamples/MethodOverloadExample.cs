using SourceGenerators;

namespace SourceGeneratorExamples
{
	public partial class MethodOverloadExample
	{
		[GenerateOverloads]
		public void Print(string message, int count = 2, bool upper = false)
		{
			for (int i = 0; i < count; i++)
			{
				Console.WriteLine(upper ? message.ToUpper() : message);
			}
		}

		[GenerateOverloads]
		public void Complex(string req, int a = 1, string? b = null, double c = 2.5, object? d = null, bool e = true)
		{
			Console.WriteLine($"Required: {req}, Optional: {a}, {b}, {c}, {d}, {e}");
			// Print out optional parameters with name and value
			Console.WriteLine("Optional parameters:");
			Console.WriteLine($"a: {a}");
			Console.WriteLine($"b: {b}");
			Console.WriteLine($"c: {c}");
			Console.WriteLine($"d: {d}");
			Console.WriteLine($"e: {e}");
		}
	}
}
