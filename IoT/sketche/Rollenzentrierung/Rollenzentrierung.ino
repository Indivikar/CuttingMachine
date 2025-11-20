#include <Wire.h>
#include <VL53L0X.h>

//---------- ALLGEMEINE EINSTELLUNGEN ----------
const int SERIAL_BAUDRATE = 115200;   // Baudrate für die serielle Kommunikation
const int LOOP_DELAY = 100;           // Verzögerung (in ms) im Hauptloop

//---------- I2C EINSTELLUNGEN ----------
const int SDA_PIN = 21;               // I2C SDA Pin
const int SCL_PIN = 22;               // I2C SCL Pin

//---------- VL53L0X SENSOR EINSTELLUNGEN ----------
// Objekte für die beiden Sensoren erstellen
VL53L0X sensor1;
VL53L0X sensor2;

// Pins für XSHUT der beiden Sensoren
const int XSHUT_SENSOR1 = 23;         // XSHUT Pin für Sensor 1 (Sensor 1 muss auf die linke Seite)
const int XSHUT_SENSOR2 = 19;         // XSHUT Pin für Sensor 2 (Sensor 2 muss auf die rechte Seite)

// Adressen für die Sensoren
const uint8_t SENSOR1_ADDRESS = 0x30; // I2C-Adresse für Sensor 1 (Sensor 1 muss auf die linke Seite)
const uint8_t SENSOR2_ADDRESS = 0x31; // I2C-Adresse für Sensor 2 (Sensor 2 muss auf die rechte Seite)

// Betriebseinstellungen für Sensoren
const int SENSOR_TIMING_BUDGET = 100000;    // Timing-Budget in Mikrosekunden (höher = genauer, aber langsamer)
const int SENSOR_THRESHOLD = 40;            // Schwellenwert für Sensoraktivierung in mm
const int SENSOR_TRIGGER_COUNT = 5;         // Anzahl der Unterschreitungen bis zur Motoraktivierung
const bool ENABLE_SENSOR_LONGRANGE = false; // Long-Range-Modus aktivieren (reduziert Präzision)

//---------- TMC2209 TREIBER EINSTELLUNGEN ----------
const int EN_PIN = 25;    // Enable Pin
const int STEP_PIN = 26;  // Step Pin
const int DIR_PIN = 27;   // Direction Pin

// Motor-Parameter
const int STEPS_PER_REV = 51200;  // Schritte pro Umdrehung
const int MICROSTEPS = 4;         // 4 Mikroschritte (MS1=GND, MS2=VDD)
const int TOTAL_STEPS = STEPS_PER_REV / MICROSTEPS;

// Timing-Parameter
int calculateMinDelay(int microsteps) {
  // Basisverzögerung für 2 Mikroschritte
  const int BASE_DELAY_US = 10;
  // Minimale Verzögerung skaliert linear mit der Anzahl der Mikroschritte
  return BASE_DELAY_US * (microsteps / 2);
}

const int STEP_DELAY_US = calculateMinDelay(MICROSTEPS);  // Verzögerung zwischen Schritten

// Variablen zur Fehlerdiagnose
bool sensor1Ready = false;
bool sensor2Ready = false;
unsigned long lastStatusTime = 0;
const unsigned long STATUS_INTERVAL = 2000; // Status alle 2 Sekunden ausgeben

// Zähler für Sensor-Trigger
int sensor1TriggerCount = 0;
int sensor2TriggerCount = 0;

// WICHTIG: Flag um zu verhindern, dass während Bewegung Counter erhöht werden
bool motorIsMoving = false;

// Konstanten für Bewegungsrichtungen
const bool RIGHT_DIRECTION = HIGH;  // Gegen den Uhrzeigersinn
const bool LEFT_DIRECTION = LOW;    // Im Uhrzeigersinn

// Befehle der Schneidmaschinen-App empfangen
char c;                         // eingehende Daten in einzelne Zeichen aufgliedern
String appendSerialData = "";   // einzelne Zeichen in eine Zeichenkette umwandeln

