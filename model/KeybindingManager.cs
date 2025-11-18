using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace SchneidMaschine.model
{
    [Serializable]
    public class KeybindingData
    {
        public string Action { get; set; }
        public Key Key { get; set; }
    }

    public class KeybindingManager
    {
        private Dictionary<string, Key> keybindings;
        private string settingsFilePath;

        public KeybindingManager()
        {
            keybindings = new Dictionary<string, Key>();
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SchneidMaschine"
            );
            settingsFilePath = Path.Combine(appDataFolder, "keybindings.xml");

            InitializeDefaults();
            LoadSettings();
        }

        private void InitializeDefaults()
        {
            // EinzelSchritt
            keybindings["EinzelSchritt_1mm"] = Key.F1;
            keybindings["EinzelSchritt_10mm"] = Key.F2;
            keybindings["EinzelSchritt_100mm"] = Key.F3;
            keybindings["EinzelSchritt_Sollwert"] = Key.F4;
            keybindings["EinzelSchritt_Schneiden"] = Key.F5;
            keybindings["EinzelSchritt_Kopfschnitt"] = Key.F6;
            keybindings["EinzelSchritt_Handrad"] = Key.F7;
            keybindings["EinzelSchritt_Stop"] = Key.Escape;

            // HalbAuto
            keybindings["HalbAuto_Start"] = Key.F1;
            keybindings["HalbAuto_Schneiden"] = Key.F2;
            keybindings["HalbAuto_Stop"] = Key.Escape;

            // Auto
            keybindings["Auto_Start"] = Key.F1;
            keybindings["Auto_Pause"] = Key.F2;
            keybindings["Auto_DurchlaufStart"] = Key.F3;
            keybindings["Auto_DurchlaufStop"] = Key.F4;
            keybindings["Auto_Stop"] = Key.Escape;
        }

        public Key GetKey(string action)
        {
            return keybindings.ContainsKey(action) ? keybindings[action] : Key.None;
        }

        public void SetKey(string action, Key key)
        {
            keybindings[action] = key;
        }

        public Dictionary<string, Key> GetAllBindings()
        {
            return new Dictionary<string, Key>(keybindings);
        }

        public void ResetToDefaults()
        {
            keybindings.Clear();
            InitializeDefaults();
        }

        public void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));

                var dataList = new List<KeybindingData>();
                foreach (var kvp in keybindings)
                {
                    dataList.Add(new KeybindingData { Action = kvp.Key, Key = kvp.Value });
                }

                XmlSerializer serializer = new XmlSerializer(typeof(List<KeybindingData>));
                using (StreamWriter writer = new StreamWriter(settingsFilePath))
                {
                    serializer.Serialize(writer, dataList);
                }

                Console.WriteLine("Keybindings gespeichert in: " + settingsFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Speichern der Keybindings: " + ex.Message);
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KeybindingData>));
                    using (StreamReader reader = new StreamReader(settingsFilePath))
                    {
                        var dataList = (List<KeybindingData>)serializer.Deserialize(reader);
                        foreach (var data in dataList)
                        {
                            keybindings[data.Action] = data.Key;
                        }
                    }

                    Console.WriteLine("Keybindings geladen von: " + settingsFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Laden der Keybindings: " + ex.Message);
            }
        }

        public string GetKeyDisplayName(Key key)
        {
            if (key == Key.None)
                return "-";
            return key.ToString();
        }

        public string GetActionDisplayName(string action)
        {
            // Konvertiere "EinzelSchritt_1mm" zu "1mm"
            if (action.Contains("_"))
            {
                var parts = action.Split('_');
                if (parts.Length >= 2)
                {
                    return parts[1];
                }
            }
            return action;
        }
    }
}
