using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SPP2LetterSearch.Models
{
    public class LetterDocumentMetadata
    {
        public string Id { get; set; }
        public string LetterNo { get; set; }
        public string FolderName { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public long FileSize { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }

        public LetterDocumentMetadata() { }

        public LetterDocumentMetadata(string folderName, string fileName, string fullPath)
        {
            FolderName = folderName;
            FileName = fileName;
            FullPath = fullPath;
            Id = GenerateId(fullPath);
            ExtractLetterNo();
            UpdateFileInfo();
        }

        private void UpdateFileInfo()
        {
            if (File.Exists(FullPath))
            {
                var fileInfo = new FileInfo(FullPath);
                FileSize = fileInfo.Length;
                LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            }
        }

        private string GenerateId(string path)
        {
            var bytes = Encoding.UTF8.GetBytes(path.ToLowerInvariant());
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        private void ExtractLetterNo()
        {
            try
            {
                var idx = FolderName.IndexOf(Constants.LetterFolderPattern, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var startIdx = idx + Constants.LetterFolderPattern.Length;
                    if (startIdx + Constants.LetterNumberLength <= FolderName.Length)
                    {
                        LetterNo = FolderName.Substring(startIdx, Constants.LetterNumberLength);
                        return;
                    }
                }
                LetterNo = "0000";
            }
            catch
            {
                LetterNo = "0000";
            }
        }
    }
}
