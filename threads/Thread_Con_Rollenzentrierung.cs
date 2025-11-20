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
        private volatile bool _shouldStop = false;

        public delegate void AddNewPorts(string[] portList);

        public Thread_Con_Rollenzentrierung(DataModel dataModel)
        {
            this.dataModel = dataModel;
        }

        public void Stop()
        {
            _shouldStop = true;
        }

        public void checkVerbindung_Rollenzentrierung()
        {
            while (!_shouldStop)
            {
                Thread.Sleep(1000);

                try
                {
                    // Prüfe ob MainWindow und Dispatcher noch verfügbar sind
                    if (dataModel.MainWindow != null && !dataModel.MainWindow.Dispatcher.HasShutdownStarted)
                    {
                        dataModel.MainWindow.Dispatcher.Invoke(() => {
                            dataModel.MainWindow.checkConnection();
                        });
                    }
                    else
                    {
                        // MainWindow wurde geschlossen, beende Thread
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    // Dispatcher wurde bereits heruntergefahren, beende Thread sauber
                    Console.WriteLine("Rollenzentrierung Thread: Dispatcher shutdown erkannt");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Rollenzentrierung Thread Fehler: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine("Rollenzentrierung Thread beendet");
        }
    }
}
