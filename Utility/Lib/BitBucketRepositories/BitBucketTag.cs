namespace Utility.Lib.BitBucketRepositories
{
    public class TagResponse : BaseUtility
    {
        public List<Tag> values { get; set; }
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

    public class Tag : aSaveable
    {
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
    }
}
