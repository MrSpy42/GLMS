using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS.Commands
{
    internal class WhoAmICommand : ICommand
    {
        public WhoAmICommand()
        {
            Key = "whoami";
            helpEntry = "whoami (gets current user)";
        }
        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            Log.Information($"Current user: {SettingsManager.GetSettings().username}");
            return true;
        }
    }
}
