namespace Utility.Lib.PatchSync
{
    [Serializable]
    public class PatchSyncData : BaseUtility
    {
        public ListHandler<string> StandardFiles { get; set; } = new ListHandler<string>();
        public ListHandler<string> DifferentFiles { get; set; } = new ListHandler<string>();
    }
}
