using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ScanServer_NetCore
{
    public class Program
    {
        private const int httpPort = 6933;
        private const int httpsPort = 44309;

        public static void Main(string[] args)
        {
            CreateBroadCastRecievers();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseIISIntegration()
                    .UseUrls($"https://0.0.0.0:{httpsPort}", $"http://0.0.0.0:{httpPort}")
                    .UseStartup<Startup>();
                    
                });

        public static void CreateBroadCastRecievers()
        {
            StartReceiveThread(httpPort);
            StartReceiveThread(httpsPort);
        }

        private static void StartReceiveThread(int port)
        {
            var httpsListener = new UdpClient(port);
            var httpsEndpoint = new IPEndPoint(IPAddress.Any, port);
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Debug.WriteLine($"==> [Port {port}] Waiting to receive bytes..");
                        var bytes = httpsListener.Receive(ref httpsEndpoint);
                        Debug.WriteLine($"==> [Port {port}] Got {bytes.Length} bytes, answering...");
                        _ = httpsListener.SendAsync(bytes, bytes.Length);
                        Debug.WriteLine($"==> [Port {port}] Did send same bytes back");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"==> [Port {port}] Exception while recieving/answering broadcast: {e.Message}");
                    }
                }
            }).Start();
        }

    }
}
