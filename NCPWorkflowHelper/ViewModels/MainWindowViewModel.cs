using System.IO;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Datasheet;
using Utility.Lib.NewSystemWorkspace;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;
using Utility.SQL;

namespace NCPWorkflowHelper.ViewModels
{
    public class MainWindowViewModel : BaseUtility
    {
        public DelegateCommand SaveDataCommand { get; private set; }
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand OnClosingCommand { get; private set; }
        private SettingHandler<UserAccount> UserAccountCfgHandler { get; set; }
        private SettingHandler<StorageDat<Project>> ProjectDataHandler { get; set; }
        private SettingHandler<StorageDat<Branch>> BranchDataHandler { get; set; }
        private SettingHandler<StorageDat<Repository>> RepositoryDataHandler { get; set; }
        private SettingHandler<StorageDat<Commit>> CommitDataHandler { get; set; }
        private SettingHandler<StorageDat<Tag>> TagDataHandler { get; set; }
        private SettingHandler<NewSystemStorage> NewSystemHandler { get; set; }
        private SettingHandler<PatchFilesStorage> PatchDataHandler { get; set; }
        private SettingHandler<PatchForwardLookup> PatchForwardLUHandler { get; set; }
        private SettingHandler<PatchReverseLookup> PatchReverseLUHandler { get; set; }
        private SQLLib sQLLib { get; set; }
        string StartupPath = AppDomain.CurrentDomain.BaseDirectory;
        string UserAccount_Filename = "Data\\UserAcc.xml";
        string ProjectDataset_Filename = "Data\\ProjectDataset.xml";
        string RepositoryDataset_Filename = "Data\\RepositoryDataset.xml";
        string NewSystemDataset_Filename = "Data\\NewSystemDataset.xml";
        string CommitDataset_Filename = "Data\\CommitDataset.xml";
        string BranchDataset_Filename = "Data\\BranchDataset.xml";
        string TagDataset_Filename = "Data\\TagDataset.xml";
        string PatchDataset_Filename = "Data\\PatchDataset.xml";
        string PatchForward_Filename = "Data\\PatchForwardLookup.xml";
        string PatchReverse_Filename = "Data\\PatchReverseLookup.xml";
        public MainWindowViewModel(SettingHandler<UserAccount> useraccountHandler, SettingHandler<StorageDat<Project>> projectDataHandler, SettingHandler<StorageDat<Branch>> branchDataHandler, SettingHandler<StorageDat<Repository>> repositoryDataHandler, SettingHandler<StorageDat<Commit>> commitDataHandler, SettingHandler<StorageDat<Tag>> tagDataHandler, SettingHandler<NewSystemStorage> newsystemhandler, SettingHandler<PatchFilesStorage> patchDataHandler, SettingHandler<PatchForwardLookup> forwardlookup, SettingHandler<PatchReverseLookup> reverselookup, SQLLib sql)
        {
            UserAccountCfgHandler = useraccountHandler;
            ProjectDataHandler = projectDataHandler;
            BranchDataHandler = branchDataHandler;
            RepositoryDataHandler = repositoryDataHandler;
            CommitDataHandler = commitDataHandler;
            TagDataHandler = tagDataHandler;
            PatchDataHandler = patchDataHandler;
            PatchForwardLUHandler = forwardlookup;
            PatchReverseLUHandler = reverselookup;
            NewSystemHandler = newsystemhandler;

            UserAccountCfgHandler.SetPathnLoad(Path.Combine(StartupPath, UserAccount_Filename));
            ProjectDataHandler.SetPathnLoad(Path.Combine(StartupPath, ProjectDataset_Filename));
            RepositoryDataHandler.SetPathnLoad(Path.Combine(StartupPath, RepositoryDataset_Filename));
            CommitDataHandler.SetPathnLoad(Path.Combine(StartupPath, CommitDataset_Filename));
            BranchDataHandler.SetPathnLoad(Path.Combine(StartupPath, BranchDataset_Filename));
            TagDataHandler.SetPathnLoad(Path.Combine(StartupPath, TagDataset_Filename));
            PatchDataHandler.SetPathnLoad(Path.Combine(StartupPath, PatchDataset_Filename));
            PatchForwardLUHandler.SetPathnLoad(Path.Combine(StartupPath, PatchForward_Filename));
            PatchReverseLUHandler.SetPathnLoad(Path.Combine(StartupPath, PatchReverse_Filename));
            PatchReverseLUHandler.SetPathnLoad(Path.Combine(StartupPath, PatchReverse_Filename));
            NewSystemHandler.SetPathnLoad(Path.Combine(StartupPath, NewSystemDataset_Filename));

            sQLLib = new SQLLib();
            sQLLib.CreateTables();
            SaveDataCommand = new DelegateCommand(() => SaveData());
            ImportDataCommand = new DelegateCommand(() => ImportData());
            OnClosingCommand = new DelegateCommand(() => SaveData());
        }
        private void SaveData()
        {
            UserAccountCfgHandler.Save();
            ProjectDataHandler.Save();
            BranchDataHandler.Save();
            RepositoryDataHandler.Save();
            CommitDataHandler.Save();
            TagDataHandler.Save();
            PatchDataHandler.Save();
            NewSystemHandler.Save();
        }
        private void ImportData()
        {
            //this.issueListViewModel.DatasetCfg.UpdatefromJira();
        }
    }
}
