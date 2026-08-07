using FileSystem.Services;
using Microsoft.Extensions.DependencyInjection;
using NCPWorkflowHelper.Views;
using System.Net.Http;
using System.Windows;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Datasheet;
using Utility.Lib.NewSystemWorkspace;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;
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
            containerRegistry.RegisterSingleton<SettingHandler<StorageDat<Project>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDat<Branch>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDat<Repository>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDat<Commit>>>();
            containerRegistry.RegisterSingleton<SettingHandler<StorageDat<Tag>>>();
            containerRegistry.RegisterSingleton<SettingHandler<UserAccount>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchFilesStorage>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchForwardLookup>>();
            containerRegistry.RegisterSingleton<SettingHandler<PatchReverseLookup>>();
            containerRegistry.RegisterSingleton<SettingHandler<NewSystemStorage>>();
            containerRegistry.RegisterSingleton<SQLLib>();
            containerRegistry.RegisterSingleton<HttpClient>();
            containerRegistry.RegisterSingleton<IFileSystemWatchService, FileSystemWatchService>();
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
