using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Directory = System.IO.Directory;
using DirectoryReader = Lucene.Net.Index.DirectoryReader;
using FSDirectory = Lucene.Net.Store.FSDirectory;
using SPP2LetterSearch.Models;

namespace SPP2LetterSearch.Services
{
    public class SearchService
    {
        private readonly LoggingService _logger;
        private readonly string _indexPath;

        public SearchService(LoggingService logger)
        {
            _logger = logger;
            _indexPath = Constants.IndexPath;
        }

        public List<SearchResultItem> Search(string queryString, int maxResults = 1000)
        {
            var results = new List<SearchResultItem>();

            try
            {
                if (string.IsNullOrWhiteSpace(queryString))
                {
                    return results;
                }

                if (!Directory.Exists(_indexPath))
                {
                    _logger.LogError("Index directory does not exist");
                    return results;
                }

                var analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
                var dir = FSDirectory.Open(_indexPath);

                using (var reader = DirectoryReader.Open(dir))
                {
                    var searcher = new IndexSearcher(reader);
                    var parser = new MultiFieldQueryParser(
                        Lucene.Net.Util.LuceneVersion.LUCENE_48,
                        new[] { "content", "fileName", "folderName" },
                        analyzer)
                    {
                        DefaultOperator = Lucene.Net.QueryParsers.Classic.QueryParser.OR_OPERATOR
                    };

                    try
                    {
                        var query = parser.Parse(queryString);
                        var hits = searcher.Search(query, maxResults);

                        foreach (var scoreDoc in hits.ScoreDocs)
                        {
                            var doc = reader.Document(scoreDoc.Doc);
                            var content = doc.Get("content") ?? "";
                            var snippet = ExtractSnippet(content, queryString, 150);

                            results.Add(new SearchResultItem
                            {
                                LetterNo = doc.Get("letterNo"),
                                FileName = doc.Get("fileName"),
                                FullPath = doc.Get("fullPath"),
                                FolderName = doc.Get("folderName"),
                                Score = scoreDoc.Score,
                                Snippet = snippet
                            });
                        }

                        // Sort by letterNo (numeric) and then by score
                        results = results.OrderBy(x => int.TryParse(x.LetterNo, out var num) ? num : 9999)
                                        .ThenByDescending(x => x.Score)
                                        .ToList();

                        _logger.Log($"Search completed: '{queryString}' - {results.Count} results");
                    }
                    catch (ParseException ex)
                    {
                        _logger.LogError($"Failed to parse query: {queryString}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Search error", ex);
            }

            return results;
        }

        private string ExtractSnippet(string content, string queryString, int snippetLength)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return "";

                // Extract first meaningful term from query
                var terms = ExtractTermsFromQuery(queryString);
                if (terms.Count == 0)
                    return content.Substring(0, Math.Min(snippetLength, content.Length));

                // Find first occurrence of any term
                foreach (var term in terms)
                {
                    var idx = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        var start = Math.Max(0, idx - snippetLength / 2);
                        var length = Math.Min(snippetLength, content.Length - start);
                        var snippet = content.Substring(start, length).Trim();

                        if (start > 0)
                            snippet = "..." + snippet;
                        if (start + length < content.Length)
                            snippet = snippet + "...";

                        return snippet;
                    }
                }

                // Fallback: return first part of content
                return content.Substring(0, Math.Min(snippetLength, content.Length)).Trim() + "...";
            }
            catch
            {
                return content.Substring(0, Math.Min(100, content.Length)).Trim() + "...";
            }
        }

        private List<string> ExtractTermsFromQuery(string queryString)
        {
            var terms = new List<string>();

            // Remove quotes and common operators
            var cleaned = queryString.Replace("\"", "").Replace(" AND ", " ").Replace(" OR ", " ");
            var parts = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (!part.StartsWith("-") && part.Length > 2)
                {
                    terms.Add(part);
                }
            }

            return terms.Take(5).ToList();
        }
    }
}
