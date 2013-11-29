using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;
using EasyTransferNet;

namespace EasyTransferTest
{
    public struct IMU_DATA
    {
        public Int16 ax;
        public Int16 ay;
        public Int16 az;
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
            EasyTransfer<IMU_DATA> reader = new EasyTransfer<IMU_DATA>();

            reader.Begin("COM17", 38400);

            reader.DataReceived = (imu) => {
                Console.WriteLine(FramerateCounter.CalculateFrameRate() + "\tAx:" + imu.ax + "\tAy:" + imu.ay + "\tAz:" + imu.az + "\tGx:" + imu.gx + "\tGy:" + imu.gy + "\tGz:" + imu.gz);
            };

            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
        }

    }

}
