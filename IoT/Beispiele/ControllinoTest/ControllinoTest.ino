#include <Controllino.h> 
/* Usage of CONTROLLINO library allows you to use CONTROLLINO_xx aliases in your sketch. */

// the setup routine runs once when you press reset:
void setup() {
    // initialize necessary pin as input pin
    pinMode(CONTROLLINO_A5, INPUT);
    pinMode(CONTROLLINO_D0, OUTPUT);
    pinMode(CONTROLLINO_D1, INPUT);
    pinMode(CONTROLLINO_D2, INPUT);
    pinMode(CONTROLLINO_D3, INPUT);
    pinMode(CONTROLLINO_D4, INPUT);
    pinMode(CONTROLLINO_D5, INPUT);
    pinMode(CONTROLLINO_D6, INPUT);
    pinMode(CONTROLLINO_D7, INPUT);
    pinMode(CONTROLLINO_IN0, INPUT);
    pinMode(CONTROLLINO_IN1, INPUT);
    

    

//    digitalWrite(CONTROLLINO_D0, HIGH);
 
    // initialize serial communication at 9600 bits per second:
    Serial.begin(9600);
}

// the loop routine runs over and over again forever:
void loop() {
    // read the input on analog pin 0:
    int sensorValue = analogRead(CONTROLLINO_A5);
    int digitalValueIN0 = digitalRead(CONTROLLINO_IN0);
    int digitalValueD0 = digitalRead(CONTROLLINO_D0);
    int digitalValueD1 = digitalRead(CONTROLLINO_D1);
    int digitalValueD2 = digitalRead(CONTROLLINO_D2);
    int digitalValueD3 = digitalRead(CONTROLLINO_D3);
    int digitalValueD4 = digitalRead(CONTROLLINO_D4);
    int digitalValueD5 = digitalRead(CONTROLLINO_D5);
    int digitalValueD6 = digitalRead(CONTROLLINO_D6);
    int digitalValueD7 = digitalRead(CONTROLLINO_D7);
    // print out the value you read:
    
    Serial.print("Analog: ");
    Serial.println(sensorValue);
    Serial.print("IN0: ");
    Serial.println(digitalValueIN0);
    Serial.print("Digital D0: ");
    Serial.println(digitalValueD0);
    Serial.print("Digital D1: ");
    Serial.println(digitalValueD1);
    Serial.print("Digital D2: ");
    Serial.println(digitalValueD2);
    Serial.print("Digital D3: ");
    Serial.println(digitalValueD3);
    Serial.print("Digital D4: ");
    Serial.println(digitalValueD4);
    Serial.print("Digital D5: ");
    Serial.println(digitalValueD5);
    Serial.print("Digital D6: ");
    Serial.println(digitalValueD6);
    Serial.print("Digital D7: ");
    Serial.println(digitalValueD7);
    delay(1000); // delay in between reads for stability

//      digitalWrite(CONTROLLINO_D2, HIGH);  
//  delay(1000);                         
//  digitalWrite(CONTROLLINO_D2, LOW);   
//  delay(1000); 
}
