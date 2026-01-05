using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;
using Teigha.DatabaseServices;

using BIMStructureMgd.DatabaseObjects;
using BIMStructureMgd.Common;
using BIMStructureMgd.ObjectProperties;

using NervanaNcBIMsMgd.Extensions;
using NervanaNcBIMsMgd.Geometry;


namespace NervanaNcBIMsMgd.Functions
{
    enum RoomFuncVariant
    {
        Nervana_CopyObjectsToRoom,
        Nervana_Room2Floor,
        Nervana_Floor2Room,
        Nervana_Polylines2Room,
        Nervana_LinkWallsToRoom,
        Nervana_LinkObjectsToRoom,
    }
    internal class RoomFuncs
    {
        private ObjectIdCollection? _sourceObjects1;
        private ObjectIdCollection? _sourceObjects2;
        private readonly RoomFuncVariant _funcMode;

        private RoomFuncs(RoomFuncVariant funcMode)
        {
            _funcMode = funcMode;
            _sourceObjects1 = new ObjectIdCollection();
            _sourceObjects2 = new ObjectIdCollection();
        }

        public static RoomFuncs CreateFor (RoomFuncVariant mode)
        {
            RoomFuncs func = new RoomFuncs(mode);

            if (mode == RoomFuncVariant.Nervana_CopyObjectsToRoom)
            {
                func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(StructuralPart) });
                func._sourceObjects2 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) });
            }
            else if (mode == RoomFuncVariant.Nervana_Floor2Room) func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(BuildingSlab) });
            else if (mode == RoomFuncVariant.Nervana_Room2Floor) func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) });
            else if (mode == RoomFuncVariant.Nervana_Polylines2Room) func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(Polyline), typeof(Polyline2d), typeof (Polyline3d) });
            else if (mode == RoomFuncVariant.Nervana_LinkWallsToRoom) 
            {
                func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(BuildingWallBase) });
                func._sourceObjects2 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) });
            }
            else if (mode == RoomFuncVariant.Nervana_LinkObjectsToRoom)
            {
                func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(StructuralPart) });
                func._sourceObjects2 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) });
            }

            return func;
        }

        public void Start()
        {
            switch (this._funcMode)
            {
                case RoomFuncVariant.Nervana_CopyObjectsToRoom:
                    for_CopyObjectsToRooms();
                    break;
                case RoomFuncVariant.Nervana_Floor2Room:
                    for_Floor2Rooms();
                    break;
                case RoomFuncVariant.Nervana_Room2Floor:
                    for_Rooms2Floors();
                    break;
                case RoomFuncVariant.Nervana_Polylines2Room:
                    for_Contours2Rooms();
                    break;
                case RoomFuncVariant.Nervana_LinkWallsToRoom:
                case RoomFuncVariant.Nervana_LinkObjectsToRoom:
                    for_LinkObjectsWithRoom();
                    break;
            }
        }

        private void for_CopyObjectsToRooms()
        {
            if (this._sourceObjects1 == null) return;
            if (this._sourceObjects2 == null) return;
            Dictionary<long, Point3d> room2centroid = new Dictionary<long, Point3d>();

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                //BlockTable? ncBlkTbl = tr.GetObject(Utils.CurrentDoc.Database.BlockTableId,
                //                                OpenMode.ForRead) as BlockTable;
                //if (ncBlkTbl == null) return;

                //BlockTableRecord? ncMspace = tr.GetObject(ncBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                //if (ncMspace == null) return;

                foreach (ObjectId spaceEntId in this._sourceObjects2)
                {
                    SpaceEntity? spaceEntInstance = tr.GetObject(spaceEntId, OpenMode.ForRead) as SpaceEntity;
                    if (spaceEntInstance == null) continue;

                    long spaceEntInstanceHandleId = spaceEntInstance.Handle.Value;
                    if (!room2centroid.ContainsKey(spaceEntInstanceHandleId)) room2centroid.Add(spaceEntInstanceHandleId, spaceEntInstance.GetCentroid());
                    Point3d roomCentroid = room2centroid[spaceEntInstanceHandleId];

                    Vector3d roomVector = new Vector3d(roomCentroid.X, roomCentroid.Y, roomCentroid.Z);

                    foreach (ObjectId structurePartId in this._sourceObjects1)
                    {
                        StructuralPart? structuralPartInstance = tr.GetObject(structurePartId, OpenMode.ForRead) as StructuralPart;
                        if (structuralPartInstance == null) continue;

                        StructuralPart? structuralPartInstanceCopy = structuralPartInstance.Clone() as StructuralPart;
                        if (structuralPartInstanceCopy == null) continue;

                        structuralPartInstanceCopy.TransformBy(Matrix3d.Displacement(roomVector));

                        Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, structuralPartInstanceCopy);

                        //ncMspace.AppendEntity(structuralPartInstanceCopy);
                        //tr.AddNewlyCreatedDBObject(structuralPartInstanceCopy, true);  
                    }
                }
                tr.Commit();
            } 
        }

        private void for_Floor2Rooms()
        {
            if (this._sourceObjects1 == null) return;

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId floorEntId in this._sourceObjects1)
                {
                    BuildingSlab? floorInstance = tr.GetObject(floorEntId, OpenMode.ForRead) as BuildingSlab;
                    if (floorInstance == null) continue;

                    var newRoom = SpaceEntityFactory.Create(floorInstance.GetContour(), 3000);
                    Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, newRoom);
                }
                tr.Commit();
            }
        }

        private void for_Rooms2Floors()
        {
            if (this._sourceObjects1 == null) return;

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId spaceEntId in this._sourceObjects1)
                {
                    SpaceEntity? spaceEntInstance = tr.GetObject(spaceEntId, OpenMode.ForRead) as SpaceEntity;
                    if (spaceEntInstance == null) continue;

                    var newFloor = BuildingSlabFactory.Create(spaceEntInstance.GetFloorContours().First(), 200);
                    Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, newFloor);
                }
                tr.Commit();
            }
        }

        private void for_Contours2Rooms()
        {
            if (this._sourceObjects1 == null) return;

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId plineEntId in this._sourceObjects1)
                {
                    Polyline? polylineInstance = tr.GetObject(plineEntId, OpenMode.ForRead) as Polyline;
                    if (polylineInstance == null) continue;

                    var newRoom = SpaceEntityFactory.Create(polylineInstance, 3000);
                    Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, newRoom);
                }
                tr.Commit();
            }
        }

        private void for_LinkObjectsWithRoom() //too LinkWallsWithRoom
        {
            if (this._sourceObjects1 == null) return;
            if (this._sourceObjects2 == null) return;

            // 1. Кэшируем в памяти данные об объектах, чтобы не выполнять операции в теле транзакции
            ObjectWithGeometry[] tmpSpaceEntities = new ObjectWithGeometry[_sourceObjects2.Count];
            ObjectWithGeometry[] tmpAnalyzedEntities = new ObjectWithGeometry[_sourceObjects1.Count];

            double trimmedPline = 0.0;
            if (this._funcMode == RoomFuncVariant.Nervana_LinkWallsToRoom)
            {
                double? trimmedPline2 = Utils.GetUsersDoubleInput("Величина смещения для анализируемого контура помещения");
                if (trimmedPline2 == null || trimmedPline2 < 0) trimmedPline = 100.0;
            }

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                int spaceEntCounter = 0;
                foreach (ObjectId spaceEntId in this._sourceObjects2)
                {
                    SpaceEntity? spaceEntInstance = tr.GetObject(spaceEntId, OpenMode.ForRead) as SpaceEntity;
                    if (spaceEntInstance == null) continue;
                    
                    ObjectWithGeometry spaceGeometry = new ObjectWithGeometry();
                    spaceGeometry.ObjectId = spaceEntInstance.ObjectId;
                    spaceGeometry.GeometryType = GeometryVariant.Contour;
                    spaceGeometry.Geometry = spaceEntInstance.GetBoundary();
                    if (this._funcMode == RoomFuncVariant.Nervana_LinkWallsToRoom)
                    {
                        ContourTools ct = new ContourTools(spaceEntInstance.GetBoundary());
                        spaceGeometry.Geometry = ct.OffsetTo(trimmedPline);
                    }
                    spaceGeometry.Bounds = spaceEntInstance.GetBounds2();
                    tmpSpaceEntities[spaceEntCounter] = spaceGeometry;
                    spaceEntCounter++;
                }

                int tructPartEntCounter = 0;
                foreach (ObjectId entId in this._sourceObjects1)
                {
                    if (this._funcMode == RoomFuncVariant.Nervana_LinkObjectsToRoom)
                    {
                        StructuralPart? structuralPartInstance = tr.GetObject(entId, OpenMode.ForRead) as StructuralPart;
                        if (structuralPartInstance == null) continue;

                        ObjectWithGeometry structuralPartGeometry = new ObjectWithGeometry();
                        structuralPartGeometry.ObjectId = entId;
                        structuralPartGeometry.GeometryType = GeometryVariant.Point;
                        structuralPartGeometry.Geometry = structuralPartInstance.BasePoint;

                        tmpAnalyzedEntities[tructPartEntCounter] = structuralPartGeometry;
                        tructPartEntCounter++;
                    }
                    else if (this._funcMode == RoomFuncVariant.Nervana_LinkWallsToRoom)
                    {
                        BuildingWallBase? wallBaseInstance = tr.GetObject(entId, OpenMode.ForRead) as BuildingWallBase;
                        if (wallBaseInstance == null) continue;

                        ObjectWithGeometry wallBaseInstanceGeometry = new ObjectWithGeometry();
                        wallBaseInstanceGeometry.ObjectId = entId;
                        wallBaseInstanceGeometry.GeometryType = GeometryVariant.Line;
                        wallBaseInstanceGeometry.Geometry = new Point3d[] { wallBaseInstance.StartPoint, wallBaseInstance.EndPoint };

                        tmpAnalyzedEntities[tructPartEntCounter] = wallBaseInstanceGeometry;
                        tructPartEntCounter++;
                    };     
                }
            }

            // 2. Начинаем анализ, перебирая сами объекты
            Dictionary<ObjectId, ObjectId> structObject2Room = new Dictionary<ObjectId, ObjectId>();

            foreach (ObjectWithGeometry strObjectInfo in tmpAnalyzedEntities)
            {
                foreach (ObjectWithGeometry RoomInfo in tmpSpaceEntities)
                {
                    if (RoomInfo.Contains(strObjectInfo))
                    {
                        structObject2Room.Add(strObjectInfo.ObjectId, RoomInfo.ObjectId);
                        break;
                    }
                }
            }

            // 3. Для найденных соответвий заполняем свойства проверяемых объектов : добавляем привязку к помещениям
            // Делаем в теле транзакций

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (var structObject2RoomInfo in structObject2Room)
                {
                    SpaceEntity? roomInstance = tr.GetObject(structObject2RoomInfo.Value, OpenMode.ForRead) as SpaceEntity;
                    if (roomInstance == null) continue;

                    DBObject linkedEntityObject = tr.GetObject(structObject2RoomInfo.Key, OpenMode.ForRead);
                    IParametricObject? linkedEntity = linkedEntityObject as IParametricObject;
                    if (linkedEntity == null) continue;
                    ElementData? linkedEntityElmData = linkedEntity?.GetElementData();
                    if (linkedEntityElmData == null) continue;

                    linkedEntityElmData.SetParameter("PARENT_SPACE_HANDLE", roomInstance.Handle.Value);
                    linkedEntityElmData.SetParameter("PARENT_SPACE_NAME", roomInstance.Name);
                    linkedEntityElmData.SetParameter("PARENT_SPACE_NUMBER", roomInstance.Number);
                }

                tr.Commit();
            }
        }
    }
}
