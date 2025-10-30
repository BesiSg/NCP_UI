using BitBucket;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class TaglistViewModel : ViewModelBase<Commit>
    {
        public TaglistViewModel(IEventAggregator ea, SettingHandler<BitBucketStorage<Commit>> datasetcfg, SettingHandler<UserAccount> userAccHandler) : base(ea, datasetcfg, userAccHandler)
        {
            _handler = new BitBucketTagHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get);
        }

        protected override void UpdateCollection()
        {
            Content.Clear();
            if (project == null || repository == null || branch == null || !DatasetHandler.Get.Storage.ContainsKey($"{project?.key} {repository?.slug} {branch?.displayId}")) return;
            Content.AddRange(DatasetHandler.Get.Storage[$"{project?.key} {repository?.slug} {branch.displayId}"].ToList());
        }

        protected override async void BranchReceived(Branch message)
        {
            branch = message;
            if (AutoUpdateEnabled) await ImportDataAsync();
            UpdateCollection();
        }

        protected override void ProjectReceived(Project message)
        {
            project = message;
            UpdateCollection();
        }

        protected override void RepositoryReceived(Repository message)
        {
            repository = message;
            UpdateCollection();
        }

        protected override void SaveData()
        {
            DatasetHandler.Save();
        }

        protected override async Task ImportDataAsync()
        {
            if (project == null || repository == null || branch == null) return;
            await _handler.GetAllAsync($"{project.key} {repository.slug} {branch.displayId}");
        }

        protected override void PublishEvent(Commit selected)
        {
            _ea.GetEvent<CommitSelectedChanged>().Publish(selected);
        }
    }
}
