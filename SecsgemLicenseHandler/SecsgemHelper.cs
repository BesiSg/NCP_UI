using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace LicenseHandler
{
    public class Secsgem
    {
        const string verificationkey = "GWGEM";
        private string _rootPath = string.Empty;
        public Secsgem(string rootpath)
        {
            _rootPath = rootpath;
            if (!Directory.Exists(rootpath))
                MessageBox.Show($"rootpath does not exist");

        }
        public Task<string> ReformatFileName(string serial_number)
        {
            return Task.Run(() =>
            {
                var files = Directory.GetFiles(_rootPath, "*.pdf");
                var licensenum = string.Empty;
                foreach (var file in files)
                {
                    var pdf = new PDFHelper.PDFHelper(file);
                    pdf.ReadnExtract();
                    if (pdf.PDFContainsLine(verificationkey))
                    {
                        licensenum = ExtractLicenseNum(pdf.AllText);
                        if (licensenum != string.Empty)
                        {
                            var newFileName = $"{licensenum}_{serial_number}.pdf";
                            var newPath = Path.Combine(_rootPath, newFileName);
                            if (File.Exists(newPath)) continue;
                            File.Move(file, newPath);
                        }
                    }
                }
                return licensenum;
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
            var serialMatch = Regex.Match(
                text,
                pattern,
                RegexOptions.IgnoreCase);

            if (serialMatch.Success)
            {
                string serial = serialMatch.Groups[1].Value.Trim();

                serial = Regex.Replace(serial, @"\s+", "");
                serial = NormalizeDigits(serial);

                return serial;
            }

            // Format 2: GWGEM 4614-T04 N/A
            var licenseMatch = Regex.Match(
                text,
                @"\b(\d{4}-T\d+)\b",
                RegexOptions.IgnoreCase);

            if (licenseMatch.Success)
                return licenseMatch.Groups[1].Value;
            return string.Empty;
        }

    }
}
