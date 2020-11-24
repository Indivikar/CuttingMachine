using SchneidMaschine.db;
using SchneidMaschine.model;
using SchneidMaschine.pages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Ports;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchneidMaschine
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataModel dataModel;
        private SerialPort serialPort1;
        private CommandLine commandLine;
        private Thread threadCheckConnection;
        private Thread threadCheckPorts;

        delegate void SetTextCallback(string text);

        
        StringBuilder sb = new StringBuilder();

        StringBuilder befehlBuilder = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
            this.dataModel = new DataModel(this);            
            this.serialPort1 = dataModel.SerialPort1;
            this.commandLine = dataModel.CommandLine;

            Main.Content = dataModel.Home;

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived_1);

            this.threadCheckPorts = new Thread(dataModel.MyThreads.checkPorts);
            threadCheckPorts.Start();

            Init();
        }

        private void Init()
        {
            BtnVerbinden.IsEnabled = true;
            BtnTrennen.IsEnabled = false;
            Main.IsEnabled = false;

            comboBoxPorts.ItemsSource = dataModel.PortList;

            SetText("&-----------------------------------------------------------------------------------------------\n" +
                "Der Arduino muss mit \"Connected\" antworten, wenn der Arduino nicht antwortet,\n" +
                "könnte es der falsche Port sein oder es ist das falsche Programm auf dem Arduino.\n" +
                "-----------------------------------------------------------------------------------------------\n\n&");
        }


        private void BtnClickVerbinden(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("try to Connect with Arduino...");                
                SetText("&try to Connect with Arduino....&\n");
                
                serialPort1.PortName = comboBoxPorts.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBoxBautRate.Text);

                //          serialPort1 = new SerialPort("COM5",
                //9600, Parity.None, 8, StopBits.One);

                //serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                //serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                //serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);
                //serialPort1.DataReceived += new SerialDataReceivedEventHandler(dataModel.port_DataReceived_1);
                //serialPort1.DataReceived += new SerialDataReceivedEventHandler(tmrPollForRecivedData_Tick);
                serialPort1.Open();

                isConnected();

                //ProgressBarStatus.Value = 100;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickTrennen(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPort1.Close();
                isConnected();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickSenden(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPort1.Write(textBoxSenden.Text + "#");
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickTextDelete(object sender, RoutedEventArgs e)
        {
            sb.Clear();
            textBoxAusgabe.Text = String.Empty;
        }

        private void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            serialPort1.Close();
            if (threadCheckConnection != null) {
                threadCheckConnection.Abort();
            }

            if (threadCheckPorts != null) {
                threadCheckPorts.Abort();
            }
        }

        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            string InputData = serialPort1.ReadExisting();
            if (InputData != String.Empty)
            {
                Dispatcher.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }

        private string stringToChar(string text) {

            string newText = null;

            text = text.Trim();
            text = Regex.Replace(text, @"\t+|\n+|\r+|(\r\n)+", "");

            char[] charArr = text.ToCharArray();
            foreach (char ch in charArr)
            {
                if (ch.Equals('%') || ch.Equals('#'))
                {
                    newText = null;
                    befehlBuilder.Clear();
                }

                befehlBuilder.Append(ch);

                if (ch.Equals('@'))
                {
                    befehlBuilder.Append("\n");
                    newText = befehlBuilder.ToString();
                }
            }

            return newText;
        }


        private void SetText(string text)
        {
            
            // normaler Text aus C# Programm
            if (text.StartsWith("&"))
            {
                text = Regex.Replace(text, @"&", "");
                this.textBoxAusgabe.Text += text;
                return;
            }

            if (stringToChar(text) == null)
            {
                return;
            }

            text = befehlBuilder.ToString();

            handleCommandLine(text);

            //Console.WriteLine("text -> " + text);

            //string firstChar = text.Substring(0, 1);

            // Befehl von Arduino an C# ohne Text-Ausgabe
            if (text.StartsWith("%"))
            {
                return;
            }

            if (text.StartsWith("#"))
            {
                text = Regex.Replace(text, @"#|@|%", "");
                text = "Arduino antwortet>> " + text;
            }

            sb.Append(text);
            string allLines = sb.ToString();
            string[] lines = allLines.Split('\n');

            if (lines.Length > 100)
            {
                var _values = lines.Skip(Math.Max(0, lines.Count() - 100));

                var last_lines = string.Join("\n", _values.ToArray());

                this.textBoxAusgabe.Text = last_lines;
            }
            else
            {
                this.textBoxAusgabe.Text += text;
            }

            textBoxAusgabe.ScrollToEnd();
        }

        private void handleCommandLine(string text) {

            text = text.Trim();
            text = Regex.Replace(text, @"\t|\n|\r", "");

            //string firstChar = text.Substring(0, 1);
            string lastChar = text.Substring(text.Length - 1);


            if (lastChar.Equals("@"))
            {
                text = Regex.Replace(text, @"#|@|%", "");
                Console.WriteLine(text);
                commandReceived(text);
            }

        }

        private void commandReceived(string text) {

            Console.WriteLine("commandReceived: " + text);

            string[] befehl = text.Split('_');

            foreach (COMMAND item in (COMMAND[])Enum.GetValues(typeof(COMMAND)))
            {
                if (befehl[0].Equals(item.ToString())) {
                    commandRun(item, befehl);
                }
            }
        }

        private void commandRun(COMMAND cmd, string[] befehl)
        {
            switch (cmd)
            {

                case COMMAND.allesGestoppt:
                    {
                        Console.WriteLine("COMMAND.allesGestoppt");
                        dataModel.HalbAuto.BtnModusHalbAutoStart.IsEnabled = true;
                        dataModel.HalbAuto.BtnModusHalbAutoStop.IsEnabled = false;

                        dataModel.Auto.TextBoxRuns.IsEnabled = true;
                        dataModel.Auto.BtnModusAutoStart.IsEnabled = true;
                        dataModel.Auto.BtnModusAutoPause.IsEnabled = false;
                        dataModel.Auto.BtnModusAutoStop.IsEnabled = false;

                        SetText("#alles gestoppt@");
                        break;
                    }

                case COMMAND.handradOn:
                    {
                        Console.WriteLine("COMMAND.handradOn");
                        dataModel.EinzelSchritt.ToggleBtn_Handwheel.IsChecked = true;
                        break;
                    }

                case COMMAND.handradOff:
                    {
                        Console.WriteLine("COMMAND.handradOff");
                        dataModel.EinzelSchritt.ToggleBtn_Handwheel.IsChecked = false;
                        break;
                    }

                case COMMAND.schneidenStartet:
                    {
                        Console.WriteLine("COMMAND.schneidenStartet");
                        Main.IsEnabled = false;
                        break;
                    }
                    

                case COMMAND.schneidenBeendet:
                    {
                        Console.WriteLine("COMMAND.schneidenBeendet");
                        dataModel.IsCutFinished = true;
                        Main.IsEnabled = true;
                        refreshStats(false);
                        dataModel.DBHandler.updateCut();
                        break;
                    }

                case COMMAND.kopfSchnittBeendet:
                    {
                        Console.WriteLine("COMMAND.kopfSchnittBeendet");
                        dataModel.IsCutFinished = true;
                        Main.IsEnabled = true;
                        refreshStats(true);
                        dataModel.DBHandler.updateCut();
                        break;
                    }

                case COMMAND.steps:
                    {
                        Console.WriteLine("COMMAND.steps_" + befehl[1]);
                        dataModel.setIstWert(befehl[1]);
                        //dataModel.EinzelSchritt.setIstWertInMM(befehl[1]);
                        break;
                    }

                case COMMAND.stepperFinished:
                    {
                        Console.WriteLine("COMMAND.stepperFinished -> " + befehl[1]);
                        dataModel.IsStepperFinished = true;
                        dataModel.setIstWert(befehl[1]);

                        if (dataModel.EinzelSchritt.IsVisible)
                        {
                            dataModel.EinzelSchritt.StackPanelControlsEnable();
                        }
                        
                        //if (dataModel.HalbAuto.IsVisible) 
                        //{
                        //    dataModel.HalbAuto.cut();
                        //}



                        //dataModel.EinzelSchritt.setIstWertInMM(befehl[1]);
                        //this.textBoxAusgabe.Text += Regex.Replace(befehl[0], @"_", "");
                        break;
                    }

                case COMMAND.Connected:
                    {
                        Console.WriteLine("COMMAND.Connected");
                        Main.IsEnabled = true;
                        labelConnection.Foreground = Brushes.Green;
                        labelConnection.Text = "CONNECTED";
                        Console.WriteLine("Connected");

                        break;
                    }

                case COMMAND.resetIstWert:
                    {
                        Console.WriteLine("COMMAND.resetIstWert");

                        Statistik stats = dataModel.Statistik;
                        stats.StreifenLaengeIst = 0;

                        break;
                    }

                default: break;
            }
        }

        private void refreshStats(bool isKopfschnitt) 
        {
            Statistik stats = dataModel.Statistik;

            long streifenLaengeIst = stats.StreifenLaengeIst;

            bool a = dataModel.SelectedStreifen.Equals(STREIFEN.C4_40_Schachtel_KURZ) && !isKopfschnitt;
            bool b = dataModel.SelectedStreifen.Equals(STREIFEN.C4_40_Schachtel_LANG) && !isKopfschnitt;
            bool c = dataModel.SelectedStreifen.Equals(STREIFEN.C4_70_Deckel) && !isKopfschnitt;

            if(a) stats.HeuteStreifen40erKurz += dataModel.StreifenProSchnitt40er;
            if(b) stats.HeuteStreifen40erLang += dataModel.StreifenProSchnitt40er;
            if(c) stats.HeuteStreifen70erDeckel += dataModel.StreifenProSchnitt70er;
            stats.HeuteRolleAbgewickelt += streifenLaengeIst;

            if(a) stats.RolleStreifen40erKurz += dataModel.StreifenProSchnitt40er;
            if(b) stats.RolleStreifen40erLang += dataModel.StreifenProSchnitt40er;
            if(c) stats.RolleStreifen70erDeckel += dataModel.StreifenProSchnitt70er;
            stats.RolleIstLaenge -= streifenLaengeIst;

            if(a) stats.LangzeitStreifen40erKurz += dataModel.StreifenProSchnitt40er;
            if(b) stats.LangzeitStreifen40erLang += dataModel.StreifenProSchnitt40er;
            if(c) stats.LangzeitStreifen70erDeckel += dataModel.StreifenProSchnitt70er;


            stats.StreifenLaengeIst = 0;

            dataModel.Auto.SetMaxDurchlauf();
        }


        private void isConnected()
        {
            if (serialPort1.IsOpen)
            {                
                BtnVerbinden.IsEnabled = false;
                BtnTrennen.IsEnabled = true;
                //Main.IsEnabled = true;
                //labelConnection.Foreground = Brushes.Green;
                //labelConnection.Text = "CONNECTED";
                //Console.WriteLine("Connected");              
                //serialPort1.Write("Connected#");

                commandLine.setCommandLine(COMMAND.Connected, 0, false);
                dataModel.sendText(commandLine.getCommandLine());

                this.threadCheckConnection = new Thread(dataModel.MyThreads.checkVerbindung);
                threadCheckConnection.Start();
            }
            else
            {
                BtnVerbinden.IsEnabled = true;
                BtnTrennen.IsEnabled = false;
                Main.IsEnabled = false;
                labelConnection.Foreground = Brushes.Red;
                labelConnection.Text = "DISCONNECTED";
                Console.WriteLine("Disconnected");
                SetText("Disconnected\n");

                threadCheckConnection.Abort();               
            }
        }

        public void checkConnection() {
            if (!serialPort1.IsOpen)
            {
                BtnVerbinden.IsEnabled = true;
                BtnTrennen.IsEnabled = false;
                Main.IsEnabled = false;
                labelConnection.Foreground = Brushes.Red;
                labelConnection.Text = "Disconnected";
                Console.WriteLine("Disconnected");
                SetText("Disconnected\n");

                threadCheckConnection.Abort();
            }
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonStatsOpen_Click(object sender, RoutedEventArgs e)
        {
            ButtonStatsOpen.Visibility = Visibility.Collapsed;
            ButtonStatsClose.Visibility = Visibility.Visible;
        }

        private void ButtonStatsClose_Click(object sender, RoutedEventArgs e)
        {
            ButtonStatsOpen.Visibility = Visibility.Visible;
            ButtonStatsClose.Visibility = Visibility.Collapsed;
        }

        private void ButtonHeuteReset_Click(object sender, RoutedEventArgs e)
        {
            dataModel.DBHandler.resetHeute();

        }

        private void ButtonResetRolle_Click(object sender, RoutedEventArgs e)
        {
            dataModel.DBHandler.resetRolle();
        }

        private void ButtonNeueRolle_Click(object sender, RoutedEventArgs e)
        {
            dataModel.DBHandler.resetRolle();

            dataModel.Statistik.LangzeitVerbrauchteRollen += 1;
            dataModel.DBHandler.updateVerbrauchteRolle();
        }

        private void ButtonLangzeitReset_Click(object sender, RoutedEventArgs e)
        {
            dataModel.DBHandler.resetLangzeit();

        }
    }
}
