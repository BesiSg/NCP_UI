using NCPWorkflowHelper.Views;
using System.Windows;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace NCPWorkflowHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public static MainWindow _MainWindow;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<SettingHandler<BitBucketStorage<Project>>>();
            containerRegistry.RegisterSingleton<SettingHandler<BitBucketStorage<Branch>>>();
            containerRegistry.RegisterSingleton<SettingHandler<BitBucketStorage<Repository>>>();
            containerRegistry.RegisterSingleton<SettingHandler<BitBucketStorage<Commit>>>();
            containerRegistry.RegisterSingleton<SettingHandler<BitBucketStorage<Tag>>>();
            containerRegistry.RegisterSingleton<SettingHandler<UserAccount>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchFilesStorage>>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<RepositoriesModule.RepositoriesModule>();
            moduleCatalog.AddModule<UserAccountModule.UserAccountModule>();
            moduleCatalog.AddModule<PatchViewerModule.PatchViewerModule>();
        }
    }

}
