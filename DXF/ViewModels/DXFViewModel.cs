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

namespace DXF.ViewModels
{
    public class DXFViewModel : ViewModelBase
    {
        public ICommand ApplyCommand { get; }
        public ICommand SelectFileCommand { get; }
        public ICommand CreateManualCommand { get; }

        public DXFViewModel(NavigationStore navigation, double height)
        {
            Height = Convert.ToString(height);
            //Model = readDFX("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\SamplesV2\\Separatör_Pneumatrol_rev0.dxf", height);
            ApplyCommand = new ApplyCommand(this, navigation);
            SelectFileCommand = new SelectFileCommand(this, navigation);
            CreateManualCommand = new CreateManualCommand(this, navigation);
            Vertex[] vertices = new Vertex[5];
            vertices[0] = new Vertex(new Vector3D(0, 0, 0), new Vector2D(0, 0));
            vertices[1] = new Vertex(new Vector3D(2, 0, 0), new Vector2D(0, 0));
            vertices[2] = new Vertex(new Vector3D(2, 2, 0), new Vector2D(0, 0));
            vertices[3] = new Vertex(new Vector3D(1, 3, 0), new Vector2D(0, 0));
            vertices[4] = new Vertex(new Vector3D(0, 2, 0), new Vector2D(0, 0));
            var sol = new Csg.Polygon(vertices);
            Solid solid = new Solid();
            StreamWriter writer = new StreamWriter("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\3DModels\\StlOutput.stl");
            sol.WriteStl(writer);
            writer.Close();
            var importer = new ModelImporter();
            var modelGroup = importer.Load("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\3DModels\\StlOutput.stl");
            //Model = modelGroup;
            //Model = createPolygon(Model, borders, 5);
            //CombinedGeometry = createPoligon(5);
            //CombinedGeometry.
            //if(FilePath != null)
            //    Model = GetSolid(FilePath, height);
            //Model.Children.Add(createPolygon(Model, height));

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

        private CombinedGeometry combinedGeometry;
        public CombinedGeometry CombinedGeometry
        {
            get
            {
                return combinedGeometry;
            }
            set
            {
                combinedGeometry = value;
                OnPropertyChanged(nameof(CombinedGeometry));
            }
        }

        #region SolidGeometry

        public Model3DGroup GetSolid(string filePath, double height)
        {
            var dxf = DxfDocument.Load(filePath);
            var entity = dxf.Entities;

            double[] sizes = findSizes(entity);

            Solid body = Cube(size: new Vector3D(sizes[0], height, sizes[1])).Translate(sizes[2], 0, sizes[3]);
            Solid holes = createCylinder(entity, height);

            Solid asd = Difference(body, holes);
            asd = asd.Transform(Matrix4x4.RotationX(90));

            StreamWriter writer = new StreamWriter("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\3DModels\\StlOutput.stl");
            asd.WriteStl("StlOutput", writer);
            writer.Close();
            findSquares(entity);
            var importer = new ModelImporter();
            var modelGroup = importer.Load("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\3DModels\\StlOutput.stl");
            //Model3DGroup modelGroup = new Model3DGroup();
            return modelGroup;
        }
        public Model3DGroup GetSolid(double height)
        {

            Solid body = Cube(size: new Vector3D(1000, height, 800)).Translate(0, 0, 0);
            Solid holes = createManual(height);

            Solid asd = Difference(body, holes);
            asd = asd.Transform(Matrix4x4.RotationX(90));

            StreamWriter writer = new StreamWriter("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\3DModels\\StlOutput.stl");
            asd.WriteStl("StlOutput", writer);
            writer.Close();

            var importer = new ModelImporter();
            var modelGroup = importer.Load("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\3DModels\\StlOutput.stl");
            //Model3DGroup modelGroup = new Model3DGroup();
            return modelGroup;
        }
        public Solid createManual(double height)
        {
            Solid solids = new Solid();
            double length = 100;
            double width = 100;
            for(int i = 0; i < 4; i++)
            {
                
                for(int j = 0; j < 4; j++)
                {
                    Solid cube = Cube(size: new Vector3D(length, height, width)).Transform(Matrix4x4.RotationY(45)).Translate((i * 200) + 120, 0, (j * 180) + 120);
                    
                    solids = Union(solids, cube);
                    for(int k = 0; k < 4; k++)
                    {
                        double centerX = 0, centerY = 0, radius = 10;
                        if(k == 0)
                        {
                            centerX = (i * 200) + 120 + radius;
                            centerY = (j * 180) + 120;
                        }
                        else if(k == 1)
                        {
                            centerX = Math.Sqrt(2) * length +  (i * 200) + 120 - radius;
                            centerY = (j * 180) + 120;
                        }else if(k == 2)
                        {
                            centerX = Math.Sqrt(2) * length / 2 + (i * 200) + 120;
                            centerY = (j * 180) + 120 - Math.Sqrt(2) * length /2 + radius;
                        }else if(k == 3)
                        {
                            centerX = Math.Sqrt(2) * length / 2 + (i * 200) + 120;
                            centerY = (j * 180) + 120 + Math.Sqrt(2) * length / 2 - radius;
                        }
                        Vector3D start = (true ? new Vector3D(centerX, (0.0 - height * 2) / 2.0, centerY) : new Vector3D(0.0, 0.0, 0.0));
                        Vector3D end = (true ? new Vector3D(centerX, height * 2 / 2.0, centerY) : new Vector3D(0.0, height, 0.0));
                        Solid cylinder = Cylinder(new CylinderOptions
                        {
                            Start = start,
                            End = end,
                            RadiusStart = radius,
                            RadiusEnd = radius,
                            Resolution = 100

                        });
                        solids = Union(solids, cylinder);
                    }
                    
                }
                //Vector3D start = (true ? new Vector3D(circle.Center.X, (0.0 - height * 2) / 2.0, circle.Center.Y) : new Vector3D(0.0, 0.0, 0.0));
                //Vector3D end = (true ? new Vector3D(circle.Center.X, height * 2 / 2.0, circle.Center.Y) : new Vector3D(0.0, height, 0.0));
                //Solid cylinder = Cylinder(new CylinderOptions
                //{
                //    Start = start,
                //    End = end,
                //    RadiusStart = circle.Radius,
                //    RadiusEnd = circle.Radius,
                //    Resolution = 100

                //});


            }

            return solids;
        }


        public Solid createCylinder(DrawingEntities entity, double height)
        {
            Solid cylinders = new Solid();
            foreach (netDxf.Entities.Circle circle in entity.Circles)
            {
                Vector3D start = (true ? new Vector3D(circle.Center.X, (0.0 - height * 2) / 2.0, circle.Center.Y) : new Vector3D(0.0, 0.0, 0.0));
                Vector3D end = (true ? new Vector3D(circle.Center.X, height * 2 / 2.0, circle.Center.Y) : new Vector3D(0.0, height, 0.0));
                Solid cylinder = Cylinder(new CylinderOptions
                {
                    Start = start,
                    End = end,
                    RadiusStart = circle.Radius,
                    RadiusEnd = circle.Radius,
                    Resolution = 100

                });
                cylinders = Union(cylinders, cylinder);

            }

            return cylinders;
        }

        public Solid createSquare(DrawingEntities entity, double height)
        {
            Solid cylinders = new Solid();
            foreach (netDxf.Entities.Line line in entity.Lines)
            {



            }

            return cylinders;
        }

        public List<List<Line>> findSquares(DrawingEntities entity)
        {
            List<List<Line>> squares = new List<List<Line>>();
            List<Line> square = new List<Line>();
            List<Line> lines = new List<Line>();
            var asd = entity.Lines.GetEnumerator();
            foreach (netDxf.Entities.Line line in entity.Lines)
            {
                var startPoint = new Point(line.StartPoint.X, line.StartPoint.Y);
                var endPoint = new Point(line.EndPoint.X, line.EndPoint.Y);
                var length = Math.Sqrt(Math.Pow(line.StartPoint.X - line.EndPoint.X, 2) + Math.Pow(line.StartPoint.Y - line.EndPoint.Y, 2));
                var angle = Math.Atan((line.StartPoint.X - line.EndPoint.X) / (line.StartPoint.Y - line.EndPoint.Y));
                lines.Add(new Line(startPoint, endPoint, length, angle));
            }            
            while (lines.Count > 0)
            {
                if(lines.Count == 1)
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
                        if((line.startPoint.X == lines[i].startPoint.X) && (line.startPoint.Y == lines[i].startPoint.Y))
                        {
                            line.connectedLines++;

                        }
                    }
                }
            }
            return squares;
        }


