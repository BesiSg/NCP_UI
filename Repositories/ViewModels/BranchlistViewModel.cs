using BitBucket;
using System.Diagnostics;
using System.Net.Http;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class BranchlistViewModel : ViewModelBase<Branch>
    {
        public BranchlistViewModel(IEventAggregator ea, SettingHandler<StorageDat<Branch>> datasetcfg, SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(ea, datasetcfg, userAccHandler)
        {
            _handler = new BitBucketBranchHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get, client);
        }

        protected override void BranchReceived(Branch message)
        {
            if (message == null) return;
            Selected = message;
        }

        protected override void ProjectReceived(Project message)
        {
            Selected = null;
            project = message;
            UpdateCollection();
        }

        protected override async void RepositoryReceived(Repository message)
        {
            Selected = null;
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

        protected override bool SkipUpdate()
        {
            return project == null || repository == null || !DatasetHandler.Get.Storage.ContainsKey($"{project?.key} {repository?.slug}");
        }

        protected override List<Branch> GetDisplayData()
        {
            return DatasetHandler.Get.Storage[$"{project?.key} {repository?.slug}"].OrderBy(item => item.displayId).ToList();
        }

        protected override void Open(Branch source)
        {
            if (source == null)
                return;
            var browsecommit = RightClickOption ? "browse?at=refs%2Fheads%2F" : "commits?until=refs%2Fheads%2F";
            var branch = source.displayId.Replace("/", "%2F");
            var url = $"http://bb.eu.besi.corp/projects/{project.key}/repos/{repository.slug}/{browsecommit}{branch}";
        //http://bb.eu.besi.corp/projects/NCP/repos/units_pr3n/browse?at=refs%2Fheads%2Fsupport%2Fbedu%2Fv2.14.9
        //http://bb.eu.besi.corp/projects/NCP/repos/units_pr3n/commits?until=refs%2Fheads%2Fsupport%2Fbedu%2Fv2.14.9
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        protected override async Task Search()
        {
            var filtered = await Task.Run(() => FullContent.Where(content => content.displayId.Contains(SearchText)).ToList());
            UpdateDisplay(filtered);
        }
    }
}
