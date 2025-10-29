namespace Utility.Lib.BitBucketRepositories
{
    public class BitBucketStorage<T> : aSaveable
        where T : aSaveable
    {
        public event EventHandler<List<T>> Changed;
        public XmlDictionary<string, List<T>> Storage { get; set; } = new XmlDictionary<string, List<T>>();
        public void Update(IEnumerable<T> source, string key)
        {
            if (!Storage.ContainsKey(key))
                Storage[key] = new List<T>();
            Storage[key].Clear();
            Storage[key].AddRange(source);
            Changed?.Invoke(this, Storage[key]);
        }
    }
}
