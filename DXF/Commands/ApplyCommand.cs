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
        public ApplyCommand(DXFViewModel DXFviewModel) 
        { 
            _DXFviewModel = DXFviewModel;

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

                _DXFviewModel.Model = _DXFviewModel.showModel(_DXFviewModel.FilePath, Convert.ToDouble(_DXFviewModel.Height));                
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
