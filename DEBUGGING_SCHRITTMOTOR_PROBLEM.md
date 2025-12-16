# Debugging: Schrittmotor "stepperFinished" Problem

**Datum:** 15.12.2025

## Problem-Beschreibung

Der Befehl `stepperFinished_` kommt nicht immer bei der SchneidMaschine App an, und die Buttons bleiben deaktiviert.

Ähnliches Problem gab es bereits beim Schneiden - wurde durch einen fehlenden Widerstand in der Schaltung gelöst.

**Frage:** Ist es ein Hardware-Problem (fehlender Widerstand) oder ein Software-Problem?

---

## Arduino Pin-Belegung (SchneidMaschine.ino)

### Handrad
- **Pin 13 (A)**: Handrad - Signal A
- **Pin 12 (B)**: Handrad - Signal B

### Schrittmotor-Treiber
- **Pin 4 (puls)**: Schrittimpulse an Treiber
- **Pin 5 (dir)**: Drehrichtung (HIGH=vorwärts, LOW=rückwärts)
- **Pin 6 (enable)**: Aktivierung des Schrittmotor-Treibers

### Schneidemechanismus
- **Pin 10 (cut)**: Relay - Schneiden (Ausgang)
- **Pin 8 (cutTaster)**: Taster - Schneiden (Eingang)

### LOGO-SPS Signal
- **Pin 7 (motorRunning)**: Signal von LOGO-SPS
  - **LOW (0V)**: Motor läuft gerade (Schnitt in Arbeit)
  - **HIGH (5V)**: Motor ist fertig (Schnitt beendet)
  - Wird in `motorFinished()` überwacht (Zeile 102)
  - Timeout: 10 Sekunden

---

## Kommunikation Arduino ↔ C# App

**WICHTIG:** Kommunikation läuft NICHT über Pins, sondern über **serielle Schnittstelle (USB)**!

### Arduino → C# App (Senden)
- `Serial.begin(9600);` - öffnet serielle Verbindung
- `sendCommand()` Funktion (Zeile 443-449)
- Beispiel: `sendCommand("stepperFinished_" + String(stepCounter), true);` (Zeile 233-234)

### C# App → Arduino (Empfangen)
- `dataReceived()` Funktion (Zeile 156-269)
- Beispiel Befehl: `stepperStart_300_forward`
- Wird verarbeitet in Zeile 223-235

---

## Code-Analyse: Potentielles Problem

### Arduino Code (SchneidMaschine.ino)

```cpp
void stepper(unsigned long steps, String drehRichtung) {
    // ... Richtung setzen

    for(int i = 0; i < steps; i++) {
        isAllesStop();  // ← PROBLEM: Liest WÄHREND der Bewegung Serial-Daten!

        // Steps ausführen
        ++stepCounter;
        digitalWrite(puls, HIGH);
        delayMicroseconds(500);
        digitalWrite(puls, LOW);
        delayMicroseconds(500);
    }
}

// NACH der Schleife:
sendCommand("stepperFinished_" + String(stepCounter), true);
```

### Mögliche Ursachen

1. **Serial Buffer Overflow**
   - Bei langen Bewegungen (viele Steps) kann der Serial-Buffer überlaufen
   - `isAllesStop()` liest während der Bewegung kontinuierlich Serial-Daten

2. **Gleichzeitige Serial-Zugriffe**
   - `isAllesStop()` liest Serial
   - Dann wird `stepperFinished_` gesendet
   - Möglicher Konflikt

3. **Hardware-Interferenz (EMI)**
   - Laufender Schrittmotor erzeugt elektrische Störungen
   - Diese können USB/Serial-Kommunikation stören
   - Besonders bei langen Bewegungen

---

## Log-Ausgabe der SchneidMaschine App

