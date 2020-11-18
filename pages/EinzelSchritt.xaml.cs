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

            init();

        }

        private void init() {
            this.RestLaengeRolle.Text = dataModel.RollenLaengeAktuell;
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

        private void Btn_1mm_Click(object sender, RoutedEventArgs e)
        {
            StackPanelControls.IsEnabled = false;
            commandLine.setCommandLine(COMMAND.stepperStart, 1, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_10mm_Click(object sender, RoutedEventArgs e)
        {
            StackPanelControls.IsEnabled = false;
            commandLine.setCommandLine(COMMAND.stepperStart, 10, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_100mm_Click(object sender, RoutedEventArgs e)
        {
            StackPanelControls.IsEnabled = false;
            commandLine.setCommandLine(COMMAND.stepperStart, 100, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void Btn_soll_Click(object sender, RoutedEventArgs e)
        {
            StackPanelControls.IsEnabled = false;

            commandLine.setCommandLine(COMMAND.stepperStart, dataModel.SelectedLength, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        public void StackPanelControlsEnable()
        {
            StackPanelControls.IsEnabled = true;
        }

        private void Btn_Cut(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND.schneidenStart, 0, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());           
        }

        private void BtnClickKopfschnitt(object sender, RoutedEventArgs e)
        {
            commandLine.setCommandLine(COMMAND.schneidenStart, 0, ToggleButton_Direction.IsChecked == true);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void ToggleBtn_Click_Handwheel(object sender, RoutedEventArgs e)
        {
            bool b = this.ToggleBtn_Handwheel.IsChecked == true;

            // Console.WriteLine("Handrad");

            if (b)
            {
                commandLine.setCommandLine(COMMAND.handradOn, 0, false);
                dataModel.sendText(commandLine.getCommandLine());
                // Console.WriteLine("An");
            }
            else
            {
                commandLine.setCommandLine(COMMAND.handradOff, 0, false);
                dataModel.sendText(commandLine.getCommandLine());
                // Console.WriteLine("Aus");
            }
        }

        public void setIstWertInMM(long istWertInSteps) 
        {
            istWertInSteps = dataModel.stepsToMM(istWertInSteps);

            //this.streifenIstWert.Text = "";
            //this.streifenIstWert.Text = Convert.ToString(istWertInSteps);

            dataModel.IstWertInMM = istWertInSteps;
        }

        public void setIstWertInMM(string istWertInSteps)
        {
            setIstWertInMM(Int64.Parse(istWertInSteps));

        }

        private void Btn_Stop(object sender, RoutedEventArgs e)
        {          
            commandLine.setCommandLine(COMMAND.allesStop, 0, false);
            dataModel.sendText(commandLine.getCommandLine());
        }

        private void EnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.BtnStop.IsEnabled = !this.StackPanelControls.IsEnabled;
        }

    }
}

