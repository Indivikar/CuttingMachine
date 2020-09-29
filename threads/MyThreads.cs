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
    public class MyThreads
    {
        private DataModel dataModel;
        private ComboBox comboBoxPorts;

        public delegate void AddNewPorts(string[] portList);

        public MyThreads(DataModel dataModel) 
        {
            this.dataModel = dataModel;
            this.comboBoxPorts = dataModel.MainWindow.comboBoxPorts;
        }

        public void checkVerbindung()
        {
            while (true)
            {
                Thread.Sleep(1000);
                //Console.WriteLine("neue Ports ");
                dataModel.MainWindow.Dispatcher.Invoke(() => {
                    dataModel.MainWindow.checkConnection();
                });
            }
        }

        public void checkPorts()
        {
            while (true)
            {
                Thread.Sleep(1000);

                SerialPort serialPort1 = new SerialPort();
                string[] portList = SerialPort.GetPortNames();

                int oldPorts = comboBoxPorts.Items.Count;
                int newPorts = portList.Length;

                //Console.WriteLine("Ports alt " + oldPorts + " == " + newPorts);

                if (oldPorts != newPorts)
                {
                    //Console.WriteLine("neue Ports ");
                    comboBoxPorts.Dispatcher.Invoke(() => {
                        comboBoxPorts.ItemsSource = portList;
                    });
                }
               
            }
        }
    }
}
