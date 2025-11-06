using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class BranchlistViewModel : ViewModelBase<Branch>
    {
        public BranchlistViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Branch>> datasetcfg, SettingHandler<UserAccount> userAccHandler) : base(ea, datasetcfg, userAccHandler)
        {
            _handler = new BitBucketBranchHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get);
        }

        protected override void UpdateCollection()
        {
            Content.Clear();
            if (project == null || repository == null || !DatasetHandler.Get.Storage.ContainsKey($"{project?.key} {repository?.slug}")) return;
            Content.AddRange(DatasetHandler.Get.Storage[$"{project?.key} {repository?.slug}"].ToList());
        }

        protected override void BranchReceived(Branch message)
        {
            Selected = branch;
        }

        protected override void ProjectReceived(Project message)
        {
            project = message;
            UpdateCollection();
        }

        protected override async void RepositoryReceived(Repository message)
        {
            repository = message;
            if (AutoUpdateEnabled) await ImportData();
            UpdateCollection();
        }

        protected override async Task SaveData()
        {
            await Task.Run(() => DatasetHandler.Save());
        }

        protected override async Task ImportData()
        {
            if (project == null || repository == null) return;
            await _handler.GetAllAsync($"{project.key} {repository.slug}");
        }

        protected override void PublishEvent(Branch selected)
        {
            _ea.GetEvent<BranchSelectedChanged>().Publish(selected);
        }
    }
}