```
try to Connect with Arduino....

Arduino antwortet>> Connected
Arduino antwortet>> handradOff_
Arduino antwortet>> steps_4064
✓ Board identifiziert: Schneidmaschine an COM5

Arduino antwortet>> DEBUG: Pin7VORSchnitt= 1(erwarte 1=HIGH)
Arduino antwortet>> DEBUG: Pin7LOW- Motorl??uft
Arduino antwortet>> DEBUG: Pin7HIGH- Motorfertig
Arduino antwortet>> schneidenBeendet_
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_4064
Arduino antwortet>> schneidenStartet_
Arduino antwortet>> DEBUG: Pin7 VOR Schnitt =1(erwarte1=HIGH)
Arduino antwortet>> DEBUG:Pin7 LOW -Motor l??uft
Arduino antwortet>> DEBUG: Pin7 HIGH- Motorfertig
Arduino antwortet>> schneidenBeendet_
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_4064
Arduino antwortet>> DEBUG:Pin7VORSchnitt= 1(erwarte 1=HIGH)
Arduino antwortet>> DEBUG: Pin7LOW- Motorl??uft
Arduino antwortet>> schneidenBeendet_
Arduino antwortet>> Schrittmotor starten...
Arduino antwortet>> stepperFinished_4064
Arduino antwortet>> schneidenStartet_
Arduino antwortet>> DEBUG:Pin7 VOR Schnitt =1 (erwarte 1=HIGH)
Arduino antwortet>> DEBUG:Pin7LOW- Motorl??uft
Arduino antwortet>> DEBUG:Pin7 HIGH -Motor fertig
Arduino antwortet>> schneidenBeendet_
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_4064
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_8128
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_8255
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_8382
Arduino antwortet>> Schrittmotor starten...
Arduino antwortet>> stepperFinished_8509
Arduino antwortet>> Schrittmotor starten...
Arduino antwortet>> stepperFinished_8636
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_8763
Arduino antwortet>> Schrittmotorstarten...
Arduino antwortet>> stepperFinished_8890
Arduino antwortet>> DEBUG: Pin7VORSchnitt= 1(erwarte1=HIGH)
Arduino antwortet>> DEBUG: Pin7 LOW -Motor l??uft
Arduino antwortet>> DEBUG: Pin7HIGH- Motorfertig
Arduino antwortet>> schneidenBeendet_
Arduino antwortet>> Schrittmotor starten...
```

### Beobachtungen aus dem Log

1. ✅ `stepperFinished_` Nachrichten kommen an (z.B. 4064, 8128, 8255, etc.)
2. ✅ `schneidenBeendet_` Nachrichten kommen an
3. ⚠️ Seltsame Zeichen bei "l??uft" - möglicherweise Encoding-Problem mit Umlauten
4. ⚠️ Unregelmäßige Abstände zwischen den Nachrichten

---

## Diagnose-Fragen & Erkenntnisse

### ✅ Bestätigt (15.12.2025)
1. **Fehler ist UNABHÄNGIG von der Länge/Steps** - tritt sowohl bei kurzen als auch bei langen Bewegungen auf
2. **Arduino-Sketch funktioniert EINWANDFREI**:
   - Test im Arduino Serial Monitor: ~100 Bewegungen durchgeführt
   - JEDE Bewegung wurde korrekt mit `stepperFinished_` beendet
   - **Fazit**: Hardware und Arduino-Code sind OK
3. **USB-Kabel ist NICHT das Problem**:
   - Gleiches USB-Kabel wurde in alter Version der SchneidMaschine App verwendet
   - Dort gab es das Problem NICHT
   - **Fazit**: Das Problem liegt in der C# App, nicht in der Hardware

### 🔍 Neue Erkenntnis
**Das Problem muss in der C# App liegen!**
- Arduino sendet `stepperFinished_` korrekt (im Serial Monitor bestätigt)
- Hardware ist in Ordnung (altes USB-Kabel funktionierte früher)
- → Die C# App empfängt oder verarbeitet die Nachricht nicht korrekt

### ❓ Noch zu klären
4. **Kommt `stepperFinished_` in der C# App an, wird aber nicht verarbeitet?**
5. **Oder kommt die Nachricht gar nicht erst in der C# App an?**
6. **Gab es Änderungen in der Serial-Empfangs-Logik der C# App?**

---

## Hardware-Checks (für nächste Session)

- [ ] **Masse-Verbindung**: Arduino GND mit Schrittmotor-Treiber GND verbunden?
- [ ] **USB-Kabel**: Kurzes, geschirmtes USB-Kabel verwenden
- [ ] **Ferrite-Ring**: Am USB-Kabel gegen EMI-Störungen
- [ ] **Abstand**: Arduino möglichst weit weg vom Schrittmotor-Treiber
- [ ] **Widerstand**: Fehlt ein Pull-Up/Pull-Down Widerstand wie beim Schneiden-Problem?

---

## Lösungsansätze (ToDo für nächste Session)

