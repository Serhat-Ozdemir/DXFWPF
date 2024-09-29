using DXF.Commands;
using DXF.Stores;
using HelixToolkit.Wpf;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using netDxf;
using Csg;
using static Csg.Solids;
using System.Windows;
using Vector3D = Csg.Vector3D;
using System.Windows.Documents;
using System.Collections.Generic;
using DXF.Models;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.IO;
using netDxf.Collections;
using SharpDX.Direct3D11;
using System.Collections;
using Aspose.ThreeD;
using System.Linq;
using netDxf.Entities;
using Line = DXF.Models.Line;
using Solid = Csg.Solid;

namespace DXF.ViewModels
{
    public class DXFViewModel : ViewModelBase
    {
        public ICommand ApplyCommand { get; }
        public ICommand SelectFileCommand { get; }


        public List<Line> lines = new List<Line>();

        public DXFViewModel(NavigationStore navigation, double height)
        {
            Height = Convert.ToString(height);
            ApplyCommand = new ApplyCommand(this);
            SelectFileCommand = new SelectFileCommand(this);

        }

        private Point3D _cameraPosition;
        public Point3D CameraPosition
        {
            get
            {
                return _cameraPosition;
            }
            set
            {
                _cameraPosition = value;
                OnPropertyChanged(nameof(CameraPosition));
            }
        }

        private System.Windows.Media.Media3D.Vector3D _cameraLookDirection;
        public System.Windows.Media.Media3D.Vector3D CameraLookDirection
        {
            get
            {
                return _cameraLookDirection;
            }
            set
            {
                _cameraLookDirection = value;
                OnPropertyChanged(nameof(CameraLookDirection));
            }
        }


        private string _filePath;
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        private string _height;
        public string Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }


