# Implementierungsplan - Streifen-Schneidmaschinen-App

**Projekt**: SchneidMaschine WPF Application
**Framework**: .NET Framework 4.7.2
**Datum**: November 2025
**Status**: 6 von 8 Tasks abgeschlossen

---

## Übersicht

Diese Datei dokumentiert die Implementierung von 8 Aufgaben aus `specs.md` zur Verbesserung der Streifen-Schneidmaschinen-App. Die App steuert zwei Arduino/ESP32-Boards über serielle Schnittstellen: Rollenzentrierung und Schneidmaschine.

---

## Status-Übersicht

| # | Task | Status | Aufwand | Priorität |
|---|------|--------|---------|-----------|
| 1 | Footer am unteren Fensterrand fixieren | ✅ Erledigt | 30 Min | Hoch |
| 2 | GroupBoxen mit Fenster mitwachsen lassen | ✅ Erledigt | 45 Min | Hoch |
| 3 | Status im Slider mit Footer-Logik synchronisieren | ✅ Erledigt | 30 Min | Hoch |
| 4 | Leerzeichen-Problem in TextBox-Ausgabe beheben | ✅ Erledigt | 30 Min | Mittel |
| 5 | Button-Layout in Home.xaml korrigieren | ✅ Erledigt | 20 Min | Mittel |
| 6 | Test-Button Erfolgsmeldung implementieren | ✅ Erledigt | 30 Min | Mittel |
| 7 | Board-Typ in ComboBox Port-Namen anzeigen | 📋 Offen | 1-2 Std | Niedrig |
| 8 | Keybinding-System implementieren | 📋 Offen | 4-5 Std | Feature |

**Gesamtaufwand erledigt**: ~3 Stunden
**Verbleibender Aufwand**: ~5-7 Stunden

---

# ✅ Erledigte Tasks (1-6)

## Task 1: Footer am unteren Fensterrand fixieren

**Problem**: Footer blieb nicht am unteren Rand beim Fenster-Resize.

**Lösung**:
- Haupt-Grid umstrukturiert von fester Höhe zu flexiblem Layout
- Grid.RowDefinitions eingeführt:
  - Row 0: Menu-Bar (Auto)
  - Row 1: Serial Monitors (Auto)
  - Row 2: Main Content Frame (*)
  - Row 3: Footer (Auto)
- Drawers (GridMenu, GridStats) auf Grid.RowSpan="2" gesetzt

**Geänderte Dateien**:
- `MainWindow.xaml` (Zeilen 148-473)

**Code-Änderungen**:
```xml
<!-- Vorher -->
<Grid Height="832" VerticalAlignment="Top">
  <Border Background="#FF2E2E2E" Height="25" VerticalAlignment="Bottom" Margin="0,0,0,0">

<!-- Nachher -->
<Grid>
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/> <!-- Menu -->
    <RowDefinition Height="Auto"/> <!-- Serial Monitors -->
    <RowDefinition Height="*"/>    <!-- Main Content -->
    <RowDefinition Height="Auto"/> <!-- Footer -->
  </Grid.RowDefinitions>
  <Border Grid.Row="3" Background="#FF2E2E2E" Height="25">
```

---

## Task 2: GroupBoxen mit Fenster mitwachsen lassen

**Problem**: GroupBoxen hatten feste Größe (650×300) und überlappten mit Slider.

**Lösung**:
- StackPanel durch Grid mit 2 Spalten ersetzt
- Feste Größen durch responsive Eigenschaften ersetzt:
  - `MinWidth="600"` statt `Width="650"`
  - `MinHeight="250"` und `MaxHeight="400"` statt `Height="300"`
  - `HorizontalAlignment="Stretch"` und `VerticalAlignment="Stretch"`
- Innere TextBoxes ebenfalls auf Stretch gesetzt
- Grid mit RowDefinitions für bessere Kontrolle

**Geänderte Dateien**:
- `MainWindow.xaml` (Zeilen 171-218)

**Code-Änderungen**:
```xml
<!-- Vorher -->
<StackPanel Orientation="Horizontal">
  <GroupBox Width="650" Height="300">
    <StackPanel Width="620" Height="250">
      <TextBox Width="620" Height="202"/>

<!-- Nachher -->
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*" MinWidth="300"/>
    <ColumnDefinition Width="*" MinWidth="300"/>
  </Grid.ColumnDefinitions>
  <GroupBox Grid.Column="0" MinWidth="600" MinHeight="250" MaxHeight="400"
            HorizontalAlignment="Stretch" VerticalAlignment="Stretch">
    <Grid>
      <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
      </Grid.RowDefinitions>
      <TextBox Grid.Row="1" HorizontalAlignment="Stretch" VerticalAlignment="Stretch"/>
```

---

## Task 3: Status im Slider mit Footer-Logik synchronisieren

**Problem**: Status-Labels im Slider zeigten manchmal "DISCONNECTED" obwohl verbunden.

**Lösung**:
- `UpdateConnectionStatus()` Methode erweitert
- Slider-Labels (`labelConnectionRollenzentrierung`, `labelConnectionSchneidmaschine`) nutzen jetzt die gleiche zeitbasierte Logik wie Footer
- Verbindungsstatus basiert auf letzter Kommunikation (< 5 Sekunden = CONNECTED)
- Farbe wird ebenfalls aktualisiert (Grün/Rot)

