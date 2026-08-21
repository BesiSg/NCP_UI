namespace Utility.Lib.Storage
{
    public class StorageDatList<T> : BaseUtility
        where T : BaseUtility
    {
        public event EventHandler<List<T>> Changed;
        public XmlDictionary<string, List<T>> Storage { get; set; } = new XmlDictionary<string, List<T>>();
        public void Replace(IEnumerable<T> source, string key)
        {
            if (!Storage.ContainsKey(key))
                Storage[key] = new List<T>();
            Storage[key].Clear();
            Storage[key].AddRange(source);
            Changed?.Invoke(this, Storage[key]);
        }
        public void Add(IEnumerable<T> source, string key)
        {
            if (!Storage.ContainsKey(key))
                Storage[key] = new List<T>();
            Storage[key].AddRange(source);
            Changed?.Invoke(this, Storage[key]);
        }
    }
}
