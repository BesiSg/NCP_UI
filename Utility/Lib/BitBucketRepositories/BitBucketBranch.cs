namespace Utility.Lib.BitBucketRepositories
{
    //public class RepositorySerializer : aSaveable
    //{
    //    public Repository Self { get; set; }
    //    public XmlDictionary<string, BranchSerializer> Children { get; set; } = new XmlDictionary<string, BranchSerializer>();
    //    public void Update(Repository source)
    //    {
    //        if (Self == null)
    //        {
    //            Self = source;
    //            Children.Clear();
    //        }
    //        else if (Self.AreEqual(source))
    //            return;
    //        else
    //        {
    //            Self.CopyProperties(source);
    //            Children.Clear();
    //        }
    //    }
    //}
    public class BranchResponse : BaseUtility
    {
        public int size
        {
            get => this.GetValue(() => size);
            set => this.SetValue(() => size, value);
        }
        public int limit
        {
            get => this.GetValue(() => limit);
            set => this.SetValue(() => limit, value);
        }
        public bool isLastPage
        {
            get => this.GetValue(() => isLastPage);
            set => this.SetValue(() => isLastPage, value);
        }
        public int start
        {
            get => this.GetValue(() => start);
            set => this.SetValue(() => start, value);
        }
        public List<Branch> values { get; set; }
    }

    public class Branch : BaseUtility
    {
        public string id
        {
            get => this.GetValue(() => id);
            set => this.SetValue(() => id, value);
        }
        public string displayId
        {
            get => this.GetValue(() => displayId);
            set => this.SetValue(() => displayId, value);
        }   // e.g. "master"
        public string latestCommit
        {
            get => this.GetValue(() => latestCommit);
            set => this.SetValue(() => latestCommit, value);
        }
        public bool isDefault
        {
            get => this.GetValue(() => isDefault);
            set => this.SetValue(() => isDefault, value);
        }
    }
}