void setup() {
  Serial.begin(SERIAL_BAUDRATE);
  
  // Warten bis die serielle Verbindung hergestellt ist
  delay(2000);
  
  printAndSend("\n\n-------------------------------------");
  printAndSend("ESP32 TMC2209 Sensor-Stepper - Optimierte Version mit Trigger-Count");
  printAndSend("-------------------------------------");
  
  // I2C starten
  Wire.begin(SDA_PIN, SCL_PIN);
  Serial.println("I2C gestartet auf Pins SDA=" + String(SDA_PIN) + ", SCL=" + String(SCL_PIN));

  // TMC2209 Pins konfigurieren
  pinMode(EN_PIN, OUTPUT);
  pinMode(STEP_PIN, OUTPUT);
  pinMode(DIR_PIN, OUTPUT);
  
  // Treiber deaktivieren beim Start (high-active für den EN_PIN)
  digitalWrite(EN_PIN, HIGH);
  Serial.println("TMC2209 Pins konfiguriert (EN_PIN=" + String(EN_PIN) + ", STEP_PIN=" + String(STEP_PIN) + ", DIR_PIN=" + String(DIR_PIN) + ")");

  // Sensoren initialisieren
  initializeSensors();

  // Warte kurz, bis alles initialisiert ist
  delay(1000);
  
  printAndSend("Setup abgeschlossen.");
  printAndSend("Schwellenwert für Sensoren: " + String(SENSOR_THRESHOLD) + " mm");
  printAndSend("Erforderliche Trigger: " + String(SENSOR_TRIGGER_COUNT) + " mal hintereinander");
  printAndSend("-------------------------------------");
  
  // Führe Kalibrierung durch (fährt beim Start nach links und rechts, bis die jeweiligen Sensoren erreicht werden)
  //calibrateSystem();
}

// Initialisiert beide Sensoren
void initializeSensors() {
  Serial.println("Initialisiere Sensoren...");
  pinMode(XSHUT_SENSOR1, OUTPUT);
  pinMode(XSHUT_SENSOR2, OUTPUT);

  // Beide Sensoren deaktivieren, um Adressenkonflikte zu vermeiden
  digitalWrite(XSHUT_SENSOR1, LOW);
  digitalWrite(XSHUT_SENSOR2, LOW);
  delay(10);
  Serial.println("Sensoren zurückgesetzt");

  // Sensor 1 aktivieren und konfigurieren
  digitalWrite(XSHUT_SENSOR1, HIGH);
  delay(50);
  
  Serial.println("Initialisiere Sensor 1...");
  if (sensor1.init()) {
    Serial.println("Sensor 1 erfolgreich initialisiert");
    sensor1Ready = true;
    
    sensor1.setAddress(SENSOR1_ADDRESS);
    Serial.println("Sensor 1 Adresse gesetzt auf: 0x" + String(SENSOR1_ADDRESS, HEX));
    
    sensor1.setTimeout(500);
    if (ENABLE_SENSOR_LONGRANGE) {
      sensor1.setSignalRateLimit(0.1);
      sensor1.setVcselPulsePeriod(VL53L0X::VcselPeriodPreRange, 18);
      sensor1.setVcselPulsePeriod(VL53L0X::VcselPeriodFinalRange, 14);
    }
    sensor1.setMeasurementTimingBudget(SENSOR_TIMING_BUDGET);
    sensor1.startContinuous();
  } else {
    Serial.println("FEHLER: Sensor 1 konnte nicht initialisiert werden!");
  }
  
  // Sensor 2 aktivieren und konfigurieren
  Serial.println("Initialisiere Sensor 2...");
  digitalWrite(XSHUT_SENSOR2, HIGH);
  delay(50);
  
  if (sensor2.init()) {
    Serial.println("Sensor 2 erfolgreich initialisiert");
    sensor2Ready = true;
    
    sensor2.setAddress(SENSOR2_ADDRESS);
    Serial.println("Sensor 2 Adresse gesetzt auf: 0x" + String(SENSOR2_ADDRESS, HEX));
    
    sensor2.setTimeout(500);
    if (ENABLE_SENSOR_LONGRANGE) {
      sensor2.setSignalRateLimit(0.1);
      sensor2.setVcselPulsePeriod(VL53L0X::VcselPeriodPreRange, 18);
      sensor2.setVcselPulsePeriod(VL53L0X::VcselPeriodFinalRange, 14);
    }
    sensor2.setMeasurementTimingBudget(SENSOR_TIMING_BUDGET);
    sensor2.startContinuous();
  } else {
    Serial.println("FEHLER: Sensor 2 konnte nicht initialisiert werden!");
  }
}

