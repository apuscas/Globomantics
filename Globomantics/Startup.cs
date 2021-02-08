using Exchange;
using Exchange.Interfaces;
using Globomantics.AuthMiddleware;
using Globomantics.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Globomantics
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Authentication
            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-5.0
            services.AddAuthentication()
               .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            //// Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("BasicAuthentication",
                    new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
            });

            #region Dependency container
            //services.AddSingleton<IConferenceService, ConferenceApiService>();

            services.AddSingleton<IConferenceService, ConferenceMemoryService>();
            services.AddSingleton<IProposalService, ProposalMemoryService>();

            services.AddSingleton<IExchangeServiceWrapper, ExchangeServiceWrapper>();
            services.AddSingleton<IExchangeServiceClient, ExchangeServiceClient>();

            // When you run your program with dotnet run, the hosted service kicks in and it places the MailboxPolling in motion with the Timer.
            services.AddHostedService<MailboxPolling>();
            #endregion

            services.Configure<GlobomanticsOptions>(configuration.GetSection("Globomantics"));

            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            //Must be placed before UseEndpoints because you want auth to be done before the request reaches MVC or our endpoints
            app.UseAuthentication();
           
            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthorization(); //must be placed between UseRouting and UseEndpoints
            
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Conference}/{action=Index}/{id?}");
            });
        }
    }
}
