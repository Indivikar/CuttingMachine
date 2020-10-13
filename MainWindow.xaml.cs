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
        private Thread threadCheckConnection;
        private Thread threadCheckPorts;

        delegate void SetTextCallback(string text);

        private string commandLine;
        StringBuilder sb = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
            this.dataModel = new DataModel(this);
            this.serialPort1 = dataModel.SerialPort1;

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

            SetText("-----------------------------------------------------------------------------------------------\n" +
                "Der Arduino muss mit \"Connected\" antworten, wenn der Arduino nicht antwortet,\n" +
                "könnte es der falsche Port sein oder es ist das falsche Programm auf dem Arduino.\n" +
                "-----------------------------------------------------------------------------------------------\n\n");
        }


        private void BtnClickVerbinden(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("try to Connect with Arduino...");                
                SetText("try to Connect with Arduino....\n");
                
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
                //Console.WriteLine("empfangen");
                Dispatcher.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }

        private void SetText(string text)
        {

            //Console.WriteLine(text);

            handleCommandLine(text);

            if (text.Length < 1)
            {
                Console.WriteLine("kleiner als 1");
                return;
            }

            string firstChar = text.Substring(0, 1);

            if (firstChar.Equals("#"))
            {
                text = "Arduino antwortet>> " + text;
            }

            text = Regex.Replace(text, @"#|@", "");

            sb.Append(text);
            string allLines = sb.ToString();

            string[] lines = allLines.Split('\n');

            

            if (lines.Length > 100)
            {
                var _values = lines.Skip(Math.Max(0, lines.Count() - 100));
                
                var last_lines = string.Join("", _values.ToArray()); 

                this.textBoxAusgabe.Text = last_lines;
            } 
            else 
            {
                this.textBoxAusgabe.Text += text;
            }

           
            textBoxAusgabe.ScrollToEnd();

            

            //string replacement = Regex.Replace(text, @"\t|\n|\r", "");
            //Console.WriteLine(line);
            //Console.WriteLine("replacement: " + replacement);

            //char c = text.Last();
            //if (c.Equals('@')) {
            //    //Console.WriteLine("empfangen");
            //    text = text.Remove(text.Length - 1);
            //    commandReceived(text);
            //}
        }

        private void handleCommandLine(string text) {

            text = text.Trim();
            text = Regex.Replace(text, @"\t|\n|\r", "");

            if (text.Length < 1) {
                Console.WriteLine("kleiner als 1");
                return;
            }

            string firstChar = text.Substring(0, 1);
            string lastChar = text.Substring(text.Length - 1);

            if (firstChar.Equals("#")) {
                commandLine = "";
            }

            commandLine += text;

            if (lastChar.Equals("@"))
            {
                Console.WriteLine(commandLine);
                commandLine = Regex.Replace(commandLine, @"#|@", "");
                commandReceived(commandLine);
            }

        }

        private void commandReceived(string text) {

            Console.WriteLine("commandReceived: " + text);

            foreach (COMMAND item in (COMMAND[])Enum.GetValues(typeof(COMMAND)))
            {
                if (text.Equals(item.ToString())) {
                    commandRun(item);
                }
            }
        }

        private void commandRun(COMMAND cmd)
        {
            switch (cmd)
            {
                case COMMAND.stepperFinished:
                    {
                        Console.WriteLine("COMMAND.stepperFinished");
                        dataModel.EinzelSchritt.StackPanelControlsEnable();
                        break;
                    }
                case COMMAND.Connected:
                    {
                        Console.WriteLine("COMMAND.Connected");
                        Main.IsEnabled = true;
                        break;
                    }
                default: break;
            }
        }

        private void isConnected()
        {
            if (serialPort1.IsOpen)
            {                
                BtnVerbinden.IsEnabled = false;
                BtnTrennen.IsEnabled = true;
                //Main.IsEnabled = true;
                labelConnection.Foreground = Brushes.Green;
                labelConnection.Text = "CONNECTED";
                Console.WriteLine("Connected");              
                serialPort1.Write("Connected#");

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
    }
}
