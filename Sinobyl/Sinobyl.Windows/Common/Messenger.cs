using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Windows.Common
{

    public interface IMessenger: GalaSoft.MvvmLight.Messaging.IMessenger
    {

    }

    public class Messenger: GalaSoft.MvvmLight.Messaging.Messenger, IMessenger
    {

    }
}
