/*
 * Rollenzentrierung_Taster_Test.ino
 *
 * TEST-SKETCH für manuelle Taster + Schrittmotor-Synchronisation
 *
 * Zweck:
 * - Testet manuelle Taster-Steuerung (Pin 32 LINKS, Pin 33 RECHTS)
 * - Testet Sensor-Simulation mit Tastern (Pin 35, Pin 26)
 * - Testet Schrittmotor-Synchronisation (Pin 34 von Arduino)
 * - Verwendet Bounce2 Library für Entprellung
 * - Motor-Simulation mit LED statt echtem TMC2209
 *
 * Hardware:
 * - ESP32
 * - Pin 32: Taster LINKS (mit 10kΩ Pull-Down)
 * - Pin 33: Taster RECHTS (mit 10kΩ Pull-Down)
 * - Pin 34: INPUT vom Arduino Pin 9 (Schrittmotor-Status)
 * - Pin 35: Taster Sensor1-Simulation (mit 10kΩ Pull-Down)
 * - Pin 26: Taster Sensor2-Simulation (mit 10kΩ Pull-Down)
 * - Pin 16: Motor-LED (simuliert Motor-Bewegung, mit 220Ω Vorwiderstand)
 * - Pin 17: Status-LED (zeigt Arduino-Schrittmotor-Status, mit 220Ω Vorwiderstand)
 *
 * Datum: 15. Januar 2026
 */

#include <Bounce2.h>

//---------- ALLGEMEINE EINSTELLUNGEN ----------
const int SERIAL_BAUDRATE = 115200;
const int LOOP_DELAY = 50;  // 50ms für schnellere Reaktion

//---------- MOTOR-SIMULATION EINSTELLUNGEN ----------
// Statt echtem TMC2209: LED-Simulation
const int MOTOR_LED_PIN = 16;      // LED simuliert Motor-Bewegung
const int MOTOR_SIMULATION_TIME = 2000;  // 2 Sekunden LED an = Motor bewegt sich

//---------- RICHTUNGS-KONSTANTEN ----------
const bool RIGHT_DIRECTION = HIGH;  // Gegen den Uhrzeigersinn
const bool LEFT_DIRECTION = LOW;    // Im Uhrzeigersinn

//---------- NEU: PIN-DEFINITIONEN FÜR TASTER UND SIGNALE ----------
// Manuelle Taster (Pull-Down extern mit 10kΩ)
const int BUTTON_LEFT = 32;        // Taster LINKS - bewegt nach LINKS
const int BUTTON_RIGHT = 33;       // Taster RECHTS - bewegt nach RECHTS

// Signal vom Arduino
const int STEPPER_SIGNAL = 34;     // Signal von Arduino Pin 9 (HIGH = Schrittmotor läuft)

// Sensor-Simulation durch Taster (Pull-Down extern mit 10kΩ)
const int SENSOR1_SIM = 35;        // Simuliert VL53L0X Sensor 1 (LINKS)
const int SENSOR2_SIM = 26;        // Simuliert VL53L0X Sensor 2 (RECHTS)

// Status-LEDs
const int LED_PIN = 17;            // Status-LED zeigt Arduino-Schrittmotor-Status (mit 220Ω Vorwiderstand)

//---------- SENSOR EINSTELLUNGEN ----------
const int SENSOR_TRIGGER_COUNT = 5;  // 5x Taster drücken = Bewegung auslösen

//---------- NEU: BOUNCE2 OBJEKTE ----------
// Manuelle Taster
Bounce buttonLeft = Bounce();
Bounce buttonRight = Bounce();

// Sensor-Simulation
Bounce sensor1Sim = Bounce();
Bounce sensor2Sim = Bounce();

//---------- VARIABLEN ----------
// Zähler für Sensor-Trigger
int sensor1TriggerCount = 0;
int sensor2TriggerCount = 0;

// Status-Flags
bool motorIsMoving = false;        // Läuft der Rollenzentrierung-Motor?
bool stepperIsRunning = false;     // Läuft der Arduino-Schrittmotor?

// Status-Ausgabe
unsigned long lastStatusTime = 0;
const unsigned long STATUS_INTERVAL = 3000; // Status alle 3 Sekunden

