using HostMgd.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Runtime;

using NervanaNcMgd.Functions;
using NervanaNcMgd.UI.Windows;

namespace NervanaNcMgd
{

    public class Loader : IExtensionApplication
    {
        private void RunExplorer(MgdMode mode)
        {
            object? data = null;
            if (mode == MgdMode.Documents) data = HostMgd.ApplicationServices.Application.DocumentManager;
            else if (mode == MgdMode.Document) data = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            else if (mode == MgdMode.Database) data = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            else if (mode == MgdMode.Application) data = MgdMode.Application;
            else if (mode == MgdMode.Objects)
            {
                Editor ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

                SelectionSet SelSet = ed.SelectImplied().Value;
                if (SelSet.Count > 0)
                {
                    data = new Teigha.DatabaseServices.ObjectIdCollection(SelSet.GetObjectIds());
                }
                else
                {
                    PromptSelectionResult res = ed.GetSelection();
                    if (res.Status == PromptStatus.OK && res.Value.Count > 0)
                    {
                        data = new Teigha.DatabaseServices.ObjectIdCollection(res.Value.GetObjectIds());
                    }
                }
            }
            else if (mode == MgdMode.Objects2) MgdExplorerReflection_PaletteManager.CreatePalette();

            if (mode == MgdMode.Objects2)
            {
                // ничего, событие смены выбора
            }
            else
            {
                Nervana_ExplorerSpace expl = new Nervana_ExplorerSpace(data);
                HostMgd.ApplicationServices.Application.ShowModalWindow(expl);
            }
        }

        [CommandMethod("Nervana_MgdExplorerReflectionApplication")]
        public void command_1()
        {
            RunExplorer(MgdMode.Application);
        }

        [CommandMethod("Nervana_MgdExplorerReflectionDocuments")]
        public void command_2()
        {
            RunExplorer(MgdMode.Documents);
        }

        [CommandMethod("Nervana_MgdExplorerReflectionDocument")]
        public void command_3()
        {
            RunExplorer(MgdMode.Document);
        }

        [CommandMethod("Nervana_MgdExplorerReflectionDatabase")]
        public void command_4()
        {
            RunExplorer(MgdMode.Database);
        }

        [CommandMethod("Nervana_MgdExplorerReflectionEntities", CommandFlags.UsePickSet)]
        public void command_5()
        {
            RunExplorer(MgdMode.Objects);
        }

        [CommandMethod("Nervana_MgdExplorerReflectionEntities2", CommandFlags.UsePickSet)]
        public void command_6()
        {
            RunExplorer(MgdMode.Objects2);
        }


        public void Initialize()
        {
;
        }

        public void Terminate()
        {

        }
    }
}
