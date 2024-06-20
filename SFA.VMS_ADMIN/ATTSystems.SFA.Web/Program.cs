using ATTSystems.SFA.Web;

namespace ATTSystems.NetCoreProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logBuilder =>
            {
                logBuilder.ClearProviders(); // in LoggerFactory eliminate the entire providers
                logBuilder.AddConsole();
                logBuilder.AddTraceSource("Information, ActivityTracing"); //to include the trace Listener Provider
            })
           .ConfigureWebHostDefaults(webBuilder =>
           {
                webBuilder.UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                });

                webBuilder.UseIISIntegration().UseIIS();
               webBuilder.ConfigureServices(services =>
               {
                   services.AddControllersWithViews().AddRazorRuntimeCompilation();
               });
               webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.AddServerHeader = false;
                });

                webBuilder.UseStartup<Startup>();
           });
    }
}