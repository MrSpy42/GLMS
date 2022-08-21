using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS.Commands
{
    public class GenerateKeysCommand : ICommand
    {
        public GenerateKeysCommand()
        {
            Key = "generate-keys";
            helpEntry = "generate-keys (generates your personal keys)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            EncryptionManager.GeneratePersonalKeys();
            string contents = EncryptionManager.GetKeyStringRepresentation(true) + '\n' + EncryptionManager.GetKeyStringRepresentation(false);

            try
            {
                File.WriteAllText("keys/personal-keys.glks", contents);
            } catch(Exception ex)
            {
                Log.Error(ex,"Error writing to file, keys were set but were not saved, please try again.");
                return false;
            }

            Log.Information("Keys were successfully generated.");
            return true;
        }
    }
}
