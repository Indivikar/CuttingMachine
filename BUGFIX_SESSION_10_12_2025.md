# Bug-Fix Session - 10. Dezember 2025

**Projekt**: SchneidMaschine WPF Application
**Session Start**: 10.12.2025, ~13:30 Uhr
**Session Ende**: 10.12.2025, ~16:30 Uhr
**Durchgeführt von**: Claude Code (Sonnet 4.5)
**Branch**: dev
**Status**: ⚠️ HARDWARE-ÄNDERUNG ERFORDERLICH (10kΩ Pull-Down Widerstand)

---

## 🎯 Ziel der Session

Behebung kritischer Bugs, die nach der Überarbeitung von Sketch und App aufgetreten sind:
- `ArgumentOutOfRangeException` beim Verbinden
- Keine Rückmeldungen in TextBox
- `schneidenBeendet_` kommt vor dem physischen Schnitt

---

## 📋 Behobene Bugs

### Bug #1: ArgumentOutOfRangeException beim Verbinden

**Symptom**:
```
System.ArgumentOutOfRangeException: "StartIndex darf nicht kleiner als Null sein."
Bei MainWindow.xaml.cs:827
```

**Ursache**:
Nach dem `.Clear()` Fix konnte ein leerer String übergeben werden. Die Methode versuchte dann `text.Substring(text.Length - 1)` → `-1` → Exception.

**Fix**:
```csharp
if (string.IsNullOrEmpty(text)) { return; }
```

**Dateien geändert**:
- `MainWindow.xaml.cs:826-830` - `handleCommandLineSchneidmaschine()`
- `MainWindow.xaml.cs:404-408` - `handleCommandLineRollenzentrierung()`

---

### Bug #2: Keine Rückmeldungen in TextBox

**Symptom**:
- Keine "Connected" Meldung
- Keine "Arduino antwortet>>" Nachrichten
- Leere Zeilen erscheinen
- Fehlende Leerzeichen: "Schrittmotorstarten..."

**Ursache**:
```csharp
// FALSCH:
if (stringToCharSchneidmaschine(text) == null) { return; }
text = befehlBuilderSchneidmaschine.ToString();  // ← Builder bereits leer!
```

Die Methode `stringToChar*()` leert den Builder mit `.Clear()`, dann wird der leere Builder ausgelesen.

**Fix**:
```csharp
// RICHTIG:
string processedText = stringToCharSchneidmaschine(text);
if (processedText == null) { return; }
text = processedText;  // ← Verwende Rückgabewert!
```

**Dateien geändert**:
- `MainWindow.xaml.cs:774-782` - `SetTextSchneidmaschine()`
- `MainWindow.xaml.cs:345-353` - `SetTextRollenzentrierung()`

---

### Bug #3: schneidenBeendet kommt VOR physischem Schnitt

**Symptom**:
- `schneidenBeendet_` wird sofort gesendet
- LOGO-SPS Motor läuft noch
- Buttons werden zu früh aktiviert

**Ursprüngliche Vermutung (FALSCH)**:
Pin 7 wird nicht überwacht → Einfach direkt `schneidenBeendet_` senden wie v1.0.0

**Tatsächliche Ursache (KORREKT)**:
Pin 7 wurde überwacht, aber falsch:
```cpp
// ALT (FALSCH):
if(valMotorRunning == HIGH && isMotorRunning) {
    sendCommand("schneidenBeendet_", true);
}
```

Problem: Pin 7 ist `INPUT_PULLUP` → **immer HIGH** (wenn nicht auf LOW gezogen)
→ Bedingung war sofort erfüllt!

**Korrekte Lösung**:
Warten auf **Statuswechsel** LOW → HIGH:

1. Warte bis Pin 7 **LOW** wird (LOGO-SPS: Motor läuft)
2. Dann warte bis Pin 7 **HIGH** wird (LOGO-SPS: Motor fertig)
3. Erst dann `schneidenBeendet_` senden

**Implementierung**:

Neue Variablen:
```cpp
boolean motorStartedSignalReceived = false;
unsigned long motorStartTime = 0;
```

