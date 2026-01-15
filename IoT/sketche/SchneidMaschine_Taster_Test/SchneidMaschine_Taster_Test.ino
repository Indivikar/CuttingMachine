/*
 * SchneidMaschine_Taster_Test.ino
 *
 * TEST-SKETCH für Schrittmotor-Synchronisation mit Rollenzentrierung
 *
 * Zweck:
 * - Testet Pin 9 Signal-Steuerung (HIGH = Schrittmotor läuft)
 * - Vereinfachte Version ohne LOGO-SPS Integration
 * - Nur für Büro-Test (ohne echte Hardware)
 *
 * Hardware:
 * - Arduino (Uno/Nano/Mega)
 * - Pin 9: OUTPUT → Signal an ESP32 Pin 34
 * - Pin 4-6: Schrittmotor-Treiber (optional für Test)
 * - LED Pin 13: Visualisierung Schrittmotor-Status (optional)
 *
 * Datum: 15. Januar 2026
 */

//---------- CONFIG ----------
double mmInSteps = 13.3;         // Steps pro mm
int startDelay = 5000;           // Start-Pause für langsamen Anlauf
int minDelay = 500;              // Min-Pause zwischen Steps

//---------- PINS ----------
// Schrittmotor-Treiber
const int puls = 4;              // Schrittmotor-Treiber - Puls
const int dir = 5;               // Schrittmotor-Treiber - Direction
const int enable = 6;            // Schrittmotor-Treiber - Enable

// NEU: Signal an ESP32
const int STEPPER_STATUS_PIN = 9;  // HIGH = Schrittmotor läuft, LOW = steht

// Status-LED (optional)
const int LED_PIN = 13;          // Built-in LED zur Visualisierung

//---------- VARIABLEN ----------
int delayHandler;                // Zum langsamen Anfahren/Abbremsen

boolean allesStoppen = false;    // true = for-Schleife unterbrechen
long stepCounter = 0;            // Zählt Schritte vom Schrittmotor

// Empfangene Daten
char c;                          // Eingehende Daten in einzelne Zeichen aufgliedern
String appendSerialData = "";    // Einzelne Zeichen in Zeichenkette umwandeln

// Buffer für isAllesStop()
char stopChar;
String stopBuffer = "";

//---------- SETUP ----------
void setup() {
  // SerialPort
  Serial.begin(9600);
  Serial.println("\n\n========================================");
  Serial.println("SchneidMaschine_Taster_Test - GESTARTET");
  Serial.println("========================================");

  // PinMode Einstellungen
  pinMode(puls, OUTPUT);
  pinMode(dir, OUTPUT);
  pinMode(enable, OUTPUT);

  // NEU: Pin 9 als Ausgang für Schrittmotor-Status
  pinMode(STEPPER_STATUS_PIN, OUTPUT);
  digitalWrite(STEPPER_STATUS_PIN, LOW);  // Initial: Schrittmotor steht
  Serial.println("Pin 9: OUTPUT - Signal an ESP32 (Initial: LOW)");

  // Status-LED
  pinMode(LED_PIN, OUTPUT);
  digitalWrite(LED_PIN, LOW);
  Serial.println("Pin 13: LED - Visualisierung Schrittmotor-Status");

  // Ausgangs-Stellung
  digitalWrite(enable, LOW);  // Schrittmotor-Treiber aktiviert

  Serial.println("Setup abgeschlossen.");
  Serial.println("Verfügbare Befehle:");
  Serial.println("  %stepperStart_[steps]_[forward/backward]#");
  Serial.println("  %allesStop#");
  Serial.println("Beispiel: %stepperStart_1000_forward#");
  Serial.println("========================================\n");
}

//---------- MAIN LOOP ----------
void loop() {
  dataReceived();
}

