using Utility;
using Utility.Lib.PatchSync;
using Utility.Lib.SettingHandler;

namespace PatchViewerModule.ViewModels
{
    public abstract class ViewModelBase<Tbase>(IEventAggregator ea, SettingHandler<PatchFilesStorage> datasetsource, SettingHandler<PatchForwardLookup> forwardlookup, SettingHandler<PatchReverseLookup> reverselookup) : ViewModelBase
        where Tbase : BaseUtility
    {
        protected IEventAggregator _ea = ea;
        protected PatchFilesStorage fileStorage = datasetsource.Get;
        protected PatchForwardLookup fwLookup = forwardlookup.Get;
        protected PatchReverseLookup revLookup = reverselookup.Get;
        protected SettingHandler<PatchFilesStorage> fileStorageHandler = datasetsource;
        protected SettingHandler<PatchForwardLookup> fwLookupHandler = forwardlookup;
        protected SettingHandler<PatchReverseLookup> revLookupHandler = reverselookup;
        protected abstract void PublishEvent(Tbase selected);
        protected abstract Task SaveData();

        public AsyncDelegateCommand SaveDataCommand { get; protected set; }
        public Tbase Selected
        {
            get => this.GetValue(() => this.Selected);
            set
            {
                if (Selected == null && value == null) return;
                if (Selected?.Equals(value) == true) return;
                this.SetValue(() => this.Selected, value);
                PublishEvent(Selected);
            }
        }
    }
}
