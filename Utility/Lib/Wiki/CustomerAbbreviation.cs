namespace Utility.Lib.Wiki
{
    public class CustomerAbbreviation : BaseUtility
    {
        public string Customer
        {
            get => GetValue(() => Customer);
            set => SetValue(() => Customer, value);
        }
        public string City
        {
            get => GetValue(() => City);
            set => SetValue(() => City, value);
        }
        public string Country
        {
            get => GetValue(() => Country);
            set => SetValue(() => Country, value);
        }
        public string Abbreviation
        {
            get => GetValue(() => Abbreviation);
            set => SetValue(() => Abbreviation, value);
        }
        public string PatchBranch
        {
            get => GetValue(() => PatchBranch);
            set => SetValue(() => PatchBranch, value);
        }
        public string Notes
        {
            get => GetValue(() => Notes);
            set => SetValue(() => Notes, value);
        }
    }
}
