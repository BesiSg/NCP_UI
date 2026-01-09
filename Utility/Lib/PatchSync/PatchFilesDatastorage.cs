namespace Utility.Lib.PatchSync
{
    [Serializable]
    public class PatchFilesStorage : aSaveable<PatchSyncData>
    {
        public ListHandler<string> GetStandardFiles(string key)
        {
            if (!ContainsKey(key) || this[key] == null)
                this[key] = new PatchSyncData();
            if (this[key].StandardFiles == null)
                this[key].StandardFiles = new ListHandler<string>();
            return this[key].StandardFiles;
        }
        public ListHandler<string> GetDifferentFiles(string key)
        {
            if (!ContainsKey(key) || this[key] == null)
                this[key] = new PatchSyncData();
            if (this[key].DifferentFiles == null)
                this[key].DifferentFiles = new ListHandler<string>();
            return this[key].DifferentFiles;
        }
    }
}
