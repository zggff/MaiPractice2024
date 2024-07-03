namespace Zggff.MaiPractice;

public class Program
{
    public static void Main(string[] args)
    {
        System.Console.WriteLine("Server is here");
        CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

}