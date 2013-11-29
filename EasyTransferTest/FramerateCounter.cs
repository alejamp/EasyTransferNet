using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTransferTest
{
    public class FramerateCounter
    {
        public static int CalculateFrameRate()
        {
            if (System.Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;

                frameRate = 0;

                lastTick = System.Environment.TickCount;
            }

            frameRate++;

            return lastFrameRate;
        }

        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;
    }
}