        public double[] findSizes(DrawingEntities entity)
        {
            double[] sizes = new double[4];
            sizes[2] = 9999;
            sizes[3] = 9999;
            foreach (netDxf.Entities.Line line in entity.Lines)
            {
                double length = Math.Sqrt(Math.Pow((line.StartPoint.X - line.EndPoint.X), 2) + Math.Pow((line.StartPoint.Y - line.EndPoint.Y), 2));
                if (length > sizes[1] && length > sizes[0])
                {
                    sizes[1] = sizes[0];
                    sizes[0] = length;
                }
                else if (length > sizes[1] && length < sizes[0])
                {
                    sizes[1] = length;
                }

                if (line.StartPoint.X < sizes[2])
                    sizes[2] = line.StartPoint.X;
                if (line.EndPoint.X < sizes[2])
                    sizes[2] = line.EndPoint.X;

                if (line.StartPoint.Y < sizes[3])
                    sizes[3] = line.StartPoint.Y;
                if (line.EndPoint.Y < sizes[3])
                    sizes[3] = line.EndPoint.Y;
            }
            return sizes;
        }

        #endregion


        #region WindowsMedia

        //private Model3DGroup readDFX(string filePath, double height)
        //{
        //    var modelGroup = new Model3DGroup();
        //    var dxf = DxfDocument.Load(filePath);
        //    var entity = dxf.Entities;
        //    int a = 0;
        //    foreach (var element in entity.All)
        //    {
        //        if (element.GetType() == typeof(netDxf.Entities.Line))
        //            modelGroup.Children.Add(createLine((netDxf.Entities.Line)element, height));
        //        else if (element.GetType() == typeof(netDxf.Entities.Circle))
        //            modelGroup.Children.Add(createPipe((netDxf.Entities.Circle)element, height));
        //        else if (element.GetType() == typeof(netDxf.Entities.Polyline2D))
        //            modelGroup.Children.Add(createPolyLine((netDxf.Entities.Polyline2D)element, height));
        //        else if (element.GetType() == typeof(netDxf.Entities.Solid))
        //            modelGroup.Children.Add(createSolid((netDxf.Entities.Solid)element, height));
        //        else if (element.GetType() == typeof(netDxf.Entities.Arc))
        //            modelGroup.Children.Add(createArcWithLines((netDxf.Entities.Arc)element, height, 50));
        //    }

