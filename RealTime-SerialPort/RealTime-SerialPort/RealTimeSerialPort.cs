using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealTime_SerialPort
{
    public delegate void SuccessReceivedEventHandler(string text);
    public delegate void ErrorReceivedEventHandler(string exceptionMessage);

    class RealTimeSerialPort
    {
        public event SuccessReceivedEventHandler SuccessReceived;
        public event ErrorReceivedEventHandler ErrorReceived;

        private int _interval;
        private bool _autoConnect;

        private SerialPort _serialPort;

        public string PortName { get; set; }
        public int BaundRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public bool RtsEnable { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadBufferSize { get; set; }
        public int WriteBufferSize { get; set; }

        private Thread _tDataReceived;
        private Thread _PortListener;

        bool _connStatus = false;
        bool _threadBreak = false; //Listener threadinden çıkmak için
        bool flag; // Bekleme kısmına bir kere girmesi için

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="interval">ConnectionCheckTime</param>
        /// <param name="autoConnect">Automatically connect when USB is connected?</param>
        public RealTimeSerialPort(int interval, bool autoConnect)
        {
            _serialPort = new SerialPort();
            _interval = interval;
            _autoConnect = autoConnect;

            //Property Default Value
            PortName = "COM1";
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.One;
            BaundRate = 9600;
            RtsEnable = false;
            ReadTimeout = SerialPort.InfiniteTimeout;
            WriteTimeout = SerialPort.InfiniteTimeout;
            ReadTimeout = SerialPort.InfiniteTimeout;
            ReadBufferSize = 1024;
            WriteBufferSize = 1024;

            _tDataReceived = new Thread(new ParameterizedThreadStart(DataReceived));
            _tDataReceived.Start(_interval);
            _tDataReceived.IsBackground = true;

            _PortListener = new Thread(ConnectionListener);
            _PortListener.IsBackground = true;
            _PortListener.Start();
        }

        private void DataReceived(object obj)
        {
            int interval = (int)obj;

            while (true)
            {
                if (_serialPort.IsOpen)
                {
                    try
                    {
                        if (_serialPort != null)
                        {
                            string text = _serialPort.ReadExisting();
                            SuccessReceived(text);
                        }

                    }
                    catch
                    {
                    }
                }
                Thread.Sleep(interval);
            }
        }

        private void ConnectionListener()
        {
            while (true)
            {
                if (!_threadBreak)
                {
                    try
                    {
                        if (!_serialPort.IsOpen && _connStatus)
                        {
                            _serialPort.Open();
                            flag = false;
                        }
                    }
                    catch
                    {
                        if (!flag)
                        {
                            ErrorReceived("Bağlantı Beklemede");
                            flag = true;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Opens a new serialport connection
        /// </summary>
        public void Open()
        {
            _serialPort.PortName = PortName;
            _serialPort.BaudRate = BaundRate;
            _serialPort.Parity = Parity;
            _serialPort.StopBits = StopBits;
            _serialPort.DataBits = DataBits;
            _serialPort.RtsEnable = RtsEnable;
            _serialPort.ReadTimeout = ReadTimeout;
            _serialPort.WriteTimeout = WriteTimeout;
            _serialPort.ReadBufferSize = ReadBufferSize;
            _serialPort.WriteBufferSize = WriteBufferSize;

            if (!_serialPort.IsOpen) _serialPort.Open();

            if (_autoConnect && _serialPort.IsOpen)
            {
                _connStatus = true;
                _threadBreak = false;
            }
        }

        /// <summary>
        /// Serialport close
        /// </summary>
        public void Close()
        {
            _serialPort.Close();
            _connStatus = false;
            _threadBreak = true;
        }

        /// <summary>
        /// Discards data from the serial driver's receive buffer.
        /// </summary>
        public void DiscardInBuffer()
        {
            _serialPort.DiscardInBuffer();
        }

        /// <summary>
        /// Discards data from the serial driver's transmit buffer
        /// </summary>
        public void DiscardOutBuffer()
        {
            _serialPort.DiscardOutBuffer();
        }

        /// <summary>
        /// Gets an array of serialport names for the current computer
        /// </summary>
        /// <returns></returns>
        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }
        public void Write(string text)
        {
            _serialPort.Write(text);
        }
        public void Write(char[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }
        public void WriteLine(string text)
        {
            _serialPort.WriteLine(text);
        }

        public bool IsOpen()
        {
            return _serialPort.IsOpen;

        }
    }
}
