using UserAccountModule.Views;

namespace UserAccountModule
{
    public class UserAccountModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("UserAccountRegion", typeof(UserAccountForm));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}
