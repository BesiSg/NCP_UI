using RepositoriesModule.Views;

namespace RepositoriesModule
{
    public class RepositoriesModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("FetchBranchRegion", typeof(RepoPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }

}