        //    a = 2;
        //    return modelGroup;
        //}

        //public GeometryModel3D createLine(double startPointX, double startPointY, double endPointX, double endPointY, double height, double width)
        //{
        //    List<Point> points = new List<Point>
        //    {
        //        new Point(startPointX, startPointY),
        //        new Point(endPointX, endPointY)
        //    };

        //    var meshBuilder = new MeshBuilder();
        //    //Center Point of the box
        //    Point3D center = new Point3D((startPointX + endPointX) / 2, (startPointY + endPointY) / 2, 0 + height / 2);
        //    //double width = 0.01;
        //    //Length Calculation
        //    double length = Math.Sqrt(Math.Pow((startPointX - endPointX), 2) + Math.Pow((startPointY - endPointY), 2));

        //    //Angle Calculatiion
        //    double angle = Math.Atan((startPointX - endPointX) / (startPointY - endPointY));
        //    angle = -angle * 180 / Math.PI;

        //    //Creating Model
        //    meshBuilder.AddBox(center, width, length, height);
        //    var mesh = meshBuilder.ToMesh();
        //    var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
        //    var model = new GeometryModel3D(mesh, material);

        //    //Rotating model
        //    System.Windows.Media.Media3D.Vector3D axis = new System.Windows.Media.Media3D.Vector3D(0, 0, 1); //In case you want to rotate it about the z-axis
        //    Matrix3D transformationMatrix = model.Transform.Value; //Gets the matrix indicating the current transformation value
        //    transformationMatrix.RotateAt(new Quaternion(axis, angle), center); //Makes a rotation transformation over this matrix
        //    model.Transform = new MatrixTransform3D(transformationMatrix); //Applies the transformation to your model

