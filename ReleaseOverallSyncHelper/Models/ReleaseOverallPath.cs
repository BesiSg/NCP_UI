using System.IO;
using Utility;

namespace ReleaseOverallSyncHelper.Models
{
    public class ReleaseOverallPath : BaseUtility
    {
        public string Fullpath
        {
            get => GetValue(() => Fullpath);
            set => SetValue(() => Fullpath, value);
        }
        public string Name
        {
            get => GetValue(() => Name);
            set => SetValue(() => Name, value);
        }
        public ReleaseOverallPath(string fullpath)
        {
            Fullpath = fullpath;
            Name = Path.GetFileName(Fullpath);
        }
    }
}
