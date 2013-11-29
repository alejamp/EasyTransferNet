using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace EasyTransferNet
{
    public class EasyTransfer<T> : IDisposable where T : struct
    {

        SerialPort _serialPort = new SerialPort();
        int structSize = 0;
        List<byte> bBuffer = new List<byte>();
        int frameSize = 0;
        List<byte> frameBuffer = new List<byte>();




        public Action<T> DataReceived;
        public SerialPort SerialPort
        {
            get { return _serialPort; }
        }

        public EasyTransfer()
        {
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        public void Begin(string portName, int baudRate = 9600)
        {
            structSize = Marshal.SizeOf(typeof(T));

            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Open();
        }

        private void Stop()
        {
            _serialPort.Close();
            frameBuffer.Clear();
            frameSize = 0;
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort com = (SerialPort)sender;
            //Console.WriteLine("Data Received:");
            // Buffer and process binary data
            while (com.BytesToRead > 0)
            {
                // Wait for a header
                while (com.ReadByte() != 0x06 && (frameSize == 0))
                {
                    //frameBuffer.Clear();
                    if (com.BytesToRead < 3)
                        return;
                }
                if (frameSize == 0)
                    if (com.ReadByte() == 0x85)
                    {
                        frameSize = com.ReadByte();
                        if (frameSize != structSize) return;
                    }

                if (frameSize > 0)
                {
                    while (com.BytesToRead > 0 && (frameBuffer.Count < frameSize))
                    {
                        frameBuffer.Add((byte)com.ReadByte());
                    }

                    if (frameSize != frameBuffer.Count)
                    {
                        //Console.WriteLine("Trunked frame");
                        return;
                    }

                    byte calcCS = (byte)frameSize;
                    for (int i = 0; i < frameSize; i++)
                    {
                        calcCS ^= frameBuffer[i];
                    }

                    // Read CS from serial
                    byte orgCS = (byte)com.ReadByte();
                    // checksum ok
                    if (calcCS == orgCS)
                        ProcessFrame(frameBuffer.ToArray());

                    frameSize = 0;
                    frameBuffer.Clear();
                }
            }
        }

        private void ProcessFrame(byte[] buffer)
        {
            T aux = ByteArrayToStructure<T>(buffer);
            if (DataReceived != null) DataReceived(aux);
        }

        private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }


        private static byte[] StructToByteArray<T>(T data) where T : struct
        {
            byte[] rawData = new byte[Marshal.SizeOf(data)];
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(data, rawDataPtr, false);
            }
            finally
            {
                handle.Free();
            }
            return rawData;
        }

        public void Dispose()
        {
            _serialPort.Close();
        }
    }
}
