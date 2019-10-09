using FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{
    public class DataOverview
    {
        public string patientID { get; set; }
        public BikeData bikeData { get; set; }
        public HeartData heartData { get; set; }

        public DataOverview(string patientID, BikeData bikeData, HeartData heartData)
        {
            this.patientID = patientID;
            this.bikeData = bikeData;
            this.heartData = heartData;
        }
    }
}
