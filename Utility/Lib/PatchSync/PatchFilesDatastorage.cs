namespace Utility.Lib.PatchSync
{
    public class PatchFilesStorage : aSaveable
    {
        public XmlDictionary<string, List<string>> Storage { get; set; } = new XmlDictionary<string, List<string>>();
    }
}
