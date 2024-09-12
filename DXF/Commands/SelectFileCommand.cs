using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using DXF.Stores;
using DXF.ViewModels;
using Microsoft.Win32;

namespace DXF.Commands
{
    class SelectFileCommand : CommandBase
    {
        DXFViewModel _DXFviewModel;
        public SelectFileCommand(DXFViewModel DXFviewModel, NavigationStore navigationStore)
        {
            _DXFviewModel = DXFviewModel;

        }

        public override void Execute(object parameter)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "DXF Files | *.DXF";
            fileDialog.InitialDirectory = "C:\\Users\\serhat.ozdemir\\source\\repos\\DXF\\DXF\\SamplesV2";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                _DXFviewModel.FilePath = fileDialog.FileName;
                _DXFviewModel.Model = _DXFviewModel.GetSolid(_DXFviewModel.FilePath, Convert.ToDouble(_DXFviewModel.Height));

                if (_DXFviewModel.Model == null) return;

                Rect3D bounds = _DXFviewModel.Model.Bounds;
                Point3D center = new Point3D(
                    bounds.X + bounds.SizeX / 2,
                    bounds.Y + bounds.SizeY / 2,
                    bounds.Z + bounds.SizeZ / 2);

                double distance = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ)) * 5;

                _DXFviewModel.CameraPosition = new Point3D(center.X, center.Y, center.Z + distance);
                _DXFviewModel.CameraLookDirection = new System.Windows.Media.Media3D.Vector3D(center.X - _DXFviewModel.CameraPosition.X, center.Y - _DXFviewModel.CameraPosition.Y, center.Z - _DXFviewModel.CameraPosition.Z);
                //    new PerspectiveCamera
                //{
                //    Position = ,
                //    LookDirection = new System.Windows.Media.Media3D.Vector3D(center.X - _DXFviewModel.Camera.Position.X, center.Y - _DXFviewModel.Camera.Position.Y, center.Z - _DXFviewModel.Camera.Position.Z),
                //    UpDirection = new Vector3D(0, 1, 0),
                //    FieldOfView = 45
                //};
            }
            else
                giveError();
        }

        public void giveError()
        {
            MessageBoxResult result = MessageBox.Show("Choose Something To Delete",
                                          "Error",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
