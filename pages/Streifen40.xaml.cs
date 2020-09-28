using SchneidMaschine.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// 



    public partial class Streifen40 : Page
    {
        private DataModel dataModel;

        public Streifen40(DataModel dataModel)
        {     
            InitializeComponent();
            this.dataModel = dataModel;

            setButtonText();
        }

        private void setButtonText()
        {
            this.BtnC4Kurz.Content = "C4 " + (int) STREIFEN_40er.C4_KURZ + "er";
            this.BtnC4Lang.Content = "C4 " + (int) STREIFEN_40er.C4_LANG + "er";
            this.BtnC5Kurz.Content = "C5 " + (int) STREIFEN_40er.C5_KURZ + "er";
            this.BtnC5Lang.Content = "C5 " + (int) STREIFEN_40er.C5_LANG + "er";
        }

        private void BtnHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Content = dataModel.Home;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnC4Kurz_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnC4Lang_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnC5Kurz_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnC5Lang_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnEigeneLaenge_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
