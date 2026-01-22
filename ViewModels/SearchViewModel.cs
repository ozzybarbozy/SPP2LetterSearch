using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SPP2LetterSearch.Commands;
using SPP2LetterSearch.Models;
using SPP2LetterSearch.Services;

namespace SPP2LetterSearch.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        private string _selectedFolder;
        private string _searchQuery;
        private string _statusMessage;
        private bool _isIndexing;
        private bool _isSearching;
        private int _progressValue;
        private int _progressMax;
        private string _progressText;
        private double _indexProgress;
        private bool _openOnDoubleClick;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly IndexService _indexService;
        private readonly SearchService _searchService;
        private readonly LoggingService _logger;

        public ObservableCollection<SearchResultItem> SearchResults { get; }

        public string SelectedFolder
        {
            get => _selectedFolder;
            set { if (_selectedFolder != value) { _selectedFolder = value; OnPropertyChanged(nameof(SelectedFolder)); } }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set { if (_searchQuery != value) { _searchQuery = value; OnPropertyChanged(nameof(SearchQuery)); } }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { if (_statusMessage != value) { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); } }
        }

        public bool IsIndexing
        {
            get => _isIndexing;
            set { if (_isIndexing != value) { _isIndexing = value; OnPropertyChanged(nameof(IsIndexing)); } }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set { if (_isSearching != value) { _isSearching = value; OnPropertyChanged(nameof(IsSearching)); } }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set { if (_progressValue != value) { _progressValue = value; OnPropertyChanged(nameof(ProgressValue)); } }
        }

        public int ProgressMax
        {
            get => _progressMax;
            set { if (_progressMax != value) { _progressMax = value; OnPropertyChanged(nameof(ProgressMax)); } }
        }

        public string ProgressText
        {
            get => _progressText;
            set { if (_progressText != value) { _progressText = value; OnPropertyChanged(nameof(ProgressText)); } }
        }

        public double IndexProgress
        {
            get => _indexProgress;
            set { if (_indexProgress != value) { _indexProgress = value; OnPropertyChanged(nameof(IndexProgress)); } }
        }

        public bool OpenOnDoubleClick
        {
            get => _openOnDoubleClick;
            set { if (_openOnDoubleClick != value) { _openOnDoubleClick = value; OnPropertyChanged(nameof(OpenOnDoubleClick)); } }
        }

        public ICommand BrowseFolderCommand { get; }
        public ICommand BuildIndexCommand { get; }
        public ICommand RebuildIndexCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OpenPdfCommand { get; }
        public ICommand DoubleClickCommand { get; }

        public SearchViewModel()
        {
            SearchResults = new ObservableCollection<SearchResultItem>();
            _logger = new LoggingService();
            var pdfExtractor = new PdfTextExtractor(_logger);
            var docxExtractor = new DocxTextExtractor(_logger);
            var metadataStore = new MetadataStore(_logger);
            _indexService = new IndexService(_logger, pdfExtractor, docxExtractor, metadataStore);
            _searchService = new SearchService(_logger);
            _openOnDoubleClick = true;

            BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
            BuildIndexCommand = new RelayCommand(_ => BuildIndex(), _ => !IsIndexing && !string.IsNullOrWhiteSpace(SelectedFolder));
            RebuildIndexCommand = new RelayCommand(_ => RebuildIndex(), _ => !IsIndexing && !string.IsNullOrWhiteSpace(SelectedFolder));
            CancelCommand = new RelayCommand(_ => Cancel(), _ => IsIndexing);
            SearchCommand = new RelayCommand(_ => PerformSearch(), _ => !string.IsNullOrWhiteSpace(SearchQuery) && !IsSearching);
            OpenPdfCommand = new RelayCommand<SearchResultItem>(OpenPdf, _ => true);
            DoubleClickCommand = new RelayCommand<SearchResultItem>(DoubleClick, _ => true);

            SelectedFolder = Constants.DefaultLetterFolder;
            StatusMessage = "Ready. Select a folder to begin.";
            _logger.Log("Application started");
        }

        private void BrowseFolder()
        {
            var folder = FolderBrowserHelper.BrowseForFolder("Select root letter folder");
            if (folder != null)
            {
                SelectedFolder = folder;
                StatusMessage = $"Selected: {SelectedFolder}";
                _logger.Log($"Folder selected: {SelectedFolder}");
            }
        }

        private void BuildIndex()
        {
            if (string.IsNullOrWhiteSpace(SelectedFolder) || !Directory.Exists(SelectedFolder))
            {
                StatusMessage = "Invalid folder selected";
                return;
            }

            IsIndexing = true;
            ProgressValue = 0;
            ProgressMax = 100;
            _cancellationTokenSource = new CancellationTokenSource();

            var progress = new Progress<(int total, int indexed, string currentFolder)>(report =>
            {
                ProgressMax = report.total;
                ProgressValue = report.indexed;
                ProgressText = $"{report.indexed} / {report.total}: {Path.GetFileName(report.currentFolder)}";
                IndexProgress = report.total > 0 ? (report.indexed * 100.0) / report.total : 0;
            });

            Task.Run(async () =>
            {
                try
                {
                    await _indexService.BuildIncrementalIndexAsync(SelectedFolder, progress, _cancellationTokenSource.Token);
                    StatusMessage = "Indexing complete";
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = "Indexing cancelled";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Indexing error: {ex.Message}";
                    _logger.LogError("Build index error", ex);
                }
                finally
                {
                    IsIndexing = false;
                }
            });
        }

        private void RebuildIndex()
        {
            if (string.IsNullOrWhiteSpace(SelectedFolder) || !Directory.Exists(SelectedFolder))
            {
                StatusMessage = "Invalid folder selected";
                return;
            }

            IsIndexing = true;
            ProgressValue = 0;
            ProgressMax = 100;
            _cancellationTokenSource = new CancellationTokenSource();

            var progress = new Progress<(int total, int indexed, string currentFolder)>(report =>
            {
                ProgressMax = report.total;
                ProgressValue = report.indexed;
                ProgressText = $"{report.indexed} / {report.total}: {Path.GetFileName(report.currentFolder)}";
                IndexProgress = report.total > 0 ? (report.indexed * 100.0) / report.total : 0;
            });

            Task.Run(async () =>
            {
                try
                {
                    await _indexService.RebuildIndexAsync(SelectedFolder, progress, _cancellationTokenSource.Token);
                    StatusMessage = "Index rebuild complete";
                }
                catch (OperationCanceledException)
                {
                    StatusMessage = "Index rebuild cancelled";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Rebuild error: {ex.Message}";
                    _logger.LogError("Rebuild index error", ex);
                }
                finally
                {
                    IsIndexing = false;
                }
            });
        }

        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling...";
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            IsSearching = true;
            Task.Run(() =>
            {
                try
                {
                    var results = _searchService.Search(SearchQuery);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SearchResults.Clear();
                        foreach (var result in results)
                        {
                            SearchResults.Add(result);
                        }
                        StatusMessage = $"Found {results.Count} result(s)";
                    });
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Search error: {ex.Message}";
                    _logger.LogError("Search error", ex);
                }
                finally
                {
                    IsSearching = false;
                }
            });
        }

        private void OpenPdf(SearchResultItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.FullPath))
                return;

            try
            {
                if (!File.Exists(item.FullPath))
                {
                    StatusMessage = "PDF file not found";
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = item.FullPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
                _logger.Log($"Opened PDF: {item.FullPath}");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error opening PDF";
                _logger.LogError("Error opening PDF", ex);
            }
        }

        private void DoubleClick(SearchResultItem item)
        {
            if (OpenOnDoubleClick)
            {
                OpenPdf(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
