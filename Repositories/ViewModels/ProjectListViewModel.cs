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
    public class ProjectListViewModel : ViewModelBase<Project>
    {
        public ProjectListViewModel(IEventAggregator ea, SettingHandler<StorageDatList<Project>> dataset, SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(ea, dataset, userAccHandler)
        {
            _handler = new BitBucketProjectHandler(UserAccountHandler.Get.BBHTMLToken, dataset.Get, client);
        }

        protected override void BranchReceived(Branch message)
        {
            return;
        }

        protected override void ProjectReceived(Project message)
        {
            Selected = message;
        }

        protected override void RepositoryReceived(Utility.Lib.BitBucketRepositories.Repository message)
        {
            return;
        }

        protected override async Task SaveData()
        {
            await Task.Run(() => DatasetHandler.Save());
        }

        protected override async Task ImportData()
        {
            await _handler.GetAllAsync("0");
        }

        protected override void PublishEvent(Project selected)
        {
            _ea.GetEvent<ProjectSelectedChanged>().Publish(selected);
        }

        protected override bool SkipUpdate()
        {
            return !DatasetHandler.Get.Storage.ContainsKey("0");
        }

        protected override List<Project> GetDisplayData()
        {
            return DatasetHandler.Get.Storage["0"].OrderBy(item => item.key).ToList();
        }

        protected override void Open(Project source)
        {
            if (source == null)
                return;

            var url = $"http://bb.eu.besi.corp/projects/{source.key}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        protected override async Task Search()
        {
            var filtered = await Task.Run(() => FullContent.Where(content => content.key.Contains(SearchText)).ToList());
            UpdateDisplay(filtered);
        }
    }
}
