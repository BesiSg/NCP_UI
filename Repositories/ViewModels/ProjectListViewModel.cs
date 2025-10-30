using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class ProjectListViewModel : ViewModelBase<Project>
    {
        public ProjectListViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Project>> dataset, SettingHandler<UserAccount> userAccHandler) : base(ea, dataset, userAccHandler)
        {
            _handler = new BitBucketProjectHandler(UserAccountHandler.Get.BBHTMLToken, dataset.Get);
        }

        protected override void UpdateCollection()
        {
            Content.Clear();
            if (!DatasetHandler.Get.Storage.ContainsKey("0")) return;
            Content.AddRange(DatasetHandler.Get.Storage["0"]);
        }

        protected override void BranchReceived(Branch message)
        {
        }

        protected override void ProjectReceived(Project message)
        {
            Selected = project;
        }

        protected override void RepositoryReceived(Repository message)
        {
        }

        protected override void SaveData()
        {
            DatasetHandler.Save();
        }

        protected override async Task ImportDataAsync()
        {
            await _handler.GetAllAsync("0");
        }

        protected override void PublishEvent(Project selected)
        {
            _ea.GetEvent<ProjectSelectedChanged>().Publish(selected);
        }
    }
}
