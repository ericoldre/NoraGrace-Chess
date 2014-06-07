using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Windows.ViewModel
{
    public class RootVM: ViewModelBase
    {
        protected WelcomeVM _welcome;

        public RootVM(Common.IMessenger messenger)
            : base(messenger)
        {
            _welcome = new WelcomeVM(messenger);
            this.ActiveContent = this.Welcome;
        }

        private ViewModelBase _activeContent;
        public ViewModelBase ActiveContent
        {
            get { return _activeContent; }
            protected set { Set(() => ActiveContent, ref _activeContent, value); }
        }

        public WelcomeVM Welcome
        {
            get { return _welcome; }
        }

    }
}
