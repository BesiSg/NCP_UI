using System.Text.RegularExpressions;

namespace FileSearcher
{
    public class FolderSearcherHelper
    {
        private string _root = string.Empty;
        private string _matchheader = string.Empty;
        private string _matchend = string.Empty;
        private string _matchanywhere = string.Empty;
        private string _extension = string.Empty;
        private string _pattern = string.Empty;
        public Dictionary<string, List<PathName>> ReleaseOverall { get; private set; } = [];
        public FolderSearcherHelper(string rootpath, string matchheader, string matchend, string matchanywhere, string extension, string pattern)
        {
            _root = rootpath;
            _matchheader = matchheader;
            _matchend = matchend;
            _matchanywhere = matchanywhere;
            _extension = extension;
            _pattern = pattern;
        }

        public async Task Search(CancellationToken token = default)
        {
            var regex = new Regex(
                _pattern,
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var releaseOverall = await Task.Run(() =>
            {
                IEnumerable<string> folders = Enumerable.Empty<string>();

                if (!string.IsNullOrWhiteSpace(_matchheader) ||
                    !string.IsNullOrWhiteSpace(_matchend))
                {
                    folders = Directory.EnumerateDirectories(
                        _root,
                        $"{_matchheader}*{_matchend}");
                }
                else if (!string.IsNullOrWhiteSpace(_matchanywhere))
                {
                    folders = Directory.EnumerateDirectories(
                        _root,
                        $"*{_matchanywhere}*");
                }

                return folders
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .WithCancellation(token)
                    .SelectMany(folder =>
                    {
                        return Directory
                            .EnumerateFiles(folder, $"*{_extension}")
                            .Select(file =>
                            {
                                var match = regex.Match(Path.GetFileNameWithoutExtension(file));

                                if (!match.Success)
                                    return null;

                                var version = match.Groups["Version"]
                                    .Value
                                    .ToLowerInvariant();

                                var customer = match.Groups["Customer"]
                                    .Value
                                    .ToLowerInvariant();

                                return new
                                {
                                    Key = $"support/{customer}/{version}",
                                    File = new PathName(file)
                                };
                            })
                            .Where(x => x != null)!;
                    })
                    .GroupBy(x => x!.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x!.File).ToList());
            }, token);

            ReleaseOverall.Clear();

            foreach (var kvp in releaseOverall)
            {
                ReleaseOverall[kvp.Key] = kvp.Value;
            }
        }
    }
}
