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
        private SerialPort serialPortRollenzentrierung;
        private SerialPort serialPortSchneidmaschine;
        private CommandLine commandLine;
        private Thread threadCheckConnection_Rollenzentrierung;
        private Thread threadCheckConnection_Schneidmaschine;
        private Thread threadCheckPorts;

        delegate void SetTextCallback(string text);

        
        StringBuilder sbRollenzentrierung = new StringBuilder();
        StringBuilder sbSchneidmaschine = new StringBuilder();

        StringBuilder befehlBuilder = new StringBuilder();
        
        // Zeitstempel für Verbindungsüberwachung
        DateTime lastCommunicationSchneidmaschine = DateTime.MinValue;
        DateTime lastCommunicationRollenzentrierung = DateTime.MinValue;
        
        // Timer für Verbindungsüberwachung
        System.Windows.Threading.DispatcherTimer connectionCheckTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.dataModel = new DataModel(this);
            this.serialPortRollenzentrierung = dataModel.SerialPortRollenzentrierung;
            this.serialPortSchneidmaschine = dataModel.SerialPortSchneidmaschine;
            this.commandLine = dataModel.CommandLine;

            this.Title = "Zuschnitt " + dataModel.AppVersion;

            Main.Content = dataModel.Home;

            serialPortRollenzentrierung.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived_Rollenzentrierung);
            serialPortSchneidmaschine.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived_Schneidmaschine);

            this.threadCheckPorts = new Thread(dataModel.Thread_Port_Scanner.checkPorts);
            threadCheckPorts.Name = "Port_Scanner";
            threadCheckPorts.Start();

            // Timer für Verbindungsüberwachung initialisieren
            InitializeConnectionMonitoring();

            Init();
        }
        
        private void InitializeConnectionMonitoring()
        {
            connectionCheckTimer = new System.Windows.Threading.DispatcherTimer();
            connectionCheckTimer.Interval = TimeSpan.FromSeconds(2); // Check alle 2 Sekunden
            connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
            connectionCheckTimer.Start();
            
            // Initial alle Punkte auf rot setzen
            UpdateConnectionStatus(false, false);
        }
        
        private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
        {
            // Prüfe ob letzte Kommunikation länger als 5 Sekunden her ist
            DateTime now = DateTime.Now;
            bool schneidmaschineConnected = (now - lastCommunicationSchneidmaschine).TotalSeconds < 5;
            bool rollenzentrierungConnected = (now - lastCommunicationRollenzentrierung).TotalSeconds < 5;
            
            UpdateConnectionStatus(schneidmaschineConnected, rollenzentrierungConnected);
        }
        
        private void UpdateConnectionStatus(bool schneidmaschineConnected, bool rollenzentrierungConnected)
        {
            // Status-Punkte aktualisieren
            statusDotSchneidmaschine.Fill = schneidmaschineConnected ? Brushes.Green : Brushes.Red;
            statusDotRollenzentrierung.Fill = rollenzentrierungConnected ? Brushes.Green : Brushes.Red;
            
            // Separate Zeitstempel für jedes Gerät anzeigen
            if (lastCommunicationRollenzentrierung != DateTime.MinValue)
            {
                textBlockLastCommRollenzentrierung.Text = "(" + lastCommunicationRollenzentrierung.ToString("HH:mm:ss") + ")";
            }
            else
            {
                textBlockLastCommRollenzentrierung.Text = "(nie)";
            }
            
            if (lastCommunicationSchneidmaschine != DateTime.MinValue)
            {
                textBlockLastCommSchneidmaschine.Text = "(" + lastCommunicationSchneidmaschine.ToString("HH:mm:ss") + ")";
            }
            else
            {
                textBlockLastCommSchneidmaschine.Text = "(nie)";
            }
        }

        private void Init()
        {
            BtnVerbindenRollenzentrierung.IsEnabled = true;
            BtnTrennenRollenzentrierung.IsEnabled = false;
            BtnVerbindenSchneidmaschine.IsEnabled = true;
            BtnTrennenSchneidmaschine.IsEnabled = false;

            Main.IsEnabled = false;

            comboBoxPortsRollenzentrierung.ItemsSource = dataModel.PortList;
            comboBoxPortsSchneidmaschine.ItemsSource = dataModel.PortList;

            SetTextRollenzentrierung("&-----------------------------------------------------------------------------------------------\n" +
                "Der ESP32 muss mit \"Connected\" antworten, wenn der ESP32 nicht antwortet,\n" +
                "könnte es der falsche Port sein oder es ist das falsche Programm auf dem ESP32.\n" +
                "-----------------------------------------------------------------------------------------------\n\n&");

            SetTextSchneidmaschine("&-----------------------------------------------------------------------------------------------\n" +
                "Der Arduino muss mit \"Connected\" antworten, wenn der Arduino nicht antwortet,\n" +
                "könnte es der falsche Port sein oder es ist das falsche Programm auf dem Arduino.\n" +
                "-----------------------------------------------------------------------------------------------\n\n&");
        }

        // --------------------------------------
        //      Verbindung Rollenzentrierung
        // --------------------------------------

        private void BtnClickVerbindenRollenzentrierung(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("try to Connect with ESP32...");
                SetTextRollenzentrierung("&try to Connect with ESP32....&\n");

                serialPortRollenzentrierung.PortName = comboBoxPortsRollenzentrierung.Text;
                serialPortRollenzentrierung.BaudRate = Convert.ToInt32(comboBoxBautRateRollenzentrierung.Text);

                //          serialPortSchneidmaschine = new SerialPort("COM5",
                //9600, Parity.None, 8, StopBits.One);

                //serialPortSchneidmaschine.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                //serialPortSchneidmaschine.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                //serialPortSchneidmaschine.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);
                //serialPortSchneidmaschine.DataReceived += new SerialDataReceivedEventHandler(dataModel.port_DataReceived_Schneidmaschine);
                //serialPortSchneidmaschine.DataReceived += new SerialDataReceivedEventHandler(tmrPollForRecivedData_Tick);
                serialPortRollenzentrierung.Open();

                isConnected_Rollenzentrierung();

                //ProgressBarStatus.Value = 100;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickTrennenRollenzentrierung(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPortRollenzentrierung.Close();
                isConnected_Rollenzentrierung();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickSendenRollenzentrierung(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPortRollenzentrierung.Write(textBoxSendenRollenzentrierung.Text + (char)CharApp.END_CHAR);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickTextDeleteRollenzentrierung(object sender, RoutedEventArgs e)
        {
            sbRollenzentrierung.Clear();
            textBoxAusgabeRollenzentrierung.Text = String.Empty;
        }

        private void isConnected_Rollenzentrierung()
        {
            if (serialPortRollenzentrierung.IsOpen)
            {
                BtnVerbindenRollenzentrierung.IsEnabled = false;
                BtnTrennenRollenzentrierung.IsEnabled = true;
                buttonTestRollenzentrierung.IsEnabled = true;

                commandLine.setCommandLine(COMMAND_Schneidmaschine.Connected, 0, false);
                dataModel.sendTextRollenzentrierung(commandLine.getCommandLine());

                if (threadCheckConnection_Rollenzentrierung == null)
                {
                    this.threadCheckConnection_Rollenzentrierung = new Thread(dataModel.Thread_Con_Rollenzentrierung.checkVerbindung_Rollenzentrierung);
                    threadCheckConnection_Rollenzentrierung.Name = "Rollenzentrierung_VerbindungsCheck";
                    //threadCheckConnection_Rollenzentrierung.IsBackground = true;
                }

                if (!threadCheckConnection_Rollenzentrierung.IsAlive)
                {
                    threadCheckConnection_Rollenzentrierung.Start();
                }
            }
            else
            {
                BtnVerbindenRollenzentrierung.IsEnabled = true;
                BtnTrennenRollenzentrierung.IsEnabled = false;
                buttonTestRollenzentrierung.IsEnabled = false;
                //Main.IsEnabled = false;
                labelConnectionRollenzentrierung.Foreground = Brushes.Red;
                labelConnectionRollenzentrierung.Text = "DISCONNECTED R";
                Console.WriteLine("Disconnected Rollenzentrierung");
                SetTextRollenzentrierung("Disconnected Rollenzentrierung\n");

                if (threadCheckConnection_Rollenzentrierung != null && threadCheckConnection_Rollenzentrierung.IsAlive)
                {
                    threadCheckConnection_Rollenzentrierung.Abort();
                    threadCheckConnection_Rollenzentrierung = null;
                }
            }
        }

        private void SetTextRollenzentrierung(string text)
        {

            // normaler Text aus C# Programm
            if (text.StartsWith("&"))
            {
                text = Regex.Replace(text, @"&", "");
                this.textBoxAusgabeRollenzentrierung.Text += text;
                return;
            }

            if (stringToChar(text) == null)
            {
                return;
            }

            text = befehlBuilder.ToString();

            handleCommandLineRollenzentrierung(text);

            //Console.WriteLine("text -> " + text);

            //string firstChar = text.Substring(0, 1);

            // Befehl von Arduino an C# ohne Text-Ausgabe
            if (text.StartsWith("%"))
            {
                return;
            }
             
            if (text.StartsWith(Char.ToString((char)CharArduino.START_CHAR)))
            {
                text = Regex.Replace(text, @"~|#|@|%", "");
                text = "ESP32 antwortet>> " + text;
            }

            sbRollenzentrierung.Append(text);
            string allLines = sbRollenzentrierung.ToString();
            string[] lines = allLines.Split('\n');

            if (lines.Length > 100)
            {
                var _values = lines.Skip(Math.Max(0, lines.Count() - 100));

                var last_lines = string.Join("\n", _values.ToArray());

                this.textBoxAusgabeRollenzentrierung.Text = last_lines;
            }
            else
            {
                this.textBoxAusgabeRollenzentrierung.Text += text;
            }

            textBoxAusgabeRollenzentrierung.ScrollToEnd();
        }

        private void handleCommandLineRollenzentrierung(string text)
        {

            text = text.Trim();
            text = Regex.Replace(text, @"\t|\n|\r", "");

            //string firstChar = text.Substring(0, 1);
            string lastChar = text.Substring(text.Length - 1);


            if (lastChar.Equals("@"))
            {
                text = Regex.Replace(text, @"~|#|@|%", "");
                //Console.WriteLine(text);
                commandReceivedRollenzentrierung(text);
            }
        }

        private void commandReceivedRollenzentrierung(string text)
        {

            Console.WriteLine("commandReceivedRollenzentrierung: " + text);

            string[] befehl = text.Split('_');

            foreach (COMMAND_Rollenzentrierung item in (COMMAND_Rollenzentrierung[])Enum.GetValues(typeof(COMMAND_Rollenzentrierung)))
            {
                if (befehl[0].Equals(item.ToString()))
                {
                    commandRunRollenzentrierung(item, befehl);
                }
            }
        }

        private void commandRunRollenzentrierung(COMMAND_Rollenzentrierung cmd, string[] befehl)
        {
            Console.WriteLine(cmd);
            switch (cmd)
            {

                case COMMAND_Rollenzentrierung.steps:
                    {
                        Console.WriteLine("COMMAND_Rollenzentrierung.steps_" + befehl[1]);
                        dataModel.setIstWert(befehl[1]);
                        //dataModel.EinzelSchritt.setIstWertInMM(befehl[1]);
                        break;
                    }

                case COMMAND_Rollenzentrierung.stepperFinished:
                    {
                        Console.WriteLine("COMMAND_Rollenzentrierung.stepperFinished -> " + befehl[1]);
                        dataModel.IsStepperFinished = true;
                        dataModel.setIstWert(befehl[1]);

                        if (dataModel.EinzelSchritt.IsVisible)
                        {
                            dataModel.EinzelSchritt.StackPanelControlsEnable();
                        }
                        break;
                    }

                case COMMAND_Rollenzentrierung.Connected:
                    {
                        Console.WriteLine("COMMAND_Rollenzentrierung.Connected");
                        Main.IsEnabled = true;
                        labelConnectionRollenzentrierung.Foreground = Brushes.Green;
                        labelConnectionRollenzentrierung.Text = "CONNECTED";
                        Console.WriteLine("Connected");

                        break;
                    }

                default: break;
            }
        }

        private void port_DataReceived_Rollenzentrierung(object sender, SerialDataReceivedEventArgs e)
        {
            string InputData = serialPortRollenzentrierung.ReadExisting();
            if (InputData != String.Empty)
            {
                // Zeitstempel für Verbindungsüberwachung aktualisieren
                lastCommunicationRollenzentrierung = DateTime.Now;
                Dispatcher.BeginInvoke(new SetTextCallback(SetTextRollenzentrierung), new object[] { InputData });
            }
        }

        // --------------------------------------
        //      Verbindung Schneidmaschine
        // --------------------------------------

        private void BtnClickVerbindenSchneidmaschine(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("try to Connect with Arduino...");
                SetTextSchneidmaschine("&try to Connect with Arduino....&\n");

                serialPortSchneidmaschine.PortName = comboBoxPortsSchneidmaschine.Text;
                serialPortSchneidmaschine.BaudRate = Convert.ToInt32(comboBoxBautRateSchneidmaschine.Text);

                //          serialPortSchneidmaschine = new SerialPort("COM5",
                //9600, Parity.None, 8, StopBits.One);

                //serialPortSchneidmaschine.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                //serialPortSchneidmaschine.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                //serialPortSchneidmaschine.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);
                //serialPortSchneidmaschine.DataReceived += new SerialDataReceivedEventHandler(dataModel.port_DataReceived_Schneidmaschine);
                //serialPortSchneidmaschine.DataReceived += new SerialDataReceivedEventHandler(tmrPollForRecivedData_Tick);
                serialPortSchneidmaschine.Open();

                isConnected_Schneidmaschine();

                //ProgressBarStatus.Value = 100;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickTrennenSchneidmaschine(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPortSchneidmaschine.Close();
                isConnected_Schneidmaschine();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickSendenSchneidmaschine(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPortSchneidmaschine.Write(textBoxSendenSchneidmaschine.Text + (char)CharApp.END_CHAR);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void BtnClickTextDeleteSchneidmaschine(object sender, RoutedEventArgs e)
        {
            sbSchneidmaschine.Clear();
            textBoxAusgabeSchneidmaschine.Text = String.Empty;
        }
        
        // Test-Button-Handler für Verbindungstests
        private void BtnClickTestRollenzentrierung(object sender, RoutedEventArgs e)
        {
            if (serialPortRollenzentrierung.IsOpen)
            {
                try
                {
                    serialPortRollenzentrierung.WriteLine("%TEST#");
                    SetTextRollenzentrierung("Test-Befehl an Rollenzentrierung gesendet\n");
                }
                catch (Exception ex)
                {
                    SetTextRollenzentrierung("Fehler beim Senden des Test-Befehls: " + ex.Message + "\n");
                }
            }
            else
            {
                SetTextRollenzentrierung("Rollenzentrierung nicht verbunden - Test nicht möglich\n");
            }
        }
        
        private void BtnClickTestSchneidmaschine(object sender, RoutedEventArgs e)
        {
            if (serialPortSchneidmaschine.IsOpen)
            {
                try
                {
                    serialPortSchneidmaschine.WriteLine("%TEST#");
                    SetTextSchneidmaschine("Test-Befehl an Schneidmaschine gesendet\n");
                }
                catch (Exception ex)
                {
                    SetTextSchneidmaschine("Fehler beim Senden des Test-Befehls: " + ex.Message + "\n");
                }
            }
            else
            {
                SetTextSchneidmaschine("Schneidmaschine nicht verbunden - Test nicht möglich\n");
            }
        }

        private void isConnected_Schneidmaschine()
        {
            if (serialPortSchneidmaschine.IsOpen)
            {
                BtnVerbindenSchneidmaschine.IsEnabled = false;
                BtnTrennenSchneidmaschine.IsEnabled = true;
                buttonTestSchneidmaschine.IsEnabled = true;

                commandLine.setCommandLine(COMMAND_Schneidmaschine.Connected, 0, false);
                dataModel.sendTextSchneidmaschine(commandLine.getCommandLine());

                if (threadCheckConnection_Schneidmaschine == null)
                {
                    this.threadCheckConnection_Schneidmaschine = new Thread(dataModel.Thread_Con_Schneidmaschine.checkVerbindung_Schneidmaschine);
                    threadCheckConnection_Schneidmaschine.Name = "Schneidmaschine_VerbindungsCheck";
                }

                if (!threadCheckConnection_Schneidmaschine.IsAlive)
                {
                    threadCheckConnection_Schneidmaschine.Start();
                }
            }
            else
            {
                BtnVerbindenSchneidmaschine.IsEnabled = true;
                BtnTrennenSchneidmaschine.IsEnabled = false;
                buttonTestSchneidmaschine.IsEnabled = false;
                Main.IsEnabled = false;
                labelConnectionSchneidmaschine.Foreground = Brushes.Red;
                labelConnectionSchneidmaschine.Text = "DISCONNECTED S";
                Console.WriteLine("Disconnected Schneidmaschine");
                SetTextSchneidmaschine("Disconnected Schneidmaschine\n");


                if (threadCheckConnection_Schneidmaschine != null && threadCheckConnection_Schneidmaschine.IsAlive)
                {
                    threadCheckConnection_Schneidmaschine.Abort();
                    threadCheckConnection_Schneidmaschine = null;
                }
            }
        }

        private void SetTextSchneidmaschine(string text)
        {
            
            // normaler Text aus C# Programm
            if (text.StartsWith("&"))
            {
                text = Regex.Replace(text, @"&", "");
                this.textBoxAusgabeSchneidmaschine.Text += text;
                return;
            }

            if (stringToChar(text) == null)
            {
                return;
            }

            text = befehlBuilder.ToString();

            handleCommandLineSchneidmaschine(text);

            //Console.WriteLine("text -> " + text);

            //string firstChar = text.Substring(0, 1);

            // Befehl von Arduino an C# ohne Text-Ausgabe
            if (text.StartsWith("%"))
            {
                return;
            }

            if (text.StartsWith(Char.ToString((char)CharArduino.START_CHAR)))
            {
                text = Regex.Replace(text, @"~|#|@|%", "");
                text = "Arduino antwortet>> " + text;
            }

            sbSchneidmaschine.Append(text);
            string allLines = sbSchneidmaschine.ToString();
            string[] lines = allLines.Split('\n');

            if (lines.Length > 100)
            {
                var _values = lines.Skip(Math.Max(0, lines.Count() - 100));

                var last_lines = string.Join("\n", _values.ToArray());

                this.textBoxAusgabeSchneidmaschine.Text = last_lines;
            }
            else
            {
                this.textBoxAusgabeSchneidmaschine.Text += text;
            }

            textBoxAusgabeSchneidmaschine.ScrollToEnd();
        }

        private void handleCommandLineSchneidmaschine(string text) {

            text = text.Trim();
            text = Regex.Replace(text, @"\t|\n|\r", "");

            //string firstChar = text.Substring(0, 1);
            string lastChar = text.Substring(text.Length - 1);


            if (lastChar.Equals("@"))
            {
                text = Regex.Replace(text, @"~|#|@|%", "");
                //Console.WriteLine(text);
                commandReceivedSchneidmaschine(text);
            }
        }

        private void commandReceivedSchneidmaschine(string text) {

            Console.WriteLine("commandReceivedSchneidmaschine: " + text);

            string[] befehl = text.Split('_');

            foreach (COMMAND_Schneidmaschine item in (COMMAND_Schneidmaschine[])Enum.GetValues(typeof(COMMAND_Schneidmaschine)))
            {
                if (befehl[0].Equals(item.ToString())) {
                    commandRunSchneidmaschine(item, befehl);
                }
            }
        }

        private void commandRunSchneidmaschine(COMMAND_Schneidmaschine cmd, string[] befehl)
        {
            Console.WriteLine(cmd);
            switch (cmd)
            {

                case COMMAND_Schneidmaschine.allesGestoppt:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.allesGestoppt");
                        dataModel.HalbAuto.BtnModusHalbAutoStart.IsEnabled = true;
                        dataModel.HalbAuto.BtnModusHalbAutoStop.IsEnabled = false;

                        dataModel.Auto.TextBoxRuns.IsEnabled = true;
                        dataModel.Auto.BtnModusAutoStart.IsEnabled = true;
                        dataModel.Auto.BtnModusAutoPause.IsEnabled = false;
                        dataModel.Auto.BtnModusAutoStop.IsEnabled = false;

                        SetTextSchneidmaschine("#alles gestoppt@");
                        break;
                    }

                case COMMAND_Schneidmaschine.handradOn:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.handradOn");
                        dataModel.EinzelSchritt.ToggleBtn_Handwheel.IsChecked = true;
                        break;
                    }

                case COMMAND_Schneidmaschine.handradOff:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.handradOff");
                        dataModel.EinzelSchritt.ToggleBtn_Handwheel.IsChecked = false;
                        break;
                    }

                case COMMAND_Schneidmaschine.schneidenStartet:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.schneidenStartet");
                        Main.IsEnabled = false;
                        break;
                    }
                    

                case COMMAND_Schneidmaschine.schneidenBeendet:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.schneidenBeendet");
                        dataModel.IsCutFinished = true;
                        Main.IsEnabled = true;
                        refreshStats(false);
                        dataModel.DBHandler.updateCut();
                        break;
                    }

                case COMMAND_Schneidmaschine.kopfSchnittBeendet:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.kopfSchnittBeendet");
                        dataModel.IsCutFinished = true;
                        Main.IsEnabled = true;
                        refreshStats(true);
                        dataModel.DBHandler.updateCut();
                        break;
                    }

                case COMMAND_Schneidmaschine.steps:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.steps_" + befehl[1]);
                        dataModel.setIstWert(befehl[1]);
                        //dataModel.EinzelSchritt.setIstWertInMM(befehl[1]);
                        break;
                    }

                case COMMAND_Schneidmaschine.stepperFinished:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.stepperFinished -> " + befehl[1]);
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

                case COMMAND_Schneidmaschine.Connected:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.Connected");
                        Main.IsEnabled = true;
                        labelConnectionSchneidmaschine.Foreground = Brushes.Green;
                        labelConnectionSchneidmaschine.Text = "CONNECTED";
                        Console.WriteLine("Connected");

                        break;
                    }

                case COMMAND_Schneidmaschine.resetIstWert:
                    {
                        Console.WriteLine("COMMAND_Schneidmaschine.resetIstWert");

                        Statistik stats = dataModel.Statistik;
                        stats.StreifenLaengeIst = 0;

                        break;
                    }

                default: break;
            }
        }

        private void port_DataReceived_Schneidmaschine(object sender, SerialDataReceivedEventArgs e)
        {
            string InputData = serialPortSchneidmaschine.ReadExisting();
            if (InputData != String.Empty)
            {
                // Zeitstempel für Verbindungsüberwachung aktualisieren
                lastCommunicationSchneidmaschine = DateTime.Now;
                Dispatcher.BeginInvoke(new SetTextCallback(SetTextSchneidmaschine), new object[] { InputData });
            }
        }

        // --------------------------------------------------------
        //      Verbindung Rollenzentrierung & Schneidmaschine
        // --------------------------------------------------------

        private void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            serialPortRollenzentrierung.Close();
            serialPortSchneidmaschine.Close();

            if (threadCheckConnection_Schneidmaschine != null) {
                threadCheckConnection_Schneidmaschine.Abort();
            }

            if (threadCheckPorts != null) {
                threadCheckPorts.Abort();
            }
        }



        public void checkConnection()
        {
            if (!serialPortRollenzentrierung.IsOpen)
            {
                BtnVerbindenRollenzentrierung.IsEnabled = true;
                BtnTrennenRollenzentrierung.IsEnabled = false;

                //Main.IsEnabled = false;
                labelConnectionRollenzentrierung.Foreground = Brushes.Red;
                labelConnectionRollenzentrierung.Text = "Disconnected R";
                Console.WriteLine("Disconnected R");
                SetTextRollenzentrierung("Disconnected R\n");

                if (threadCheckConnection_Rollenzentrierung != null && threadCheckConnection_Rollenzentrierung.IsAlive)
                {
                    threadCheckConnection_Rollenzentrierung.Abort();
                    threadCheckConnection_Rollenzentrierung = null;
                }
            }

            if (!serialPortSchneidmaschine.IsOpen)
            {
                BtnVerbindenSchneidmaschine.IsEnabled = true;
                BtnTrennenSchneidmaschine.IsEnabled = false;

                Main.IsEnabled = false;
                labelConnectionSchneidmaschine.Foreground = Brushes.Red;
                labelConnectionSchneidmaschine.Text = "Disconnected S";
                Console.WriteLine("Disconnected S");
                SetTextSchneidmaschine("Disconnected S\n");

                if (threadCheckConnection_Schneidmaschine != null && threadCheckConnection_Schneidmaschine.IsAlive)
                {
                    threadCheckConnection_Schneidmaschine.Abort();
                    threadCheckConnection_Schneidmaschine = null;
                }
            }
        }

        // --------------------------------------
        //               Statistik
        // --------------------------------------

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

        private string stringToChar(string text)
        {

            string newText = null;

            text = text.Trim();
            text = Regex.Replace(text, @"\t+|\n+|\r+|(\r\n)+", "");

            char[] charArr = text.ToCharArray();
            foreach (char ch in charArr)
            {
                // Start der Commandline
                if (ch.Equals('%') || ch.Equals((char)CharArduino.START_CHAR))
                {
                    newText = null;
                    befehlBuilder.Clear();
                }

                befehlBuilder.Append(ch);

                // Ende der Commandline
                if (ch.Equals((char)CharArduino.END_CHAR))
                {
                    befehlBuilder.Append("\n");
                    newText = befehlBuilder.ToString();
                }
            }

            return newText;
        }


        private void ButtonOpenConnections_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenConnections.Visibility = Visibility.Collapsed;
            ButtonCloseConnections.Visibility = Visibility.Visible;
        }

        private void ButtonCloseConnections_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenConnections.Visibility = Visibility.Visible;
            ButtonCloseConnections.Visibility = Visibility.Collapsed;
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
