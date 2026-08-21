using System.Text.RegularExpressions;
using Utility.Lib.Datasheet;

namespace SystemDatasheetHandler
{
    public class DatasheetHelper
    {
        public string Path => _helper.Path;
        public Datasheetproperties Properties { get; private set; }
        private PDFHelper.PDFHelper _helper;
        private List<string> _pages => _helper.Pages;

        public DatasheetHelper(string path)
        {
            _helper = new PDFHelper.PDFHelper(path);
        }
        private string ExtractNcpNumber()
        {
            if (_helper.Pages.Count == 0)
                return string.Empty;

            // Regex pattern, ignore case
            Regex ncpPattern = new Regex(@"Software patches requirement\s*:\s*(YES|NO)\s*,\s*(NCP-\d+)", RegexOptions.IgnoreCase);

            foreach (var text in _pages)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                // Normalize: replace multiple whitespace with single space, trim, and lowercase
                string normalized = Regex.Replace(text, @"\s+", " ").Trim().ToLower();

                Match match = ncpPattern.Match(normalized);
                if (match.Success)
                {
                    string ncp = match.Groups[2].Value;
                    return ncp.ToUpper();
                }
            }
            return string.Empty;
        }
        private string ExtractRefSystemSN()
        {
            if (_pages.Count == 0)
                return string.Empty;

            // Compile regex patterns
            Regex sectionStartPattern = new Regex(@"4\.\s*System Configuration", RegexOptions.IgnoreCase);
            Regex sectionEndPattern = new Regex(@"^\d+\.", RegexOptions.Multiline);
            Regex repeatMachinePattern = new Regex(@"machine number", RegexOptions.IgnoreCase);
            Regex sixDigitPattern = new Regex(@"\b\d{6}\b");

            foreach (var text in _pages)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                // Find the start of the section
                Match sectionStartMatch = sectionStartPattern.Match(text);
                if (!sectionStartMatch.Success)
                    continue;

                // Extract section text from the start of the match to the end
                string sectionText = text.Substring(sectionStartMatch.Index);

                // Find the end of the section (next numbered section), starting search after 20 chars
                Match sectionEndMatch = sectionEndPattern.Match(sectionText, 20);
                if (sectionEndMatch.Success)
                {
                    sectionText = sectionText.Substring(0, sectionEndMatch.Index);
                }

                // Process each line in the section
                string[] lines = sectionText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    // Normalize line: replace multiple whitespace with single space, trim, lowercase
                    string normalized = Regex.Replace(line, @"\s+", " ").Trim().ToLower();

                    if (repeatMachinePattern.IsMatch(normalized))
                    {
                        // Find all 6-digit numbers in the line
                        MatchCollection matches = sixDigitPattern.Matches(normalized);
                        if (matches.Count > 0)
                        {
                            // Return the last matched 6-digit number
                            return matches[matches.Count - 1].Value;
                        }
                    }
                }
            }

            // Not found
            return string.Empty;
        }
        private string ExtractSystemSN()
        {
            if (_pages.Count == 0)
                return string.Empty;

            Regex pattern = new Regex(@"Sub projects kicked\s*:\s*([\d,]+)", RegexOptions.IgnoreCase);

            foreach (var text in _pages)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                // Normalize spaces (replace multiple whitespace with single space)
                string normalized = Regex.Replace(text, @"\s+", " ").Trim();

                Match match = pattern.Match(normalized);
                if (match.Success)
                {
                    string numbersStr = match.Groups[1].Value; // e.g. "852571,85257151,52,31,81,85,"
                    string[] numbers = numbersStr.Split(',');

                    foreach (var num in numbers)
                    {
                        if (!string.IsNullOrWhiteSpace(num) && Regex.IsMatch(num, @"^\d+$"))
                        {
                            return num; // Return the first valid number
                        }
                    }
                }
            }

            return string.Empty;
        }
        private string ExtractSapNumber()
        {
            if (_pages.Count == 0)
                return string.Empty;

            Regex sapPattern = new Regex(@"Main Project\s*:\s*([\d\-\/]+)", RegexOptions.IgnoreCase);

            foreach (var text in _pages)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                // Normalize spaces
                string normalized = Regex.Replace(text, @"\s+", " ").Trim();

                Match match = sapPattern.Match(normalized);
                if (match.Success)
                {
                    string sapRaw = match.Groups[1].Value;

                    string[] parts = null;

                    if (sapRaw.Contains("/"))
                    {
                        parts = sapRaw.Split('/');
                    }
                    else if (sapRaw.Contains("-"))
                    {
                        parts = sapRaw.Split('-');
                    }
                    else
                    {
                        // No separator, return raw value
                        return sapRaw;
                    }

                    if (parts.Length == 2)
                    {
                        string part1 = parts[0];
                        string part2 = parts[1].TrimStart('0');

                        if (string.IsNullOrEmpty(part2))
                        {
                            part2 = "0";
                        }

                        return $"{part1}-{part2}";
                    }
                    else
                    {
                        return sapRaw;
                    }
                }
            }

            return string.Empty;
        }
        private string ExtractCustomerDestination()
        {
            if (_pages.Count == 0)
                return string.Empty;

            Regex pattern = new Regex(@"Software specification\s*/\s*version\s*:\s*(.+)", RegexOptions.IgnoreCase);

            foreach (var text in _pages)
            {
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    Match match = pattern.Match(trimmedLine);

                    if (match.Success)
                    {
                        string versionStr = match.Groups[1].Value.Trim();

                        // Split by spaces, dots, commas, plus signs, etc.
                        string[] tokens = Regex.Split(versionStr, @"[.\s,+]+");

                        // Filter tokens that are exactly 4 letters long (letters only)
                        foreach (string token in tokens)
                        {
                            var matchversion = Regex.Match(token, @"^(?<Customer>[A-Za-z]{4,5})\d*$");

                            if (matchversion.Success)
                            {
                                return matchversion.Groups["Customer"].Value;
                            }
                        }

                        // Fallback: return last token without punctuation if any
                        if (tokens.Length > 0)
                        {
                            string lastToken = tokens[tokens.Length - 1].TrimEnd('.', ',', ';', ':');
                            return string.IsNullOrEmpty(lastToken) ? string.Empty : lastToken;
                        }
                    }
                }
            }

            return string.Empty;
        }
        public Task ExtractData()
        {
            return Task.Run(() =>
            {
                _helper.ReadnExtract();
                var ncp = ExtractNcpNumber();
                var refsystemsn = ExtractRefSystemSN();
                var systemsn = ExtractSystemSN();
                var sapnum = ExtractSapNumber();
                var customerabbre = ExtractCustomerDestination();
                Properties = new Datasheetproperties(ncp, refsystemsn, systemsn, sapnum, customerabbre, _helper.Path);
            });
        }
    }
}