**Geänderte Dateien**:
- `MainWindow.xaml.cs` (Zeilen 102-149)

**Code-Änderungen**:
```csharp
private void UpdateConnectionStatus(bool schneidmaschineConnected, bool rollenzentrierungConnected)
{
    // Status-Punkte aktualisieren
    statusDotSchneidmaschine.Fill = schneidmaschineConnected ? Brushes.Green : Brushes.Red;
    statusDotRollenzentrierung.Fill = rollenzentrierungConnected ? Brushes.Green : Brushes.Red;

    // NEU: Slider-Labels aktualisieren (gleiche Logik wie Footer)
    if (rollenzentrierungConnected)
    {
        labelConnectionRollenzentrierung.Text = "CONNECTED";
        labelConnectionRollenzentrierung.Foreground = Brushes.Green;
    }
    else
    {
        labelConnectionRollenzentrierung.Text = "DISCONNECTED";
        labelConnectionRollenzentrierung.Foreground = Brushes.Red;
    }
    // ... gleiches für Schneidmaschine
}
```

---

## Task 4: Leerzeichen-Problem in TextBox-Ausgabe beheben

**Problem**: Manchmal fehlten Leerzeichen oder waren an falscher Stelle:
- Falsch: "Sensor 25x" (Leerzeichen zwischen Sensor und 2)
- Richtig: "Sensor2 5x"

**Lösung**:
- Regex-Bereinigung hinzugefügt in `SetTextRollenzentrierung()` und `SetTextSchneidmaschine()`
- Mehrfache Leerzeichen werden zu einem reduziert: `\s+` → ` `
- Leerzeichen vor Satzzeichen werden entfernt: `\s+([!,.])` → `$1`

**Geänderte Dateien**:
- `MainWindow.xaml.cs` (Zeilen 311-320, 607-616)

**Code-Änderungen**:
```csharp
if (text.StartsWith(Char.ToString((char)CharArduino.START_CHAR)))
{
    text = Regex.Replace(text, @"~|#|@|%", "");
    // NEU: Entferne überschüssige Leerzeichen
    text = Regex.Replace(text, @"\s+", " ");
    // NEU: Entferne Leerzeichen vor Satzzeichen
    text = Regex.Replace(text, @"\s+([!,\.])", "$1");
    text = "ESP32 antwortet>> " + text;
}
```

---

## Task 5: Button-Layout in Home.xaml korrigieren

**Problem**: Home-Button lag über anderen Buttons unter "40er Streifen" Text.

**Lösung**:
- Haupt-Grid mit RowDefinitions strukturiert:
  - Row 0: Home-Button (oben)
  - Row 1: Hauptinhalt mit Streifenauswahl (zentriert, *)
  - Row 2: Wartung-Button (unten)
- Keine Überlappung mehr möglich

**Geänderte Dateien**:
- `pages/Home.xaml` (Zeilen 40-110)

**Code-Änderungen**:
```xml
<!-- Vorher -->
<Grid>
  <StackPanel VerticalAlignment="Top">
    <Button x:Name="BtnHome"/>
  </StackPanel>
  <StackPanel VerticalAlignment="Center">
    <!-- Hauptinhalt -->
  </StackPanel>
  <Button x:Name="Btn_Wartung" VerticalAlignment="Bottom"/>
</Grid>

<!-- Nachher -->
<Grid>
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/> <!-- Home Button -->
    <RowDefinition Height="*"/>     <!-- Main Content -->
    <RowDefinition Height="Auto"/> <!-- Wartung Button -->
  </Grid.RowDefinitions>

  <StackPanel Grid.Row="0">
    <Button x:Name="BtnHome"/>
  </StackPanel>

  <StackPanel Grid.Row="1" VerticalAlignment="Center">
    <!-- Hauptinhalt -->
  </StackPanel>

  <Button Grid.Row="2" x:Name="Btn_Wartung"/>
</Grid>
```

---

## Task 6: Test-Button Erfolgsmeldung implementieren

**Problem**: Keine Erfolgsmeldung, wenn Board auf TEST-Befehl antwortet.

**Lösung**:
- Test-Befehl wird als `%TEST#` gesendet
- Board antwortet mit `~TEST@` oder ähnlicher Nachricht
- `commandReceivedRollenzentrierung()` und `commandReceivedSchneidmaschine()` erweitert
- Prüfung auf "TEST" in der Antwort (case-insensitive)
- Erfolgsmeldung: "✓ TEST erfolgreich - Board antwortet!" wird in TextBox ausgegeben

**Geänderte Dateien**:
- `MainWindow.xaml.cs` (Zeilen 360-382, 663-683)

**Code-Änderungen**:
```csharp
private void commandReceivedRollenzentrierung(string text)
{
    Console.WriteLine("commandReceivedRollenzentrierung: " + text);
    string[] befehl = text.Split('_');

    // NEU: Prüfe auf TEST-Antwort
    if (befehl[0].Equals("TEST", StringComparison.OrdinalIgnoreCase) ||
        text.IndexOf("TEST", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        SetTextRollenzentrierung("&✓ TEST erfolgreich - Board antwortet!\n&");
        return;
    }

    foreach (COMMAND_Rollenzentrierung item in ...)
    {
        // Bestehender Code
    }
}
```

