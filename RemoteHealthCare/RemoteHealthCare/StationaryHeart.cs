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
                Console.WriteLine("Connection couldn't be established, running sim");
                this.simBArray = SimulateHeartRate();
                StartSim(this.simBArray);
            }
            else
            {
                Console.WriteLine("Connection Established, getting data");
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

        public void Heart_SubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            Console.WriteLine("Received from {0}: {1}", e.ServiceName, BitConverter.ToString(e.Data).Replace("-", " "));

            byte[] data = e.Data;
            string[] pageData = BitConverter.ToString(e.Data).Split('-'); // split the string into individual pieces

            if (pageData[0] == "16")
            {
                //Console.WriteLine(data[1].ToString()); // write the HeartRate data to the console
                this.heartRate = data[1];
                Console.WriteLine(this.heartRate.ToString());
            }
        }

    }
}
