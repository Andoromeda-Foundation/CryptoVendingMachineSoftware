using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using XiaoTianQuanServer.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XiaoTianQuanServer.Authorizations;
using XiaoTianQuanServer.Extensions;
using XiaoTianQuanServer.Services;
using XiaoTianQuanServer.Services.CoinMarketCap;

namespace XiaoTianQuanServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
            });

            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews(options =>
            {
                options.RespectBrowserAcceptHeader = true; 
            }).AddNewtonsoftJson().AddXmlSerializerFormatters();
            services.AddRazorPages();

            services.ConfigureApplicationCookie(options =>
            {
                Task ReturnUnauthorizedForApiInsteadOfRedirect(RedirectContext<CookieAuthenticationOptions> ctx)
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") &&
                        ctx.Response.StatusCode == (int)HttpStatusCode.OK)
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    }
                    else
                    {
                        ctx.Response.Redirect(ctx.RedirectUri);
                    }

                    return Task.CompletedTask;
                }

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ReturnUnauthorizedForApiInsteadOfRedirect,
                    OnRedirectToAccessDenied = ReturnUnauthorizedForApiInsteadOfRedirect,
                };
            });

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.VendingMachine,
                    policy => policy.Requirements.Add(new VerifiedVendingMachineRequirement()));
            });

            // Authorization handler
            services.AddSingleton<IAuthorizationHandler, VerifiedVendingMachineHandler>();

            // Configurations
            services.Configure<Settings.LndSettings>(Configuration.GetSection("LndSettings"));
            services.Configure<Settings.ServiceBus>(Configuration.GetSection("ServiceBus"));
            services.Configure<Settings.CoinMarketCapSettings>(Configuration.GetSection("CoinMarketCapSettings"));
            services.Configure<Settings.DefaultConfiguration>(Configuration.GetSection("DefaultConfiguration"));
            var trustedIssuerB64 = Configuration.GetSection("Authorization").GetValue<string>("TrustedIssuer");
            var authorizationSettings = new Settings.AuthorizationSettings
            {
                TrustedIssuer =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        Convert.FromBase64String(trustedIssuerB64))
            };
            services.AddSingleton(authorizationSettings);

            // Lightning Network Service
            services.AddTransient<Services.LightningNetwork.LightningNetworkRequestService>();
            services.AddHostedService<Services.LightningNetwork.LightningNetworkSubscribeService>();

            // Exchange service
            services.AddSingleton<CoinMarketCapCurrencyExchangeService>();
            services.AddSingleton<ICurrencyExchangeService>(sp => sp.GetService<CoinMarketCapCurrencyExchangeService>());
            services.AddHostedService(sp => sp.GetService<CoinMarketCapCurrencyExchangeService>());

            // VendingMachineDataService
            services.AddTransient<IVendingMachineDataService, Services.Impl.VendingMachineDataService>();

            // VendingMachineControlService
            services.AddTransient<IVendingMachineControlService, Services.Impl.VendingMachineControlService>();

            // MachineConfigurationService
            services.AddSingleton<IMachineConfigurationService, Services.Impl.MachineConfigurationService>();

            // TransactionManager
            services.AddTransient<ITransactionManager, Services.Impl.TransactionManager>();

            // TransactionSettlementService
            services.AddSingleton<ITransactionSettlementService, Services.Impl.TransactionSettlementService>();
            services.AddHostedService(st => st.GetService<ITransactionSettlementService>());


            // VendingJobQueue
            services.AddSingleton<Services.Impl.AzureServiceBusVendingJobQueue>();
            services.AddSingleton<IVendingJobQueue>(sp => sp.GetService<Services.Impl.AzureServiceBusVendingJobQueue>());
            services.AddHostedService(sp => sp.GetService<Services.Impl.AzureServiceBusVendingJobQueue>());

            // Redis
            var redisConnectString = Configuration.GetConnectionString("Redis");
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
                StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectString));
            services.AddTransient<IKvCacheManager, Services.Impl.RedisKvCacheManager>();

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<Hubs.VendingMachine>("/hub/vendingmachine");
            });
        }
    }
}
