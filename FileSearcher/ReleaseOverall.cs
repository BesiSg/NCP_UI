using Utility;

namespace FileSearcher
{
    public class PathName : BaseUtility
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
        public PathName(string fullpath)
        {
            Fullpath = fullpath;
            Name = Path.GetFileName(Fullpath);
        }
    }
}
