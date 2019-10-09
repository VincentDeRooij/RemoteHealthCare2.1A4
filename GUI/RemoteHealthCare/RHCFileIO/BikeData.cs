using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    public class BikeData
    {
        [JsonProperty]
        public List<int> averageRPM { get; set; }
        [JsonProperty]
        public List<int> currentRPM { get; set; }
        [JsonProperty]
        public List<int> torque { get; set; }
        [JsonProperty]
        public List<int> distanceTraversed { get; set; }
        [JsonProperty]
        public List<int> averageSpeed { get; set; }
        [JsonProperty]
        public List<int> currentSpeed { get; set; }

        public BikeData()
        {
            this.averageRPM = new List<int>();
            this.currentRPM = new List<int>();
            this.averageSpeed = new List<int>();
            this.currentSpeed = new List<int>();
            this.torque = new List<int>();
            this.distanceTraversed = new List<int>();
        }

        public int CalcTotalAverage(List<int> dataList) // calculates the mean average over the entire routine. 
        {
            int total = 0;
            int amountOfData = dataList.Count;

            foreach (int item in dataList)
            {
                total += item;
            }
            return total / amountOfData;
        }

        public void addToListAVG_RPM(int data)
        {
            this.averageRPM.Add(data);
        }

        public void addToListCUR_RPM(int data)
        {
            this.currentRPM.Add(data);
        }

        public void addToListAVG_Speed(int data)
        {
            this.averageSpeed.Add(data);
        }

        public void addToListCUR_Speed(int data)
        {
            this.currentSpeed.Add(data);
        }

        public void addToListTorque(int data)
        {
            this.torque.Add(data);
        }

        public void addToListDistanceTraversed(int data)
        {
            this.distanceTraversed.Add(data);
        }
    }
}
