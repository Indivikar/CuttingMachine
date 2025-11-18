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

        private Task taskHalbAutoRun;

        private bool allesStoppen = false;

        public HalbAuto(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.commandLine = dataModel.CommandLine;

            init();
        }

        private void init()
        {
            this.BtnModusHalbAutoStop.IsEnabled = false;
        }

        async Task<bool> TaskHalbAutoModus(bool isRadioButtonChecked)
        {           
            await Task.Run(() => aufgabe(isRadioButtonChecked));
            return false;
        }

        private void aufgabe(bool isRadioButtonChecked)
        {
            Thread.Sleep(1000);

            //dataModel.Auto.Dispatcher.Invoke(() => {
            //    this.TextBoxRunsIst.Text = Convert.ToString(i + 1);
            //});

            // Reinfolge -> soll zuerst geschnitten oder auf Länge gefahren werden
            if (isRadioButtonChecked)
            {
                laengeAnfahren();
                abschneiden();
            }
            else
            {
                abschneiden();
                laengeAnfahren();
            }

            dataModel.Auto.Dispatcher.Invoke(() =>
            {
                this.BtnModusHalbAutoStart.IsEnabled = true;
                this.BtnModusHalbAutoStop.IsEnabled = false;
            });
        }

        private void laengeAnfahren() 
        {
            while (!dataModel.IsCutFinished) Thread.Sleep(1000);

            if (!allesStoppen)
            {
                commandLine.setCommandLine(COMMAND_Schneidmaschine.stepperStart, dataModel.SelectedLength, false);
                dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
            }
        }

        private void abschneiden()
        {
            while (!dataModel.IsStepperFinished) Thread.Sleep(1000);

            if (!allesStoppen)
            {
                commandLine.setCommandLine(COMMAND_Schneidmaschine.schneidenStart, 0, false);
                dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
            }
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
            commandLine.setCommandLine(COMMAND_Schneidmaschine.resetIstWert, 0, true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void BtnClickModusHalbAutoStart(object sender, RoutedEventArgs e)
        {
            allesStoppen = false;
            this.BtnModusHalbAutoStart.IsEnabled = false;
            this.BtnModusHalbAutoStop.IsEnabled = true;

            this.taskHalbAutoRun = TaskHalbAutoModus(RadioButtonLaenge.IsChecked == true);

            //commandLine.setCommandLine(COMMAND_Schneidmaschine.stepperStart, dataModel.SelectedLength, false);
            //dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void BtnClickModusHalbAutoStop(object sender, RoutedEventArgs e)
        {
            allesStoppen = true;
            commandLine.setCommandLine(COMMAND_Schneidmaschine.allesStop, 0, false);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }


        public void cut() {
            if (!allesStoppen) 
            {
                commandLine.setCommandLine(COMMAND_Schneidmaschine.schneidenStart, 0, true);
                dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
            }


        }

    }
}
