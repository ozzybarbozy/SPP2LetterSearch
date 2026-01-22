using System;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SPP2LetterSearch.Services
{
    public class DocxTextExtractor
    {
        private readonly LoggingService _logger;

        public DocxTextExtractor(LoggingService logger)
        {
            _logger = logger;
        }

        public string ExtractText(string docxPath)
        {
            try
            {
                if (!File.Exists(docxPath))
                {
                    _logger.LogError($"DOCX file not found: {docxPath}");
                    return string.Empty;
                }

                using (var document = WordprocessingDocument.Open(docxPath, false))
                {
                    var body = document.MainDocumentPart?.Document?.Body;
                    if (body == null)
                    {
                        _logger.Log($"Warning: No body found in DOCX: {docxPath}");
                        return string.Empty;
                    }

                    var sb = new StringBuilder();
                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        var text = paragraph.InnerText;
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
                _logger.LogError($"Failed to extract text from DOCX: {docxPath}", ex);
                return string.Empty;
            }
        }
    }
}
