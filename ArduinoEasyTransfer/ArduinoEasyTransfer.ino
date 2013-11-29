// ---------------------------------------------------------------------------
// Alejandro Martin Pirola
// EasyTransfer
// 11/2013 
//
// ---------------------------------------------------------------------------
#include "EasyTransfer.h"


EasyTransfer ET; 

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

SEND_DATA_STRUCTURE1 imu_data1;
SEND_DATA_STRUCTURE2 imu_data2;


void setup() {

    Serial.begin(38400);

	// register structs in order
    ET.RegisterMessageType(details(imu_data1));  // Message type 0
    ET.RegisterMessageType(details(imu_data2));	 // Message type 1
    ET.begin(&Serial);

}


void loop() {
    // read raw accel/gyro measurements from device
    imu_data1.ax = 1;
    imu_data1.ay = 2;
    imu_data1.az = 3;
    
	imu_data2.gx = 4;
    imu_data2.gy = 5;
    imu_data2.gz = 6;
   
    ET.sendData(0);
    ET.sendData(1);
}
