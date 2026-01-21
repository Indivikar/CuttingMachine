# PLAN - Manuelle Rollenzentrierung mit Schrittmotor-Synchronisation

**Projekt**: SchneidMaschine - Rollenzentrierung Erweiterung
**Datum**: 15. Januar 2026
**Status**: 📋 PLANUNG
**Branch**: dev

---

## 🎯 Ziel der Erweiterung

Die Rollenzentrierung soll erweitert werden, um:
1. **Manuelle Steuerung** über 2 Taster (unabhängig von Sensoren)
2. **Synchronisation mit Schneidmaschine**: Automatische Bewegung (durch Sensoren) nur wenn Schrittmotor der Schneidmaschine läuft
3. **Test-Sketche** erstellen, um die neuen Funktionen im Büro zu testen (ohne echte Hardware)

---

## 📋 Anforderungen

### 1. Hardware-Anforderungen

#### ESP32 (Rollenzentrierung)
- **2x Taster** für manuelle Steuerung:
  - Pin 32: Taster LINKS
  - Pin 33: Taster RECHTS
  - Logik: Pull-Down (HIGH = gedrückt)
  - Empfohlener Widerstand: **10kΩ Pull-Down** (zwischen Pin und GND)
  - Taster schaltet Pin auf VCC (3.3V)

- **1x Eingangspin** für Schrittmotor-Status von Arduino:
  - Pin 34: Signal vom Arduino Pin 9 (HIGH = Schrittmotor läuft)
  - Pin 34 ist Input-Only beim ESP32 → perfekt für diese Anwendung
  - Kein Pull-Up/Pull-Down nötig (Arduino sendet aktiv HIGH/LOW)

#### Arduino (Schneidmaschine)
- **1x Ausgangspin** für Schrittmotor-Status:
  - Pin 9: Sendet HIGH wenn Schrittmotor läuft, LOW wenn Schrittmotor steht
  - Wird in `stepper()` Funktion gesetzt

#### Schaltplan

```
Hardware-Verbindung:

Arduino Pin 9 -----> ESP32 Pin 34
      (OUTPUT)         (INPUT)

ESP32 Pin 32 <----- Taster LINKS
      |
   [10kΩ]  ← Pull-Down
      |
     GND

Taster LINKS: zwischen Pin 32 und VCC (3.3V)

ESP32 Pin 33 <----- Taster RECHTS
      |
   [10kΩ]  ← Pull-Down
      |
     GND

Taster RECHTS: zwischen Pin 33 und VCC (3.3V)
```

**Empfohlene Widerstände für Taster:**
- **10kΩ Pull-Down Widerstand** (Farbcode: Braun-Schwarz-Orange)
- Alternativ: 4,7kΩ - 22kΩ (je kleiner, desto weniger anfällig für Störungen)
- **Nicht verwenden**: > 47kΩ (zu hochohmig, anfällig für Rauschen)

---

### 2. Software-Anforderungen

#### Bounce2 Library
- **Für Taster**: Entprellen der manuellen Taster (Standard-Anwendung)
- **Für Sensoren**: Entprellen der VL53L0X Sensor-Signale (simuliert durch Taster im Test)
  - Verhindert, dass Vibrationen oder schnelle Bewegungen Fehlmessungen auslösen
  - Stabilisiert die Trigger-Erkennung

#### Neue Funktionen

**ESP32 (Rollenzentrierung):**
1. `checkManualButtons()` - Prüft Taster-Status und bewegt Motor manuell
2. `isStepperRunning()` - Prüft ob Arduino-Schrittmotor läuft (Pin 34)
3. Anpassung `loop()` - Sensor-Bewegung nur wenn Schrittmotor läuft
4. Bounce2 Integration für Taster und Sensor-Simulation

**Arduino (Schneidmaschine):**
1. Pin 9 Steuerung in `stepper()` - HIGH am Anfang, LOW am Ende
2. Pin 9 Initialisierung in `setup()`

---

## 📝 Detaillierte Spezifikation

### ESP32 - Rollenzentrierung_Taster_Test.ino

#### Neue Variablen
```cpp
// Pin-Definitionen
const int BUTTON_LEFT = 32;        // Taster LINKS
const int BUTTON_RIGHT = 33;       // Taster RECHTS
const int STEPPER_SIGNAL = 34;     // Signal vom Arduino (HIGH = Schrittmotor läuft)

// Bounce2 Objekte für Taster (manuelle Steuerung)
Bounce buttonLeft = Bounce();
Bounce buttonRight = Bounce();

// Bounce2 Objekte für Sensor-Simulation (Taster statt echte Sensoren)
Bounce sensor1Sim = Bounce();      // Simuliert VL53L0X Sensor 1
Bounce sensor2Sim = Bounce();      // Simuliert VL53L0X Sensor 2

// Status-Variablen
bool stepperIsRunning = false;     // Läuft der Arduino-Schrittmotor?
```

#### Neue Funktionen