//---------- SETUP ----------
void setup() {
  Serial.begin(SERIAL_BAUDRATE);
  delay(2000);

  Serial.println("\n\n=============================================");
  Serial.println("Rollenzentrierung_Taster_Test - GESTARTET");
  Serial.println("=============================================");

  // Motor-LED konfigurieren (simuliert TMC2209 Motor)
  pinMode(MOTOR_LED_PIN, OUTPUT);
  digitalWrite(MOTOR_LED_PIN, LOW);  // LED aus = Motor steht
  Serial.println("Motor-LED Pin 16: Simuliert Motor-Bewegung");

  // Taster und Signal-Pins konfigurieren
  pinMode(BUTTON_LEFT, INPUT);     // Pull-Down extern (10kΩ)
  pinMode(BUTTON_RIGHT, INPUT);    // Pull-Down extern (10kΩ)
  pinMode(STEPPER_SIGNAL, INPUT);  // Signal vom Arduino (Pin 34 = Input-Only)
  pinMode(SENSOR1_SIM, INPUT);     // Pull-Down extern (10kΩ)
  pinMode(SENSOR2_SIM, INPUT);     // Pull-Down extern (10kΩ)
  pinMode(LED_PIN, OUTPUT);
  digitalWrite(LED_PIN, LOW);

  Serial.println("Pin 32: Taster LINKS (INPUT)");
  Serial.println("Pin 33: Taster RECHTS (INPUT)");
  Serial.println("Pin 34: Signal vom Arduino (INPUT)");
  Serial.println("Pin 35: Sensor1-Simulation (INPUT)");
  Serial.println("Pin 26: Sensor2-Simulation (INPUT)");
  Serial.println("Pin 17: Arduino-Status-LED (OUTPUT)");

  // Bounce2 Initialisierung
  buttonLeft.attach(BUTTON_LEFT);
  buttonLeft.interval(50);  // 50ms Entprellzeit

  buttonRight.attach(BUTTON_RIGHT);
  buttonRight.interval(50);

  sensor1Sim.attach(SENSOR1_SIM);
  sensor1Sim.interval(50);

  sensor2Sim.attach(SENSOR2_SIM);
  sensor2Sim.interval(50);

  Serial.println("Bounce2: Entprellung aktiviert (50ms)");

  Serial.println("\nSetup abgeschlossen.");
  Serial.println("=============================================");
  Serial.println("FUNKTIONEN:");
  Serial.println("  - Taster LINKS/RECHTS: Manuelle Bewegung (IMMER)");
  Serial.println("  - Sensor-Taster: Bewegung nur bei Arduino-Motor aktiv");
  Serial.println("  - LED Pin 17: Zeigt Arduino-Schrittmotor Status");
  Serial.println("  - LED Pin 16: Simuliert Motor-Bewegung (2 Sekunden)");
  Serial.println("=============================================\n");

  delay(1000);
}

//---------- MAIN LOOP ----------
void loop() {
  // 1. IMMER: Manuelle Taster prüfen (unabhängig vom Schrittmotor-Status)
  checkManualButtons();

  // 2. Schrittmotor-Status vom Arduino prüfen
  stepperIsRunning = isStepperRunning();

  // 3. LED entsprechend setzen
  digitalWrite(LED_PIN, stepperIsRunning ? HIGH : LOW);

  // 4. NUR wenn Schrittmotor läuft: Sensor-basierte Bewegung erlauben
  if (stepperIsRunning && !motorIsMoving) {
    checkSensorSimulation();
  } else if (!stepperIsRunning) {
    // Schrittmotor läuft NICHT → Counter zurücksetzen
    sensor1TriggerCount = 0;
    sensor2TriggerCount = 0;
  }

  // 5. Status-Ausgabe
  statusOutput();

  delay(LOOP_DELAY);
}

//---------- NEU: MANUELLE TASTER PRÜFEN ----------
void checkManualButtons() {
  // Taster-Status aktualisieren
  buttonLeft.update();
  buttonRight.update();

  // TASTER LINKS gedrückt → nach LINKS bewegen
  if (buttonLeft.rose()) {  // rose() = Flanke LOW→HIGH (Taster gedrückt)
    Serial.println("\n>>> TASTER LINKS gedrückt");

    if (motorIsMoving) {
      Serial.println("    Motor bewegt sich bereits - IGNORIERE Taster\n");
      return;
    }

    // Manuelle Bewegung hat VORRANG - ignoriert Schrittmotor-Status
    Serial.println("    Bewege nach LINKS (manuell - VORRANG)");
    motorIsMoving = true;
    simulateMotorMovement(LEFT_DIRECTION);
    motorIsMoving = false;
    Serial.println("    Bewegung LINKS abgeschlossen\n");
  }

  // TASTER RECHTS gedrückt → nach RECHTS bewegen
  if (buttonRight.rose()) {
    Serial.println("\n>>> TASTER RECHTS gedrückt");

    if (motorIsMoving) {
      Serial.println("    Motor bewegt sich bereits - IGNORIERE Taster\n");
      return;
    }

    Serial.println("    Bewege nach RECHTS (manuell - VORRANG)");
    motorIsMoving = true;
    simulateMotorMovement(RIGHT_DIRECTION);
    motorIsMoving = false;
    Serial.println("    Bewegung RECHTS abgeschlossen\n");
  }
}

//---------- NEU: SCHRITTMOTOR-STATUS PRÜFEN ----------
bool isStepperRunning() {
  // Pin 34 lesen: HIGH = Schrittmotor läuft, LOW = steht
  bool running = digitalRead(STEPPER_SIGNAL) == HIGH;

  // Status-Wechsel erkennen
  static bool lastState = false;
  if (running != lastState) {
    if (running) {
      Serial.println("\n[SIGNAL] Arduino-Schrittmotor läuft → Sensor-Bewegung AKTIV");
    } else {
      Serial.println("\n[SIGNAL] Arduino-Schrittmotor steht → Sensor-Bewegung DEAKTIVIERT");
    }
    lastState = running;
  }

  return running;
}

