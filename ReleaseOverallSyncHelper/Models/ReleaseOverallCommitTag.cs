using System.IO;
using Utility;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public class ReleaseOverall : BaseUtility
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
        public ReleaseOverall(string fullpath)
        {
            Fullpath = fullpath;
            Name = Path.GetFileName(Fullpath);
        }
    }
}
