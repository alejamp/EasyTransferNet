using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace EasyTransferNet
{

    public class MessageType
    {
        public int structSize;
        public Type StructType;
    }

    public class EasyTransfer : IDisposable //where T : struct
    {

        SerialPort _serialPort = new SerialPort();
        List<MessageType> _messageTypes = new List<MessageType>();

        public int frameSize = 0 ;
        public List<byte> frameBuffer = new List<byte>();

        public Action<object> DataReceived;
        MessageType _currentMessageType;

        
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
            //structSize = Marshal.SizeOf(typeof(T));

            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Open();
        }

        public void RegisterMessageType(Type t)
        {
            _messageTypes.Add(new MessageType() { 
                structSize = Marshal.SizeOf(t),
                StructType = t
            });
        }

        public MessageType GetRegisteredMessageType(object data)
        {
            var td = data.GetType();
            foreach (var t in _messageTypes)
            {
                if (t.StructType == td) return t;
            }
            return null;
        }

        public void Stop()
        {
            frameBuffer.Clear();
            frameSize = 0;
            _serialPort.Close();
        }

        public void Send(object data)
        {
            var mt = GetRegisteredMessageType(data);
            byte mtindex = (byte)_messageTypes.IndexOf(mt);
            if (mt == null) throw new Exception("MessageType not found. Please send only registered structs.");

            List<byte> buffer = new List<byte>();
            buffer.AddRange (new byte[] { 0x06, 0x85, (byte)mtindex, (byte)mt.structSize});
            //_serialPort.Write(b, 0 , b.Length);
            
            var payload = StructToByteArray(data);
            byte CS = (byte)mt.structSize;
            for (int i = 0; i < mt.structSize; i++)
            {
                CS ^= payload[i];
            }
            buffer.AddRange(payload);
            buffer.Add(CS);
            _serialPort.Write(buffer.ToArray(), 0, buffer.Count);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //Console.WriteLine("Serial:" + _serialPort.ReadExisting());
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
                        var t = (byte)com.ReadByte(); 
                        if (t>=_messageTypes.Count) return;
                        _currentMessageType = _messageTypes[t];
                        frameSize = com.ReadByte();
                        if (frameSize != _currentMessageType.structSize) return;
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
                        ProcessFrame(frameBuffer.ToArray(), _currentMessageType.StructType);

                    frameSize = 0;
                    frameBuffer.Clear();
                }
            }
        }

        private void ProcessFrame(byte[] buffer, Type t)
        {
            
            object aux = ByteArrayToStructure(buffer, t);
            if (DataReceived != null) DataReceived(aux);
        }

        private object ByteArrayToStructure (byte[] bytes, Type t) 
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            object stuff = Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                t);
            handle.Free();
            return stuff;
        }


        private static byte[] StructToByteArray(object data) 
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
