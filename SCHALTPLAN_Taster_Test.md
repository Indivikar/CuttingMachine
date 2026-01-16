# Schaltplan - Taster Test Setup

**Projekt**: SchneidMaschine - Rollenzentrierung Test
**Datum**: 15. Januar 2026

---

## Übersicht

Dieses Dokument zeigt die komplette Verdrahtung für den Test-Aufbau der manuellen Taster und Schrittmotor-Synchronisation.

---

## Komponenten-Liste

### Arduino (Schneidmaschine)
- 1x Arduino Uno/Nano/Mega
- 1x USB-Kabel
- Optional: Schrittmotor-Treiber (für echten Test)

### ESP32 (Rollenzentrierung)
- 1x ESP32 Dev Board
- 1x USB-Kabel
- 1x LED (für Motor-Simulation)
- 1x 220Ω Vorwiderstand (Rot-Rot-Braun)

### Taster und Widerstände
- 4x Taster (Öffner/NO - Normally Open)
- 4x 10kΩ Widerstände (Pull-Down) - Farbcode: Braun-Schwarz-Orange

### Verkabelung
- 2x Jumperwire Male-Male (Pin 9→34, GND→GND)
- 8x Jumperwire Male-Male (Taster zu ESP32)
- Breadboard (empfohlen für Test-Aufbau)

---

## Gesamtschaltplan (ASCII)

```
                   +5V (USB)                        +3.3V (ESP32)
                      |                                	|
                      |                                	|
    ┌─────────────────┴────────────────┐              	|
    │         ARDUINO UNO/MEGA         │              	|
    │                                  │              	|
    │  ┌────────────────────────────┐  │               	|
    │  │ Pin 9  (OUTPUT)            │──┼────────────────┼──────┐
    │  │                            │  │               	|      │
    │  │ Pin 13 (LED - Built-in)    │  │               	|      │
    │  │                            │  │               	|      │
    │  │ Pin 4  (STEP - optional)   │  │               	|      │
    │  │ Pin 5  (DIR  - optional)   │  │               	|      │
    │  │ Pin 6  (EN   - optional)   │  │               	|      │
    │  │                            │  │               	|      │
    │  │ GND                        │──┼────────────────┼──┐   │
    │  └────────────────────────────┘  │               	|  │   │
    └──────────────────────────────────┘               	|  │   │
                                                        |  │   │
                                                        |  │   │
    ┌───────────────────────────────────────────────────┘  │   │
    │                                                      │   │
    │              ESP32 DEV MODULE                        │   │
    │  ┌────────────────────────────────────────────────┐  │   │
    │  │                                                │  │   │
    │  │ Pin 34 (INPUT - Arduino Signal)  ◄─────────────┼──┘   │
    │  │                                                │      │
    │  │ Pin 32 (INPUT - Taster LINKS)    ◄─────────┐   │      │
    │  │                                            │   │      │
    │  │ Pin 33 (INPUT - Taster RECHTS)   ◄─────┐   │   │      │
    │  │                                        │   │   │      │
    │  │ Pin 35 (INPUT - Sensor1 Sim)     ◄───┐ │   │   │      │
    │  │                                      │ │   │   │      │
    │  │ Pin 26 (INPUT - Sensor2 Sim)     ◄─┐ │ │   │   │      │
    │  │                                    │ │ │   │   │      │
    │  │ Pin 2  (LED - Built-in)           	│ │ │   │   │      │
    │  │                                    │ │ │   │   │      │
    │  │ Pin 16 (Motor-LED)  ───────────┐   │ │ │   │   │      │
    │  │                                │   │ │ │   │   │      │
    │  │ GND ───────────────────────────┼───┼─┼─┼───┼───┼──┐   │
    │  │                                │   │ │ │   │   │  │   │
    │  │ 3.3V ──────────────────────────┼───┼─┼─┼───┼───┼──┼───┘
    │  └────────────────────────────────┼───┼─┼─┼───┼───┼──┼──┐
    └───────────────────────────────────┼───┼─┼─┼───┼───┼──┼──┼──┐
                                        │   │ │ │   │   │  │  │  │
                                        │   │ │ │   │   │  │  │  │
                              ┌─────────┘   │ │ │   │   │  │  │  │
                              │  LED + 220Ω │ │ │   │   │  │  │  │
                              │  ┌────────┐ │ │ │   │   │  │  │  │
                              │  │   LED  │ │ │ │   │   │  │  │  │
                              │  │ Anode  │◄┘ │ │   │   │  │  │  │
                              │  │        │   │ │   │   │  │  │  │
                              │  │ Kathode├───┤ │   │   │  │  │  │
                              │  └────┬───┘   │ │   │   │  │  │  │
                              │     [220Ω]    │ │   │   │  │  │  │
                              │       │       │ │   │   │  │  │  │
                              │      GND◄─────┼─┼───┼───┼──┘  │  │
                              │               │ │   │   │     │  │
                              └───────────────┘ │   │   │     │  │
                                                    │   │     │     │
                        ┌───────────────────────────┘   │     │     │
                        │  Taster LINKS (NO)            │     │     │
                        │  ┌────────┐                   │     │     │
                        │  │        │                   │     │     │
                 Pin 32 ├──┤ Taster ├──── 3.3V         	│     │     │
                        │  │        │                   │     │     │
                        │  └────┬───┘                   │     │     │
                        │       │                       │     │     │
                        │    [10kΩ]  Pull-Down         	│     │     │
                        │       │                       │     │     │
                        └───────┴────── GND             │     │     │
                                                        │     │     │
                        ┌───────────────────────────────┘     │     │
                        │  Taster RECHTS (NO)                 │     │
                        │  ┌────────┐                         │     │
                        │  │        │                         │     │
                 Pin 33 ├──┤ Taster ├──── 3.3V                │     │
                        │  │        │                         │     │
                        │  └────┬───┘                         │     │
                        │       │                             │     │
                        │    [10kΩ]  Pull-Down                │     │
                        │       │                             │     │
                        └───────┴────── GND                   │     │
                                                              │     │
                        ┌─────────────────────────────────────┘     │
                        │  Taster SENSOR1 (NO)                      │
                        │  ┌────────┐                               │
                        │  │        │                               │
                 Pin 35 ├──┤ Taster ├──── 3.3V                     	│
                        │  │        │                               │
                        │  └────┬───┘                               │
                        │       │                                   │
                        │    [10kΩ]  Pull-Down                     	│
                        │       │                                   │
                        └───────┴────── GND                         │
                                                                    │
                        ┌───────────────────────────────────────────┘
                        │  Taster SENSOR2 (NO)
                        │  ┌────────┐
                        │  │        │
                 Pin 26 ├──┤ Taster ├──── 3.3V
                        │  │        │
                        │  └────┬───┘
                        │       │
                        │    [10kΩ]  Pull-Down
                        │       │
                        └───────┴────── GND
```

