using BitBucket;
using JiraWrapper;
using LicenseHandler;
using System.IO;
using System.Text.RegularExpressions;
using SystemDatasheetHandler;
using Utility.Lib.Datasheet;
using Utility.Lib.Workspace;

namespace NewSystemCreationHelperModule.Model
{
    public class WorkspaceHandler
    {
        private AtlassianJira _jiraHandler;
        private BitBucketRestAPI _bitbucketHandler;
        private DatasheetHelper _datasheetHelper;
        private IEnumerable<string> _listofRepos;
        public WorkspacePaths WorkspacePaths { get; private set; }
        public Datasheetproperties Datasheetproperties => _datasheetHelper.Properties;
        public WorkspaceHandler(DatasheetHelper datasheet, AtlassianJira jira, BitBucketRestAPI bbhandler, IEnumerable<string> listofrepos)
        {
            _jiraHandler = jira;
            _bitbucketHandler = bbhandler;
            _datasheetHelper = datasheet;
            _listofRepos = listofrepos;

        }
        public async Task<bool> CreateWorkspace()
        {
            try
            {
                WorkspacePaths = WorkspaceCreator.CreateWorkspace(_datasheetHelper.Properties);
                await _jiraHandler.GetPDFAttachment(_datasheetHelper.Properties.NCP, WorkspacePaths.License);
                var match = _listofRepos.FirstOrDefault(r =>
                    r.EndsWith($"_{_datasheetHelper.Properties.RefSystemSN}", StringComparison.OrdinalIgnoreCase));
                var secsgem = new Secsgem(WorkspacePaths.License);
                var vision = new Vision(WorkspacePaths.License);
                var taskslist = new List<Task>();
                taskslist.Add(DownloadMachineCfg("NCPC", match, _datasheetHelper.Properties.RefSystemSN, WorkspacePaths.RefSysCFG, WorkspacePaths.SysCfg));
                taskslist.Add(DownloadSecsgemCfg("NCPC", match, _datasheetHelper.Properties.RefSystemSN, WorkspacePaths.RefSysCFG, WorkspacePaths.SysCfg));
                var secsgemtask = secsgem.ReformatFileName(_datasheetHelper.Properties.SystemSN);
                taskslist.Add(secsgemtask);
                taskslist.Add(vision.ReformatFileName(_datasheetHelper.Properties.SapNumber, _datasheetHelper.Properties.SystemSN));
                await Task.WhenAll(taskslist);
                var secsgemcfgpath = Path.Combine(WorkspacePaths.SysCfg, "secsgem.cfg");
                var updatesecsgemcfgtask = UpdateSecsgemCFG(secsgemcfgpath, secsgemtask.Result);
                var datasheetnewpath = Path.Combine(WorkspacePaths.Datasheet, Path.GetFileName(_datasheetHelper.Path));
                File.Move(_datasheetHelper.Path, datasheetnewpath);
                updatesecsgemcfgtask.Wait();
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        private Task DownloadMachineCfg(
            string key,
            string repoSlug,
            string refSerialNumber,
            string refSysCfg,
            string sysCfg)
        {
            return Task.Run(() =>
            {
                var file = "machine.cfg";
                var paths = new List<string>()
                {
                    Path.Combine(
                        refSysCfg,
                        $"Machine_{refSerialNumber}.cfg"),
                    Path.Combine(
                        sysCfg,
                        "machine.cfg")
                };
                _bitbucketHandler.DownloadFile("NCPC", repoSlug, GitRefType.Branch, string.Empty, file, refSerialNumber, paths).Wait();
            });
        }
        private Task DownloadSecsgemCfg(
            string key,
            string repoSlug,
            string refSerialNumber,
            string refSysCfg,
            string sysCfg)
        {
            return Task.Run(() =>
            {
                var file = "secsgem.cfg";
                var paths = new List<string>()
                {
                    Path.Combine(
                        refSysCfg,
                        $"Secsgem_{refSerialNumber}.cfg"),
                    Path.Combine(
                        sysCfg,
                        "secsgem.cfg")
                };
                _bitbucketHandler.DownloadFile("NCPC", repoSlug, GitRefType.Branch, string.Empty, file, refSerialNumber, paths).Wait();
            });
        }
        private Task UpdateSecsgemCFG(
            string filename, string license)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(filename)) return;
                var content = File.ReadAllText(filename);

                content = Regex.Replace(
                    content,
                    @"(?m)^License\s*=.*$",
                    $"License    = {license}");

                content = Regex.Replace(
                    content,
                    @"(?m)^DbRevision\s*=.*$",
                    "DbRevision = $Revision:   1.0  $");

                File.WriteAllText(filename, content.ReplaceLineEndings("\r\n"));
            });
        }
    }
}