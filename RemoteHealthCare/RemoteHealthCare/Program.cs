using Avans.TI.BLE;
using libantplus.DataPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthCare
{
    class Program
    {
        private static List<StationaryBike> connectedBikes;
        private static List<BLE> connectedHRDevices;

        public static async Task Main(string[] args)
        {
            int errorCode = 0;

            connectedBikes       = new List<StationaryBike>();
            connectedHRDevices   = new List<BLE>();

            BLE connectiveBLE = new BLE();
            List<string> devices = connectiveBLE.ListDevices();

            await Task.Delay(1000);

            connectedBikes.Add(new StationaryBike("Tacx Flux 01140"));
            connectedHRDevices.Add(await SetupHR("Decathlon Dual HR"));

            while (true)
            {
                for (int i = 0; i < connectedBikes.Count; i++)
                {
                    Console.WriteLine("KM: " + connectedBikes[i].Distance);
                }
            }
        }

        private static async Task<BLE> SetupHR(string deviceName)
        {
            BLE hrDevice = new BLE();
            await hrDevice.OpenDevice(deviceName);
            await hrDevice.SetService("HeartRate");
            hrDevice.SubscriptionValueChanged += OnHRSubscriptionValueChanged;
            await hrDevice.SubscribeToCharacteristic("HeartRateMeasurement");
            return hrDevice;
        }

        private static void OnHRSubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.Write("HR: ");
            for (int i = 0; i < e.Data.Length; i++)
            {
                Console.Write(e.Data[i] + " ");
            }
            Console.Write("\n");
        }
    }
}