Geänderte `schneiden()`:
```cpp
void schneiden() {
    sendCommand("schneidenStartet_", true);
    isMotorRunning = true;
    motorStartedSignalReceived = false;  // Reset
    motorStartTime = millis();           // Für Timeout

    digitalWrite(cut, LOW);
    delay(500);
    digitalWrite(cut, HIGH);

    // NICHT hier beenden - motorFinished() überwacht Pin 7
}
```

Neue `motorFinished()` Logik:
```cpp
void motorFinished() {
    if(!isMotorRunning) { return; }

    valMotorRunning = digitalRead(motorRunning);

    // TIMEOUT: 10 Sekunden
    if(millis() - motorStartTime > 10000) {
        sendText("!!! WARNUNG: Pin7 Timeout !!!");
        // Sende trotzdem schneidenBeendet_
        // ...
        return;
    }

    // Schritt 1: Warte auf LOW
    if(!motorStartedSignalReceived && valMotorRunning == LOW) {
        motorStartedSignalReceived = true;
        sendText("DEBUG: Pin7 LOW - Motor läuft");
    }

    // Schritt 2: Warte auf HIGH (nur nach LOW!)
    if(motorStartedSignalReceived && valMotorRunning == HIGH) {
        sendText("DEBUG: Pin7 HIGH - Motor fertig");
        sendCommand("schneidenBeendet_", true);
        isMotorRunning = false;
        motorStartedSignalReceived = false;
    }
}
```

**Dateien geändert**:
- `IoT/sketche/SchneidMaschine/SchneidMaschine.ino:31-32` - Neue Variablen
- `IoT/sketche/SchneidMaschine/SchneidMaschine.ino:267-280` - `schneiden()`
- `IoT/sketche/SchneidMaschine/SchneidMaschine.ino:102-152` - `motorFinished()`

---

### Debug-Ausgaben hinzugefügt

Zur Fehlersuche wurden Debug-Ausgaben hinzugefügt:

**C# App**:
```csharp
// In commandReceivedSchneidmaschine()
SetTextSchneidmaschine("&[DEBUG] Befehl empfangen: [" + text + "]\n&");

// In commandRunSchneidmaschine() bei schneidenBeendet
SetTextSchneidmaschine("&[DEBUG] schneidenBeendet empfangen - Aktiviere Buttons\n&");
```

**Arduino**:
```cpp
sendText("DEBUG: Pin7 LOW - Motor läuft");
sendText("DEBUG: Pin7 HIGH - Motor fertig");
sendText("!!! WARNUNG: Pin7 Timeout - kein Signal von LOGO-SPS !!!");
```

**Dateien geändert**:
- `MainWindow.xaml.cs:860` - Debug in `commandReceivedSchneidmaschine()`
- `MainWindow.xaml.cs:925` - Debug in `commandRunSchneidmaschine()`

---

## 📊 Geänderte Dateien - Zusammenfassung

### C# App (MainWindow.xaml.cs)

| Zeile | Methode | Änderung |
|-------|---------|----------|
| 404-408 | `handleCommandLineRollenzentrierung()` | Empty String Check |
| 345-353 | `SetTextRollenzentrierung()` | Rückgabewert verwenden |
| 826-830 | `handleCommandLineSchneidmaschine()` | Empty String Check |
| 774-782 | `SetTextSchneidmaschine()` | Rückgabewert verwenden |
| 860 | `commandReceivedSchneidmaschine()` | Debug-Ausgabe |
| 925 | `commandRunSchneidmaschine()` | Debug-Ausgabe |

**Gesamt**: 6 Methoden geändert, ~30 Zeilen Code geändert

### Arduino Sketch (SchneidMaschine.ino)

| Zeile | Element | Änderung |
|-------|---------|----------|
| 31-32 | Variablen | `motorStartedSignalReceived`, `motorStartTime` |
| 267-280 | `schneiden()` | Sendet nicht mehr direkt `schneidenBeendet_` |
| 102-152 | `motorFinished()` | Komplett neu: Statuswechsel-Erkennung |

