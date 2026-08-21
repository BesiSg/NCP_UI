namespace Utility.Lib.Workspace
{
    public class WorkspacePaths : BaseUtility
    {
        public string Workspace
        {
            get => GetValue(() => Workspace);
            set => SetValue(() => Workspace, value);
        }
        public string RefSysCFG
        {
            get => GetValue(() => RefSysCFG);
            set => SetValue(() => RefSysCFG, value);
        }
        public string Datasheet
        {
            get => GetValue(() => Datasheet);
            set => SetValue(() => Datasheet, value);
        }
        public string SysCfg
        {
            get => GetValue(() => SysCfg);
            set => SetValue(() => SysCfg, value);
        }
        public string License
        {
            get => GetValue(() => License);
            set => SetValue(() => License, value);
        }
        public WorkspacePaths(string _workspace, string _refSysCFG, string _datasheet, string _sysCfg, string _license)
        {
            Workspace = _workspace;
            RefSysCFG = _refSysCFG;
            Datasheet = _datasheet;
            SysCfg = _sysCfg;
            License = _license;
        }
        public WorkspacePaths() { }
        public void Copy(WorkspacePaths source)
        {
            Workspace = source.Workspace;
            RefSysCFG = source.RefSysCFG;
            Datasheet = source.Datasheet;
            SysCfg = source.SysCfg;
            License = source.License;
        }
    }
}
