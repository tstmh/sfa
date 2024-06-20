using ATTSystems.SAF.API;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args).ConfigureLogging(logBuilder =>
        {
            logBuilder.ClearProviders(); // in LoggerFactory eliminate the entire providers
            logBuilder.AddConsole();
            logBuilder.AddTraceSource("Information, ActivityTracing"); //to include the trace Listener Provider
        })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}