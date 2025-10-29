using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class RepositorylistViewModel : BaseUtility
    {
        public ObservableCollection<Repository> Content { get; set; } = new ObservableCollection<Repository>();
        private SettingHandler<UserAccount> UserAccountHandler;
        private SettingHandler<BitBucketStorage<Repository>> DatasetHandler;
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
        private Project project = null;
        public Repository Selected
        {
            get => this.GetValue(() => this.Selected);
            set
            {
                if (Selected == null && value == null) return;
                if (Selected?.Equals(value) == true) return;
                this.SetValue(() => this.Selected, value);
                _ea.GetEvent<RepositorySelectedChanged>().Publish(Selected);
            }
        }
        private BitBucketRepositoryHandler _handler;
        IEventAggregator _ea;
        public RepositorylistViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Repository>> datasetcfg, SettingHandler<UserAccount> userAccHandler)
        {
            _ea = ea;
            UserAccountHandler = userAccHandler;
            DatasetHandler = datasetcfg;
            _handler = new BitBucketRepositoryHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get);
            DatasetHandler.Get.Changed += Get_RepositoryChanged; ;
            ImportDataCommand = new DelegateCommand(() => ImportData());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            UpdateCollection();
            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(MessageReceived);
            _ea.GetEvent<RepositorySelectedChanged>().Subscribe(RepositoryReceived);
        }

        private void RepositoryReceived(Repository repository)
        {
            Selected=repository;
        }

        private void UpdateCollection()
        {
            Content.Clear();
            if (project == null || !DatasetHandler.Get.Storage.ContainsKey(project?.key)) return;
            Content.AddRange(DatasetHandler.Get.Storage[project.key].ToList());
        }
        private void Get_RepositoryChanged(object? sender, List<Repository> e)
        {
            UpdateCollection();
        }

        private void MessageReceived(Project message)
        {
            project = message;
            UpdateCollection();
        }
        private void SaveData()
        {
            DatasetHandler.Save();
        }

        private async void ImportData()
        {
            if (project == null) return;
            await _handler.SearchForRepositoriesAsync(project.key);
        }
    }
}
