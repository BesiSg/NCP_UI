namespace Utility.Lib.UserAccount
{
    [Serializable]
    public class UserAccount : BaseUtility
    {
        public string Email
        {
            get => this.GetValue(() => this.Email);
            set => this.SetValue(() => this.Email, value);
        }
        public string BBHTMLToken
        {
            get => this.GetValue(() => this.BBHTMLToken);
            set => this.SetValue(() => this.BBHTMLToken, value);
        }
        public string JiraPAToken
        {
            get => this.GetValue(() => this.JiraPAToken);
            set => this.SetValue(() => this.JiraPAToken, value);
        }
    }
}