        //    Line border = new Line(points[0], points[1], length, angle);
        //    borders.Add(border);
        //    return model;
        //}

        //public GeometryModel3D createLine(netDxf.Entities.Line line, double height)
        //{
        //    List<Point> points = new List<Point>
        //    {
        //        new Point(line.StartPoint.X, line.StartPoint.Y),
        //        new Point(line.EndPoint.X, line.EndPoint.Y)
        //    };

        //    var meshBuilder = new MeshBuilder();
        //    //Center Point of the box
        //    Point3D center = new Point3D((line.StartPoint.X + line.EndPoint.X) / 2, (line.StartPoint.Y + line.EndPoint.Y) / 2, 0 + height / 2);
        //    double width = 0.01;
        //    //Length Calculation
        //    double length = Math.Sqrt(Math.Pow((line.StartPoint.X - line.EndPoint.X), 2) + Math.Pow((line.StartPoint.Y - line.EndPoint.Y), 2));

        //    //Angle Calculatiion
        //    double angle = Math.Atan((line.StartPoint.X - line.EndPoint.X) / (line.StartPoint.Y - line.EndPoint.Y));
        //    angle = -angle * 180 / Math.PI;

        //    //Creating Model
        //    meshBuilder.AddBox(center, width, length, height);
        //    var mesh = meshBuilder.ToMesh();
        //    var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
        //    var model = new GeometryModel3D(mesh, material);

        //    //Rotating model
        //    System.Windows.Media.Media3D.Vector3D axis = new System.Windows.Media.Media3D.Vector3D(0, 0, 1); //In case you want to rotate it about the z-axis
        //    Matrix3D transformationMatrix = model.Transform.Value; //Gets the matrix indicating the current transformation value
        //    transformationMatrix.RotateAt(new Quaternion(axis, angle), center); //Makes a rotation transformation over this matrix
        //    model.Transform = new MatrixTransform3D(transformationMatrix); //Applies the transformation to your model

        //    Line border = new Line(points[0], points[1], length, angle);
        //    borders.Add(border);
            
        //    return model;
        //}

        //public GeometryModel3D createPipe(netDxf.Entities.Circle circle, double height)
        //{
        //    var meshBuilder = new MeshBuilder();

        //    double centerX = circle.Center.X;
        //    double centerY = circle.Center.Y;
        //    double radius = circle.Radius;

        //    meshBuilder.AddPipe(new Point3D(centerX, centerY, 0), new Point3D(centerX, centerY, height), radius, radius + 0.1, 70);
        //    var mesh = meshBuilder.ToMesh();
        //    var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
        //    var model = new GeometryModel3D(mesh, material);

        //    return model;
        //}
        //public Model3DGroup createPolyLine(netDxf.Entities.Polyline2D polyline2D, double height)
        //{
        //    Model3DGroup modelGroup = new Model3DGroup();
        //    var meshBuilder = new MeshBuilder();
        //    for (int i = 0; i < polyline2D.Vertexes.Count - 1; i++)
        //    {
        //        // Get the start and end points of each segment
        //        var start = polyline2D.Vertexes[i].Position;
        //        var end = polyline2D.Vertexes[i + 1].Position;
        //        modelGroup.Children.Add(createLine(start.X, start.Y, end.X, end.Y, height, 0.01));
        //    }
        //    return modelGroup;
        //}

        //public GeometryModel3D createSolid(netDxf.Entities.Solid solid, double height)
        //{
        //    var meshBuilder = new MeshBuilder();
        //    var p1 = solid.FirstVertex;
        //    var p2 = solid.SecondVertex;
        //    var p3 = solid.ThirdVertex;
        //    var p4 = solid.FourthVertex;

