using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kentico.Kontent.Boilerplate
{
    public class Program
    {
        // This constant must match <UserSecretsId> value in Kentico.Kontent.Boilerplate.csproj
        public const string USER_SECRETS_ID = "Kentico.Kontent.Boilerplate";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
               .Build() 
               .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    // delete all default configuration providers
                    config.Sources.Clear();
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets(USER_SECRETS_ID)
                    .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddDebug();
                    }
                })
              .UseStartup<Startup>();
    }
}
