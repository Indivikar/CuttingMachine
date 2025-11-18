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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

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

            init();

        }

        private void init() {
            
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

        public void Btn_1mm_Click(object sender, RoutedEventArgs e)
        {
            StackPanelControls.IsEnabled = false;
            commandLine.setCommandLine(COMMAND_Schneidmaschine.stepperStart, 1, ToggleButton_Direction.IsChecked == true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void Btn_10mm_Click(object sender, RoutedEventArgs e)
        {
            StackPanelControls.IsEnabled = false;
            commandLine.setCommandLine(COMMAND_Schneidmaschine.stepperStart, 10, ToggleButton_Direction.IsChecked == true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void Btn_100mm_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleButton_Direction.IsChecked == true && !MessageBoxBack(100))
            {
                return;
            }

            StackPanelControls.IsEnabled = false;

            commandLine.setCommandLine(COMMAND_Schneidmaschine.stepperStart, 100, ToggleButton_Direction.IsChecked == true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());

        }

        public void Btn_soll_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleButton_Direction.IsChecked == true && !MessageBoxBack(dataModel.SelectedLength))
            {
                return;
            }

            StackPanelControls.IsEnabled = false;

            commandLine.setCommandLine(COMMAND_Schneidmaschine.stepperStart, dataModel.SelectedLength, ToggleButton_Direction.IsChecked == true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void StackPanelControlsEnable()
        {
            StackPanelControls.IsEnabled = true;
        }

        public void Btn_Cut(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND_Schneidmaschine.schneidenStart, 0, ToggleButton_Direction.IsChecked == true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void BtnClickKopfschnitt(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND_Schneidmaschine.kopfSchnittStart, 0, ToggleButton_Direction.IsChecked == true);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        public void ToggleBtn_Click_Handwheel(object sender, RoutedEventArgs e)
        {
            bool b = this.ToggleBtn_Handwheel.IsChecked == true;

            // Console.WriteLine("Handrad");

            if (b)
            {
                commandLine.setCommandLine(COMMAND_Schneidmaschine.handradOn, 0, false);
                dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
                // Console.WriteLine("An");
            }
            else
            {
                commandLine.setCommandLine(COMMAND_Schneidmaschine.handradOff, 0, false);
                dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
                // Console.WriteLine("Aus");
            }
        }

        public void setIstWertInMM(long istWertInSteps) 
        {
            istWertInSteps = dataModel.stepsToMM(istWertInSteps);

            //this.streifenIstWert.Text = "";
            //this.streifenIstWert.Text = Convert.ToString(istWertInSteps);

            //dataModel.IstWertInMM = istWertInSteps;
        }

        public void setIstWertInMM(string istWertInSteps)
        {
            setIstWertInMM(Int64.Parse(istWertInSteps));

        }

        public void Btn_Stop(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND_Schneidmaschine.allesStop, 0, false);
            dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());
        }

        private void EnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.BtnStop.IsEnabled = !this.StackPanelControls.IsEnabled;
        }

        private bool MessageBoxBack(int wert)
        {
            DialogResult Result = MessageBox.Show("Willst du wirklich " + wert + "mm zurück Fahren?", "ACHTUNG!" , MessageBoxButtons.YesNo);
            if (Result == DialogResult.Yes)
            {
                return true;
            }
            else 
            {
                return false;
            }
          
        }

    }
}