**1. setup() - Initialisierung**
```cpp
void setup() {
  // Taster-Pins konfigurieren
  pinMode(BUTTON_LEFT, INPUT);   // Pull-Down extern (10kΩ)
  pinMode(BUTTON_RIGHT, INPUT);  // Pull-Down extern (10kΩ)
  pinMode(STEPPER_SIGNAL, INPUT); // Signal vom Arduino

  // Bounce2 Initialisierung für manuelle Taster
  buttonLeft.attach(BUTTON_LEFT);
  buttonLeft.interval(50);  // 50ms Entprellzeit

  buttonRight.attach(BUTTON_RIGHT);
  buttonRight.interval(50);

  // Bounce2 Initialisierung für Sensor-Simulation
  sensor1Sim.attach(SENSOR1_SIM_PIN);  // z.B. Pin 35
  sensor1Sim.interval(50);

  sensor2Sim.attach(SENSOR2_SIM_PIN);  // z.B. Pin 36
  sensor2Sim.interval(50);

  // TMC2209 Pins wie bisher...
}
```

**2. checkManualButtons() - Manuelle Taster-Steuerung**
```cpp
void checkManualButtons() {
  // Taster-Status aktualisieren
  buttonLeft.update();
  buttonRight.update();

  // TASTER LINKS gedrückt → nach LINKS bewegen
  if (buttonLeft.rose()) {  // rose() = Flanke LOW→HIGH (Taster gedrückt)
    Serial.println("Taster LINKS gedrückt - bewege nach LINKS");

    if (motorIsMoving) {
      Serial.println("Motor bewegt sich bereits - ignoriere Taster");
      return;
    }

    // Manuelle Bewegung hat VORRANG - ignoriert Schrittmotor-Status
    motorIsMoving = true;
    enableMotor();
    rotateMotorFixedSteps(TOTAL_STEPS, LEFT_DIRECTION);
    disableMotor();
    motorIsMoving = false;

    Serial.println("Manuelle Bewegung nach LINKS abgeschlossen");
  }

  // TASTER RECHTS gedrückt → nach RECHTS bewegen
  if (buttonRight.rose()) {
    Serial.println("Taster RECHTS gedrückt - bewege nach RECHTS");

    if (motorIsMoving) {
      Serial.println("Motor bewegt sich bereits - ignoriere Taster");
      return;
    }

    // Manuelle Bewegung hat VORRANG
    motorIsMoving = true;
    enableMotor();
    rotateMotorFixedSteps(TOTAL_STEPS, RIGHT_DIRECTION);
    disableMotor();
    motorIsMoving = false;

    Serial.println("Manuelle Bewegung nach RECHTS abgeschlossen");
  }
}
```

**3. isStepperRunning() - Schrittmotor-Status prüfen**
```cpp
bool isStepperRunning() {
  // Pin 34 lesen: HIGH = Schrittmotor läuft, LOW = Schrittmotor steht
  stepperIsRunning = digitalRead(STEPPER_SIGNAL) == HIGH;
  return stepperIsRunning;
}
```

**4. loop() - Hauptschleife mit Sensor-Simulation**
```cpp
void loop() {
  // 1. Befehle von C# App empfangen (wie bisher)
  dataReceived();

  // 2. IMMER: Manuelle Taster prüfen (unabhängig vom Schrittmotor-Status)
  checkManualButtons();

  // 3. Schrittmotor-Status prüfen
  bool stepperRunning = isStepperRunning();

  // 4. Sensor-Simulation: Taster statt echte VL53L0X Sensoren
  //    Update Bounce2 Objekte
  sensor1Sim.update();
  sensor2Sim.update();

  // Simuliere Sensor-Trigger: Taster gedrückt = Sensor unter Schwellenwert
  bool sensor1Triggered = sensor1Sim.read() == HIGH;  // HIGH = gedrückt
  bool sensor2Triggered = sensor2Sim.read() == HIGH;

  // 5. NUR wenn Schrittmotor läuft: Sensor-basierte Bewegung erlauben
  if (stepperRunning && !motorIsMoving) {

    // Verhindere gleichzeitigen Trigger beider Sensoren
    if (sensor1Triggered && sensor2Triggered) {
      Serial.println("WARNUNG: Beide Sensoren gleichzeitig - KEINE BEWEGUNG");
      sensor1TriggerCount = 0;
      sensor2TriggerCount = 0;
      return;
    }

    // Sensor 1 Trigger-Count erhöhen
    if (sensor1Triggered) {
      sensor1TriggerCount++;
      Serial.println("Sensor 1 getriggert [" + String(sensor1TriggerCount) +
                     "/" + String(SENSOR_TRIGGER_COUNT) + "]");

      if (sensor1TriggerCount >= SENSOR_TRIGGER_COUNT) {
        Serial.println("Sensor 1: " + String(SENSOR_TRIGGER_COUNT) +
                       "x getriggert - bewege nach RECHTS");
        motorIsMoving = true;
        respondToSensor(0, RIGHT_DIRECTION);  // distance ignoriert im Test
      }
    } else {
      sensor1TriggerCount = 0;
    }

    // Sensor 2 Trigger-Count erhöhen
    if (sensor2Triggered) {
      sensor2TriggerCount++;
      Serial.println("Sensor 2 getriggert [" + String(sensor2TriggerCount) +
                     "/" + String(SENSOR_TRIGGER_COUNT) + "]");

      if (sensor2TriggerCount >= SENSOR_TRIGGER_COUNT) {
        Serial.println("Sensor 2: " + String(SENSOR_TRIGGER_COUNT) +
                       "x getriggert - bewege nach LINKS");
        motorIsMoving = true;
        respondToSensor(0, LEFT_DIRECTION);
      }
    } else {
      sensor2TriggerCount = 0;
    }

  } else if (!stepperRunning) {
    // Schrittmotor läuft NICHT → Sensor-Bewegung DEAKTIVIERT
    // Counter zurücksetzen
    sensor1TriggerCount = 0;
    sensor2TriggerCount = 0;
    disableMotor();
  }

  delay(LOOP_DELAY);
}
```