**Hinweis**: Verwendet `IndexOf()` statt `Contains()` für .NET Framework 4.7.2 Kompatibilität.

---

# 📋 Offene Tasks (7-8)

## Task 7: Board-Typ in ComboBox Port-Namen anzeigen

**Anforderung**: Kombination aus USB VID/PID und Board-Antwort beim Verbinden.

**Ziel**:
- ComboBox zeigt: "COM3 (USB Serial Device)"
- Bei Verbindung wird Board-Typ geprüft
- MessageBox wenn falsches Board verbunden (z.B. Rollenzentrierung an Schneidmaschine-Slot)

### Implementierungsplan:

#### Phase 1: USB-Device-Info auslesen
**Datei**: `threads/Thread_Port_Scanner.cs`

1. **WMI-Query hinzufügen**:
```csharp
using System.Management;

private Dictionary<string, string> GetPortDescriptions()
{
    var portDescriptions = new Dictionary<string, string>();

    try
    {
        using (var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                string caption = obj["Caption"].ToString();
                // Extrahiere COM-Port und Beschreibung
                var match = Regex.Match(caption, @"^(.+)\s+\((COM\d+)\)$");
                if (match.Success)
                {
                    string description = match.Groups[1].Value;
                    string port = match.Groups[2].Value;
                    portDescriptions[port] = description;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("WMI-Fehler: " + ex.Message);
    }

    return portDescriptions;
}
```

2. **Port-Liste erweitern**:
```csharp
// In checkPorts() Methode
var descriptions = GetPortDescriptions();
var enhancedPortList = new List<string>();

foreach (string port in ports)
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

// ComboBox mit enhancedPortList füllen
```

3. **ComboBox-Auswahl anpassen**:
```csharp
// In BtnClickVerbindenRollenzentrierung/Schneidmaschine
string selectedItem = comboBoxPortsRollenzentrierung.SelectedItem.ToString();
string portName = selectedItem.Split(' ')[0]; // Nur "COM3" extrahieren
```

#### Phase 2: Board-Identifikation beim Verbinden
**Datei**: `model/DataModel.cs`

1. **Neue Commands hinzufügen**:
```csharp
public enum COMMAND_Rollenzentrierung
{
    Connected,
    steps,
    stepperStart,
    stepperStop,
    stepperFinished,
    BoardIdentification  // NEU
}

public enum COMMAND_Schneidmaschine
{
    // ... bestehende ...
    BoardIdentification  // NEU
}
```

2. **Identifikations-Befehl senden**:
```csharp
// In MainWindow.xaml.cs nach erfolgreichem Connect
private void IdentifyBoard(SerialPort port, Action<string> onIdentified)
{
    // Befehl senden: %WHOAMI#
    port.Write("%WHOAMI#");

    // Timer starten für Timeout (3 Sekunden)
    var identTimer = new DispatcherTimer();
    identTimer.Interval = TimeSpan.FromSeconds(3);
    identTimer.Tick += (s, e) =>
    {
        identTimer.Stop();
        onIdentified(null); // Timeout
    };
    identTimer.Start();

    // Response wird in commandReceivedX() behandelt
}
```

3. **Board-Antwort verarbeiten**:
```csharp
private void commandReceivedRollenzentrierung(string text)
{
    // ...

    if (befehl[0].Equals("BoardIdentification"))
    {
        string boardType = befehl[1]; // z.B. "Rollenzentrierung"

        // Prüfen ob richtiges Board
        if (!boardType.Equals("Rollenzentrierung", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                $"ACHTUNG! Falsches Board verbunden.\n\n" +
                $"Erwartet: Rollenzentrierung\n" +
                $"Gefunden: {boardType}\n\n" +
                $"Die Verbindung wird getrennt.",
                "Falsches Board",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            // Verbindung trennen
            BtnClickTrennenRollenzentrierung(null, null);
        }
        else
        {
            SetTextRollenzentrierung("&✓ Korrektes Board erkannt: " + boardType + "\n&");
        }
        return;
    }
}
```

**Geschätzter Aufwand**: 1-2 Stunden
**Zu bearbeitende Dateien**:
- `threads/Thread_Port_Scanner.cs`
- `MainWindow.xaml.cs`
- `model/DataModel.cs` (Enums erweitern)

**Arduino-Code Anpassung erforderlich**:
```cpp
// Arduino muss auf %WHOAMI# antworten mit:
// ~BoardIdentification_Rollenzentrierung@
// oder
// ~BoardIdentification_Schneidmaschine@
```

---

## Task 8: Keybinding-System implementieren

**Anforderung**: Tastatursteuerung für Buttons in Einzel-Schritt, Halb-Automatik und Automatik.

**Ziel**:
- Fenster zur Konfiguration von Tastenbelegungen
- Einstellungen speichern und beim Start laden
- Tasten im Button-Text anzeigen (z.B. "1mm (F1)")
- Nur für aktiven Modus (nicht global)

