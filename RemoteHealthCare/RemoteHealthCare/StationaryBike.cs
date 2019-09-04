using Avans.TI.BLE;
using libantplus.DataPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteHealthCare
{
    public class StationaryBike : BLE
    {
        private StationaryBikeData lastBikeData;
        private GeneralFEData lastGeneralData;

        private float distanceTravelled;
        public float Distance => distanceTravelled;

        public StationaryBike(string deviceName)
            : base()
        {
            distanceTravelled = 0f;
            int e = this.OpenDevice(deviceName).Result;
            e = this.SetService("6e40fec1-b5a3-f393-e0a9-e50e24dcca9e").Result;
            this.SubscriptionValueChanged += OnBikeSubscriptionValueChanged;
            e = this.SubscribeToCharacteristic("6e40fec2-b5a3-f393-e0a9-e50e24dcca9e").Result;
        }

        private void OnBikeSubscriptionValueChanged(object sender, BLESubscriptionValueChangedEventArgs e)
        {
            try
            {
                SerialMessage<BaseDataModel> dataModel = libantplus.Converters.SerialMessageConverter.ConvertSerialMessage(e.Data);

                if (dataModel.DataPage.DataPageNumber == 0x10 && lastGeneralData != null)
                {
                    GeneralFEData data = (dataModel.DataPage as GeneralFEData);
                    if (data.Distance >= lastGeneralData.Distance)
                    {
                        float dist = (data.Distance - lastGeneralData.Distance) / 1000.0f;
                        distanceTravelled += dist < 0 ? dist * -1 : dist;
                    }
                    lastGeneralData = dataModel.DataPage as GeneralFEData;
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
                Console.WriteLine(ex.Message);
            }
        }
    }
}
