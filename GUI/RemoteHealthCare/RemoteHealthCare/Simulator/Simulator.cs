using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHealthCare.Simulator
{
    public sealed class Simulator
    {
        private List<Devices.IDevice> connectedDevices;
        private static Random rnd = new Random(DateTime.Now.Millisecond);

        public Simulator()
        {
            connectedDevices = new List<Devices.IDevice>();
            for (int i = 0; i < 15; i++)
            {
                Devices.IDevice simulatedDevice = rnd.Next(2) > 0 ? new Devices.StationaryBike($"Tacx Flux { string.Format("{0:D5}", rnd.Next(0, 20000))}","") : null;
                if (simulatedDevice != null && connectedDevices.Where(x => x.DeviceName == simulatedDevice.DeviceName).Count() == 0)
                    connectedDevices.Add(simulatedDevice);
            }

            new Thread(() =>
            {
                while (true)
                {
                    connectedDevices.ForEach(x => x.PushDataChange(new byte[] { 0xA4, 0x09, 0x4E, 0x05, 0x10, 0x19, 0x04, (byte)rnd.Next(0, 254), 0x01, 0x00, 0xFF, 0x24, 0x30 }));
                    connectedDevices.ForEach(x => x.PushDataChange(new byte[] { 0xA4, 0x09, 0x4E, 0x05, 0x19, 0x00, (byte)rnd.Next(30, 120), 0x00, 0x00, 0x00, 0x00, 0x00, 0x30 }));
                    Thread.Sleep(20);
                }
            }).Start();
        }

        public Devices.IDevice OpenDevice(string deviceName)
        {
            if (connectedDevices.Where(x => x.DeviceName == deviceName).Count() > 0) return connectedDevices.Where(x => x.DeviceName == deviceName).First();
            else return null;
        }

        public List<string> ListDevices()
        {
            List<string> devices = new List<string>();
            connectedDevices.ForEach(x => devices.Add(x.DeviceName));
            return devices;
        }

        private static Simulator instance;
        public static Simulator Instance
        {
            get
            {
                if (instance == null)
                    instance = new Simulator();

                return instance;
            }
        }
    }
}