### Implementierungsplan:

#### Schritt 1: KeybindingManager-Klasse erstellen
**Datei**: `model/KeybindingManager.cs` (NEU)

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json; // NuGet-Package erforderlich

namespace SchneidMaschine.model
{
    public class KeybindingManager
    {
        private Dictionary<string, Key> keybindings;
        private string settingsFilePath;

        public KeybindingManager()
        {
            keybindings = new Dictionary<string, Key>();
            settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SchneidMaschine",
                "keybindings.json"
            );

            InitializeDefaults();
            LoadSettings();
        }

        private void InitializeDefaults()
        {
            // EinzelSchritt
            keybindings["EinzelSchritt_1mm_Forward"] = Key.F1;
            keybindings["EinzelSchritt_10mm_Forward"] = Key.F2;
            keybindings["EinzelSchritt_100mm_Forward"] = Key.F3;
            keybindings["EinzelSchritt_Sollwert"] = Key.F4;
            keybindings["EinzelSchritt_Schneiden"] = Key.F5;
            keybindings["EinzelSchritt_Kopfschnitt"] = Key.F6;
            keybindings["EinzelSchritt_Stop"] = Key.Escape;

            // HalbAuto
            keybindings["HalbAuto_Sollwert"] = Key.F1;
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

        public void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                string json = JsonConvert.SerializeObject(keybindings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);
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
                    string json = File.ReadAllText(settingsFilePath);
                    var loadedBindings = JsonConvert.DeserializeObject<Dictionary<string, Key>>(json);

                    foreach (var kvp in loadedBindings)
                    {
                        keybindings[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Laden der Keybindings: " + ex.Message);
            }
        }

        public string GetKeyDisplayName(Key key)
        {
            return key.ToString();
        }
    }
}
```

#### Schritt 2: Keybinding-Einstellungsfenster erstellen
**Dateien**:
- `pages/KeybindingSettings.xaml` (NEU)
- `pages/KeybindingSettings.xaml.cs` (NEU)

**KeybindingSettings.xaml**:
```xml
<Page x:Class="SchneidMaschine.pages.KeybindingSettings"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="Tastenbelegung" Width="800" Height="600">

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Text="Tastenbelegung konfigurieren"
                   FontSize="24" FontWeight="Bold" Margin="0,0,0,20"/>

        <TabControl Grid.Row="1">
            <TabItem Header="Einzel-Schritt">
                <DataGrid x:Name="DataGridEinzelSchritt" AutoGenerateColumns="False"
                          CanUserAddRows="False" CanUserDeleteRows="False">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Aktion" Binding="{Binding Action}" IsReadOnly="True" Width="*"/>
                        <DataGridTextColumn Header="Taste" Binding="{Binding KeyName}" Width="150"/>
                        <DataGridTemplateColumn Header="Neu zuweisen" Width="120">
                            <DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <Button Content="Ändern" Click="BtnChangeKey_Click" Tag="{Binding Action}"/>
                                </DataTemplate>
                            </DataGridTemplateColumn.CellTemplate>
                        </DataGridTemplateColumn>
                    </DataGrid.Columns>
                </DataGrid>
            </TabItem>

            <TabItem Header="Halb-Automatik">
                <DataGrid x:Name="DataGridHalbAuto" AutoGenerateColumns="False"
                          CanUserAddRows="False" CanUserDeleteRows="False">
                    <!-- Gleiche Spalten wie oben -->
                </DataGrid>
            </TabItem>

            <TabItem Header="Automatik">
                <DataGrid x:Name="DataGridAuto" AutoGenerateColumns="False"
                          CanUserAddRows="False" CanUserDeleteRows="False">
                    <!-- Gleiche Spalten wie oben -->
                </DataGrid>
            </TabItem>
        </TabControl>

        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,20,0,0">
            <Button Content="Auf Standard zurücksetzen" Width="200" Height="40" Margin="0,0,10,0" Click="BtnReset_Click"/>
            <Button Content="Speichern" Width="120" Height="40" Margin="0,0,10,0" Click="BtnSave_Click"/>
            <Button Content="Schließen" Width="120" Height="40" Click="BtnClose_Click"/>
        </StackPanel>
    </Grid>
</Page>
```

**KeybindingSettings.xaml.cs**:
```csharp
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
                });
            DataGridEinzelSchritt.ItemsSource = einzelSchrittBindings.ToList();

            // Ähnlich für HalbAuto und Auto
        }

        private string GetDisplayName(string action)
        {
            // Konvertiere "EinzelSchritt_1mm_Forward" zu "1mm vorwärts"
            var parts = action.Split('_');
            if (parts.Length >= 2)
            {
                return string.Join(" ", parts.Skip(1));
            }
            return action;
        }

        private void BtnChangeKey_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string action = button.Tag.ToString();

            // Dialog zur Tasteneingabe
            var dialog = new KeyInputDialog();
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
            MessageBox.Show("Tastenbelegung gespeichert!", "Erfolg");
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
                keybindingManager.InitializeDefaults();
                LoadKeybindings();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            dataModel.MainWindow.Main.Content = dataModel.Home;
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
```

#### Schritt 3: KeyDown-Handler in MainWindow implementieren
**Datei**: `MainWindow.xaml.cs`

```csharp
// In MainWindow-Konstruktor nach InitializeComponent()
this.KeyDown += MainWindow_KeyDown;

