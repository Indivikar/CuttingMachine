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

            init();
        }

        private void init()
        {
            this.BtnModusAutoStart.IsEnabled = false;
            this.BtnModusAutoPause.IsEnabled = false;
            this.BtnModusAutoStop.IsEnabled = false;

        }

        public void SetMaxDurchlauf() 
        {
            int selectedLength = dataModel.SelectedLength;
            long rollenLaengeAktuell = dataModel.Statistik.RolleIstLaenge;

            if (rollenLaengeAktuell == 0 || selectedLength == 0) { return; }

            long erg = rollenLaengeAktuell / Convert.ToInt64(selectedLength);

            TextBlockMaxRuns.Text = erg + "";

            SetMaxStreifen(erg);
        }

        public void SetMaxStreifen(long wert)
        {
            long erg = wert * dataModel.StreifenProSchnitt40er;

            TextBlockMaxStreifen.Text = erg + "";

            SetMaxSchachteln(erg);
        }

        public void SetMaxSchachteln(long wert)
        {
            long erg = wert / dataModel.StreifenProSchachtel;

            TextBlockMaxSchachteln.Text = erg + "";
        }

        public void output() 
        {
            string textRuns = this.TextBoxRuns.Text;

            this.TextBoxRunsIst.Text = "0";

            if (textRuns == null || textRuns.Equals(""))
            {
                this.TextBoxRunsSoll.Text = "0";
                this.TextBlockStreifen.Text = "0";
                this.TextBlockSchachteln.Text = "0";
                this.TextBlockSchachteln.Text = "0";
            }
            else
            {
                this.TextBoxRunsSoll.Text = textRuns;

                long runs = Int64.Parse(textRuns);
                TextBlockStreifen.Text = runs * 24 + "";

                long erg = runs * dataModel.StreifenProSchnitt40er / dataModel.StreifenProSchachtel;
                this.TextBlockSchachteln.Text = erg + "";
            }
        }

        async Task<bool> TaskAutoModus(int runs)
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

                while (!dataModel.IsCutFinished) Thread.Sleep(1000);

                commandLine. setCommandLine(COMMAND.stepperStart, dataModel.SelectedLength, false);
                dataModel.sendText(commandLine.getCommandLine());

                while (!dataModel.IsStepperFinished) Thread.Sleep(1000);

                commandLine.setCommandLine(COMMAND.schneidenStart, 0, false);
                dataModel.sendText(commandLine.getCommandLine());

                // for-Schleife pausieren, wenn Pause gedrückt wurde
                while (isPause) Thread.Sleep(200);

                // for-Schleife beenden, wenn Stop gedrückt wurde
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

            this.taskAutoRun = TaskAutoModus(runs);

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
            output();

            if (TextBoxRuns.Text == null || TextBoxRuns.Text.Equals("")) 
            {
                BtnModusAutoStart.IsEnabled = false;
                return;
            }

            int sollDurchlaeufe = int.Parse(TextBoxRuns.Text);
            int maxDurchlaeufe = int.Parse(TextBlockMaxRuns.Text);

            if (sollDurchlaeufe > maxDurchlaeufe)
            {
                TextBoxRuns.Foreground = Brushes.Red;
                BtnModusAutoStart.IsEnabled = false;
            }
            else
            {
                TextBoxRuns.Foreground = Brushes.Black;
                BtnModusAutoStart.IsEnabled = true;
            }

            
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
