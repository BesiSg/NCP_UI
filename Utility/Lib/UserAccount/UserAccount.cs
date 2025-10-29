namespace Utility.Lib.UserAccount
{
    [Serializable]
    public class UserAccount : aSaveable
    {
        public string BBHTMLToken
        {
            get => this.GetValue(() => this.BBHTMLToken);
            set => this.SetValue(() => this.BBHTMLToken, value);
        }
    }
}