**Wichtige Anpassungen:**
- VL53L0X Bibliothek NICHT einbinden (Sensoren werden durch Taster simuliert)
- `initializeSensors()` ENTFERNEN oder durch Taster-Initialisierung ersetzen
- `calibrateSystem()` ENTFERNEN (nicht für Test nötig)
- `respondToSensor()` vereinfachen (keine echten Distanz-Messungen)

---

### Arduino - SchneidMaschine_Taster_Test.ino

#### Neue Variablen
```cpp
const int STEPPER_STATUS_PIN = 9;  // Signal an ESP32: HIGH = Schrittmotor läuft
```

#### setup() Ergänzung
```cpp
void setup() {
  Serial.begin(9600);

  // Pin 9 als Ausgang für Schrittmotor-Status
  pinMode(STEPPER_STATUS_PIN, OUTPUT);
  digitalWrite(STEPPER_STATUS_PIN, LOW);  // Initial: Schrittmotor steht

  // ... restliche Pins wie bisher ...
}
```

#### stepper() Anpassung
```cpp
void stepper(unsigned long steps, String drehRichtung) {
  // *** NEU: Signal an ESP32 senden - Schrittmotor startet ***
  digitalWrite(STEPPER_STATUS_PIN, HIGH);
  Serial.println("[DEBUG] Pin9 HIGH - Schrittmotor läuft");

  // Richtung setzen
  if(drehRichtung.equals("forward")) {
    digitalWrite(dir, HIGH);
  }
  if(drehRichtung.equals("backward")) {
    digitalWrite(dir, LOW);
  }

  // Schritte ausführen (wie bisher)
  for(int i = 0; i < steps; i++) {
    isAllesStop();
    if(allesStoppen) {
      break;
    }

    if(drehRichtung.equals("forward")) {
      ++stepCounter;
    }
    if(drehRichtung.equals("backward")) {
      --stepCounter;
    }

    digitalWrite(puls, HIGH);
    delayMicroseconds(500);
    digitalWrite(puls, LOW);
    delayMicroseconds(500);
  }

  // *** NEU: Signal an ESP32 senden - Schrittmotor stoppt ***
  digitalWrite(STEPPER_STATUS_PIN, LOW);
  Serial.println("[DEBUG] Pin9 LOW - Schrittmotor steht");
}
```

#### Vereinfachungen für Test-Sketch
- `motorFinished()` ENTFERNEN (Pin 7 LOGO-SPS Überwachung nicht nötig)
- `schneiden()` vereinfachen: nur Relay schalten, KEIN Pin 7 Monitoring
- `tasterSchneiden()` optional (für Test nicht zwingend nötig)
- Handrad-Funktionen KÖNNEN entfernt werden (nicht für Test nötig)

**Vereinfachte schneiden() Funktion:**
```cpp
void schneiden() {
  sendCommand("schneidenStartet_", true);

  digitalWrite(cut, LOW);   // Relay an
  delay(500);
  digitalWrite(cut, HIGH);  // Relay aus

  stepCounter = 0;
  sendCommand("schneidenBeendet_", true);

  Serial.println("Schnitt simuliert (ohne LOGO-SPS)");
}
```

---

## 🔧 Implementierungsschritte

### Phase 1: Test-Sketch für Arduino erstellen
1. ✅ Neue Datei `IoT/sketche/SchneidMaschine_Taster_Test/SchneidMaschine_Taster_Test.ino` erstellen
2. ✅ Basis von `SchneidMaschine.ino` kopieren
3. ✅ Vereinfachen: nur `stepper()`, `dataReceived()`, serielle Kommunikation
4. ✅ Pin 9 Steuerung hinzufügen
5. ✅ `motorFinished()` und LOGO-SPS Logik entfernen
6. ✅ `schneiden()` vereinfachen
7. ✅ Testen: Sketch kompiliert ohne Fehler

