using CommandMessenger;
using CommandMessenger.Transport.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace F4ToPokeys
{
    public class ArduinoECMDevice
    {
        public string SerialNumber { get; set; }
        public string PortName { get; set; }
    }

    public class ArduinoECMDriver : IDisposable
    {
        enum Command
        {
            HandshakeRequest,
            HandshakeResponse,
            SetLed,
            Status,
        }

        #region Static

        public static int BAUD_RATE = 57600;

        public static List<ArduinoECMDevice> GetConnectedDevices()
        {
            List<ArduinoECMDevice> devices = new List<ArduinoECMDevice>();

            string[] ports = SerialPort.GetPortNames();
            foreach (var portname in ports)
            {
                Debug.WriteLine("Attempting to connect to " + portname);
                var device = DetectArduinoECMDevice(portname);
                if (device != null)
                    devices.Add(device);
            }

            return devices;
        }

        private static ArduinoECMDevice DetectArduinoECMDevice(string portname)
        {
            SerialTransport serialTransport = new SerialTransport()
            {
                CurrentSerialSettings = { PortName = portname, BaudRate = BAUD_RATE, DtrEnable = false }
            };
            CmdMessenger cmdMessenger = new CmdMessenger(serialTransport);

            try
            {
                bool connected = cmdMessenger.Connect();

                var command = new SendCommand((int)Command.HandshakeRequest, (int)Command.HandshakeResponse, 1000);
                var handshakeResultCommand = cmdMessenger.SendCommand(command);

                if (handshakeResultCommand.Ok)
                {
                    // read response
                    var software = handshakeResultCommand.ReadStringArg();
                    var serialNumber = handshakeResultCommand.ReadStringArg();

                    if (software.Contains("ArduinoECM"))
                    {
                        // create device
                        ArduinoECMDevice device = new ArduinoECMDevice()
                        {
                            PortName = portname,
                            SerialNumber = serialNumber
                        };

                        return device;
                    }
                    else
                    {
                        Debug.WriteLine("Connected to Arduino, but not an Arduino ECM device");
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Handshake FAILED");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
            finally
            {
                cmdMessenger.Disconnect();
                cmdMessenger.Dispose();
                serialTransport.Dispose();
            }
        }

        #endregion

        SerialTransport serialTransport;
        CmdMessenger cmdMessenger;
        public string SerialNumber { get; private set; }

        #region Construction
        public ArduinoECMDriver(ArduinoECMDevice device)
        {
            SerialNumber = device.SerialNumber;

            serialTransport = new SerialTransport()
            {
                CurrentSerialSettings = { PortName = device.PortName, BaudRate = BAUD_RATE, DtrEnable = false }
            };

            cmdMessenger = new CmdMessenger(serialTransport);

            cmdMessenger.Connect();
        }

        public void Dispose()
        {
            cmdMessenger.Disconnect();
            cmdMessenger.Dispose(); 
            serialTransport.Dispose();
        }
        #endregion

        #region SetLed
        public bool SetLed(byte row, byte column, bool value)
        {
            var command = new SendCommand((int)Command.SetLed, (int)Command.Status, 500);
            command.AddArgument(column);
            command.AddArgument(row);
            command.AddArgument(value);

            var setLedStatus = cmdMessenger.SendCommand(command);

            if (setLedStatus.ReadStringArg() == "LED_Set")
                return true;
            else
                return false;
        }
        #endregion
    }
}
