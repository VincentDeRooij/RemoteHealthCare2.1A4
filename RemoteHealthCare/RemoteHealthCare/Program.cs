#define MULTI_DEVICE

using Avans.TI.BLE;
using libantplus.DataPages;
using RemoteHealthCare.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    
                break;
            }
        }

        private static void OnSubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            connectedDevices[(BLE)sender].PushDataChange(e.Data);
        }
    }
}
