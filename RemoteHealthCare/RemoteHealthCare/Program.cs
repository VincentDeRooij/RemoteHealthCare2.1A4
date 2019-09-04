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
        private static List<BLE> connectedBikes;
        private static List<BLE> connectedHRDevices;

        public static async Task Main(string[] args)
        {
            int errorCode = 0;

            connectedBikes       = new List<BLE>();
            connectedHRDevices   = new List<BLE>();

            BLE connectiveBLE = new BLE();
            List<string> devices = connectiveBLE.ListDevices();

            await Task.Delay(1000);

            connectedBikes.Add(await SetupBike("Tacx Flux 01140"));
            connectedHRDevices.Add(await SetupHR("Decathlon Dual HR"));

            Console.Read();
        }

        private static async Task<BLE> SetupBike(string deviceName)
        {
            BLE device = new BLE();
            await device.OpenDevice(deviceName);
            await device.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e");
            device.SubscriptionValueChanged += OnBikeSubscriptionValueChanged;
            await device.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e");
            return device;
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

        private static void OnBikeSubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.Write("BIKE: ");

            try
            {
                SerialMessage<BaseDataModel> dataModel = libantplus.Converters.SerialMessageConverter.ConvertSerialMessage(e.Data);
                Console.Write(dataModel.DataPage.DataPageNumber);
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Console.Write("\n");
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
