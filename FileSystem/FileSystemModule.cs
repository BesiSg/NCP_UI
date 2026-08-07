using FileSystem.Views;

namespace FileSystem
{
    public class FileSystemModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("FilesystemRegion", typeof(FileSystemViewer));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}