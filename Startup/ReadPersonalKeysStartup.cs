using System;
using Serilog;

namespace GLMS.Startup
{
    class ReadPersonalKeysStartupTask : IStartupTask
    {
        public string Name { get; }

        public ReadPersonalKeysStartupTask()
        {
            Name = "Encryption";
        }

        public void Execute()
        {
            //format: array[0] is the private key and array[1] is the public key
            string[] fileData;
            string fileName = "keys/personal-keys.glks";
            if(!File.Exists(fileName))
            {
                // TODO log error
                Log.Error($"Personal key file does not exist. Generate new keys or place key file (.glks) in working directory");
                return;
            }

            fileData = File.ReadAllLines(fileName);

            if(fileData.Length != 2)
            {
                Log.Error($"Personal key file is in unexpected format. Generate new keys and try again.");
                return;
            }

            EncryptionManager.SetPrivateKey(fileData[0]);
            EncryptionManager.SetPublicKey(fileData[1]);

            Log.Information($"Successfully loaded personal keys.");
        }
    }
}
