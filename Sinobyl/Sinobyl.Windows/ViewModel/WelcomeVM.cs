using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sinobyl.Windows.ViewModel
{
    public class WelcomeVM : ViewModelBase
    {

        public WelcomeVM(Common.IMessenger messenger)
            : base(messenger)
        {
            GalaSoft.MvvmLight.Messaging.MessageBase
        }


        
        public ICommand PlayAsWhiteCommand
        {
            get
            {
                return new GalaSoft.MvvmLight.Command.RelayCommand(PlayAsWhite);
            }
        }
        public void PlayAsWhite()
        {

        }

        public ICommand PlayAsBlackCommand
        {
            get
            {
                return new GalaSoft.MvvmLight.Command.RelayCommand(PlayAsBlack);
            }
        }
        public void PlayAsBlack()
        {

        }

        public ICommand ComputerVsComputerCommand
        {
            get
            {
                return new GalaSoft.MvvmLight.Command.RelayCommand(ComputerVsComputer);
            }
        }
        public void ComputerVsComputer()
        {

        }


    }
}
