using System;
using GLMS.Networking;
using Serilog;

namespace GLMS.Startup
{
    internal class ReadUserListStartupTask : IStartupTask
    {
        public string Name { get; }

        public ReadUserListStartupTask()
        {
            Name = "UserList";
        }

        public void Execute()
        {
            string[] fileData;
            string fileName = "settings/user-ips.dat";
            if(!File.Exists(fileName))
            {
                Log.Warning($"[{Name}] User IP file does not exist.");
                return;
            }

            fileData = File.ReadAllLines(fileName);

            foreach (string line in fileData)
            {
                string[] pair = line.Split('@');
                UserList.AddUser(pair[0], pair[1]);
            }

            Log.Information($"[{Name}] Finished adding users.");
        }
    }
}
