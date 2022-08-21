using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS.Commands
{
    public class SetUserCommand : ICommand
    {
        public SetUserCommand()
        {
            Key = "set-user";
            helpEntry = "set-user [username] (sets your username)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            if(args.Length != 1)
            {
                Log.Warning("Invalid arguments");
                return false;
            }

            SettingsManager.GetSettings().username = args[0];
            Log.Information($"Changed user to {args[0]}");
            return true;
        }
    }
}
