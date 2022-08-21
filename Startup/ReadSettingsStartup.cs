using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace GLMS.Startup
{
    class ReadSettingsStartupTask : IStartupTask
    {
        public String Name { get; }

        public ReadSettingsStartupTask()
        {
            Name = "Settings";
        }

        public void Execute()
        {
            string fileContents;
            string fileName = "settings/glms-settings.dat";
            if(!File.Exists(fileName))
            {
                GenerateSettingsFile();
            }

            fileContents = File.ReadAllText(fileName);

            var sl = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            
            using (StringReader reader = new StringReader(fileContents))
            {
                var fileData = sl.Deserialize(reader);
                if(fileData is Settings ss)
                {
                    SettingsManager.GetSettings().username = ss.username;
                }
            }
            Log.Information($"Successfully parsed settings.");
            

        }

        private void GenerateSettingsFile()
        {
            Log.Information($"Creating new settings file...");
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            var sw = new StringWriter();
            serializer.Serialize(sw, new Settings());

            File.WriteAllText("settings/glms-settings.dat", sw.ToString());
        }
    }
}
