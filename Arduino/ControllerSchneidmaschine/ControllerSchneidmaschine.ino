// Config
int startDelay = 5000;  // Start-Pause zwischen den Steps für langsamen Anlauf vom SchrittMotor
int minDelay = 500;     // min-Pause zwischen den Steps, beeinflusst die Drehzahl, darf nicht < 200 sein

int delayHandler;

int A = 13;  // Pin
int B = 8; // Pin

int valB = 0; // Wert vom Pin
int valA = 0; // Wert vom Pin

boolean lockA = false; 
boolean lockB = false;

boolean allesStoppen= false;

boolean isHandradOn = false;

int stepCounter = 0;
int dir = 0; // Drehrichtung
unsigned long oneStep = 0; // ein Schritt

// Empfangene Daten
char c; //use to save every incoming data
String appendSerialData = ""; //use ti save data from c variable

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600); //Open serial port, set data rate to 9600 bit per second (bps)

  pinMode(B, INPUT_PULLUP);
  pinMode(A, INPUT_PULLUP);

  pinMode(4, OUTPUT); // Puls      
  pinMode(5, OUTPUT); // Direction
  pinMode(6, OUTPUT); // Enable
  pinMode(7, OUTPUT); // Schneiden

  digitalWrite(7, HIGH);
  digitalWrite(6, LOW); 
}

void loop() {
  dataReceived();
  
}

  void set() {
        delay(100);
        if(isHandradOn) {
          sendCommand("handradOn_", true); 
        } else {
          sendCommand("handradOff_", true); 
        }       

        delay(100);
        sendCommand("steps_" + String(stepCounter), true);
  }

  void dataReceived() {
      while(Serial.available() > 0) { //get the number of bytes (characters) available that already arrived and stored in the serial receive buffer
          c = Serial.read(); //read incoming serial data and store it into c variable
          appendSerialData += c; //append data in c and store it in this variable
      }

      if(isHandradOn) {
          forward();
          backward();
                  
          setStep();
      }
    
    if(c == '#') { //if data inside c equals to end character (#) then execute this  
          // Leerzeichen, Zeilenumbrüche und letzte Zeichen entfernen (#)
          appendSerialData.trim();
          appendSerialData = appendSerialData.substring(0, appendSerialData.length() - 1); 
  
          if(appendSerialData.equals("Connected")) {
              sendText("Connected");  
              set();
          } 

          allesStoppen = false;
  
          // stepper_300_forward (befehl_steps_richtung)
          String befehl = split(appendSerialData, '_', 0);

          if(befehl.equals("allesStop")) {
              allesStoppen = true;
              sendCommand("allesGestoppt", false);           
          } 
          
          if(befehl.equals("handradOn")) {
              isHandradOn = true;
              sendText("Handrad An");
              
              // nur zum simulieren vom handrad
              for(int i = 0; i < 100; i++) {
                  stepper(1, "forward");
                  //delay(100);
                  sendCommand("steps_" + String(stepCounter), false);
              }   
          } 
          
          if(befehl.equals("handradOff")) {
              isHandradOn = false;
              sendText("Handrad Aus");
          }
          
          if(befehl.equals("stepperStart")) {
              sendText("Schrittmotor starten...");
              int steps = split(appendSerialData, '_', 1).toInt();
              String drehRichtung = split(appendSerialData, '_', 2);
              stepper(steps, drehRichtung);
  
              delay(100);                     
              sendCommand("stepperFinished_" + String(stepCounter), true);           
          }
  
          if(befehl.equals("schneidenStart")) {
              sendText("Schneiden starten...");
              digitalWrite(7, LOW);    
              delay(500);
              digitalWrite(7, HIGH);  
              stepCounter = 0;    
              sendCommand("steps_" + String(stepCounter), true);
          }
  
          appendSerialData = ""; //empty data inside appendSerialData variable
          c = 0; //empty data inside c variable
      }
  }

  void sendText(String text) {
        Serial.println("#" + text + "@"); // send the data back to C#
  }

  void sendCommand(String text, boolean showText) {
      if(showText) {
        Serial.println("#" + text + "@");
      } else {
        Serial.println("%" + text + "@");
      }
  }

  void forward(){
     valA = digitalRead(A);   // read the input pin
     if(valA == HIGH) {
        lockA = true;
        if(dir == 0) {
  //          Serial.println("dir = 1");
            dir = 1;
        }
     } else {
        lockA = false;
     }
  }

  void backward(){
     valB = digitalRead(B);   // read the input pin
     if(valB == HIGH) {
        lockB = true;
         if(dir == 0) {
            dir = 2;
  //          Serial.println("dir = 2");
         }   
     } else {
        lockB = false;
     }
  }

  void setStep(){
      if(lockA && lockB) {
         if (oneStep == 0) {
             if(dir == 1) {      
  //              Serial.print("HIGH A  ");
                // Serial.println(++count);

                // ++stepCounter;
                            
                //sendCommand(count);
                stepper(1, "forward");
                sendText("steps_" + String(stepCounter));   
             }  
      
             if(dir == 2) {
  //              Serial.println("HIGH B");
                  // Serial.println(--count);
                  // --stepCounter; 
                  
                  stepper(1, "backward");     
                  sendCommand("steps_" + String(stepCounter), true);      
             }  
         }
         oneStep = 1;
     } 
  
     if (!lockA && !lockB) {
        oneStep = 0; 
        dir = 0; 
     }
  }

  void isAllesStop() {
      while(Serial.available() > 0) { //get the number of bytes (characters) available that already arrived and stored in the serial receive buffer
          c = Serial.read(); //read incoming serial data and store it into c variable
          appendSerialData += c; //append data in c and store it in this variable
      }
  
      if(c == '#') { //if data inside c equals to end character (#) then execute this  
          // Leerzeichen, Zeilenumbrüche und letzte Zeichen entfernen (#)
          appendSerialData.trim();
          appendSerialData = appendSerialData.substring(0, appendSerialData.length() - 1); 
  
          allesStoppen = false;
  
          // stepper_300_forward (befehl_steps_richtung)
          String befehl = split(appendSerialData, '_', 0);

          if(befehl.equals("allesStop")) {
              allesStoppen = true;
              sendCommand("allesGestoppt", false);           
          } 
          
          appendSerialData = ""; //empty data inside appendSerialData variable
          c = 0; //empty data inside c variable
      }
  }

  void stepper(int steps, String drehRichtung) {
      int stopRange = steps * 0.90;
    
      if(drehRichtung.equals("forward")) {
        digitalWrite(5, HIGH);
      }
      
      if(drehRichtung.equals("backward")) {
        digitalWrite(5, LOW);
      }
          
      for(int i = 0; i < steps; i++) {

          // Die Schleife kann in C# unterbrochen werden
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

          if(i == 1){
              delayHandler = startDelay;
          }

          slowStartStop(i, stopRange);
      
          digitalWrite(4, HIGH);
          //delayMicroseconds(5000);
          delayMicroseconds(delayHandler);
          digitalWrite(4, LOW);
          //delayMicroseconds(5000);
          delayMicroseconds(delayHandler);
      }     
  }

  void slowStartStop(int i, int stopRange) {
      if(i > stopRange) {
          if(i % 5 == 1  && delayHandler < startDelay){
              delayHandler = delayHandler + 100;
              //sendText(String(delayHandler) + " -> " + i);
          }
      } else {
          if(i % 5 == 1  && delayHandler > minDelay){
              delayHandler = delayHandler - 100;
              //sendText(String(delayHandler) + " -> " + i);
          }
      }
  }

    String split(String data, char separator, int index) {
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
  
