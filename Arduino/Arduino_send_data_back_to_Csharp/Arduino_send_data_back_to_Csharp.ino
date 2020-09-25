//below is the global variable
char c; //use to save every incoming data
String appendSerialData; //use ti save data from c variable

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200); //Open serial port, set data rate to 9600 bit per second (bps)
}

void loop() {
  // put your main code here, to run repeatedly:

  while(Serial.available()>0) //get the number of bytes (characters) available that already arrived and stored in the serial receive buffer
    {
        c = Serial.read(); //read incoming serial data and store it into c variable
        appendSerialData += c; //append data in c and store it in this variable
    }
  if(c == '#') //if data inside c equals to end character (#) then execute this
    {
        Serial.print("Arduino Say>> "); //send "Arduino Say>> " to C#
        Serial.println(appendSerialData); //send the data back to C#
        appendSerialData=""; //empty data inside appendSerialData variable
        c=0; //empty data inside c variable
    }
}
