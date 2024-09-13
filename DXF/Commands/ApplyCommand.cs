using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using DXF.ViewModels;
using DXF.Stores;

namespace DXF.Commands
{
    public  class ApplyCommand : CommandBase
    {
        DXFViewModel _DXFviewModel;
        NavigationStore _navigationStore;
        public ApplyCommand(DXFViewModel DXFviewModel, NavigationStore navigationStore) 
        { 
            _DXFviewModel = DXFviewModel;
            _navigationStore = navigationStore;

        }

        public override void Execute(object parameter)
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

            if (_DXFviewModel.FilePath != null)
            {

                _DXFviewModel.Model = _DXFviewModel.getModel(_DXFviewModel.FilePath, Convert.ToDouble(_DXFviewModel.Height));
                //_navigationStore.CurrentViewModel = new DXFViewModel(_navigationStore, Convert.ToDouble(_DXFviewModel.Height), _DXFviewModel.filePath);
            }
            else if (_DXFviewModel.Model != null)
            {
                _DXFviewModel.Model = _DXFviewModel.GetSolid(Convert.ToDouble(_DXFviewModel.Height));
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
