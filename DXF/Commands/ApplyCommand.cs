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

            if (true)
            {
                _navigationStore.CurrentViewModel = new DXFViewModel(_navigationStore, Convert.ToDouble(_DXFviewModel.Height));
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