**Gesamt**: 3 Funktionen/Bereiche geändert, ~60 Zeilen Code geändert

---

## 🔧 Build & Test Status

### Build Status

**C# App**:
- ✅ Kompiliert erfolgreich (MSBuild 16.10.2)
- ⚠️ 4-6 Warnungen (harmlos - unused fields)
- 0 Fehler
- Output: `bin\Debug\SchneidMaschine.exe`

**Arduino Sketch**:
- ✅ Code geschrieben und gespeichert
- ⚠️ Noch nicht auf Hardware hochgeladen
- Datei: `IoT/sketche/SchneidMaschine/SchneidMaschine.ino`

### Test Status

**Getestet**:
- ✅ C# App kompiliert
- ✅ Keine Syntax-Fehler
- ✅ Debug-Ausgaben implementiert

**Noch nicht getestet**:
- ⚠️ Hardware-Test mit echtem Arduino/ESP32
- ⚠️ Hardware-Test mit LOGO-SPS
- ⚠️ Mehrfach-Schnitte
- ⚠️ Schrittmotor-Bewegungen

---

## 📝 Nächste Schritte für User

### Sofort

1. **Arduino Sketch hochladen**:
   ```
   - Öffne Arduino IDE
   - Öffne: IoT\sketche\SchneidMaschine\SchneidMaschine.ino
   - Wähle Board: ESP32/Arduino (je nach Hardware)
   - Wähle COM-Port
   - Klicke "Upload"
   ```

2. **C# App neu starten**:
   ```
   - Schließe alte App (falls läuft)
   - Starte: bin\Debug\SchneidMaschine.exe
   ```

3. **Test durchführen**:
   ```
   - Verbinde mit Arduino
   - Führe 5-10 Schnitte durch
   - Beobachte Debug-Ausgaben
   - Prüfe ob Buttons aktiviert werden
   ```

### Erwartete Ausgabe (mit LOGO-SPS)

```
Arduino antwortet>> schneidenStartet_
[DEBUG] Befehl empfangen: [schneidenStartet]
Arduino antwortet>> DEBUG: Pin7 LOW - Motor läuft
Arduino antwortet>> DEBUG: Pin7 HIGH - Motor fertig
Arduino antwortet>> schneidenBeendet_
[DEBUG] Befehl empfangen: [schneidenBeendet]
[DEBUG] schneidenBeendet empfangen - Aktiviere Buttons
```

### Erwartete Ausgabe (ohne LOGO-SPS)

```
Arduino antwortet>> schneidenStartet_
[DEBUG] Befehl empfangen: [schneidenStartet]
[... 10 Sekunden warten ...]
Arduino antwortet>> !!! WARNUNG: Pin7 Timeout - kein Signal von LOGO-SPS !!!
Arduino antwortet>> schneidenBeendet_
[DEBUG] Befehl empfangen: [schneidenBeendet]
[DEBUG] schneidenBeendet empfangen - Aktiviere Buttons
```

### Nach erfolgreichem Test

1. **Debug-Ausgaben entfernen** (optional):
   - `[DEBUG] Befehl empfangen:` in MainWindow.xaml.cs
   - `[DEBUG] schneidenBeendet empfangen` in MainWindow.xaml.cs
   - Pin7 Debug-Meldungen können bleiben (hilfreich für Diagnose)

2. **Git Commit erstellen**:
   ```bash
   git add MainWindow.xaml.cs
   git add IoT/sketche/SchneidMaschine/SchneidMaschine.ino
   git commit -m "[FIX] Kritische Bugs behoben

   - ArgumentOutOfRangeException beim Verbinden (Empty String Check)
   - Keine TextBox Rückmeldungen (Rückgabewert verwenden)
   - schneidenBeendet Timing (Pin7 Statuswechsel-Erkennung)
   - Debug-Ausgaben für Fehlersuche hinzugefügt
   - 10 Sekunden Timeout als Sicherheit"
   ```

3. **TODO.md aktualisieren**:
   - Status von ⚠️ auf ✅ ändern nach erfolgreichem Hardware-Test

