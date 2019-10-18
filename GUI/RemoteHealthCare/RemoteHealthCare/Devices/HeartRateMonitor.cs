using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHealthCare.Devices
{
    class HeartRateMonitor : IDevice
    {
        private int heartRate;
        private byte[] simBArray;

        private BLE bluetoothLinkedDevice;

        public event EventHandler DeviceDataChanged;

        public BLE BluetoothLinkedDevice => bluetoothLinkedDevice;
        public EDeviceType DeviceType => EDeviceType.HeartRateMonitor;

        public HeartRateMonitor()
        {
           
        }

        public int HeartRate { get { return this.heartRate; } set { this.heartRate = value; } }
        public BLE Device { get { return this.bluetoothLinkedDevice; } }

        public string DeviceName => "Decathlon Dual HR";

        public byte[] SimulateHeartRate()
        {
            byte[] simBytes = {
                0x58, 0x54,0x54, 0x55, 0x52, 0x51, 0x55, 0x54, 0x60, 0x58, 0x54, 0x49, 0x48, 0x44, 0x49,
                0x58, 0x54,0x54, 0x55, 0x52, 0x51, 0x55, 0x54, 0x60, 0x58, 0x54, 0x49, 0x48, 0x44, 0x49,
                0x58, 0x54,0x54, 0x55, 0x52, 0x51, 0x55, 0x54, 0x60, 0x58, 0x54, 0x49, 0x48, 0x44, 0x49,
                0x58, 0x54,0x54, 0x55, 0x52, 0x51, 0x55, 0x54, 0x60, 0x58, 0x54, 0x49, 0x48, 0x44, 0x49
            };
            return simBytes;
        }

        public void StartSim(byte[] bArray)
        {
            foreach (byte data in bArray)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"HeartRate {data}");
                this.heartRate = data;
            }
        }
        
        private void OnNotifyDataChanged(object sender, BLESubscriptionValueChangedEventArgs e) => ParseData(e.Data);
        public void PushDataChange(byte[] data) => ParseData(data);

        protected virtual void ParseData(byte[] data)
        {
            try
            {
                string[] pageData = BitConverter.ToString(data).Split('-'); // split the string into individual pieces

                if (pageData[0] == "16")
                {
                    this.heartRate = data[1];
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
