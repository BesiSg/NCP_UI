using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace LicenseHandler
{
    public class Vision
    {
        const string verificationkey = "DELIVERY NOTE for SOFTWARE LICENSE 9645304";
        private string _rootPath = string.Empty;
        public Vision(string rootpath)
        {
            _rootPath = rootpath;
            if (!Directory.Exists(rootpath))
                MessageBox.Show($"rootpath does not exist");

        }
        public async Task ReformatFileName(string sapId, string serial_number)
        {
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(_rootPath, "*.pdf");
                var newFileName = $"{sapId}_Vision_License_{serial_number}.pdf";
                var newPath = Path.Combine(_rootPath, newFileName);
                try
                {
                    foreach (var file in files)
                    {
                        var pdf = new PDFHelper.PDFHelper(file);
                        pdf.ReadnExtract();
                        if (pdf.PDFContainsLine(verificationkey))
                        {
                            File.Move(file, newPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to rename vision license file '{_rootPath}': {ex.Message}");
                }
            });
        }
        private readonly Dictionary<char, char> DigitMap = new()
        {
            ['Ϭ'] = '0',
            ['ϭ'] = '1',
            ['Ϯ'] = '2',
            ['ϯ'] = '3',
            ['ϰ'] = '4',
            ['ϱ'] = '5',
            ['ϲ'] = '6',
            ['ϳ'] = '7',
            ['ϴ'] = '8',
            ['ϵ'] = '9'
        };
        private string NormalizeDigits(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                sb.Append(DigitMap.TryGetValue(c, out char digit)
                    ? digit
                    : c);
            }

            return sb.ToString();
        }
        private string ExtractLicenseNum(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var pattern = @"Serial Number:\s*(.+)";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                return string.Empty;

            string serial = match.Groups[1].Value.Trim();

            // Remove whitespace
            serial = Regex.Replace(serial, @"\s+", "");

            // Fix malformed PDF digit mappings
            serial = NormalizeDigits(serial);

            return serial;
        }

    }
}
