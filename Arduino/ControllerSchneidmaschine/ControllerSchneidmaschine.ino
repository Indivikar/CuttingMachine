// Config
int startDelay = 5000;          // Start-Pause zwischen den Steps für langsamen Anlauf vom SchrittMotor
int minDelay = 500;             // min-Pause zwischen den Steps, beeinflusst die Drehzahl, darf nicht < 200 sein

int A = 13;                     // Pin Handrad - A
int B = 12;                     // Pin Handrad - B

int motorRunning = 7;              // Puls -> wenn Motor abschneiden Stoppt

int puls = 4;                   // Pin Schrittmotor-Treiber - Puls
int dir = 5;                    // Pin Schrittmotor-Treiber - Direction
int enable = 6;                 // Pin Schrittmotor-Treiber - Enable

int cutTaster = 8;              // Pin Taster - schneiden
int cut = 10;                   // Pin Relay - schneiden

// Variablen
int delayHandler;               // zum langsamen Anfahren und Abbremsen vom Schrittmotor

int valB = 0;                   // Input vom Pin Handrad - A
int valA = 0;                   // Input vom Pin Handrad - B
int valMotorRunning =  0;          // 

boolean lockA = false;          // 
boolean lockB = false;          //

boolean isMotorRunning = false; // ist der Motor zum abschneiden fertig (damit der Befehl nur einmal gesendet wird)

boolean allesStoppen = false;   // true = for-Schleife für Steps vom Schrittmotor wird unterbrochen
boolean isHandradOn = false;    //

int stepCounter = 0;            // zählt die Schritte vom Schrittmotor
int setDir = 0;                 // Drehrichtung    
unsigned long oneStep = 0;      // ein Schritt


