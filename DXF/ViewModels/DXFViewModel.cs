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

namespace DXF.ViewModels
{
    public class DXFViewModel : ViewModelBase
    {
        public ICommand ApplyCommand { get; }
        public DXFViewModel(NavigationStore navigation, double height)
        {
            Model = readDFX("C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\SamplesV2\\Separatör_Pneumatrol_rev0.dxf", height);
            ApplyCommand = new ApplyCommand(this, navigation);
            //CombinedGeometry = createPoligon(5);
            //CombinedGeometry.
            //Model = GetSolid();
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

        public Model3DGroup GetSolid()
        {
            //double BodyWidth = 40;
            //double BodyHeight = 40;
            //double BodyDepth = 40;
            //double BodyDescent = 20;
            //double TotalWidth = 55.5;
            //double BracketHeight = 3;
            //double TotalHeight = 44;
            //double HoleSpacing = 10.0;
            //double HoleDiameter = 4;
            //double HoleInset = 3;
            //Solid body =
            //        Cube(size: new Vector3D(BodyWidth, BodyHeight, BodyDepth)).
            //        Translate(10, 20, 0);
            ////Solid bracket =
            ////    Cube(TotalWidth + 70, BracketHeight, BodyDepth).
            ////    Translate(-30, 20, 0);
            //Solid hole1 =
            //    Cylinder(HoleDiameter / 2, 40 * 4, center: true).
            //    Translate(15, 20, 10);
            //Solid hole4 =
            //    Cylinder(HoleDiameter / 2, 40 * 4, center: true).
            //    Translate(TotalWidth / 2 - HoleInset, BodyDescent, HoleSpacing / 2);
            Solid plane = Cube(size: new Vector3D(50, 50, 5), new Vector3D(0,0,0));
            Solid hole1 = Cube(size: new Vector3D(2, 5, 5), new Vector3D(0, 22.5, 0));
            Solid asd = Difference(plane, hole1).Translate(y: -70 / 2);
            var modelGroup = new Model3DGroup();
            var meshBuilder = new MeshBuilder();

            double squareSize = 100; // Size of the square
            double holeRadius = 20;  // Radius of the circular hole
            double centerX = squareSize / 2; // Center X of the circle in the square
            double centerY = squareSize / 2; // Center Y of the circle in the square

            var squareGeometry = new RectangleGeometry(new Rect(0, 0, squareSize, squareSize));
            var holeGeometry = new EllipseGeometry(new Rect(centerX - holeRadius, centerY - holeRadius, holeRadius * 2, holeRadius * 2));

            var combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, squareGeometry, holeGeometry);

            CombinedGeometry = combinedGeometry;
            var sss = combinedGeometry.GetOutlinedPathGeometry();

            foreach (var polygon in asd.Polygons)
            {
                var points = new Point3DCollection();
                foreach (var vertex in polygon.Vertices)
                {

                    points.Add(new Point3D(vertex.Pos.X, vertex.Pos.Y, vertex.Pos.Z));
                }
                if (points.Count > 4)
                {
                    points.RemoveAt(4);
                }

                meshBuilder.AddPolygon(points);

                var mesh = meshBuilder.ToMesh();
                var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
                var model = new GeometryModel3D(mesh, material);
                modelGroup.Children.Add(model);
            }
            return modelGroup;
        }

        private Model3DGroup readDFX(string filePath, double height)
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
            //modelGroup.Children.Add(createPolygon(height));
            //DXFMapMatrix asd = new DXFMapMatrix(modelGroup);
            return modelGroup;
        }

        public GeometryModel3D createLine(double startPointX, double startPointY, double endPointX, double endPointY, double height)
        {
            var meshBuilder = new MeshBuilder();
            //Center Point of the box
            Point3D center = new Point3D((startPointX + endPointX) / 2, (startPointY + endPointY) / 2, 0 + height / 2);
            double width = 0.01;
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
            model.Transform = new MatrixTransform3D(transformationMatrix); //Applies the transformation to your model
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
            model.Transform = new MatrixTransform3D(transformationMatrix); //Applies the transformation to your model

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
                modelGroup.Children.Add(createLine(start.X, start.Y, end.X, end.Y, height));
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

            //points.Add(new System.Windows.Point(p1.X,p1.Y));
            //points.Add(new System.Windows.Point(p2.X,p2.Y));
            //points.Add(new System.Windows.Point(p3.X,p3.Y));
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
                modelGroup.Children.Add(createLine(pStart.X, pStart.Y, pEnd.X, pEnd.Y, height));
                pStart = pEnd;
            }
            var sdas = modelGroup.Children;
            return modelGroup;
        }

        public GeometryModel3D createPolygon(double height)
        {
            var meshBuilder = new MeshBuilder();
            var polygonPoints = new List<Point3D>
            {
                new Point3D(-10, -10,2),
                new Point3D(780, -10,2),
                new Point3D(780, 580,2),
                new Point3D(-10, 580,2),
            };
            meshBuilder.AddPolygon(polygonPoints);
            var mesh = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            var model = new GeometryModel3D(mesh, material);
            return model;
        }
    }


}
