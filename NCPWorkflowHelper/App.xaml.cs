using FileSystem.Services;
using NCPWorkflowHelper.Views;
using System.Net.Http;
using System.Windows;
using Utility.Lib.AtlassianService;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.NewSystemWorkspace;
using Utility.Lib.PatchSync;
using Utility.Lib.Release;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;
using Utility.Lib.Wiki;
using Utility.SQL;

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
            containerRegistry.RegisterSingleton<SettingHandler<StorageDatList<Project>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDatList<Branch>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDatList<Repository>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDatList<Commit>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDatList<Tag>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDat<CustomerAbbreviation>>>();
            containerRegistry.RegisterSingleton<SettingHandler<UserAccount>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchFilesStorage>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchForwardLookup>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchReverseLookup>>();
            containerRegistry.RegisterSingleton<SettingHandler<NewSystemStorage>>();
            containerRegistry.RegisterSingleton<SettingHandler<ReleaseStorage>>();
            containerRegistry.RegisterSingleton<SQLLib>();
            containerRegistry.RegisterSingleton<HttpClient>();
            containerRegistry.RegisterSingleton<IFileSystemWatchService, FileSystemWatchService>();
            containerRegistry.RegisterSingleton<AtlassianService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<RepositoriesModule.RepositoriesModule>();
            moduleCatalog.AddModule<UserAccountModule.UserAccountModule>();
            moduleCatalog.AddModule<PatchViewerModule.PatchViewerModule>();
            moduleCatalog.AddModule<NewSystemCreationHelperModule.NewSystemCreationHelperModule>();
            moduleCatalog.AddModule<FileSystem.FileSystemModule>();
            moduleCatalog.AddModule<ReleaseOverallSyncHelper.ReleaseOverallSyncHelperModule>();
        }
    }

}
