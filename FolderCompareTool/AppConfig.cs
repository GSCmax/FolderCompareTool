namespace FolderCompareTool
{
    class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}Config.json";

        public int CompareMode { get; set; } = 0;

        public int FileNameTolerance { get; set; } = 3;

        public int FileNameToleranceMax { get; set; } = 5;

        public int HashAlgorithm { get; set; } = 0;
    }
}
