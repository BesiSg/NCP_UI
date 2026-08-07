using NewSystemCreationHelperModule.Views;

namespace NewSystemCreationHelperModule
{
    public class NewSystemCreationHelperModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("NewSysCreateRegion", typeof(NewSystemHelperPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}