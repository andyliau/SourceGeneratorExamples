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
	}
}
