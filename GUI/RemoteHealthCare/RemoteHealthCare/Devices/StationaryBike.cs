using Avans.TI.BLE;
using libantplus.DataPages;
using RemoteHealthCare.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthCare.Devices
{
    public class StationaryBike : IDevice
    {
        private BLE bluetoothLinkedDevice;
        public BLE BluetoothLinkedDevice => bluetoothLinkedDevice;
        public EDeviceType DeviceType => EDeviceType.StationaryBike;

        private StationaryBikeData lastBikeData;
        private GeneralFEData lastGeneralData;

        private int averageRPMCounted;
        private int averageRPM;
        private int AverageRPM => averageRPM;
        public double currentSpeed;
        public double CurrentSpeed => currentSpeed;

        private int averageSpeedCounted;
        public double averageSpeed;
        public double AverageSpeed => averageSpeed;

        public int CurrentRPM => lastBikeData.RPM;

        private float distanceTraveled;
        public float Distance { get; set; }

        public string deviceName;

        public event EventHandler DeviceDataChanged;

        private string DeviceName => deviceName;

        string IDevice.DeviceName => deviceName;

        public string deviceNameData;
        public string userNameData;
        public double currentSpeedData;
        public double averageSpeedData;
        public double distanceData;

        public StationaryBike(string deviceName,string username)
            : base()
        {
            distanceTraveled = 0f;
            averageRPMCounted = 0;
            averageRPM = 0;
            averageSpeed = 0;
            averageSpeedCounted = 0;
            this.deviceName = deviceName;
            this.deviceNameData = deviceName;
            this.userNameData = username;
#if !SIM
            Task.Run(async () =>
            {
                await SetupDevice(deviceName);
            }).Wait();
#endif
            OnDeviceDataChanged();
        }

#if !SIM
        ~StationaryBike() => bluetoothLinkedDevice.CloseDevice();
#endif

        private async Task SetupDevice(string deviceName)
        {
            bluetoothLinkedDevice = new BLE();
            int errorCode = await bluetoothLinkedDevice.OpenDevice(deviceName);
            errorCode = await bluetoothLinkedDevice.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            bluetoothLinkedDevice.SubscriptionValueChanged += OnNotifyDataChanged;
            errorCode = await bluetoothLinkedDevice.SubscribeToCharacteristic("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
        }

        private void OnNotifyDataChanged(object sender, BLESubscriptionValueChangedEventArgs e) => ParseData(e.Data);
        public void PushDataChange(byte[] data) => ParseData(data);

        protected virtual void ParseData(byte[] data)
        {
            try
            {
                SerialMessage<BaseDataModel> dataModel = libantplus.Converters.SerialMessageConverter.ConvertSerialMessage(data);

                if (dataModel.DataPage.DataPageNumber == 0x10 && lastGeneralData != null)
                {
                    GeneralFEData generalData = (dataModel.DataPage as GeneralFEData);
                    if (generalData.Distance >= lastGeneralData.Distance)
                    {
                        float dif = (generalData.Distance - lastGeneralData.Distance) / 1000.0f;
                        distanceTraveled += dif < 0 ? dif * -1 : dif;
                    }
                    double currentSpeed = ((double)((lastGeneralData.PageData[5] << 8) | lastGeneralData.PageData[4])) / 1000;
                    this.currentSpeed = currentSpeed;
                    averageSpeed = ((averageSpeed * averageSpeedCounted) + currentSpeed) / ++averageSpeedCounted;
                    lastGeneralData = generalData;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x10 && lastGeneralData == null)
                {
                    lastGeneralData = dataModel.DataPage as GeneralFEData;
                    averageSpeedCounted++;
                    currentSpeed = ((double)((lastGeneralData.PageData[5] << 8) | lastGeneralData.PageData[4])) / 1000;
                    averageSpeed = currentSpeed;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x19 && lastBikeData != null)
                {
                    StationaryBikeData bikeData = dataModel.DataPage as StationaryBikeData;
                    averageRPM = ((averageRPM * averageRPMCounted) + bikeData.RPM) / ++averageRPMCounted;
                    lastBikeData = bikeData;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x19 && lastBikeData == null)
                {
                    lastBikeData = dataModel.DataPage as StationaryBikeData;
                    averageRPMCounted++;
                    averageRPM = lastBikeData.RPM;
                }
            }
            catch (InvalidCastException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            OnDeviceDataChanged();
        }

        protected virtual void OnDeviceDataChanged()
        {
            this.DeviceDataChanged?.Invoke(this, null);
        }

        private byte CalCheckSum(byte[] data)
        {
            int packetLength = data.Length;

            Byte checkSumByte = 0x00;
            for (int i = 0; i < packetLength; i++)
                checkSumByte ^= data[i];
            return checkSumByte;
        }

        private void changeBikeResistance(byte resistance)
        {
            string characteristic = "6e40fec3-b5a3-f393-e0a9-e50e24dcca9e";
            
            byte[] data = new byte[13];

            data[0] = 0x4A; // Sync bit;
            data[1] = 0x09; // Message Length
            data[2] = 0x4E; // Message type
            data[3] = 0x05; // Message type
            data[4] = 0x30; // Data Type
            data[11] = resistance; // resistance
            data[12] = CalCheckSum(data);

            bluetoothLinkedDevice.WriteCharacteristic(characteristic, data);
        }
    }
}
