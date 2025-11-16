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
    public class Thread_Con_Rollenzentrierung
    {
        private DataModel dataModel;
        private ComboBox comboBoxPorts;

        public delegate void AddNewPorts(string[] portList);

        public Thread_Con_Rollenzentrierung(DataModel dataModel) 
        {
            this.dataModel = dataModel;
        }

        public void checkVerbindung_Rollenzentrierung()
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
    }
}
