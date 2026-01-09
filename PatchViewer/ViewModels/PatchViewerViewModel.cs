using System.Collections.ObjectModel;
using Utility.EventAggregator;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;
using Utility.SQL;

namespace PatchViewerModule.ViewModels
{
    public class PatchViewerViewModel : ViewModelBase<PatchSync>
    {
        public ObservableCollection<string> Standard
        {
            get => GetValue(() => Standard);
            set => SetValue(() => Standard, value);
        }
        public ObservableCollection<string> Difference
        {
            get => GetValue(() => Difference);
            set => SetValue(() => Difference, value);
        }
        public ObservableCollection<string> UnitCFG
        {
            get => GetValue(() => UnitCFG);
            set => SetValue(() => UnitCFG, value);
        }
        public AsyncDelegateCommand TransferandUnzipCommand { get; protected set; }
        public AsyncDelegateCommand PopulateCommand { get; protected set; }
        public AsyncDelegateCommand ValidateCommand { get; protected set; }
        public AsyncDelegateCommand GetUnitCFGCommand { get; protected set; }
        public AsyncDelegateCommand OpenNetworkFolderCommand { get; protected set; }
        public AsyncDelegateCommand OpenLocalFolderCommand { get; protected set; }
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
        public bool CanGetUnitCFG
        {
            get => GetValue(() => CanGetUnitCFG);
            set => SetValue(() => CanGetUnitCFG, value);
        }
        public bool CanOpenNetworkFolderCommand
        {
            get => GetValue(() => CanOpenNetworkFolderCommand);
            set => SetValue(() => CanOpenNetworkFolderCommand, value);
        }
        public bool CanOpenLocalFolderCommand
        {
            get => GetValue(() => CanOpenLocalFolderCommand);
            set => SetValue(() => CanOpenLocalFolderCommand, value);
        }

        public PatchViewerViewModel(IEventAggregator ea, SettingHandler<PatchFilesStorage> datasetcfg, SettingHandler<PatchForwardLookup> forwardlookup, SettingHandler<PatchReverseLookup> revlookup, SQLLib sql) : base(ea, datasetcfg, forwardlookup, revlookup, sql)
        {
            CanTransferandUnzip = true;
            CanPopulate = true;
            CanValidate = true;
            CanGetUnitCFG = true;
            Standard = new ObservableCollection<string>();
            Difference = new ObservableCollection<string>();
            UnitCFG = new ObservableCollection<string>();
            _ea.GetEvent<PatchSyncSelectedChanged>().Subscribe(SelectedUpdated);
        }

        private void SelectedUpdated(PatchSync sync)
        {
            Unsubscribe();

            Selected = sync;
            Standard.Clear();
            Difference.Clear();
            UnitCFG.Clear();

            if (Selected != null)
            {
                UpdateAndSubscribe();
            }
        }

        private void PatchViewerViewModel_UnitCfgListChanged(object? sender, Utility.ListHandler<string> e)
        {
            UnitCFG.Clear();
            UnitCFG.AddRange(e);
        }

        private void PatchViewerViewModel_DifferenceListChanged(object? sender, Utility.ListHandler<string> e)
        {
            Difference.Clear();
            Difference.AddRange(e);
        }

        private void PatchViewerViewModel_StandardListChanged(object? sender, List<string> e)
        {
            Standard.Clear();
            Standard.AddRange(e);
        }
        private void Unsubscribe()
        {
            if (Selected != null)
            {
                if (fileStorage.ContainsKey(Selected.PatchNameWithoutExtension))
                {
                    var standardfiles = fileStorage.GetStandardFiles(Selected.PatchNameWithoutExtension);
                    standardfiles.ListChanged -= PatchViewerViewModel_StandardListChanged;
                    var differencefiles = fileStorage.GetDifferentFiles(Selected.PatchNameWithoutExtension);
                    differencefiles.ListChanged -= PatchViewerViewModel_DifferenceListChanged;
                }
                if (fwLookup.ContainsKey(Selected.PatchNameWithoutExtension))
                {
                    var unitcfgversions = fwLookup[Selected.PatchNameWithoutExtension];
                    unitcfgversions.ListChanged -= PatchViewerViewModel_UnitCfgListChanged;
                }
            }
        }
        private void UpdateAndSubscribe()
        {
            if (Selected != null)
            {
                if (fileStorage.ContainsKey(Selected.PatchNameWithoutExtension))
                {
                    var standardfiles = fileStorage.GetStandardFiles(Selected.PatchNameWithoutExtension);
                    Standard.AddRange(standardfiles);
                    standardfiles.ListChanged += PatchViewerViewModel_StandardListChanged;

                    var differencefiles = fileStorage.GetDifferentFiles(Selected.PatchNameWithoutExtension);
                    Difference.AddRange(differencefiles);
                    differencefiles.ListChanged += PatchViewerViewModel_DifferenceListChanged;
                }
                if (fwLookup.ContainsKey(Selected.PatchNameWithoutExtension))
                {
                    var unitcfgversions = fwLookup[Selected.PatchNameWithoutExtension];
                    UnitCFG.AddRange(unitcfgversions);
                    unitcfgversions.ListChanged += PatchViewerViewModel_UnitCfgListChanged;
                }
            }
        }

        private async void Populate()
        {
            CanPopulate = false;
            await Task.Run(() => Selected?.PopulateFiles());
            CanPopulate = true;
        }
        private async void Validate()
        {
            CanValidate = false;
            await Task.Run(() => Selected?.Validate());
            CanValidate = true;
        }

        private async void TransferandUnzip()
        {
            CanTransferandUnzip = false;
            await Task.Run(() => Selected?.TransferandUnzip());
            CanTransferandUnzip = true;
        }

        private async void GetUnitCFG()
        {
            CanGetUnitCFG = false;
            await Task.Run(() => Selected?.GetUnitCfgInfo());
            CanGetUnitCFG = true;
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
