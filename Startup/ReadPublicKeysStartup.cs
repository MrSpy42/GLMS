using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS.Startup
{
    class ReadPublicKeysStartupTask : IStartupTask
    {
        public string Name { get; }

        public ReadPublicKeysStartupTask()
        {
            Name = "Encryption";
        }

        public void Execute()
        {
            string[] fileData;
            string fileName = "keys/public-keys.glks";

            if(!File.Exists(fileName))
            {
                Log.Warning($"Public key file does not exist.");
                return;
            }

            fileData = File.ReadAllLines(fileName);

            foreach(string line in fileData)
            {
                string[] pair = line.Split('@');
                EncryptionManager.AddPublicKey(pair[0], EncryptionManager.GetKeyFromString(pair[1]));
            }

            Log.Information($"Finished adding public keys");
        }
    }
}
