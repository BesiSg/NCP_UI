using System.Collections.ObjectModel;
using System.Net.Http;
using Utility;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public class ReleaseOverallPageViewModel : ViewModelBase
    {
        public ObservableCollection<ReleaseOverall> Collection = new ObservableCollection<ReleaseOverall>();
        public ReleaseOverallViewerViewModel _remote { get; }
        public ReleaseOverallViewerViewModel _local { get; }

        public ReleaseOverallPageViewModel(SettingHandler<UserAccount> userAccHandler, HttpClient client)
        {
            _remote = new ReleaseOverallViewerViewModel(@"L:\Develop", userAccHandler, client);
            _local = new ReleaseOverallViewerViewModel(@"P:\fngams\develop", userAccHandler, client);
        }
    }
}
