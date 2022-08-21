using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS.Commands
{
    internal class AboutCommand : ICommand
    {
        public AboutCommand()
        {
            Key = "about";
            helpEntry = "about (about this project)";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            Log.Information("[About]GLMS (Great Lil Messaging System) was made for educational purposes by MrSpy42 (https://github.com/MrSpy42). It's just another communication system that uses RSA encryption to send users encrypted messages through UDP.");
            return true;
        }
    }
}
