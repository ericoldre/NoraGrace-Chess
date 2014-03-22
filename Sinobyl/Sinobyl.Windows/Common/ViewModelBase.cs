using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Windows.Common
{
    public abstract class ViewModelBase: GalaSoft.MvvmLight.ViewModelBase
    {
        protected readonly Common.IMessenger _messenger;

        public ViewModelBase(Common.IMessenger messenger)
        {
            _messenger = messenger;
        }

        public Common.IMessenger Messenger
        {
            get { return _messenger; }
        }
    }
}
