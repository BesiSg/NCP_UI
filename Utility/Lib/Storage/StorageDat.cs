namespace Utility.Lib.Storage
{
    public class StorageDat<T> : BaseUtility
        where T : BaseUtility
    {
        public event EventHandler<T> Changed;
        public XmlDictionary<string, T> Storage { get; set; } = new XmlDictionary<string, T>();
        public void Update(T source, string key)
        {
            Storage[key] = source;
            Changed?.Invoke(this, Storage[key]);
        }
    }
}