// Führt eine Systemkalibrierung durch, indem nach rechts und dann nach links gefahren wird
void calibrateSystem() {
  Serial.println("\n----- SYSTEM KALIBRIERUNG STARTEN -----");
  
  // Überprüfe, ob Sensoren bereit sind
  if (!sensor1Ready || !sensor2Ready) {
    Serial.println("FEHLER: Kann nicht kalibrieren, Sensoren nicht bereit!");
    return;
  }
  
  // Motor aktivieren
  enableMotor();
  delay(100);  // Kurze Wartezeit für stabilen Motor
  
  // 1. Schritt: Nach rechts fahren bis Sensor 2 aktiviert wird
  Serial.println("Kalibrierung: Fahre nach rechts bis zur Aktivierung von Sensor 2...");
  
  // Überprüfe den aktuellen Status von Sensor 2
  int distance2 = sensor2.readRangeContinuousMillimeters();
  
  // Wenn Sensor 2 bereits aktiviert ist, bewege erst etwas nach links
  if (distance2 < SENSOR_THRESHOLD) {
    Serial.println("Sensor 2 bereits aktiviert. Bewege zuerst nach links...");
    rotateMotorFixedSteps(TOTAL_STEPS, LEFT_DIRECTION);
    delay(200);
    distance2 = sensor2.readRangeContinuousMillimeters();
  }
  
  // Fahre nach rechts, bis Sensor 2 aktiviert wird
  bool found = false;
  int rotations = 0;
  const int MAX_CALIBRATION_ROTATIONS = 20; // Maximale Anzahl von Umdrehungen
  
  while (!found && rotations < MAX_CALIBRATION_ROTATIONS) {
    // Eine volle Umdrehung nach rechts
    rotateMotorFixedSteps(TOTAL_STEPS, RIGHT_DIRECTION);
    rotations++;
    
    // Warte kurz für stabilere Messung
    //delay(100);
    
    // Überprüfe Sensor
    distance2 = sensor2.readRangeContinuousMillimeters();
    
    Serial.println("Umdrehung nach rechts " + String(rotations) + ", Sensor 2 = " + String(distance2) + "mm");
    
    if (distance2 < SENSOR_THRESHOLD) {
      found = true;
      Serial.println("Sensor 2 aktiviert bei " + String(distance2) + "mm nach " + String(rotations) + " Umdrehungen!");
    }
  }
  
  if (!found) {
    Serial.println("Fehler: Konnte rechten Anschlag nicht finden!");
  }
  
  delay(200);
  
  // 2. Schritt: Nach links fahren bis Sensor 1 aktiviert wird
  Serial.println("Kalibrierung: Fahre nach links bis zur Aktivierung von Sensor 1...");
  
  // Überprüfe den aktuellen Status von Sensor 1
  int distance1 = sensor1.readRangeContinuousMillimeters();
  
  // Fahre nach links, bis Sensor 1 aktiviert wird
  found = false;
  rotations = 0;
  
  while (!found && rotations < MAX_CALIBRATION_ROTATIONS) {
    // Eine volle Umdrehung nach links
    rotateMotorFixedSteps(TOTAL_STEPS, LEFT_DIRECTION);
    rotations++;
    
    // Warte kurz für stabilere Messung
    //delay(100);
    
    // Überprüfe Sensor
    distance1 = sensor1.readRangeContinuousMillimeters();
    
    Serial.println("Umdrehung nach links " + String(rotations) + ", Sensor 1 = " + String(distance1) + "mm");
    
    if (distance1 < SENSOR_THRESHOLD) {
      found = true;
      Serial.println("Sensor 1 aktiviert bei " + String(distance1) + "mm nach " + String(rotations) + " Umdrehungen!");
    }
  }
  
  if (!found) {
    Serial.println("Fehler: Konnte linken Anschlag nicht finden!");
  }
  
  // Motor deaktivieren
  disableMotor();
  
  Serial.println("----- SYSTEM KALIBRIERUNG ABGESCHLOSSEN -----\n");
}

// Aktiviert den Motor
void enableMotor() {
  // Treiber aktivieren (LOW für TMC2209)
  digitalWrite(EN_PIN, LOW);
  delay(10);  // Kurze Pause zur Stabilisierung
  Serial.println("*** Motor-Treiber AKTIVIERT (EN_PIN=LOW) ***");
}

