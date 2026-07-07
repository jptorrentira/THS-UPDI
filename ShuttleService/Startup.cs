using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShuttleService.BackgroundService;
using ShuttleService.Data;
using ShuttleService.Models;
using ShuttleService.Services;

namespace ShuttleService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.CommandTimeout(180) // Timeout in seconds
                )
                .EnableSensitiveDataLogging()   // Correct placement
            );

            // Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // App services
            services.AddScoped<PeopleCoreLinkedServerDBService>();
            services.AddScoped<ParcelRequestIdSequenceService>();

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(2));

            // Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrator"));
                options.AddPolicy("RequireAGSRole", policy => policy.RequireRole("AGS Personnel"));
                options.AddPolicy("RequireRequestorRole", policy => policy.RequireRole("Requestor"));
                options.AddPolicy("RequireAllRole", policy => policy.RequireRole("Requestor", "Administrator", "AGS Personnel"));
                options.AddPolicy("RequireAdminAGSRole", policy => policy.RequireRole("Administrator", "AGS Personnel"));
                options.AddPolicy("RequireOverallApprover", policy => policy.RequireRole("Overall Approver"));
            });

            // Configure cookie settings for Identity
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1480);
                options.LoginPath = "/Accounts/Login";
                options.AccessDeniedPath = "/Accounts/AccessDenied";
                options.SlidingExpiration = true;
            });

            // HttpClient and background hosted service
            services.AddHttpClient();
            services.AddHostedService<CheckApprovalViaSms>();

            // Cache & Session
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
            });



            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // Configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Add CSP headers middleware
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "script-src 'unsafe-inline'; style-src 'unsafe-inline';");
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // Enforce HTTPS
            }

            // REMOVE ASP.NET VERSION HEADERS
            //app.Use(async (context, next) =>
            //{
            //    context.Response.OnStarting(() =>
            //    {
            //        context.Response.Headers.Remove("Server");
            //        context.Response.Headers.Remove("X-Powered-By");
            //        context.Response.Headers.Remove("X-AspNet-Version");
            //        context.Response.Headers.Remove("X-AspNetMvc-Version");
            //        return Task.CompletedTask;
            //    });

            //    await next();
            //});

            //// GLOBAL SECURITY HEADERS
            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            //    context.Response.Headers["X-Frame-Options"] = "DENY";
            //    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            //    context.Response.Headers["Permissions-Policy"] =
            //        "camera=(), microphone=(), geolocation=()";

            //    await next();
            //});

            // CONTENT SECURITY POLICY (CSP LEVEL 3)
            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers["Content-Security-Policy"] =
            //        "default-src 'self'; " +
            //        "script-src 'self' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://repo.aunasin.com 'unsafe-hashes'; " +
            //        "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://repo.aunasin.com; " +
            //        "img-src 'self' data: https://repo.aunasin.com; " +
            //        "font-src 'self' data: https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://repo.aunasin.com; " +
            //        "object-src 'none'; " +
            //        "frame-ancestors 'none';";


            //    await next();
            //});

            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("Content-Security-Policy",
            //        "default-src 'self'; " +
            //        "script-src 'self' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://repo.aunasin.com 'unsafe-inline'; " +
            //        "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://repo.aunasin.com; " +
            //        "img-src 'self' data: https://repo.aunasin.com; " +
            //        "font-src 'self' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://repo.aunasin.com; " +
            //        "connect-src 'self' https://repo.aunasin.com; " +
            //        "frame-ancestors 'self';");
            //    await next();
            //});

            app.UseMiddleware<SecurityHeadersMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
