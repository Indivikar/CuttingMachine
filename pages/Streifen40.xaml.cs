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
    /// Interaktionslogik für Streifen40.xaml
    /// </summary>
    public partial class Streifen40 : Page
    {
        private DataModel dataModel;

        public Streifen40(DataModel dataModel)
        {     
            InitializeComponent();
            this.dataModel = dataModel;
        }

        private void BtnHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Content = dataModel.Home;
        }
    }
}
