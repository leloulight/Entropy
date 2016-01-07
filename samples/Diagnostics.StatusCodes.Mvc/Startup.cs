using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Diagnostics.StatusCodes.Mvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Register how to generate response bodies for 400-599 status codes.
            // This example ends up using the MVC ErrorsController.
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute("ActionAsMethod", "{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        public static void Main(string[] args)
        {
            var application = new WebApplicationBuilder()
                .UseConfiguration(WebApplicationConfiguration.GetDefault(args))
                .UseStartup<Startup>()
                .Build();

            application.Run();
        }
    }
}
