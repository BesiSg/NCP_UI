using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class BranchlistViewModel : BaseUtility
    {
        public ObservableCollection<Branch> Content { get; set; } = new ObservableCollection<Branch>();
        private SettingHandler<UserAccount> UserAccountHandler;
        private SettingHandler<BitBucketStorage<Branch>> DatasetHandler;
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
        private Project project = null;
        private Repository repository = null;
        public Branch Selected
        {
            get => this.GetValue(() => this.Selected);
            set
            {
                if (Selected == null && value == null) return;
                if (Selected?.Equals(value) == true) return;
                this.SetValue(() => this.Selected, value);
                _ea.GetEvent<BranchSelectedChanged>().Publish(Selected);
            }
        }
        private BitBucketBranchHandler _handler;
        IEventAggregator _ea;
        public BranchlistViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Branch>> datasetcfg, SettingHandler<UserAccount> userAccHandler)
        {
            _ea = ea;
            UserAccountHandler = userAccHandler;
            DatasetHandler = datasetcfg;
            _handler = new BitBucketBranchHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get);
            DatasetHandler.Get.Changed += Changed;
            ImportDataCommand = new DelegateCommand(() => ImportData());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            UpdateCollection();
            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(ProjectReceived);
            _ea.GetEvent<RepositorySelectedChanged>().Subscribe(RepositoryReceived);
            _ea.GetEvent<BranchSelectedChanged>().Subscribe(BranchReceived);
        }

        private void BranchReceived(Branch branch)
        {
            Selected = branch;
        }

        private void Changed(object? sender, List<Branch> e)
        {
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            Content.Clear();
            if (project == null || repository == null || !DatasetHandler.Get.Storage.ContainsKey($"{project?.key} {repository?.slug}")) return;
            Content.AddRange(DatasetHandler.Get.Storage[$"{project?.key} {repository?.slug}"].ToList());
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
            DatasetHandler.Save();
        }

        private async void ImportData()
        {
            if (project == null || repository == null) return;
            await _handler.GetAllBranchesAsync(project.key, repository.slug);
        }
    }
}