// Deaktiviert den Motor
void disableMotor() {
  // Treiber deaktivieren (HIGH für TMC2209)
  digitalWrite(EN_PIN, HIGH);
  Serial.println("Motor-Treiber DEAKTIVIERT (EN_PIN=HIGH)");
}

// Dreht den Motor mit voller Kraft für eine bestimmte Anzahl von Schritten
void rotateMotorFixedSteps(int steps, bool direction) {
  // Richtung setzen und kurz warten
  digitalWrite(DIR_PIN, direction);
  delayMicroseconds(50);  // Kurze Pause nach Richtungswechsel, aber nicht zu lang
  
  // Führe die angegebene Anzahl von Schritten aus
  for (int i = 0; i < steps; i++) {
    digitalWrite(STEP_PIN, HIGH);
    delayMicroseconds(STEP_DELAY_US);
    digitalWrite(STEP_PIN, LOW);
    delayMicroseconds(STEP_DELAY_US);
  }
}

// Reaktion auf Sensoraktivierung
void respondToSensor(int sensorDistance, bool direction) {
  // Motor aktivieren
  enableMotor();

  // Warte kurz, damit der Treiber voll aktiviert wird
  delay(1);

  // Maximale Anzahl von Bewegungsiterationen zur Sicherheit
  const int MAX_MOVE_ITERATIONS = 50;
  int moveCount = 0;
  bool targetReached = false;

  // Verwende eine while-Schleife statt Rekursion für bessere Kontrolle
  while (!targetReached && moveCount < MAX_MOVE_ITERATIONS) {
    moveCount++;

    // Bewege den Motor um eine feste Anzahl von Schritten
    rotateMotorFixedSteps(TOTAL_STEPS, direction);

    // Lese beide Sensoren nach der Bewegung
    int distance1 = sensor1.readRangeContinuousMillimeters();
    int distance2 = sensor2.readRangeContinuousMillimeters();

    // WICHTIG: Prüfe ob beide Sensoren unter Schwellenwert sind
    if (distance1 <= SENSOR_THRESHOLD && distance2 <= SENSOR_THRESHOLD) {
      Serial.println("!!! WARNUNG: Beide Sensoren unter Schwellenwert während Bewegung - STOPPE MOTOR !!!");
      sensor1TriggerCount = 0;
      sensor2TriggerCount = 0;
      disableMotor();
      motorIsMoving = false;
      return;
    }

    // Bestimme die relevante Distanz basierend auf der Bewegungsrichtung
    int newDistance;
    if (direction == LEFT_DIRECTION) {  // Bewegung nach links
      newDistance = distance1;
      Serial.println("Nach Bewegung " + String(moveCount) + ": Sensor 1 = " + String(newDistance) + " mm");
    } else {  // Bewegung nach rechts
      newDistance = distance2;
      Serial.println("Nach Bewegung " + String(moveCount) + ": Sensor 2 = " + String(newDistance) + " mm");
    }

    // Prüfe ob Ziel erreicht wurde
    if (newDistance >= SENSOR_THRESHOLD) {
      // Ziel erreicht - Counter zurücksetzen
      Serial.println("Ziel erreicht! Distanz: " + String(newDistance) + "mm nach " + String(moveCount) + " Bewegungen");
      targetReached = true;
    } else {
      Serial.println("Immer noch unter Schwellenwert, weitere Bewegung erforderlich");
    }
  }

  // Sicherheits-Warnung falls Maximum erreicht
  if (moveCount >= MAX_MOVE_ITERATIONS) {
    Serial.println("!!! WARNUNG: Maximale Anzahl von Bewegungen erreicht - STOPPE MOTOR !!!");
  }

  // Motor deaktivieren und Flags zurücksetzen
  sensor1TriggerCount = 0;
  sensor2TriggerCount = 0;
  disableMotor();
  motorIsMoving = false;
}

// Wenn beide Sensoren gleichzeitig getriggert sind - keine Bewegung und true zurück geben
bool checkSensors(int distance1, int distance2, int SENSOR_THRESHOLD) {
  if (distance1 <= SENSOR_THRESHOLD && distance2 <= SENSOR_THRESHOLD) {
    Serial.println("!!! WARNUNG: Beide Sensoren gleichzeitig unter Schwellenwert - KEINE BEWEGUNG !!!");
    disableMotor();
    sensor1TriggerCount = 0;
    sensor2TriggerCount = 0;
    motorIsMoving = false;
    return true; // Bedingung erfüllt, Schleife soll "abbrechen"
  }
  return false; // Bedingung nicht erfüllt
}

