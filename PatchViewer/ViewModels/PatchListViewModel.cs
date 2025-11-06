using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;
using Utility.PatchSync;

namespace PatchViewerModule.ViewModels
{
    public class PatchListViewModel : ViewModelBase<PatchSync>
    {
        public ObservableCollection<PatchSync> Content { get; private set; } = new ObservableCollection<PatchSync>();

        public AsyncDelegateCommand ImportDataCommand { get; protected set; }
        public AsyncDelegateCommand TransferandUnzipCommand { get; protected set; }
        public AsyncDelegateCommand PopulateCommand { get; protected set; }
        public AsyncDelegateCommand ValidateCommand { get; protected set; }
        public AsyncDelegateCommand GetUnitCFGInfoCommand { get; protected set; }
        public bool CanTransferandUnzip
        {
            get => GetValue(() => CanTransferandUnzip);
            set => SetValue(() => CanTransferandUnzip, value);
        }
        public bool CanPopulate
        {
            get => GetValue(() => CanPopulate);
            set => SetValue(() => CanPopulate, value);
        }
        public bool CanValidate
        {
            get => GetValue(() => CanValidate);
            set => SetValue(() => CanValidate, value);
        }
        public bool CanGetUnitCFGInfo
        {
            get => GetValue(() => CanGetUnitCFGInfo);
            set => SetValue(() => CanGetUnitCFGInfo, value);
        }

        public PatchListViewModel(IEventAggregator ea, SettingHandler<PatchFilesStorage> datasetcfg, SettingHandler<PatchForwardLookup> forwardlookup, SettingHandler<PatchReverseLookup> reverselookup) : base(ea, datasetcfg, forwardlookup, reverselookup)
        {
            Content.CollectionChanged += Content_CollectionChanged;
            fileStorage.GetKeys().ForEach(patchname => Content.Add(new PatchSync(NCPFileStructure.networkDevelop, NCPFileStructure.localDevelop, patchname, fileStorage, fwLookup, revLookup)));
            CanTransferandUnzip = true;
            CanPopulate = true;
            CanValidate = true;
            CanGetUnitCFGInfo = true;
        }

        private async Task GetUnitCFGInfo()
        {
            CanGetUnitCFGInfo = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(() => Task.Run(() => patchtask.GetUnitCfgInfo()));
            }
            var threads = new ThreadHandler(tasks, 3);
            await threads.RunTasks();
            CanGetUnitCFGInfo = true;
        }
        private async Task Populate()
        {
            CanPopulate = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(() => Task.Run(() => patchtask.PopulateFiles()));
            }
            var threads = new ThreadHandler(tasks, 3);
            await threads.RunTasks();
            CanPopulate = true;
        }
        private async Task Validate()
        {
            CanValidate = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(() => Task.Run(() => patchtask.Validate()));
            }
            var threads = new ThreadHandler(tasks, 3);
            await threads.RunTasks();
            CanValidate = true;
        }
        private async Task TransferandUnzip()
        {
            CanTransferandUnzip = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(() => Task.Run(() => patchtask.TransferandUnzip()));
            }
            var threads = new ThreadHandler(tasks, 3);
            await threads.RunTasks();
            CanTransferandUnzip = true;
        }
        private async Task ImportData()
        {
            await Task.Run(() =>
            {
                var files = NCPFileStructure.GetFiles(NCPFileStructure.networkDevelop);
                Content.Clear();
                foreach (var file in files)
                {
                    var filenamewithoutext = Path.GetFileNameWithoutExtension(file);
                    if (filenamewithoutext == null) continue;
                    if (!filenamewithoutext.Contains("patch")) continue;
                    var filename = Path.GetFileName(file);
                    Content.Add(new PatchSync(NCPFileStructure.networkDevelop, NCPFileStructure.localDevelop, filename, fileStorage, fwLookup, revLookup));
                }
            });
            CanTransferandUnzip = true;
        }

        private void Content_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (var item in e.NewItems)
            {
                var patch = item as PatchSync;
                patch?.CheckIfEmpty();
            }
        }
        protected override void PublishEvent(PatchSync selected)
        {
            _ea.GetEvent<PatchSyncSelectedChanged>().Publish(selected);
        }

        protected override async Task SaveData()
        {
            await Task.Run(() =>
            {
                fileStorageHandler.Save();
                fwLookupHandler.Save();
                revLookupHandler.Save();
            });
        }
    }
}