---

## 🔍 Erkenntnisse & Lessons Learned

### 1. Serial Communication ist fehleranfällig

**Problem**: Fragmentierte Nachrichten, PING dazwischen
**Lösung**: Sauberes Buffer-Management mit `.Clear()` nach vollständiger Nachricht

### 2. INPUT_PULLUP Pins sind immer HIGH

**Problem**: Pin 7 mit `INPUT_PULLUP` → immer HIGH wenn nicht auf LOW gezogen
**Lösung**: Auf Statuswechsel warten (LOW → HIGH), nicht nur auf HIGH

### 3. Rückgabewerte verwenden statt Seiteneffekte

**Problem**: Methode leert Buffer, dann wird leerer Buffer ausgelesen
**Lösung**: Rückgabewert verwenden statt globalen State zu verändern

### 4. Timeouts sind wichtig

**Problem**: Wenn LOGO-SPS nicht antwortet, hängt App
**Lösung**: 10 Sekunden Timeout implementiert

### 5. Debug-Ausgaben sind Gold wert

**Problem**: Schwer zu debuggen ohne Visibility
**Lösung**: Debug-Ausgaben in C# und Arduino für jeden kritischen Schritt

---

## 📚 Referenzen

- **TODO.md**: Vollständige Dokumentation aller Fixes
- **MainWindow.xaml.cs**: C# App Hauptlogik
- **SchneidMaschine.ino**: Arduino Sketch
- **specs.md**: Offene Features (noch nicht behoben)

---

## ⚠️ Wichtige Hinweise

### Pin 7 (motorRunning)

Die neue Logik **erfordert** ein Signal von der LOGO-SPS:
- LOW = Motor läuft
- HIGH = Motor fertig

**Ohne LOGO-SPS**: 10 Sekunden Timeout, dann wird trotzdem beendet.

### mmInSteps wurde geändert

**Beobachtung**: User hat `mmInSteps = 13.1` gesetzt (war 12.9, davor 12.7)
→ Dies ist eine Kalibrierung und wurde beibehalten.

### Leerzeichen-Problem nicht behoben

"Schrittmotorstarten..." vs "Schrittmotor starten..." ist **noch offen**.
Ursache: C# Regex entfernt Leerzeichen bei fragmentierten Nachrichten.
Priorität: Niedrig (kosmetisches Problem)

---

---

## 🔍 Hardware-Test Ergebnis (15:30 - 16:30 Uhr)

### Test mit Multimeter

**Beobachtung**: Mit angeschlossenem Multimeter funktioniert der Fix!

**Test-Aufbau**:
```
LOGO-SPS Q3 -----> Arduino Pin 7 -----> Multimeter
```

**Ergebnis**:
- ✅ Multimeter zeigt: 5V → 0V → 5V (korrekt!)
- ✅ Arduino Serial Monitor zeigt:
  ```
  ~schneidenStartet_@
  ~DEBUG: Pin7 VOR Schnitt = 1@
  ~DEBUG: Pin7 LOW - Motor läuft@
  ~DEBUG: Pin7 HIGH - Motor fertig@
  ~schneidenBeendet_@
  ```
- ✅ **Fix funktioniert perfekt mit Multimeter!**

### Test ohne Multimeter

**Ergebnis**:
- ❌ Timeout nach 10 Sekunden
- ❌ "DEBUG: Pin7 LOW - Motor läuft" erscheint **NIE**
- ❌ Nur Timeout-Warnung

**Arduino Serial Monitor**:
```
~schneidenStartet_@
~!!! WARNUNG: Pin7 Timeout - kein Signal von LOGO-SPS !!!@
~schneidenBeendet_@
```

### Root Cause - HARDWARE Problem

**Das Multimeter wirkt als Pull-Down Widerstand!**

- Multimeter-Eingangswiderstand: ~10 MΩ nach GND
- Dieser stabilisiert den Pin
- Ohne Multimeter: Pin "schwebt" (floating input)
- Arduino liest undefinierte Werte (Rauschen, kapazitive Kopplung)

