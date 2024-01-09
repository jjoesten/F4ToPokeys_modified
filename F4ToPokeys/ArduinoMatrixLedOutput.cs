using System.Data;
using System.IO.Ports;
using System.Windows;
using System.Xml.Serialization;

namespace F4ToPokeys
{
    public class ArduinoMatrixLedOutput : FalconLightConsumer
    {
        #region Construction
        public ArduinoMatrixLedOutput()
        {
            RemoveMatrixLedOutputCommand = new RelayCommand(ExecuteRemoveMatrixLedOutput);
            MatrixLed = new ArduinoMatrixLed();
        }
        #endregion

        #region MatrixLed
        private ArduinoMatrixLed matrixLed;

        [XmlIgnore]
        public ArduinoMatrixLed MatrixLed
        {
            get { return matrixLed; }
            set
            {
                if (matrixLed == value)
                    return;
                matrixLed = value;
                RaisePropertyChanged("MatrixLed");

                UpdateStatus();
            }
        }
        #endregion

        #region Row
        private byte? row;

        public byte? Row
        {
            get { return row; }
            set
            {
                if (row == value)
                    return;
                row = value;
                RaisePropertyChanged("Row");

                UpdateStatus();
            }
        }
        #endregion

        #region Column
        private byte? column;

        public byte? Column
        {
            get { return column; }
            set
            { 
                if (column == value) 
                    return;
                column = value;
                RaisePropertyChanged("Column");

                UpdateStatus();
            }
        }
        #endregion

        #region RemoveMatrixLedOutputCommand
        [XmlIgnore]
        public RelayCommand RemoveMatrixLedOutputCommand { get; private set; }

        private void ExecuteRemoveMatrixLedOutput(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveECMMatrixLedOutputText, Row, Column),
                Translations.Main.RemoveMatrixLedOutputCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.MatrixLedOutputList.Remove(this);
            Dispose();
        }
        #endregion

        #region owner
        private ArduinoECM owner;
        
        public void SetOwner(ArduinoECM arduinoECM) 
        {
            owner = arduinoECM;
            UpdateStatus();
        }
        #endregion

        #region Error
        private string error;

        [XmlIgnore]
        public string Error
        {
            get { return error; }
            set
            {
                if (error == value)
                    return;
                error = value;
                RaisePropertyChanged("Error");
            }
        }
        #endregion

        #region UpdateStatus
        public void UpdateStatus()
        {
            if (owner == null)
                return;

            //if (!owner.Connected)
            //    return;

            if (MatrixLed == null || !Row.HasValue || !Column.HasValue)
            {
                Error = null;
            }            

            writeOutputState();
        }
        #endregion

        #region Write Output State (Falcon Light Consumer)
        protected override void writeOutputState()
        {
            if (string.IsNullOrEmpty(Error) && owner != null /*&& owner.Connected*/ && MatrixLed != null && Row.HasValue && Column.HasValue)
            {
                if (!MatrixLed.SetLed(owner.Device, (byte)(Row.Value - 1), (byte)(Column.Value - 1), OutputState))
                {
                    Error = Translations.Main.MatrixLedErrorWrite;
                }
            }
        }
        #endregion
    }
}