### Phase 2: Test-Sketch für ESP32 erstellen
1. ✅ Neue Datei `IoT/sketche/Rollenzentrierung_Taster_Test/Rollenzentrierung_Taster_Test.ino` erstellen
2. ✅ Basis von `Rollenzentrierung.ino` kopieren
3. ✅ Bounce2 Library einbinden
4. ✅ VL53L0X Sensor-Code durch Taster-Simulation ersetzen
5. ✅ Pin 32, 33, 34 konfigurieren
6. ✅ `checkManualButtons()` implementieren
7. ✅ `isStepperRunning()` implementieren
8. ✅ `loop()` anpassen mit Bedingung "nur bei laufendem Schrittmotor"
9. ✅ `calibrateSystem()` und `initializeSensors()` entfernen
10. ✅ Testen: Sketch kompiliert ohne Fehler

### Phase 3: Hardware-Aufbau (User macht das)
1. ⚠️ Arduino Pin 9 mit ESP32 Pin 34 verbinden (Jumperwire)
2. ⚠️ 2x 10kΩ Pull-Down Widerstände einbauen:
   - Pin 32 → GND (10kΩ)
   - Pin 33 → GND (10kΩ)
3. ⚠️ Taster LINKS: zwischen Pin 32 und VCC (3.3V)
4. ⚠️ Taster RECHTS: zwischen Pin 33 und VCC (3.3V)
5. ⚠️ 2x Taster für Sensor-Simulation:
   - Taster Sensor1: z.B. Pin 35 (mit Pull-Down)
   - Taster Sensor2: z.B. Pin 36 (mit Pull-Down)
6. ⚠️ TMC2209 Treiber anschließen (wie im Original-Sketch)

### Phase 4: Software-Test
1. ⚠️ Arduino-Sketch hochladen auf Arduino
2. ⚠️ ESP32-Sketch hochladen auf ESP32
3. ⚠️ Arduino Serial Monitor öffnen (9600 baud)
4. ⚠️ ESP32 Serial Monitor öffnen (115200 baud)

**Test-Szenarien:**

**Szenario 1: Manuelle Taster (immer funktionieren)**
- Taster LINKS drücken → Rollenzentrierung bewegt sich nach LINKS
- Taster RECHTS drücken → Rollenzentrierung bewegt sich nach RECHTS
- Wiederhole mehrmals
- ✅ Erwartung: Funktioniert IMMER, egal ob Arduino-Schrittmotor läuft

**Szenario 2: Sensor-Simulation (nur bei laufendem Schrittmotor)**
- Arduino-Befehl senden: `%stepperStart_1000_forward#`
- Während Bewegung: Sensor-Taster 1 drücken (5x kurz hintereinander)
- ✅ Erwartung: Rollenzentrierung bewegt sich nach RECHTS
- Arduino-Befehl warten bis fertig
- Nach Bewegung: Sensor-Taster 1 drücken (5x)
- ✅ Erwartung: Rollenzentrierung bewegt sich NICHT (Schrittmotor steht)

**Szenario 3: Kombiniert**
- Arduino-Befehl senden: `%stepperStart_2000_forward#`
- Während Bewegung: Taster LINKS drücken (manuell)
- ✅ Erwartung: Manuelle Bewegung funktioniert
- Während Bewegung: Sensor-Taster 2 drücken (5x)
- ✅ Erwartung: Sensor-basierte Bewegung funktioniert

### Phase 5: Anpassung produktive Sketche (später)
Nach erfolgreichem Test:
1. ⚠️ Änderungen in `SchneidMaschine.ino` übertragen
2. ⚠️ Änderungen in `Rollenzentrierung.ino` übertragen
3. ⚠️ VL53L0X Sensoren wieder einbauen (statt Taster-Simulation)
4. ⚠️ Bounce2 für echte Sensoren anpassen
5. ⚠️ Hardware-Test an echter Maschine

---

## 📊 Änderungen - Übersicht

### Arduino (SchneidMaschine_Taster_Test.ino)

| Datei | Änderung | Zeilen |
|-------|----------|--------|
| `setup()` | Pin 9 als OUTPUT konfigurieren | +3 |
| `stepper()` | Pin 9 HIGH am Anfang, LOW am Ende | +4 |
| `schneiden()` | Vereinfacht (ohne LOGO-SPS) | -50 |
| `motorFinished()` | ENTFERNT | -50 |
| Handrad | Optional ENTFERNT | -200 |

**Gesamt**: ~150 Zeilen weniger, +7 Zeilen neu

### ESP32 (Rollenzentrierung_Taster_Test.ino)

| Datei | Änderung | Zeilen |
|-------|----------|--------|
| Includes | Bounce2.h hinzufügen | +1 |
| Variablen | Pins + Bounce2 Objekte | +10 |
| `setup()` | Taster + Sensor-Simulation Init | +15 |
| `checkManualButtons()` | NEU | +40 |
| `isStepperRunning()` | NEU | +5 |
| `loop()` | Logik mit Bedingung angepasst | +30 |
| VL53L0X Code | ENTFERNT (durch Taster ersetzt) | -150 |
| `calibrateSystem()` | ENTFERNT | -100 |

