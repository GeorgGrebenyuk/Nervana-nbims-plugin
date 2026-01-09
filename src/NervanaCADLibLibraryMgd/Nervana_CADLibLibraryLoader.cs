using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CADLib;
using NervanaCADLibLibraryMgd.Functions;

namespace NervanaCADLibLibraryMgd
{
    public partial class Nervana_CADLibLibraryLoader : Form, ICADLibPlugin
    {
        public Nervana_CADLibLibraryLoader()
        {
            InitializeComponent();
        }

        private void nervanaCommandImportRevitSharedParametersFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FuncManager.CreateInstance().RunCommand(FuctionVariant.ImportRevitSharedParametersFile);
        }

        private void nervanaCommandExportRevitSharedParametersFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FuncManager.CreateInstance().RunCommand(FuctionVariant.ExportRevitSharedParametersFile);
        }

        public MenuStrip GetMenu()
        {
            return this.menuStrip1;
        }

        public ToolStripContainer GetToolbars()
        {
            return null;
        }

        public void TrackInterfaceItems(InterfaceTracker tracker)
        {
            tracker.Add(new InterfaceItemState(this.nervanaCommandImportRevitSharedParametersFileToolStripMenuItem, LibConnectionState.Connected, LibFolderState.DoesNotMatter, LibObjectState.DoesNotMatter, LibRequiredPermission.EditParametersRegistry));
            tracker.Add(new InterfaceItemState(this.nervanaCommandExportRevitSharedParametersFileToolStripMenuItem, LibConnectionState.Connected, LibFolderState.DoesNotMatter, LibObjectState.DoesNotMatter, LibRequiredPermission.EditParametersRegistry));
        }
    }
}
