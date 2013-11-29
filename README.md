EasyTransferNet
===============

Arduino -> .NET serial binary data transfer made easy.
Send binary data from Arduino to .NET throught serial port.

* Binary data
* Checksum
* Easy

Setup
--------------
You need:
* Visual Studio 2010>
* Arduino Studio
* Arduino compatible board
* EasyTransfer Arduino Lib https://github.com/madsci1016/Arduino-EasyTransfer

Install
--------------
* Deploy into Arduino ArduinoSample.ino
* Connect Arduino trought serial port
* Config Port Name:

        reader.Begin("COM17", 38400);

* Run EasyTranferTest.

Use
--------------

Arduino side sample structure definition
        
        struct SEND_DATA_STRUCTURE1 {
          //put your variable definitions here for the data you want to send
          //THIS MUST BE EXACTLY THE SAME ON THE OTHER ARDUINO
          int16_t ax;
          int16_t ay;
          int16_t az;
        };

.NET side same structure definition

    public struct IMU_DATA1
    {
        public Int16 ax;
        public Int16 ay;
        public Int16 az;
    }

Arduino setup

        Serial.begin(38400);

        // register structs in order
        ET.RegisterMessageType(details(imu_data1));     // Message type 0
        ET.RegisterMessageType(details(imu_data2));	 // Message type 1
        ET.begin(&Serial);
    
Arduino push 

        void loop() {
            // read raw accel/gyro measurements from device
            imu_data1.ax = 1;
            imu_data1.ay = 2;
            imu_data1.az = 3;
            
        	imu_data2.gx = 4;
            imu_data2.gy = 5;
            imu_data2.gz = 6;
           
            ET.sendData(0);  // send imu_data1
            ET.sendData(1);  // send imu_data2
        }

.NET receive data

            EasyTransfer reader = new EasyTransfer();

            reader.Begin("COM17", 38400);
            reader.RegisterMessageType(typeof(IMU_DATA1));
            reader.RegisterMessageType(typeof(IMU_DATA2));

            reader.DataReceived = (obj) => {
                if (obj is IMU_DATA1)
                {
                    var imu = (IMU_DATA1)obj;
                    Console.WriteLine(FramerateCounter.CalculateFrameRate() + "IMU_DATA1> \tAx:" + imu.ax + "\tAy:" + imu.ay + "\tAz:" + imu.az );
                }
                if (obj is IMU_DATA2)
                {
                    var imu = (IMU_DATA2)obj;
                    Console.WriteLine(FramerateCounter.CalculateFrameRate() + "IMU_DATA2> \tGx:" + imu.gx + "\tGy:" + imu.gy + "\tGz:" + imu.gz);
                }
            };




