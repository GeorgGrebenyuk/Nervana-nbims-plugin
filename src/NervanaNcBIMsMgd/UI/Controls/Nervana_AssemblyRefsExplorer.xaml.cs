using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Teigha.DatabaseServices;
using HostMgd.ApplicationServices;
using Teigha.Geometry;

using BIMStructureMgd.DatabaseObjects;
using BIMStructureMgd.Common;

using NervanaCommonMgd;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace NervanaNcBIMsMgd.UI.Controls
{
    internal class AssemblyRefInfo
    {
        public string Name { get; set; } = "";
        public string Group { get; set; } = "";
        public string AssemblyGroup { get; set; } = "";
        public string AssemblyPrefix { get; set; } = "";
        public string Speciality { get; set; } = "";

        public ObjectId Id;

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            AssemblyRefInfo? objAsAssemblyRefInfo = obj as AssemblyRefInfo;
            if (objAsAssemblyRefInfo == null) return false;
            return (
                objAsAssemblyRefInfo.Group == this.Group &&
                objAsAssemblyRefInfo.AssemblyGroup == this.AssemblyGroup &&
                objAsAssemblyRefInfo.AssemblyPrefix == this.AssemblyPrefix &&
                objAsAssemblyRefInfo.Speciality == this.Speciality);
        }
        public override int GetHashCode() { return Id.GetHashCode(); }

    }
    /// <summary>
    /// Interaction logic for Nervana_AssemblyRefsExplorer.xaml
    /// </summary>
    public partial class Nervana_AssemblyRefsExplorer : UserControl
    {
        public Nervana_AssemblyRefsExplorer()
        {
            InitializeComponent();

            DocumentCollection dm = HostMgd.ApplicationServices.Application.DocumentManager;
            dm.DocumentCreated += new DocumentCollectionEventHandler(callback_DocumentCreated);
            dm.DocumentToBeDestroyed += new DocumentCollectionEventHandler(callback_DocumentToBeDestroyed);
            dm.DocumentBecameCurrent += new DocumentCollectionEventHandler(callback_Dm_DocumentBecameCurrent);

            mAssemblyRefs = new ObservableCollection<AssemblyRefInfo>();

            this.ListView_ConstructionAssemblyRefs.SelectionMode = SelectionMode.Single;
            this.ListView_ConstructionAssemblyRefs.SelectionChanged += ListView_ConstructionAssemblyRefs_SelectionChanged;

            updateAssemblyRefList();
            //this.ListView_ConstructionAssemblyRefs.ItemsSource = mAssemblyRefs;
        }

        private void ListView_ConstructionAssemblyRefs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AssemblyRefInfo? selectedInfo = this.ListView_ConstructionAssemblyRefs.SelectedItem as AssemblyRefInfo;
            if (selectedInfo == null) return;

            // Нужно предложить Пользователю ввести точку, куда будет скопирована данная К.Сборка
            HostMgd.EditorInput.Editor ed = Utils.CurrentDoc.Editor;
            var point = ed.GetPoint("Введите точку вставки конструктивной сборки ... ");
            if (point.Status != HostMgd.EditorInput.PromptStatus.OK) return;

            using Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction();

            ConstructionAssemblyRef? assRef = tr.GetObject(selectedInfo.Id, OpenMode.ForWrite) as ConstructionAssemblyRef;
            if (assRef == null) return;
           
            Vector3d assRefPlacement = new Vector3d(assRef.Position.X, assRef.Position.Y, assRef.Position.Z);
            TraceWriter.Log($"ParametricEntity getting success. Old place {assRefPlacement.ToString()}", LogType.Add);

            ConstructionAssemblyRef? assRefCopy = assRef.Clone() as ConstructionAssemblyRef;
            if (assRefCopy == null) return;
            TraceWriter.Log($"ParametricEntity CLONE getting success", LogType.Add);

            Vector3d newPlace = new Vector3d(point.Value.X - assRefPlacement.X, point.Value.Y - assRefPlacement.Y, 0);
            TraceWriter.Log($"newPlace {newPlace.ToString()}", LogType.Add);
            assRef.TransformBy(Matrix3d.Displacement(newPlace));
            TraceWriter.Log($"ParametricEntity Pre add", LogType.Add);
            Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, assRefCopy);
            TraceWriter.Log($"ParametricEntity Post add", LogType.Add);
            tr.Commit();
        }

        private void callback_Dm_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            updateAssemblyRefList();
        }

        private void callback_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            updateAssemblyRefList();
        }

        private void callback_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            mAssemblyRefs = new ObservableCollection<AssemblyRefInfo>();
        }

        private void updateAssemblyRefList()
        {
            mAssemblyRefs = new ObservableCollection<AssemblyRefInfo>();
            if (Utils.CurrentDoc == null) return;
            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable? acBlkTbl;
                acBlkTbl = tr.GetObject(Utils.CurrentDoc.Database.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return;

                // Open the Block table record Model space for write
                BlockTableRecord? acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForRead) as BlockTableRecord;
                if (acBlkTblRec == null) return;

                foreach (ObjectId entId in acBlkTblRec)
                {
                    ConstructionAssemblyRef? assRef = tr.GetObject(entId, OpenMode.ForRead) as ConstructionAssemblyRef;
                    if (assRef == null) continue;

                    TraceWriter.Log($"Сборка была получена! {assRef.Name}");

                    AssemblyRefInfo assRefInfo = new AssemblyRefInfo();
                    assRefInfo.Name = assRef.Name;
                    assRefInfo.Group = assRef.GetElementData().GetParameter("PART_GROUP").Value;
                    assRefInfo.Speciality = assRef.GetElementData().GetParameter("PART_SPECIALITY").Value;
                    assRefInfo.AssemblyGroup = assRef.GetElementData().GetParameter("AEC_ASSEMBLY_GROUP").Value;
                    assRefInfo.AssemblyPrefix = assRef.GetElementData().GetParameter("AEC_ASSEMBLY_PREFIX").Value;
                    assRefInfo.Id = entId;

                    bool isEquals = false;
                    foreach (var existedAssRefs in mAssemblyRefs)
                    {
                        isEquals = existedAssRefs.Equals(assRef);
                        if (isEquals) break;
                    }

                    if (!isEquals) mAssemblyRefs.Add(assRefInfo);
                }

                tr.Abort();
            }

            this.ListView_ConstructionAssemblyRefs.Items.Clear();
            foreach (var assRefInfo in mAssemblyRefs)
            {
                this.ListView_ConstructionAssemblyRefs.Items.Add(assRefInfo);
            }
        }

        private ObservableCollection<AssemblyRefInfo> mAssemblyRefs;
    }
}
