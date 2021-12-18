using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanServer_NetCore.Middlewares.Extensions;
using ScanServer_NetCore.Services.Implementations;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
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
            if (string.IsNullOrEmpty(AppSettings.BasePath))
            {
                var argumentNullException = new NullReferenceException($"BasePath cannot be null or empty!");
                throw argumentNullException;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                AppSettings.BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AppSettings.BasePath);
            }
            else
            {
                if (string.IsNullOrEmpty(AppSettings.BasePathUser))
                {
                    var argumentNullException = new NullReferenceException($"BasePathUser cannot be null or empty!");
                    throw argumentNullException;
                }
                if (string.IsNullOrEmpty(AppSettings.BasePathUserPassword))
                {
                    var argumentNullException = new NullReferenceException($"BasePathUserPassword cannot be null or empty!");
                    throw argumentNullException;
                }
                var netowrkDriveNames = Enumerable.Range((int)'A', ((int)'Z' - (int)'A') + 1).Select(s => ((char) s) + ":\\").ToList();
                var drives = DriveInfo.GetDrives();
                var availableNames = netowrkDriveNames.Where(name => !drives.Any(d => d.Name == name)).ToList();
                var nd = new NetworkDrive();
                Console.WriteLine($"Will use {availableNames.First()} as driveletter.");
                nd.MapNetworkDrive(AppSettings.BasePath, availableNames.First(), AppSettings.BasePathUser, AppSettings.BasePathUserPassword);

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
                builder.AddConsole();
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
            services.AddSwaggerDocument(con =>
            {
                con.GenerateXmlObjects = true;
                con.UseControllerSummaryAsTagDescription = true;
                con.Description = $"Generated at last start from {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseRequestResponseLogging();
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


    public class NetworkDrive
    {
        public enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        }

        public enum ResourceType
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        }

        public enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        }

        public enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        }

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public ResourceScope dwScope = 0;
            public ResourceType dwType = 0;
            public ResourceDisplayType dwDisplayType = 0;
            public ResourceUsage dwUsage = 0;
            public string lpLocalName = null;
            public string lpRemoteName = null;
            public string lpComment = null;
            public string lpProvider = null;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NETRESOURCE lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        public int MapNetworkDrive(string unc, string drive, string user, string password)
        {
            var myNetResource = new NETRESOURCE
            {
                lpLocalName = drive,
                lpRemoteName = unc,
                lpProvider = null
            };
            int result = WNetAddConnection2(myNetResource, password, user, 0);
            return result;
        }
    }
}