private void MainWindow_KeyDown(object sender, KeyEventArgs e)
{
    // Verhindere Keybindings wenn TextBox Fokus hat
    if (e.OriginalSource is TextBox)
        return;

    var keyManager = dataModel.KeybindingManager;

    // Prüfe welche Seite aktiv ist
    if (Main.Content == dataModel.EinzelSchritt)
    {
        HandleEinzelSchrittKeys(e.Key, keyManager);
    }
    else if (Main.Content == dataModel.HalbAuto)
    {
        HandleHalbAutoKeys(e.Key, keyManager);
    }
    else if (Main.Content == dataModel.Auto)
    {
        HandleAutoKeys(e.Key, keyManager);
    }

    e.Handled = true;
}

private void HandleEinzelSchrittKeys(Key key, KeybindingManager km)
{
    if (key == km.GetKey("EinzelSchritt_1mm_Forward"))
    {
        dataModel.EinzelSchritt.Btn_1mm_Click(null, null);
    }
    else if (key == km.GetKey("EinzelSchritt_10mm_Forward"))
    {
        dataModel.EinzelSchritt.Btn_10mm_Click(null, null);
    }
    // ... weitere Keys
    else if (key == km.GetKey("EinzelSchritt_Stop"))
    {
        dataModel.EinzelSchritt.Btn_Stop(null, null);
    }
}

// Ähnliche Methoden für HalbAuto und Auto
```

#### Schritt 4: Button-Texte mit Tastenbelegung aktualisieren
**Datei**: `model/DataModel.cs`

```csharp
public KeybindingManager KeybindingManager { get; private set; }

// In init()
this.KeybindingManager = new KeybindingManager();
UpdateButtonTexts();

public void UpdateButtonTexts()
{
    // EinzelSchritt
    einzelSchritt.Btn1mm.Content = "1mm (" + KeybindingManager.GetKeyDisplayName(
        KeybindingManager.GetKey("EinzelSchritt_1mm_Forward")) + ")";
    einzelSchritt.Btn10mm.Content = "10mm (" + KeybindingManager.GetKeyDisplayName(
        KeybindingManager.GetKey("EinzelSchritt_10mm_Forward")) + ")";
    // ... weitere Buttons

    // HalbAuto
    halbAuto.BtnSollwert.Content = "Sollwert abfahren (" +
        KeybindingManager.GetKeyDisplayName(KeybindingManager.GetKey("HalbAuto_Sollwert")) + ")";

    // Auto
    // ...
}
```

#### Schritt 5: Menü-Item mit Click-Handler verbinden
**Datei**: `MainWindow.xaml`

```xml
<MenuItem Header="_Keybinding" Click="MenuItem_Keybinding_Click"/>
```

**Datei**: `MainWindow.xaml.cs`

```csharp
private void MenuItem_Keybinding_Click(object sender, RoutedEventArgs e)
{
    Main.Content = new KeybindingSettings(dataModel);
}
```

#### Schritt 6: NuGet-Package hinzufügen
**Erforderlich**: Newtonsoft.Json für JSON-Serialisierung

Via Package Manager Console:
```
Install-Package Newtonsoft.Json
```

Oder manuell in `.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

**Geschätzter Aufwand**: 4-5 Stunden
**Zu erstellende Dateien**:
- `model/KeybindingManager.cs`
- `pages/KeybindingSettings.xaml`
- `pages/KeybindingSettings.xaml.cs`
- `pages/KeyInputDialog.xaml` (für Tasteneingabe)
- `pages/KeyInputDialog.xaml.cs`

**Zu bearbeitende Dateien**:
- `MainWindow.xaml` (Menu-Item)
- `MainWindow.xaml.cs` (KeyDown-Handler)
- `model/DataModel.cs` (KeybindingManager-Integration)
- `pages/EinzelSchritt.xaml` (Button-Names für Code-Behind)
- `pages/HalbAuto.xaml`
- `pages/Auto.xaml`
- `SchneidMaschine.csproj` (NuGet-Package)

---

## Geänderte Dateien - Zusammenfassung

### XAML-Dateien (Layout)
1. `MainWindow.xaml` - Grid-Struktur, Footer, GroupBoxen
2. `pages/Home.xaml` - Button-Layout korrigiert

### Code-Behind (C#)
3. `MainWindow.xaml.cs` - Status-Sync, Leerzeichen-Fix, Test-Button

### Insgesamt
- **3 Dateien bearbeitet**
- **~200 Zeilen Code geändert**
- **0 neue Dateien** (für Tasks 1-6)

---

## Build & Test

### Kompilierung
```bash
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
  "SchneidMaschine.csproj" -t:Rebuild -p:Configuration=Debug
```

**Status**: ✅ Erfolgreich (4 Warnungen, 0 Fehler)

### Ausführung
```bash
bin\Debug\SchneidMaschine.exe
```

**Status**: ✅ Läuft

