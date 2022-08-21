using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GLMS
{
    public class Settings
    {
        public string username { get; set; }

        public Settings()
        {
            username = "glms-user";
        }
    }
    public class RuntimeSettings
    {
        public string helpMessage { get; set; }
        public bool isAuthenticated { get; set; }
        public bool isExiting { get; set; }
        public RuntimeSettings()
        {
            helpMessage = "";
            isAuthenticated = false;
            isExiting = false;
        }
    }

    public static class SettingsManager
    {
        private static RuntimeSettings runtimeSettings = new RuntimeSettings();
        private static Settings settings = new Settings();

        public static RuntimeSettings GetRuntimeSettings()
        {
            return runtimeSettings;
        }

        public static Settings GetSettings()
        {
            return settings;
        }

        public static void WriteToFile()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            var sw = new StringWriter();
            serializer.Serialize(sw, SettingsManager.GetSettings());
            try
            {
                File.WriteAllText("settings/glms-settings.dat", sw.ToString());
            }
            catch (Exception ex2)
            {
                Log.Error(ex2,"Unable to save settings to file.");
                return;
            }

            Log.Information($"Saved settings to file.");
        }
    }
}
