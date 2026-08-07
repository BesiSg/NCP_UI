using Utility;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace UserAccountModule.ViewModels
{
    public class UserAccountFormViewModel : ViewModelBase
    {
        public AsyncDelegateCommand SaveDataCommand { get; protected set; }
        public SettingHandler<UserAccount> userAccountHandler;
        public UserAccount UserAccount => userAccountHandler.Get;
        public UserAccountFormViewModel(SettingHandler<UserAccount> user)
        {
            this.userAccountHandler = user;
        }
        protected async Task SaveData()
        {
            await Task.Run(() => userAccountHandler.Save());
        }


    }
}