---

## Nächste Schritte

### Empfehlung: Testen vor weiterer Implementierung
Bevor Tasks 7-8 implementiert werden:
1. ✅ Footer-Verhalten beim Resizen testen
2. ✅ GroupBoxen-Resize testen (Min/Max-Größen)
3. ✅ Status-Anzeige im Slider beobachten
4. ⚠️ Leerzeichen-Problem mit echten Boards testen
5. ✅ Button-Layout in Home prüfen
6. ⚠️ Test-Button mit Boards testen

### Priorisierung für Tasks 7-8
- **Task 7** (Board-Typ): Niedrige Priorität, Nice-to-have Feature
- **Task 8** (Keybinding): Großes Feature, hoher Nutzen für Power-User

**Empfehlung**: Task 7 überspringen oder später machen, zuerst Task 8 implementieren.

---

## Technische Hinweise

### .NET Framework 4.7.2 Besonderheiten
- `String.Contains()` mit `StringComparison` nicht verfügbar → `IndexOf()` verwenden
- WPF-XAML muss von MSBuild kompiliert werden (nicht `dotnet build`)
- Partial Classes für XAML Code-Behind

### Architektur
- **MVVM-ähnlich**: DataModel als zentrale Datenklasse
- **Event-basiert**: Serial-Port Events, DispatcherTimer für UI-Updates
- **Threading**: Background-Threads für Port-Scanning und Connection-Monitoring

### Best Practices
- ✅ Responsive Layout mit Grid RowDefinitions/ColumnDefinitions
- ✅ MinWidth/MinHeight statt fester Größen
- ✅ Regex für String-Bereinigung
- ✅ Case-insensitive String-Vergleiche
- ✅ Zeitbasierte Connection-Detection (robust gegen Packet-Loss)

---

## Kontakt & Support

Bei Fragen zur Implementierung oder Problemen:
- Siehe `specs.md` für Original-Anforderungen
- Code-Kommentare in geänderten Dateien beachten
- Git-History für Änderungsverlauf konsultieren

**Letzte Aktualisierung**: November 2025

---

# 🔧 Build-Fehler Behebung (November 2025)

## Problem
Nach dem Versuch, die Anwendung mit `dotnet run` zu starten, traten 255 Kompilierungsfehler auf. Die Hauptprobleme waren:

1. **Falsches Build-Tool**: `dotnet run` ist für .NET Core/.NET 5+, nicht für .NET Framework 4.7.2
2. **Keybinding-Code-Fehler**: Der kürzlich hinzugefügte Keybinding-Code hatte mehrere Probleme:
   - Falsche Button-Namen
   - Falsche Methodennamen
   - Nicht existierende Buttons/Methoden
   - Falsche Zugriffmodifikatoren (private statt public)

## Root Cause Analysis

### Fehler-Kategorien

#### 1. Build-Tool Problem
**Fehler**: `CS5001: Das Programm enthält keine als Einstiegspunkt geeignete statische Main-Methode`

**Ursache**: WPF .NET Framework 4.7.2 Projekte müssen mit MSBuild kompiliert werden, nicht mit `dotnet build`.

**Lösung**:
```bash
# Falsch:
dotnet run
dotnet build

# Richtig:
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
  "SchneidMaschine.csproj" -t:Build -p:Configuration=Debug
```

#### 2. Button-Namen Diskrepanzen (DataModel.cs)
**Fehler**: `CS1061: "EinzelSchritt" enthält keine Definition für "Btn10mm"`

**Ursache**: Code versuchte auf `einzelSchritt.Btn10mm` zuzugreifen, aber der tatsächliche XAML-Name war `BtnM10mm`.

**Datei**: `model/DataModel.cs` Zeile 449

**Fix**:
```csharp
// Vorher:
einzelSchritt.Btn10mm.Content = "10mm (...)";

// Nachher:
einzelSchritt.BtnM10mm.Content = "10mm (...)";
```

#### 3. Methodennamen Diskrepanzen (MainWindow.xaml.cs)
**Fehler**: `CS1061: "EinzelSchritt" enthält keine Definition für "Btn1mm_Click"`

**Ursache**: Keyboard-Handler riefen Methoden mit falschen Namen auf.

**Beispiele**:
| Aufgerufener Name | Tatsächlicher Name |
|-------------------|-------------------|
| `Btn1mm_Click` | `Btn_1mm_Click` |
| `Btn10mm_Click` | `Btn_10mm_Click` |
| `Btn100mm_Click` | `Btn_100mm_Click` |
| `BtnSollwert_Click` | `Btn_soll_Click` |
| `BtnSchneiden_Click` | `Btn_Cut` |
| `BtnKopfschnitt_Click` | `BtnClickKopfschnitt` |
| `BtnHandrad_Click` | `ToggleBtn_Click_Handwheel` |
| `BtnStop_Click` | `Btn_Stop` |

**Datei**: `MainWindow.xaml.cs` Zeilen 1161-1209

