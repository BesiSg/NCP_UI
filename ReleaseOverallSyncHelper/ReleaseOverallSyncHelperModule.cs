
using ReleaseOverallSyncHelper.Views;

namespace ReleaseOverallSyncHelper
{
    public class ReleaseOverallSyncHelperModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ReleaseOverAllHelperRegion", typeof(ReleaseOverallPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
