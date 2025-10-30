using System.IO;
using Utility;

namespace Utility.PatchSync
{
    public class NCPFileStructure
    {
        public const string localNCP = $"D:\\LocalBuild\\nldvdata";
        public const string networkNCP = $"\\\\NLDVFS11";
        public const string fngams = $"Applicdata\\PVCS_M\\Data\\FNGAMS";
        public const string develop = "Develop";
        public const string localFNGAMS = $"{localNCP}\\{fngams}";
        public const string networkFNGAMS = $"{networkNCP}\\{fngams}";
        public const string localDevelop = $"{localFNGAMS}\\{develop}";
        public const string networkDevelop = $"{networkFNGAMS}\\{develop}";

        private PatchSync patchSync = new PatchSync();
        public static async Task<IEnumerable<string>> GetFileAsync(string folder)
        {
            return GetFileAsync(folder, "").Result;
        }
        public static async Task<IEnumerable<string>> GetFileAsync(string folder, string extension)
        {
            var list = new List<string>();
            if (!Directory.Exists(folder)) return list;
            return Directory.GetFiles(folder, extension).ToList();
        }
        public static async Task<IEnumerable<string>> GetFoldersAsync(string folder)
        {
            var list = new List<string>();
            if (!Directory.Exists(folder)) return list;
            return Directory.GetDirectories(folder).ToList();
        }
    }
}