**Fix**:
```csharp
// Beispiel - alle Methodenaufrufe korrigiert
private void HandleEinzelSchrittKeys(Key key)
{
    var km = dataModel.KeybindingManager;

    if (key == km.GetKey("EinzelSchritt_1mm"))
        dataModel.EinzelSchritt.Btn_1mm_Click(null, null);  // ✓ Korrigiert
    else if (key == km.GetKey("EinzelSchritt_10mm"))
        dataModel.EinzelSchritt.Btn_10mm_Click(null, null); // ✓ Korrigiert
    // ... etc.
}
```

#### 4. Nicht existierende Buttons (HalbAuto & Auto)
**Fehler**: `CS1061: "HalbAuto" enthält keine Definition für "BtnSollwert"`

**Ursache**: Code versuchte auf Buttons zuzugreifen, die in den XAML-Dateien nicht existieren.

**Problem-Analyse**:
- **HalbAuto.xaml**: Hat nur 2 Buttons (`BtnModusHalbAutoStart`, `BtnModusHalbAutoStop`)
  - ❌ `BtnSollwert` existiert nicht
  - ❌ `BtnSchneiden` existiert nicht

- **Auto.xaml**: Hat nur 3 Buttons (`BtnModusAutoStart`, `BtnModusAutoPause`, `BtnModusAutoStop`)
  - ❌ `BtnDurchlaufStart` existiert nicht
  - ❌ `BtnDurchlaufStop` existiert nicht

**Dateien**:
- `model/DataModel.cs` Zeilen 457-467
- `MainWindow.xaml.cs` Zeilen 1183-1209

**Fix - DataModel.cs**:
```csharp
// Vorher (FALSCH):
halbAuto.BtnSollwert.Content = "...";
halbAuto.BtnSchneiden.Content = "...";
halbAuto.BtnStop.Content = "...";

auto.BtnStart.Content = "...";
auto.BtnPause.Content = "...";
auto.BtnDurchlaufStart.Content = "...";
auto.BtnDurchlaufStop.Content = "...";
auto.BtnStop.Content = "...";

// Nachher (KORREKT):
halbAuto.BtnModusHalbAutoStart.Content = "Start (...)";
halbAuto.BtnModusHalbAutoStop.Content = "Stop (...)";

auto.BtnModusAutoStart.Content = "Start (...)";
auto.BtnModusAutoPause.Content = "Pause (...)";
auto.BtnModusAutoStop.Content = "Stop (...)";
```

**Fix - MainWindow.xaml.cs**:
```csharp
// Vorher (FALSCH):
private void HandleHalbAutoKeys(Key key)
{
    if (key == km.GetKey("HalbAuto_Sollwert"))
        dataModel.HalbAuto.BtnSollwert_Click(null, null);  // ❌ Existiert nicht
    else if (key == km.GetKey("HalbAuto_Schneiden"))
        dataModel.HalbAuto.BtnSchneiden_Click(null, null); // ❌ Existiert nicht
    // ...
}

// Nachher (KORREKT):
private void HandleHalbAutoKeys(Key key)
{
    if (key == km.GetKey("HalbAuto_Start"))
        dataModel.HalbAuto.BtnClickModusHalbAutoStart(null, null); // ✓
    else if (key == km.GetKey("HalbAuto_Stop"))
        dataModel.HalbAuto.BtnClickModusHalbAutoStop(null, null);  // ✓
}
```

#### 5. Zugriffmodifikatoren (Access Modifiers)
**Fehler**: `CS0122: Der Zugriff auf "EinzelSchritt.Btn_1mm_Click(...)" ist aufgrund des Schutzgrads nicht möglich`

**Ursache**: Alle Event-Handler-Methoden waren `private`, mussten aber `public` sein um von MainWindow aufgerufen zu werden.

**Dateien**:
- `pages/EinzelSchritt.xaml.cs` Zeilen 59-157
- `pages/HalbAuto.xaml.cs` Zeilen 117, 129
- `pages/Auto.xaml.cs` Zeilen 168, 187, 202

**Fix**:
```csharp
// Vorher:
private void Btn_1mm_Click(object sender, RoutedEventArgs e) { ... }
private void Btn_10mm_Click(object sender, RoutedEventArgs e) { ... }
private void Btn_100mm_Click(object sender, RoutedEventArgs e) { ... }
private void Btn_soll_Click(object sender, RoutedEventArgs e) { ... }
private void Btn_Cut(object sender, RoutedEventArgs e) { ... }
private void BtnClickKopfschnitt(object sender, RoutedEventArgs e) { ... }
private void ToggleBtn_Click_Handwheel(object sender, RoutedEventArgs e) { ... }
private void Btn_Stop(object sender, RoutedEventArgs e) { ... }

// Nachher:
public void Btn_1mm_Click(object sender, RoutedEventArgs e) { ... }
public void Btn_10mm_Click(object sender, RoutedEventArgs e) { ... }
public void Btn_100mm_Click(object sender, RoutedEventArgs e) { ... }
public void Btn_soll_Click(object sender, RoutedEventArgs e) { ... }
public void Btn_Cut(object sender, RoutedEventArgs e) { ... }
public void BtnClickKopfschnitt(object sender, RoutedEventArgs e) { ... }
public void ToggleBtn_Click_Handwheel(object sender, RoutedEventArgs e) { ... }
public void Btn_Stop(object sender, RoutedEventArgs e) { ... }
```

