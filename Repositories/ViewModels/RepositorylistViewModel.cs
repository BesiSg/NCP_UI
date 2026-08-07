using BitBucket;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Controls.Primitives;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public class RepositorylistViewModel : ViewModelBase<Repository>
    {
        public RepositorylistViewModel(IEventAggregator ea, SettingHandler<StorageDat<Repository>> datasetcfg, SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(ea, datasetcfg, userAccHandler)
        {
            _handler = new BitBucketRepositoryHandler(UserAccountHandler.Get.BBHTMLToken, datasetcfg.Get, client);
        }

        protected override void BranchReceived(Branch message)
        {
            return;
        }

        protected override async void ProjectReceived(Project message)
        {
            Selected = null;
            project = message;
            if (AutoUpdateEnabled) await ImportData();
            UpdateCollection();
        }

        protected override void RepositoryReceived(Repository message)
        {
            if (message == null) return;
            Selected = message;
        }

        protected override async Task SaveData()
        {
            await Task.Run(() => DatasetHandler.Save());
        }

        protected override async Task ImportData()
        {
            if (project == null) return;
            await _handler.GetAllAsync(project.key);
        }

        protected override void PublishEvent(Repository selected)
        {
            _ea.GetEvent<RepositorySelectedChanged>().Publish(Selected);
        }

        protected override bool SkipUpdate()
        {
            return project == null || !DatasetHandler.Get.Storage.ContainsKey(project?.key);
        }

        protected override List<Repository> GetDisplayData()
        {
            return DatasetHandler.Get.Storage[project.key].OrderBy(item => item.slug).ToList();
        }

        protected override void Open(Repository source)
        {
            if (source == null)
                return;
            var browsecommit = RightClickOption ? "browse" : "commits";
            var url = $"http://bb.eu.besi.corp/projects/{project.key}/repos/{source.slug}/{browsecommit}";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        protected override async Task Search()
        {
            var filtered = await Task.Run(() => FullContent.Where(content => content.slug.Contains(SearchText)).ToList());
            UpdateDisplay(filtered);
        }
    }
}
