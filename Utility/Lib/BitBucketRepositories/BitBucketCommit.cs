namespace Utility.Lib.BitBucketRepositories
{
    //public class BranchSerializer : aSaveable
    //{
    //    public Branch Self { get; set; }
    //    public XmlDictionary<string, Commit> Children { get; set; } = new XmlDictionary<string, Commit>();
    //    public void Update(Branch source)
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
    public class CommitResponse : BaseUtility
    {
        public List<Commit> values { get; set; }
        public bool? isLastPage
        {
            get => this.GetValue(() => this.isLastPage);
            set => this.SetValue(() => this.isLastPage, value);
        }
        public int? nextPageStart
        {
            get => this.GetValue(() => this.nextPageStart);
            set => this.SetValue(() => this.nextPageStart, value);
        }
    }

    public class Commit : BaseUtility
    {
        public string id
        {
            get => this.GetValue(() => this.id);
            set => this.SetValue(() => this.id, value);
        }
        public string displayId
        {
            get => this.GetValue(() => this.displayId);
            set => this.SetValue(() => this.displayId, value);
        }
        public string latestCommit
        {
            get => this.GetValue(() => this.latestCommit);
            set => this.SetValue(() => this.latestCommit, value);
        }
        public void UpdateTag(Tag source)
        {
            displayId = source.displayId;
            latestCommit = source.latestCommit;
        }
    }
}