//---------- DATEN EMPFANGEN ----------
void dataReceived() {
  while(Serial.available() > 0) {
    c = Serial.read();
    appendSerialData += c;
  }

  // Prüfe ob vollständiger Befehl vorhanden ist
  if(c == '#' || appendSerialData.endsWith("#")) {
    appendSerialData.trim();
    appendSerialData = appendSerialData.substring(0, appendSerialData.length() - 1);

    // Start-Zeichen "%" entfernen
    if(appendSerialData.startsWith("%")) {
      appendSerialData = appendSerialData.substring(1);
    }

    allesStoppen = false;

    String befehl = split(appendSerialData, '_', 0);

    // Befehl: stepperStart
    if(befehl.equals("stepperStart")) {
      Serial.println("\n>>> Befehl empfangen: stepperStart");

      unsigned long steps = split(appendSerialData, '_', 1).toInt();
      String drehRichtung = split(appendSerialData, '_', 2);

      Serial.println("    Steps: " + String(steps));
      Serial.println("    Richtung: " + drehRichtung);
      Serial.println("    Starte Schrittmotor...\n");

      stepper(steps, drehRichtung);

      Serial.println("\n>>> Schrittmotor FERTIG");
      Serial.println("    Step Counter: " + String(stepCounter));
      Serial.println("    Pin 9: LOW (Schrittmotor steht)\n");
    }

    // Befehl: allesStop
    if(befehl.equals("allesStop")) {
      Serial.println("\n!!! ALLES STOP !!!\n");
      allesStoppen = true;
    }

    // Befehl: resetIstWert
    if(befehl.equals("resetIstWert")) {
      stepCounter = 0;
      Serial.println("\n>>> Step Counter zurückgesetzt: " + String(stepCounter) + "\n");
    }

    appendSerialData = "";
    c = 0;
  }
}

//---------- SCHRITTMOTOR BEWEGEN ----------
void stepper(unsigned long steps, String drehRichtung) {
  // *** NEU: Signal an ESP32 senden - Schrittmotor startet ***
  digitalWrite(STEPPER_STATUS_PIN, HIGH);
  digitalWrite(LED_PIN, HIGH);  // LED an
  Serial.println("[PIN 9] HIGH - Schrittmotor läuft (Signal an ESP32 gesendet)");

  // Richtung setzen
  if(drehRichtung.equals("forward")) {
    digitalWrite(dir, HIGH);
    Serial.println("[RICHTUNG] Vorwärts");
  }

  if(drehRichtung.equals("backward")) {
    digitalWrite(dir, LOW);
    Serial.println("[RICHTUNG] Rückwärts");
  }

  // Schritte ausführen
  for(unsigned long i = 0; i < steps; i++) {
    // Kontrolle, ob Schleife unterbrochen werden soll
    isAllesStop();
    if(allesStoppen) {
      Serial.println("\n!!! Bewegung ABGEBROCHEN durch allesStop !!!");
      break;
    }

    // Step Counter anpassen
    if(drehRichtung.equals("forward")) {
      ++stepCounter;
    }
    if(drehRichtung.equals("backward")) {
      --stepCounter;
    }

    // Step ausführen
    digitalWrite(puls, HIGH);
    delayMicroseconds(500);
    digitalWrite(puls, LOW);
    delayMicroseconds(500);

    // Progress-Anzeige alle 100 Steps
    if((i + 1) % 100 == 0) {
      Serial.println("  Progress: " + String(i + 1) + "/" + String(steps) + " Steps");
    }
  }

  // *** NEU: Signal an ESP32 senden - Schrittmotor stoppt ***
  digitalWrite(STEPPER_STATUS_PIN, LOW);
  digitalWrite(LED_PIN, LOW);  // LED aus
  Serial.println("[PIN 9] LOW - Schrittmotor steht (Signal an ESP32 gesendet)");
}

//---------- ALLES STOPPEN WÄHREND BEWEGUNG ----------
void isAllesStop() {
  while(Serial.available() > 0) {
    stopChar = Serial.read();
    stopBuffer += stopChar;
  }

  if(stopChar == '#') {
    stopBuffer.trim();
    stopBuffer = stopBuffer.substring(0, stopBuffer.length() - 1);

    if(stopBuffer.startsWith("%")) {
      stopBuffer = stopBuffer.substring(1);
    }

    String befehl = split(stopBuffer, '_', 0);

    if(befehl.equals("allesStop")) {
      allesStoppen = true;
      Serial.println("\n!!! allesStop empfangen während Bewegung !!!");
    } else {
      // Andere Befehle für späteren Aufruf speichern
      appendSerialData += "%" + stopBuffer + "#";
      Serial.println("[DEBUG] Befehl gespeichert für später: " + stopBuffer);
    }

    stopBuffer = "";
    stopChar = 0;
  }
}

//---------- SPLIT FUNKTION ----------
String split(String data, char separator, int index) {
  int found = 0;
  int strIndex[] = {0, -1};
  int maxIndex = data.length() - 1;

  for(int i = 0; i <= maxIndex && found <= index; i++) {
    if(data.charAt(i) == separator || i == maxIndex) {
      found++;
      strIndex[0] = strIndex[1] + 1;
      strIndex[1] = (i == maxIndex) ? i+1 : i;
    }
  }

  return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}
