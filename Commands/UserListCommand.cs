using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLMS.Networking;
using Serilog;

namespace GLMS.Commands
{
    internal class UserListCommand : ICommand
    {
        public UserListCommand()
        {
            Key = "ulist";
            helpEntry = "ulist [get/clear] [user]";
        }

        public string Key { get; set; }
        public string helpEntry { get; }

        public bool Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Log.Warning("[UserList] No arguments were given.");
                return false;
            }

            if (args[0] == "get")
            {
                List<string> data = UserList.GetUserListAsList();
                foreach (string s in data)
                {
                    Log.Information($"*{s}");
                }
            }

            if (args[0] == "clear")
            {
                if (args.Length > 1)
                {
                    UserList.RemoveUser(args[1]);
                }
                else
                {
                    UserList.ResetUserList();
                }
            }

            return true;
        }
    }
}
