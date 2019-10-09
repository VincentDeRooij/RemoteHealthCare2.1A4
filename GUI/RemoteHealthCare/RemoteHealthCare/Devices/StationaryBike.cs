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
        private int currentSpeed;
        public int CurrentSpeed => currentSpeed;

        private int averageSpeedCounted;
        private int averageSpeed;
        public int AverageSpeed => averageSpeed;

        private float distanceTraveled;
        public float Distance => distanceTraveled;

        private string deviceName;

        public event EventHandler DeviceDataChanged;

        public string DeviceName => deviceName;

        public StationaryBike(string deviceName)
            : base()
        {
            distanceTraveled = 0f;
            averageRPMCounted = 0;
            averageRPM = 0;
            averageSpeed = 0;
            averageSpeedCounted = 0;
            this.deviceName = deviceName;
#if !SIM
            Task.Run(async () =>
            {
                await SetupDevice(deviceName);
            }).Wait();
#endif
            OnDeviceDataChanged();
        }

        ~StationaryBike() => bluetoothLinkedDevice.CloseDevice();

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
                    int currentSpeed = BitConverter.ToInt32(new byte[2] { lastGeneralData.PageData[5], lastGeneralData.PageData[4] }, 0);
                    this.currentSpeed = currentSpeed;
                    averageSpeed = ((averageSpeed * averageSpeedCounted) + currentSpeed) / ++averageSpeedCounted;
                    lastGeneralData = generalData;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x10 && lastGeneralData == null)
                {
                    lastGeneralData = dataModel.DataPage as GeneralFEData;
                    averageSpeedCounted++;
                    averageSpeed = BitConverter.ToInt32(new byte[2] { lastGeneralData.PageData[5], lastGeneralData.PageData[4] }, 0);
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
    }
}
