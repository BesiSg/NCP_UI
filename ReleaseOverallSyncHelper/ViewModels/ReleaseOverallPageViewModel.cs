using FileSearcher;
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
            var pattern = @"^R(?<Version>\d+\.\d+\.\d+)\.(?<Customer>[A-Za-z]+?)(?<Revision>\d+(?:-[A-Za-z0-9]+)?|-[A-Za-z0-9]+)\.cfg$";
            _remote = new ReleaseOverallViewerViewModel(@"L:\Develop", pattern, userAccHandler, client);
            _local = new ReleaseOverallViewerViewModel(@"P:\fngams\develop", pattern, userAccHandler, client);
        }
    }
}
