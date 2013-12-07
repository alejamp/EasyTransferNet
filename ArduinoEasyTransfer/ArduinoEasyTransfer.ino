// ---------------------------------------------------------------------------
// Alejandro Martin Pirola
// EasyTransfer
// 11/2013 
//
// ---------------------------------------------------------------------------
#include "EasyTransfer.h"
#include <Servo.h> 

EasyTransfer ET; 

Servo servo1;
Servo servo2;

struct SEND_DATA_STRUCTURE1 {
  //put your variable definitions here for the data you want to send
  //THIS MUST BE EXACTLY THE SAME ON THE OTHER ARDUINO
  int16_t ax;
  int16_t ay;
  int16_t az;
};

struct SEND_DATA_STRUCTURE2 {
  //put your variable definitions here for the data you want to send
  //THIS MUST BE EXACTLY THE SAME ON THE OTHER ARDUINO
  int16_t gx;
  int16_t gy;
  int16_t gz;
  int16_t mx;
  int16_t my;
  int16_t mz;
};

struct SERVO_OUTPUT
{
	int16_t s1;
	int16_t s2;
};

SEND_DATA_STRUCTURE1 imu_data1;
SEND_DATA_STRUCTURE2 imu_data2;
SERVO_OUTPUT servo_data3;

void setup() {

    Serial.begin(38400);

	// register structs in order
    ET.RegisterMessageType(details(imu_data1));		// Message type 0
    ET.RegisterMessageType(details(imu_data2));		// Message type 1
	ET.RegisterMessageType(details(servo_data3));	// Message type 2
    ET.begin(&Serial);

	servo1.attach(9);  // liga el servo conectado en el pin 9 al objeto servo
	servo2.attach(10); // liga el servo conectado en el pin 10 al objeto servo

}



void loop() {
    // read raw accel/gyro measurements from device
    imu_data1.ax = servo_data3.s1;
    imu_data1.ay = servo_data3.s2;
    imu_data1.az = 3;
    
	imu_data2.gx = 4;
    imu_data2.gy = 5;
    imu_data2.gz = 6;
   
    ET.sendData(0);
    ET.sendData(1);

	  for(int i=0; i<5; i++)
	  {
		//remember, you could use an if() here to check for new data, this time it's not needed.
		if (ET.receiveData())
		{  
			//set our servo position based on what we received from the other Arduino
			//we will also map the ADC value to a servo value
			servo1.write(map(servo_data3.s1, 0, 1023, 0, 179));
			servo2.write(map(servo_data3.s2, 0, 1023, 0, 179));

		}
    
	}


}
