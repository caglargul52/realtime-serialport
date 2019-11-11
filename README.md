# RealTime SerialPort
The purpose of writing the library is to automatically connect the USB plug-out.

        static void Main(string[] args)
        {
            RealTimeSerialPort _serialPort = new RealTimeSerialPort(1000, true);
            _serialPort.SuccessReceived += _serialPort_SuccessReceived;
            _serialPort.ErrorReceived += _serialPort_ErrorReceived;
            _serialPort.PortName = "COM1";
            _serialPort.BaundRate = 9500;
            _serialPort.Open();
        }

        private static void _serialPort_ErrorReceived(string exceptionMessage)
        {
            Console.WriteLine(exceptionMessage);
        }

        private static void _serialPort_SuccessReceived(string text)
        {
            Console.WriteLine(text);
        }


