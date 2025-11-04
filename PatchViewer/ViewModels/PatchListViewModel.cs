using FilesHandler.Develop;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Utility;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;
using Utility.PatchSync;

namespace PatchViewerModule.ViewModels
{
    public class PatchListViewModel : ViewModelBase
    {
        public ObservableCollection<PatchSync> Content { get; private set; } = new ObservableCollection<PatchSync>();
        private SettingHandler<PatchFilesStorage> dataset;
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
        public DelegateCommand TransferandUnzipCommand { get; private set; }
        public DelegateCommand PopulateCommand { get; private set; }
        public DelegateCommand ValidateCommand { get; private set; }
        public bool CanTransferandUnzip
        {
            get => GetValue(() => CanTransferandUnzip);
            set
            {
                SetValue(() => CanTransferandUnzip, value);
                //TransferandUnzipCommand.RaiseCanExecuteChanged();
            }
        }
        public bool CanPopulate
        {
            get => GetValue(() => CanPopulate);
            set
            {
                SetValue(() => CanPopulate, value);
                //PopulateCommand.RaiseCanExecuteChanged();
            }
        }
        public bool CanValidate
        {
            get => GetValue(() => CanValidate);
            set
            {
                SetValue(() => CanValidate, value);
                //PopulateCommand.RaiseCanExecuteChanged();
            }
        }
        private bool CanTransferandUnzipCommand() => CanTransferandUnzip;
        private bool CanPopulateCommand() => CanPopulate;
        private bool CanValidateCommand() => CanValidate;
        public PatchListViewModel(IEventAggregator ea, SettingHandler<PatchFilesStorage> datasetcfg) : base(ea)
        {
            Content.CollectionChanged += Content_CollectionChanged;
            dataset = datasetcfg;
            dataset.Get.Storage.Keys.ToList().ForEach(patchname => Content.Add(new PatchSync(NCPFileStructure.networkDevelop, NCPFileStructure.localDevelop, patchname, dataset.Get)));
            ImportDataCommand = new DelegateCommand(() => ImportDataAsync());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            TransferandUnzipCommand = new DelegateCommand(() => TransferandUnzip(), ()=> CanTransferandUnzipCommand());
            PopulateCommand = new DelegateCommand(() => Populate(), () => CanPopulateCommand());
            ValidateCommand = new DelegateCommand(()=> ValidateAsync(), () => CanValidateCommand());
            CanTransferandUnzip = true;
            CanPopulate = true;
            CanValidate = true;
        }
        private async void Populate()
        {
            CanPopulate = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(async () =>
                {
                    await Task.Run(()=> patchtask.PopulateFiles()); // simulate async work
                });
            }
            await Util.RunTasksWithLimitedConcurrency(tasks, 3);
            CanPopulate = true;
        }
        private async void ValidateAsync()
        {
            CanValidate = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(async () =>
                {
                    await Task.Run(() => patchtask.Validate()); // simulate async work
                });
            }
            await Util.RunTasksWithLimitedConcurrency(tasks, 3);
            CanValidate = true;
        }

        private async void TransferandUnzip()
        {
            CanTransferandUnzip = false;
            var tasks = new List<Func<Task>>();
            foreach (var patchtask in Content)
            {
                tasks.Add(async () =>
                {
                    await patchtask.TransferandUnzip(); // simulate async work
                });
            }
            await Util.RunTasksWithLimitedConcurrency(tasks, 3);
            CanTransferandUnzip = true;
        }

        private void Content_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (var item in e.NewItems)
            {
                var patch = item as PatchSync;
                patch?.CheckIfEmpty();
            }
        }

        private void SaveData()
        {
            dataset.Save();
        }
        private void ImportDataAsync()
        {
            var files = NCPFileStructure.GetFiles(NCPFileStructure.networkDevelop);
            Content.Clear();
            foreach (var file in files)
            {
                var filenamewithoutext = Path.GetFileNameWithoutExtension(file);
                if (filenamewithoutext == null) continue;
                if (!filenamewithoutext.Contains("patch")) continue;
                var filename = Path.GetFileName(file);
                Content.Add(new PatchSync(NCPFileStructure.networkDevelop, NCPFileStructure.localDevelop, filename, dataset.Get));
            }
        }

    }
}
