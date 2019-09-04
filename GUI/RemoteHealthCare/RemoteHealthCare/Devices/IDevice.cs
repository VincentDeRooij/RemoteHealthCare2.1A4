using Avans.TI.BLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthCare.Devices
{
    public enum EDeviceType
    {
        StationaryBike,
        HeartRateMonitor
    }

    public interface IDevice
    {
        EDeviceType DeviceType { get; }
        BLE BluetoothLinkedDevice { get; }
        void PushDataChange(byte[] data);
    }
}
