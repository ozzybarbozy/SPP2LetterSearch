using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using SPP2LetterSearch.Models;

namespace SPP2LetterSearch.Services
{
    public class MetadataStore
    {
        private readonly string _dbPath;
        private readonly LoggingService _logger;

        public MetadataStore(LoggingService logger)
        {
            _logger = logger;
            _dbPath = Constants.MetadataDbPath;
            EnsureDb();
        }

        private void EnsureDb()
        {
            try
            {
                var dir = Path.GetDirectoryName(_dbPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create metadata directory", ex);
            }
        }

        public void SaveMetadata(LetterDocumentMetadata metadata)
        {
            try
            {
                using (var db = new LiteDatabase($"Filename={_dbPath}"))
                {
                    var col = db.GetCollection<LetterDocumentMetadata>("metadata");
                    col.Upsert(metadata);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save metadata", ex);
            }
        }

        public void SaveMetadataList(List<LetterDocumentMetadata> metadataList)
        {
            try
            {
                using (var db = new LiteDatabase($"Filename={_dbPath}"))
                {
                    var col = db.GetCollection<LetterDocumentMetadata>("metadata");
                    foreach (var metadata in metadataList)
                    {
                        col.Upsert(metadata);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save metadata list", ex);
            }
        }

        public LetterDocumentMetadata GetMetadata(string id)
        {
            try
            {
                using (var db = new LiteDatabase($"Filename={_dbPath}"))
                {
                    var col = db.GetCollection<LetterDocumentMetadata>("metadata");
                    return col.FindById(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get metadata", ex);
                return null;
            }
        }

        public List<LetterDocumentMetadata> GetAllMetadata()
        {
            try
            {
                using (var db = new LiteDatabase($"Filename={_dbPath}"))
                {
                    var col = db.GetCollection<LetterDocumentMetadata>("metadata");
                    return col.FindAll().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get all metadata", ex);
                return new List<LetterDocumentMetadata>();
            }
        }

        public void DeleteMetadata(string id)
        {
            try
            {
                using (var db = new LiteDatabase($"Filename={_dbPath}"))
                {
                    var col = db.GetCollection<LetterDocumentMetadata>("metadata");
                    col.Delete(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete metadata", ex);
            }
        }

        public void ClearAllMetadata()
        {
            try
            {
                using (var db = new LiteDatabase($"Filename={_dbPath}"))
                {
                    var col = db.GetCollection<LetterDocumentMetadata>("metadata");
                    col.DeleteMany(x => true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to clear metadata", ex);
            }
        }

        public bool HasChanged(LetterDocumentMetadata metadata)
        {
            var existing = GetMetadata(metadata.Id);
            if (existing == null)
                return true;

            return existing.LastWriteTimeUtc != metadata.LastWriteTimeUtc ||
                   existing.FileSize != metadata.FileSize;
        }
    }
}