**Gesamt**: ~250 Zeilen entfernt, +101 Zeilen neu

---

## 🎯 Erfolgskriterien

### Muss funktionieren:
- ✅ Manuelle Taster bewegen Rollenzentrierung IMMER (unabhängig von Schrittmotor)
- ✅ Sensor-basierte Bewegung NUR wenn Arduino-Schrittmotor läuft
- ✅ Pin 9 Signal wird korrekt gesendet (HIGH/LOW)
- ✅ Pin 34 Signal wird korrekt empfangen
- ✅ Bounce2 entprellt Taster sauber (keine Doppel-Trigger)
- ✅ Motor bewegt sich nicht bei gleichzeitigem Sensor-Trigger

### Soll funktionieren:
- ✅ Sensor-Simulation durch Taster (5x drücken = Bewegung)
- ✅ Serial Monitor zeigt Debug-Ausgaben
- ✅ Keine Race Conditions bei schnellen Tastendrücken

### Nice-to-have:
- 📋 LED zur Visualisierung des Schrittmotor-Status
- 📋 Zähler für manuelle Bewegungen

---

## ⚠️ Wichtige Hinweise

### Hardware
1. **10kΩ Pull-Down Widerstände sind PFLICHT** für die Taster
   - Ohne Widerstände: Pins schweben → undefiniertes Verhalten
   - Farbcode: Braun-Schwarz-Orange

2. **Pin 34 am ESP32**
   - Pin 34 ist **Input-Only** → perfekt für Signal-Empfang
   - Kein Pull-Up/Pull-Down einstellbar → Arduino muss aktiv HIGH/LOW senden

3. **Gemeinsame Masse (GND)**
   - Arduino GND MUSS mit ESP32 GND verbunden sein
   - Sonst funktioniert Pin 9 → Pin 34 Signal nicht!

### Software
1. **Bounce2 Library installieren**
   - Arduino IDE: Sketch → Include Library → Manage Libraries → "Bounce2"
   - Version 2.x verwenden (neueste)

2. **Serial Monitor Baudrate**
   - Arduino: 9600 baud
   - ESP32: 115200 baud
   - NICHT verwechseln!

3. **Test vor Produktion**
   - Test-Sketche MÜSSEN erfolgreich getestet werden
   - Erst DANN produktive Sketche anpassen
   - Backup der produktiven Sketche erstellen!

---

## 📚 Verwendete Bibliotheken

### ESP32
- **Bounce2** (Version 2.x) - Taster-Entprellung
  - GitHub: https://github.com/thomasfredericks/Bounce2
  - Installation: Arduino IDE Library Manager

### Arduino
- Keine neuen Bibliotheken nötig
- Standard Arduino Bibliotheken ausreichend

---

## 🔄 Nächste Schritte (nach diesem Plan)

1. ✅ Plan fertigstellen und reviewen
2. ⚠️ User bestätigt Plan
3. ⚠️ Test-Sketche implementieren (Phase 1 + 2)
4. ⚠️ User baut Hardware auf (Phase 3)
5. ⚠️ Gemeinsam testen (Phase 4)
6. ⚠️ Bei Erfolg: Produktive Sketche anpassen (Phase 5)
7. ⚠️ Weitere Funktion planen (User erwähnte eine zweite Funktion)

---

**Erstellt**: 15. Januar 2026
**Letzte Änderung**: 15. Januar 2026
**Erstellt von**: Claude Code Planning Session
**Review benötigt**: ✅ JA - User muss Plan bestätigen

---

## ✅ User-Bestätigung (15. Januar 2026)

1. ✅ Pin-Wahl bestätigt (Arduino Pin 9, ESP32 Pin 32/33/34)
2. ✅ 10kΩ Pull-Down Widerstände
3. ✅ Bounce2 Library für Sensoren
4. ✅ Test-Sketch Namen
5. ✅ Sensor-Simulation: Pin 35 und 26
6. ✅ LED zur Visualisierung: JA (Pin wird im Code definiert)
7. ✅ Serial-Kommunikation im Test: NEIN (nur Serial Monitor Debug-Ausgaben)

---

**STATUS**: ✅ Implementierung ABGESCHLOSSEN

---

## 📦 Implementierte Dateien

### 1. SchneidMaschine_Taster_Test.ino
**Pfad**: `IoT/sketche/SchneidMaschine_Taster_Test/SchneidMaschine_Taster_Test.ino`

**Features:**
- ✅ Taster START auf Pin 2 (mit 10kΩ Pull-Down)
- ✅ Pin 9 Steuerung (HIGH = Schrittmotor läuft, LOW = steht)
- ✅ LED Pin 10 zur Visualisierung (leuchtet während Bewegung, mit 220Ω Vorwiderstand)
- ✅ Vereinfachte Version ohne LOGO-SPS
- ✅ Taster startet 2000 Steps Simulation
- ✅ Serial Monitor Befehle (optional): `%stepperStart_[steps]_[forward/backward]#`
- ✅ Debug-Ausgaben für alle Aktionen
- ✅ allesStop Funktionalität
- ✅ Software-Entprellung (50ms)

