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

        //public Task Search()
        //{
        //    return Task.Run(() =>
        //    {
        //        ReleaseOverall.Clear();
        //        IEnumerable<string> result = Enumerable.Empty<string>();

        //        if (!string.IsNullOrWhiteSpace(_matchheader) ||
        //            !string.IsNullOrWhiteSpace(_matchend))
        //        {
        //            result = Directory.EnumerateDirectories(
        //                _root,
        //                $"{_matchheader}*{_matchend}");
        //        }
        //        else if (!string.IsNullOrWhiteSpace(_matchanywhere))
        //        {
        //            result = Directory.EnumerateDirectories(
        //                _root,
        //                $"*{_matchanywhere}*");
        //        }

        //        var files = new List<PathName>();
        //        foreach (var folder in result)
        //        {

        //            foreach (var file in Directory.EnumerateFiles(folder, $"*{_extension}"))
        //            {
        //                var match = Regex.Match(Path.GetFileName(file), _pattern, RegexOptions.IgnoreCase);
        //                if (!match.Success)
        //                    continue;
        //                var version = match.Groups["Version"].Value.ToLower();
        //                var customer = match.Groups["Customer"].Value.ToLower();
        //                if (!ReleaseOverall.ContainsKey($"support/{customer}/{version}"))
        //                    ReleaseOverall[$"support/{customer}/{version}"] = new List<PathName>();
        //                ReleaseOverall[$"support/{customer}/{version}"].Add(new PathName(file));
        //            }
        //        }
        //    });
        //}

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
                                var match = regex.Match(Path.GetFileName(file));

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
