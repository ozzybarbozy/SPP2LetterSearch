# Testing Guide

## Sample Folder Structure

For testing the application, create a folder structure like this:

```
C:\TestLetters\
├── SPP2-KLN-PRO-LET-0001\
│   └── letter.pdf
├── SPP2-KLN-PRO-LET-0002\
│   └── document.pdf
├── SPP2-KLN-PRO-LET-0003\
│   └── letter.pdf
└── SPP2-KLN-PRO-LET-0004\
    ├── letter.pdf
    └── scanned_copy.pdf  (only first PDF will be indexed)
```

**Note**: PDFs must have searchable text. Image-only PDFs will not return text.

## Quick Test Scenario

1. **Create test structure**: Make folders named `SPP2-KLN-PRO-LET-0001` through `SPP2-KLN-PRO-LET-0005`

2. **Add PDF files**: Place sample OCR'd PDFs in each folder (any searchable PDF will work for testing)

3. **Launch application**: 
   ```powershell
   cd c:\LetterMaster\SPP2LetterSearch
   dotnet run
   ```

4. **Select folder**: Click Browse → select `C:\TestLetters\`

5. **Build index**: Click "Build/Update Index" → watch progress bar

6. **Search**: Type "material" or any word from your PDFs → click Search

7. **View results**: Click "Open" to view PDF or double-click row

## Features to Test

- [ ] Folder selection dialog works
- [ ] Progress bar shows during indexing
- [ ] Cancel button stops indexing
- [ ] Search results appear
- [ ] PDFs open in default viewer
- [ ] Incremental indexing (re-run, only modified files re-indexed)
- [ ] Rebuild index clears and rebuilds everything
- [ ] Snippet text shows context around match
- [ ] Results sorted by letter number
- [ ] UI remains responsive during search

## Test Queries

- `material` - Single term
- `material certificate` - Multiple terms (AND)
- `"material certificate"` - Exact phrase
- `steel OR iron` - OR query
- `weld AND NOT plastic` - Negation

## Troubleshooting Tests

### No results found?
1. Check log file: `%LOCALAPPDATA%\SPP2LetterSearch\log.txt`
2. Verify PDF contains searchable text (not image-only)
3. Try "Rebuild Index" to reset
4. Check folder names start with `SPP2-KLN-PRO-LET-`

### Index is empty?
1. Look at progress bar - did it complete?
2. Check extraction worked: Are PDFs valid and searchable?
3. Open one PDF in Adobe Reader - can you select text?
4. If image PDF, app will log warning but continue

## Performance Test

Time how long it takes to index 100 PDFs:
- Note start time
- Click "Build/Update Index"
- Note completion time
- Expected: 1-5 minutes depending on PDF size

Run second index (incremental) without changing files:
- Expected: <10 seconds (only validates files, no re-extraction)

Modify one PDF and re-index:
- Expected: Only that file re-indexed, others skipped
