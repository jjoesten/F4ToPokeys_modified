using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Ink;

namespace F4ToPokeys
{
    public class ArduinoMatrixLed
    {
        #region Static
        private static readonly List<byte> availableIndexList;
        public static List<byte> AvailableIndexList
        {
            get { return availableIndexList; }
        }
        #endregion

        #region Construction
        static ArduinoMatrixLed()
        {
            availableIndexList = Enumerable.Range(1, 8).Select(i => (byte)i).ToList();
        }

        public ArduinoMatrixLed()
        {

        }
        #endregion

        #region SetPixel
        public bool SetLed(ArduinoECMDriver device, byte row, byte column, bool value)
        {
            return device.SetLed(row, column, value);
        }
        #endregion
    }
}