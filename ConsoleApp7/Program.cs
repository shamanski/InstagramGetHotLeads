using Insta;

internal class Program
{
    private static void Main(string[] args)
    {
        var result = Task.Run(Loader.MainAsync).GetAwaiter().GetResult();
        if (result)
            return;
        Console.ReadKey();
    }
}