//---------------------------------------------------
//    Kommunikation mit der Schneidmaschinen-App
//---------------------------------------------------

void sendText(String text) {                                  // sende Text an den Serial Monitor, in der C#-App
      Serial.println("~" + text + "@");                     
}

// Neue Funktion: Sendet Text nur an CS-App (um doppelte Ausgaben zu vermeiden)
void printAndSend(String text) {
    sendText(text);                    // Nur für CS-App TextBox
}

void sendCommand(String text, boolean showText) {             // sende ein Befehl an C#-App (Befehl muss mit "_" enden)
  if(showText) {
    Serial.println("~" + text + "@");                       // mit Textausgabe im Serial Monitor
  } else {
    Serial.println("%" + text + "@");                       // ohne Textausgabe
  }
}

void dataReceived() {
    
    while(Serial.available() > 0) {   // wartet auf eingehende Daten und speichert die Anzahl der Zeichen (Chars)
        c = Serial.read();            // speichert jedes einzelne Zeichen (Char)
        appendSerialData += c;        // erstellt eine Zeichenkette (String) aus den einzelnen Zeichen (Chars)
    }

  if(c == '#') {                                          // das Zeichen "#" markiert das Ende der eingehenden Daten
        appendSerialData.trim();                          // Leerzeichen, Zeilenumbrüche entfernen
        appendSerialData = appendSerialData               // letzte Zeichen "#" entfernen
          .substring(0, appendSerialData.length() - 1);

        // Entferne das Start-Zeichen "%" am Anfang, falls vorhanden
        if(appendSerialData.startsWith("%")) {
          appendSerialData = appendSerialData.substring(1);
        }

        String befehl = split(appendSerialData, '_', 0);  // aufteilen der Befehls-Zeile, Trenn-Zeichen ist "_"
                                                          // z.B.: stepper_300_forward (befehl_steps_richtung)

        if(befehl.equals("Connected")) {
            sendText("Connected");                        // Sendet Bestätigung das Verbunden wurde                                                     // Setzt Standart-Werte nach Verbindung
        }
        else if(befehl.equals("TEST")) {
            String testResponse = "TEST";
            sendText(testResponse);                       // Sendet TEST-Bestätigung zurück
        }
        else if(befehl.equals("WHOAMI")) {
            sendCommand("WHOAMI_Rollenzentrierung", false); // Sendet Board-Identifikation ohne Textausgabe
        }

        appendSerialData = "";                                      // eingegangene Daten löschen
        c = 0;                                                      // eingegangene Daten löschen
    }
}

String split(String data, char separator, int index) {        // Befehls-Line Trennen, gibt die einzelnen Teile mit Index zurück
    int found = 0;
    int strIndex[] = { 0, -1 };
    int maxIndex = data.length() - 1;

    for (int i = 0; i <= maxIndex && found <= index; i++) {
        if (data.charAt(i) == separator || i == maxIndex) {
            found++;
            strIndex[0] = strIndex[1] + 1;
            strIndex[1] = (i == maxIndex) ? i+1 : i;
        }
    }
    return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}

