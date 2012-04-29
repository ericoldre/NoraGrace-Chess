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

            Sinobyl.WPF.Models.BoardModel model = (Sinobyl.WPF.Models.BoardModel)viewmodel.Model;

            var x = model.Pieces.First(p => p.Position == Engine.ChessPosition.A2) as Sinobyl.WPF.Models.PieceModel;
            x.Position = Engine.ChessPosition.A4;
            
            //while(viewmodel.Model.Moves.Count()>0)
            //{
            //    viewmodel.Model.Moves.RemoveAt(0);
            //}
            

        }
    }
}