### Software-Lösungen

1. **Serial Buffer erhöhen**
   ```cpp
   Serial.setRxBufferSize(256); // vor Serial.begin()
   ```

2. **isAllesStop() optimieren**
   - Nicht bei jedem Step aufrufen
   - Nur alle X Steps prüfen

3. **Acknowledge-System**
   - C# App sendet Bestätigung zurück
   - Arduino wartet auf Bestätigung

4. **Timeout-Mechanismus in C# App**
   - Wenn `stepperFinished_` nicht ankommt, nachfragen

### Hardware-Lösungen

1. **Ferrite-Ring** am USB-Kabel (gegen EMI)
2. **Geschirmtes USB-Kabel** verwenden
3. **Pull-Down Widerstand** an Serial-Pins (falls nötig)
4. **Optokoppler** zur galvanischen Trennung
5. **Separate Stromversorgung** für Arduino und Schrittmotor-Treiber

---

## Code-Analyse der C# App (16.12.2025)

### Serial-Kommunikations-Flow

1. **`port_DataReceived_Schneidmaschine()`** (Zeile 1084)
   - Empfängt Rohdaten vom Arduino
   - Ruft `SetTextSchneidmaschine()` auf

2. **`stringToCharSchneidmaschine()`** (Zeile 1222)
   - Sammelt Zeichen zwischen START_CHAR (`~` oder `%`) und END_CHAR (`@`)
   - Verwendet `befehlBuilderSchneidmaschine` als Buffer
   - **WICHTIG**: Gibt `null` zurück, wenn kein vollständiger Befehl empfangen wurde!
   - Problem: Wenn Serial-Daten in mehreren Paketen ankommen und das `@` fehlt, wird der Befehl ignoriert

3. **`handleCommandLineSchneidmaschine()`** (Zeile 828)
   - Prüft, ob Text mit `@` endet
   - Wenn ja: `commandReceivedSchneidmaschine()` aufrufen
   - Wenn nein: Befehl wird ignoriert

4. **`commandReceivedSchneidmaschine()`** (Zeile 857)
   - Teilt den Befehl an `_` auf
   - Sucht nach passenden COMMAND_Schneidmaschine Enum

5. **`commandRunSchneidmaschine()`** (Zeile 952)
   - COMMAND_Schneidmaschine.stepperFinished
   - Setzt `dataModel.IsStepperFinished = true`
   - Ruft `StackPanelControlsEnable()` auf (nur wenn EinzelSchritt sichtbar ist!)

### Potentielle Probleme

1. **Serial Buffer Fragmentierung**
   - Wenn `stepperFinished_4064@` in mehreren Paketen ankommt
   - z.B.: Paket 1: `~stepperFin`, Paket 2: `ished_4064@`
   - Dann wird beim ersten Paket `null` zurückgegeben → Befehl ignoriert

2. **Timing-Problem**
   - Befehl kommt an, aber `EinzelSchritt.IsVisible = false`
   - Dann werden Buttons NICHT aktiviert!

### Debug-Ausgaben hinzugefügt (16.12.2025)

✅ **In `stringToCharSchneidmaschine()`:**
- Input-Text und Buffer-Zustand vor Verarbeitung
- Erkennung von START_CHAR und END_CHAR
- Warnung bei unvollständigem Befehl
- Return-Wert (null oder vollständiger Befehl)

✅ **In `SetTextSchneidmaschine()`:**
- Warnung wenn processedText = null (Befehl wird ignoriert)
- Anzeige des verarbeiteten Texts

✅ **In `stepperFinished` Case:**
- Ausführliche Ausgabe wenn stepperFinished empfangen wird
- Anzeige der Steps
- Prüfung ob EinzelSchritt sichtbar ist
- Warnung falls Buttons nicht aktiviert werden

### Test-Anleitung

1. **App neu kompilieren** mit den Debug-Ausgaben
2. **Schrittmotor mehrmals bewegen** (z.B. 10-20x)
3. **Konsolen-Output analysieren**:
   - Kommt `stepperFinished` in `stringToCharSchneidmaschine()` vollständig an?
   - Wird `processedText = null` gesetzt?
   - Wird `COMMAND_Schneidmaschine.stepperFinished` aufgerufen?
   - Ist `EinzelSchritt.IsVisible = true`?