**Wichtige Funktionen:**
- `checkButton()` - Prüft Taster mit Entprellung, startet Simulation
- `stepper()` - Setzt Pin 9 HIGH/LOW automatisch, LED an/aus
- `dataReceived()` - Empfängt Befehle über Serial Monitor (optional)
- `isAllesStop()` - Stoppt Bewegung während Ausführung

**Baudrate**: 9600

### 2. Rollenzentrierung_Taster_Test.ino
**Pfad**: `IoT/sketche/Rollenzentrierung_Taster_Test/Rollenzentrierung_Taster_Test.ino`

**Features:**
- ✅ 2x Manuelle Taster (Pin 32 LINKS, Pin 33 RECHTS)
- ✅ 2x Sensor-Simulation Taster (Pin 35, Pin 26)
- ✅ Pin 34 Eingang vom Arduino (Schrittmotor-Status)
- ✅ LED Pin 17 zeigt Arduino-Schrittmotor Status (mit 220Ω Vorwiderstand)
- ✅ LED Pin 16 simuliert Motor-Bewegung (mit 220Ω Vorwiderstand, statt TMC2209)
- ✅ Bounce2 Library für alle Taster
- ✅ Sensor-Bewegung nur bei aktivem Arduino-Schrittmotor
- ✅ Manuelle Taster funktionieren IMMER
- ✅ Status-Ausgabe alle 3 Sekunden

**Wichtige Funktionen:**
- `checkManualButtons()` - Prüft Taster LINKS/RECHTS (IMMER)
- `isStepperRunning()` - Liest Pin 34 Status
- `checkSensorSimulation()` - Prüft Sensor-Taster (NUR bei aktivem Schrittmotor)
- `simulateMotorMovement()` - LED-Simulation statt echter Motor (2 Sekunden)

**Baudrate**: 115200

---

## 🔌 Hardware-Aufbau

### Verbindungen

```
Arduino
=======
Pin 2 <-------- Taster START
   |
 [10kΩ] Pull-Down
   |
  GND

Taster START: zwischen Pin 2 und VCC (5V)


Arduino <--> ESP32
================
Pin 9 ---------> Pin 34 (Signal: HIGH = Schrittmotor läuft)
GND -----------> GND (WICHTIG: Gemeinsame Masse!)


ESP32 Taster
============
Pin 32 <-------- Taster LINKS
   |
 [10kΩ] Pull-Down
   |
  GND

Taster LINKS: zwischen Pin 32 und VCC (3.3V)


Pin 33 <-------- Taster RECHTS
   |
 [10kΩ] Pull-Down
   |
  GND

Taster RECHTS: zwischen Pin 33 und VCC (3.3V)


Pin 35 <-------- Taster Sensor1-Simulation
   |
 [10kΩ] Pull-Down
   |
  GND

Taster Sensor1: zwischen Pin 35 und VCC (3.3V)


Pin 26 <-------- Taster Sensor2-Simulation
   |
 [10kΩ] Pull-Down
   |
  GND

Taster Sensor2: zwischen Pin 26 und VCC (3.3V)


LEDs
====
Arduino Pin 10: Status-LED (Schrittmotor-Status, mit 220Ω Vorwiderstand)
ESP32 Pin 17: Status-LED (Arduino-Schrittmotor-Status, mit 220Ω Vorwiderstand)
ESP32 Pin 16: Motor-LED (simuliert Bewegung, 2 Sekunden an, mit 220Ω Vorwiderstand)
```

### Benötigte Bauteile

| Komponente | Anzahl | Wert | Bemerkung |
|------------|--------|------|-----------|
| Pull-Down Widerstand | 5x | 10kΩ | Braun-Schwarz-Orange (1x Arduino, 4x ESP32) |
| Taster | 5x | - | Öffner (NO) - 1x Arduino START, 4x ESP32 |
| LED | 3x | - | 1x Arduino Pin 10, 2x ESP32 Pin 16+17 |
| Vorwiderstand für LED | 3x | 220Ω | Rot-Rot-Braun (je 1x pro LED) |
| Jumperwire | 1x | - | Arduino Pin 9 → ESP32 Pin 34 |
| Jumperwire | 1x | - | Arduino GND → ESP32 GND |

---

## 🧪 Test-Anleitung

### Schritt 1: Arduino-Sketch hochladen

1. Öffne Arduino IDE
2. Datei öffnen: `IoT/sketche/SchneidMaschine_Taster_Test/SchneidMaschine_Taster_Test.ino`
3. Board wählen: Arduino Uno/Nano/Mega (je nach Hardware)
4. COM-Port wählen
5. Upload klicken
6. Serial Monitor öffnen (9600 baud)

