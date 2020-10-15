using SchneidMaschine.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaktionslogik für HalbAuto.xaml
    /// </summary>
    public partial class HalbAuto : Page
    {
        private DataModel dataModel;
        private CommandLine commandLine;

        private bool allesStoppen = false;

        public HalbAuto(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.commandLine = dataModel.CommandLine;

            this.BtnModusHalbAutoStop.IsEnabled = false;
        }

        private void BtnClickHome(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Home;
        }

        private void BtnClickSchnittModus(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.SchnittModus;
        }

        private void Btn_Click_Reset_IstWert(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND.resetIstWert, 0, true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void BtnClickModusHalbAutoStart(object sender, RoutedEventArgs e)
        {
            allesStoppen = false;
            this.BtnModusHalbAutoStart.IsEnabled = false;
            this.BtnModusHalbAutoStop.IsEnabled = true;

            commandLine.setCommandLine(COMMAND.stepperStart, dataModel.SelectedLength, false);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void BtnClickModusHalbAutoStop(object sender, RoutedEventArgs e)
        {
            allesStoppen = true;
            commandLine.setCommandLine(COMMAND.allesStop, 0, false);
            dataModel.sendText(commandLine.getCommandLine());
        }


        public void cut() {
            if (!allesStoppen) 
            {
                commandLine.setCommandLine(COMMAND.schneidenStart, 0, true);
                dataModel.sendText(commandLine.getCommandLine());
            }

            this.BtnModusHalbAutoStart.IsEnabled = true;
            this.BtnModusHalbAutoStop.IsEnabled = false;
        }

    }
}
