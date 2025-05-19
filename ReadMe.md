# SourceGeneratorExamples

This repository demonstrates the use of C# source generators to automatically create method overloads for methods with optional parameters. It consists of two projects:

- **SourceGeneratorExamples**: A sample console application that uses the source generator.
- **SourceGenerators**: A C# source generator that scans for methods marked with the [`SourceGenerators.GenerateOverloadsAttribute`](SourceGenerators/GenerateOverloadsAttribute.cs) and generates overloads by omitting optional parameters.

## How It Works

1. Mark a method with `[GenerateOverloads]`.
2. The [`SourceGenerators.MethodOverloadGenerator`](SourceGenerators/MethodOverloadGenerator.cs) will generate additional overloads for that method, each omitting one or more optional parameters.

**Example:**

```csharp
using SourceGenerators;

public partial class Example
{
    [GenerateOverloads]
    public void Print(string message, int count = 2, bool upper = false)
    {
        // Implementation...
    }
}
```

The generator will produce overloads like:

```csharp
public void Print(string message, int count)
public void Print(string message)
```

## Getting Started

1. Clone the repository.
2. Open `SourceGeneratorExamples.sln` in Visual Studio 2022 or later.
3. Build the solution. The generator will run automatically and generate overloads for marked methods.

## Contributing

Contributions are welcome! To contribute:

1. Fork this repository.
2. Create a new branch for your feature or bugfix.
3. Make your changes and add tests if applicable.
4. Submit a pull request describing your changes.

Please ensure your code follows the existing style and passes all builds.

## License

This project is licensed under the MIT License.