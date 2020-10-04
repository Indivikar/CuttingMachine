//below is the global variable
char c; //use to save every incoming data
String appendSerialData = ""; //use ti save data from c variable

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600); //Open serial port, set data rate to 9600 bit per second (bps)

  pinMode(6, OUTPUT); //Enable
  pinMode(5, OUTPUT); //Puls
  pinMode(4, OUTPUT); //Direction

  digitalWrite(6,LOW);
}

  
//  String split(String s, char parser, int index) {
//      String rs="";
//      int parserIndex = index;
//      int parserCnt=0;
//      int rFromIndex=0, rToIndex=-1;
//      while (index >= parserCnt) {
//        rFromIndex = rToIndex+1;
//        rToIndex = s.indexOf(parser,rFromIndex);
//        if (index == parserCnt) {
//            if (rToIndex == 0 || rToIndex == -1) return "";
//            return s.substring(rFromIndex,rToIndex);
//        } else {
//          parserCnt++;
//        } 
//    }
//    return rs;
//  }

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

  void stepper(int steps, String drehRichtung) {
      if(drehRichtung.equals("forward")) {
        digitalWrite(4, HIGH);
      }
      
      if(drehRichtung.equals("backward")) {
        digitalWrite(4, LOW);
      }

      for(int i = 0; i < steps; i++) {
          digitalWrite(5,HIGH);
          delayMicroseconds(500);
          digitalWrite(5,LOW);
          delayMicroseconds(500);
      }     
  }

  void sendText(String text) {
        Serial.print("Arduino Say>> "); //send "Arduino Say>> " to C#
        Serial.println(text); //send the data back to C#
  }

  void sendCommand(String text) {
        Serial.print("Command>> "); //send "Arduino Say>> " to C#
        Serial.println(text + "@"); //send the data back to C#
  }

void loop() {
  
  while(Serial.available()>0) //get the number of bytes (characters) available that already arrived and stored in the serial receive buffer
    {
        c = Serial.read(); //read incoming serial data and store it into c variable
        appendSerialData += c; //append data in c and store it in this variable
    }
  if(c == '#') { //if data inside c equals to end character (#) then execute this  
    
        // Leerzeichen, Zeilenumbrüche und letzte Zeichen entfernen (#)
        appendSerialData.trim();
        appendSerialData = appendSerialData.substring(0, appendSerialData.length() - 1); 
     
        // stepper_300_forward (befehl_steps_richtung)
        String befehl = split(appendSerialData, '_', 0);
        if(befehl.equals("stepperStart")){
            sendText("Schrittmotor starten...");
            int steps = split(appendSerialData, '_', 1).toInt();
            String drehRichtung = split(appendSerialData, '_', 2);
            stepper(steps, drehRichtung);

            sendCommand("stepperFertig");
        }

        appendSerialData=""; //empty data inside appendSerialData variable
        c=0; //empty data inside c variable
    }




}
