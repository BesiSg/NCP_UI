namespace Utility.Lib.PatchSync
{
    [Serializable]
    public class PatchFilesStorage : aSaveable<PatchSyncData>
    {
        public ListHandler<string> GetStandardFiles(string key)
        {
            if (!Data.ContainsKey(key) || Data[key] == null)
                Data[key] = new PatchSyncData();
            if (Data[key].StandardFiles == null)
                Data[key].StandardFiles = new ListHandler<string>();
            return Data[key].StandardFiles;
        }
        public ListHandler<string> GetDifferentFiles(string key)
        {
            if (!Data.ContainsKey(key) || Data[key] == null)
                Data[key] = new PatchSyncData();
            if (Data[key].DifferentFiles == null)
                Data[key].DifferentFiles = new ListHandler<string>();
            return Data[key].DifferentFiles;
        }
    }
}
