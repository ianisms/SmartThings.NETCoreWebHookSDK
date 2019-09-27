using ASPNetCoreWebAPI.WebhookHandlers;
using ianisms.SmartThings.NETCoreWebHookSDK.Crypto;
using ianisms.SmartThings.NETCoreWebHookSDK.Extensions;
using ianisms.SmartThings.NETCoreWebHookSDK.WebhookHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASPNetCoreWebAPI
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
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddLogging()
                .Configure<CryptoUtilsConfig>(Configuration.GetSection(nameof(CryptoUtilsConfig)))
                .AddSingleton<IConfigWebhookHandler, MyConfigWebhookHandler>()
                .AddSingleton<IInstallWebhookHandler, MyInstallWebhookHandler>()
                .AddSingleton<IUpdateWebhookHandler, MyUpdateWebhookHandler>()
                .AddSingleton<IUninstallWebhookHandler, MyUninstallWebhookHandler>()
                .AddSingleton<IEventWebhookHandler, MyEventWebhookHandler>()
                .AddWebhookHandlers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