**Erwartete Ausgabe:**
```
========================================
SchneidMaschine_Taster_Test - GESTARTET
========================================
Pin 2: Taster START (INPUT)
Pin 9: OUTPUT - Signal an ESP32 (Initial: LOW)
Pin 10: LED - Visualisierung Schrittmotor-Status
Setup abgeschlossen.
========================================
STEUERUNG:
  - Taster START drücken: Startet 2000 Steps
  - LED leuchtet während Bewegung
  - Pin 9 HIGH während Bewegung (Signal an ESP32)
========================================
Serial-Befehle (optional):
  %stepperStart_[steps]_[forward/backward]#
  %allesStop#
Beispiel: %stepperStart_1000_forward#
========================================
```

### Schritt 2: ESP32-Sketch hochladen

1. Öffne Arduino IDE (neue Instanz oder Tab)
2. Datei öffnen: `IoT/sketche/Rollenzentrierung_Taster_Test/Rollenzentrierung_Taster_Test.ino`
3. Board wählen: ESP32 Dev Module
4. COM-Port wählen (anderer als Arduino!)
5. Upload klicken
6. Serial Monitor öffnen (115200 baud)

**Erwartete Ausgabe:**
```
=============================================
Rollenzentrierung_Taster_Test - GESTARTET
=============================================
Motor-LED Pin 16: Simuliert Motor-Bewegung
Pin 32: Taster LINKS (INPUT)
Pin 33: Taster RECHTS (INPUT)
Pin 34: Signal vom Arduino (INPUT)
Pin 35: Sensor1-Simulation (INPUT)
Pin 26: Sensor2-Simulation (INPUT)
Pin 17: Arduino-Status-LED (OUTPUT)
Bounce2: Entprellung aktiviert (50ms)

Setup abgeschlossen.
=============================================
FUNKTIONEN:
  - Taster LINKS/RECHTS: Manuelle Bewegung (IMMER)
  - Sensor-Taster: Bewegung nur bei Arduino-Motor aktiv
  - LED Pin 17: Zeigt Arduino-Schrittmotor Status
  - LED Pin 16: Simuliert Motor-Bewegung (2 Sekunden)
=============================================
```

### Schritt 3: Hardware verbinden

1. ⚠️ **BEIDE Geräte vom Strom trennen!**
2. Arduino Pin 9 mit ESP32 Pin 34 verbinden (Jumperwire)
3. Arduino GND mit ESP32 GND verbinden (Jumperwire)
4. Arduino Taster START an Pin 2 aufbauen (mit 10kΩ Pull-Down)
5. 4x ESP32 Taster mit Pull-Down Widerständen aufbauen (siehe Schaltplan)
6. LED an ESP32 Pin 16 anschließen (mit 220Ω Vorwiderstand nach GND)
7. **BEIDE Geräte wieder anschließen**

### Schritt 4: Funktionstest

#### Test 1: Manuelle Taster (IMMER funktionieren)

**Was testen:**
- Taster LINKS drücken → Rollenzentrierung nach LINKS
- Taster RECHTS drücken → Rollenzentrierung nach RECHTS

**Erwartete Ausgabe (ESP32):**
```
>>> TASTER LINKS gedrückt
    Bewege nach LINKS (manuell - VORRANG)
  [MOTOR-SIM] LED an - simuliere Bewegung nach LINKS
  [MOTOR-SIM] Dauer: 2000ms
    Progress: 25%
    Progress: 50%
    Progress: 75%
    Progress: 100%
  [MOTOR-SIM] LED aus - Bewegung beendet
    Bewegung LINKS abgeschlossen
```

**✅ Erfolgskriterium:**
- Taster funktionieren unabhängig vom Arduino-Schrittmotor Status
- LED Pin 16 leuchtet für 2 Sekunden

#### Test 2: Arduino Taster START

**Was testen:**
1. Arduino Taster START drücken
2. LED Pin 10 sollte leuchten
3. ESP32 LED Pin 17 sollte leuchten (Signal empfangen)

**Erwartete Ausgabe (Arduino):**
```
>>> TASTER START gedrückt
    Starte Schrittmotor-Simulation...
    Steps: 2000
    Richtung: forward

[PIN 9] HIGH - Schrittmotor läuft (Signal an ESP32 gesendet)
[RICHTUNG] Vorwärts
  Progress: 100/2000 Steps
  Progress: 200/2000 Steps
  ...

>>> Schrittmotor FERTIG
    Step Counter: 2000
    Pin 9: LOW (Schrittmotor steht)
```

**✅ Erfolgskriterium:**
- Arduino LED Pin 10 leuchtet während Bewegung
- ESP32 LED Pin 17 leuchtet während Arduino-Bewegung

#### Test 3: Sensor-Simulation (NUR bei aktivem Schrittmotor)

**Was testen:**
1. Arduino Taster START drücken (oder Serial-Befehl: `%stepperStart_2000_forward#`)
2. Während Arduino bewegt: Sensor1-Taster 5x schnell drücken
3. ESP32 sollte sich nach RECHTS bewegen

