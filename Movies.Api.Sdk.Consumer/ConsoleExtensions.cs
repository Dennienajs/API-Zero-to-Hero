using System.Text.Json;

namespace Movies.Api.Sdk.Consumer;

public static class ConsoleExtensions
{
    public static void WriteToConsole(this object obj) => Console.WriteLine(JsonSerializer.Serialize(obj));
}