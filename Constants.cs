using System;
using System.IO;

namespace SPP2LetterSearch
{
    public static class Constants
    {
        // Keep the original single pattern for any legacy code that still uses it
        public const string LetterFolderPattern = "SPP2-KLN-PRO-LET-";

        // New: allow multiple patterns
        public static readonly string[] LetterFolderPatterns =
        {
            "SPP2-KLN-PRO-LET-",
            "SPP2-PRO-KLN-LET-"
        };

        public const int LetterNumberLength = 4;

        public const string DefaultLetterFolder = @"\\10.10.8.32\00_dcc\01-Project Letters\KLN-PRO";

        public static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SPP2LetterSearch");

        public static readonly string IndexPath = Path.Combine(AppDataPath, "Index");
        public static readonly string LogFilePath = Path.Combine(AppDataPath, "log.txt");
        public static readonly string MetadataDbPath = Path.Combine(AppDataPath, "metadata.db");
    }
}
