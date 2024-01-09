using System.Collections.Generic;
using System.Windows.Documents;

namespace F4ToPokeys
{
    public class ArduinoECMEnumerator
    {
        #region Singleton
        private static ArduinoECMEnumerator singleton;
        public static ArduinoECMEnumerator Singleton
        {
            get
            {
                if (singleton == null)
                    singleton = new ArduinoECMEnumerator();
                return singleton;
            }
        }
        #endregion

        public List<ArduinoECMDevice> AvailableArduinoECMDeviceList { get; }

        public ArduinoECMEnumerator()
        {
            AvailableArduinoECMDeviceList = new List<ArduinoECMDevice>();
            RefreshAvailableArduinoECMDeviceList();
        }

        private void RefreshAvailableArduinoECMDeviceList()
        {
            AvailableArduinoECMDeviceList.Clear();
            var items = ArduinoECMDriver.GetConnectedDevices();
            AvailableArduinoECMDeviceList.AddRange(items);
        }
    }
}