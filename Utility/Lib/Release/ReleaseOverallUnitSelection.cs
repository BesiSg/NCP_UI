using System.IO;
using System.Text.RegularExpressions;

namespace Utility.Lib.Release
{
    public class ReleaseOverallUnitSelection : BaseUtility
    {
        public string Unit
        {
            get => GetValue(() => Unit);
            set => SetValue(() => Unit, value);
        }
        public string Version
        {
            get => GetValue(() => Version);
            set => SetValue(() => Version, value);
        }
        public bool Selected
        {
            get => GetValue(() => Selected);
            set => SetValue(() => Selected, value);
        }
        public string ClonePath
        {
            get => GetValue(() => ClonePath);
            set => SetValue(() => ClonePath, value);
        }
        public ReleaseOverallUnitSelection(string releasetag, string unit, string unittag)
        {
            Unit = unit;
            Version = unittag;
            Selected = true;
            var folder = Version.Replace('.', '_');
            var buildpath = ToPatchFolderName(releasetag);
            var unitparts = Unit.Split('_');
            var unitlabel = unitparts.LastOrDefault();
            unitlabel = char.ToUpperInvariant(unitlabel[0]) + unitlabel.Substring(1).ToLowerInvariant();
            var path = Path.Combine("P:\\fngams\\develop\\", buildpath, unitlabel, folder);
            ClonePath = path;
        }
        public ReleaseOverallUnitSelection()
        { }
        private string ToPatchFolderName(string tag)
        {
            var match = Regex.Match(
                tag,
                @"^(R\d+\.\d+\.\d+)", RegexOptions.IgnoreCase);

            if (!match.Success)
                throw new ArgumentException("Invalid tag format.");

            return match.Value.Replace('.', '_') + "_patch";
        }
    }
}
