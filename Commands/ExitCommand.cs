using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLMS.Commands
{
    public class ExitCommand : ICommand
    {
        public ExitCommand()
        {
            Key = "exit";
            helpEntry = "exit (exits the program)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            SettingsManager.GetRuntimeSettings().isExiting = true;
            return true;
        }
    }
}
