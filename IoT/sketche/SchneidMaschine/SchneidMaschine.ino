// Empfangene Daten
char c;                         // eingehende Daten in einzelne Zeichen aufgliedern
String appendSerialData = "";   // einzelne Zeichen in eine Zeichenkette umwandeln

unsigned long counter = 0;


void setup() {
Serial.begin(115200);

}

void loop() {
  delay(1000);                       // warten
  
  // Sende Ping-Nachricht sowohl an Serial als auch an CS-App
  String pingMessage = "Ping " + String(counter++);
  Serial.println(pingMessage);        // Für Serial Monitor
  sendText(pingMessage);              // Für CS-App TextBox

  dataReceived();
}

//---------------------------------------------------
//    Kommunikation mit der Schneidmaschinen-App
//---------------------------------------------------

void sendText(String text) {                                  // sende Text an den Serial Monitor, in der C#-App
      Serial.println("~" + text + "@");                     
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

        String befehl = split(appendSerialData, '_', 0);  // aufteilen der Befehls-Zeile, Trenn-Zeichen ist "_" 
                                                          // z.B.: stepper_300_forward (befehl_steps_richtung)

        if(befehl.equals("Connected")) {      
            sendText("Connected");                        // Sendet Bestätigung das Verbunden wurde                                                     // Setzt Standart-Werte nach Verbindung
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





