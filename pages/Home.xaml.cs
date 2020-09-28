using SchneidMaschine.model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
    /// Interaktionslogik für Home.xaml
    /// </summary>
    /// 

    enum STREIFEN_40er
    {
        C4_KURZ = 320,
        C4_LANG = 710,
        C5_KURZ = 200,
        C5_LANG = 250
    }

    public partial class Home : Page
    {

        private DataModel dataModel;
        private SerialPort serialPort1;

        delegate void SetTextCallback(string text);

        public Home(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.serialPort1 = dataModel.SerialPort1;

            // 70er Streifen gibt es noch nicht
            Btn_C4_70_Deckel.IsEnabled = false;

            setButtonText();

        }


        private void setButtonText()
        {
            this.BtnC4Kurz.Content = "C4 " + (int)STREIFEN_40er.C4_KURZ + "er";
            this.BtnC4Lang.Content = "C4 " + (int)STREIFEN_40er.C4_LANG + "er";
            this.BtnC5Kurz.Content = "C5 " + (int)STREIFEN_40er.C5_KURZ + "er";
            this.BtnC5Lang.Content = "C5 " + (int)STREIFEN_40er.C5_LANG + "er";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnC4Kurz_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void BtnC4Lang_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void BtnC5Kurz_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void BtnC5Lang_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void BtnEigeneLaenge_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void BtnC4_70_Deckel_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }
    }
}