**Erwartete Ausgabe (ESP32):**
```
[SIGNAL] Arduino-Schrittmotor läuft → Sensor-Bewegung AKTIV

[SENSOR1] Getriggert [1/5]
[SENSOR1] Getriggert [2/5]
[SENSOR1] Getriggert [3/5]
[SENSOR1] Getriggert [4/5]
[SENSOR1] Getriggert [5/5]

>>> SENSOR1: 5x getriggert → Bewege nach RECHTS
  [MOTOR-SIM] LED an - simuliere Bewegung nach RECHTS
  [MOTOR-SIM] Dauer: 2000ms
    Progress: 25%
    Progress: 50%
    Progress: 75%
    Progress: 100%
  [MOTOR-SIM] LED aus - Bewegung beendet
    Sensor-Bewegung RECHTS abgeschlossen

[SIGNAL] Arduino-Schrittmotor steht → Sensor-Bewegung DEAKTIVIERT
```

**✅ Erfolgskriterium:**
- Sensor-Bewegung funktioniert NUR wenn Arduino-Schrittmotor läuft
- LED Pin 17 leuchtet während Arduino bewegt
- LED Pin 16 leuchtet für 2 Sekunden während Sensor-Bewegung

#### Test 4: Sensor-Simulation OHNE aktivem Schrittmotor

**Was testen:**
1. Arduino-Schrittmotor steht (kein Befehl gesendet)
2. Sensor1-Taster 5x drücken
3. ESP32 sollte sich NICHT bewegen

**Erwartete Ausgabe (ESP32):**
```
--- Status Update ---
Arduino-Schrittmotor: STEHT
Rollenzentrierung-Motor: STEHT
Sensor1 Trigger Count: 0/5
Sensor2 Trigger Count: 0/5
---------------------
```

**✅ Erfolgskriterium:**
- Sensor-Taster lösen KEINE Bewegung aus wenn Arduino-Schrittmotor steht

#### Test 5: Kombiniert

**Was testen:**
1. Arduino-Befehl: `%stepperStart_3000_forward#`
2. Während Bewegung: Taster LINKS drücken (manuell)
3. Während Bewegung: Sensor2-Taster 5x drücken

**Erwartete Ausgabe:**
- Manuelle Bewegung funktioniert
- Sensor-Bewegung funktioniert
- Keine Konflikte

**✅ Erfolgskriterium:**
- Alle Funktionen arbeiten parallel korrekt

---

## ⚠️ Troubleshooting

### Problem: ESP32 LED blinkt nicht bei Arduino-Bewegung

**Ursache:** Pin 9 → Pin 34 Verbindung fehlt oder GND nicht verbunden

**Lösung:**
1. Beide Geräte vom Strom trennen
2. Jumperwire überprüfen: Arduino Pin 9 → ESP32 Pin 34
3. GND-Verbindung überprüfen: Arduino GND → ESP32 GND
4. Mit Multimeter messen: Pin 9 sollte 5V zeigen während Bewegung

### Problem: Taster lösen keine Bewegung aus

**Ursache:** Pull-Down Widerstände fehlen oder falsch verbunden

**Lösung:**
1. Pull-Down Widerstand prüfen: Pin → GND (10kΩ)
2. Taster prüfen: Pin → VCC (3.3V)
3. Mit Multimeter messen: Pin sollte 3.3V zeigen wenn Taster gedrückt

### Problem: Sensor-Bewegung funktioniert IMMER

**Ursache:** Pin 34 liest immer HIGH (Signal-Verbindung fehlt)

**Lösung:**
1. Pin 34 Verbindung prüfen
2. Arduino-Sketch prüfen: Wird Pin 9 richtig gesetzt?
3. Mit Serial Monitor prüfen: `[PIN 9] HIGH` Ausgaben erscheinen?

### Problem: LED Pin 16 leuchtet nicht

**Ursache:** LED falsch angeschlossen oder Vorwiderstand fehlt

**Lösung:**
1. LED-Polung prüfen (lange Seite = Anode = Pin 16, kurze Seite = Kathode = GND)
2. Vorwiderstand 220Ω zwischen LED Kathode und GND
3. Serial Monitor: Erscheint `[MOTOR-SIM] LED an`?
4. Mit Multimeter LED-Verbindung prüfen

---

## 📊 Nächste Schritte

### Nach erfolgreichem Test:

1. ✅ Test-Sketche funktionieren im Büro
2. ⚠️ Produktive Sketche anpassen:
   - `SchneidMaschine.ino` → Pin 9 Steuerung hinzufügen
   - `Rollenzentrierung.ino` → Taster + Pin 34 + Bounce2 integrieren
   - VL53L0X Sensoren wieder einbauen (statt Taster-Simulation)
3. ⚠️ Hardware-Test an echter Maschine
4. ⚠️ Weitere Funktion planen (User erwähnte zweite Funktion)

---

**Implementierung abgeschlossen**: 15. Januar 2026
**Bereit für Test**: ✅ JA