        //    //points.Add(new System.Windows.Point(p1.X,p1.Y));
        //    //points.Add(new System.Windows.Point(p2.X,p2.Y));
        //    //points.Add(new System.Windows.Point(p3.X,p3.Y));
        //    var points = new Point3DCollection
        //    {
        //        new Point3D(p2.X,p2.Y, 0),
        //        new Point3D(p4.X,p4.Y, 0),
        //        new Point3D(p3.X,p3.Y, 0),
        //        new Point3D(p2.X,p2.Y, 0)
        //    };
        //    meshBuilder.AddPolygon(points);
        //    var mesh = meshBuilder.ToMesh();
        //    var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
        //    var model = new GeometryModel3D(mesh, material);
        //    return model;
        //}

        //public GeometryModel3D createArc(netDxf.Entities.Arc arc, double height, int segments)
        //{
        //    var meshBuilder = new MeshBuilder();

        //    // Create a collection to hold the arc points
        //    var points = new Point3DCollection();
        //    double startRad = 0;
        //    double endRad = 0;
        //    if (arc.StartAngle > arc.EndAngle)
        //    {
        //        startRad = arc.StartAngle * Math.PI / 180.0;
        //        endRad = arc.EndAngle * Math.PI / 180.0;
        //    }
        //    else if (arc.StartAngle < arc.EndAngle)
        //    {
        //        startRad = -arc.StartAngle * Math.PI / 180.0;
        //        endRad = -arc.EndAngle * Math.PI / 180.0;
        //    }

        //    // Calculate the angle step
        //    double angleStep = (startRad - endRad) / segments;
        //    Point3D pStart = new Point3D(0, 0, 0);
        //    Point3D pEnd = new Point3D(0, 0, 0);

        //    // Generate the points along the arc
        //    for (int i = 0; i <= segments; i++)
        //    {
        //        double angle = arc.StartAngle * Math.PI / 180.0;
        //        angle = angle + i * angleStep;
        //        double x = arc.Center.X + (arc.Radius * Math.Cos(angle));
        //        double y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
        //        points.Add(new Point3D(x, y, height - 0.05)); // Z-axis is zero for a 2D arc
        //    }

        //    meshBuilder.AddTube(points, 0.1, segments, false);
        //    var mesh = meshBuilder.ToMesh();
        //    var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
        //    var model = new GeometryModel3D(mesh, material);
        //    return model;
        //}

        //public Model3DGroup createArcWithLines(netDxf.Entities.Arc arc, double height, int segments)
        //{
        //    Model3DGroup modelGroup = new Model3DGroup();
        //    var meshBuilder = new MeshBuilder();

        //    // Create a collection to hold the arc points
        //    var points = new Point3DCollection();
        //    double startRad = 0;
        //    double endRad = 0;
        //    if (arc.StartAngle > arc.EndAngle)
        //    {
        //        startRad = arc.StartAngle * Math.PI / 180.0;
        //        endRad = arc.EndAngle * Math.PI / 180.0;
        //    }
        //    else if (arc.StartAngle < arc.EndAngle)
        //    {
        //        startRad = -arc.StartAngle * Math.PI / 180.0;
        //        endRad = -arc.EndAngle * Math.PI / 180.0;
        //    }

        //    // Calculate the angle step
        //    double angleStep = (startRad - endRad) / segments;

        //    Point3D pStart = new Point3D(0, 0, 0);
        //    Point3D pEnd = new Point3D(0, 0, 0);

        //    double angle = arc.StartAngle * Math.PI / 180.0;
        //    double x = arc.Center.X + (arc.Radius * Math.Cos(angle));
        //    double y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
        //    pStart = new Point3D(x, y, 0);

