using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;

namespace Utility.Lib.BitBucketActive.Task
{
    public class BitBucketTask : aSaveable
    {
        public string Query
        {
            get => this.GetValue(() => Query);
            set => this.SetValue(() => Query, value);
        }
        public Project Project
        {
            get => this.GetValue(() => Project);
            set
            {
                if (Project == null && value == null) return;
                if (Project?.Equals(value) == true) return;
                this.SetValue(() => Project, value);
                _ea.GetEvent<ProjectSelectedChanged>().Publish(Project);
            }
        }
        public Repository Repository
        {
            get => this.GetValue(() => Repository);
            set
            {
                if (Repository == null && value == null) return;
                if (Repository?.Equals(value) == true) return;
                this.SetValue(() => Repository, value);
                _ea.GetEvent<RepositorySelectedChanged>().Publish(Repository);
            }
        }
        public Branch Branch
        {
            get => this.GetValue(() => Branch);
            set
            {
                if (Branch == null && value == null) return;
                if (Branch?.Equals(value) == true) return;
                this.SetValue(() => Branch, value);
                _ea.GetEvent<BranchSelectedChanged>().Publish(Branch);
            }
        }
        public Tag LatestTag
        {
            get => this.GetValue(() => LatestTag);
            set => this.SetValue(() => LatestTag, value);
        }
        public string NextTag
        {
            get => this.GetValue(() => NextTag);
            set => this.SetValue(() => NextTag, value);
        }
        public void Update(Project source)
        {
            Project.CopyProperties(source);
        }
        public void Update(Repository source)
        {
            Repository.CopyProperties(source);
        }
        public void Update(Branch source)
        {
            Branch.CopyProperties(source);
        }
        public void Update(Tag source)
        {
            LatestTag.CopyProperties(source);
        }

        public bool CanGetNextTag()
        {
            return Project!=null && Branch!=null && Repository!=null;
        }
        public BitBucketTask() : this(null)
        {
        }
        IEventAggregator _ea;
        public BitBucketTask(IEventAggregator ea)
        {
            _ea = ea;
        }
    }
}