        private Model3DGroup _model;
        public Model3DGroup Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
            }
        }
        #region SolidGeometry

        public Model3DGroup showModel(string filePath, double height)//Save as stl then import
        {
            Solid model = getModelAsSolid(filePath, height);
            StreamWriter writer = new StreamWriter("StlOutput.stl");
            model.WriteStl("StlOutput", writer);
            writer.Close();
            var importer = new ModelImporter();
            var modelGroup = importer.Load("StlOutput.stl");
            return modelGroup;
        }
        private Solid getModelAsSolid(string filePath, double height)
        {
            Solid model = new Solid();
            Solid body = new Solid();
            Solid holes = new Solid();

            DxfDocument dxf = DxfDocument.Load(filePath);
            DrawingEntities entity = dxf.Entities;

            addLines(entity);

            List<Solid> solids = getSolidsList(findConnectedLines(), height);//Get line connected solids

            //Extract the body
            body = getLargestSolid(solids);
            solids.Remove(body);

            //Recreate the body too perform difference properly
            double xLength  = body.Polygons[0].BoundingBox.Size.X;
            double yLength  = body.Polygons[0].BoundingBox.Size.Y;
            double xPos = body.Polygons[0].BoundingBox.Min.X;
            double yPos = body.Polygons[0].BoundingBox.Min.Y;
            body = Cube(size: new Vector3D(xLength, yLength, height)).Translate(xPos, yPos, 0);

            //Gather other line connected solids (Assumes they are not intersecting)
            if (solids.Count > 0)
                for (int i = 0; i < solids.Count; i++)
                    holes = UnionForNonIntersecting(holes, solids[i]);
            //Gather circles as cylinders (Assumes they are not intersecting)
            holes = UnionForNonIntersecting(holes, createCylinder(entity, height));

            //Extract holes from body to create model
            model = Difference(body, holes);
            return model;
        }

        Solid getLargestSolid(List<Solid> solids)
        {
            Solid largest = new Solid();
            Vector2D largestSize = new Vector2D(0, 0);
            for (int i = 0; i < solids.Count; i++)
            {
                Solid solid = solids[i];
                //Compare x and y sizes and get the biggest
                if (solid.Polygons[0].BoundingBox.Size.X > largestSize.X && solid.Polygons[0].BoundingBox.Size.Y > largestSize.Y)
                {
                    largestSize.X = solid.Polygons[0].BoundingBox.Size.X;
                    largestSize.Y = solid.Polygons[0].BoundingBox.Size.Y;
                    largest = solid;
                }
            }
            return largest;
        }

        
        Solid UnionForNonIntersecting(Solid firstSolid, Solid secondSolid)//Expand polygons' list (Library Function)
        {
            var newpolygons = new List<Csg.Polygon>(firstSolid.Polygons);
            newpolygons.AddRange(secondSolid.Polygons);
            var result = Solid.FromPolygons(newpolygons);
            result.IsCanonicalized = firstSolid.IsCanonicalized && secondSolid.IsCanonicalized;
            result.IsRetesselated = firstSolid.IsRetesselated && secondSolid.IsRetesselated;
            return result;
        }

        public List<Solid> getSolidsList(List<List<Line>> solidsList, double height)//Get every individual solid
        {
            List<Solid> solids = new List<Solid>();
            foreach (var solid in solidsList)
            {
                solids.Add(createSolidWithLines(solid, height));
            }
            return solids;
        }
        public Solid createSolidWithLines(List<Line> solid, double height)
        {
            int lineCount = solid.Count;//Number of edges of the solid
            int pointsCount = lineCount * 2;//Number of vertices of the solid
            Vector3D[] points = new Vector3D[pointsCount];//Vertices of the solid
            int[][] polygons = new int[lineCount + 2][];//Polygons points of the solid


            //Add vertices of the solid
            for (int i = 0; i < lineCount - 1; i++)
            {
                points[i] = new Vector3D(solid[i].startPoint.X, solid[i].startPoint.Y, 0);
                points[i + 1] = new Vector3D(solid[i + 1].startPoint.X, solid[i + 1].startPoint.Y, 0);
                points[i + lineCount] = new Vector3D(solid[i].startPoint.X, solid[i].startPoint.Y, height);
                points[i + lineCount + 1] = new Vector3D(solid[i + 1].startPoint.X, solid[i + 1].startPoint.Y, height);
            }

            int polygonsLength = polygons.GetLength(0);

            //Use indices of points to adjust polygons
            int leftFlag = 0, rightFlag = lineCount + 1;
            for (int i = 0; i < polygonsLength; i++)
            {
                if (i == 0)//Bottom face of the solid
                {
                    int[] bottomFace = new int[lineCount];
                    for (int j = 0; j < bottomFace.Length; j++)
                        bottomFace[j] = j;
                    polygons[i] = bottomFace;
                }
                else if (i == 1)//Top face of the solid
                {
                    int[] topFace = new int[lineCount];
                    for (int j = 0; j < topFace.Length; j++)
                        topFace[j] = j + lineCount;
                    polygons[i] = topFace;
                }
                else if (i == polygonsLength - 1) //Last side face of the solid
                    polygons[i] = new int[] { pointsCount - lineCount - 1, 0, pointsCount - lineCount, pointsCount - 1 };
                else//Side faces of the solid
                {
                    polygons[i] = new int[] { leftFlag, leftFlag + 1, rightFlag, rightFlag - 1 };
                    leftFlag++;
                    rightFlag++;
                }

            }

            Vector3D[] vertices = points;
            if (vertices == null || vertices.Length == 0 || polygons == null || polygons.Length == 0)
            {
                return new Solid();
            }

            // Create polygons from the vertex data
            return Solid.FromPolygons(
                polygons.Select((int[] indices) =>
                {
                    // For each polygon, map its indices to the corresponding vertices
                    var polygonVertices = indices.Select(index => new Vertex(vertices[index], new Vector2D(0, 0))).ToList();
                    return new Csg.Polygon(polygonVertices);
                }).ToList()
            );

        }

        public Solid createCylinder(DrawingEntities entity, double height)
        {
            Solid cylinders = new Solid();
            foreach (netDxf.Entities.Circle circle in entity.Circles)
            {
                Vector3D start = (true ? new Vector3D(circle.Center.X, circle.Center.Y, 0.0) : new Vector3D(0.0, 0.0, 0.0));
                Vector3D end = (true ? new Vector3D(circle.Center.X, circle.Center.Y, height * 2 / 2.0) : new Vector3D(0.0, height, 0.0));
                Solid cylinder = Cylinder(new CylinderOptions
                {
                    Start = start,
                    End = end,
                    RadiusStart = circle.Radius,
                    RadiusEnd = circle.Radius,
                    Resolution = 100

                });
                cylinders = UnionForNonIntersecting(cylinders, cylinder);

            }

            return cylinders;
        }

        public void addLines(DrawingEntities entity)
        {
            readDxfLines(entity.Lines);
            readDxfPolylines2D(entity.Polylines2D);
            readDxfArcs(entity.Arcs);

        }

        public void readDxfLines(IEnumerable<netDxf.Entities.Line> dxfLines)
        {
            foreach (netDxf.Entities.Line line in dxfLines)
            {
                var startPoint = new System.Windows.Point(line.StartPoint.X, line.StartPoint.Y);
                var endPoint = new System.Windows.Point(line.EndPoint.X, line.EndPoint.Y);
                var length = Math.Sqrt(Math.Pow(line.StartPoint.X - line.EndPoint.X, 2) + Math.Pow(line.StartPoint.Y - line.EndPoint.Y, 2));
                var angle = Math.Atan((line.StartPoint.X - line.EndPoint.X) / (line.StartPoint.Y - line.EndPoint.Y));
                lines.Add(new Line(startPoint, endPoint, length, angle));
            }
        }

        public void readDxfPolylines2D(IEnumerable<netDxf.Entities.Polyline2D> dxfPolylines2D)
        {
            foreach (var polyline2D in dxfPolylines2D)
            {

                for (int i = 0; i < polyline2D.Vertexes.Count - 1; i++)
                {
                    // Get the start and end points of each segment
                    var startPoint = new System.Windows.Point(polyline2D.Vertexes[i].Position.X, polyline2D.Vertexes[i].Position.Y);
                    var endPoint = new System.Windows.Point(polyline2D.Vertexes[i + 1].Position.X, polyline2D.Vertexes[i + 1].Position.Y);
                    var length = Math.Sqrt(Math.Pow(startPoint.X - endPoint.X, 2) + Math.Pow(startPoint.Y - endPoint.Y, 2));
                    var angle = Math.Atan((startPoint.X - endPoint.X) / (startPoint.Y - endPoint.Y));
                    lines.Add(new Line(startPoint, endPoint, length, angle));
                }

            }
        }

        public void readDxfArcs(IEnumerable<netDxf.Entities.Arc> dxfArcs)
        {
            foreach (var arc in dxfArcs)
            {
                var points = new Point3DCollection();
                double startRad = 0;
                double endRad = 0;
                double segments = 70;
                if (arc.StartAngle > arc.EndAngle)
                {
                    startRad = arc.StartAngle * Math.PI / 180.0;
                    endRad = arc.EndAngle * Math.PI / 180.0;
                }
                else if (arc.StartAngle < arc.EndAngle)
                {
                    startRad = -arc.StartAngle * Math.PI / 180.0;
                    endRad = -arc.EndAngle * Math.PI / 180.0;
                }

                // Calculate the angle step
                double angleStep = (startRad - endRad) / segments;

                System.Windows.Point pStart = new System.Windows.Point(0, 0);
                System.Windows.Point pEnd = new System.Windows.Point(0, 0);

                double angle = arc.StartAngle * Math.PI / 180.0;
                double x = arc.Center.X + (arc.Radius * Math.Cos(angle));
                double y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
                pStart = new System.Windows.Point(x, y);

                // Generate the points along the arc
                for (int i = 1; i <= segments; i++)
                {
                    angle = arc.StartAngle * Math.PI / 180.0;
                    angle = angle + i * angleStep;
                    x = arc.Center.X + (arc.Radius * Math.Cos(angle));
                    y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
                    pEnd = new System.Windows.Point(x, y);
                    var length = Math.Sqrt(Math.Pow(pStart.X - pEnd.X, 2) + Math.Pow(pStart.Y - pEnd.Y, 2));
                    var lineAngle = Math.Atan((pStart.X - pEnd.X) / (pStart.Y - pEnd.Y));
                    lines.Add(new Line(pStart, pEnd, length, lineAngle));
                    pStart = pEnd;
                }
            }
        }
        
        public List<List<Line>> findConnectedLines()
        {
            List<List<Line>> squares = new List<List<Line>>();

            while (lines.Count > 0)
            {
                List<Line> square = new List<Line>();
                if (lines.Count == 1)
                {
                    square.Add(lines[0]);
                    lines.RemoveAt(0);
                }
                else
                {
                    Line line = lines[0];
                    square.Add(line);
                    lines.RemoveAt(0);
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (Math.Abs((line.endPoint.X - lines[i].startPoint.X)) < 0.5 && Math.Abs((line.endPoint.Y - lines[i].startPoint.Y)) < 0.5)
                        {
                            lines[i].startPoint = line.endPoint;
                            line = lines[i];
                            square.Add(line);
                            lines.RemoveAt(i);
                            i = -1;
                        }
                        else if (Math.Abs((line.endPoint.X - lines[i].endPoint.X)) < 0.5 && Math.Abs((line.endPoint.Y - lines[i].endPoint.Y)) < 0.5)
                        {
                            System.Windows.Point endPoint = lines[i].startPoint;
                            lines[i].startPoint = line.endPoint;
                            lines[i].endPoint = endPoint;
                            line = lines[i];
                            square.Add(line);
                            lines.RemoveAt(i);
                            i = -1;
                        }
                        //else if ((line.endPoint.X == lines[i].startPoint.X) && (line.endPoint.Y - lines[i].startPoint.Y < 3))
                        //{
                        //    line = lines[i];
                        //    square.Add(line);
                        //    lines.RemoveAt(i);
                        //    i = -1;
                        //}else if ((line.endPoint.X - lines[i].startPoint.X < 3) && (line.endPoint.Y == lines[i].startPoint.Y))
                        //{
                        //    line = lines[i];
                        //    square.Add(line);
                        //    lines.RemoveAt(i);
                        //    i = -1;
                        //}
                        //else if ((line.startPoint.X == lines[i].endPoint.X) && (line.startPoint.Y == lines[i].endPoint.Y))
                        //{
                        //    line = lines[i];
                        //    square.Add(line);
                        //    lines.RemoveAt(i);
                        //    i = 0;
                        //}
                        //else if ((line.endPoint.X == lines[i].endPoint.X) && (line.endPoint.Y == lines[i].endPoint.Y))
                        //{
                        //    line = lines[i];
                        //    square.Add(line);
                        //    lines.RemoveAt(i);
                        //    i = 0;
                        //}
                        //else if ((line.endPoint.X == lines[i].startPoint.X) && (line.endPoint.Y == lines[i].startPoint.Y))
                        //{
                        //    line = lines[i];
                        //    square.Add(line);
                        //    lines.RemoveAt(i);
                        //    i = 0;
                        //}
                    }
                    squares.Add(square);
                }
            }
            return squares;
        }

        //    public Solid IrregularShape()
        //    {
        //        Vector3D[] points = new Vector3D[]
        //{
        //    new Vector3D(0, 0, 0),   // Point 0
        //    new Vector3D(2, 0, 0),   // Point 1
        //    new Vector3D(2, 2, 0),   // Point 2
        //    new Vector3D(0, 2, 0),   // Point 3

        //    new Vector3D(0, 0, 1),   // Point 4
        //    new Vector3D(2, 0, 1),   // Point 5
        //    new Vector3D(2, 2, 1),   // Point 6
        //    new Vector3D(0, 2, 1),    // Point 7

        //    new Vector3D(1, 0, 0),   // Point 8
        //    new Vector3D(3, 0, 0),   // Point 9
        //    new Vector3D(3, -1, 0),   // Point 10
        //    new Vector3D(2, -2, 0),   // Point 11
        //    new Vector3D(1, -1, 0),   // Point 12

        //    new Vector3D(1, 0, 1),   // Point 13
        //    new Vector3D(3, 0, 1),   // Point 14
        //    new Vector3D(3, -1, 1),   // Point 15
        //    new Vector3D(2, -2, 1),   // Point 16
        //    new Vector3D(1, -1, 1),   // Point 17

        //};
        //        int[][] polygons = new int[][]
        //{
        //    new int[] { 0, 1, 2, 3 },   // Bottom face
        //    new int[] { 4, 5, 6, 7 },   // Top face
        //    new int[] { 0, 1, 5, 4 },   // Side face
        //    new int[] { 1, 2, 6, 5 },   // Side face
        //    new int[] { 2, 3, 7, 6 },   // Side face
        //    new int[] { 3, 0, 4, 7 },   // Side face


        //    new int[] { 8, 9, 10, 11, 12 },   // Bottom face
        //    new int[] { 13, 14, 15,16,17 },   // Top face
        //    new int[] { 8, 9, 14, 13 },   // Side face
        //    new int[] { 9, 10, 15, 14 },   // Side face
        //    new int[] { 10, 11, 16, 15 },   // Side face
        //    new int[] { 11, 12, 17, 16 },    // Side face
        //    new int[] { 12, 8, 13, 17 }    // Side face
        //};

        //        Vector3D[] vertices = points;  // Array of irregular vertices
        //                                       // Each element defines a polygon using vertex indices

        //        // Make sure there are valid vertices and polygons
        //        if (vertices == null || vertices.Length == 0 || polygons == null || polygons.Length == 0)
        //        {
        //            return new Solid();
        //        }

        //        // Create polygons from the vertex data
        //        return Solid.FromPolygons(
        //            polygons.Select((int[] indices) =>
        //            {
        //                // For each polygon, map its indices to the corresponding vertices
        //                var polygonVertices = indices.Select(index => new Vertex(vertices[index], new Vector2D(0, 0))).ToList();
        //                return new Csg.Polygon(polygonVertices);
        //            }).ToList()
        //        );
        //    }
        //public Solid Prism(double height)
        //{
        //    int sides = 4; // Number of sides at the base
        //    Vector3D c = new Vector3D(0, 0, 0);
        //    Vector3D r = new Vector3D(2000, 2000, 2000);

        //    if (r.X == 0.0 || r.Y == 0.0 || r.Z == 0.0 || sides < 3)
        //    {
        //        return new Solid();
        //    }

        //    // Generate vertices for the base and top polygons of the prism
        //    List<int[]> prismData = new List<int[]>();
        //    List<Vector3D> baseVertices = new List<Vector3D>();
        //    List<Vector3D> topVertices = new List<Vector3D>();

        //    // Calculate the angle between each vertex of the base
        //    double angleStep = 2 * Math.PI / sides;

        //    for (int i = 0; i < sides; i++)
        //    {
        //        double angle = i * angleStep;
        //        double x = c.X + r.X * Math.Cos(angle); // Base vertex X coordinate
        //        double y = c.Y + r.Y * Math.Sin(angle); // Base vertex Y coordinate
        //        baseVertices.Add(new Vector3D(x, y, c.Z)); // Bottom base vertex
        //        topVertices.Add(new Vector3D(x, y, c.Z + height )); // Top base vertex
        //    }

        //    // Add base and top polygons to the data
        //    prismData.Add(baseVertices.Select((v, i) => i).ToArray());  // Bottom face
        //    prismData.Add(topVertices.Select((v, i) => i + sides).ToArray());  // Top face

        //    // Add side faces (each rectangle formed between consecutive vertices on base and top)
        //    for (int i = 0; i < sides; i++)
        //    {
        //        int next = (i + 1) % sides; // Wrap around to the first vertex
        //        prismData.Add(new int[] { i, next, next + sides, i + sides });
        //    }

        //    // Create polygons based on the vertices
        //    return Solid.FromPolygons(
        //        prismData.Select((int[] info) =>
        //        {
        //            var vertices = info.Select(i => new Vertex(i < sides ? baseVertices[i] : topVertices[i - sides], new Vector2D(0, 0))).ToList();
        //            return new Csg.Polygon(vertices);
        //        }).ToList()
        //    );
        //}
        //public Solid createManual(double height)
        //{
        //    Solid solids = new Solid();


        //    double length = 100;
        //    double width = 100;
        //    for (int i = 0; i < 4; i++)
        //    {

        //        for (int j = 0; j < 4; j++)
        //        {
        //            Solid cube = Cube(size: new Vector3D(length, height, width)).Transform(Matrix4x4.RotationY(45)).Translate((i * 200) + 120, 0, (j * 180) + 120);

        //            solids = Union(solids, cube);
        //            for (int k = 0; k < 4; k++)
        //            {
        //                double centerX = 0, centerY = 0, radius = 10;
        //                if (k == 0)
        //                {
        //                    centerX = (i * 200) + 120 + radius;
        //                    centerY = (j * 180) + 120;
        //                }
        //                else if (k == 1)
        //                {
        //                    centerX = Math.Sqrt(2) * length + (i * 200) + 120 - radius;
        //                    centerY = (j * 180) + 120;
        //                }
        //                else if (k == 2)
        //                {
        //                    centerX = Math.Sqrt(2) * length / 2 + (i * 200) + 120;
        //                    centerY = (j * 180) + 120 - Math.Sqrt(2) * length / 2 + radius;
        //                }
        //                else if (k == 3)
        //                {
        //                    centerX = Math.Sqrt(2) * length / 2 + (i * 200) + 120;
        //                    centerY = (j * 180) + 120 + Math.Sqrt(2) * length / 2 - radius;
        //                }
        //                Vector3D start = (true ? new Vector3D(centerX, (0.0 - height * 2) / 2.0, centerY) : new Vector3D(0.0, 0.0, 0.0));
        //                Vector3D end = (true ? new Vector3D(centerX, height * 2 / 2.0, centerY) : new Vector3D(0.0, height, 0.0));
        //                Solid cylinder = Cylinder(new CylinderOptions
        //                {
        //                    Start = start,
        //                    End = end,
        //                    RadiusStart = radius,
        //                    RadiusEnd = radius,
        //                    Resolution = 100

        //                });
        //                solids = Union(solids, cylinder);
        //            }

        //        }
        //    }

        //    return solids;
        //}



        #endregion


        #region WindowsMedia

        public Model3DGroup readDFX(string filePath, double height)
        {
            var modelGroup = new Model3DGroup();
            var dxf = DxfDocument.Load(filePath);
            var entity = dxf.Entities;

            foreach (var element in entity.All)
            {
                if (element.GetType() == typeof(netDxf.Entities.Line))
                    modelGroup.Children.Add(createLine((netDxf.Entities.Line)element, height));
                else if (element.GetType() == typeof(netDxf.Entities.Circle))
                    modelGroup.Children.Add(createPipe((netDxf.Entities.Circle)element, height));
                else if (element.GetType() == typeof(netDxf.Entities.Polyline2D))
                    modelGroup.Children.Add(createPolyLine((netDxf.Entities.Polyline2D)element, height));
                else if (element.GetType() == typeof(netDxf.Entities.Solid))
                    modelGroup.Children.Add(createSolid((netDxf.Entities.Solid)element, height));
                else if (element.GetType() == typeof(netDxf.Entities.Arc))
                    modelGroup.Children.Add(createArcWithLines((netDxf.Entities.Arc)element, height, 50));
            }

            return modelGroup;
        }

        public GeometryModel3D createLine(double startPointX, double startPointY, double endPointX, double endPointY, double height, double width)
        {


            var meshBuilder = new MeshBuilder();

            //Center Point of the box
            Point3D center = new Point3D((startPointX + endPointX) / 2, (startPointY + endPointY) / 2, 0 + height / 2);

            //Length Calculation
            double length = Math.Sqrt(Math.Pow((startPointX - endPointX), 2) + Math.Pow((startPointY - endPointY), 2));

            //Angle Calculatiion
            double angle = Math.Atan((startPointX - endPointX) / (startPointY - endPointY));
            angle = -angle * 180 / Math.PI;

            //Creating Model
            meshBuilder.AddBox(center, width, length, height);
            var mesh = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            var model = new GeometryModel3D(mesh, material);

            //Rotating model
            System.Windows.Media.Media3D.Vector3D axis = new System.Windows.Media.Media3D.Vector3D(0, 0, 1); //In case you want to rotate it about the z-axis
            Matrix3D transformationMatrix = model.Transform.Value; //Gets the matrix indicating the current transformation value
            transformationMatrix.RotateAt(new Quaternion(axis, angle), center); //Makes a rotation transformation over this matrix
            model.Transform = new MatrixTransform3D(transformationMatrix); //Applies the transformation to model

            return model;
        }

        public GeometryModel3D createLine(netDxf.Entities.Line line, double height)
        {


            var meshBuilder = new MeshBuilder();

            //Center Point of the box
            Point3D center = new Point3D((line.StartPoint.X + line.EndPoint.X) / 2, (line.StartPoint.Y + line.EndPoint.Y) / 2, 0 + height / 2);
            double width = 0.01;

            //Length Calculation
            double length = Math.Sqrt(Math.Pow((line.StartPoint.X - line.EndPoint.X), 2) + Math.Pow((line.StartPoint.Y - line.EndPoint.Y), 2));

            //Angle Calculatiion
            double angle = Math.Atan((line.StartPoint.X - line.EndPoint.X) / (line.StartPoint.Y - line.EndPoint.Y));
            angle = -angle * 180 / Math.PI;

            //Creating Model
            meshBuilder.AddBox(center, width, length, height);
            var mesh = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            var model = new GeometryModel3D(mesh, material);

            //Rotating model
            System.Windows.Media.Media3D.Vector3D axis = new System.Windows.Media.Media3D.Vector3D(0, 0, 1); //In case you want to rotate it about the z-axis
            Matrix3D transformationMatrix = model.Transform.Value; //Gets the matrix indicating the current transformation value
            transformationMatrix.RotateAt(new Quaternion(axis, angle), center); //Makes a rotation transformation over this matrix
            model.Transform = new MatrixTransform3D(transformationMatrix); //Applies the transformation to model

            return model;
        }

        public GeometryModel3D createPipe(netDxf.Entities.Circle circle, double height)
        {
            var meshBuilder = new MeshBuilder();

            double centerX = circle.Center.X;
            double centerY = circle.Center.Y;
            double radius = circle.Radius;

            meshBuilder.AddPipe(new Point3D(centerX, centerY, 0), new Point3D(centerX, centerY, height), radius, radius + 0.1, 70);
            var mesh = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            var model = new GeometryModel3D(mesh, material);

            return model;
        }
        public Model3DGroup createPolyLine(netDxf.Entities.Polyline2D polyline2D, double height)
        {
            Model3DGroup modelGroup = new Model3DGroup();
            var meshBuilder = new MeshBuilder();
            for (int i = 0; i < polyline2D.Vertexes.Count - 1; i++)
            {
                // Get the start and end points of each segment
                var start = polyline2D.Vertexes[i].Position;
                var end = polyline2D.Vertexes[i + 1].Position;
                modelGroup.Children.Add(createLine(start.X, start.Y, end.X, end.Y, height, 0.01));
            }
            return modelGroup;
        }

        public GeometryModel3D createSolid(netDxf.Entities.Solid solid, double height)
        {
            var meshBuilder = new MeshBuilder();
            var p1 = solid.FirstVertex;
            var p2 = solid.SecondVertex;
            var p3 = solid.ThirdVertex;
            var p4 = solid.FourthVertex;

            var points = new Point3DCollection
            {
                new Point3D(p2.X,p2.Y, 0),
                new Point3D(p4.X,p4.Y, 0),
                new Point3D(p3.X,p3.Y, 0),
                new Point3D(p2.X,p2.Y, 0)
            };
            meshBuilder.AddPolygon(points);
            var mesh = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            var model = new GeometryModel3D(mesh, material);
            return model;
        }

        public GeometryModel3D createArc(netDxf.Entities.Arc arc, double height, int segments)
        {
            var meshBuilder = new MeshBuilder();

            // Create a collection to hold the arc points
            var points = new Point3DCollection();
            double startRad = 0;
            double endRad = 0;
            if (arc.StartAngle > arc.EndAngle)
            {
                startRad = arc.StartAngle * Math.PI / 180.0;
                endRad = arc.EndAngle * Math.PI / 180.0;
            }
            else if (arc.StartAngle < arc.EndAngle)
            {
                startRad = -arc.StartAngle * Math.PI / 180.0;
                endRad = -arc.EndAngle * Math.PI / 180.0;
            }

            // Calculate the angle step
            double angleStep = (startRad - endRad) / segments;
            Point3D pStart = new Point3D(0, 0, 0);
            Point3D pEnd = new Point3D(0, 0, 0);

            // Generate the points along the arc
            for (int i = 0; i <= segments; i++)
            {
                double angle = arc.StartAngle * Math.PI / 180.0;
                angle = angle + i * angleStep;
                double x = arc.Center.X + (arc.Radius * Math.Cos(angle));
                double y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
                points.Add(new Point3D(x, y, height - 0.05)); // Z-axis is zero for a 2D arc
            }

            meshBuilder.AddTube(points, 0.1, segments, false);
            var mesh = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            var model = new GeometryModel3D(mesh, material);
            return model;
        }

        public Model3DGroup createArcWithLines(netDxf.Entities.Arc arc, double height, int segments)
        {
            Model3DGroup modelGroup = new Model3DGroup();
            var meshBuilder = new MeshBuilder();

            // Create a collection to hold the arc points
            var points = new Point3DCollection();
            double startRad = 0;
            double endRad = 0;
            if (arc.StartAngle > arc.EndAngle)
            {
                startRad = arc.StartAngle * Math.PI / 180.0;
                endRad = arc.EndAngle * Math.PI / 180.0;
            }
            else if (arc.StartAngle < arc.EndAngle)
            {
                startRad = -arc.StartAngle * Math.PI / 180.0;
                endRad = -arc.EndAngle * Math.PI / 180.0;
            }

            // Calculate the angle step
            double angleStep = (startRad - endRad) / segments;

            Point3D pStart = new Point3D(0, 0, 0);
            Point3D pEnd = new Point3D(0, 0, 0);

            double angle = arc.StartAngle * Math.PI / 180.0;
            double x = arc.Center.X + (arc.Radius * Math.Cos(angle));
            double y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
            pStart = new Point3D(x, y, 0);

            // Generate the points along the arc
            for (int i = 1; i <= segments; i++)
            {
                angle = arc.StartAngle * Math.PI / 180.0;
                angle = angle + i * angleStep;
                x = arc.Center.X + (arc.Radius * Math.Cos(angle));
                y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
                pEnd = new Point3D(x, y, 0);
                modelGroup.Children.Add(createLine(pStart.X, pStart.Y, pEnd.X, pEnd.Y, height, 0.01));
                pStart = pEnd;
            }
            var sdas = modelGroup.Children;
            return modelGroup;
        }

        public Model3DGroup createPolygon(Model3DGroup modelgroup, double height)
        {
            var meshBuilder = new MeshBuilder();
            var group = new Model3DGroup();
            var polygonPoints = new List<List<Point3D>>();
            int i = 0;
            var mesh = new MeshGeometry3D();
            foreach (var points in polygonPoints)
            {
                meshBuilder.AddPolygon(points);
                mesh = meshBuilder.ToMesh();
                var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
                var model = new GeometryModel3D(mesh, material);
                group.Children.Add(model);

                i++;
            }
            return group;
        }
        #endregion
    }
}
