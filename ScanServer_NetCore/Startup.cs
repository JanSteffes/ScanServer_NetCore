using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanServer_NetCore.Services.Implementations;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScanServer_NetCore
{
    public class Startup
    {
        private readonly AppSettings AppSettings;
        public IConfiguration Configuration { get; }

        private ILogger? StartupLogger { get; set; }

        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                builder.AddJsonFile("appsettings.raspberrypi.json", true, true);
            }
            else
            {
                builder.AddJsonFile("appsettings.Development.json", true, true);
            }

            Configuration = builder.Build();
            AppSettings = Configuration.Get<AppSettings>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (string.IsNullOrEmpty(AppSettings.BasePath))
                {
                    var argumentNullException = new NullReferenceException($"BasePath cannot be null or empty!");
                    throw argumentNullException;
                }
                AppSettings.BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AppSettings.BasePath);
            }
            Console.WriteLine($"==> BasePath = {AppSettings.BasePath}");
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddEventSourceLogger();
            });
            StartupLogger = loggerFactory.CreateLogger<Startup>();
            StartupLogger.LogInformation($"Using basePath {AppSettings.BasePath}");
            if (string.IsNullOrEmpty(AppSettings.BasePath))
            {
                throw new NullReferenceException($"BasePath cannot be null or empty!");
            }
            services.AddSingleton(typeof(IFileService), new FileService(loggerFactory, AppSettings.BasePath));
            services.AddSingleton(typeof(IScanService), new ScanService(loggerFactory, AppSettings.BasePath));
            services.AddControllers();
            // Register the Swagger services
            services.AddSwaggerDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            //app.UseHttpsRedirection();

            app.UseRouting();
            

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // load swagger file to local file
            _ = LoadSwaggerFile();
        }

        private static async Task LoadSwaggerFile()
        {
            var client = new HttpClient();
            using var stream = await client.GetStreamAsync("https://localhost:44309/swagger/v1/swagger.json");
            if (File.Exists("swagger.json"))
            {
                File.Delete("swagger.json");
            }
            using var localFileStream = File.OpenWrite("swagger.json");
            await stream.CopyToAsync(localFileStream);
        }
    }
}
