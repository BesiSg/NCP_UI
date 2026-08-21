namespace Utility.Lib.NewSystemWorkspace
{
    public class NewSystemStorage : BaseUtility
    {
        public XmlDictionary<string, NewSystemWorkspacePaths> Storage { get; set; } = new XmlDictionary<string, NewSystemWorkspacePaths>();

        public event EventHandler<NewSystemWorkspacePaths> Changed;
        public void Update(NewSystemWorkspacePaths source, string key)
        {
            Storage[key] = source;
            Changed?.Invoke(this, Storage[key]);
        }
    }
}
