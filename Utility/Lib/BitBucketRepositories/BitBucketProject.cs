namespace Utility.Lib.BitBucketRepositories
{
    // Classes to deserialize JSON response
    public class ProjectResponse : BaseUtility
    {
        public List<Project> values { get; set; }
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

    public class Project : aSaveable
    {
        public string key
        {
            get => this.GetValue(() => this.key);
            set => this.SetValue(() => this.key, value);
        }
        public string name
        {
            get => this.GetValue(() => this.name);
            set => this.SetValue(() => this.name, value);
        }
        public string description
        {
            get => this.GetValue(() => this.description);
            set => this.SetValue(() => this.description, value);
        }
        public string type
        {
            get => this.GetValue(() => this.type);
            set => this.SetValue(() => this.type, value);
        }
        // Add other fields if needed
    }
}
