using BitBucket;
using System.Collections.ObjectModel;
using System.Net.Http;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketActive.Task;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class TaskFormViewModel : ViewModelBase
    {
        public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();
        public ObservableCollection<Repository> Repositories { get; set; } = new ObservableCollection<Repository>();
        public ObservableCollection<Branch> Branches { get; set; } = new ObservableCollection<Branch>();
        public BitBucketTask Form { get; private set; }
        private SettingHandler<UserAccount> UserAccountHandler;
        private SettingHandler<StorageDat<Tag>> TagDatasetHandler;
        private SettingHandler<StorageDat<Commit>> CommitDatasetHandler;
        private SettingHandler<StorageDat<Branch>> BranchDatasetHandler;
        private SettingHandler<StorageDat<Repository>> RepositoryDatasetHandler;
        private SettingHandler<StorageDat<Project>> ProjectDatasetHandler;
        public AsyncDelegateCommand GetNextTagCommand { get; protected set; }
        public AsyncDelegateCommand SaveDataCommand { get; protected set; }

        private string projectkey => "0";
        private string repositorykey => Form?.Project?.key;
        private string branchkey => $"{Form?.Project?.key} {Form?.Repository?.slug}";
        private string tagkey => $"{Form?.Project?.key} {Form?.Repository?.slug} {Form?.Branch?.displayId}";
        public Commit Selected
        {
            get => this.GetValue(() => this.Selected);
            set
            {
                if (Selected?.Equals(value) == true) return;
                this.SetValue(() => this.Selected, value);
                _ea.GetEvent<CommitSelectedChanged>().Publish(Selected);
            }
        }
        private BitBucketTagHandler _tagHandler;

        IEventAggregator _ea;
        public TaskFormViewModel(IEventAggregator ea, SettingHandler<StorageDat<Tag>> tag, SettingHandler<StorageDat<Commit>> commit, SettingHandler<StorageDat<Branch>> branch, SettingHandler<StorageDat<Repository>> repository, SettingHandler<StorageDat<Project>> project, SettingHandler<UserAccount> userAccHandler, HttpClient client)
        {
            _ea = ea;
            Form = new BitBucketTask(ea);
            UserAccountHandler = userAccHandler;
            TagDatasetHandler = tag;
            CommitDatasetHandler = commit;
            BranchDatasetHandler = branch;
            RepositoryDatasetHandler = repository;
            ProjectDatasetHandler = project;

            TagDatasetHandler.Get.Changed += TagChanged;
            BranchDatasetHandler.Get.Changed += BranchChanged;
            RepositoryDatasetHandler.Get.Changed += RepositoryChanged;
            ProjectDatasetHandler.Get.Changed += ProjectChanged;

            UpdateProject();
            UpdateRepository();
            UpdateBranch();

            _tagHandler = new BitBucketTagHandler(UserAccountHandler.Get.BBHTMLToken, CommitDatasetHandler.Get, client);

            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(ProjectReceived);
            _ea.GetEvent<RepositorySelectedChanged>().Subscribe(RepositoryReceived);
            _ea.GetEvent<BranchSelectedChanged>().Subscribe(BranchReceived);
        }
        private void ProjectChanged(object? sender, List<Project> e)
        {
            UpdateProject();
        }
        private void RepositoryChanged(object? sender, List<Repository> e)
        {
            UpdateRepository();
        }
        private void BranchChanged(object? sender, List<Branch> e)
        {
            UpdateBranch();
        }
        private void TagChanged(object? sender, List<Tag> e)
        {
        }

        private void UpdateProject()
        {
            Projects.Clear();
            if (!ProjectDatasetHandler.Get.Storage.ContainsKey(projectkey)) return;
            Projects.AddRange(ProjectDatasetHandler.Get.Storage[projectkey]);
        }

        private void UpdateRepository()
        {
            Repositories.Clear();
            if (Form?.Project == null || !RepositoryDatasetHandler.Get.Storage.ContainsKey(repositorykey)) return;
            Repositories.AddRange(RepositoryDatasetHandler.Get.Storage[repositorykey]);
        }
        private void UpdateBranch()
        {
            Branches.Clear();
            if (Form.Project == null || Form.Repository == null || !BranchDatasetHandler.Get.Storage.ContainsKey(branchkey)) return;
            Branches.AddRange(BranchDatasetHandler.Get.Storage[branchkey]);
        }
        private async void BranchReceived(Branch message)
        {
            if (Form.Branch == message) return;
            Form.Branch = message;
            await GetNextTag();
        }

        private void ProjectReceived(Project message)
        {
            Form.Project = message;
            UpdateRepository();
            UpdateBranch();
            ClearTags();
        }
        private void RepositoryReceived(Repository message)
        {
            Form.Repository = message;
            UpdateBranch();
            ClearTags();
        }
        private void ClearTags()
        {
            Form.LatestTag = null;
            Form.NextTag = string.Empty;
        }
        private async Task SaveData()
        {
            await Task.Run(() =>
            {
                TagDatasetHandler.Save();
                CommitDatasetHandler.Save();
            });
        }
        private async Task GetNextTag()
        {
            await Task.Run(() =>
            {
                var result = _tagHandler.GetLatestTagAndNext(Form.Project?.key, Form.Repository?.name, Form.Branch?.displayId);
                Form.LatestTag = result.Item1;
                Form.NextTag = result.Item2;
                Selected = result.Item1;
            });
        }
    }
}
