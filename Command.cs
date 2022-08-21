using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS
{
    public interface ICommand
    {
        string Key { get; }

        string helpEntry { get; }

        bool Execute(String[] args);
    }

    public class CommandList
    { 
        public CommandList()
        {
            int errorCount = 0;
            int commandCount = 0;
            var iCommand = typeof(ICommand);
            var commandTypes = typeof(Program).Assembly.GetTypes()
                .Where(type => iCommand.IsAssignableFrom(type) && type.IsClass);
            List<ICommand> commands = new List<ICommand>();

            foreach (var command in commandTypes)
            {
                try
                {
                    if (Activator.CreateInstance(command) is ICommand com)
                    {
                        commands.Add(com);
                    }
                    
                }
                catch (Exception ex)
                {
                    Log.Error(ex,$"Error while adding iCommand to list");
                    errorCount++;
                }
                
            }

            foreach(var command in commands)
            {
                try
                {
                    Commands.Add(command.Key, command);
                    SettingsManager.GetRuntimeSettings().helpMessage += command.helpEntry + '\n';
                    commandCount++;
                } catch(Exception ex)
                {
                    Log.Error($"Error while adding command to dictionary",ex);
                    errorCount++;
                }
            }

            Log.Information($"Indexed {commandCount} commands with {errorCount} error(s)");
        }

        private Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>();

        public void ExecuteCommand(string? fullInput)
        {
            Log.Verbose($"Executing {fullInput} input.");
            if(fullInput == null)
            {
                return;
            }

            string[] input = fullInput.Split(' ');

            string[] arguments = { };
            if(input.Length > 1)
            {
                arguments = new string[input.Length - 1];
                for (int i = 0; i < input.Length - 1; i++)
                {
                    arguments[i] = input[i + 1];
                }
            }

            ICommand? command;

            if (Commands.TryGetValue(input[0], out command) == false)
            {
                Log.Debug($"Failed to fetch {fullInput} command");
                return;
            } 

            bool status = command.Execute(arguments);
            if(!status)
            {
                Log.Error($"Something went wrong while executing the {fullInput} command");
            }
            
        }

    }

}
