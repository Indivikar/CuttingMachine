/**
 * Arduino Sketch für SmartHome yourself - Arduino Lehrgang
 * 
 * Tag 24: I2C - Serieller Datenbus
 * Heute lassen wir zwei Arduinos über I2C miteinander kommunizieren. 
 * Am Master-Arduino sind ein Poti und ein Taster angeschlossen, am Slave eine LED.
 * Mit dem Taster wollen wir die LED am Slave ein/ausschalten. Mit dem Poti soll die Helligkeit geregelt werden.
 * Dazu sendet der Master entsprechende Informationen über I2C an den Slave. Der Slave reagiert auf die empfangenen Daten und schaltet die LED entsprechend.
 * 
 * https://www.arduino.cc/en/Reference/Wire
 * 
 * By Daniel Scheidler - Dezember 2019
 */
 #include <Wire.h>                                      // Wire Library einbinden

const unsigned int BUTTON_PIN = 3;                         
const unsigned int POTI_PIN = A0;

const int CLIENT_ID = 8;                                // Adresse des Slaves an dem die LED hängt. 


// Config
double mmProTakt = 2.28; // ein Takt sind wieviel mm
int kurze_streifen = 320; // in mm
int lange_streifen = 635; // in mm

// Welche Streifen werden geschnitten
int mode = 0;

// Analog-Pins vom Arduino
int sensorPin = A0;
int sensorPin_2 = A1;

// Analog-Werte
int sensorValue = 0;
int sensorValue_2 = 0;

// LEDs, wenn Lichtschranke ein Signal hat
int ledPin = 13;
int ledPin_2 = 12;

// Taster
int taster_reset = 8;
int taster_reset_count = 9;
int taster_mode = 10;

//int taster = 8;

int richtung = 0; // 1 = vor, 2 = zurück

// Counter
double countSlots = 0;
double gesamtSchnitte = 0;
double gesamtMM = 0;

// nur einmal pro Takt, die Werte ändern
unsigned long takt = 0; 


//unsigned long saveCountSlots = 0;

// erst wenn es einen hohen Wert und einen tiefen Wert gibt,
// erst dann ein "countSlots" mehr
bool sensorAn = false;
bool sensorAus = false;

bool sensorAn_2 = false;
bool sensorAus_2 = false;

void setup() {
  Serial.begin(38400);                                  // Serielle Kommunikation starten
  pinMode(BUTTON_PIN, INPUT_PULLUP);                    // Button-Pin auf Eingangsmodus mit internem PullUp-Widerstand setzen
  pinMode(POTI_PIN, INPUT);                             // Poti auf Eingangsmodus setzen.

   pinMode(ledPin, OUTPUT); 
   pinMode(ledPin_2, OUTPUT); 

   digitalWrite(ledPin, HIGH);
   digitalWrite(ledPin_2, HIGH);

  
  Wire.begin();                                         // I2C Kommunikation starten
  Serial.println("Master bereit...");
}

void loop() {
//  char cstr[4];                                         // 4-stelliges Char-Array erzeugen

   sensorValue = analogRead(sensorPin);
   sensorValue_2 = analogRead(sensorPin_2);
  
//  if(digitalRead(BUTTON_PIN) == LOW){                   // Prüfen ob Button gedrückt ist
//    Serial.println("BUTTON PRESSED");                   // Wenn ja, auf Seriellem Monitor ausgeben
//    Wire.beginTransmission(CLIENT_ID);                  // Kommunikation über I2C mit Client starten
//    Wire.write('#');                                    // "#" an Client senden
//    Wire.endTransmission();                             // Kommunikation beenden
//    
//    while(digitalRead(BUTTON_PIN) == LOW){              // Warten, bis Taster losgelassen wird
//                                                        // Das warten sorgt dafür, dass die Raute nicht mehrfach gesendet wird. 
//    }
//  }

//  sprintf(cstr, "%03d", analogRead(POTI_PIN)/4 );       // Poti-Einstellung an Poti-Pin auslesen. Diesen Wert durch 4 teilen um Wert zwischen 0 und 255 zu erhalten.
                                                        // Dieser Wert zwischen 0 und 255 wird mit sprintf nun bei Bedarf auf 3-stelligen Wert mit führenden Nullen aufgefüllt.
     lichtschranke_1();
     lichtschranke_2();
  
     setTakt();                                                      // eine 3 wird also in 003 umgewandelt. Das muss sein, damit der Slave immer einen 3-stelligen Wert erhält, 
                                                        // auch wenn die Zahl nur 1- oder 2-stellig ist.
  
//  Serial.print("Helligkeit: ");                         // Ermittelten Wert auf Seriellem Monitor ausgeben
//  Serial.println(cstr);                                 
//  Wire.beginTransmission(CLIENT_ID);                    // I2C Kommunikation starten
//  Wire.write(cstr);                                     // 3-stelligen Wert an Slave übertragen
//  Wire.endTransmission();                               // Kommunikation beenden
   
//  delay(100);                                           // 100ms warten
}

void lichtschranke_1(){
     if (sensorValue > 500) {
      //Serial.println("Lichtschranke 1 An: ");
      if(richtung == 0) {       
        richtung = 1;
        Serial.println("+1");
      }
      digitalWrite(ledPin, HIGH);
      sensorAn = true;    
   } else {
      digitalWrite(ledPin, LOW);
      sensorAn = false;
   }
}

void lichtschranke_2(){
     if (sensorValue_2 > 500) {
    //Serial.println("Lichtschranke 2 An: ");
      if(richtung == 0) {
        richtung = 2;
        Serial.println("-1");
      }
        digitalWrite(ledPin_2, HIGH);
        sensorAn_2 = true;  
   } else {
      digitalWrite(ledPin_2, LOW);
      sensorAn_2 = false; 
   }
}

void setTakt(){
   if (sensorAn && sensorAn_2) {
      digitalWrite(ledPin_2, HIGH);
      if (takt == 0) {


          if(richtung == 1) {
            //Serial.println("+1");
            countSlots++;
            Wire.beginTransmission(CLIENT_ID);
            Wire.write('+'); 
            Wire.endTransmission();
          }

          if(richtung == 2) {
            //Serial.println("-1");
            countSlots--;
            Wire.beginTransmission(CLIENT_ID);
            Wire.write('-'); 
            Wire.endTransmission();
          }
          
          //sensorAn_2 = false;

          takt = 1; 

//          Wire.beginTransmission(CLIENT_ID);                  // Kommunikation über I2C mit Client starten
//          Wire.write('#');                                    // "#" an Client senden
           

          Serial.print("Takt: ");
          Serial.println(countSlots);
//          Serial.print("Takt: ");
//          Serial.print(countSlots);
//          Serial.print(" = ");
//          Serial.print(countSlots * mmProTakt);
//          Serial.print("/");
//          Serial.print(mode);
//          Serial.println(" mm");
//          Serial.print("Richtung: ");
//          Serial.println(richtung);
      }
   }
   
   if (!sensorAn && !sensorAn_2) {
      digitalWrite(ledPin_2, LOW);
      takt = 0;
      richtung = 0;
   }
}
