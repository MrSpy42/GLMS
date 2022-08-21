using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using GLMS.Networking;

namespace GLMS.Commands
{
    public class StartSessionCommand : ICommand
    {
        public StartSessionCommand()
        {
            Key = "start-session";
            helpEntry = "start-session [user] (generates session key for user)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            if(args.Length == 0)
            {
                Log.Warning("No arguments were given.");
                return true;
            }

            NetworkHandler.SendSessionStart(args[0]);

            return true;
        }

    }
}
