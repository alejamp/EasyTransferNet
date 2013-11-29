using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;
using EasyTransferNet;

namespace EasyTransferTest
{
    public struct IMU_DATA1
    {
        public Int16 ax;
        public Int16 ay;
        public Int16 az;
    }

    public struct IMU_DATA2
    {
        public Int16 gx;
        public Int16 gy;
        public Int16 gz;
        public Int16 mx;
        public Int16 my;
        public Int16 mz;
    }

    class Program
    {
        public static void Main()
        {
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

            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
        }

    }

}
