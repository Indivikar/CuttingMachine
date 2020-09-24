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
    /// Interaktionslogik für Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private DataModel dataModel;

        public Home(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
        }

        private void BtnClickStreifen40(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Content = dataModel.Streifen40;
        }

        private void BtnClickStreifen70(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Content = dataModel.Streifen70;
        }
    }
}