        //    // Generate the points along the arc
        //    for (int i = 1; i <= segments; i++)
        //    {
        //        angle = arc.StartAngle * Math.PI / 180.0;
        //        angle = angle + i * angleStep;
        //        x = arc.Center.X + (arc.Radius * Math.Cos(angle));
        //        y = arc.Center.Y + (arc.Radius * Math.Sin(angle));
        //        pEnd = new Point3D(x, y, 0);
        //        modelGroup.Children.Add(createLine(pStart.X, pStart.Y, pEnd.X, pEnd.Y, height, 0.01));
        //        pStart = pEnd;
        //    }
        //    var sdas = modelGroup.Children;
        //    return modelGroup;
        //}

        //public Model3DGroup createPolygon(Model3DGroup modelgroup, List<List<Point>> borders, double height)
        //{
        //    var meshBuilder = new MeshBuilder();
        //    var group = new Model3DGroup();
        //    var polygonPoints = new List<List<Point3D>>();
        //    int i = 0;
        //    var mesh = new MeshGeometry3D();
        //    foreach (var points in polygonPoints)
        //    {
        //        meshBuilder.AddPolygon(points);
        //        mesh = meshBuilder.ToMesh();
        //        var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
        //        var model = new GeometryModel3D(mesh, material);
        //        group.Children.Add(model);

        //        i++;
        //    }
        //    return group;
        //}

        //public List<List<Line>> getConsequtiveLines(List<Line> lines)
        //{
        //    while (lines.Count > 0)
        //    {
        //        Line line = lines[0];
        //    }
        //    return null;
        //}


























        //public List<List<Point3D>> getPlynomIndices(Model3DGroup model, List<List<Point>> borders, double height)
        //{
        //    List<List<Point3D>> polynomIndices = new List<List<Point3D>>();
        //    Point3D p1, p2 = new Point3D(), p3 = new Point3D(), p4;

        //    bool isAssigned = false;
        //    int[,] matrix = getMatrix(model, borders);
        //    for (int i = 2; i < matrix.GetLength(0) - 2; i++)
        //    {
        //        int j = 2;
        //        p1 = new Point3D(j, i, height);
        //        for (j = 2; j < matrix.GetLength(1) - 2; j++)
        //        {
        //            if (matrix[i, j + 1] == 2)
        //            {
        //                if (!isAssigned)
        //                {

        //                    p2 = new Point3D(j, i, height);
        //                    if (matrix[i + 1, j + 1] < 2)
        //                        p3 = new Point3D(j + 1, i + 1, height);
        //                    else if (matrix[i + 1, j] < 2)
        //                        p3 = new Point3D(j, i + 1, height);
        //                    else if (matrix[i + 1, j - 1] < 2)
        //                        p3 = new Point3D(j - 1, i + 1, height);
        //                    isAssigned = true;
        //                    p4 = new Point3D(p1.X, i + 1, height);
        //                    List<Point3D> points = new List<Point3D>();
        //                    points.Add(p1);
        //                    points.Add(p2);
        //                    points.Add(p3);
        //                    points.Add(p4);
        //                    polynomIndices.Add(points);
        //                }
        //            }
        //            if (isAssigned && matrix[i, j + 1] == 1)
        //            {
        //                isAssigned = false;
        //                p1 = new Point3D(j + 1, i, height);
        //            }
        //        }
        //        isAssigned = false;

        //    }
        //    return polynomIndices;
        //}

        //public int[,] getMatrix(Model3DGroup model, List<List<Point>> points)
        //{
        //    var bounds = model.Bounds;
        //    int xlength = Convert.ToInt32(bounds.SizeX);
        //    int ylength = Convert.ToInt32(bounds.SizeY);
        //    int modelStartX = Convert.ToInt32(bounds.X);
        //    int modelStartY = Convert.ToInt32(bounds.Y);

        //    int[,] matrix = new int[ylength + 1, xlength + 1];
        //    int startX, startY, endX, endY, length;
        //    int add = 0;
        //    foreach (var pointList in points)
        //    {
        //        startX = Convert.ToInt32((pointList[0].X - modelStartX));
        //        startY = Convert.ToInt32((pointList[0].Y - modelStartY));
        //        endX = Convert.ToInt32((pointList[1].X - modelStartX));
        //        endY = Convert.ToInt32((pointList[1].Y - modelStartY));
        //        length = Convert.ToInt32(Math.Sqrt(Math.Pow((startX - endX), 2) + Math.Pow((startY - endY), 2)));
        //        matrix[startY, startX] = 1;
        //        matrix[endY, endX] = 1;
        //        Point[] pointsToFillBetween = new Point[length];
        //        if (length > 0)
        //        {
        //            pointsToFillBetween = getPoints(length * 3, startX, startY, endX, endY);
        //        }

