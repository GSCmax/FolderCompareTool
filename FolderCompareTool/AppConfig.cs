using System.Runtime.Serialization;

namespace FolderCompareTool
{
    class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}Config.json";

        public int CompareMode { get; set; } = 0;

        public int HashAlgorithm { get; set; } = 0;

        public int FileNameTolerance { get; set; } = 0;

        public int FileNameToleranceMax { get; set; } = 5;

        public int FileSizeTolerance { get; set; } = 0;

        public int FileSizeToleranceMax { get; set; } = 1024;

        public bool IsTopmost { get; set; } = true;

        public int RecentPathCount { get; set; } = 10;

        public NonRepeatObservableCollection<string> RecentPaths { get; set; } = [];

        [OnSerializing]
        internal void OnSerializing(StreamingContext context) => RecentPaths = new NonRepeatObservableCollection<string>(RecentPaths.Take(RecentPathCount));
    }
}