---

## Detaillierte Einzelschaltungen

### 1. Arduino Pin 9 → ESP32 Pin 34 (Signal)

```
Arduino                          ESP32
┌──────┐                      ┌──────┐
│      │                      │      │
│ Pin 9├──────────────────────┤Pin 34│
│(OUT) │    Jumperwire        │(IN)  │
│      │                      │      │
│ GND  ├──────────────────────┤ GND  │
│      │    Jumperwire        │      │
└──────┘                      └──────┘

WICHTIG: GND-Verbindung ist ZWINGEND erforderlich!
```

**Funktion:**
- Arduino Pin 9 sendet HIGH (5V) wenn Schrittmotor läuft
- ESP32 Pin 34 empfängt dieses Signal (5V-tolerant)
- Gemeinsame Masse (GND) für stabile Signalübertragung

---

### 2. Taster LINKS (Pin 32)

```
                3.3V (ESP32)
                    │
                    │
               ┌────┴────┐
               │  Taster │  (Normally Open)
               │  LINKS  │
               └────┬────┘
                    │
                    ├───────────► Pin 32 (ESP32)
                    │
                 [10kΩ]  Pull-Down Widerstand
                    │
                   GND

Funktionsweise:
- Taster NICHT gedrückt: Pin 32 = LOW (0V) durch Pull-Down
- Taster gedrückt:       Pin 32 = HIGH (3.3V)
```

**Breadboard-Aufbau:**
1. Pin 32 → Reihe A (z.B. A10)
2. 10kΩ Widerstand: A10 → GND-Rail
3. Taster: A10 → 3.3V-Rail
4. Taster-Pin 1: Reihe A10
5. Taster-Pin 2: Reihe 3.3V-Rail

---

### 3. Taster RECHTS (Pin 33)

