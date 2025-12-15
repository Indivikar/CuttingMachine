# TODO - SchneidMaschine App v1.1.0+

**Projekt**: SchneidMaschine WPF Application
**Framework**: .NET Framework 4.7.2
**Letzte Aktualisierung**: 10. Dezember 2025
**Status**: Kritische Bugs BEHOBEN - Hardware-Test erforderlich

---

## ✅ BEHOBEN - Bug-Fixes implementiert (10.12.2025)

### Bug #1: ArgumentOutOfRangeException beim Verbinden

**Status**: ✅ BEHOBEN
**Priorität**: KRITISCH
**Datum**: 10.12.2025

#### Problem-Beschreibung

Beim Verbinden mit dem Arduino:
```
System.ArgumentOutOfRangeException: "StartIndex darf nicht kleiner als Null sein.
Parametername: startIndex"
```

**Stack Trace**:
- `MainWindow.xaml.cs:827` - `handleCommandLineSchneidmaschine()`
- `MainWindow.xaml.cs:405` - `handleCommandLineRollenzentrierung()`

#### Root Cause

Nach dem `.Clear()` Fix (siehe Bug #2) konnte ein leerer String übergeben werden.
Die Methoden versuchten dann `text.Substring(text.Length - 1)` auszuführen, was bei einem leeren String zu `-1` führt.

#### Fix implementiert

**Datei**: `MainWindow.xaml.cs`

```csharp
// handleCommandLineSchneidmaschine() - Zeile 826-830
// handleCommandLineRollenzentrierung() - Zeile 404-408

text = text.Trim();
text = text.Replace("\t", "").Replace("\n", "").Replace("\r", "");

// Prüfe, ob der String leer ist
if (string.IsNullOrEmpty(text))
{
    return;
}

string lastChar = text.Substring(text.Length - 1);
```

**Betroffene Methoden**:
- `handleCommandLineSchneidmaschine()` (Zeile 820-855)
- `handleCommandLineRollenzentrierung()` (Zeile 397-413)

---

### Bug #2: Keine Rückmeldungen in TextBox / Leere befehlBuilder

**Status**: ✅ BEHOBEN
**Priorität**: KRITISCH
**Datum**: 10.12.2025

#### Problem-Beschreibung

Nach dem ersten `.Clear()` Fix gab es keine TextBox-Ausgaben mehr:
- Keine "Connected" Meldung
- Keine "Arduino antwortet>>" Nachrichten
- Leere Zeilen in der TextBox

#### Root Cause

Die Methoden `SetTextSchneidmaschine()` und `SetTextRollenzentrierung()` hatten einen logischen Fehler:

```csharp
// FALSCH (alter Code):
if (stringToCharSchneidmaschine(text) == null) { return; }
text = befehlBuilderSchneidmaschine.ToString();  // ← Builder ist bereits leer!
handleCommandLineSchneidmaschine(text);
```

**Problem**:
1. `stringToCharSchneidmaschine(text)` wird aufgerufen
2. Diese Methode leert den Builder mit `.Clear()` (Zeile 1243)
3. Dann wird der **leere** Builder nochmal ausgelesen (Zeile 779)
4. `handleCommandLineSchneidmaschine()` bekommt einen leeren String

#### Fix implementiert

**Datei**: `MainWindow.xaml.cs`

```csharp
// SetTextSchneidmaschine() - Zeile 774-782
string processedText = stringToCharSchneidmaschine(text);
if (processedText == null)
{
    return;
}

text = processedText;  // ← Verwende den Rückgabewert!
handleCommandLineSchneidmaschine(text);

// SetTextRollenzentrierung() - Zeile 345-353
string processedText = stringToCharRollenzentrierung(text);
if (processedText == null)
{
    return;
}

text = processedText;  // ← Verwende den Rückgabewert!
handleCommandLineRollenzentrierung(text);
```

**Betroffene Methoden**:
- `SetTextSchneidmaschine()` (Zeile 763-818)
- `SetTextRollenzentrierung()` (Zeile 334-395)

---

### Bug #3: schneidenBeendet kommt VOR dem physischen Schnitt

**Status**: ✅ BEHOBEN
**Priorität**: KRITISCH
**Datum**: 10.12.2025

#### Problem-Beschreibung

Die App empfing `schneidenBeendet_` BEVOR der physische Schnitt fertig war:
- LOGO-SPS Motor läuft noch
- `schneidenBeendet_` wird bereits gesendet
- Buttons werden zu früh aktiviert

#### Root Cause - Original Analyse (FALSCH)

Ursprünglich wurde vermutet, dass Pin 7 nicht überwacht wurde. **Dies war falsch!**

Die alte Logik wartete auf `valMotorRunning == HIGH && isMotorRunning`:
- Pin 7 ist mit `INPUT_PULLUP` konfiguriert → **immer HIGH** (wenn nicht auf LOW gezogen)
- LOGO-SPS sollte Pin 7 auf LOW ziehen (Motor läuft), dann auf HIGH gehen (Motor fertig)
- Aber: Pin 7 blieb immer HIGH → Bedingung war sofort erfüllt

#### Root Cause - KORREKT (nach Hardware-Test #1)

Pin 7 muss auf **Statuswechsel** überwacht werden, nicht nur auf HIGH:

1. **Warten bis Pin 7 LOW wird** (LOGO-SPS signalisiert: Motor läuft)
2. **Dann warten bis Pin 7 HIGH wird** (LOGO-SPS signalisiert: Motor fertig)

Die alte Logik prüfte nur: `if(HIGH && isMotorRunning)` → sofort true!

#### Root Cause - FINAL (nach Hardware-Test #2 mit Multimeter)

**Das eigentliche Problem**: `INPUT_PULLUP` verhindert, dass Pin 7 LOW gelesen wird!

**Hardware-Test Ergebnis**:
- Mit Multimeter angeschlossen: ✅ **Funktioniert** (Pin liest LOW/HIGH korrekt)
- Ohne Multimeter: ❌ **Funktioniert nicht** (Pin liest nur HIGH, nie LOW)

**Warum?**
- `INPUT_PULLUP` aktiviert internen Pull-Up Widerstand (20-50kΩ) nach VCC (5V)
- LOGO-SPS schaltet zwar auf 0V (gemessen mit Multimeter!)
- Aber: Spannungsteiler zwischen LOGO (0V) und Pull-Up (5V)
- Arduino liest den Pin als HIGH, obwohl LOGO 0V sendet
- **Multimeter-Eingangswiderstand (~10MΩ) wirkt als Pull-Down → stabilisiert Pin**

**Lösung - Teil 1: INPUT statt INPUT_PULLUP**:
```cpp
// ALT (FALSCH):
pinMode(motorRunning, INPUT_PULLUP);   // ← Pull-Up verhindert LOW-Erkennung!

// NEU (RICHTIG):
pinMode(motorRunning, INPUT);   // Kein Pull-Up - Pin kann schweben
```

**Lösung - Teil 2: Externer Pull-Down Widerstand ERFORDERLICH**:

Da LOGO-SPS aktiv 5V/0V schaltet (Push-Pull), aber der Pin ohne Pull-Up "schwebt" (floating),
muss ein **externer Pull-Down Widerstand** hinzugefügt werden:

```
Hardware-Schaltung:

LOGO-SPS Q3 Ausgang -----> Arduino Pin 7
                              |
                            [10kΩ]  ← Pull-Down Widerstand
                              |
                             GND
```

**Empfohlener Widerstandswert**: **10 kΩ** (alternativ: 4,7 kΩ - 47 kΩ)

**Warum 10kΩ?**
- Zu klein (< 1kΩ): Belastet LOGO-SPS zu stark
- 10kΩ - 47kΩ: Optimal - stabilisiert Pin, belastet LOGO nicht (0,5 mA)
- Zu groß (> 100kΩ): Hilft nicht gegen Rauschen, Pin schwebt

**Funktionsweise**:
- LOGO-SPS HIGH (5V): Überschreibt Pull-Down → Arduino liest HIGH ✅
- LOGO-SPS LOW (0V): Pull-Down zieht Pin auf GND → Arduino liest LOW ✅
- Ohne Signal: Pull-Down hält Pin auf LOW (definierter Zustand)

#### Fix implementiert

**Datei**: `IoT/sketche/SchneidMaschine/SchneidMaschine.ino`

**Neue Variablen** (Zeile 31-32):
```cpp
boolean motorStartedSignalReceived = false; // wurde das LOW-Signal empfangen?
unsigned long motorStartTime = 0; // Zeitpunkt des Schnitt-Starts (für Timeout)
```

**KRITISCHE Änderung: pinMode() in setup()** (Zeile 56):
```cpp
// ALT (FALSCH):
pinMode(motorRunning, INPUT_PULLUP);   // ← Pull-Up verhindert LOW-Erkennung!

// NEU (RICHTIG):
pinMode(motorRunning, INPUT);   // Kein Pull-Up - ERFORDERT externen Pull-Down!
```

**Geänderte Funktion: `schneiden()`** (Zeile 285-302):
```cpp
void schneiden() {
    sendCommand("schneidenStartet_", true);
    isMotorRunning = true;                  // Aktiviere Motor-Überwachung
    motorStartedSignalReceived = false;     // Reset: warte auf LOW-Signal
    motorStartTime = millis();              // Merke Start-Zeit für Timeout

    // DEBUG: Zeige Pin7 Status VOR dem Schnitt
    int pinStatusVor = digitalRead(motorRunning);
    sendText("DEBUG: Pin7 VOR Schnitt = " + String(pinStatusVor) + " (erwarte 1=HIGH)");

    digitalWrite(cut, LOW);                 // Schalte Relay -> Schneiden Start
    delay(500);
    digitalWrite(cut, HIGH);                // Schalte Relay -> Schneiden Stop

    // NICHT hier beenden!
    // motorFinished() überwacht Pin 7 und sendet schneidenBeendet_
}
```

**Komplett neue Logik: `motorFinished()`** (Zeile 102-152):
```cpp
void motorFinished() {
    if(!isMotorRunning) { return; }

    valMotorRunning = digitalRead(motorRunning);

    // TIMEOUT: 10 Sekunden - falls LOGO-SPS nicht antwortet
    unsigned long motorTimeout = 10000;
    if(millis() - motorStartTime > motorTimeout) {
        sendText("!!! WARNUNG: Pin7 Timeout - kein Signal von LOGO-SPS !!!");
        // Sende trotzdem schneidenBeendet_
        stepCounter = 0;
        if(isKopfSchnitt) {
            sendCommand("kopfSchnittBeendet_", true);
            isKopfSchnitt = false;
        } else {
            sendCommand("schneidenBeendet_", true);
        }
        isMotorRunning = false;
        motorStartedSignalReceived = false;
        return;
    }

    // Schritt 1: Warte auf LOW-Signal (LOGO-SPS meldet: Motor läuft)
    if(!motorStartedSignalReceived && valMotorRunning == LOW) {
        motorStartedSignalReceived = true;
        sendText("DEBUG: Pin7 LOW - Motor läuft");
    }

    // Schritt 2: Warte auf HIGH-Signal (LOGO-SPS meldet: Motor fertig)
    // Nur NACHDEM wir das LOW-Signal empfangen haben!
    if(motorStartedSignalReceived && valMotorRunning == HIGH) {
        sendText("DEBUG: Pin7 HIGH - Motor fertig");

        stepCounter = 0;
        if(isKopfSchnitt) {
            sendCommand("kopfSchnittBeendet_", true);
            isKopfSchnitt = false;
        } else {
            sendCommand("schneidenBeendet_", true);
        }
        isMotorRunning = false;
        motorStartedSignalReceived = false;
    }
}
```

**Wichtige Änderungen**:
- ✅ `INPUT` statt `INPUT_PULLUP` (Zeile 56)
- ✅ **HARDWARE ERFORDERLICH**: Externer 10kΩ Pull-Down Widerstand (Pin 7 → GND)
- ✅ Wartet auf Statuswechsel LOW → HIGH (nicht nur HIGH)
- ✅ 10 Sekunden Timeout als Sicherheit
- ✅ Debug-Ausgaben für Pin 7 Status vor und während Schnitt
- ✅ Korrekte Sequenz: schneidenStartet → [warte auf LOGO-SPS] → schneidenBeendet

---

### Debug-Ausgaben hinzugefügt (C# App)

**Datei**: `MainWindow.xaml.cs`

**In `commandReceivedSchneidmaschine()`** (Zeile 860):
```csharp
Console.WriteLine("commandReceivedSchneidmaschine: " + text);
SetTextSchneidmaschine("&[DEBUG] Befehl empfangen: [" + text + "]\n&");
```

**In `commandRunSchneidmaschine()` bei schneidenBeendet** (Zeile 925):
```csharp
case COMMAND_Schneidmaschine.schneidenBeendet:
    {
        Console.WriteLine("COMMAND_Schneidmaschine.schneidenBeendet");
        SetTextSchneidmaschine("&[DEBUG] schneidenBeendet empfangen - Aktiviere Buttons\n&");
        dataModel.IsCutFinished = true;
        Main.IsEnabled = true;
        refreshStats(false);
        dataModel.DBHandler.updateCut();
        break;
    }
```

Diese Debug-Ausgaben helfen bei der Fehlersuche und zeigen:
- Welche Befehle empfangen werden
- Ob Befehle korrekt erkannt werden
- Ob die UI aktualisiert wird

---

## 📊 Zusammenfassung der Änderungen

### C# App (MainWindow.xaml.cs)

| Methode | Zeile | Änderung |
|---------|-------|----------|
| `handleCommandLineSchneidmaschine()` | 826-830 | Empty String Check hinzugefügt |
| `handleCommandLineRollenzentrierung()` | 404-408 | Empty String Check hinzugefügt |
| `SetTextSchneidmaschine()` | 774-782 | Verwendung des Rückgabewerts von `stringToChar*()` |
| `SetTextRollenzentrierung()` | 345-353 | Verwendung des Rückgabewerts von `stringToChar*()` |
| `commandReceivedSchneidmaschine()` | 860 | Debug-Ausgabe hinzugefügt |
| `commandRunSchneidmaschine()` | 925 | Debug-Ausgabe bei schneidenBeendet |

### Arduino Sketch (SchneidMaschine.ino)

| Element | Zeile | Änderung |
|---------|-------|----------|
| Variablen | 31-32 | `motorStartedSignalReceived`, `motorStartTime` hinzugefügt |
| `setup()` pinMode | 56 | **`INPUT_PULLUP` → `INPUT`** (KRITISCH!) |
| `schneiden()` | 285-302 | Sendet NICHT mehr direkt `schneidenBeendet_` + Debug-Ausgabe Pin7 |
| `motorFinished()` | 102-152 | Komplett neu: Statuswechsel-Erkennung + Timeout |

### Hardware-Änderung ERFORDERLICH

| Komponente | Wert | Anschluss |
|------------|------|-----------|
| Pull-Down Widerstand | **10 kΩ** (4,7-47kΩ) | Pin 7 → GND |

**Schaltplan**:
```
LOGO-SPS Q3 -----> Arduino Pin 7
                      |
                   [10kΩ]
                      |
                     GND
```

### Build Status

**C# App**:
- ✅ Kompiliert erfolgreich
- ⚠️ 4-6 Warnungen (harmlose unused fields)
- 0 Fehler
- Executable: `bin\Debug\SchneidMaschine.exe`

**Arduino Sketch**:
- ⚠️ Muss auf ESP32/Arduino hochgeladen werden
- Datei: `IoT\sketche\SchneidMaschine\SchneidMaschine.ino`

---

## ⚠️ WICHTIG: Hardware-Test erforderlich!

### Testanleitung

**WICHTIG**: Vor dem Test MUSS der **10kΩ Pull-Down Widerstand** eingebaut sein!

0. **Hardware vorbereiten** ⚠️ **KRITISCH**:
   - Besorge einen 10kΩ Widerstand (Farbcode: Braun-Schwarz-Orange)
   - Löte/stecke den Widerstand zwischen:
     - Eine Seite: Arduino Pin 7
     - Andere Seite: Arduino GND (Masse)
   - **OHNE diesen Widerstand funktioniert der Fix nicht!**

1. **Arduino-Sketch hochladen**:
   - Öffne `IoT\sketche\SchneidMaschine\SchneidMaschine.ino` in Arduino IDE
   - Lade auf ESP32/Arduino hoch
   - Warte auf "Upload erfolgreich"

2. **C# App schließen** (falls läuft):
   - Schließe alle laufenden `SchneidMaschine.exe` Instanzen

3. **C# App starten**:
   - Starte `bin\Debug\SchneidMaschine.exe`

4. **Verbindung herstellen**:
   - Wähle COM-Port aus
   - Klicke "Verbinden"
   - Erwartete Ausgabe:
     ```
     try to Connect with Arduino....
     Arduino antwortet>> Connected
     Arduino antwortet>> handradOff_
     Arduino antwortet>> steps_0
     ✓ Board identifiziert: Schneidmaschine an COM5
     ```

5. **Schnitt durchführen**:
   - Klicke "Schneiden" Button (oder betätige Taster)
   - **Mit LOGO-SPS verbunden UND 10kΩ Widerstand** - Erwartete Ausgabe:
     ```
     Arduino antwortet>> schneidenStartet_
     [DEBUG] Befehl empfangen: [schneidenStartet]
     Arduino antwortet>> DEBUG: Pin7 VOR Schnitt = 1 (erwarte 1=HIGH)
     Arduino antwortet>> DEBUG: Pin7 LOW - Motor läuft
     Arduino antwortet>> DEBUG: Pin7 HIGH - Motor fertig
     Arduino antwortet>> schneidenBeendet_
     [DEBUG] Befehl empfangen: [schneidenBeendet]
     [DEBUG] schneidenBeendet empfangen - Aktiviere Buttons
     ```

   - **OHNE 10kΩ Widerstand** - Fehlverhalten:
     ```
     Arduino antwortet>> schneidenStartet_
     [DEBUG] Befehl empfangen: [schneidenStartet]
     Arduino antwortet>> DEBUG: Pin7 VOR Schnitt = 1 (erwarte 1=HIGH)
     [... 10 Sekunden warten ...]
     Arduino antwortet>> !!! WARNUNG: Pin7 Timeout - kein Signal von LOGO-SPS !!!
     Arduino antwortet>> schneidenBeendet_
     [DEBUG] Befehl empfangen: [schneidenBeendet]
     [DEBUG] schneidenBeendet empfangen - Aktiviere Buttons
     ```
     ⚠️ **"DEBUG: Pin7 LOW - Motor läuft" fehlt** → Widerstand fehlt!

   - **OHNE LOGO-SPS (aber MIT 10kΩ Widerstand)** - Erwartete Ausgabe (nach 10 Sekunden):
     ```
     Arduino antwortet>> schneidenStartet_
     [DEBUG] Befehl empfangen: [schneidenStartet]
     Arduino antwortet>> DEBUG: Pin7 VOR Schnitt = 0 (erwarte 1=HIGH)  ← Pin auf LOW!
     [... 10 Sekunden warten ...]
     Arduino antwortet>> !!! WARNUNG: Pin7 Timeout - kein Signal von LOGO-SPS !!!
     Arduino antwortet>> schneidenBeendet_
     [DEBUG] Befehl empfangen: [schneidenBeendet]
     [DEBUG] schneidenBeendet empfangen - Aktiviere Buttons
     ```

6. **Mehrere Schnitte testen**:
   - Führe 5-10 Schnitte durch
   - Alle sollten `schneidenBeendet_` empfangen
   - Buttons sollten jedes Mal aktiviert werden

7. **Schrittmotor testen**:
   - Bewege Schrittmotor (z.B. 100mm)
   - Erwartete Ausgabe:
     ```
     Arduino antwortet>> Schrittmotor starten...
     Arduino antwortet>> stepperFinished_1270
     ```
   - Wiederhole mehrfach
   - Alle sollten `stepperFinished_` empfangen

### Erfolgskriterien

- ✅ Keine Exception beim Verbinden
- ✅ TextBox zeigt alle Nachrichten
- ✅ Keine leeren Zeilen
- ✅ `schneidenBeendet_` kommt erst nach physischem Schnitt
- ✅ Buttons werden nach jedem Schnitt aktiviert
- ✅ `stepperFinished_` kommt nach jeder Bewegung
- ✅ Debug-Ausgaben zeigen korrekte Befehlsverarbeitung

---

## 📋 Offene Features aus specs.md

### Feature #1: Test-Button funktioniert nicht

**Status**: 📋 Offen (Low Priority)
**Beschreibung**: Test-Button soll Befehl an Board senden und Antwort anzeigen
**Aufwand**: 30 Minuten

### Feature #2: Fehlende Leerzeichen in TextBox-Ausgabe

**Status**: 📋 Offen (Low Priority)
**Beschreibung**:
- Manchmal: "Schrittmotorstarten..." (falsch)
- Richtig: "Schrittmotor starten..."

**Ursache**: C# Regex entfernt zu viele/zu wenige Leerzeichen
**Aufwand**: 1 Stunde

### Feature #3: Footer am unteren Fensterrand

**Status**: 📋 Offen (Low Priority)
**Beschreibung**: Footer soll immer am unteren Fensterrand bleiben (auch beim Resize)
**Aufwand**: 30 Minuten

### Feature #4: GroupBoxen sollen mit Fenster mitwachsen

**Status**: 📋 Offen (Low Priority)
**Aufwand**: 1 Stunde

### Feature #5: Status im Slider wird nicht aktualisiert

**Status**: 📋 Offen (Medium Priority)
**Aufwand**: 30 Minuten

### Feature #6: Button-Darstellung verschoben

**Status**: 📋 Offen (Low Priority)
**Aufwand**: 30 Minuten

### Feature #7: Board-Typ in ComboBox Port-Namen anzeigen

**Status**: 📋 Offen (Nice-to-have)
**Aufwand**: 1-2 Stunden
**Beschreibung**: "COM3 (Schneidmaschine)" statt nur "COM3"

### Feature #8: Keybinding-System

**Status**: ✅ Bereits implementiert!
**Commits**: `dc95b64`, `bbfd6d6`, weitere

---

## 🎯 Nächste Schritte

### Sofort (Heute)

1. ✅ **C# App kompiliert** (erledigt)
2. ✅ **Arduino Sketch geändert** (erledigt)
3. ⚠️ **HARDWARE: 10kΩ Pull-Down Widerstand einbauen** (KRITISCH!)
   - Zwischen Pin 7 und GND löten/stecken
   - Farbcode: Braun-Schwarz-Orange
   - **OHNE diesen Widerstand funktioniert der Fix nicht!**
4. ⚠️ **Arduino Sketch hochladen** (User muss machen)
5. ⚠️ **Hardware-Test durchführen** (siehe Testanleitung oben)
6. ⚠️ **Ergebnisse dokumentieren**

### Diese Woche

6. **Debug-Ausgaben entfernen** (falls alles funktioniert):
   - `[DEBUG] Befehl empfangen:` Zeilen entfernen
   - `[DEBUG] schneidenBeendet empfangen` entfernen
   - Pin7 Debug-Meldungen optional behalten

7. **Git Commit erstellen**:
   ```
   [FIX] Kritische Bugs behoben: ArgumentOutOfRange, leere TextBox, schneidenBeendet Timing

   - MainWindow.xaml.cs: Empty String Check in handleCommandLine*()
   - MainWindow.xaml.cs: Verwendung des Rückgabewerts in SetText*()
   - SchneidMaschine.ino: INPUT_PULLUP → INPUT (Pin 7)
   - SchneidMaschine.ino: Pin7 Statuswechsel-Erkennung mit Timeout
   - Debug-Ausgaben hinzugefügt für Fehlersuche

   HARDWARE ERFORDERLICH: 10kΩ Pull-Down Widerstand (Pin 7 → GND)
   ```

8. **Pull Request erstellen** (optional):
   - Aus `dev` Branch nach `master`

### Später (Optional)

9. **Offene Features** aus specs.md abarbeiten (siehe oben)

10. **Dokumentation vervollständigen**:
    - `readme.md` aktualisieren
    - `IMPLEMENTATION_PLAN.md` aktualisieren
    - Bekannte Probleme entfernen (wenn behoben)

---

## 📝 Wichtige Hinweise

### ⚠️ KRITISCH: 10kΩ Pull-Down Widerstand ERFORDERLICH!

**Hardware-Anforderung**:
- **10kΩ Widerstand** zwischen Pin 7 und GND
- Farbcode: Braun-Schwarz-Orange
- **OHNE diesen Widerstand funktioniert der Fix nicht!**

**Warum?**
- Pin 7 ist jetzt `INPUT` (ohne internen Pull-Up)
- LOGO-SPS schaltet aktiv 5V/0V (Push-Pull)
- Pull-Down stabilisiert den Pin gegen Rauschen/Floating
- Mit Multimeter funktioniert es (Multimeter wirkt als Pull-Down)
- Ohne Multimeter schwebt der Pin → undefiniertes Verhalten

**Schaltung**:
```
LOGO-SPS Q3 -----> Arduino Pin 7
                      |
                   [10kΩ]  ← Pull-Down Widerstand
                      |
                     GND
```

### Pin 7 (motorRunning) MUSS angeschlossen sein

Die neue Logik **erwartet ein Signal von der LOGO-SPS**:
- Ohne LOGO-SPS: 10 Sekunden Timeout, dann `schneidenBeendet_`
- Mit LOGO-SPS UND Pull-Down: Wartet auf Statuswechsel LOW → HIGH

**Wenn LOGO-SPS nicht angeschlossen**:
- Timeout-Warnung wird angezeigt
- Schnitt wird trotzdem beendet (nach 10s)
- App funktioniert, aber nicht optimal

**Wenn Pull-Down Widerstand fehlt**:
- Pin liest nie LOW (obwohl LOGO 0V sendet!)
- Timeout nach 10 Sekunden
- "DEBUG: Pin7 LOW - Motor läuft" erscheint nie
- **Fix funktioniert nicht!**

### Leerzeichen-Problem

Das Problem "Schrittmotorstarten..." vs "Schrittmotor starten..." wurde **NICHT** behoben.

**Ursache**: C# Regex in `SetTextSchneidmaschine()` (Zeile 791-793):
```csharp
text = Regex.Replace(text, @"\s+", " ");           // Mehrere Leerzeichen → eins
text = Regex.Replace(text, @"\s+([!,\.])", "$1");  // Leerzeichen vor Satzzeichen
```

Dies entfernt manchmal zu viele Leerzeichen, wenn Nachrichten fragmentiert ankommen.

**Lösung**: Entweder Arduino-Nachrichten anpassen oder C# Regex verbessern (niedrige Priorität).

---

**Letzte Aktualisierung**: 10. Dezember 2025, 16:30 Uhr
**Erstellt von**: Claude Code Analysis & Bug Fixing Session
**Projekt-Version**: v1.1.0+ (dev branch)

**WICHTIGER HINWEIS**: Hardware-Änderung erforderlich (10kΩ Pull-Down an Pin 7)
