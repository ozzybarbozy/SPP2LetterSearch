using System;
using System.IO;

namespace SPP2LetterSearch
{
    public static class Constants
    {
        public static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SPP2LetterSearch");

        public static readonly string IndexPath = Path.Combine(AppDataPath, "Index");
        public static readonly string LogFilePath = Path.Combine(AppDataPath, "log.txt");
        public static readonly string MetadataDbPath = Path.Combine(AppDataPath, "metadata.db");

        public const string LetterFolderPattern = "SPP2-KLN-PRO-LET-";
        public const int LetterNumberLength = 4;

        public const string DefaultLetterFolder = @"\\10.10.2.32\00_dcc\01-Project Letters\KLN-PRO";
    }
}
