using Newtonsoft.Json;
using System.IO;

namespace FolderCompareTool
{
    static class GlobalDataHelper
    {
        public static AppConfig? appConfig;

        private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB" };

        public static string FormatFileSize(long bytes, bool useDecimal = false)
        {
            double size = bytes;
            int unitIndex = 0;
            int divisor = useDecimal ? 1000 : 1024;
            while (size >= divisor && unitIndex < Units.Length - 1)
            {
                size /= divisor;
                unitIndex++;
            }

            return $"{size:0.##} {Units[unitIndex]}";
        }

        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
                try
                {
                    var json = File.ReadAllText(AppConfig.SavePath);
                    appConfig = (string.IsNullOrEmpty(json) ? new AppConfig() : JsonConvert.DeserializeObject<AppConfig>(json)) ?? new AppConfig();
                }
                catch
                {
                    appConfig = new AppConfig();
                }
            else
                appConfig = new AppConfig();
        }

        public static void Save()
        {
            var json1 = JsonConvert.SerializeObject(appConfig, Formatting.Indented);
            File.WriteAllText(AppConfig.SavePath, json1);
        }
    }
}
