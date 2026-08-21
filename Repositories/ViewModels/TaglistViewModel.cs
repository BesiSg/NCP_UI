using BitBucket;
using System.Net.Http;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class TaglistViewModel : ViewModelBase<Tag>
    {
        public TaglistViewModel(IEventAggregator ea, SettingHandler<StorageDatList<Tag>> datasetcfg, SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(ea, datasetcfg, userAccHandler)
        {
            _handler = new BitBucketTagHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get, client);
        }

        protected override async void BranchReceived(Branch message)
        {
            branch = message;
            if (AutoUpdateEnabled) await ImportData();
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

        protected override async Task SaveData()
        {
            await Task.Run(() => DatasetHandler.Save());
        }

        protected override async Task ImportData()
        {
            if (project == null || repository == null || branch == null) return;
            await _handler.GetAllAsync($"{project.key} {repository.slug} {branch.displayId}");
        }

        protected override void PublishEvent(Tag selected)
        {
            _ea.GetEvent<TagSelectedChanged>().Publish(selected);
        }

        protected override bool SkipUpdate()
        {
            return project == null || repository == null || branch == null || !DatasetHandler.Get.Storage.ContainsKey($"{project?.key} {repository?.slug} {branch?.displayId}");
        }

        protected override List<Tag> GetDisplayData()
        {
            return DatasetHandler.Get.Storage[$"{project?.key} {repository?.slug} {branch.displayId}"].OrderBy(item => item.displayId).ToList();
        }

        protected override void Open(Tag source)
        {
            throw new NotImplementedException();
        }

        protected override Task Search()
        {
            throw new NotImplementedException();
        }
    }
}
