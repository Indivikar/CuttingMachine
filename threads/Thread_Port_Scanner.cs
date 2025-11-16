using SchneidMaschine.model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SchneidMaschine.threads
{
    public class Thread_Port_Scanner
    {
        private DataModel dataModel;
        private ComboBox comboBoxPorts_Schneidmaschine;
        private ComboBox comboBoxPorts_Rollenzentrierung;

        public delegate void AddNewPorts(string[] portList);

        public Thread_Port_Scanner(DataModel dataModel) 
        {
            this.dataModel = dataModel;
            this.comboBoxPorts_Rollenzentrierung = dataModel.MainWindow.comboBoxPortsRollenzentrierung;
            this.comboBoxPorts_Schneidmaschine = dataModel.MainWindow.comboBoxPortsSchneidmaschine;
        }

        public void checkPorts()
        {
            while (true)
            {
                Thread.Sleep(1000);

                SerialPort serialPort1 = new SerialPort();
                string[] portList = SerialPort.GetPortNames();

                int oldPorts = comboBoxPorts_Rollenzentrierung.Items.Count;
                int newPorts = portList.Length;

                //Console.WriteLine("Ports alt " + oldPorts + " == " + newPorts);

                if (oldPorts != newPorts)
                {
                    //Console.WriteLine("neue Ports ");
                    comboBoxPorts_Rollenzentrierung.Dispatcher.Invoke(() => {
                        comboBoxPorts_Rollenzentrierung.ItemsSource = portList;
                    });

                    comboBoxPorts_Schneidmaschine.Dispatcher.Invoke(() => {
                        comboBoxPorts_Schneidmaschine.ItemsSource = portList;
                    });
                }

            }
        }
    }
}
