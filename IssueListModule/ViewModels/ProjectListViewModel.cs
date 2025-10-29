using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class ProjectListViewModel : BaseUtility
    {
        public ObservableCollection<Project> Content { get; set; } = new ObservableCollection<Project>();
        private SettingHandler<UserAccount> UserAccountHandler;
        private SettingHandler<BitBucketStorage<Project>> DatasetHandler;
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
        IEventAggregator _ea;
        public Project Selected
        {
            get => this.GetValue(() => this.Selected);
            set
            {
                if (Selected == null && value == null) return;
                if (Selected?.Equals(value) == true) return;
                this.SetValue(() => this.Selected, value);
                _ea.GetEvent<ProjectSelectedChanged>().Publish(Selected);
            }
        }

        private BitBucketProjectHandler _handler;
        public ProjectListViewModel(IEventAggregator ea, SettingHandler<UserAccount> userAccHandler, SettingHandler<BitBucketStorage<Project>> dataset)
        {
            _ea = ea;
            UserAccountHandler = userAccHandler;
            DatasetHandler = dataset;
            _handler = new BitBucketProjectHandler(UserAccountHandler.Get.BBHTMLToken, dataset.Get);
            DatasetHandler.Get.Changed += Changed; ;
            ImportDataCommand = new DelegateCommand(() => ImportData());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            UpdateCollection();
            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(ProjectReceived);
        }

        private void ProjectReceived(Project project)
        {
            Selected=project;
        }

        private void Changed(object? sender, List<Project> e)
        {
            UpdateCollection();
        }

        private void SaveData()
        {
            DatasetHandler.Save();
        }

        private async void ImportData()
        {
            await _handler.SearchForProjectsAsync();
        }

        private void UpdateCollection()
        {
            Content.Clear();
            if (!DatasetHandler.Get.Storage.ContainsKey("0")) return;
            Content.AddRange(DatasetHandler.Get.Storage["0"]);
        }
    }
}
