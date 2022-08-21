/* Main class for GLMS (Great Little Messaging System) 
 *
 * 
 *
 */

//AuthServer needed to resolve username 

using System;
using System.Diagnostics;
using GLMS.Networking;
using Serilog;

namespace GLMS
{
    class Program
    {
        static void Main(String[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.File("logs/glms-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Starting GLMS...");
            Stopwatch sw = Stopwatch.StartNew();

            if (!Directory.Exists("keys"))
            {
                try
                {
                    var di = Directory.CreateDirectory("keys");
                } catch (Exception ex)
                {
                    Log.Error(ex, "Unable to create directory keys/");
                }
            }
            if(!Directory.Exists("settings"))
            {
                try
                {
                    var dd = Directory.CreateDirectory("settings");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unable to create directory settings/");
                }
            }

            StartupList startup = new StartupList();
            startup.ExecuteStartupTasks();

            CommandList commandList = new CommandList();

            PortForwarder portForwarder = new PortForwarder();
            NetworkHandler.StartFirestarter();

            sw.Stop();

            Log.Information($"Ready, time taken: {sw.Elapsed.TotalMilliseconds} ms");

            while(!SettingsManager.GetRuntimeSettings().isExiting)
            {
                Console.Write(">");
                string? input = Console.ReadLine();
                commandList.ExecuteCommand(input);
            }

            Log.Information("Exiting...");
            SettingsManager.WriteToFile();
            EncryptionManager.WritePublicKeysToFile();
            UserList.SaveToFile();
            NetworkHandler.StopFirestarter();
            Log.CloseAndFlush();
        }
    }
}
