using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DXF.Models
{
    public class DXFMapMatrix
    {
        public DXFMapMatrix(Model3DGroup modelGroup)
        {
            foreach (Model3D model in modelGroup.Children)
            {
                if (model is GeometryModel3D geometryModel)
                {
                    // Access properties of the GeometryModel3D
                    var mesh = geometryModel.Geometry as MeshGeometry3D;
                    var sss = mesh.Bounds;

                }

            }
        }

    }
}
