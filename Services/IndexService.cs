using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = System.IO.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using DirectoryReader = Lucene.Net.Index.DirectoryReader;
using SPP2LetterSearch.Models;

namespace SPP2LetterSearch.Services
{
    public class IndexService
    {
        private readonly LoggingService _logger;
        private readonly PdfTextExtractor _pdfExtractor;
        private readonly MetadataStore _metadataStore;
        private readonly string _indexPath;

        public IndexService(LoggingService logger, PdfTextExtractor pdfExtractor, MetadataStore metadataStore)
        {
            _logger = logger;
            _pdfExtractor = pdfExtractor;
            _metadataStore = metadataStore;
            _indexPath = Constants.IndexPath;
            EnsureIndexDirectory();
        }

        private void EnsureIndexDirectory()
        {
            try
            {
                if (!Directory.Exists(_indexPath))
                {
                    Directory.CreateDirectory(_indexPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create index directory", ex);
            }
        }

        public async Task<int> BuildIncrementalIndexAsync(
            string rootFolder,
            IProgress<(int total, int indexed, string currentFolder)> progress,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() => BuildIncrementalIndex(rootFolder, progress, cancellationToken), cancellationToken);
        }

        private int BuildIncrementalIndex(
            string rootFolder,
            IProgress<(int total, int indexed, string currentFolder)> progress,
            CancellationToken cancellationToken)
        {
            try
            {
                var letterFolders = Directory.GetDirectories(rootFolder)
                    .Where(d => Path.GetFileName(d).StartsWith(Constants.LetterFolderPattern, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x)
                    .ToList();

                if (letterFolders.Count == 0)
                {
                    _logger.Log("No letter folders found in root folder");
                    return 0;
                }

                var analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
                var config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
                config.OpenMode = OpenMode.CREATE_OR_APPEND;

                using (var dir = FSDirectory.Open(_indexPath))
                using (var writer = new IndexWriter(dir, config))
                {
                    var existingMetadata = _metadataStore.GetAllMetadata().ToDictionary(x => x.Id);
                    var processedIds = new HashSet<string>();
                    int indexedCount = 0;

                    for (int i = 0; i < letterFolders.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var folder = letterFolders[i];
                        var folderName = Path.GetFileName(folder);

                        progress?.Report((letterFolders.Count, indexedCount, folderName));

                        var pdfFiles = Directory.GetFiles(folder, "*.pdf");
                        if (pdfFiles.Length == 0)
                        {
                            _logger.Log($"No PDF files found in {folderName}");
                            continue;
                        }

                        foreach (var pdfPath in pdfFiles)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var fileName = Path.GetFileName(pdfPath);

                            var metadata = new LetterDocumentMetadata(folderName, fileName, pdfPath);
                            processedIds.Add(metadata.Id);

                            bool needsReindexing = !existingMetadata.ContainsKey(metadata.Id) ||
                                                 _metadataStore.HasChanged(metadata);

                            if (needsReindexing)
                            {
                                _logger.Log($"Indexing: {folderName}/{fileName}");

                                var content = _pdfExtractor.ExtractText(pdfPath);
                                if (string.IsNullOrWhiteSpace(content))
                                {
                                    _logger.Log($"Warning: No text extracted from {pdfPath}");
                                    continue;
                                }

                                var doc = CreateDocument(metadata, content);
                                writer.UpdateDocument(new Term("id", metadata.Id), doc);
                                indexedCount++;

                                _metadataStore.SaveMetadata(metadata);
                            }
                        }
                    }

                    // Remove documents for deleted files
                    var deletedIds = existingMetadata.Keys.Except(processedIds).ToList();
                    foreach (var id in deletedIds)
                    {
                        writer.DeleteDocuments(new Term("id", id));
                        _metadataStore.DeleteMetadata(id);
                        _logger.Log($"Removed deleted document: {id}");
                    }

                    writer.Commit();
                    _logger.Log($"Index build complete. Indexed {indexedCount} documents");
                    return indexedCount;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Indexing cancelled by user");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during incremental indexing", ex);
                throw;
            }
        }

        public async Task RebuildIndexAsync(
            string rootFolder,
            IProgress<(int total, int indexed, string currentFolder)> progress,
            CancellationToken cancellationToken)
        {
            await Task.Run(() => RebuildIndex(rootFolder, progress, cancellationToken), cancellationToken);
        }

        private void RebuildIndex(
            string rootFolder,
            IProgress<(int total, int indexed, string currentFolder)> progress,
            CancellationToken cancellationToken)
        {
            try
            {
                // Clear index
                if (Directory.Exists(_indexPath))
                {
                    Directory.Delete(_indexPath, true);
                }
                Directory.CreateDirectory(_indexPath);

                _metadataStore.ClearAllMetadata();

                // Rebuild from scratch
                BuildIncrementalIndex(rootFolder, progress, cancellationToken);
                _logger.Log("Index rebuild complete");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during index rebuild", ex);
                throw;
            }
        }

        private Document CreateDocument(LetterDocumentMetadata metadata, string content)
        {
            var doc = new Document();
            doc.Add(new StringField("id", metadata.Id, Field.Store.YES));
            doc.Add(new StringField("letterNo", metadata.LetterNo, Field.Store.YES));
            doc.Add(new StringField("folderName", metadata.FolderName, Field.Store.YES));
            doc.Add(new StringField("fileName", metadata.FileName, Field.Store.YES));
            doc.Add(new StringField("fullPath", metadata.FullPath, Field.Store.YES));
            doc.Add(new TextField("content", content, Field.Store.YES));
            doc.Add(new Int64Field("lastWriteTimeUtc", metadata.LastWriteTimeUtc.Ticks, Field.Store.YES));
            return doc;
        }

        public bool IndexExists()
        {
            try
            {
                var dir = FSDirectory.Open(_indexPath);
                return DirectoryReader.IndexExists(dir);
            }
            catch
            {
                return false;
            }
        }

        public void ClearIndex()
        {
            try
            {
                if (Directory.Exists(_indexPath))
                {
                    Directory.Delete(_indexPath, true);
                }
                Directory.CreateDirectory(_indexPath);
                _metadataStore.ClearAllMetadata();
                _logger.Log("Index cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error clearing index", ex);
            }
        }
    }
}
