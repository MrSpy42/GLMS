using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLMS.Networking;
using Serilog;

namespace GLMS.Commands
{
    internal class HandshakeCommand : ICommand
    {
        public HandshakeCommand()
        {
            Key = "handshake";
            helpEntry = "handshake [ip] (manually exchange public keys for communication)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            if(args.Length == 0)
            {
                Log.Warning("No arguments were given");
                return true;
            }

            NetworkHandler.SendHandshake(args[0],true);
            return true;
        }
    }
}
