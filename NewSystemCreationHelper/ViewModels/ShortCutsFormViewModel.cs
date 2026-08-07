using System.Diagnostics;
using Utility;
using Utility.Lib.NewSystemWorkspace;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace NewSystemCreationHelperModule.ViewModels
{
    public class ShortCutsFormViewModel : ViewModelBase
    {
        public AsyncDelegateCommand SaveDataCommand { get; protected set; }
        public AsyncDelegateCommand RunAgileCommand { get; protected set; }

        protected SettingHandler<UserAccount> UserAccountHandler;
        protected SettingHandler<NewSystemStorage> NewSystemStorage;
        public ShortCutsFormViewModel(IEventAggregator ea, SettingHandler<UserAccount> userAccHandler, SettingHandler<NewSystemStorage> newsystemstorage)
        {
            UserAccountHandler = userAccHandler;
            NewSystemStorage = newsystemstorage;
            CanRunAgile = true;
        }
        public bool CanRunAgile
        {
            get => GetValue(() => CanRunAgile);
            set => SetValue(() => CanRunAgile, value);
        }
        protected async Task SaveData()
        {
            await Task.Run(() => NewSystemStorage.Save());
        }
        private Task RunAgile()
        {
            CanRunAgile = false;
            Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\ProgramData\Oracle\Agile\EDM\AgileClientLauncher.cmd",
                UseShellExecute = true
            });
            CanRunAgile = true;
            return Task.CompletedTask;
        }

    }
}
