using Utility.Lib.Datasheet;
using Utility.Lib.Workspace;

namespace Utility.Lib.NewSystemWorkspace
{
    public class NewSystemWorkspacePaths : BaseUtility
    {
        public Datasheetproperties Properties { get; set; }
        public WorkspacePaths Paths { get; set; }
        public NewSystemWorkspacePaths()
        {
            Properties = new Datasheetproperties();
            Paths = new WorkspacePaths();
        }
        public void Copy(Datasheetproperties datasheet, WorkspacePaths newPaths)
        {
            Properties.Copy(datasheet);
            Paths.Copy(newPaths);
        }
    }
}
