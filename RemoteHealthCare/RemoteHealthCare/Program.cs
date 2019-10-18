#define MULTI_DEVICE


using System.Data;
using RHCCore.Networking;
using Avans.TI.BLE;
using libantplus.DataPages;
using RemoteHealthCare.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace RemoteHealthCare
{
    class Program
    {
        private static Dictionary<BLE, IDevice> connectedDevices;

        public static async Task Main(string[] args)
        {
            connectedDevices = new Dictionary<BLE, IDevice>();

#if SIM

            new Thread(() =>
            {
                wnd_Simulator simulator = new wnd_Simulator();
                connectedDevices.Add(new BLE(), simulator.SimulatingBike);
                simulator.SimulatingBike.DeviceDataChanged += OnDeviceDataChanged;
                simulator.ShowDialog();
            }).Start();

            NetworkConectionHandler test = new NetworkConectionHandler("localhost");
            test.sendData();


#else

#if !MULTI_DEVICE
            BLE connectiveBLE = new BLE();
            List<string> devices = connectiveBLE.ListDevices();

            await Task.Delay(1000);
#endif

            await AddDeviceAsync("Tacx Flux 01140", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e", "6e40fec1-b5a3-f393-e0a9-e50e24dcca9e", EDeviceType.StationaryBike);
            //await AddDeviceAsync("Decathlon Dual HR", "HeartRate", "HeartRateMeasurement", EDeviceType.HeartRateMonitor);

#endif

            while (false)
            {

                foreach (IDevice device in connectedDevices.Values)
                {
                    if (device.DeviceType == EDeviceType.StationaryBike)
                    {
                        Console.WriteLine("KM: " + ((StationaryBike)device).Distance);
                    }
                }

                Thread.Sleep(20);
                Console.Clear();
            }
        }

        private static void OnDeviceDataChanged(object sender, EventArgs e)
        {
            
        }

        static private byte CalCheckSum(byte[] data)
        {
            int packetLength = data.Length;

            Byte checkSumByte = 0x00;
            for (int i = 0; i < packetLength; i++)
                checkSumByte ^= data[i];
            return checkSumByte;
        }

        private static void changeBikeResistance(BLE bike, byte resistance)
        {
            string characteristic = "6e40fec3-b5a3-f393-e0a9-e50e24dcca9e";

            byte[] data = new byte[13];

            data[0] = 0x4A; // Sync bit;
            data[1] = 0x09; // Message Length
            data[2] = 0x4E; // Message type
            data[3] = 0x05; // Message type
            data[4] = 0x30; // Data Type
            data[11] = resistance; // resistance in 
            data[12] = CalCheckSum(data);

            bike.WriteCharacteristic(characteristic, data);
        }

        private static async Task AddDeviceAsync(string deviceName, string serviceName, string characteristic, EDeviceType deviceType)
        {
            BLE deviceService = new BLE();
            Thread.Sleep(1000);
            List<String> bleBikeList = deviceService.ListDevices();
            Console.WriteLine("Devices found: ");
            foreach (var name in bleBikeList)
            {
                Console.WriteLine($"Device: {name}");
            }
            await deviceService.OpenDevice(deviceName);
            foreach (var item in deviceService.GetServices) {
                Console.WriteLine(item.Name);
            }
            await deviceService.SetService(serviceName);
                        
            deviceService.SubscriptionValueChanged += OnSubscriptionValueChanged;
            await deviceService.SubscribeToCharacteristic(characteristic);

            switch (deviceType)
            {
                case EDeviceType.StationaryBike:
                    StationaryBike device = new StationaryBike(deviceName);
                    device.DeviceDataChanged += OnDeviceDataChanged;
                    connectedDevices.Add(deviceService, device);
                break;

                case EDeviceType.HeartRateMonitor:
                    BLE heartSensor = new BLE();
                    int errorCode = await heartSensor.OpenDevice("Decathlon Dual HR");
                    StationaryHeart heart = new StationaryHeart(heartSensor, errorCode);
                    await heart.Device.SetService("HeartRate");
                    heart.Device.SubscriptionValueChanged += heart.Heart_SubscriptionValueChanged;
                    await heart.Device.SubscribeToCharacteristic("HeartRateMeasurement");
                break;
            }
        }

        

        private static void OnSubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            connectedDevices[(BLE)sender].PushDataChange(e.Data);
        }

    }
}
