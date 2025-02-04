namespace FolderCompareTool
{
    class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}Config.json";

        public int CompareMode { get; set; } = 0;

        public int FileNameTolerance { get; set; } = 3;
    }
}
