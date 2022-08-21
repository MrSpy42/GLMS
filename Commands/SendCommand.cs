using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLMS.Networking;
using Serilog;

namespace GLMS.Commands
{
    public class SendCommand : ICommand
    {
        public SendCommand()
        {
            Key = "send";
            helpEntry = "send [user] [data]";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            if(args.Length < 2)
            {
                Log.Warning("Invalid amount of arguments");
                return false;
            }
            string text;
            if(args.Length > 2)
            {
                string[] userInput = args.Skip(1).ToArray();
                text = string.Join(' ', userInput);
            } else
            {
                text = args[1];
            }

            NetworkHandler.SendMessage(args[0], text);
            return true;
        }
    }
}
