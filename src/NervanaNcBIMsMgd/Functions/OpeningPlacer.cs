using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.DatabaseServices;
using Teigha.Geometry;

using BIMStructureMgd.Common;
using BIMStructureMgd.DatabaseObjects;
using NervanaNcBIMsMgd.Extensions;
using NervanaCommonMgd;

namespace NervanaNcBIMsMgd.Functions
{
    enum OpeningFigure
    {
        Circle,
        Rectangle
    }
    class OpeningInfo
    {
        public long Handle;
        public Point3d Position;
        public OpeningFigure Figure;
        public double Size;
    }
    class OpeningsInfo
    {

    }
    internal class OpeningPlacer
    {
        
        public OpeningPlacer()
        {

        }

        public void SetPlaces(OpeningInfo[] places)
        {
            mPlaces = places;
        }

        public void SetPlaces(string filePath)
        {
            if (!File.Exists(filePath)) return;

            string[] fileData = File.ReadAllLines(filePath).Skip(1).ToArray();
            List<OpeningInfo> openings = new List<OpeningInfo>();
            foreach (string str in fileData)
            {
                if (!str.Contains("\t")) continue;
                string[] openingArr = str.Split("\t");
                if (openingArr.Length != 4) continue;

                OpeningInfo openingInfo = new OpeningInfo();
                openingInfo.Handle = Convert.ToInt64(openingArr[0]);
                double[] posRaw = openingArr[1].Split(";").Select(num  => Convert.ToDouble(num)).ToArray();
                openingInfo.Position = new Point3d(posRaw[0], posRaw[1], posRaw[2]);
                openingInfo.Figure = (OpeningFigure)Enum.Parse(typeof(OpeningFigure), openingArr[2]);
                openingInfo.Size = Convert.ToDouble(openingArr[3]);

                openings.Add(openingInfo);
            }
            SetPlaces(openings.ToArray());
        }

        public void SetPlaces()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор файла с отверстиями";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Текстовый файл (*.TXT, *.xml) | *.TXT;*.txt";

#if DEBUG
            SetPlaces(@"C:\Users\Georg\Documents\GitHub\Nervana-nbims-plugin\tests\NervanaTest_OpeningsInfo1.txt");
#endif
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    SetPlaces(openFileDialog.FileName);
            //}
        }

        public void Start()
        {
            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (OpeningInfo openingInfo in this.mPlaces)
                {
                    Handle entityHandle = new Handle(openingInfo.Handle);
                    ObjectId entityObjectId = Utils.CurrentDoc.Database.GetObjectId(false, entityHandle, 0);
                    if (!entityObjectId.IsValid || entityObjectId.IsErased) continue;

                    DBObject entityInstance = tr.GetObject(entityObjectId, OpenMode.ForWrite);

                    BuildingWallBase? entityAsWall = entityInstance as BuildingWallBase;
                    BuildingSlab? entytyAsFloor = entityInstance as BuildingSlab;

                    if (entityAsWall != null)
                    {
                        // В стену необходимо вставить проём из параметрического объекта из БСК
                        string openingName = "Проем пустой круглый";
                        if (openingInfo.Figure == OpeningFigure.Rectangle) openingName = "Проем пустой прямоугольный";

                        LibraryService service = LibraryService.GetLibraryService();
                        LibraryRequest request = LibraryRequest.CreateDatabaseRequest();
                        //request.AddCategoryCondition(LibraryObject.StructuralPartCategory);
                        request.AddCondition("PART_NAME", "=", openingName);
                        var objects = request.Execute();
                        if (objects == null ) continue;
                        if (!objects.Any())
                        {
                            Utils.CurrentDoc.Editor.WriteMessage("Не было найдено объектов для данных условий!");
                            continue;
                        }


                        var opening = BuildingOpeningFactory.Create(objects.First());
                        opening.DimDepth = entityAsWall.Thickness;
                        opening.Elevation = openingInfo.Position.Z - entityAsWall.StartPoint.Z;

                        TraceWriter.Log("Elevation = " + opening.Elevation.ToString());

                        // В завсимости от типа стены -- по линии или по дуге, будут разные способы
                        // расчта положения отверстия по длине стены "openingPlacePart"
                        LinearBuildingWall? wallAsLinear = entityAsWall as LinearBuildingWall;
                        ArcBuildingWall? wallAsArc = entityAsWall as ArcBuildingWall;

                        double openingPlacePart;
                        if (wallAsLinear != null)
                        {
                            openingPlacePart = Math.Sqrt(
                            Math.Pow(openingInfo.Position.X - entityAsWall.StartPoint.X, 2) +
                            Math.Pow(openingInfo.Position.Y - entityAsWall.StartPoint.Y, 2));
                        }
                        else
                        {
                            Arc wallArc = wallAsArc.GetBaseline2();
                            openingPlacePart = wallArc.GetParameterAtPoint(openingInfo.Position) * wallAsArc.Length;
                        }
                        opening.Position = openingPlacePart;

                        TraceWriter.Log("opening.Position = " + opening.Position.ToString());
                        TraceWriter.Log("wall.Length = " + entityAsWall.Length.ToString());

                        Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, opening);

                        // Редактируем размеры отверстия
                        opening.GetElementData().SetParameter("DIM_WIDTH", openingInfo.Size);
                        opening.GetElementData().SetParameter("DIM_HEIGHT", openingInfo.Size);

                        // Присоединение проёма к стене возможно только после появления у него ID. То есть после добавление объекта в модель.
                        opening.ConnectToSurface(entityAsWall);

                        TraceWriter.Log("ConnectToSurface");
                    }
                    else if (entytyAsFloor != null)
                    {
                        // В перекрытии нужно вырезать отверстие методом CutContour
                        double side = openingInfo.Size / 2;
                        Point2d openingInfoPosition2 = new Point2d(openingInfo.Position.X, openingInfo.Position.Y);
                        Point2d[] contourPoints = new Point2d[]
                        {
                            new Point2d(openingInfo.Position.X - side, openingInfo.Position.Y - side),
                            new Point2d(openingInfo.Position.X - side, openingInfo.Position.Y + side),
                            new Point2d(openingInfo.Position.X + side, openingInfo.Position.Y + side),
                            new Point2d(openingInfo.Position.X + side, openingInfo.Position.Y - side),
                            //new Point2d(openingInfo.Position.X - side, openingInfo.Position.Y - side)
                        };
                        entytyAsFloor.CutContour(contourPoints);

                        TraceWriter.Log("CutContour");
                    }

                }

                TraceWriter.Log("End!");
                tr.Commit();
            }
        }


        private OpeningInfo[] mPlaces;
    }
}
