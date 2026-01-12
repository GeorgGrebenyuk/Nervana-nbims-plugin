using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HostMgd.EditorInput;
using Teigha.Geometry;
using Teigha.DatabaseServices;

using BIMStructureMgd.DatabaseObjects;
using BIMStructureMgd.Common;
using BIMStructureMgd.ObjectProperties;

using NervanaCommonMgd;
using NervanaNcMgd.Common;
using NervanaNcMgd.Common.Geometry;
using NervanaNcBIMsMgd.Extensions;

namespace NervanaNcBIMsMgd.Functions
{
    enum RoomFuncVariant
    {
        Nervana_PlaceRoomInContour,
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
                func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(ParametricEntity) }, "Выберите параметрические объекты для копирования ");
                func._sourceObjects2 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) }, "Выберите помещения ");
            }
            else if (mode == RoomFuncVariant.Nervana_Floor2Room) func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(BuildingSlab) }, "Выберите перекрытия ");
            else if (mode == RoomFuncVariant.Nervana_Room2Floor) func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) }, "Выберите помещения ");
            else if (mode == RoomFuncVariant.Nervana_Polylines2Room) func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(Polyline), typeof(Polyline2d), typeof (Polyline3d) }, "Выберите замкнутые полилинии ");
            else if (mode == RoomFuncVariant.Nervana_LinkWallsToRoom) 
            {
                func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(LinearBuildingWall ) }, "Выберите стены ");
                func._sourceObjects2 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) }, "Выберите помещения ");
            }
            else if (mode == RoomFuncVariant.Nervana_LinkObjectsToRoom)
            {
                func._sourceObjects1 = Utils.SelectObjectsByTypes(new Type[] { typeof(ParametricEntity) }, "Выберите параметрические объекты ");
                func._sourceObjects2 = Utils.SelectObjectsByTypes(new Type[] { typeof(SpaceEntity) }, "Выберите помещения ");
            }

            return func;
        }

        public void Start()
        {
            switch (this._funcMode)
            {
                case RoomFuncVariant.Nervana_PlaceRoomInContour:
                    for_PlaceRoomInContour();
                    break;
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

        private void for_PlaceRoomInContour()
        {
            var CurrentDoc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = CurrentDoc.Editor;
            PromptPointResult pointRes = ed.GetPoint("Укажите точку внутри помещения ");
            if (pointRes.Status != PromptStatus.OK) return;

            var plines = ed.TraceBoundary(pointRes.Value, false);

            using (Transaction tr = CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (DBObject obj in plines)
                {
                    Polyline? pline = obj as Polyline;
                    if (pline == null || !pline.Closed) continue;

                    var newRoom = SpaceEntityFactory.Create(pline, 3000);
                    Utilities.AddEntityToDatabase(CurrentDoc.Database, tr, newRoom);

                    // Больше одной границы и не нужно. Хотя, есть ещё вырезы,
                    // но базовая механика всё равно не умеет делать островки
                    break;
                }
                tr.Commit();
            }

            // и т.д. до прерывания через Esc
            for_PlaceRoomInContour();
        }

        private void for_CopyObjectsToRooms()
        {
            if (this._sourceObjects1 == null) return;
            if (this._sourceObjects2 == null) return;
            Dictionary<long, Point3d> room2centroid = new Dictionary<long, Point3d>();

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId spaceEntId in this._sourceObjects2)
                {
                    SpaceEntity? spaceEntInstance = tr.GetObject(spaceEntId, OpenMode.ForRead) as SpaceEntity;
                    if (spaceEntInstance == null) continue;

                    long spaceEntInstanceHandleId = spaceEntInstance.Handle.Value;
                    if (!room2centroid.ContainsKey(spaceEntInstanceHandleId)) room2centroid.Add(spaceEntInstanceHandleId, spaceEntInstance.GetCentroid());
                    Point3d roomCentroid = room2centroid[spaceEntInstanceHandleId];
                    TraceWriter.Log($"Centroid was calced {roomCentroid.ToString()}", LogType.Modify);

                    Vector3d roomVector = new Vector3d(roomCentroid.X, roomCentroid.Y, roomCentroid.Z);

                    foreach (ObjectId structurePartId in this._sourceObjects1)
                    {
                        ParametricEntity? ParametricEntityInstance = tr.GetObject(structurePartId, OpenMode.ForRead) as ParametricEntity;
                        if (ParametricEntityInstance == null) continue;
                        TraceWriter.Log($"ParametricEntity getting success", LogType.Add);
                        Vector3d ParametricEntityInstancePlacement = new Vector3d(ParametricEntityInstance.BasePoint.X, ParametricEntityInstance.BasePoint.Y, ParametricEntityInstance.BasePoint.Z);
                        

                        ParametricEntity? ParametricEntityInstanceCopy = ParametricEntityInstance.Clone() as ParametricEntity;
                        if (ParametricEntityInstanceCopy == null) continue;
                        TraceWriter.Log($"ParametricEntity CLONE getting success", LogType.Add);

                        ParametricEntityInstanceCopy.TransformBy(Matrix3d.Displacement(roomVector - ParametricEntityInstancePlacement));

                        Utilities.AddEntityToDatabase(CommonUtils.CurrentDoc.Database, tr, ParametricEntityInstanceCopy);

                        TraceWriter.Log($"ParametricEntity ADDED success", LogType.Add);

                        //ncMspace.AppendEntity(ParametricEntityInstanceCopy);
                        //tr.AddNewlyCreatedDBObject(ParametricEntityInstanceCopy, true);  
                    }
                }
                tr.Commit();
            } 
        }

        private void for_Floor2Rooms()
        {
            if (this._sourceObjects1 == null) return;

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId floorEntId in this._sourceObjects1)
                {
                    BuildingSlab? floorInstance = tr.GetObject(floorEntId, OpenMode.ForRead) as BuildingSlab;
                    if (floorInstance == null) continue;

                    var newRoom = SpaceEntityFactory.Create(floorInstance.GetContour(), 3000);
                    Utilities.AddEntityToDatabase(CommonUtils.CurrentDoc.Database, tr, newRoom);
                }
                tr.Commit();
            }
        }

        private void for_Rooms2Floors()
        {
            if (this._sourceObjects1 == null) return;

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId spaceEntId in this._sourceObjects1)
                {
                    SpaceEntity? spaceEntInstance = tr.GetObject(spaceEntId, OpenMode.ForRead) as SpaceEntity;
                    if (spaceEntInstance == null) continue;

                    var newFloor = BuildingSlabFactory.Create(spaceEntInstance.GetFloorContours().First(), 200);
                    Utilities.AddEntityToDatabase(CommonUtils.CurrentDoc.Database, tr, newFloor);
                }
                tr.Commit();
            }
        }

        private void for_Contours2Rooms()
        {
            if (this._sourceObjects1 == null) return;

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId plineEntId in this._sourceObjects1)
                {
                    Polyline? polylineInstance = tr.GetObject(plineEntId, OpenMode.ForRead) as Polyline;
                    if (polylineInstance == null) continue;

                    var newRoom = SpaceEntityFactory.Create(polylineInstance, 3000);
                    Utilities.AddEntityToDatabase(CommonUtils.CurrentDoc.Database, tr, newRoom);
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

            double trimmedPline = 800.0;

            if (this._funcMode == RoomFuncVariant.Nervana_LinkWallsToRoom)
            {
                trimmedPline = UserInput.GetUserInput("Величина смещения для анализируемого контура помещения", 800.0);
            }

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
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
                        var offs = ct.OffsetTo(trimmedPline);
                        string offsetPoints = string.Join(";", offs.Select(x => x.ToString()));
                        TraceWriter.Log($"Calced offset curve = " + offsetPoints, LogType.Add);
                        spaceGeometry.Geometry = offs;
                    }
                    spaceGeometry.Bounds = spaceEntInstance.GetBounds2(trimmedPline);

                    tmpSpaceEntities[spaceEntCounter] = spaceGeometry;
                    spaceEntCounter++;
                }

                int tructPartEntCounter = 0;
                foreach (ObjectId entId in this._sourceObjects1)
                {
                    if (this._funcMode == RoomFuncVariant.Nervana_LinkObjectsToRoom)
                    {
                        ParametricEntity? ParametricEntityInstance = tr.GetObject(entId, OpenMode.ForRead) as ParametricEntity;
                        if (ParametricEntityInstance == null) continue;

                        ObjectWithGeometry ParametricEntityGeometry = new ObjectWithGeometry();
                        ParametricEntityGeometry.ObjectId = entId;
                        ParametricEntityGeometry.GeometryType = GeometryVariant.Point;
                        ParametricEntityGeometry.Geometry = ParametricEntityInstance.BasePoint;

                        tmpAnalyzedEntities[tructPartEntCounter] = ParametricEntityGeometry;
                        tructPartEntCounter++;
                    }
                    else if (this._funcMode == RoomFuncVariant.Nervana_LinkWallsToRoom)
                    {
                        LinearBuildingWall ? wallBaseInstance = tr.GetObject(entId, OpenMode.ForRead) as LinearBuildingWall ;
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

            TraceWriter.Log($"Pre reading success. Parametric ents = {tmpAnalyzedEntities.Length}; Rooms = {tmpSpaceEntities.Length}", LogType.Add);

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

            TraceWriter.Log($"Anayze was finished = {structObject2Room.Count} results", LogType.Add);

            // 3. Для найденных соответвий заполняем свойства проверяемых объектов : добавляем привязку к помещениям
            // Делаем в теле транзакций

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (var structObject2RoomInfo in structObject2Room)
                {
                    SpaceEntity? roomInstance = tr.GetObject(structObject2RoomInfo.Value, OpenMode.ForRead) as SpaceEntity;
                    if (roomInstance == null) continue;

                    DBObject linkedEntityObject = tr.GetObject(structObject2RoomInfo.Key, OpenMode.ForWrite);
                    IParametricObject? linkedEntity = linkedEntityObject as IParametricObject;
                    if (linkedEntity == null) continue;

                    TraceWriter.Log($"Pre add room's params", LogType.Add);

                    linkedEntity.GetElementData().SetParameter("PARENT_SPACE_HANDLE", roomInstance.Handle.Value.ToString());
                    linkedEntity.GetElementData().SetParameter("PARENT_SPACE_NAME", roomInstance.Name, "Имя родительского помещения", "");
                    linkedEntity.GetElementData().SetParameter("PARENT_SPACE_NUMBER", roomInstance.Number, "Номер родительского помещения", "");

                    TraceWriter.Log($"Post add room's params", LogType.Add);

                }

                tr.Commit();
            }
        }
    }
}