```
                3.3V (ESP32)
                    │
                    │
               ┌────┴────┐
               │  Taster │  (Normally Open)
               │ RECHTS  │
               └────┬────┘
                    │
                    ├───────────► Pin 33 (ESP32)
                    │
                 [10kΩ]  Pull-Down Widerstand
                    │
                   GND

Aufbau identisch zu Taster LINKS, nur anderer Pin
```

---

### 4. Taster SENSOR1 (Pin 35) - Simulation

```
                3.3V (ESP32)
                    │
                    │
               ┌────┴────┐
               │  Taster │  (Normally Open)
               │SENSOR1  │
               └────┬────┘
                    │
                    ├───────────► Pin 35 (ESP32)
                    │
                 [10kΩ]  Pull-Down Widerstand
                    │
                   GND

Funktion: Simuliert VL53L0X Sensor 1 (LINKS)
- 5x schnell drücken = Sensor getriggert
```

---

### 5. Taster SENSOR2 (Pin 26) - Simulation

```
                3.3V (ESP32)
                    │
                    │
               ┌────┴────┐
               │  Taster │  (Normally Open)
               │SENSOR2  │
               └────┬────┘
                    │
                    ├───────────► Pin 26 (ESP32)
                    │
                 [10kΩ]  Pull-Down Widerstand
                    │
                   GND

Funktion: Simuliert VL53L0X Sensor 2 (RECHTS)
- 5x schnell drücken = Sensor getriggert
```

---

### 6. Motor-LED (Pin 16) - Simuliert Bewegung

```
ESP32                          LED + Vorwiderstand
┌──────┐                      ┌────────────────┐
│      │                      │                │
│      │                      │     LED        │
│Pin 16├──────────────────────┤ Anode (+)      │
│      │                      │ lange Seite    │
│      │                      │                │
│      │                      │ Kathode (-)    │
│      │                      │ kurze Seite    │
│      │                      └────┬───────────┘
│      │                           │
│      │                        [220Ω]  ← Vorwiderstand
│      │                           │    (Rot-Rot-Braun)
│ GND  ├───────────────────────────┘
│      │
└──────┘

Funktionsweise:
- LED leuchtet 2 Sekunden = Motor-Bewegung wird simuliert
- Vorwiderstand schützt LED vor zu hohem Strom
- Anode (langes Bein) → Pin 16
- Kathode (kurzes Bein) → 220Ω → GND
```

**LED-Anschluss:**
- Anode (langes Bein, +): ESP32 Pin 16
- Kathode (kurzes Bein, -): 220Ω Vorwiderstand
- Vorwiderstand: GND

**Wichtig:**
- LED-Polung beachten! Falsche Polung = LED leuchtet nicht
- Vorwiderstand IMMER verwenden (sonst LED kaputt)
- LED leuchtet = Motor würde sich bewegen
- Dauer: 2 Sekunden pro Bewegung

---

## Breadboard-Layout (Vogelperspektive)

```
Power Rails:        Breadboard Rows:                        Power Rails:
    +3.3V                                                       GND
      │                                                          │
      ↓              1  2  3  4  5  6  7  8  9 10              ↓
   ═══════════════════════════════════════════════════════════════
   ║  +  ║ A [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]  Pin32──┐        ║ - ║
   ║  +  ║ B [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │        ║ - ║
   ║  +  ║ C [Taster LINKS           ][ ] ◄─────10kΩ─────┐   ║ - ║
   ║  +  ║ D [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ E [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║     ║───────────────────────────────────────────────────║   ║
   ║  +  ║ F [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ G [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ H [Taster RECHTS          ][ ] ◄─────10kΩ─────┤   ║ - ║
   ║  +  ║ I [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ] Pin33───┘    │   ║ - ║
   ║  +  ║ J [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]              │   ║ - ║
   ║     ║───────────────────────────────────────────────────║   ║
   ║  +  ║ A [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ] Pin35───┐    │   ║ - ║
   ║  +  ║ B [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ C [Taster SENSOR1         ][ ] ◄─────10kΩ─────┤   ║ - ║
   ║  +  ║ D [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ E [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║     ║───────────────────────────────────────────────────║   ║
   ║  +  ║ F [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ G [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]         │    │   ║ - ║
   ║  +  ║ H [Taster SENSOR2         ][ ] ◄─────10kΩ─────┤   ║ - ║
   ║  +  ║ I [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ] Pin26───┘    │   ║ - ║
   ║  +  ║ J [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]              │   ║ - ║
   ═══════════════════════════════════════════════════════════════
      │                                                          │
      │                                                          │
   3.3V───┐                                                   GND────┐
          │                                                          │
      ESP32 Pin                                              ESP32 Pin
```

