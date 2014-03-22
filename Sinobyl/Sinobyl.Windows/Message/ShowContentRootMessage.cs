using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Windows.Message
{
    public class ShowContentRootMessage: GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public readonly ViewModel.ViewModelBase _viewModelToShow;
        public ShowContentRootMessage(ViewModel.ViewModelBase viewModelToShow)
        {
            _viewModelToShow = viewModelToShow;
        }

        public ViewModel.ViewModelBase ViewModelToShow
        {
            get { return _viewModelToShow; }
        }
    }
}
