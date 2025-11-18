using SchneidMaschine.model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
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

        private Dictionary<string, string> GetPortDescriptions()
        {
            var portDescriptions = new Dictionary<string, string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string caption = obj["Caption"]?.ToString();
                        if (string.IsNullOrEmpty(caption))
                            continue;

                        // Extrahiere COM-Port und Beschreibung aus "USB Serial Device (COM3)"
                        var match = Regex.Match(caption, @"^(.+?)\s*\((COM\d+)\)$");
                        if (match.Success)
                        {
                            string description = match.Groups[1].Value.Trim();
                            string port = match.Groups[2].Value;
                            portDescriptions[port] = description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WMI-Fehler beim Abrufen der Port-Beschreibungen: " + ex.Message);
            }

            return portDescriptions;
        }

        public void checkPorts()
        {
            while (true)
            {
                Thread.Sleep(1000);

                string[] portList = SerialPort.GetPortNames();
                int oldPorts = comboBoxPorts_Rollenzentrierung.Items.Count;
                int newPorts = portList.Length;

                if (oldPorts != newPorts)
                {
                    // Hole Port-Beschreibungen via WMI
                    var descriptions = GetPortDescriptions();
                    var enhancedPortList = new List<string>();

                    foreach (string port in portList)
                    {
                        if (descriptions.ContainsKey(port))
                        {
                            enhancedPortList.Add($"{port} ({descriptions[port]})");
                        }
                        else
                        {
                            enhancedPortList.Add(port);
                        }
                    }

                    comboBoxPorts_Rollenzentrierung.Dispatcher.Invoke(() =>
                    {
                        comboBoxPorts_Rollenzentrierung.ItemsSource = enhancedPortList;
                    });

                    comboBoxPorts_Schneidmaschine.Dispatcher.Invoke(() =>
                    {
                        comboBoxPorts_Schneidmaschine.ItemsSource = enhancedPortList;
                    });
                }
            }
        }
    }
}
