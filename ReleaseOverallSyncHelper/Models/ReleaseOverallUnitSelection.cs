using System.IO;
using System.Text.RegularExpressions;
using Utility;

namespace ReleaseOverallSyncHelper.Models
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
        public ReleaseOverallUnitSelection(string tag, string unit, string version)
        {
            Unit = unit;
            Version = version;
            Selected = true;
            var folder = Version.Replace('.', '_');
            var buildpath = ToPatchFolderName(tag);
            var unitparts = Unit.Split('_');
            var unitlabel = unitparts.LastOrDefault();
            var path = Path.Combine("P:\\fngams\\develop\\", buildpath, unitlabel, folder);
            ClonePath = path;
        }
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
