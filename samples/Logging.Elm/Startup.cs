using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElmSampleApp
{
    /// <summary>
    /// Simple page that displays "Hello World". Navigate to localhost/foo to see the elm logs
    /// </summary>
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddElm();
            services.ConfigureElm(options =>
            {
                options.Path = new PathString("/foo");  // defaults to "/Elm"
                options.Filter = (name, level) => level >= LogLevel.Information;
            });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory factory)
        {
            app.UseElmPage(); // Shows the logs at the specified path
            app.UseElmCapture(); // Adds the ElmLoggerProvider

            var logger = factory.CreateLogger<Startup>();
            using (logger.BeginScope("startup"))
            {
                logger.LogWarning("Starting up");
            }

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello world");
                using (logger.BeginScope("world"))
                {
                    logger.LogInformation("Hello world!");
                    logger.LogError("Mort");
                }
                // This will not get logged because the filter has been set to LogLevel.Information and above
                using (logger.BeginScope("debug"))
                {
                    logger.LogDebug("some debug stuff");
                }
            });
            logger.LogInformation("This message is not in a scope");
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
