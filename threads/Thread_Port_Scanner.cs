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

        public void InitializePortList()
        {
            // Initiale Befüllung der ComboBoxen mit Geräte-Manager-Beschreibungen
            string[] portList = SerialPort.GetPortNames();
            UpdatePortList(portList);
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
                    UpdatePortList(portList);
                }
            }
        }

        public void UpdatePortIdentities()
        {
            // Diese Methode wird aufgerufen, wenn ein Board identifiziert wurde
            string[] portList = SerialPort.GetPortNames();
            UpdatePortList(portList);
        }

        private void UpdatePortList(string[] portList)
        {
            // Hole Port-Beschreibungen via WMI
            var descriptions = GetPortDescriptions();
            var enhancedPortList = new List<string>();

            foreach (string port in portList)
            {
                string displayText = port;

                // Prüfe ob wir eine Identität für diesen Port haben
                if (MainWindow.portIdentities.ContainsKey(port))
                {
                    // Board wurde identifiziert - zeige Board-Namen in Klammern
                    // Entferne "WHOAMI_" Präfix für bessere Lesbarkeit
                    string boardName = MainWindow.portIdentities[port].Replace("WHOAMI_", "");
                    displayText = $"{port} ({boardName})";
                }
                else if (descriptions.ContainsKey(port))
                {
                    // Noch nicht identifiziert - zeige Geräte-Manager-Namen in Klammern
                    displayText = $"{port} ({descriptions[port]})";
                }

                enhancedPortList.Add(displayText);
            }

            comboBoxPorts_Rollenzentrierung.Dispatcher.Invoke(() =>
            {
                // Speichere aktuelle Auswahl
                string currentSelection = comboBoxPorts_Rollenzentrierung.Text;

                comboBoxPorts_Rollenzentrierung.ItemsSource = enhancedPortList;

                // Stelle Auswahl wieder her (suche nach Eintrag, der mit dem Port beginnt)
                if (!string.IsNullOrEmpty(currentSelection))
                {
                    string portName = ExtractPortNameFromText(currentSelection);
                    foreach (string item in enhancedPortList)
                    {
                        if (item.StartsWith(portName))
                        {
                            comboBoxPorts_Rollenzentrierung.Text = item;
                            break;
                        }
                    }
                }
            });

            comboBoxPorts_Schneidmaschine.Dispatcher.Invoke(() =>
            {
                // Speichere aktuelle Auswahl
                string currentSelection = comboBoxPorts_Schneidmaschine.Text;

                comboBoxPorts_Schneidmaschine.ItemsSource = enhancedPortList;

                // Stelle Auswahl wieder her (suche nach Eintrag, der mit dem Port beginnt)
                if (!string.IsNullOrEmpty(currentSelection))
                {
                    string portName = ExtractPortNameFromText(currentSelection);
                    foreach (string item in enhancedPortList)
                    {
                        if (item.StartsWith(portName))
                        {
                            comboBoxPorts_Schneidmaschine.Text = item;
                            break;
                        }
                    }
                }
            });
        }

        private string ExtractPortNameFromText(string text)
        {
            // Extrahiere "COM3" aus "COM3 - Rollenzentrierung" oder "COM3 (USB Serial Device)"
            if (string.IsNullOrEmpty(text))
                return text;

            int spaceIndex = text.IndexOf(' ');
            if (spaceIndex > 0)
            {
                return text.Substring(0, spaceIndex);
            }
            return text;
        }
    }
}