4. **Suche nach diesen Mustern**:
   ```
   [stringToCharSchneidmaschine] WARNUNG: Unvollständiger Befehl
   [SetTextSchneidmaschine] processedText ist NULL
   ⚠ WARNUNG: EinzelSchritt ist NICHT sichtbar
   ```

---

## Nächste Schritte

1. ✅ Code-Analyse durchgeführt
2. ✅ Debug-Ausgaben hinzugefügt
3. ✅ App neu kompiliert und getestet
4. ✅ Konsolen-Output analysiert - Ursache gefunden!
5. ✅ Software-Lösung implementiert

---

## 🔍 PROBLEM GEFUNDEN! (16.12.2025)

### Root Cause Analysis

**Das Problem lag im Arduino-Code, nicht in der C# App!**

#### Der Bug:

In `SchneidMaschine.ino` gibt es **ZWEI Funktionen**, die die **gleiche globale Variable `appendSerialData`** verwenden:

1. **`dataReceived()`** (Zeile 160)
   - Wird in der `loop()` aufgerufen
   - Liest Serial-Daten und verarbeitet Befehle

2. **`isAllesStop()`** (Zeile 361 - ALT)
   - Wird **bei jedem Step** während der Schrittmotor-Bewegung aufgerufen (Zeile 397)
   - Liest Serial-Daten um auf "allesStop" zu prüfen
   - **PROBLEM**: Löscht **ALLE** empfangenen Befehle, nicht nur "allesStop"!

#### Was genau passiert:

1. User klickt Button → `%stepperStart_127_forward#` wird gesendet
2. Arduino startet Bewegung, ruft `stepper()` auf
3. **WÄHREND der Bewegung** klickt User **NOCHMAL** auf Button
4. Neuer `%stepperStart_127_forward#` wird gesendet
5. `isAllesStop()` empfängt diesen Befehl in **jedem Step**
6. **Zeile 378 (ALT)**: `appendSerialData = "";` - **LÖSCHT DEN BEFEHL!**
7. Bewegung endet, Arduino sendet `stepperFinished_`
8. C# App empfängt `stepperFinished_`, aktiviert Buttons
9. **ABER**: Der zweite `stepperStart` Befehl ist **VERLOREN**!
10. C# App wartet auf Bestätigung, aber Arduino wartet auf neuen Befehl
11. → **Race Condition**: Buttons bleiben disabled!

#### Code-Stelle (ALT):

```cpp
void isAllesStop() {
    while(Serial.available() > 0) {
        c = Serial.read();
        appendSerialData += c;  // ← Liest in gleiche Variable wie dataReceived()
    }

    if(c == '#') {
        // ... Befehl verarbeiten

        if(befehl.equals("allesStop")) {
            allesStoppen = true;
        }

        appendSerialData = "";  // ← LÖSCHT ALLE Befehle, auch stepperStart!
        c = 0;
    }
}
```

#### Warum es zufällig auftritt:

- Tritt nur auf, wenn User **während der Bewegung** einen Button klickt
- Je schneller der User klickt, desto höher die Wahrscheinlichkeit
- Unabhängig von der Länge der Bewegung

---

## ❌ ERSTE LÖSUNG WAR FALSCH (16.12.2025)

### Fix im Arduino-Code (NICHT DIE URSACHE!)

**Datei**: `IoT/sketche/SchneidMaschine/SchneidMaschine.ino`

Zunächst wurde ein Fix im Arduino-Code implementiert (separate Buffer für `isAllesStop()`), aber das Problem trat weiterhin auf.

---

## ✅ ECHTE LÖSUNG GEFUNDEN UND IMPLEMENTIERT (16.12.2025)

### Das eigentliche Problem: Bug in der C# App!

**Datei**: `MainWindow.xaml.cs`

**Root Cause**: Wenn **zwei Befehle in einem Serial-Paket** ankommen (z.B. `~stepperFinished_635@%PING@`), wurden Befehle gelöscht!

#### Der Bug in `stringToCharSchneidmaschine()` (MainWindow.xaml.cs Zeile 1237)

**ALT (Buggy Code):**