void loop() {

  // Befehle der Schneidmaschinen-App empfangen
  dataReceived();

  // sendText("Rollenzentrierung ist Connected");

  int distance1 = 9999; // Standardwert, falls Sensor nicht reagiert
  int distance2 = 9999; // Standardwert, falls Sensor nicht reagiert

  if (sensor1Ready) {
    distance1 = sensor1.readRangeContinuousMillimeters();
  }

  if (sensor2Ready) {
    distance2 = sensor2.readRangeContinuousMillimeters();
  }

  // Wenn beide Sensoren gleichzeitig getriggert sind - keine Bewegung und hier abbrechen
  if (checkSensors(distance1, distance2, SENSOR_THRESHOLD)) {
    return;
  }

  // Statusausgabe in regelmäßigen Abständen
  unsigned long currentMillis = millis();
  if (currentMillis - lastStatusTime >= STATUS_INTERVAL) {
    lastStatusTime = currentMillis;
    Serial.println("\n--- Status Update ---");
    Serial.println("Sensor 1 bereit: " + String(sensor1Ready ? "JA" : "NEIN"));
    Serial.println("Sensor 2 bereit: " + String(sensor2Ready ? "JA" : "NEIN"));
    Serial.println("Schwellenwert: " + String(SENSOR_THRESHOLD) + " mm");
    Serial.println("Sensor 1 Trigger Count: " + String(sensor1TriggerCount) + "/" + String(SENSOR_TRIGGER_COUNT));
    Serial.println("Sensor 2 Trigger Count: " + String(sensor2TriggerCount) + "/" + String(SENSOR_TRIGGER_COUNT));
    Serial.println("Motor bewegt sich: " + String(motorIsMoving ? "JA" : "NEIN"));
  }
  
  // NUR Counter erhöhen wenn Motor NICHT in Bewegung ist
  if (!motorIsMoving) {
    // Abstand von Sensor 1 lesen 
    if (sensor1Ready) {    
      if (sensor1.timeoutOccurred()) {
        Serial.println("FEHLER: Sensor 1 timeout!");
        sensor1TriggerCount = 0; // Bei Fehler Counter zurücksetzen
      } else {
        // Prüfe ob Schwellenwert unterschritten
        if (distance1 < SENSOR_THRESHOLD) {
          sensor1TriggerCount++;
          printAndSend("Sensor 1 Distanz: " + String(distance1) + " mm ** UNTER SCHWELLENWERT ** [" + 
                       String(sensor1TriggerCount) + "/" + String(SENSOR_TRIGGER_COUNT) + "]");
        } else {
          // Schwellenwert nicht unterschritten - Counter zurücksetzen
          if (sensor1TriggerCount > 0) {
            Serial.println("Sensor 1 über Schwellenwert - Counter zurückgesetzt");
          }
          sensor1TriggerCount = 0;
          Serial.println("Sensor 1 Distanz: " + String(distance1) + " mm");
        }
      }
    }

    // Wenn beide Sensoren gleichzeitig getriggert sind - keine Bewegung und hier abbrechen
    if (checkSensors(distance1, distance2, SENSOR_THRESHOLD)) {
      return;
    }

    // Abstand von Sensor 2 lesen
    if (sensor2Ready) {
      if (sensor2.timeoutOccurred()) {
        Serial.println("FEHLER: Sensor 2 timeout!");
        sensor2TriggerCount = 0; // Bei Fehler Counter zurücksetzen
      } else {
        // Prüfe ob Schwellenwert unterschritten
        if (distance2 < SENSOR_THRESHOLD) {
          sensor2TriggerCount++;
          Serial.println("Sensor 2 Distanz: " + String(distance2) + " mm ** UNTER SCHWELLENWERT ** [" + 
                       String(sensor2TriggerCount) + "/" + String(SENSOR_TRIGGER_COUNT) + "]");
        } else {
          // Schwellenwert nicht unterschritten - Counter zurücksetzen
          if (sensor2TriggerCount > 0) {
            Serial.println("Sensor 2 über Schwellenwert - Counter zurückgesetzt");
          }
          sensor2TriggerCount = 0;
          Serial.println("Sensor 2 Distanz: " + String(distance2) + " mm");
        }
      }
    }

    // Wenn beide Sensoren gleichzeitig getriggert sind - keine Bewegung und hier abbrechen
    if (checkSensors(distance1, distance2, SENSOR_THRESHOLD)) {
      return;
    }

    // Schrittmotor steuern basierend auf Sensorwerten und Trigger-Count
    if (sensor1TriggerCount >= SENSOR_TRIGGER_COUNT) {
      // Sensor 1 wurde ausreichend oft getriggert - nach rechts bewegen
      printAndSend("!!! Sensor 1 " + String(SENSOR_TRIGGER_COUNT) + "x getriggert, bewege nach rechts !!!");
      motorIsMoving = true;  // Flag setzen
      respondToSensor(distance1, RIGHT_DIRECTION);
    } 
    else if (sensor2TriggerCount >= SENSOR_TRIGGER_COUNT) {
      // Sensor 2 wurde ausreichend oft getriggert - nach links bewegen
      printAndSend("!!! Sensor 2 " + String(SENSOR_TRIGGER_COUNT) + "x getriggert, bewege nach links !!!");
      motorIsMoving = true;  // Flag setzen
      respondToSensor(distance2, LEFT_DIRECTION);
    } 
    else {
      // Noch nicht genug Trigger - Motor deaktiviert lassen
      disableMotor();
    }
  }

  delay(LOOP_DELAY);
}