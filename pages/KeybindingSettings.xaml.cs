using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SchneidMaschine.model;

namespace SchneidMaschine.pages
{
    public partial class KeybindingSettings : Page
    {
        private KeybindingManager keybindingManager;
        private DataModel dataModel;

        public KeybindingSettings(DataModel dataModel)
        {
            InitializeComponent();
            this.dataModel = dataModel;
            this.keybindingManager = dataModel.KeybindingManager;

            LoadKeybindings();
        }

        private void LoadKeybindings()
        {
            var bindings = keybindingManager.GetAllBindings();

            // EinzelSchritt
            var einzelSchrittBindings = bindings
                .Where(kvp => kvp.Key.StartsWith("EinzelSchritt_"))
                .Select(kvp => new KeybindingItem
                {
                    Action = kvp.Key,
                    Key = kvp.Value,
                    KeyName = keybindingManager.GetKeyDisplayName(kvp.Value),
                    DisplayName = GetDisplayName(kvp.Key)
                })
                .ToList();
            DataGridEinzelSchritt.ItemsSource = einzelSchrittBindings;

            // HalbAuto
            var halbAutoBindings = bindings
                .Where(kvp => kvp.Key.StartsWith("HalbAuto_"))
                .Select(kvp => new KeybindingItem
                {
                    Action = kvp.Key,
                    Key = kvp.Value,
                    KeyName = keybindingManager.GetKeyDisplayName(kvp.Value),
                    DisplayName = GetDisplayName(kvp.Key)
                })
                .ToList();
            DataGridHalbAuto.ItemsSource = halbAutoBindings;

            // Auto
            var autoBindings = bindings
                .Where(kvp => kvp.Key.StartsWith("Auto_"))
                .Select(kvp => new KeybindingItem
                {
                    Action = kvp.Key,
                    Key = kvp.Value,
                    KeyName = keybindingManager.GetKeyDisplayName(kvp.Value),
                    DisplayName = GetDisplayName(kvp.Key)
                })
                .ToList();
            DataGridAuto.ItemsSource = autoBindings;
        }

        private string GetDisplayName(string action)
        {
            // Konvertiere "EinzelSchritt_1mm" zu "1mm"
            var parts = action.Split('_');
            if (parts.Length >= 2)
            {
                string name = parts[1];

                // Übersetze einige Namen
                switch (name)
                {
                    case "1mm": return "1mm vorwärts";
                    case "10mm": return "10mm vorwärts";
                    case "100mm": return "100mm vorwärts";
                    case "Sollwert": return "Sollwert abfahren";
                    case "Schneiden": return "Schneiden";
                    case "Kopfschnitt": return "Kopfschnitt";
                    case "Handrad": return "Handrad an/aus";
                    case "Stop": return "Stop";
                    case "Start": return "Start";
                    case "Pause": return "Pause";
                    case "DurchlaufStart": return "Durchlauf starten";
                    case "DurchlaufStop": return "Durchlauf stoppen";
                    default: return name;
                }
            }
            return action;
        }

        private void BtnChangeKey_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string action = button.Tag.ToString();

            // Öffne Dialog zur Tasteneingabe
            var dialog = new KeyInputDialog();
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                keybindingManager.SetKey(action, dialog.SelectedKey);
                LoadKeybindings(); // Refresh
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            keybindingManager.SaveSettings();
            dataModel.UpdateButtonTexts(); // Button-Texte aktualisieren
            MessageBox.Show("Tastenbelegung gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Möchten Sie die Tastenbelegung auf die Standardwerte zurücksetzen?",
                "Zurücksetzen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                keybindingManager.ResetToDefaults();
                LoadKeybindings();
                MessageBox.Show("Tastenbelegung zurückgesetzt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Schließe das Window, in dem diese Page angezeigt wird
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }
    }

    public class KeybindingItem
    {
        public string Action { get; set; }
        public Key Key { get; set; }
        public string KeyName { get; set; }
        public string DisplayName { get; set; }
    }
}