```csharp
foreach (char ch in charArr)
{
    // Start der Commandline
    if (ch.Equals('%') || ch.Equals((char)CharArduino.START_CHAR))
    {
        newText = null;  // ← HIER! Löscht den vorherigen vollständigen Befehl!
        befehlBuilderSchneidmaschine.Clear();
    }

    befehlBuilderSchneidmaschine.Append(ch);

    // Ende der Commandline
    if (ch.Equals((char)CharArduino.END_CHAR))
    {
        newText = befehlBuilderSchneidmaschine.ToString();
        befehlBuilderSchneidmaschine.Clear();
        // ← FEHLER: Schleife läuft weiter und überschreibt newText!
    }
}
```

**Was passiert bei `~stepperFinished_635@%PING@`:**
1. `stepperFinished_635@` wird erkannt → `newText` gesetzt ✅
2. Dann kommt `%` (START_CHAR für PING)
3. **`newText = null`** → Erster Befehl wird **GELÖSCHT**! ❌
4. Funktion returned `null` → `stepperFinished` wird **IGNORIERT** ❌

---

#### Der Fix (MainWindow.xaml.cs Zeile 1237-1295)

**NEU (Funktionierender Code):**

```csharp
foreach (char ch in charArr)
{
    // Start der Commandline
    if (ch.Equals('%') || ch.Equals((char)CharArduino.START_CHAR))
    {
        // WICHTIG: Wenn wir bereits einen vollständigen Befehl haben,
        // NICHT überschreiben!
        if (newText != null)
        {
            // Speichere das START_CHAR für den nächsten Aufruf
            befehlBuilderSchneidmaschine.Append(ch);
            break; // ← Verlasse die Schleife, returne den aktuellen Befehl
        }

        befehlBuilderSchneidmaschine.Clear();
    }

    befehlBuilderSchneidmaschine.Append(ch);

    // Ende der Commandline
    if (ch.Equals((char)CharArduino.END_CHAR))
    {
        newText = befehlBuilderSchneidmaschine.ToString();
        befehlBuilderSchneidmaschine.Clear();

        // WICHTIG: Nach vollständigem Befehl SOFORT stoppen!
        break; // ← Weitere Zeichen werden im nächsten Aufruf verarbeitet
    }
}
```

**Was jetzt bei `~stepperFinished_635@%PING@` passiert:**
1. `stepperFinished_635@` wird erkannt → `newText` gesetzt ✅
2. `break` → Schleife wird verlassen ✅
3. Funktion returned `stepperFinished_635` ✅
4. Nächster Aufruf verarbeitet `%PING@` ✅

---

#### Gleicher Fix auch in `stringToCharRollenzentrierung()` (Zeile 1205-1248)

Um konsistent zu sein und potentielle Probleme zu vermeiden, wurde der gleiche Fix auch in der Rollenzentrierung-Funktion implementiert.

---

### Wie die Lösung funktioniert:

1. **Break nach vollständigem Befehl**: Sobald ein Befehl vollständig ist (`@` erkannt), verlasse Schleife mit `break`
2. **Schutz vor Überschreiben**: Wenn `newText` bereits gesetzt ist und neuer START_CHAR kommt, `break` statt löschen
3. **Nächster Aufruf**: Restliche Zeichen bleiben im Buffer und werden beim nächsten `port_DataReceived` Event verarbeitet

---

## ✅ TEST ERFOLGREICH! (16.12.2025)

**Test durchgeführt**: Ca. 700 Bewegungen, auch schnell hintereinander
**Ergebnis**: ✅ **KEIN EINZIGER FEHLER!**
**Fazit**: Problem ist gelöst! 🎉

### Test-Ergebnisse:

1. ✅ **Test 1**: Normal klicken - funktioniert einwandfrei
2. ✅ **Test 2**: Schnell hintereinander klicken - alle Befehle werden verarbeitet
3. ✅ **Test 3**: Stress-Test (700x) - keine Buttons bleiben disabled
4. ✅ **Test 4**: Während Bewegung klicken - nächste Bewegung startet korrekt

---

## Code-Referenzen

- Arduino: `IoT/sketche/SchneidMaschine/SchneidMaschine.ino`
  - `stepper()` Funktion: Zeile 383
  - `sendCommand()` Funktion: Zeile 443
  - `isAllesStop()` Funktion: Zeile 357

- C# App: `MainWindow.xaml.cs`
  - `port_DataReceived_Schneidmaschine()`: Zeile 1084
  - `SetTextSchneidmaschine()`: Zeile 764
  - `handleCommandLineSchneidmaschine()`: Zeile ~900
  - `COMMAND_Schneidmaschine.stepperFinished`: Zeile 952