---

## Aufbau-Reihenfolge (Empfohlen)

### Schritt 1: ESP32 und Arduino vorbereiten
1. ESP32 auf Breadboard stecken
2. Arduino daneben platzieren (nicht auf Breadboard)
3. Power Rails verbinden: ESP32 3.3V → (+) Rail, ESP32 GND → (-) Rail

### Schritt 2: Taster LINKS aufbauen
1. 10kΩ Widerstand: Pin 32 Reihe → GND Rail
2. Taster: Pin 32 Reihe → 3.3V Rail
3. Testen: Mit Multimeter Pin 32 messen
   - Ohne Druck: 0V
   - Mit Druck: 3.3V

### Schritt 3: Taster RECHTS aufbauen
1. 10kΩ Widerstand: Pin 33 Reihe → GND Rail
2. Taster: Pin 33 Reihe → 3.3V Rail
3. Testen wie bei Taster LINKS

### Schritt 4: Sensor-Taster aufbauen
1. Sensor1 Taster: Pin 35 + 10kΩ Pull-Down
2. Sensor2 Taster: Pin 26 + 10kΩ Pull-Down
3. Testen wie vorher

### Schritt 5: Arduino Pin 9 → ESP32 Pin 34
1. Jumperwire: Arduino Pin 9 → ESP32 Pin 34
2. Jumperwire: Arduino GND → ESP32 GND (Power Rail)
3. ⚠️ WICHTIG: Beide GND müssen verbunden sein!

### Schritt 6: Motor-LED an Pin 16 anschließen
1. LED Anode (langes Bein) → ESP32 Pin 16
2. LED Kathode (kurzes Bein) → 220Ω Vorwiderstand
3. Vorwiderstand anderes Ende → GND Rail
4. Polung prüfen: Langes Bein = Anode (+), Kurzes Bein = Kathode (-)

### Schritt 7: Funktionstest
1. ESP32 Sketch hochladen
2. Arduino Sketch hochladen
3. Tests durchführen (siehe PLAN.md)

---

## Fritzing Anleitung

Falls du die Schaltung in Fritzing nachbauen möchtest:

### Benötigte Bauteile in Fritzing:
1. **Arduino Uno** (Core Parts → Arduino → Arduino Uno R3)
2. **ESP32 Dev Board** (ggf. importieren oder Generic IC verwenden)
3. **4x Pushbutton** (Core Parts → Input → Pushbutton)
4. **4x 10kΩ Resistor** (Core Parts → Basic → Resistor)
5. **Breadboard** (Core Parts → Breadboard)
6. **TMC2209** (ggf. importieren oder als Generic IC)
7. **Stepper Motor** (Core Parts → Output → Stepper Motor)
8. **Jumperwires** (automatisch beim Verbinden)

### Verbindungen in Fritzing:
1. Arduino Pin 9 → ESP32 Pin 34 (orange Wire)
2. Arduino GND → ESP32 GND (schwarz Wire)
3. Für jeden Taster:
   - Ein Pin → ESP32 GPIO (grün/gelb/blau/lila)
   - Anderer Pin → 3.3V Rail (rot)
   - 10kΩ zwischen GPIO und GND (braun)

---

## Fotos der Widerstände

### 10kΩ Widerstand erkennen:

```
Farbcode (von links nach rechts):
┌─────────────────────────────────┐
│ Braun | Schwarz | Orange | Gold │
│   1   |    0    |  x1k   | ±5%  │
└─────────────────────────────────┘
         = 10.000 Ω = 10kΩ

Alternative Farbcodes:
- Braun | Schwarz | Schwarz | Rot | Braun (5-Band: 10kΩ ±1%)
```

---

## Sicherheitshinweise

⚠️ **WICHTIG:**
1. Immer zuerst USB-Verbindungen trennen bevor Verkabelung geändert wird
2. Keine direkten Verbindungen zwischen 5V und 3.3V!
3. TMC2209 VMOT niemals direkt an ESP32 anschließen
4. Alle GND-Verbindungen müssen zusammengeführt werden
5. Motor-Stromversorgung (12-24V) getrennt von Logik (3.3-5V)
6. Bei Rauchentwicklung sofort Strom trennen!

---

**Erstellt**: 15. Januar 2026
**Für Projekt**: SchneidMaschine Taster Test
**Siehe auch**: PLAN.md für Test-Anleitung
