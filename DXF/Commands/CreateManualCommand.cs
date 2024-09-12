using DXF.Stores;
using DXF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DXF.Commands
{
    public  class CreateManualCommand : CommandBase
    {
        DXFViewModel _DXFviewModel;
        NavigationStore _navigationStore;
        public CreateManualCommand(DXFViewModel DXFviewModel, NavigationStore navigationStore)
        {
            _DXFviewModel = DXFviewModel;
            _navigationStore = navigationStore;

        }

        public override void Execute(object parameter)
        {

            if (true)
            {

                _DXFviewModel.Model = _DXFviewModel.GetSolid(Convert.ToDouble(_DXFviewModel.Height));
                if (_DXFviewModel.Model == null) return;

                Rect3D bounds = _DXFviewModel.Model.Bounds;
                Point3D center = new Point3D(
                    bounds.X + bounds.SizeX / 2,
                    bounds.Y + bounds.SizeY / 2,
                    bounds.Z + bounds.SizeZ / 2);

                double distance = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ)) * 5;

                _DXFviewModel.CameraPosition = new Point3D(center.X, center.Y, center.Z + distance);
                _DXFviewModel.CameraLookDirection = new System.Windows.Media.Media3D.Vector3D(center.X - _DXFviewModel.CameraPosition.X, center.Y - _DXFviewModel.CameraPosition.Y, center.Z - _DXFviewModel.CameraPosition.Z);

                //_navigationStore.CurrentViewModel = new DXFViewModel(_navigationStore, Convert.ToDouble(_DXFviewModel.Height), _DXFviewModel.filePath);
            }
            else
                giveError();
        }

        public void giveError()
        {
            MessageBoxResult result = MessageBox.Show("Select A File First",
                                          "Error",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
