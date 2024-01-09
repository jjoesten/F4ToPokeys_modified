using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace F4ToPokeys
{
    public class ArduinoECM : BindableObject, IDisposable
    {
        #region Construction/Destruction
        public ArduinoECM()
        {
            RemoveArduinoECMCommand = new RelayCommand(ExecuteRemoveArduinoECM);
            AddMatrixLedOutputCommand = new RelayCommand(ExecuteAddMatrixLedOutput);
        }

        public void Dispose()
        {
            foreach (ArduinoMatrixLedOutput matrixLedOutput in MatrixLedOutputList)
                matrixLedOutput.Dispose();
        }
        #endregion

        #region Owner
        private Configuration owner;
        public void SetOwner(Configuration config)
        {
            owner = config;
            UpdateStatus();

            foreach (ArduinoMatrixLedOutput matrixLedOutput in MatrixLedOutputList)
                matrixLedOutput.SetOwner(this);
        }
        #endregion

        #region Device
        private ArduinoECMDriver device;
        [XmlIgnore]
        public ArduinoECMDriver Device
        {
            get { return device; }
            set
            {
                if (device == value)
                    return;
                if (device != null)
                {
                    try
                    {
                        device.Dispose();
                    }
                    catch { }
                }
                device = value;
            }
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

        #region SerialNumber
        private string serialNumber;
        public string SerialNumber
        {
            get { return serialNumber; }
            set
            {
                serialNumber = value;
                RaisePropertyChanged("SerialNumber");

                UpdateStatus();
            }
        }

        private IEnumerable<string> SerialNumberAsEnumerable()
        {
            if (!string.IsNullOrWhiteSpace(SerialNumber))
                yield return SerialNumber;
        }

        private List<string> serialNumberList;
        [XmlIgnore]
        public List<string> SerialNumberList
        {
            get
            {
                if (serialNumberList == null)
                {
                    serialNumberList = ArduinoECMEnumerator.Singleton.AvailableArduinoECMDeviceList
                        .Select(device => device.SerialNumber)
                        .Union(SerialNumberAsEnumerable())
                        .OrderBy(serialNumber => serialNumber)
                        .ToList();
                }
                return serialNumberList;
            }
        }
        #endregion

        #region MatrixLedOutputList
        private ObservableCollection<ArduinoMatrixLedOutput> matrixLedOutputList = new ObservableCollection<ArduinoMatrixLedOutput>();
        public ObservableCollection<ArduinoMatrixLedOutput> MatrixLedOutputList
        {
            get { return matrixLedOutputList; }
            set
            {
                matrixLedOutputList = value;
                RaisePropertyChanged("MatrixLedOuputList");
            }
        }
        #endregion

        #region Update
        private void UpdateStatus()
        {
            if (owner == null)
                return;

            Device = null;

            if (string.IsNullOrWhiteSpace(SerialNumber))
                Error = null;
            else
            {
                ArduinoECMDevice availableArduinoECM = ArduinoECMEnumerator.Singleton.AvailableArduinoECMDeviceList.FirstOrDefault(XmlArrayItemAttribute => XmlArrayItemAttribute.SerialNumber == SerialNumber);
                if (availableArduinoECM == null)
                {
                    Error = Translations.Main.ArduinoECMDriverNotFoundError;
                }
                else
                {
                    try
                    {
                        Device = new ArduinoECMDriver(availableArduinoECM);
                        Error = null;
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;
                        throw;
                    }
                }
            }
        }
        #endregion

        #region Commands
        [XmlIgnore]
        public RelayCommand RemoveArduinoECMCommand { get; private set; }

        private void ExecuteRemoveArduinoECM(object o)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Translations.Main.RemoveArduinoECMText, SerialNumber),
                Translations.Main.RemoveArduinoECMCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            owner.ArduinoECMList.Remove(this);
            Dispose();
        }

        [XmlIgnore]
        public RelayCommand AddMatrixLedOutputCommand { get; private set; }

        private void ExecuteAddMatrixLedOutput(object o)
        {
            ArduinoMatrixLedOutput matrixLedOutput = new ArduinoMatrixLedOutput();
            matrixLedOutput.SetOwner(this);
            MatrixLedOutputList.Add(matrixLedOutput);
        }
        #endregion
    }
}