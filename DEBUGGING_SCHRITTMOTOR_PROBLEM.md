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

## Diagnose-Fragen (für nächste Session)

1. **Tritt das Problem nur bei langen Bewegungen auf?** (z.B. > 1000 Steps)
2. **Siehst du in der Konsole immer "stepperFinished" Ausgaben?**
3. **Passiert es zufällig oder immer bei bestimmten Längen?**
4. **Wie ist der Arduino mit dem PC verbunden?** (USB-Kabel-Länge? Über USB-Hub?)
5. **Liegt der Arduino physisch nah am Schrittmotor-Treiber?**

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

## Nächste Schritte

1. Log-Ausgabe analysieren: Kommt `stepperFinished_` wirklich NICHT an, oder wird es nicht verarbeitet?
2. Debug-Ausgaben in C# App hinzufügen um zu sehen, ob Nachricht empfangen wird
3. Hardware-Setup überprüfen
4. Eine der Software-Lösungen implementieren und testen

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
