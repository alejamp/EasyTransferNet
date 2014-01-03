using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;
using EasyTransferNet;

namespace EasyTransferTest
{
    public struct ServoOut
    {
        public Int16 pos1;
        public Int16 pos2;
    }

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

            reader.Begin("COM17", 115200);
            reader.RegisterMessageType(typeof(IMU_DATA1));
            reader.RegisterMessageType(typeof(IMU_DATA2));
            reader.RegisterMessageType(typeof(ServoOut));

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

            while (true)
            {
                Console.WriteLine("Press any key to send data");
                var k = Console.ReadKey();

                if (k.Key == ConsoleKey.Escape) break;
                var spos = Int16.Parse(new String(k.KeyChar,1));
                var sdata = new ServoOut();
                sdata.pos1 = (short)(525 + spos * 10);
                sdata.pos2 = (short)(510 + spos * 10);
                reader.Send(sdata);        
            }
        }

    }

}
