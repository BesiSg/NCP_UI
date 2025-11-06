namespace Utility.Lib.UserAccount
{
    [Serializable]
    public class UserAccount : BaseUtility
    {
        public string BBHTMLToken
        {
            get => this.GetValue(() => this.BBHTMLToken);
            set => this.SetValue(() => this.BBHTMLToken, value);
        }
    }
}
