using System;
using System.IO;
using System.Text;
using UglyToad.PdfPig;
using SPP2LetterSearch.Services;

namespace SPP2LetterSearch.Services
{
    public class PdfTextExtractor
    {
        private readonly LoggingService _logger;

        public PdfTextExtractor(LoggingService logger)
        {
            _logger = logger;
        }

        public string ExtractText(string pdfPath)
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    _logger.LogError($"PDF file not found: {pdfPath}");
                    return string.Empty;
                }

                using (var document = PdfDocument.Open(pdfPath))
                {
                    var sb = new StringBuilder();
                    foreach (var page in document.GetPages())
                    {
                        var text = page.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            sb.Append(text);
                            sb.Append(" ");
                        }
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract text from PDF: {pdfPath}", ex);
                return string.Empty;
            }
        }
    }
}
