using System;
using System.Collections.Generic;
using Serilog;

// TODO only one object per file
namespace GLMS
{
    public interface IStartupTask
    {
        // TODO no need for name there is typeof().Name
        string Name { get; }
        void Execute();
    }

    public class StartupList
    {
        private List<IStartupTask> startupTasks = new List<IStartupTask>();

        public StartupList()
        {
            int errorCount = 0;
            var iStartup = typeof(IStartupTask);
            
            var taskTypes = typeof(Program).Assembly.GetTypes()
                .Where(type => iStartup.IsAssignableFrom(type) && type.IsClass);
            List<IStartupTask> tasks = new List<IStartupTask>();

            foreach (var task in taskTypes)
            {
                try
                {
                    if(Activator.CreateInstance(task) is IStartupTask startupTask)
                    {
                        tasks.Add(startupTask);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex,"Error while adding startup task to queue");
                    errorCount++;
                }
                
            }

            startupTasks = tasks;
        }

        public void ExecuteStartupTasks()
        {
            Log.Information("Executing startup tasks...");
            int taskCount = 0;
            foreach(var task in startupTasks)
            {
                try
                {
                    task.Execute();
                } catch (Exception ex)
                {
                    Log.Error(ex,$"[{task.Name}] Error in execution.");
                    continue;
                }
                taskCount++;
            }

            Log.Information($"Done with {taskCount} startup task(s)");
        }
    }

    
}