// Empfangene Daten
char c;                         // eingehende Daten in einzelne Zeichen aufgliedern
String appendSerialData = "";   // einzelne Zeichen in eine Zeichenkette umwandeln

  void setup() {
    // SerialPort
    Serial.begin(9600);         // SerialPort öffnen und Datenrate in Bit pro Sekunde (bps)

    // PinMode Einstellungen
    pinMode(A, INPUT_PULLUP);   // Input vom Pin Handrad - A
    pinMode(B, INPUT_PULLUP);   // Input vom Pin Handrad - B

    pinMode(motorRunning, INPUT_PULLUP);   // Input von der LOGO Q3
       
    pinMode(puls, OUTPUT);      // Puls      
    pinMode(dir, OUTPUT);       // Direction
    pinMode(enable, OUTPUT);    // Enable

    pinMode(cutTaster, INPUT_PULLUP);  // Schneiden Taster
    pinMode(cut, OUTPUT);       // Schneiden

    // Ausgangs-Stellung 
    digitalWrite(cut, HIGH);    // Relay Schneiden
    digitalWrite(enable, LOW);  // Schrittmotor-Treiber - Enable

  }

  void loop() {

      taster();
      dataReceived();   
      motorFinished();

//      if(digitalRead(motorRunning) == LOW){
//          Serial.println(digitalRead(motorRunning));
//      } 
//
//      if(digitalRead(motorRunning) == HIGH){
//          Serial.println(digitalRead(motorRunning));
//      } 

        
        
  }

  void taster() {
      if(digitalRead(cutTaster) == 0  && !isMotorRunning){
          schneiden();
      }
  }

  void motorFinished() {
      valMotorRunning = digitalRead(motorRunning);
      if(valMotorRunning == HIGH && isMotorRunning){       
          sendCommand("schneidenBeendet_", true);
          isMotorRunning = false;
      }
  }
  

  // eingehende Daten Auslesen, Speichern und Befehle Ausführen
  void dataReceived() {
      
      while(Serial.available() > 0) {   // wartet auf eingehende Daten und speichert die Anzahl der Zeichen (Chars)
          c = Serial.read();            // speichert jedes einzelne Zeichen (Char)
          appendSerialData += c;        // erstellt eine Zeichenkette (String) aus den einzelnen Zeichen (Chars)
      }

      // Wenn Handrad angestellt wurde, dann Handrad Input überwachen
      if(isHandradOn) {
          forward();      // Hier wird überwacht, ob Signal "A" oder "B" zu erst da ist,
          backward();     // um die Drehrichtung zu ermitteln | A = vor, B = zurück
                  
          setStep();      // wenn Signal "A" und "B" da waren, mache ein Step
      }

    
    if(c == '#') {                                          // das Zeichen "#" markiert das Ende der eingehenden Daten        
          appendSerialData.trim();                          // Leerzeichen, Zeilenumbrüche entfernen
          appendSerialData = appendSerialData               // letzte Zeichen "#" entfernen
            .substring(0, appendSerialData.length() - 1); 

          allesStoppen = false;                             // Setzt "alles Stoppen" zurück

          String befehl = split(appendSerialData, '_', 0);  // aufteilen der Befehls-Zeile, Trenn-Zeichen ist "_" 
                                                            // z.B.: stepper_300_forward (befehl_steps_richtung)
  
          if(befehl.equals("Connected")) {      
              sendText("Connected");                        // Sendet Bestätigung das Verbunden wurde
              set();                                        // Setzt Standart-Werte nach Verbindung
          } 

          if(befehl.equals("allesStop")) {
              allesStoppen = true;                          // Setze "alles Stoppen"
              sendCommand("allesGestoppt", false);          // Sende Bestätigung, das "alles Stoppen" gesetzt wurde 
          } 
          
          if(befehl.equals("handradOn")) {
              isHandradOn = true;                           // Schalte Handrad an
              sendText("Handrad An");                       // Sende Bestätigung, das Handrad an ist
              
              // nur zum simulieren vom handrad
              for(int i = 0; i < 100; i++) {
                  stepper(1, "forward");
                  //delay(100);
                  sendCommand("steps_" + String(stepCounter), false);
              }   
          } 
          
          if(befehl.equals("handradOff")) {
              isHandradOn = false;                            // Schalte Handrad aus
              sendText("Handrad Aus");                        // Sende Bestätigung, das Handrad aus ist
          }
          
          if(befehl.equals("stepperStart")) {                         // Starte den Schrittmotor
              sendText("Schrittmotor starten...");                    // Sende Bestätigung, das Schrittmotor gestartet wird 
              int steps = split(appendSerialData, '_', 1).toInt();    // Steps auslesen
              String drehRichtung = split(appendSerialData, '_', 2);  // Drehrichtung auslesen
              stepper(steps, drehRichtung);                           // Starte den Schrittmotor
  
              delay(100);                     
              sendCommand("stepperFinished_"                          // Sende Bestätigung das Schrittmotor fertig ist
                    + String(stepCounter), true); 
          }
  
          if(befehl.equals("schneidenStart")) {                       // Starte den Schneiden

                schneiden();
//              sendCommand("schneidenStartet_", true);                 // Sende Bestätigung, das Schneiden gestartet wird   
//              isMotorRunning = true;                                 // Motor abschneiden zurücksetzen
//              digitalWrite(cut, LOW);                                 // Schalte Relay -> Schneiden Start
//              delay(500);                                             // Pause zwischen an und aus, sonst schaltet Relay nicht
//              digitalWrite(cut, HIGH);                                // Schalte Relay -> Schneiden Stop
//              stepCounter = 0;                                        // nach dem Schnitt, den Counter wieder auf 0 setzen
//              sendCommand("steps_" + String(stepCounter), true);      // Sende Bestätigung das Schneiden fertig ist
          }

          if(befehl.equals("resetIstWert")) {                         // reset den Counter 
              //sendText("istwert");                                    
              stepCounter = 0;                                        // den Counter wieder auf 0 setzen
              sendCommand("resetIstWert_"                             // Sende Bestätigung das Reset fertig ist
                  + String(stepCounter), true);
          }
  
          appendSerialData = "";                                      // eingegangene Daten löschen
          c = 0;                                                      // eingegangene Daten löschen
      }
  }

  
  void set() {                                                        // Standart-Werte setzen, nach Connect
        delay(100);                                                   // Pause, sonst gibt es Fehler beim Daten senden
        if(isHandradOn) {                                             // Ist das Handrad an oder aus
          sendCommand("handradOn_", true);                            // sende Befehl, das Button "Handrad" auf "An" gesetzt wird
        } else {
          sendCommand("handradOff_", true);                           // sende Befehl, das Button "Handrad" auf "Aus" gesetzt wird
        }       

        delay(100);                                                   // Pause, sonst gibt es Fehler beim Daten senden
        sendCommand("steps_" + String(stepCounter), true);            // sende, wieviel steps noch gespeichert sind
  }


  void schneiden() {
      sendCommand("schneidenStartet_", true);                 // Sende Bestätigung, das Schneiden gestartet wird   
      isMotorRunning = true;                                 // Motor abschneiden zurücksetzen
      digitalWrite(cut, LOW);                                 // Schalte Relay -> Schneiden Start
      delay(500);                                             // Pause zwischen an und aus, sonst schaltet Relay nicht
      digitalWrite(cut, HIGH);                                // Schalte Relay -> Schneiden Stop
      stepCounter = 0;                                        // nach dem Schnitt, den Counter wieder auf 0 setzen
      sendCommand("steps_" + String(stepCounter), true);      // Sende Bestätigung das Schneiden fertig ist
  }

  void forward(){                             // Handrad -> in welche Richtung wird gedreht hier 
     valA = digitalRead(A);                   // lese Anschluss "A" vom Handrad
     if(valA == HIGH) {                       // Signal von Anschluss "A" erkannt
        lockA = true;                         // wenn Signal "A" erkannt wurde, dann lock Signal "A"
        if(setDir == 0) {                     // wenn Drehrichtung noch nicht gesetzt wurde
            setDir = 1;                       // setze Drehrichtung auf 1 (vorwärts)
            // Serial.println("setDir = 1");
        }
     } else {
        lockA = false;                        // wenn kein Signal "A" erkannt wurde, dann lock Signal "A" nicht
     }
  }

  void backward(){                            // Handrad -> in welche Richtung wird gedreht
     valB = digitalRead(B);                   // lese Anschluss "B" vom Handrad
     if(valB == HIGH) {                       // Signal von Anschluss "B" erkannt
        lockB = true;                         
         if(setDir == 0) {                    // wenn Drehrichtung noch nicht gesetzt wurde
            setDir = 2;                       // setze Drehrichtung auf 2 (rückwärts)
            // Serial.println("setDir = 2");
         }   
     } else {
        lockB = false;                        // wenn kein Signal "B" erkannt wurde, dann lock Signal "B" nicht
     }
  }

  void setStep(){                             // Hier wird ein Step gemacht, wenn das Handrad um eins gedreht wurde
      if(lockA && lockB) {                    // wenn Signal "A" und "B" erkannt wurden
         if (oneStep == 0) {                  // ist zur Kontrolle, das wirklich nur ein Step gemacht wird, pro Durchlauf
             
             if(setDir == 1) {                // Wenn Drehrichtung 1 
                stepper(1, "forward");        // mach ein Schritt nach vorne
                sendCommand("steps_"          // sende Step an die C#-App
                    + String(stepCounter), false);   
             }  
      
             if(setDir == 2) {                // Wenn Drehrichtung 2
                  stepper(1, "backward");     // mach ein Schritt zurück
                  sendCommand("steps_"        // sende Step an die C#-App
                      + String(stepCounter), false); 
             }  
         }
         oneStep = 1;                         // es wurde ein step gemacht
     } 
  
     if (!lockA && !lockB) {                  // wenn kein Signal mehr ansteht
        oneStep = 0;                          // Reset -> es wurde ein Step gemacht
        setDir = 0;                           // Reset -> Drehrichtung
     }
  }

  void isAllesStop() {                        // Stoppt den Schrittmotor
      while(Serial.available() > 0) {         // wartet auf eingehende Daten und speichert die Anzahl der Zeichen (Chars)
          c = Serial.read();                  // speichert jedes einzelne Zeichen (Char)
          appendSerialData += c;              // erstellt eine Zeichenkette (String) aus den einzelnen Zeichen (Chars)
      }
  
      if(c == '#') {                              // das Zeichen "#" markiert das Ende der eingehenden Daten          
          appendSerialData.trim();                // Leerzeichen, Zeilenumbrüche entfernen
          appendSerialData = appendSerialData     // letzte Zeichen "#" entfernen
              .substring(0, appendSerialData.length() - 1); 
  
          allesStoppen = false;                   // Setzt "alles Stoppen" zurück
  
          String befehl = split(appendSerialData, '_', 0);  // aufteilen der Befehls-Zeile, Trenn-Zeichen ist "_" 
                                                            // z.B.: stepper_300_forward (befehl_steps_richtung)

          if(befehl.equals("allesStop")) {                  
              allesStoppen = true;                          // Setze "alles Stoppen"
              sendCommand("allesGestoppt", false);          // Sende Bestätigung, das "alles Stoppen" gesetzt wurde 
          } 
          
          appendSerialData = "";                    // eingegangene Daten löschen
          c = 0;                                    // eingegangene Daten löschen
      }
  }

  void stepper(int steps, String drehRichtung) {    // Schrittmotor bewegen
      int slowRange = steps * 0.90;                 // bei wieveil % der Steps soll der Schrittmotor langsam anlaufen oder abbremsen
    
      if(drehRichtung.equals("forward")) {          // Drehrichtung vor
        digitalWrite(dir, HIGH);
      }
      
      if(drehRichtung.equals("backward")) {         // Drehrichtung zurück
        digitalWrite(dir, LOW);
      }
          
      for(int i = 0; i < steps; i++) {

          // Die Schleife kann in C# unterbrochen werden
          isAllesStop();          // kontrolle, ob die Schleife von der C#-App unterbrochen wurde   
          if(allesStoppen) {      // unterbrechen, wenn "allesStoppen = true" ist
              break;
          }
          
          if(drehRichtung.equals("forward")) {
              ++stepCounter;                        // + 1 Step
          }
          
          if(drehRichtung.equals("backward")) {
              --stepCounter;                        // + 1 Step
          }

          if(i == 1){                               // wenn der erste Schritt gemacht wird
              delayHandler = startDelay;            // setze Start-Wert der Pause, zwischen den Steps
          }

          slowStartStop(i, slowRange);              // Schritt motor langsam anfahren und stoppen
      
          digitalWrite(puls, HIGH);                 // puls an
          delayMicroseconds(500);
          //delayMicroseconds(delayHandler);          // Pause zwischen dem Puls, darf nicht < 200 sein
          digitalWrite(puls, LOW);                  // puls aus
          delayMicroseconds(500);
          //delayMicroseconds(delayHandler);          // Pause zwischen dem Puls, darf nicht < 200 sein
      }     
  }

  void slowStartStop(int i, int slowRange) {                    // Schritt motor langsam anfahren und stoppen
      if(i > slowRange) {                                       
          if(i % 5 == 1  && delayHandler < startDelay){         
              delayHandler = delayHandler + 100;                // Schrittmotor abbremsen -> Pause zwischen dem Puls erhöhen
              //sendText(String(delayHandler) + " -> " + i);
          }
      } else {
          if(i % 5 == 1  && delayHandler > minDelay){
              delayHandler = delayHandler - 100;                // Schrittmotor beschleunigen -> Pause zwischen dem Puls verkleinern
              //sendText(String(delayHandler) + " -> " + i);
          }
      }
  }

  void sendText(String text) {                                  // sende Text an den Serial Monitor, in der C#-App
        Serial.println("#" + text + "@");                     
  }

  void sendCommand(String text, boolean showText) {             // sende ein Befehl an C#-App (Befehl muss mit "_" enden)
      if(showText) {
        Serial.println("#" + text + "@");                       // mit Textausgabe im Serial Monitor
      } else {
        Serial.println("%" + text + "@");                       // ohne Textausgabe
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
  