        //        foreach (var pointToFillBetween in pointsToFillBetween)
        //        {
        //            matrix[Convert.ToInt32(pointToFillBetween.Y), Convert.ToInt32(pointToFillBetween.X)] = 1;
        //        }

        //    }
        //    matrix = fillMatrix(matrix, xlength, ylength);
        //    string filePath = "C:\\Users\\Lenovo\\Downloads\\matrix_output.txt";

        //    // Using StreamWriter to write the matrix to the file
        //    using (StreamWriter writer = new StreamWriter(filePath))
        //    {
        //        for (int i = 0; i < matrix.GetLength(0); i++)
        //        {
        //            for (int j = 0; j < matrix.GetLength(1); j++)
        //            {
        //                // Write each element and a tab for formatting
        //                writer.Write(matrix[i, j]);
        //            }
        //            // Write a new line after each row
        //            writer.WriteLine();
        //        }
        //    }


        //    return matrix;
        //}

        //public int[,] fillMatrix(int[,] matrix, int xlength, int ylength)
        //{
        //    bool isOnProcess = false;
        //    for (int i = 0; i < matrix.GetLength(0) - 1; i++)
        //    {
        //        for (int j = 0; j < matrix.GetLength(1) - 1; j++)
        //        {
        //            if (matrix[i, j] == 0)
        //            {
        //                //do nothing
        //            }

        //            else if (matrix[i, j] == 1 && !isBorder(xlength, ylength, i, j))
        //            {
        //                if (matrix[i, j + 1] != 1 && !isOnProcess && !isSingular(i, j, matrix))
        //                {
        //                    isOnProcess = true;
        //                    j++; //Sets the next point as filled here  due to 'for' design
        //                }
        //                else if (matrix[i, j + 1] != 1 && isOnProcess)
        //                {
        //                    isOnProcess = false;
        //                }
        //            }
        //            if (isOnProcess)
        //                matrix[i, j] = 2;

        //        }
        //        isOnProcess = false;

        //    }
        //    return matrix;
        //}
        //public bool isSingular(int i, int j, int[,] matrix)
        //{
        //    int pointsAround = 0;
        //    if (matrix[i - 1, j - 1] == 1 || matrix[i - 1, j] == 1 || matrix[i - 1, j + 1] == 1)
        //        pointsAround++;
        //    if (matrix[i + 1, j - 1] == 1 || matrix[i + 1, j] == 1 || matrix[i + 1, j + 1] == 1)
        //        pointsAround++;
        //    if (matrix[i, j - 1] == 1)
        //        pointsAround = 1;

        //    return pointsAround != 2;
        //}
        //public bool isBorder(int xLength, int yLength, int i, int j)
        //{
        //    if ((i == 0 || j == 0))
        //        return true;
        //    else if ((i == xLength || j == yLength))
        //        return true;
        //    return false;
        //}
        //public Point[] getPoints(int quantity, int startX, int startY, int endX, int endY)
        //{
        //    var points = new Point[quantity];
        //    int ydiff = endY - startY, xdiff = endX - startX;
        //    double slope = (double)(endY - startY) / (endX - startX);
        //    double x, y;

        //    --quantity;

        //    for (double i = 0; i < quantity; i++)
        //    {
        //        y = slope == 0 ? 0 : ydiff * (i / quantity);
        //        x = slope == 0 ? xdiff * (i / quantity) : y / slope;
        //        points[(int)i] = new Point((int)Math.Round(x) + startX, (int)Math.Round(y) + startY);
        //    }
        //    Point pEnd = new Point(endX, endY);
        //    points[quantity] = pEnd;
        //    return points;
        //}

        #endregion
    }
}
