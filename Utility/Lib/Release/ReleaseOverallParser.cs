using System.Text.RegularExpressions;

namespace Utility.Lib.Release
{
    public class ReleaseOverallParser
    {
        private static readonly string[] EndMarkers =
        {
            "# if there are any units which may not be part of this release,",
            "# This is a dummy entry to satisfy the CopyRelease script"
        };

        public List<ReleaseOverallUnitSelection> PopulateUnitSelections(
            string tag,
            string cfgContent)
        {
            var list = new List<ReleaseOverallUnitSelection>();

            int markerIndex = -1;

            foreach (var marker in EndMarkers)
            {
                int idx = cfgContent.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase);

                if (idx >= 0 && (markerIndex == -1 || idx < markerIndex))
                {
                    markerIndex = idx;
                }
            }

            if (markerIndex >= 0)
            {
                cfgContent = cfgContent[..markerIndex];
            }

            foreach (Match match in UnitRegex.Matches(cfgContent))
            {
                list.Add(new ReleaseOverallUnitSelection(
                    tag,
                    match.Groups["Unit"].Value,
                    match.Groups["Version"].Value));
            }

            return list;
        }

        private readonly Regex UnitRegex =
            new(
                @"^(?<Unit>[A-Z0-9]{4}|Version_MMI)\s*(?:=)?\s+(?<Version>[VR]\S+)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.Multiline);
        //public List<ReleaseOverallUnitSelection> PopulateUnitSelections(string tag, string cfgContent)
        //{
        //    var list = new List<ReleaseOverallUnitSelection>();
        //    // Ignore everything after the marker
        //    int markerIndex = cfgContent.IndexOf(
        //        EndMarker,
        //        StringComparison.OrdinalIgnoreCase);

        //    if (markerIndex >= 0)
        //    {
        //        cfgContent = cfgContent[..markerIndex];
        //    }

        //    foreach (Match match in UnitRegex.Matches(cfgContent))
        //    {
        //        var item = new ReleaseOverallUnitSelection(
        //                tag,
        //                match.Groups["Unit"].Value,
        //                match.Groups["Version"].Value);
        //        list.Add(item);
        //    }
        //    return list;
        //}
    }
}
