//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Text;
//using Syncfusion.Pdf;
//using Syncfusion.Pdf.Parsing;

//namespace PDFHelper
//{
//    public class PDFHelper : IDisposable
//    {
//        private readonly string _path;
//        private PdfLoadedDocument _document;
//        private readonly List<PdfLoadedPage> _pages;
//        private string _allText = string.Empty;

//        public ReadOnlyCollection<PdfLoadedPage> Pages => _pages.AsReadOnly();
//        public string AllText => _allText;

//        public PDFHelper(string source)
//        {
//            _path = source ?? throw new ArgumentNullException(nameof(source));
//            _pages = new List<PdfLoadedPage>();
//            ReadPDF();
//            ExtractAllText();
//        }

//        private void ReadPDF()
//        {
//            try
//            {
//                _document = new PdfLoadedDocument(_path);
//                foreach (PdfLoadedPage page in _document.Pages)
//                {
//                    _pages.Add(page);
//                }
//                Console.WriteLine($"Number of pages: {_pages.Count}");
//            }
//            catch (Exception ex)
//            {
//                // Handle exceptions as needed
//                Console.WriteLine($"Error reading PDF: {ex.Message}");
//            }
//        }

//        private void ExtractAllText()
//        {
//            if (_pages.Count == 0)
//                return;

//            var allTextBuilder = new StringBuilder();

//            foreach (var page in _pages)
//            {
//                string text = page.ExtractText(true);
//                allTextBuilder.AppendLine(text);
//            }

//            _allText = allTextBuilder.ToString();
//        }

//        public bool PDFContainsLine(string lineToFind, bool ignoreCase = false)
//        {
//            if (ignoreCase)
//            {
//                return _allText.IndexOf(lineToFind, StringComparison.OrdinalIgnoreCase) >= 0;
//            }
//            else
//            {
//                return _allText.Contains(lineToFind);
//            }
//        }

//        public string GetPageText(int pageNumber)
//        {
//            if (pageNumber < 1 || pageNumber > _pages.Count)
//                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number out of range.");

//            return _pages[pageNumber - 1].ExtractText(true);
//        }

//        public void Dispose()
//        {
//            _document?.Close(true);
//            _document = null;
//        }
//    }
//}




using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PDFHelper
{
    public class PDFHelper
    {
        public string Path => _path;
        private string _path = string.Empty;
        private List<UglyToad.PdfPig.Content.Page> _pages;
        private string _allText = string.Empty;
        public List<string> Pages { get; private set; } = new List<string>();
        public string AllText => _allText;
        public PDFHelper(string source)
        {
            _path = source;
            _pages = new List<UglyToad.PdfPig.Content.Page>();
        }

        private void ReadPDF()
        {
            var result = string.Empty;
            try
            {
                // Open the PDF document
                using (PdfDocument document = PdfDocument.Open(_path))
                {
                    Console.WriteLine($"Number of pages: {document.NumberOfPages}\n");
                    _pages.AddRange(document.GetPages());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExtractAllText()
        {
            if (_pages.Count == 0)
                return;
            const double yTolerance = 3.0;
            var allText = new StringBuilder();

            foreach (var page in _pages)
            {
                var textbypage= new StringBuilder();
                var words = page.GetWords()
                    .OrderByDescending(w => w.BoundingBox.Bottom)
                    .ThenBy(w => w.BoundingBox.Left)
                    .ToList();

                var lines = new List<List<Word>>();

                foreach (var word in words)
                {
                    var line = lines.FirstOrDefault(l =>
                        Math.Abs(l[0].BoundingBox.Bottom - word.BoundingBox.Bottom) <= yTolerance);

                    if (line == null)
                    {
                        line = new List<Word>();
                        lines.Add(line);
                    }

                    line.Add(word);
                }

                foreach (var line in lines)
                {
                    string lineText = string.Join(" ",
                        line.OrderBy(w => w.BoundingBox.Left)
                            .Select(w => w.Text));
                    textbypage.AppendLine(lineText);
                    allText.AppendLine(lineText);
                }
                Pages.Add(textbypage.ToString());
                allText.AppendLine();
            }

            _allText = allText.ToString();
        }
        public bool PDFContainsLine(string lineToFind) => _allText.Contains(lineToFind);
        public void ReadnExtract()
        {
            ReadPDF();
            ExtractAllText();
        }
    }

}
