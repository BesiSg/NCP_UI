using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class TaglistViewModel : BaseUtility
    {
        public ObservableCollection<Tag> Content { get; set; } = new ObservableCollection<Tag>();
        private SettingHandler<UserAccount> UserAccountHandler;
        private SettingHandler<BitBucketStorage<Tag>> TagDatasetHandler;
        private SettingHandler<BitBucketStorage<Commit>> CommitDatasetHandler;
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
        private Project project = null;
        private Repository repository = null;
        private Branch branch = null;
        public Tag Selected
        {
            get => this.GetValue(() => this.Selected);
            set
            {
                if (Selected == null && value == null) return;
                if (Selected?.Equals(value) == true) return;
                this.SetValue(() => this.Selected, value);
                _ea.GetEvent<TagSelectedChanged>().Publish(Selected);
            }
        }
        private BitBucketTagHandler _handler;
        IEventAggregator _ea;
        public TaglistViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Tag>> datasetcfg, SettingHandler<BitBucketStorage<Commit>> datasetcommitcfg, SettingHandler<UserAccount> userAccHandler)
        {
            _ea = ea;
            UserAccountHandler = userAccHandler;
            TagDatasetHandler = datasetcfg;
            CommitDatasetHandler = datasetcommitcfg;
            _handler = new BitBucketTagHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get, datasetcommitcfg.Get);
            TagDatasetHandler.Get.Changed += Changed;
            CommitDatasetHandler.Get.Changed += CommitChanged;
            ImportDataCommand = new DelegateCommand(async () => await ImportDataAsync());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            UpdateCollection();
            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(ProjectReceived);
            _ea.GetEvent<RepositorySelectedChanged>().Subscribe(RepositoryReceived);
            _ea.GetEvent<BranchSelectedChanged>().Subscribe(BranchReceived);
        }

        private void CommitChanged(object? sender, List<Commit> e)
        {
            UpdateCollection();
        }

        private void Changed(object? sender, List<Tag> e)
        {
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            Content.Clear();
            if (project == null || repository == null || branch == null || !TagDatasetHandler.Get.Storage.ContainsKey($"{project?.key} {repository?.slug} {branch?.displayId}")) return;
            Content.AddRange(TagDatasetHandler.Get.Storage[$"{project?.key} {repository?.slug} {branch.displayId}"].ToList());
        }

        private async void BranchReceived(Branch message)
        {
            branch = message;
            await ImportDataAsync();
            UpdateCollection();
        }

        private void ProjectReceived(Project message)
        {
            project = message;
            UpdateCollection();
        }
        private void RepositoryReceived(Repository message)
        {
            repository = message;
            UpdateCollection();
        }
        private void SaveData()
        {
            TagDatasetHandler.Save();
            CommitDatasetHandler.Save();
        }

        private async Task ImportDataAsync()
        {
            if (project == null || repository == null || branch == null) return;
            await _handler.GetTags(project.key, repository.slug, branch.displayId);
        }
    }
}
