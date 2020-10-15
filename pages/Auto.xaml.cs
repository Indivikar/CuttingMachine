using SchneidMaschine.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaktionslogik für Auto.xaml
    /// </summary>
    public partial class Auto : Page
    {
        private DataModel dataModel;
        private CommandLine commandLine;

        private Task taskAutoRun;

        private bool isPause = false;
        private bool isStop = false;

        public Auto(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.commandLine = dataModel.CommandLine;

            this.BtnModusAutoPause.IsEnabled = false;
            this.BtnModusAutoStop.IsEnabled = false;   
        }

        async Task<bool> GetLeisureHoursAsync(int runs)
        {
            await Task.Run(() => aufgabe(runs));
            return false;
        }

        private void aufgabe(int runs) {
            for (int i = 0; i < runs; i++)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);

                int wert = i + 1;

                dataModel.Auto.Dispatcher.Invoke(() => {
                    this.TextBoxRunsIst.Text = Convert.ToString(i + 1);
                });


                while (isPause) Thread.Sleep(200);

                if (isStop) 
                {
                    break;
                }
            }

            dataModel.Auto.Dispatcher.Invoke(() => {
                this.TextBoxRuns.IsEnabled = true;
                this.BtnModusAutoStart.IsEnabled = true;
                this.BtnModusAutoPause.IsEnabled = false;
                this.BtnModusAutoStop.IsEnabled = false;
            });
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

        private void BtnClickModusAutoStart(object sender, RoutedEventArgs e)
        {

            isPause = false;
            isStop = false;

            int runs = Int32.Parse(this.TextBoxRuns.Text);

            this.taskAutoRun = GetLeisureHoursAsync(runs);

            this.TextBoxRunsIst.Text = "0";

            this.TextBoxRuns.IsEnabled = false;
            this.BtnModusAutoStart.IsEnabled = false;
            this.BtnModusAutoPause.IsEnabled = true;
            this.BtnModusAutoStop.IsEnabled = true;
            
        }

        private void BtnClickModusAutoPause(object sender, RoutedEventArgs e)
        {
            if (isPause)
            {
                isPause = false;
                this.BtnModusAutoPause.Content = "Pause";
            }
            else 
            {
                isPause = true;
                this.BtnModusAutoPause.Content = "Weiter";
            }
            
        }

        private void BtnClickModusAutoStop(object sender, RoutedEventArgs e)
        {
            isStop = true;
            commandLine.setCommandLine(COMMAND.allesStop, 0, false);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void TextBoxRuns_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.TextBoxRunsIst.Text = "0";
            this.TextBoxRunsSoll.Text = this.TextBoxRuns.Text;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
