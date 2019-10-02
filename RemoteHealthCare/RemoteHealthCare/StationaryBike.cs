using Avans.TI.BLE;
using libantplus.DataPages;
using RemoteHealthCare.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthCare
{
    public class StationaryBike : IDevice
    {
        private StationaryBikeData lastBikeData;
        private GeneralFEData lastGeneralData;

        private float distanceTravelled;
        public float Distance => distanceTravelled;

        public EDeviceType DeviceType => EDeviceType.StationaryBike;

        public event EventHandler DeviceDataChanged;

        public StationaryBike(string deviceName)
            : base()
        {
            distanceTravelled = 0f;
        }

        public void PushDataChange(byte[] data)
        {
            try
            {
                SerialMessage<BaseDataModel> dataModel = libantplus.Converters.SerialMessageConverter.ConvertSerialMessage(data);

                if (dataModel.DataPage.DataPageNumber == 0x10 && lastGeneralData != null)
                {
                    GeneralFEData generalData = (dataModel.DataPage as GeneralFEData);
                    if (generalData.Distance >= lastGeneralData.Distance)
                    {
                        float dist = (generalData.Distance - lastGeneralData.Distance) / 1000.0f;
                        distanceTravelled += dist < 0 ? dist * -1 : dist;
                    }
                    lastGeneralData = generalData;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x10 && lastGeneralData == null)
                {
                    lastGeneralData = dataModel.DataPage as GeneralFEData;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x19 && lastBikeData != null)
                {
                    lastBikeData = dataModel.DataPage as StationaryBikeData;
                }
                else if (dataModel.DataPage.DataPageNumber == 0x19 && lastBikeData == null)
                {
                    lastBikeData = dataModel.DataPage as StationaryBikeData;
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

        public float getdistancetraveled() {
            return distanceTravelled;
        }
    }
}
