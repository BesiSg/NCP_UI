namespace Utility.Lib.BitBucketRepositories
{
    //public class ProjectSerializer : aSaveable
    //{
    //    public Project Self { get; set; }= new Project();
    //    public XmlDictionary<string, RepositorySerializer> Children { get; set; } = new XmlDictionary<string, RepositorySerializer>();
    //    public void Update(Project project)
    //    {
    //        if (Self == null)
    //        {
    //            Self = project;
    //            Children.Clear();
    //        }
    //        else if (Self.AreEqual(project)) 
    //            return;
    //        else
    //        {
    //            Self.CopyProperties(project);
    //            Children.Clear();
    //        }
    //    }
    //}
    // Define classes for deserialization
    public class RepoResponse : BaseUtility
    {
        public List<Repository> values { get; set; }
        public bool isLastPage
        {
            get => this.GetValue(() => this.isLastPage);
            set => this.SetValue(() => this.isLastPage, value);
        }
        public int size
        {
            get => this.GetValue(() => this.size);
            set => this.SetValue(() => this.size, value);
        }
        public int limit
        {
            get => this.GetValue(() => this.limit);
            set => this.SetValue(() => this.limit, value);
        }
        public int start
        {
            get => this.GetValue(() => this.start);
            set => this.SetValue(() => this.start, value);
        }
    }

    public class Repository : aSaveable
    {
        public string slug
        {
            get => this.GetValue(() => this.slug);
            set => this.SetValue(() => this.slug, value);
        }
        public string name
        {
            get => this.GetValue(() => this.name);
            set => this.SetValue(() => this.name, value);
        }
        // Add other fields if needed
    }
}
