using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Sinobyl.WPF.Models;
using Sinobyl.WPF.ViewModels;
using Sinobyl.Engine;

namespace Sinobyl.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow window = new MainWindow();


            var viewModel = new BoardVM(new BoardModel(new ChessBoard(new ChessFEN(ChessFEN.FENStart))));


            // Allow all controls in the window to 
            // bind to the ViewModel by setting the 
            // DataContext, which propagates down 
            // the element tree.
            window.DataContext = viewModel;

            window.Show();
        }
    }
}
