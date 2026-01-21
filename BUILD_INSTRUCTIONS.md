# SPP2 Letter Search - Complete Delivery

## Build and Run Instructions

### Option 1: Quick Start (Recommended)

```powershell
cd c:\LetterMaster\SPP2LetterSearch
dotnet restore
dotnet build
dotnet run
```

### Option 2: Build Release Version

```powershell
cd c:\LetterMaster\SPP2LetterSearch
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

### Option 3: Publish Standalone Executable

```powershell
cd c:\LetterMaster\SPP2LetterSearch
dotnet publish -c Release -o publish
# Executable location: .\publish\SPP2LetterSearch.exe
```

## Complete File Structure

```
c:\LetterMaster\SPP2LetterSearch\
│
├── SPP2LetterSearch.csproj                 ← Project file (NuGet dependencies)
├── Constants.cs                             ← App constants and paths
├── README.md                                ← Comprehensive documentation
├── TESTING.md                               ← Testing guide
│
├── App.xaml                                 ← WPF application resource
├── App.xaml.cs                              ← Application entry point
│
├── MainWindow.xaml                          ← Main UI layout (XAML)
├── MainWindow.xaml.cs                       ← UI code-behind
│
├── Models/
│   ├── LetterDocumentMetadata.cs           ← PDF metadata model
│   └── SearchResultItem.cs                  ← Search result display model
│
├── ViewModels/
│   └── SearchViewModel.cs                   ← MVVM ViewModel with all logic
│
├── Services/
│   ├── LoggingService.cs                   ← File-based logging to %LOCALAPPDATA%
│   ├── PdfTextExtractor.cs                 ← PDF text extraction (UglyToad.PdfPig)
│   ├── IndexService.cs                     ← Lucene.NET indexing orchestration
│   ├── SearchService.cs                    ← Lucene.NET search queries
│   ├── MetadataStore.cs                    ← LiteDB metadata persistence
│   └── FolderBrowserHelper.cs              ← Windows API folder browser
│
├── Commands/
│   └── RelayCommand.cs                     ← MVVM ICommand implementation
│
└── bin/Debug/net8.0-windows/
    └── SPP2LetterSearch.dll                ← Built assembly (after build)
```

## File Descriptions

### Configuration Files

**SPP2LetterSearch.csproj** - .NET project file with all dependencies:
- Target: .NET 8 WPF on Windows
- NuGet packages: Lucene.Net 4.8.0, UglyToad.PdfPig 1.7.0, LiteDB 5.0.17
- Output: Windows executable

### Application Shell

**App.xaml / App.xaml.cs** - WPF application entry point
- Initializes MainWindow
- Sets DataContext to SearchViewModel
- Handles application lifecycle

**MainWindow.xaml** - User interface definition
- Folder selection section (Browse, Build/Update/Rebuild buttons)
- Index progress section (progress bar, status, cancel)
- Search section (textbox, search button, options)
- Results DataGrid with Open button
- Status bar with result count

**MainWindow.xaml.cs** - UI interaction code-behind
- Keyboard shortcuts (Enter to search)
- Double-click handler to open PDFs
- Event wiring for commands

### Constants

**Constants.cs** - Application-wide configuration
- `AppDataPath`: %LOCALAPPDATA%\SPP2LetterSearch
- `IndexPath`: Where Lucene index is stored
- `LogFilePath`: Application log location
- `MetadataDbPath`: LiteDB database location
- `LetterFolderPattern`: "SPP2-KLN-PRO-LET-" naming convention

### Models (Data Classes)

**LetterDocumentMetadata.cs** - Represents indexed PDF metadata
- `Id`: Unique identifier (hash of path)
- `LetterNo`: 4-digit number extracted from folder name
- `FolderName`: Source folder name
- `FileName`: PDF filename
- `FullPath`: Complete file path
- `FileSize`: Size in bytes
- `LastWriteTimeUtc`: For change detection

**SearchResultItem.cs** - Represents search result in UI
- `LetterNo`, `FileName`, `FullPath`, `FolderName`
- `Score`: Lucene relevance score
- `Snippet`: Text preview with match context
- Implements `INotifyPropertyChanged` for WPF binding

### View Model (MVVM Logic)

**SearchViewModel.cs** - Core application logic
- Properties: SelectedFolder, SearchQuery, StatusMessage, IsIndexing, etc.
- Commands: BrowseFolder, BuildIndex, RebuildIndex, Cancel, Search, OpenPdf, DoubleClick
- Async methods for background tasks
- Progress reporting with `IProgress<T>`
- Cancellation support with `CancellationToken`

### Services (Business Logic)

**LoggingService.cs** - Simple file-based logging
- Thread-safe logging to `%LOCALAPPDATA%\SPP2LetterSearch\log.txt`
- Methods: `Log()`, `LogError()`, `ClearLog()`
- Timestamp format: `[YYYY-MM-DD HH:MM:SS]`

**PdfTextExtractor.cs** - PDF text extraction
- Uses UglyToad.PdfPig library (pure .NET)
- Page-by-page text extraction
- Returns combined text from all pages
- Graceful error handling for corrupted PDFs

**IndexService.cs** - Lucene.NET indexing
- `BuildIncrementalIndexAsync()`: Smart indexing (only new/changed)
- `RebuildIndexAsync()`: Full rebuild from scratch
- `ClearIndex()`: Delete all indexed data
- Creates Document objects with all metadata fields
- Commits changes atomically

**SearchService.cs** - Lucene.NET searching
- `Search()`: Execute search query
- MultiFieldQueryParser with content, fileName, folderName
- Returns top results sorted by letter number then score
- `ExtractSnippet()`: Generate text preview around matches

**MetadataStore.cs** - LiteDB persistence
- `SaveMetadata()`, `GetMetadata()`, `GetAllMetadata()`
- `DeleteMetadata()`, `ClearAllMetadata()`
- `HasChanged()`: Detect if file needs re-indexing
- Database operations with error handling

**FolderBrowserHelper.cs** - Windows API folder selection
- Uses SHBrowseForFolder Windows API
- Pure .NET without System.Windows.Forms dependency
- Returns selected folder path or null if cancelled

### Commands (MVVM)

**RelayCommand.cs** - ICommand implementation for binding
- Generic `RelayCommand<T>` for parameterized commands
- Generic-less `RelayCommand` for simple commands
- Supports `CanExecute` predicate for enabling/disabling buttons

## Key Features Implemented

### ✅ Folder Selection
- Windows API folder browser dialog
- Validates folder exists before operations
- Displays selected path in UI

### ✅ Background Indexing
- Asynchronous PDF processing
- Non-blocking UI with real-time progress
- Cancellation support
- Error logging and recovery

### ✅ Incremental Indexing
- Compares LastWriteTimeUtc and FileSize
- Only re-indexes changed/new PDFs
- Removes deleted files from index
- Metadata stored in SQLite database

### ✅ Full-Text Search
- Lucene.NET with StandardAnalyzer
- Query types: simple, AND, OR, phrase, negation
- Relevance scoring and sorting
- Snippet extraction with context

### ✅ Result Display
- DataGrid with sortable columns
- Letter number, filename, score, snippet
- Open button for each result
- Double-click support

### ✅ Persistent Storage
- Index: `%LOCALAPPDATA%\SPP2LetterSearch\Index\`
- Metadata: `%LOCALAPPDATA%\SPP2LetterSearch\metadata.db`
- Logs: `%LOCALAPPDATA%\SPP2LetterSearch\log.txt`

### ✅ Error Handling
- Corrupted PDFs: logged, skipped, continue
- Missing files: removed from index
- Network issues: user-friendly messages
- All errors logged for debugging

### ✅ MVVM Architecture
- Separation of concerns
- INotifyPropertyChanged for bindings
- Commands for button interactions
- Observable collections for results

## NuGet Packages Used

```xml
<!-- Lucene.NET 4.8.0 - Full-text search engine -->
<PackageReference Include="Lucene.Net" Version="4.8.0-beta00016" />
<PackageReference Include="Lucene.Net.Analysis.Common" Version="4.8.0-beta00016" />
<PackageReference Include="Lucene.Net.QueryParser" Version="4.8.0-beta00016" />

