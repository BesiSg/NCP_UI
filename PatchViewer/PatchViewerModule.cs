using PatchViewerModule.Views;

namespace PatchViewerModule
{
    public class PatchViewerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("PatchViewerRegion", typeof(PatchViewerPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
