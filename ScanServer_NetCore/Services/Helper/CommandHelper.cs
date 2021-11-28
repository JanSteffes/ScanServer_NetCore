using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Newtonsoft.Json;

namespace ScanServer_NetCore.Services.Helper
{
    /// <summary>
    /// Helper to execute commands on terminal
    /// </summary>
    public static class CommandHelper
    {
        /// <summary>
        /// Execute a command in local shell
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static async Task ExecuteCommand(string command)
        {
            var proc = new Process();
            // set os specifics
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                proc.StartInfo.Verb = "runas";
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.Arguments = "/c \" " + command + " \"";
            }
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            await proc.WaitForExitAsync();
            var errors = await proc.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(errors))
            {
               Console.WriteLine($"[ExecuteCommand] Error(s): {errors}");
                Console.WriteLine($"[ExecuteCommand] Executed command: '{command}'");
            }
            proc.StandardInput.Flush();
        }

        //private static void OnExited(object? sender, EventArgs e)
        //{
        //    Trace.WriteLine($"[OnExited] EventArgs: {JsonConvert.SerializeObject(e)}");
        //}

        //private static void OnOutputDataRecived(object sender, EventArgs e)
        //{
        //    Trace.WriteLine($"[OnOutputDataRecived] EventArgs: {JsonConvert.SerializeObject(e)}");
        //}

        //private static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    Trace.WriteLine($"[OnErrorDataReceived] EventArgs: {JsonConvert.SerializeObject(e)}");
        //}
    }
}
