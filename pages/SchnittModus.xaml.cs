using SchneidMaschine.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchneidMaschine.pages
{
    /// <summary>
    /// Interaktionslogik für SchnittModus.xaml
    /// </summary>
    public partial class SchnittModus : Page
    {
        private DataModel dataModel;


        public SchnittModus(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
        }

        private void BtnClickHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Home;
        }

        private void BtnClickModusEinzelSchritt(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.EinzelSchritt;
        }

        private void BtnClickModusHalbAuto(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.HalbAuto;
        }

        private void BtnClickModusAuto(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Auto;
        }


    }
}
