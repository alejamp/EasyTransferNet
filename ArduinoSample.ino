// ---------------------------------------------------------------------------
// Alejandro Martin Pirola
// EasyTransfer
// 11/2013 
//
// This sketch needs: 
// EasyTransfer Arduino Library v2.1
//		Bill Porter
//              www.billporter.info
// https://github.com/madsci1016/Arduino-EasyTransfer
// ---------------------------------------------------------------------------
#include "EasyTransfer.h"


EasyTransfer ET; 

struct SEND_DATA_STRUCTURE{
  //put your variable definitions here for the data you want to send
  //THIS MUST BE EXACTLY THE SAME ON THE OTHER ARDUINO
  int16_t ax;
  int16_t ay;
  int16_t az;
  int16_t gx;
  int16_t gy;
  int16_t gz;
  int16_t mx;
  int16_t my;
  int16_t mz;
};

SEND_DATA_STRUCTURE imu_data;


void setup() {

    Serial.begin(38400);
    ET.begin(details(imu_data), &Serial);

}


void loop() {
    // read raw accel/gyro measurements from device
    imu_data.ax = 1;
    imu_data.ay = 2;
    imu_data.az = 3;
    imu_data.gx = 4;
    imu_data.gy = 5;
    imu_data.gz = 6;
   
    ET.sendData();
}
