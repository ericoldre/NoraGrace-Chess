using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sinobyl.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
           

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Sinobyl.WPF.ViewModels.BoardVM viewmodel = (Sinobyl.WPF.ViewModels.BoardVM)this.DataContext;
            
            var m = viewmodel.Model;

            m.ApplyMove(new Engine.ChessMove(Engine.ChessPosition.D2, Engine.ChessPosition.D4));
            m.ApplyMove(new Engine.ChessMove(Engine.ChessPosition.E7, Engine.ChessPosition.E5));
            m.ApplyMove(new Engine.ChessMove(Engine.ChessPosition.D4, Engine.ChessPosition.E5));


            //if (m.Moves.Count > 0)
            //{
            //    m.ApplyMove(m.Moves[0]);
            //}
            

        }
    }
}