//---------- NEU: SENSOR-SIMULATION PRÜFEN ----------
void checkSensorSimulation() {
  // Bounce2 Update
  sensor1Sim.update();
  sensor2Sim.update();

  // Status lesen: HIGH = Taster gedrückt (simuliert: Sensor unter Schwellenwert)
  bool sensor1Triggered = sensor1Sim.read() == HIGH;
  bool sensor2Triggered = sensor2Sim.read() == HIGH;

  // Verhindere gleichzeitigen Trigger
  if (sensor1Triggered && sensor2Triggered) {
    Serial.println("!!! WARNUNG: Beide Sensoren gleichzeitig - KEINE BEWEGUNG !!!");
    sensor1TriggerCount = 0;
    sensor2TriggerCount = 0;
    return;
  }

  // Sensor 1 (LINKS): Trigger-Count erhöhen bei Flanke
  if (sensor1Sim.rose()) {
    sensor1TriggerCount++;
    Serial.println("[SENSOR1] Getriggert [" + String(sensor1TriggerCount) +
                   "/" + String(SENSOR_TRIGGER_COUNT) + "]");

    if (sensor1TriggerCount >= SENSOR_TRIGGER_COUNT) {
      Serial.println("\n>>> SENSOR1: " + String(SENSOR_TRIGGER_COUNT) +
                     "x getriggert → Bewege nach RECHTS");
      motorIsMoving = true;
      simulateMotorMovement(RIGHT_DIRECTION);
      motorIsMoving = false;
      sensor1TriggerCount = 0;
      Serial.println("    Sensor-Bewegung RECHTS abgeschlossen\n");
    }
  }

  // Sensor 2 (RECHTS): Trigger-Count erhöhen bei Flanke
  if (sensor2Sim.rose()) {
    sensor2TriggerCount++;
    Serial.println("[SENSOR2] Getriggert [" + String(sensor2TriggerCount) +
                   "/" + String(SENSOR_TRIGGER_COUNT) + "]");

    if (sensor2TriggerCount >= SENSOR_TRIGGER_COUNT) {
      Serial.println("\n>>> SENSOR2: " + String(SENSOR_TRIGGER_COUNT) +
                     "x getriggert → Bewege nach LINKS");
      motorIsMoving = true;
      simulateMotorMovement(LEFT_DIRECTION);
      motorIsMoving = false;
      sensor2TriggerCount = 0;
      Serial.println("    Sensor-Bewegung LINKS abgeschlossen\n");
    }
  }

  // Counter zurücksetzen wenn Taster losgelassen
  if (!sensor1Triggered && sensor1TriggerCount > 0) {
    Serial.println("[SENSOR1] Losgelassen - Counter zurückgesetzt");
    sensor1TriggerCount = 0;
  }

  if (!sensor2Triggered && sensor2TriggerCount > 0) {
    Serial.println("[SENSOR2] Losgelassen - Counter zurückgesetzt");
    sensor2TriggerCount = 0;
  }
}

//---------- STATUS-AUSGABE ----------
void statusOutput() {
  unsigned long currentMillis = millis();
  if (currentMillis - lastStatusTime >= STATUS_INTERVAL) {
    lastStatusTime = currentMillis;

    Serial.println("\n--- Status Update ---");
    Serial.println("Arduino-Schrittmotor: " + String(stepperIsRunning ? "LÄUFT" : "STEHT"));
    Serial.println("Rollenzentrierung-Motor: " + String(motorIsMoving ? "BEWEGT SICH" : "STEHT"));
    Serial.println("Sensor1 Trigger Count: " + String(sensor1TriggerCount) + "/" + String(SENSOR_TRIGGER_COUNT));
    Serial.println("Sensor2 Trigger Count: " + String(sensor2TriggerCount) + "/" + String(SENSOR_TRIGGER_COUNT));
    Serial.println("---------------------\n");
  }
}

//---------- MOTOR-BEWEGUNG SIMULIEREN ----------
void simulateMotorMovement(bool direction) {
  // Statt echtem Motor: LED an + Delay

  String dirText = (direction == LEFT_DIRECTION) ? "LINKS" : "RECHTS";
  Serial.println("  [MOTOR-SIM] LED an - simuliere Bewegung nach " + dirText);
  Serial.println("  [MOTOR-SIM] Dauer: " + String(MOTOR_SIMULATION_TIME) + "ms");

  // LED an = Motor läuft
  digitalWrite(MOTOR_LED_PIN, HIGH);

  // Simuliere Motor-Bewegung mit Delay
  unsigned long startTime = millis();
  while (millis() - startTime < MOTOR_SIMULATION_TIME) {
    // Alle 500ms Progress ausgeben
    if ((millis() - startTime) % 500 == 0) {
      int progress = ((millis() - startTime) * 100) / MOTOR_SIMULATION_TIME;
      Serial.println("    Progress: " + String(progress) + "%");
      delay(50); // Kurze Pause um nicht zu viel Output zu erzeugen
    }
  }

  // LED aus = Motor steht
  digitalWrite(MOTOR_LED_PIN, LOW);
  Serial.println("  [MOTOR-SIM] LED aus - Bewegung beendet");
}
