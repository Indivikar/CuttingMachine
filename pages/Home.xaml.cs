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
            this.BtnC4Kurz.Content = "C4/C5 - " + (int)STREIFEN.C4_40_Schachtel_KURZ + "er\n   Schachtel";
            this.BtnC4Lang.Content = "C4/C5 - " + (int)STREIFEN.C4_40_Schachtel_LANG + "er\n   Schachtel";
            this.BtnC5Kurz.Content = "C5 - " + (int)STREIFEN.C5_40_Deckel + "er\n   Deckel";
            //this.BtnC5Lang.Content = "C5 " + (int)STREIFEN.C5_LANG + "er";

            this.Btn_C4_70_Deckel.Content = "C4 - " + (int)STREIFEN.C4_70_Deckel + "er\n   Deckel";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnC4Kurz_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
            dataModel.setSelectedLength(STREIFEN.C4_40_Schachtel_KURZ);
        }

        private void BtnC4Lang_Click(object sender, RoutedEventArgs e) // für C4 und C5, haben die gleiche Länge
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
            dataModel.setSelectedLength(STREIFEN.C4_40_Schachtel_LANG);
        }

        private void BtnC5Kurz_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
            dataModel.setSelectedLength(STREIFEN.C5_40_Deckel);
        }

        private void BtnC5Lang_Click(object sender, RoutedEventArgs e)
        {
            //dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
            //dataModel.setSelectedLength(STREIFEN.C4_40_Schachtel_LANG);
        }

        private void BtnEigeneLaenge_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
            dataModel.setSelectedLength(this.TextBoxEigeneLaenge.Text);
        }

        private void BtnC4_70_Deckel_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
            dataModel.setSelectedLength(STREIFEN.C4_70_Deckel);
        }
    }
}
