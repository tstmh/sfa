namespace ATTSystems.SFA.Web
{
    using ATTSystems.NetCore;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.CookiePolicy;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;


    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            APPReference.Registered(configuration);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Setup Session

            services.AddDistributedMemoryCache();
            services.AddControllersWithViews();
            services.AddSession();

            services.AddMvc();
            int sessionMin = !string.IsNullOrEmpty(Configuration["AppSettings:SessionTimeoutMin"]) ? Convert.ToInt32(Configuration["AppSettings:SessionTimeoutMin"]) : 20;
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(sessionMin);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logoff";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionMin);
            });

            #endregion

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = HttpOnlyPolicy.Always;
            });
            services.AddHttpClient();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .WithExposedHeaders("Header1", "Header2");
                });
            });

            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
                options.Preload = true;
            });
            services.AddResponseCaching();
            services.Configure<Microsoft.AspNetCore.ResponseCaching.ResponseCachingOptions>(options =>
            {
                options.UseCaseSensitivePaths = true;
            });

            var mvcBuilder = services.AddControllersWithViews();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "SFAAuthCookie";
                options.Cookie.Path = "/SFA";
            });

            services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy2", builder =>
                {
                    builder.WithOrigins("http://solutions1domain.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(o => o.LoginPath = new PathString("/Auth/Login"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy",
               "default-src 'none'; " +
                "script-src 'self' 'unsafe-inline'; " +
               //"script-src 'self' 'unsafe-inline' 'nonce-{RANDOM}' https://google.com/; " +
               "style-src 'self' 'unsafe-inline' https://google.com/; " +
               "img-src 'self' data: https:; " +
                   //"connect-src 'self' wss://localhost:44376  https://ka-f.fontawesome.com/; " +
                   "connect-src 'self' * ws://localhost:44376 wss://localhost:44376; " +
               "form-action 'self' *; " +
               "frame-ancestors 'self' *; " +
               "base-uri 'self' *; " +
                //"upgrade-insecure-requests; " +
                "font-src 'self' https://google.com/ https://ka-f.fontawesome.com/;");


                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                context.Response.Headers.Add("Cache-Control", "must-revalidate");
                context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        MustRevalidate = true,
                        NoCache = true,
                        NoStore = true,

                    };

                if (context.Request.Headers["Accept"].ToString().Contains("text/html"))
                {
                    //context.Response.Headers["Content-Type"] = "text/html";
                }

                if (context.Request.Headers["Accept"].ToString().Contains(""))
                {
                    context.Response.Headers["X-Powered-By"] = " ";
                }

                context.Response.OnStarting(state =>
                {
                    var httpContext = (HttpContext)state;
                    httpContext.Response.Headers["Server"] = " ";
                    return Task.CompletedTask;
                }, context);

                await next();
            });

            app.UseCookiePolicy();
            app.UseCors("AllowOrigin");

            app.UseResponseCaching();

            app.UseCors("AllowAll");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseCors("MyCorsPolicy2");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=OnlinePortal}/{action=Register}/{id?}");
            });
        }
    }
}
