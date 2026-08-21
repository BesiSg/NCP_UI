using System.Text.RegularExpressions;
using Utility.Lib.Storage;

namespace Utility.Lib.Release
{
    public class ReleaseStorage : StorageDatList<ReleaseOverallUnitSelection>
    {
        public string? GetUnitVersion(string releaseversion, string unit)
        {
            if (!Storage.ContainsKey(releaseversion)) return null;
            return Storage[releaseversion].First(units => units.Unit == unit).Version;
        }
        public (string Releaseversion, string Unitversion) GetReleaseVersion(string unit, string unitversion)
        {
            foreach (var releaseversion in Storage)
            {
                var pair = releaseversion.Value.Where(unitversion => string.Equals(unitversion.Unit, unit, StringComparison.CurrentCultureIgnoreCase));
                if (pair == null || pair?.Count() == 0) continue;
                var source = Regex.Replace(
                    pair.First().Version,
                    @"^[VR]",
                    "",
                    RegexOptions.IgnoreCase);
                var target = Regex.Replace(
                    unitversion,
                    @"^[VR]",
                    "",
                    RegexOptions.IgnoreCase);
                if (source.Equals(target, StringComparison.OrdinalIgnoreCase))
                {
                    return (releaseversion.Key.ToUpperInvariant(), pair.First().Version);
                }
                //var pair = releaseversion.Value.First(unitver => string.Equals(unitver.Unit, unit, StringComparison.CurrentCultureIgnoreCase) && unitversion.Contains(unitver.Version));
                //if (pair != null) return pair.Version;
            }
            return (null, null);
        }
    }
}