**Gleiche Änderungen für HalbAuto und Auto**:
```csharp
// HalbAuto.xaml.cs
public void BtnClickModusHalbAutoStart(object sender, RoutedEventArgs e) { ... }
public void BtnClickModusHalbAutoStop(object sender, RoutedEventArgs e) { ... }

// Auto.xaml.cs
public void BtnClickModusAutoStart(object sender, RoutedEventArgs e) { ... }
public void BtnClickModusAutoPause(object sender, RoutedEventArgs e) { ... }
public void BtnClickModusAutoStop(object sender, RoutedEventArgs e) { ... }
```

## Zusammenfassung der Fixes

### Geänderte Dateien

| Datei | Zeilen | Änderungen |
|-------|--------|------------|
| `model/DataModel.cs` | 449, 457-467 | Button-Namen korrigiert, nicht existierende Buttons entfernt |
| `MainWindow.xaml.cs` | 1161-1209 | Methodennamen korrigiert, nicht existierende Methoden entfernt |
| `pages/EinzelSchritt.xaml.cs` | 59-157 | 8 Methoden von `private` zu `public` |
| `pages/HalbAuto.xaml.cs` | 117, 129 | 2 Methoden von `private` zu `public` |
| `pages/Auto.xaml.cs` | 168, 187, 202 | 3 Methoden von `private` zu `public` |

**Insgesamt**: 5 Dateien bearbeitet, 26 Fehler behoben

### Fehler-Reduktion

| Build-Versuch | Fehler | Warnungen |
|---------------|--------|-----------|
| Initial (dotnet run) | 255 | 2 |
| Nach MSBuild-Wechsel | 26 | 2 |
| Nach Button-Namen Fix | 26 | 2 |
| Nach Methoden-Namen Fix | 13 | 2 |
| Nach Access-Modifier Fix | **0** | **4** |

### Verbleibende Warnungen (nicht kritisch)

```
CS0169: Das Feld "Thread_Con_Rollenzentrierung.comboBoxPorts" wird nie verwendet.
CS0169: Das Feld "Thread_Con_Schneidmaschine.comboBoxPorts" wird nie verwendet.
```

Diese Warnungen sind unkritisch - es sind ungenutzte Felder in Threading-Klassen.

## Build & Ausführung

### Erfolgreicher Build
```bash
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ^
  "C:\Users\Inge\Documents\workspace_cs\Streifen-Schneidmaschinen-App\SchneidMaschine.csproj" ^
  -t:Build -p:Configuration=Debug
```

**Ergebnis**:
```
Der Buildvorgang wurde erfolgreich ausgeführt.
    4 Warnung(en)
    0 Fehler
Verstrichene Zeit 00:00:01.56
```

### Anwendung starten
```bash
start "C:\Users\Inge\Documents\workspace_cs\Streifen-Schneidmaschinen-App\bin\Debug\SchneidMaschine.exe"
```

**Status**: ✅ Läuft erfolgreich

## Lessons Learned

### 1. .NET Framework vs .NET Core
- WPF .NET Framework 4.7.2 **MUSS** mit MSBuild kompiliert werden
- `dotnet` CLI ist nur für .NET Core/5+
- App.xaml generiert die Main-Methode automatisch (via MSBuild)

### 2. XAML Code-Behind
- Button-Namen in XAML (`x:Name="BtnM10mm"`) müssen exakt mit Code übereinstimmen
- Partial Classes werden von MSBuild aus XAML generiert (*.g.cs Dateien)
- Event-Handler können private sein wenn im XAML definiert
- Event-Handler müssen public sein wenn von außen aufgerufen

### 3. Debugging-Strategie
1. Zuerst richtiges Build-Tool verwenden (MSBuild)
2. Fehler-Log nach Patterns durchsuchen
3. XAML-Dateien prüfen für tatsächliche Element-Namen
4. Code-Behind prüfen für tatsächliche Methodennamen
5. Access-Modifier nur ändern wenn von außerhalb zugegriffen wird

### 4. Keybinding-Implementation Best Practices
- **Vor** Implementierung: XAML-Buttons und Methoden dokumentieren
- **Während** Implementierung: Testen ob Buttons/Methoden existieren
- **Nach** Implementierung: Inkrementell kompilieren statt alles auf einmal

## Prävention für zukünftige Entwicklung

### Checkliste vor Code-Commit
- [ ] Mit MSBuild kompilieren (nicht dotnet)
- [ ] Alle XAML-Button-Namen mit Code-Referenzen abgleichen
- [ ] Event-Handler-Namen verifizieren
- [ ] Access-Modifier prüfen (public wenn externe Aufrufe)
- [ ] Inkrementell testen nach jeder größeren Änderung

### Empfohlener Development Workflow
```bash
# 1. Änderungen machen
# 2. Kompilieren
MSBuild SchneidMaschine.csproj -t:Build

# 3. Fehler beheben
# 4. Erneut kompilieren
# 5. Erst dann committen
```

---

**Fix-Datum**: 18. November 2025
**Aufwand**: ~45 Minuten
**Status**: ✅ Alle Build-Fehler behoben, Anwendung läuft
