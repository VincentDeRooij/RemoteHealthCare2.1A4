using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteHealthCare
{
    class StationaryHeart
    {
        private int errorCode;
        private int heartRate;
        private BLE heartRateDevice;
        private byte[] simBArray;

        public StationaryHeart(BLE heartDevice, int errorCode)
        {
            if (errorCode != 0)
            {
                this.simBArray = SimulateHeartRate();
                StartSim(this.simBArray);
            }
            else
            {
                this.heartRateDevice = heartDevice;
            }
        }

        public int HeartRate { get { return this.heartRate; } set { this.heartRate = value; } }
        public BLE Device { get { return this.heartRateDevice; } }

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

    }
}
