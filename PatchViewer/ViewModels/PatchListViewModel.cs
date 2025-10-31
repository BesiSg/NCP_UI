using FilesHandler.Develop;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
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

        public PatchListViewModel(IEventAggregator ea, SettingHandler<PatchFilesStorage> datasetcfg) : base(ea)
        {
            Content.CollectionChanged += Content_CollectionChanged;
            dataset = datasetcfg;
            dataset.Get.Storage.Keys.ToList().ForEach(patchname => Content.Add(new PatchSync(NCPFileStructure.networkDevelop, NCPFileStructure.localDevelop, patchname, dataset.Get)));
            ImportDataCommand = new DelegateCommand(() => ImportDataAsync());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            TransferandUnzipCommand = new DelegateCommand(() => TransferandUnzip());
        }

        private async void TransferandUnzip()
        {
            //Content.ToList().ForEach(async patch => await patch.TransferandUnzip());
            await Content[0].TransferandUnzip();
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
