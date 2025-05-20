// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var overloadExample = new SourceGeneratorExamples.MethodOverloadExample();
overloadExample.Print("Hello, World!");

overloadExample.Print("Hello, World!", true);

overloadExample.Print("Hello, World!", 3);

overloadExample.Complex("required", 3);