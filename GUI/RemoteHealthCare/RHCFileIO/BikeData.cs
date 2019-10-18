using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{

    /// <Class BikeData>
    /// 
    /// This class contains data of the bike
    /// to add to a certain list call upon the corresponding method.
    /// 
    /// </summary>
    public class BikeData
    {
        public string bikeName;
        public List<int> averageRPM { get; set; }
        public List<int> currentRPM { get; set; }
        public List<int> torque { get; set; }
        public List<int> distanceTraversed { get; set; }
        public List<int> averageSpeed { get; set; }
        public List<int> currentSpeed { get; set; }

        public BikeData(string bikeName)
        {
            this.bikeName = bikeName;
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
    }
}
