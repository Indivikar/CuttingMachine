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
    /// Interaktionslogik für Wartung.xaml
    /// </summary>
    public partial class Wartung : Page
    {

        private DataModel dataModel;
        private CommandLine commandLine;

        public Wartung(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.commandLine = dataModel.CommandLine;
        }

        private void Btn_Click_SchrittMotor_Start(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND_Schneidmaschine.wartungSchrittMotor, 0, false);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        private void Btn_Click_SchrittMotor_Stop(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND_Schneidmaschine.allesStop, 0, false);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        private void BtnClickHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Home;
        }

    }
}
