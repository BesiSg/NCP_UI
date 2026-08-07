using BitBucket;
using JiraWrapper;
using NewSystemCreationHelperModule.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using SystemDatasheetHandler;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Datasheet;
using Utility.Lib.NewSystemWorkspace;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;

namespace NewSystemCreationHelperModule.ViewModels
{
    public class DragnDropDatasheetViewModel : ViewModelBase
    {
        private SettingHandler<NewSystemStorage> _newsystemHandler;
        private List<WorkspaceHandler> _workSpaces = new List<WorkspaceHandler>();
        public ObservableCollection<string> PdfFiles { get; } = new();
        public ObservableCollection<NewSystemWorkspacePaths> NewSystemWorkspacePath { get; } = new();
        public ObservableCollection<Datasheetproperties> Properties { get; } = new ObservableCollection<Datasheetproperties>();
        private Dictionary<string, DatasheetHelper> Datasheethelper = new Dictionary<string, DatasheetHelper>();
        public AsyncDelegateCommand<string[]> FilesDroppedCommand { get; protected set; }
        public AsyncDelegateCommand GenerateWorkspaceCommand { get; protected set; }
        public AsyncDelegateCommand FetchFromPDFCommand { get; protected set; }
        public DelegateCommand ClearListCommand { get; protected set; }
        public DelegateCommand<string> OpenFolderCommand { get; protected set; }
        public DelegateCommand<string> OpenFileCommand { get; protected set; }
        protected SettingHandler<UserAccount> UserAccountHandler;
        private BitBucketRestAPI _bbHandler;
        private AtlassianJira _jira;
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string _limitposURL = "&limit=1000";
        public DragnDropDatasheetViewModel(IEventAggregator ea, SettingHandler<UserAccount> userAccHandler, SettingHandler<NewSystemStorage> newsystemhandler, HttpClient httpclient)
        {
            UserAccountHandler = userAccHandler;
            _newsystemHandler = newsystemhandler;
            CanFilesDropped = true;
            CanFetchFromPDF = true;
            CanGenerateWorkspace = true;
            CanClearList = true;
            _bbHandler = new BitBucketRestAPI(_baseURL, _limitposURL, userAccHandler.Get.BBHTMLToken, httpclient);
            _jira = new AtlassianJira(userAccHandler.Get);
            _newsystemHandler.Get.Changed += Get_Changed;
            PopulateNewSystem();
        }

        private void Get_Changed(object? sender, NewSystemWorkspacePaths e)
        {
            PopulateNewSystem();
        }
        private void PopulateNewSystem()
        {
            NewSystemWorkspacePath.Clear();
            foreach (var newsystem in _newsystemHandler.Get.Storage.Values)
            {
                NewSystemWorkspacePath.Add(newsystem);
            }
        }
        public bool CanFilesDropped
        {
            get => GetValue(() => CanFilesDropped);
            set => SetValue(() => CanFilesDropped, value);
        }
        public bool CanGenerateWorkspace
        {
            get => GetValue(() => CanGenerateWorkspace);
            set => SetValue(() => CanGenerateWorkspace, value);
        }
        public bool CanClearList
        {
            get => GetValue(() => CanClearList);
            set => SetValue(() => CanClearList, value);
        }
        public bool CanFetchFromPDF
        {
            get => GetValue(() => CanFetchFromPDF);
            set => SetValue(() => CanFetchFromPDF, value);
        }
        private void ClearList()
        {
            PdfFiles.Clear();
            Properties.Clear();
        }
        private Task FilesDropped(string[] files)
        {

            if (files == null || files.Length == 0)
                return Task.CompletedTask;

            try
            {
                CanFilesDropped = false;
                var existingFileNames = new HashSet<string>(
                    PdfFiles.Select(Path.GetFileName),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var file in files)
                {
                    if (!file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var fileName = Path.GetFileName(file);

                    if (existingFileNames.Add(fileName))
                    {
                        PdfFiles.Add(file);
                    }
                }
            }
            finally
            {
                CanFilesDropped = true;
            }

            return Task.CompletedTask;

        }
        private void OpenFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!File.Exists(path))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        private void OpenFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!Directory.Exists(path))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }
        private async Task GenerateWorkspace()
        {
            try
            {
                CanClearList = false;
                CanGenerateWorkspace = false;
                var valid = true;
                foreach (var property in Properties)
                {
                    if (!property.Valid())
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid) throw new Exception("One or more invalid Datasheet property detected");
                var taskslist = new List<Task<bool>>();
                var result = await _bbHandler.GetRepositoriesOnProject(Utility.Constants.NCPC);
                var listofslugs = result.Select(repo => repo.name);
                foreach (var file in PdfFiles)
                {
                    if (!Datasheethelper.ContainsKey(file)) continue;
                    var handler = new WorkspaceHandler(Datasheethelper[file], _jira, _bbHandler, listofslugs);
                    _workSpaces.Add(handler);
                    taskslist.Add(handler.CreateWorkspace());
                }
                await Task.WhenAll(taskslist);
                foreach (var workspace in _workSpaces)
                {
                    _newsystemHandler.Get.Update(new NewSystemWorkspacePaths() { Paths = workspace.WorkspacePaths, Properties = workspace.Datasheetproperties }, workspace.Datasheetproperties.NCP);
                }
                PdfFiles.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                CanGenerateWorkspace = true; CanClearList = true;
            }


        }
        private async Task FetchFromPDF()
        {
            CanFetchFromPDF = false;
            CanClearList = false;
            var taskslist = new List<Task>();
            Datasheethelper.Clear();
            Properties.Clear();
            foreach (var file in PdfFiles)
            {
                var datasheethelper = new DatasheetHelper(file);
                Datasheethelper[file] = datasheethelper;
                taskslist.Add(datasheethelper.ExtractData());
            }
            await Task.WhenAll(taskslist);
            Datasheethelper.Values.ToList().ForEach(x => Properties.Add(x.Properties));
            CanFetchFromPDF = true;
            CanClearList = true;
        }
    }
}
