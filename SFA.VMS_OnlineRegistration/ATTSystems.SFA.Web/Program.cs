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

             Host.CreateDefaultBuilder(args).ConfigureLogging(logBuilder =>
             {
                 logBuilder.ClearProviders();
                 logBuilder.AddConsole();
                 logBuilder.AddTraceSource("Information, ActivityTracing");
             })
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseKestrel(options =>
                   {
                       options.AddServerHeader = false;

                   });

                   webBuilder.UseIISIntegration().UseIIS();

                   webBuilder.ConfigureKestrel(serverOptions =>
                   {
                       serverOptions.AddServerHeader = false;

                   });
                   webBuilder.UseStartup<Startup>();

                   webBuilder.ConfigureServices(services =>
                   {
                       services.AddControllersWithViews().AddRazorRuntimeCompilation();
                   });
               });

    }
}