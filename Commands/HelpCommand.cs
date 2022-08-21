using System;
using Serilog;

namespace GLMS.Commands
{
    public class HelpCommand : ICommand
    {
        public HelpCommand()
        {
            Key = "help";
            helpEntry = "help (displays this message)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            Log.Information(SettingsManager.GetRuntimeSettings().helpMessage);
            return true;
        }
    }
}
