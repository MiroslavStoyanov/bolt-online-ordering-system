﻿namespace Bolt.Web
{
    using System.Net;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Bolt.Data.Contexts.Bolt.Implementations;
    using Bolt.Data.Contexts.Bolt.Implementations.Repositories;
    using Bolt.Services.Interfaces;
    using Bolt.Web.Filters;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Bolt.Models;
    using Bolt.Web.Services;
    using Bolt.Web.Configuration;
    using Bolt.Core.Data.Repositories;
    using Bolt.Services.Implementations;
    using Microsoft.AspNetCore.Rewrite;
    using Bolt.Data.Contexts.Bolt.Interfaces;
    using Bolt.Data.Contexts.Bolt.Interfaces.Repositories;
    using Bolt.Web.Extensions;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        string _testSecret = null;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Add the Facebook AppId and AppSecret in the userSecrets. If it doesn't work, read the settings from appsettings.json
            this._testSecret = this.Configuration[""];

            services.AddDbContext<BoltDbContext>(options =>
                options.UseSqlServer(this.Configuration.GetConnectionString("BoltDatabaseConnection")));

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<BoltDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = this.Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = this.Configuration["Authentication:Facebook:AppSecret"];
            });

            services.AddMemoryCache();

            services.AddAutoMapper();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddMvc(
            options =>
            {
                options.Filters.Add(typeof(CustomExceptionFilter));
                options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();

            }).AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/Account/Manage");
                options.Conventions.AuthorizePage("/Account/Logout");
            });

            #region Bolt.Core.Data

            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            #endregion

            #region Bolt.Data

            services.AddScoped<IBoltDbContext, BoltDbContext>();

            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IOrdersRepository, OrdersRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IProductsRepository, ProductsRepository>();

            #endregion

            #region Bolt.Services

            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<IProductsService, ProductsService>();
            services.AddScoped<IUsersService, UsersService>();

            #endregion

            services.AddScoped<ICookieCachingService, CookieCachingService>();
            services.AddSingleton<IEmailSender, EmailSender>();

            DatabaseConfig.InitializeDatabase(services.BuildServiceProvider());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDatabaseMigration();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler(
                    options =>
                    {
                        options.Run(
                            async context =>
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentType = "text/html";
                                var ex = context.Features.Get<IExceptionHandlerFeature>();
                                if (ex != null)
                                {
                                    string err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";
                                    await context.Response.WriteAsync(err).ConfigureAwait(false);
                                }
                            });
                    }
                );
            }

            RewriteOptions rewriteOptions = new RewriteOptions().AddRedirectToHttps();

            app.UseRewriter(rewriteOptions);

            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Store}/{action=All}/{id?}");
            });
        }
    }
}