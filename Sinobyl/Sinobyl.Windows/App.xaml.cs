using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sinobyl.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ViewModel.RootVM _rootVM;
        MainWindow _window;
        Common.Messenger _messenger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _messenger = new Common.Messenger();
            _rootVM = new ViewModel.RootVM(_messenger);
            _window = new Sinobyl.Windows.MainWindow();
            _window.DataContext = _rootVM;

            _window.Show();
        }
    }
}