<!-- UglyToad.PdfPig 1.7.0 - PDF text extraction (pure .NET) -->
<PackageReference Include="UglyToad.PdfPig" Version="1.7.0-custom-5" />

<!-- LiteDB 5.0.17 - Embedded NoSQL database for metadata -->
<PackageReference Include="LiteDB" Version="5.0.17" />
```

## Data Locations

### Index Storage
```
%LOCALAPPDATA%\SPP2LetterSearch\Index\
```
Contains Lucene.NET index files:
- `segments.gen`, `_0.cfe`, `_0.cfs`, etc.

### Metadata Database
```
%LOCALAPPDATA%\SPP2LetterSearch\metadata.db
```
SQLite/LiteDB database containing:
- Document metadata for each indexed PDF
- Last modification time
- File size

### Application Log
```
%LOCALAPPDATA%\SPP2LetterSearch\log.txt
```
Human-readable log of all events and errors

## System Requirements

- **OS**: Windows 10 or Windows 11
- **.NET**: .NET 8 Runtime (included with SDK)
- **RAM**: 512MB minimum (1GB+ recommended)
- **Disk**: Varies with PDF collection size (~10-20% of source PDFs)

## Example Usage Workflow

1. **First Run**:
   - Launch application
   - Click Browse → select `C:\Letters\Archive`
   - Click "Build/Update Index"
   - Wait for indexing to complete (progress shown)

2. **Search**:
   - Type "material weld" in search box
   - Press Enter or click Search
   - Results appear in DataGrid
   - Click Open to view PDF

3. **Add New PDFs**:
   - Copy new PDFs to letter folders
   - Click "Build/Update Index" again
   - Only new files indexed (fast!)
   - Search immediately finds new content

4. **Modify Search**:
   - Change search terms
   - Press Enter
   - Previous results replaced

## Troubleshooting

### Build Fails
```
Error: Unsupported version of .NET
→ Install .NET 8 SDK: https://dotnet.microsoft.com/download
```

### Application Won't Start
```
Error: Unable to load one or more files
→ Run: dotnet restore
→ Then: dotnet build
```

### No Search Results
```
Problem: Search returns 0 results
→ Check log file for extraction errors
→ Verify PDFs have searchable text (not image-only)
→ Try rebuilding index
```

### Very Slow Indexing
```
Problem: First index takes very long
→ This is normal for large PDFs
→ Subsequent incremental indexes are much faster
→ Large PDFs (100+ pages) take more time
```

## Support & Documentation

- **README.md**: Comprehensive feature documentation
- **TESTING.md**: Testing guide with scenarios
- **This file**: Build and file structure information
- **Code comments**: Inline documentation in all source files
- **Log file**: Troubleshooting information in log.txt

---

**Version**: 1.0.0  
**Framework**: .NET 8 WPF  
**Language**: C# 12  
**License**: Internal Use - SPP2 Project  
**Created**: January 2026
