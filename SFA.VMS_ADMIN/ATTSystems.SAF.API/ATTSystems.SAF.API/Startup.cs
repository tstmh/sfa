namespace ATTSystems.SAF.API
{
    using ATTSystems.NetCore;
    using ATTSystems.SFA.DAL;
    using ATTSystems.SFA.DAL.Implementation;
    using ATTSystems.SFA.DAL.Implementations;
    using ATTSystems.SFA.DAL.Interface;
    using ATTSystems.SFA.Model.DBModel;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            APPReference.Registered(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddControllers();
            services.AddTransient<IBaseRepository, AuthenticationRepository>();
            services.AddTransient<IDisposable, AuthenticationRepository>();
            services.AddTransient<IAuthentication, AuthenticationRepository>();
            services.AddTransient<ISetting, SettingRepository>();
            services.AddTransient<IAdminPortal, AdminPortalRepository>();
            services.AddTransient<IReport, ReportRepository>();
            services.AddTransient<ISemacAccess, SemacAccessRepository>();
            services.AddTransient<IOnlinePortal, OnlinePortalRepository>();
            services.AddDbContext<DataContext>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ATTSystems.NetCoreProject.API", Version = "v1" });
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ATTSystems.NetCoreProject.API v1"));
            }
            loggerFactory.AddFile($@"{Directory.GetCurrentDirectory()}\Logs\log.txt");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
