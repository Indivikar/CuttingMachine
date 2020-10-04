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
    /// Interaktionslogik für EinzelSchritt.xaml
    /// </summary>
    public partial class EinzelSchritt : Page
    {
        private DataModel dataModel;
        private CommandLine commandLine;

        public EinzelSchritt(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.commandLine = dataModel.CommandLine;

        }

        private void BtnClickHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Home;
        }

        private void BtnClickSchnittModus(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void Btn_1mm_Click(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND.stepperStart, 1, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_10mm_Click(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND.stepperStart, 10, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_100mm_Click(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND.stepperStart, 100, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_soll_Click(object sender, RoutedEventArgs e)
        {

            bool b = this.ToggleButton_Direction.IsChecked == true;

            Console.WriteLine("Direction: " + b);

            commandLine.setCommandLine(COMMAND.stepperStart, 320, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_Cut(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleBtn_Click_Handwheel(object sender, RoutedEventArgs e)
        {

        }
    }
}
