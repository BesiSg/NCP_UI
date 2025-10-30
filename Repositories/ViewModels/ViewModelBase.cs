﻿using BitBucketHandler;
using System.Collections.ObjectModel;
using Utility;
using Utility.EventAggregator;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace RepositoriesModule.ViewModels
{
    public abstract class ViewModelBase<Tbase> : BaseUtility
        where Tbase : aSaveable
    {
        public ObservableCollection<Tbase> Content { get; set; } = new ObservableCollection<Tbase>();
        protected SettingHandler<UserAccount> UserAccountHandler;
        protected SettingHandler<BitBucketStorage<Tbase>> DatasetHandler;
        public DelegateCommand ImportDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
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

        public bool AutoUpdateEnabled
        {
            get => this.GetValue(() => this.AutoUpdateEnabled);
            set => this.SetValue(() => this.AutoUpdateEnabled, value);
        }

        protected iBitBucketHandler _handler;
        protected IEventAggregator _ea;
        public ViewModelBase(IEventAggregator ea, SettingHandler<BitBucketStorage<Tbase>> datasetcfg, SettingHandler<UserAccount> userAccHandler)
        {
            _ea = ea;
            UserAccountHandler = userAccHandler;
            DatasetHandler = datasetcfg;
            DatasetHandler.Get.Changed += Changed;
            ImportDataCommand = new DelegateCommand(async () => await ImportDataAsync());
            SaveDataCommand = new DelegateCommand(() => SaveData());
            UpdateCollection();
            _ea.GetEvent<ProjectSelectedChanged>().Subscribe(ProjectReceived);
            _ea.GetEvent<RepositorySelectedChanged>().Subscribe(RepositoryReceived);
            _ea.GetEvent<BranchSelectedChanged>().Subscribe(BranchReceived);
        }

        private void Changed(object? sender, List<Tbase> e)
        {
            UpdateCollection();
        }

        protected abstract void UpdateCollection();
        protected abstract void BranchReceived(Branch message);
        protected abstract void ProjectReceived(Project message);
        protected abstract void RepositoryReceived(Repository message);
        protected abstract void SaveData();
        protected abstract Task ImportDataAsync();
        protected abstract void PublishEvent(Tbase selected);
    }
}
