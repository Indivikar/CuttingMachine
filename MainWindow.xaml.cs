using SchneidMaschine.model;
using SchneidMaschine.pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
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
            //Main.IsEnabled = false;

            comboBoxPorts.ItemsSource = dataModel.PortList;
        }


        private void BtnClickVerbinden(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("try to Connect...");
                SetText("try to Connect...\n");
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

                //progressBar1.Value = 100;
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
                Dispatcher.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }

        private void SetText(string text)
        {
            this.textBoxAusgabe.Text += text;
            textBoxAusgabe.ScrollToEnd();
        }

        private void isConnected()
        {
            if (serialPort1.IsOpen)
            {                
                BtnVerbinden.IsEnabled = false;
                Thread.Sleep(1000);
                BtnTrennen.IsEnabled = true;
                Main.IsEnabled = true;
                labelConnection.Foreground = Brushes.Green;
                labelConnection.Text = "Connected";
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
                labelConnection.Text = "Disconnected";
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
