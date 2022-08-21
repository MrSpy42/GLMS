using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS.Commands
{
    internal class PublicKeysCommand : ICommand
    {
        public PublicKeysCommand()
        {
            Key = "pubkey";
            helpEntry = "pubkey [get/clear] [user] (gets public keys or clears a specific or every key)";
        }

        public string Key { get; }
        public string helpEntry { get; set; }

        public bool Execute(string[] args)
        {
            if(args.Length == 0)
            {
                Log.Warning("No arguments were given.");
                return false;
            }

            if(args[0] == "get")
            {
                List<string> data = EncryptionManager.GetAllPublicKeys();
                foreach(string s in data)
                {
                    Log.Information(s);
                }
            }

            if(args[0] == "clear")
            {
                if(args.Length > 1)
                {
                    EncryptionManager.RemovePublicKey(args[1]);
                } else
                {
                    EncryptionManager.ResetPublicKeys();
                }
            }
            
            return true;
        }

    }
}
