int A = 8;  // Pin
int B = 13; // Pin

int valB = 0; // Wert vom Pin
int valA = 0; // Wert vom Pin

boolean lockA = false; 
boolean lockB = false;

int count = 0;
int dir = 0; // Drehrichtung
unsigned long oneStep = 0; // ein Schritt


  void setup() {
      Serial.begin(9600);
      
      pinMode(B, INPUT_PULLUP);
      pinMode(A, INPUT_PULLUP);
      
      pinMode(4, OUTPUT); // Puls      
      pinMode(5, OUTPUT); // Direction
      pinMode(6, OUTPUT); // Enable
      
      digitalWrite(6, LOW);
    
      Serial.println("Bereit...");
  }

  void loop() {

      forward();
      backward();
      
      setStep();
      
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
                Serial.println(++count);
                stepper(1000, "forward");
             }  
      
             if(dir == 2) {
  //              Serial.println("HIGH B");
                  Serial.println(--count);  
                  stepper(1, "backward");           
             }  
         }
         oneStep = 1;
     } 
  
     if (!lockA && !lockB) {
        oneStep = 0; 
        dir = 0; 
     }
  }

  void stepper(int steps, String drehRichtung) {
      if(drehRichtung.equals("forward")) {
          digitalWrite(5, HIGH);
      }
      
      if(drehRichtung.equals("backward")) {
          digitalWrite(5, LOW);
      }

      for(int i = 0; i < steps; i++) {
          digitalWrite(4, HIGH);
          delayMicroseconds(500);
          digitalWrite(4, LOW);
          delayMicroseconds(500);
      }     
  }
