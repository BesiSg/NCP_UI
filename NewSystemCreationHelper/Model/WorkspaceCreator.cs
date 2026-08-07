using System.IO;
using SystemDatasheetHandler;
using Utility;
using Utility.Lib.Datasheet;
using Utility.Lib.Workspace;

namespace NewSystemCreationHelperModule.Model
{
    public class WorkspaceCreator
    {
        public WorkspaceCreator() { }
        public static WorkspacePaths CreateWorkspace(Datasheetproperties source)
        {
            var foldername = $"{source.SystemSN} {source.CustomerAbbr}";
            var workspacepath = Path.Combine(Utility.Constants.BasePath, foldername, source.NCP);
            if (!Directory.Exists(workspacepath))
                Directory.CreateDirectory(workspacepath);
            //Create 'cfg' subfolder 
            var refcfg_path = Path.Combine(workspacepath, Utility.Constants.RefSysCfg);
            if (!Directory.Exists(refcfg_path))
                Directory.CreateDirectory(refcfg_path);

            //Create 'datasheet' subfolder 
            var datasheet_path = Path.Combine(workspacepath, Utility.Constants.Datasheet);
            if (!Directory.Exists(datasheet_path))
                Directory.CreateDirectory(datasheet_path);

            //Create 'datasheet' subfolder 
            var syscfg_path = Path.Combine(workspacepath, Utility.Constants.SysCfg);
            if (!Directory.Exists(syscfg_path))
                Directory.CreateDirectory(syscfg_path);

            //Create 'datasheet' subfolder 
            var licenses_path = Path.Combine(workspacepath, Utility.Constants.Licenses);
            if (!Directory.Exists(licenses_path))
                Directory.CreateDirectory(licenses_path);

            return new WorkspacePaths(workspacepath, refcfg_path, datasheet_path, syscfg_path, licenses_path);
        }
    }
}