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
        public SelectFileCommand(DXFViewModel DXFviewModel)
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
                double height = 0;
                try
                {
                    height = Convert.ToDouble(_DXFviewModel.Height);
                }
                catch
                {
                    giveError();
                    return;
                }
                _DXFviewModel.FilePath = fileDialog.FileName;
                _DXFviewModel.Model = _DXFviewModel.showModel(_DXFviewModel.FilePath, Convert.ToDouble(_DXFviewModel.Height));
                //_DXFviewModel.Model = _DXFviewModel.readDFX(_DXFviewModel.FilePath, Convert.ToDouble(_DXFviewModel.Height));

                if (_DXFviewModel.Model == null) return;

                Rect3D bounds = _DXFviewModel.Model.Bounds;
                Point3D center = new Point3D(
                    bounds.X + bounds.SizeX / 2,
                    bounds.Y + bounds.SizeY / 2,
                    bounds.Z + bounds.SizeZ / 2);

                double distance = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ)) * 5;

                _DXFviewModel.CameraPosition = new Point3D(center.X, center.Y, center.Z + distance);
                _DXFviewModel.CameraLookDirection = new Vector3D(center.X - _DXFviewModel.CameraPosition.X, center.Y - _DXFviewModel.CameraPosition.Y, center.Z - _DXFviewModel.CameraPosition.Z);

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
