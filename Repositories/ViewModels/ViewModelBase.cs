using BitBucketHandler;
using System.Collections.ObjectModel;
using System.Windows;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.Storage;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public abstract class ViewModelBase<Tbase> : ViewModelBase
        where Tbase : BaseUtility
    {
        public List<Tbase> FullContent { get; set; } = new List<Tbase>();
        public ObservableCollection<Tbase> Content { get; set; } = new ObservableCollection<Tbase>();
        protected SettingHandler<UserAccount> UserAccountHandler;
        protected SettingHandler<StorageDatList<Tbase>> DatasetHandler;
        public AsyncDelegateCommand ImportDataCommand { get; protected set; }
        public AsyncDelegateCommand SaveDataCommand { get; protected set; }
        public AsyncDelegateCommand SearchCommand { get; protected set; }
        public DelegateCommand<Tbase> OpenCommand { get; protected set; }
        protected Project project = null;
        protected Repository repository = null;
        protected Branch branch = null;

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
        public string SearchText
        {
            get => this.GetValue(() => this.SearchText);
            set => this.SetValue(() => this.SearchText, value);
        }

        public bool AutoUpdateEnabled
        {
            get => this.GetValue(() => this.AutoUpdateEnabled);
            set => this.SetValue(() => this.AutoUpdateEnabled, value);
        }
        public bool RightClickOption //true: browse, false:commits
        {
            get => this.GetValue(() => this.RightClickOption);
            set => this.SetValue(() => this.RightClickOption, value);
        }
        protected iBitBucketHandler _handler;
        protected IEventAggregator _ea;
        public ViewModelBase(IEventAggregator ea, SettingHandler<StorageDatList<Tbase>> datasetcfg, SettingHandler<UserAccount> userAccHandler)
        {
            _ea = ea;
            AutoUpdateEnabled = true;
            UserAccountHandler = userAccHandler;
            DatasetHandler = datasetcfg;
            DatasetHandler.Get.Changed += Changed;
            UpdateCollection();
            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(ProjectReceived);
            _ea.GetEvent<RepositorySelectedChanged>().Subscribe(RepositoryReceived);
            _ea.GetEvent<BranchSelectedChanged>().Subscribe(BranchReceived);
        }

        private void Changed(object? sender, List<Tbase> e)
        {
            UpdateCollection();
        }

        protected void UpdateCollection()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SearchText = string.Empty;
                FullContent.Clear();
                Content.Clear();
                if (SkipUpdate()) return;
                UpdateBothList(GetDisplayData());
            });

        }
        protected void UpdateBothList(List<Tbase> source)
        {
            FullContent.AddRange(source);
            Content.AddRange(source);
        }
        protected void UpdateDisplay(List<Tbase> source)
        {
            Content.Clear();
            Content.AddRange(source);
        }
        protected abstract void BranchReceived(Branch message);
        protected abstract void ProjectReceived(Project message);
        protected abstract void RepositoryReceived(Repository message);
        protected abstract Task SaveData();
        protected abstract Task ImportData();
        protected abstract Task Search();
        protected abstract void Open(Tbase source);
        protected abstract void PublishEvent(Tbase selected);
        protected abstract bool SkipUpdate();
        protected abstract List<Tbase> GetDisplayData();
    }
}