**Warum?**
- Pin 7 ist jetzt `INPUT` (ohne internen Pull-Up)
- LOGO-SPS schaltet zwar 0V/5V
- Aber: Pin ist hochohmig → empfindlich für Rauschen
- **Ein externer Pull-Down Widerstand ist erforderlich!**

### Hardware-Lösung

**Benötigt**: 10kΩ Pull-Down Widerstand

```
Hardware-Schaltung:

LOGO-SPS Q3 Ausgang -----> Arduino Pin 7
                              |
                            [10kΩ]  ← Pull-Down Widerstand
                              |
                             GND
```

**Empfohlener Widerstandswert**: 10 kΩ (alternativ: 4,7 kΩ - 47 kΩ)
**Farbcode**: Braun-Schwarz-Orange

**Funktionsweise**:
- LOGO-SPS HIGH (5V): Überschreibt Pull-Down → Arduino liest HIGH ✅
- LOGO-SPS LOW (0V): Pull-Down zieht Pin auf GND → Arduino liest LOW ✅
- Ohne Signal: Pull-Down hält Pin auf LOW (definierter Zustand)
- Strombelastung LOGO: 5V / 10kΩ = 0,5 mA (akzeptabel)

---

## 📝 Finale Code-Änderungen

### Nach Hardware-Test hinzugefügt

**SchneidMaschine.ino - Zusätzliche Debug-Ausgabe** (Zeile 292-293):
```cpp
// In schneiden() - zeige Pin7 Status VOR dem Schnitt
int pinStatusVor = digitalRead(motorRunning);
sendText("DEBUG: Pin7 VOR Schnitt = " + String(pinStatusVor) + " (erwarte 1=HIGH)");
```

Diese Debug-Ausgabe hilft zu erkennen, ob:
- Pin 7 korrekt HIGH liest (erwarteter Zustand vor Schnitt)
- Pull-Down Widerstand fehlt (Pin liest undefinierte Werte)
- LOGO-SPS nicht angeschlossen ist (Pin liest LOW wegen Pull-Down)

---

## 📊 Finale Zusammenfassung

### Geänderte Dateien

**C# App** (MainWindow.xaml.cs):
- 6 Methoden geändert
- ~30 Zeilen Code
- Empty String Checks
- Rückgabewerte korrekt verwendet
- Debug-Ausgaben

**Arduino Sketch** (SchneidMaschine.ino):
- 1 KRITISCHE pinMode Änderung: `INPUT_PULLUP` → `INPUT`
- 2 neue Variablen (Statuswechsel-Tracking)
- 2 Funktionen komplett überarbeitet
- ~70 Zeilen Code geändert
- Zusätzliche Debug-Ausgaben

**Hardware**:
- ⚠️ **ERFORDERLICH**: 10kΩ Pull-Down Widerstand (Pin 7 → GND)
- Ohne diesen Widerstand funktioniert der Fix nicht!

### Lessons Learned (Ergänzung)

**6. Floating Inputs sind unzuverlässig**

**Problem**: INPUT ohne Pull-Up/Pull-Down → Pin schwebt
**Symptom**: Funktioniert mit Multimeter, nicht ohne
**Lösung**: Immer externen Pull-Up oder Pull-Down verwenden

**7. INPUT_PULLUP kann LOW-Signale blockieren**

**Problem**: Interner Pull-Up (20-50kΩ) verhindert LOW-Erkennung
**Symptom**: Pin liest immer HIGH, obwohl externe Quelle LOW sendet
**Lösung**: INPUT verwenden + externen Pull-Down (wenn externe Quelle Push-Pull ist)

---

**Session Ende**: 10.12.2025, ~16:30 Uhr
**Status**: ✅ Code fertig, ⚠️ **HARDWARE-ÄNDERUNG ERFORDERLICH**
**Nächster Schritt**:
1. 10kΩ Pull-Down Widerstand einbauen (Pin 7 → GND)
2. Arduino Sketch hochladen
3. Hardware-Test mit LOGO-SPS durchführen
