using Utility;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace UserAccountModule.ViewModels
{
    public class UserAccountFormViewModel : BaseUtility
    {
        public SettingHandler<UserAccount> userAccountHandler;
        public UserAccount UserAccount => userAccountHandler.Get;
        public UserAccountFormViewModel(SettingHandler<UserAccount> user)
        {
            this.userAccountHandler = user;
        }
    }
}
