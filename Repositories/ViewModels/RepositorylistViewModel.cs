using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class RepositorylistViewModel : ViewModelBase<Repository>
    {
        public RepositorylistViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Repository>> datasetcfg, SettingHandler<UserAccount> userAccHandler) : base(ea, datasetcfg, userAccHandler)
        {
            _handler = new BitBucketRepositoryHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get);
        }

        protected override void UpdateCollection()
        {
            Content.Clear();
            if (project == null || !DatasetHandler.Get.Storage.ContainsKey(project?.key)) return;
            Content.AddRange(DatasetHandler.Get.Storage[project.key].ToList());
        }

        protected override void BranchReceived(Branch message)
        {
        }

        protected override async void ProjectReceived(Project message)
        {
            project = message;
            if (AutoUpdateEnabled) await ImportDataAsync();
            UpdateCollection();
        }

        protected override void RepositoryReceived(Repository message)
        {
            Selected = repository;
        }

        protected override void SaveData()
        {
            DatasetHandler.Save();
        }

        protected override async Task ImportDataAsync()
        {
            if (project == null) return;
            await _handler.GetAllAsync(project.key);
        }

        protected override void PublishEvent(Repository selected)
        {
            _ea.GetEvent<RepositorySelectedChanged>().Publish(Selected);
        }
    }
}
